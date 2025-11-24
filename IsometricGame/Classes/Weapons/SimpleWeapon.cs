using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace IsometricGame.Classes.Weapons
{
    public class SimpleWeapon : WeaponBase
    {
        private float _range = 8.0f;
        private int _bonusProjectiles = 0;
        public SimpleWeapon(Player owner) : base(owner)
        {
            Name = "Magic Wand";
            BaseCooldown = 1.0f;
            BaseDamage = 2;        }

        public override void LevelUp()
        {
            base.LevelUp();
            if (Level % 2 == 0)
            {
                _bonusProjectiles += 1;            }
            else
            {
                BaseDamage += 3;            }
            if (Level % 3 == 0)
            {
                BaseCooldown -= 0.1f;
                if (BaseCooldown < 0.2f) BaseCooldown = 0.2f;
            }
        }

        protected override bool TryAttack()
        {
            EnemyBase closestEnemy = null;
            float closestDistSq = float.MaxValue;
            float rangeSq = (_range * _owner.RangeModifier) * (_range * _owner.RangeModifier);

            foreach (var enemy in GameEngine.AllEnemies)
            {
                if (enemy.IsRemoved) continue;
                float distSq = Vector2.DistanceSquared(
                    new Vector2(_owner.WorldPosition.X, _owner.WorldPosition.Y),
                    new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y));

                if (distSq < rangeSq && distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                Vector2 direction = new Vector2(
                    closestEnemy.WorldPosition.X - _owner.WorldPosition.X,
                    closestEnemy.WorldPosition.Y - _owner.WorldPosition.Y
                );
                if (direction != Vector2.Zero) direction.Normalize();

                Fire(direction);
                return true;
            }

            return false;
        }

        private void Fire(Vector2 direction)
        {
            int finalDamage = (int)MathF.Max(1, MathF.Round(BaseDamage * _owner.DamageModifier));

            var options = new BulletOptions
            {
                SpeedScale = 12.0f,
                Piercing = _owner.PiercingCount,                Knockback = _owner.KnockbackStrength,
                Count = _owner.ProjectileCount + _bonusProjectiles,
                SpreadArc = 0.5f,
                Damage = finalDamage,
                Scale = _owner.BulletSizeModifier
            };

            string pattern = (options.Count > 1) ? "multishot" : "single";

            var bullets = Bullet.CreateBullets(
                 pattern: pattern,
                 worldPos: new Vector2(_owner.WorldPosition.X, _owner.WorldPosition.Y),
                 worldDirection: direction,
                 isFromPlayer: true,
                 options: options
            );

            foreach (var bullet in bullets)
            {
                GameEngine.PlayerBullets.Add(bullet);
                GameEngine.AllSprites.Add(bullet);
            }

            float pitch = (float)GameEngine.Random.NextDouble() * 0.2f - 0.1f;
            if (GameEngine.Assets.Sounds.ContainsKey("shoot"))
                GameEngine.Assets.Sounds["shoot"].Play(0.4f, pitch, 0f);
        }
    }
}