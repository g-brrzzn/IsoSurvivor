using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace IsometricGame.Classes.Physics
{
    public class SpatialGrid
    {
        private int _cellSize;
        private Dictionary<Point, List<EnemyBase>> _grid;

        public SpatialGrid(int cellSize = 100)
        {
            _cellSize = cellSize;
            _grid = new Dictionary<Point, List<EnemyBase>>();
        }
        public void Clear()
        {
            foreach (var list in _grid.Values)
            {
                list.Clear();
            }
        }
        public void Register(EnemyBase enemy)
        {
            Vector2 pos = new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y);
            Point cell = GetCell(pos);
            if (!_grid.ContainsKey(cell))
            {
                _grid[cell] = new List<EnemyBase>();
            }
            _grid[cell].Add(enemy);
        }
        public List<EnemyBase> Retrieve(Vector2 position)
        {
            List<EnemyBase> found = new List<EnemyBase>();
            Point cell = GetCell(position);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Point neighbor = new Point(cell.X + x, cell.Y + y);
                    if (_grid.TryGetValue(neighbor, out List<EnemyBase> enemies))
                    {
                        found.AddRange(enemies);
                    }
                }
            }

            return found;
        }

        private Point GetCell(Vector2 position)
        {
            return new Point(
                (int)MathF.Floor(position.X / _cellSize),
                (int)MathF.Floor(position.Y / _cellSize)
            );
        }
    }
}