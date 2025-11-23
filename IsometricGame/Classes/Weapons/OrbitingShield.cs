using IsometricGame.Classes.Weapons.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes.Weapons
{
    public class OrbitingShield : WeaponBase
    {
        private float _activeDuration = 5.0f;
        public OrbitingShield(Player owner) : base(owner)
        {
            Name = "Orbiting Shield";
            BaseCooldown = 6.0f;
            BaseDamage = 2;
        }

        protected override bool TryAttack()
        {
            int count = _owner.ProjectileCount + 1;            float damage = BaseDamage * _owner.DamageModifier;

            for (int i = 0; i < count; i++)
            {
                float angle = (MathHelper.TwoPi / count) * i;

                var projectile = new OrbitingProjectile(
                    _owner,
                    angle,
                    (int)damage,
                    _activeDuration
                );

                GameEngine.AllSprites.Add(projectile);
            }

            return true;
        }
    }
}