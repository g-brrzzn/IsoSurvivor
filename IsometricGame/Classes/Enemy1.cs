using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes
{
    public class Enemy1 : EnemyBase
    {
        public Enemy1(Vector2 worldPosXY)
            : this(new Vector3(worldPosXY.X, worldPosXY.Y, 0)) { }

        public Enemy1(Vector3 worldPos)
            : base(worldPos, new List<string> {
                "enemy1_idle_south",
                "enemy1_idle_west",
                "enemy1_idle_north",
                "enemy1_idle_east"
            })
        {
            Life = 2;            Weight = 1;
            Speed = 3.0f;
        }
    }
}