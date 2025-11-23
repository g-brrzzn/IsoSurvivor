using IsometricGame.Classes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes.Weapons
{
    public abstract class WeaponBase
    {
        protected Player _owner;
        protected float _cooldownTimer;
        public string Name { get; protected set; }
        public int Level { get; protected set; } = 1;
        public float BaseCooldown { get; protected set; } = 1.0f;
        public int BaseDamage { get; protected set; } = 1;

        public WeaponBase(Player owner)
        {
            _owner = owner;
        }

        public virtual void Update(GameTime gameTime, float dt)
        {
            _cooldownTimer -= dt;
            if (_cooldownTimer <= 0)
            {
                if (TryAttack())
                {
                    _cooldownTimer = BaseCooldown;
                }
            }
        }
        protected abstract bool TryAttack();

        public virtual void LevelUp()
        {
            Level++;
        }
    }
}