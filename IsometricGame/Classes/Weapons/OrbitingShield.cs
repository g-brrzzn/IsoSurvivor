using IsometricGame.Classes.Weapons.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes.Weapons
{
    public class OrbitingShield : WeaponBase
    {
        private float _activeDuration = 3.0f;
        private int _bonusAmount = 0;

        public OrbitingShield(Player owner) : base(owner)
        {
            Name = "Orbiting Shield";
            BaseCooldown = 5.0f;
            BaseDamage = 3;
        }

        public override void LevelUp()
        {
            base.LevelUp();
            _bonusAmount++;
            _activeDuration += 0.5f;
        }

        protected override bool TryAttack()
        {
            int count = _owner.ProjectileCount + _bonusAmount;
            float damage = BaseDamage * _owner.DamageModifier;

            for (int i = 0; i < count; i++)
            {
                float angle = (MathHelper.TwoPi / count) * i;

                var projectile = new OrbitingProjectile(
                    _owner,
                    angle,
                    (int)damage,
                    _activeDuration                );

                GameEngine.AllSprites.Add(projectile);
            }

            return true;
        }
    }
}