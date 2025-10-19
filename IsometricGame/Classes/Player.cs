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
        private float _speed = 4.0f;
        public int Life { get; private set; }
        public Explosion ExplosionEffect { get; private set; }
        private double _lastHit;
        private double _invincibilityDuration = 1000;
        private bool _isInvincible = false;


        public Player(Vector2 worldPos) : base(null, worldPos)
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
            _firing = input.IsKeyDown("FIRE");
        }

        private void Fire(GameTime gameTime)
        {
            _lastShot = gameTime.TotalGameTime.TotalSeconds;
            Vector2 shotDirection = Vector2.Zero;
            switch (_currentDirection)
            {
                // Mapeia direções da TELA para vetores do MUNDO
                case "north": shotDirection = new Vector2(-1, -1); break; // Cima da Tela
                case "south": shotDirection = new Vector2(1, 1); break;   // Baixo da Tela
                case "west": shotDirection = new Vector2(-1, 1); break;  // Esquerda da Tela
                case "east": shotDirection = new Vector2(1, -1); break;  // Direita da Tela
                default: shotDirection = new Vector2(1, 1); break; // Padrão
            }

            var bullets = Bullet.CreateBullets(
                pattern: "single",
                worldPos: this.WorldPosition,
                worldDirection: shotDirection, // O vetor do mundo já está correto
                isFromPlayer: true
            );

            foreach (var bullet in bullets)
            {
                GameEngine.PlayerBullets.Add(bullet);
                GameEngine.AllSprites.Add(bullet);
            }
            GameEngine.Assets.Sounds["shoot"].Play();
        }

        private void Animate()
        {
            string targetDirection = _currentDirection;

            if (_movingUp) targetDirection = "north";
            else if (_movingDown) targetDirection = "south";
            else if (_movingLeft) targetDirection = "west";
            else if (_movingRight) targetDirection = "east";

            _currentDirection = targetDirection;

            if (_sprites.ContainsKey(_currentDirection))
                UpdateTexture(_sprites[_currentDirection]);
            if (Texture != null)
                Origin = new Vector2(Texture.Width / 2f, Texture.Height);
        }
        public override void Update(GameTime gameTime, float dt)
        {
            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;

            ExplosionEffect.Update(dt);
            Animate();
            _isInvincible = totalMilliseconds - _lastHit < _invincibilityDuration;

            Vector2 worldDirection = Vector2.Zero;
            if (_movingUp) worldDirection += new Vector2(-1, -1); // Cima (Norte)
            if (_movingDown) worldDirection += new Vector2(1, 1);   // Baixo (Sul)
            if (_movingLeft) worldDirection += new Vector2(-1, 1);  // Esquerda (Oeste)
            if (_movingRight) worldDirection += new Vector2(1, -1);  // Direita (Leste)

            if (worldDirection != Vector2.Zero)
            {
                worldDirection.Normalize();
            }
            WorldVelocity = worldDirection * _speed;
            base.Update(gameTime, dt);

            if (_firing && (gameTime.TotalGameTime.TotalSeconds - _lastShot > _shotDelay))
            {
                Fire(gameTime);
            }
        }

        public void TakeDamage(GameTime gameTime)
        {
            double totalMilliseconds = gameTime.TotalGameTime.TotalMilliseconds;
            if (!_isInvincible)            {
                Life -= 1;
                _lastHit = totalMilliseconds;                _isInvincible = true;
                if (Texture != null)
                    ExplosionEffect.Create(this.ScreenPosition.X, this.ScreenPosition.Y - (this.Texture.Height / 2f), Constants.PlayerColorGreen, speed: -5);
                GameEngine.Assets.Sounds["hit"].Play();
                GameEngine.ScreenShake = 15;
                if (Life <= 0)
                {
                    Kill();                }
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_isInvincible && !IsRemoved)
            {
                float flicker = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 100f;
                if ((int)flicker % 2 == 0)
                {
                    return;
                }
            }

            base.Draw(spriteBatch);
            ExplosionEffect.Draw(spriteBatch);
        }
    }
}