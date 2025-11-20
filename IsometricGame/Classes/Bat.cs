using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes
{
    public class Bat : EnemyBase
    {
        public Bat(Vector3 worldPos)
            : base(worldPos, new List<string> { "bat_idle" })
        {
            Life = 1;
            Weight = 1;
            Speed = 4.0f;

            KnockbackResistance = 0.0f;
            BaseYOffsetWorld = 15f;
        }
    }
}