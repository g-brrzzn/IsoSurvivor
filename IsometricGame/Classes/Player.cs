using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using IsometricGame.Classes.Particles;
using IsometricGame.Classes.Weapons;using System;

namespace IsometricGame.Classes
{
    public class Player : Sprite
    {
        private Dictionary<string, Texture2D> _sprites;
        private string _currentDirection = "south";
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int ExperienceToNextLevel { get; private set; } = 5;
        public int MaxLife { get; private set; }
        public int Life { get; private set; }
        public float MoveSpeedModifier { get; private set; } = 1.0f;
        public float DamageModifier { get; private set; } = 1.0f;
        public float BulletSizeModifier { get; private set; } = 1.0f;
        public float AttackSpeedModifier { get; private set; } = 1.0f;        public float RangeModifier { get; private set; } = 1.0f;        public float KnockbackStrength { get; private set; } = 0f;
        public float MagnetRange { get; private set; } = 3.5f;
        public int ProjectileCount { get; private set; } = 1;
        public int PiercingCount { get; private set; } = 0;
        public List<WeaponBase> Weapons { get; private set; }
        private bool _movingRight, _movingLeft, _movingUp, _movingDown;
        private float _baseSpeed = 6.0f;
        private const float _collisionRadius = .35f;
        public Explosion ExplosionEffect { get; private set; }
        private double _lastHitTime;
        private double _invincibilityDurationMs = 1000;
        private bool _isInvincible = false;

        public Player(Vector3 worldPos) : base(null, worldPos)
        {
            LoadPlayerSprites();

            if (Texture != null)
                Origin = new Vector2(Texture.Width / 2f, Texture.Height);
            MaxLife = Constants.MaxLife;
            Life = MaxLife;
            ExplosionEffect = new Explosion();
            Weapons = new List<WeaponBase>();
            Weapons.Add(new SimpleWeapon(this));
        }

        private void LoadPlayerSprites()
        {
            _sprites = new Dictionary<string, Texture2D>
            {
                { "south", GameEngine.Assets.Images["player_idle_south"] },
                { "west", GameEngine.Assets.Images["player_idle_west"] },
                { "north", GameEngine.Assets.Images["player_idle_north"] },
                { "east", GameEngine.Assets.Images["player_idle_east"] }
            };

            if (_sprites.ContainsKey(_currentDirection))
                UpdateTexture(_sprites[_currentDirection]);
        }

        public void BuffAttackSpeed(float percentage)
        {
            AttackSpeedModifier += percentage;
        }

        public void BuffMoveSpeed(float percentage) => MoveSpeedModifier += percentage;

        public void BuffRange(float percentage) => RangeModifier += percentage;

        public void BuffMaxLife(int amount)
        {
            MaxLife += amount;
            Life += amount;
        }

        public void Heal(int amount) => Life = Math.Min(Life + amount, MaxLife);

        public void BuffKnockback(float amount) => KnockbackStrength += amount;

        public void BuffMagnet(float amount) => MagnetRange += amount;

        public void BuffProjectileCount(int amount) => ProjectileCount += amount;

        public void BuffPiercing(int amount) => PiercingCount += amount;

        public void BuffDamage(float percentage) => DamageModifier += percentage;

        public void BuffBulletSize(float percentage) => BulletSizeModifier += percentage;

        public bool AddExperience(int amount)
        {
            Experience += amount;
            if (Experience >= ExperienceToNextLevel) return true;
            return false;
        }

        public void ConfirmLevelUp()
        {
            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.2f) + 5;
            if (Life < MaxLife) Life++;
        }

        public void GetInput(InputManager input)
        {
            _movingLeft = input.IsKeyDown("LEFT");
            _movingRight = input.IsKeyDown("RIGHT");
            _movingUp = input.IsKeyDown("UP");
            _movingDown = input.IsKeyDown("DOWN");
        }

