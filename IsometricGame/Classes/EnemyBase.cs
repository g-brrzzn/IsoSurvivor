using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using IsometricGame.Classes.Particles;
using System.Collections.Generic;
using System;
using IsometricGame.Pathfinding;

namespace IsometricGame.Classes
{
    public class EnemyBase : Sprite
    {
        public static SoundEffect HitSound { get; set; }

        protected Dictionary<string, Texture2D> _sprites;
        protected string _currentDirection = "south";
        protected Explosion _explosion;

        public int Life { get; protected set; }
        public float Speed { get; protected set; } = 3.0f;
        public int Weight { get; protected set; }        public float KnockbackResistance { get; protected set; } = 0f;

        private double _lastHit;
        private double _hitFlashDuration = 100;
        private const float _collisionRadius = 0.35f;
        private bool _isHit = false;

        private Vector2 _knockbackVelocity = Vector2.Zero;

        private float _friction = 5f;

        protected List<Vector3> _currentPath;
        protected float _pathTimer = 0f;
        protected const float _pathRefreshTime = 0.5f;
        protected const float _nodeReachedThreshold = 0.5f;

        public EnemyBase(Vector3 worldPos, List<string> spriteKeys) : base(null, worldPos)
        {
            _sprites = LoadSprites(spriteKeys);
            _explosion = new Explosion();

            if (_sprites.Count > 0)
            {
                if (_sprites.ContainsKey(_currentDirection)) UpdateTexture(_sprites[_currentDirection]);
                else if (_sprites.ContainsKey("default")) UpdateTexture(_sprites["default"]);

                if (Texture != null)
                    Origin = new Vector2(Texture.Width / 2f, Texture.Height);
            }

            if (Texture != null)
            {
                _explosion.Create(
                    this.ScreenPosition.X,
                    this.ScreenPosition.Y - Texture.Height / 2f,
                    color: Color.Black * 0.5f,
                    count: 15,
                    eRange: 30,
                    speed: 250f,
                    rangeVariation: 0.5f
                );
            }

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
                    string direction = "default";
                    if (key.Contains("south")) direction = "south";
                    else if (key.Contains("west")) direction = "west";
                    else if (key.Contains("north")) direction = "north";
                    else if (key.Contains("east")) direction = "east";

                    dict[direction] = texture;
                }
            }
            return dict;
        }

        public void ApplyKnockback(Vector2 force)
        {
            float effectiveForce = 1.0f - KnockbackResistance;
            _knockbackVelocity += force * effectiveForce;
        }

        public void Damage(GameTime gameTime, int amount)
        {
            _lastHit = gameTime.TotalGameTime.TotalMilliseconds;
            _isHit = true;

            if (Texture != null)
                _explosion.Create(
                    this.ScreenPosition.X,
                    this.ScreenPosition.Y - Texture.Height / 2f,
                    color: Color.White,
                    count: 5,
                    eRange: 20,
                    speed: 300f
                );

            Life -= amount;
            if (Life <= 0)
            {
                GameEngine.ScreenShake = Math.Max(GameEngine.ScreenShake, 3);
                DropExperience();
                Kill();
            }
        }

        private void DropExperience()
        {
            var gem = new ExperienceGem(this.WorldPosition, Weight);
            GameEngine.AllSprites.Add(gem);
        }

        public virtual void Move(GameTime gameTime, float dt)
        {
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved)
            {
                WorldVelocity = Vector2.Zero;
                return;
            }

            if (_knockbackVelocity.Length() > 1.0f)
            {
                WorldVelocity = Vector2.Zero;
                return;
            }

            _pathTimer -= dt;

            if (_pathTimer <= 0f)
            {
                _pathTimer = _pathRefreshTime;
                Vector2 direction = new Vector2(
                    GameEngine.Player.WorldPosition.X - WorldPosition.X,
                    GameEngine.Player.WorldPosition.Y - WorldPosition.Y
                );

                if (direction != Vector2.Zero) direction.Normalize();
                WorldVelocity = direction * Speed;

                if (_sprites.Count > 1)
                {
                    if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                        _currentDirection = direction.X > 0 ? "south" : "west";
                    else
                        _currentDirection = direction.Y > 0 ? "south" : "west";
                }
            }

            if (_sprites.ContainsKey(_currentDirection)) UpdateTexture(_sprites[_currentDirection]);
            else if (_sprites.ContainsKey("default")) UpdateTexture(_sprites["default"]);

            if (Texture != null) Origin = new Vector2(Texture.Width / 2f, Texture.Height);
        }

        public override void Update(GameTime gameTime, float dt)
        {
            Move(gameTime, dt);

            _knockbackVelocity = Vector2.Lerp(_knockbackVelocity, Vector2.Zero, _friction * dt);

            Vector2 finalVelocity = WorldVelocity + _knockbackVelocity;
            Vector2 movement = finalVelocity * dt;

            Vector3 nextPos = WorldPosition + new Vector3(movement.X, movement.Y, 0);

            if (!IsCollidingAt(nextPos))
            {
                WorldPosition += new Vector3(movement.X, movement.Y, 0);
            }

            UpdateScreenPosition();
            _explosion.Update(dt);

            if (_isHit && gameTime.TotalGameTime.TotalMilliseconds - _lastHit > _hitFlashDuration)
                _isHit = false;
        }

        private bool IsCollidingAt(Vector3 futurePosition)
        {
            Vector3 cell = new Vector3(MathF.Round(futurePosition.X), MathF.Round(futurePosition.Y), futurePosition.Z);
            return GameEngine.SolidTiles.ContainsKey(cell);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Color tint = _isHit ? Color.Red : Color.White;
            if (Texture != null && !IsRemoved)
            {
                Vector2 drawPosition = new Vector2(MathF.Round(ScreenPosition.X), MathF.Round(ScreenPosition.Y));
                float baseDepth = IsoMath.GetDepth(WorldPosition);
                spriteBatch.Draw(Texture, drawPosition, null, tint, 0f, Origin, 1.0f, SpriteEffects.None, baseDepth);
            }
            _explosion.Draw(spriteBatch);
        }
    }
}