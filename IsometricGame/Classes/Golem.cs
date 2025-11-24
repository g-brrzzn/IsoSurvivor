using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes
{
    public class Golem : EnemyBase
    {
        public Golem(Vector3 worldPos)
            : base(worldPos, new List<string> { "golem_idle" })
        {
            Life = 30;
            Weight = 50;            
            Speed = 1.5f;
            KnockbackResistance = 0.75f;

            ChestDropChance = 0f;
        }
    }
}