using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes
{
    public class Enemy1 : EnemyBase
    {
        public Enemy1(Vector2 worldPos)
            : base(worldPos, new List<string> { "enemy1_idle_south", "enemy1_idle_west" })
        {
            Life = 1;
            Weight = 1;
            Speed = 3.0f;        }
    }
}