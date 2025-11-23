using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace IsometricGame.Classes.Weapons.Projectiles
{
    public class OrbitingProjectile : Sprite
    {
        private Player _owner;
        private float _angle;
        private float _distance;
        private float _speed;
        private int _damage;
        private float _duration;
        private Dictionary<EnemyBase, double> _hitCooldowns = new Dictionary<EnemyBase, double>();
        private const double HIT_INTERVAL = 0.5;
        public int DamageAmount => _damage;
        public float KnockbackPower { get; set; } = 1.0f;
        public OrbitingProjectile(Player owner, float startAngle, int damage, float duration)
            : base(GameEngine.Assets.Images["gem_1"], owner.WorldPosition)        {
            _owner = owner;
            _angle = startAngle;
            _distance = 3.0f;
            _speed = 3.0f;
            _damage = damage;
            _duration = duration;
        }

        public override void Update(GameTime gameTime, float dt)
        {
            _duration -= dt;
            if (_duration <= 0 || _owner.IsRemoved)
            {
                Kill();
                return;
            }
            float currentDistance = _distance * _owner.RangeModifier;
            _angle += _speed * dt * _owner.AttackSpeedModifier;

            float offsetX = MathF.Cos(_angle) * currentDistance;
            float offsetY = MathF.Sin(_angle) * currentDistance;

            WorldPosition = _owner.WorldPosition + new Vector3(offsetX, offsetY, 0);
            UpdateScreenPosition();
            CleanupCooldowns();
        }

        public bool CanHit(EnemyBase enemy, GameTime gameTime)
        {
            double now = gameTime.TotalGameTime.TotalSeconds;

            if (_hitCooldowns.TryGetValue(enemy, out double nextHitTime))
            {
                if (now >= nextHitTime)
                {
                    _hitCooldowns[enemy] = now + HIT_INTERVAL;
                    return true;
                }
                return false;
            }

            _hitCooldowns[enemy] = now + HIT_INTERVAL;
            return true;
        }

        private void CleanupCooldowns()
        {
            List<EnemyBase> toRemove = new List<EnemyBase>();
            foreach (var kvp in _hitCooldowns)
            {
                if (kvp.Key.IsRemoved) toRemove.Add(kvp.Key);
            }
            foreach (var e in toRemove) _hitCooldowns.Remove(e);
        }
    }
}