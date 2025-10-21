using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using IsometricGame.Classes.Particles;
using System;

namespace IsometricGame.Classes
{
    public class Player : Sprite
    {
        private Dictionary<string, Texture2D> _sprites;
        private string _currentDirection = "south";

        private double _shotDelay = 0.25;
        private double _lastShot;

        private bool _movingRight, _movingLeft, _movingUp, _movingDown, _firing;
        private float _speed = 6.0f;
        public int Life { get; private set; }
        public Explosion ExplosionEffect { get; private set; }
        private double _lastHit;
        private double _invincibilityDuration = 1000;
        private const float _collisionRadius = .35f;
        private bool _isInvincible = false;


        public Player(Vector3 worldPos) : base(null, worldPos)
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
            if (Texture != null)
                Origin = new Vector2(Texture.Width / 2f, Texture.Height);

            Life = Constants.MaxLife;
            ExplosionEffect = new Explosion();
        }

        public void GetInput(InputManager input)
        {
            _movingLeft = input.IsKeyDown("LEFT");
            _movingRight = input.IsKeyDown("RIGHT");
            _movingUp = input.IsKeyDown("UP");
            _movingDown = input.IsKeyDown("DOWN");

            _firing = input.IsKeyDown("FIRE") || input.IsLeftMouseButtonDown();
        }

        private Vector2 GetAimDirection(InputManager input)
        {
            Vector2 targetWorldPos = GameEngine.TargetWorldPosition;            
            Vector2 playerWorldPos = new Vector2(this.WorldPosition.X, this.WorldPosition.Y);
            Vector2 aimDirection = targetWorldPos - playerWorldPos;
            if (aimDirection.LengthSquared() > 0)
            {
                aimDirection.Normalize();
            }
            return aimDirection;        }

        private void Fire(GameTime gameTime, Vector2 worldAimDirection)
        {
            _lastShot = gameTime.TotalGameTime.TotalSeconds;

            Vector2 shotDirection = worldAimDirection;

            if (shotDirection.LengthSquared() == 0)
            {
                shotDirection = new Vector2(1, 1);
            }

            var bullets = Bullet.CreateBullets(
              pattern: "single",
              worldPos: new Vector2(this.WorldPosition.X, this.WorldPosition.Y),
              worldDirection: shotDirection,                      isFromPlayer: true
            );

            foreach (var bullet in bullets)
            {
                GameEngine.PlayerBullets.Add(bullet);
                GameEngine.AllSprites.Add(bullet);
            }
            GameEngine.Assets.Sounds["shoot"].Play();
        }

        private void Animate(Vector2 worldAimDirection)
        {
            string targetDirection = _currentDirection;

            if (_movingUp) targetDirection = "north";
            else if (_movingDown) targetDirection = "south";
            else if (_movingLeft) targetDirection = "west";
            else if (_movingRight) targetDirection = "east";

            if (worldAimDirection.LengthSquared() > 0)
            {
                Vector2 screenAim = new Vector2(
                    worldAimDirection.X - worldAimDirection.Y,                    worldAimDirection.X + worldAimDirection.Y                );

                if (Math.Abs(screenAim.X) > Math.Abs(screenAim.Y))
                {
                    targetDirection = (screenAim.X > 0) ? "east" : "west";
                }
                else
                {
                    targetDirection = (screenAim.Y > 0) ? "south" : "north";
                }
            }

            _currentDirection = targetDirection;

            if (_sprites.ContainsKey(_currentDirection))
                UpdateTexture(_sprites[_currentDirection]);
            if (Texture != null)
                Origin = new Vector2(Texture.Width / 2f, Texture.Height);
        }

