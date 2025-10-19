using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using IsometricGame.Classes.Particles;
using System.Collections.Generic;
using System;

namespace IsometricGame.Classes
{
    public class EnemyBase : Sprite
    {
        public static SoundEffect HitSound { get; set; }

        protected Dictionary<string, Texture2D> _sprites;
        protected string _currentDirection = "south";
        protected Explosion _explosion;

        public int Life { get; protected set; }
        public float Speed { get; protected set; } = 3.0f;        public int Weight { get; protected set; }

        private double _lastHit;
        private double _hitFlashDuration = 100;
        private bool _isHit = false;


        public EnemyBase(Vector3 worldPos, List<string> spriteKeys) : base(null, worldPos)
        {
            _sprites = LoadSprites(spriteKeys);
            _explosion = new Explosion();

            if (_sprites.Count > 0 && _sprites.ContainsKey(_currentDirection))
            {
                UpdateTexture(_sprites[_currentDirection]);
                if (Texture != null)
                    Origin = new Vector2(Texture.Width / 2f, Texture.Height);
            }

            if (Texture != null)
                _explosion.Create(this.ScreenPosition.X, this.ScreenPosition.Y - Texture.Height / 2f, speed: -5);
            Life = 3;
            Weight = 1;
        }

        public static void LoadAssets(AssetManager assets)
        {
            HitSound = assets.Sounds["hit"];
        }

        private Dictionary<string, Texture2D> LoadSprites(List<string> spriteKeys)
        {
            var dict = new Dictionary<string, Texture2D>();
            foreach (var key in spriteKeys)
            {
                if (GameEngine.Assets.Images.TryGetValue(key, out Texture2D texture))
                {
                    string direction = key.Contains("south") ? "south" : key.Contains("west") ? "west" : "south";
                    dict[direction] = texture;
                }
            }
            return dict;
        }

        public void Damage(GameTime gameTime)
        {
            _lastHit = gameTime.TotalGameTime.TotalMilliseconds;
            _isHit = true;            if (Texture != null)
                _explosion.Create(this.ScreenPosition.X, this.ScreenPosition.Y - Texture.Height / 2f);

            Life -= 1;
            if (Life <= 0)
            {
                GameEngine.ScreenShake = Math.Max(GameEngine.ScreenShake, 5);
                Kill();
            }
        }

        protected virtual void Shoot()
        {
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved) return;

            for (int i = 0; i < Weight; i++)
            {
                if (GameEngine.Random.Next(0, 500 / Weight) < 1)                {
                    Vector2 currentPosXY = new Vector2(this.WorldPosition.X, this.WorldPosition.Y);
                    Vector2 playerPosXY = new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y);
                    Vector2 direction = playerPosXY - currentPosXY;
                    if (direction != Vector2.Zero)
                        direction.Normalize();

                    var bullets = Bullet.CreateBullets(
                        pattern: "single",
                        worldPos: currentPosXY,
                        worldDirection: direction,
                        isFromPlayer: false
                    );

                    foreach (var bullet in bullets)
                    {
                        GameEngine.EnemyBullets.Add(bullet);
                        GameEngine.AllSprites.Add(bullet);
                    }
                }
            }
        }
        public virtual void Move(GameTime gameTime, float dt)
        {
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved)
            {
                WorldVelocity = Vector2.Zero;
                return;
            }
            ;

            Vector2 direction = new Vector2(
                GameEngine.Player.WorldPosition.X - this.WorldPosition.X,
                GameEngine.Player.WorldPosition.Y - this.WorldPosition.Y
            );
            float distance = direction.Length();

            if (distance > 0.5f)
            {
                direction.Normalize();
                WorldVelocity = direction * Speed;
            }
            else
            {
                WorldVelocity = Vector2.Zero;
            }
            if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            {
                _currentDirection = direction.X > 0 ? "south" : "west";            }
            else if (direction.LengthSquared() > 0)
            {
                _currentDirection = direction.Y > 0 ? "south" : "west";            }

            if (_sprites.ContainsKey(_currentDirection))
            {
                UpdateTexture(_sprites[_currentDirection]);
                if (Texture != null)
                    Origin = new Vector2(Texture.Width / 2f, Texture.Height);
            }
                
        }
        public override void Update(GameTime gameTime, float dt)
        {
            Move(gameTime, dt);
            Shoot();
            _explosion.Update(dt);
            if (_isHit && gameTime.TotalGameTime.TotalMilliseconds - _lastHit > _hitFlashDuration)
            {
                _isHit = false;
            }
            base.Update(gameTime, dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color tint = _isHit ? Color.Red : Color.White;
            if (Texture != null && !IsRemoved)
            {
                // --- INÍCIO DA ADIÇÃO ---
                Vector2 drawPosition = new Vector2(
                    MathF.Round(ScreenPosition.X),
                    MathF.Round(ScreenPosition.Y)
                );
                // --- FIM DA ADIÇÃO ---

                float depth = IsoMath.GetDepth(WorldPosition);

                spriteBatch.Draw(Texture,
                                 drawPosition, // Usa a posição arredondada
                                 null,
                                 tint,
                                 0f,
                                 Origin,
                                 1.0f,
                                 SpriteEffects.None,
                                 depth);
            }

            _explosion.Draw(spriteBatch);
        }
    }
}