        public override void Update(GameTime gameTime, float dt)
        {
            GetInput(Game1.InputManagerInstance);
            Vector2 worldDirection = Vector2.Zero;
            if (_movingUp) worldDirection += new Vector2(-1, -1);
            if (_movingDown) worldDirection += new Vector2(1, 1);
            if (_movingLeft) worldDirection += new Vector2(-1, 1);
            if (_movingRight) worldDirection += new Vector2(1, -1);

            if (worldDirection != Vector2.Zero) worldDirection.Normalize();

            Animate(worldDirection);
            foreach (var weapon in Weapons)
            {
                weapon.Update(gameTime, dt);
            }
            ExplosionEffect.Update(dt);
            _isInvincible = gameTime.TotalGameTime.TotalMilliseconds - _lastHitTime < _invincibilityDurationMs;
            float currentSpeed = _baseSpeed * MoveSpeedModifier;
            Vector2 movement = worldDirection * currentSpeed * dt;

            Vector3 nextPos = WorldPosition + new Vector3(movement.X, 0, 0);
            if (IsCollidingAt(nextPos)) movement.X = 0;

            nextPos = WorldPosition + new Vector3(0, movement.Y, 0);
            if (IsCollidingAt(nextPos)) movement.Y = 0;

            WorldPosition += new Vector3(movement.X, movement.Y, 0);
            UpdateScreenPosition();
            WorldVelocity = (dt > 0) ? new Vector2(movement.X / dt, movement.Y / dt) : Vector2.Zero;
        }

        private void Animate(Vector2 moveDirection)
        {
            string targetDirection = _currentDirection;
            if (moveDirection.LengthSquared() > 0)
            {
                if (moveDirection.Y > 0) targetDirection = "south";
                else if (moveDirection.Y < 0) targetDirection = "north";
                else if (moveDirection.X > 0) targetDirection = "east";
                else if (moveDirection.X < 0) targetDirection = "west";
            }
            _currentDirection = targetDirection;
            if (_sprites.ContainsKey(_currentDirection)) UpdateTexture(_sprites[_currentDirection]);
            if (Texture != null) Origin = new Vector2(Texture.Width / 2f, Texture.Height);
        }

        private bool IsCollidingAt(Vector3 futurePosition)
        {
            float baseZ = futurePosition.Z;
            Vector2 posXY = new Vector2(futurePosition.X, futurePosition.Y);
            Vector2 topLeft = posXY + new Vector2(-_collisionRadius, -_collisionRadius);
            Vector2 bottomRight = posXY + new Vector2(_collisionRadius, _collisionRadius);

            Vector3 cellTL = new Vector3(MathF.Round(topLeft.X), MathF.Round(topLeft.Y), baseZ);
            Vector3 cellBR = new Vector3(MathF.Round(bottomRight.X), MathF.Round(bottomRight.Y), baseZ);

            return GameEngine.SolidTiles.ContainsKey(cellTL) || GameEngine.SolidTiles.ContainsKey(cellBR);
        }

        public void TakeDamage(GameTime gameTime)
        {
            if (!_isInvincible)
            {
                Life -= 1;
                _lastHitTime = gameTime.TotalGameTime.TotalMilliseconds;
                _isInvincible = true;

                if (Texture != null)
                    ExplosionEffect.Create(
                    this.ScreenPosition.X,
                    this.ScreenPosition.Y - Texture.Height / 2f,
                    color: Constants.PlayerColorGreen,
                    count: 50,
                    eRange: 20,
                    speed: 300f
                );

                GameEngine.Assets.Sounds["hit"].Play();
                GameEngine.ScreenShake = 15;

                if (Life <= 0) Kill();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color tint = Color.White;
            if (_isInvincible && !IsRemoved)
            {
                if (((int)(DateTime.Now.TimeOfDay.TotalMilliseconds / 100f)) % 2 == 0) tint = Color.White * 0.5f;
            }

            Vector2 drawPosition = new Vector2(MathF.Round(ScreenPosition.X), MathF.Round(ScreenPosition.Y));
            float baseDepth = IsoMath.GetDepth(WorldPosition);
            float finalDepth = MathHelper.Clamp(baseDepth - 0.0001f, 0f, 1f);

            spriteBatch.Draw(Texture, drawPosition, null, tint, 0f, Origin, 1.0f, SpriteEffects.None, finalDepth);
            ExplosionEffect.Draw(spriteBatch);
        }
    }
}