        public override void Update(GameTime gameTime, float dt)
        {
            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;

            GetInput(Game1.InputManagerInstance);
            Vector2 worldAimDirection = GetAimDirection(Game1.InputManagerInstance);

            ExplosionEffect.Update(dt);
            Animate(worldAimDirection);            _isInvincible = totalMilliseconds - _lastHit < _invincibilityDuration;

            Vector2 worldDirection = Vector2.Zero;
            if (_movingUp) worldDirection += new Vector2(-1, -1);
            if (_movingDown) worldDirection += new Vector2(1, 1);
            if (_movingLeft) worldDirection += new Vector2(-1, 1);
            if (_movingRight) worldDirection += new Vector2(1, -1);

            if (worldDirection != Vector2.Zero)
            {
                worldDirection.Normalize();
            }

            Vector2 desiredVelocity = worldDirection * _speed;


            Vector2 movement = desiredVelocity * dt;

            Vector3 nextPos = WorldPosition + new Vector3(movement.X, 0, 0);
            if (IsCollidingAt(nextPos))
            {
                movement.X = 0;
            }

            nextPos = WorldPosition + new Vector3(0, movement.Y, 0);
            if (IsCollidingAt(nextPos))
            {
                movement.Y = 0;
            }

            WorldPosition += new Vector3(movement.X, movement.Y, 0);

            UpdateScreenPosition();

            WorldVelocity = (dt > 0) ? new Vector2(movement.X / dt, movement.Y / dt) : Vector2.Zero;


            if (_firing && (gameTime.TotalGameTime.TotalSeconds - _lastShot > _shotDelay))
            {
                Fire(gameTime, worldAimDirection);            }
        }

        private bool IsCollidingAt(Vector3 futurePosition)
        {
            float baseZ = futurePosition.Z;

            Vector2 posXY = new Vector2(futurePosition.X, futurePosition.Y);
            Vector2 topLeft = posXY + new Vector2(-_collisionRadius, -_collisionRadius);
            Vector2 topRight = posXY + new Vector2(_collisionRadius, -_collisionRadius);
            Vector2 bottomLeft = posXY + new Vector2(-_collisionRadius, _collisionRadius);
            Vector2 bottomRight = posXY + new Vector2(_collisionRadius, _collisionRadius);

            Vector3 cellTL = new Vector3(MathF.Round(topLeft.X), MathF.Round(topLeft.Y), baseZ);
            Vector3 cellTR = new Vector3(MathF.Round(topRight.X), MathF.Round(topRight.Y), baseZ);
            Vector3 cellBL = new Vector3(MathF.Round(bottomLeft.X), MathF.Round(bottomLeft.Y), baseZ);
            Vector3 cellBR = new Vector3(MathF.Round(bottomRight.X), MathF.Round(bottomRight.Y), baseZ);

            if (GameEngine.SolidTiles.ContainsKey(cellTL) || GameEngine.SolidTiles.ContainsKey(cellTL + new Vector3(0, 0, 1)) ||
        GameEngine.SolidTiles.ContainsKey(cellTR) || GameEngine.SolidTiles.ContainsKey(cellTR + new Vector3(0, 0, 1)) ||
        GameEngine.SolidTiles.ContainsKey(cellBL) || GameEngine.SolidTiles.ContainsKey(cellBL + new Vector3(0, 0, 1)) ||
        GameEngine.SolidTiles.ContainsKey(cellBR) || GameEngine.SolidTiles.ContainsKey(cellBR + new Vector3(0, 0, 1)))
            {
                return true;            }

            return false;        }


        public void TakeDamage(GameTime gameTime)
        {
            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;
            if (!_isInvincible)
            {
                Life -= 1;
                _lastHit = totalMilliseconds;
                _isInvincible = true;
                if (Texture != null)
                    ExplosionEffect.Create(this.ScreenPosition.X, this.ScreenPosition.Y - (this.Texture.Height / 2f), Constants.PlayerColorGreen, speed: -5);
                GameEngine.Assets.Sounds["hit"].Play();
                GameEngine.ScreenShake = 15;

                if (Life <= 0)
                {
                    Kill();
                }
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {

            Color tint = Color.White;
            if (_isInvincible && !IsRemoved)
            {
                float flicker = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 100f;

                if ((int)flicker % 2 == 0)
                {
                    tint = Color.White * 0.5f;                }

            }



            Vector2 drawPosition = new Vector2(
        MathF.Round(ScreenPosition.X),
        MathF.Round(ScreenPosition.Y)
      );

            float baseDepth = IsoMath.GetDepth(WorldPosition);
            const float zLayerBias = 0.001f;
            const float entityBias = 0.0001f;
            float finalDepth = baseDepth - (WorldPosition.Z * zLayerBias) - entityBias;
            finalDepth = MathHelper.Clamp(finalDepth, 0f, 1f);

            spriteBatch.Draw(Texture,
                      drawPosition,
              null,
              tint,                      0f,
              Origin,
              1.0f,
                      SpriteEffects.None,
              finalDepth);

            ExplosionEffect.Draw(spriteBatch);
        }
    }
}