using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace IsometricGame.Pathfinding
{
    public static class Pathfinder
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;

        public static List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos)
        {
            Vector3 startPos = new Vector3(MathF.Round(startWorldPos.X), MathF.Round(startWorldPos.Y), startWorldPos.Z);
            Vector3 targetPos = new Vector3(MathF.Round(targetWorldPos.X), MathF.Round(targetWorldPos.Y), targetWorldPos.Z);

            PathNode startNode = new PathNode(startPos);
            PathNode targetNode = new PathNode(targetPos);
            startNode.CalculateHCost(targetPos);

            List<PathNode> openList = new List<PathNode> { startNode };
            HashSet<Vector3> closedList = new HashSet<Vector3>();

            while (openList.Count > 0)
            {
                PathNode currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].F_Cost < currentNode.F_Cost ||
                       (openList[i].F_Cost == currentNode.F_Cost && openList[i].H_Cost < currentNode.H_Cost))
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode.Position);

                if (currentNode.Position == targetNode.Position)
                {
                    return ReconstructPath(currentNode);
                }

                foreach (PathNode neighbor in GetNeighbors(currentNode, targetPos))
                {
                    if (closedList.Contains(neighbor.Position))
                        continue;

					Vector3 basePos = neighbor.Position; 
					Vector3 posAbove = basePos + new Vector3(0, 0, 1); 

					if (GameEngine.SolidTiles.ContainsKey(basePos) || GameEngine.SolidTiles.ContainsKey(posAbove))
					{
						closedList.Add(basePos);
						continue; 
					}

					if (!openList.Any(n => n.Position == neighbor.Position))
                    {
                        neighbor.G_Cost = currentNode.G_Cost + CalculateMovementCost(currentNode, neighbor);
                        neighbor.CalculateHCost(targetPos);
                        neighbor.Parent = currentNode;
                        openList.Add(neighbor);
                    }
                    else
                    {
                        int newGCost = currentNode.G_Cost + CalculateMovementCost(currentNode, neighbor);
                        if (newGCost < neighbor.G_Cost)
                        {
                            neighbor.G_Cost = newGCost;
                            neighbor.Parent = currentNode;
                        }
                    }
                }
            }

            return null;
        }

        private static List<PathNode> GetNeighbors(PathNode currentNode, Vector3 targetPos)
        {
            List<PathNode> neighbors = new List<PathNode>();
            Vector3 pos = currentNode.Position;

            int z = (int)pos.Z;
            neighbors.Add(new PathNode(new Vector3(pos.X + 1, pos.Y, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X - 1, pos.Y, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X, pos.Y + 1, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X, pos.Y - 1, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X + 1, pos.Y + 1, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X - 1, pos.Y - 1, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X - 1, pos.Y + 1, z)));
            neighbors.Add(new PathNode(new Vector3(pos.X + 1, pos.Y - 1, z)));

            return neighbors;
        }

        private static int CalculateMovementCost(PathNode from, PathNode to)
        {
            bool isDiagonal = (from.Position.X != to.Position.X) && (from.Position.Y != to.Position.Y);
            return isDiagonal ? MOVE_DIAGONAL_COST : MOVE_STRAIGHT_COST;
        }

        private static List<Vector3> ReconstructPath(PathNode endNode)
        {
            List<Vector3> path = new List<Vector3>();
            PathNode currentNode = endNode;

            while (currentNode.Parent != null)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }
            path.Reverse();            return path;
        }
    }
}