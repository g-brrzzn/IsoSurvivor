using Microsoft.Xna.Framework;
using System;

namespace IsometricGame.Pathfinding
{
    public class PathNode
    {
        public Vector3 Position { get; }
        public PathNode Parent { get; set; }

        public int G_Cost { get; set; }

        public int H_Cost { get; set; }

        public int F_Cost => G_Cost + H_Cost;

        public PathNode(Vector3 position)
        {
            Position = position;
        }

        public void CalculateHCost(Vector3 targetPosition)
        {
            int dX = (int)Math.Abs(Position.X - targetPosition.X);
            int dY = (int)Math.Abs(Position.Y - targetPosition.Y);

            int min_d = Math.Min(dX, dY);
            int max_d = Math.Max(dX, dY);
            H_Cost = (min_d * 14) + ((max_d - min_d) * 10);

        }
    }
}