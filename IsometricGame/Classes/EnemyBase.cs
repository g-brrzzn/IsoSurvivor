using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using IsometricGame.Classes.Particles;
using IsometricGame.Classes.UI;
using IsometricGame.Classes.Items;using System.Collections.Generic;
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
        public float Speed { get; protected set; } = 3.0f;
        public int Weight { get; protected set; }
        public float KnockbackResistance { get; protected set; } = 0f;
        protected float ChestDropChance { get; set; } = 0.01f;

        private double _lastHit;
        private double _hitFlashDuration = 100;
        private bool _isHit = false;

        private Vector2 _knockbackVelocity = Vector2.Zero;
        private float _friction = 5f;

        public EnemyBase(Vector3 worldPos, List<string> spriteKeys) : base(null, worldPos)
        {
            _sprites = LoadSprites(spriteKeys);
            _explosion = new Explosion();

            UpdateSpriteDirection(Vector2.Zero);

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

        private void UpdateSpriteDirection(Vector2 direction)
        {
            if (_sprites.Count > 0)
            {
                if (direction.LengthSquared() > 0.1f && _sprites.Count > 1)
                {
                    if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                        _currentDirection = direction.X > 0 ? "south" : "west";
                    else
                        _currentDirection = direction.Y > 0 ? "south" : "north";
                }

                if (_sprites.ContainsKey(_currentDirection))
                    UpdateTexture(_sprites[_currentDirection]);
                else if (_sprites.ContainsKey("default"))
                    UpdateTexture(_sprites["default"]);

                if (Texture != null)
                    Origin = new Vector2(Texture.Width / 2f, Texture.Height);
            }
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

            Vector3 textPos = new Vector3(this.WorldPosition.X, this.WorldPosition.Y, this.WorldPosition.Z + 2.0f);
            Color damageColor = (amount > 5) ? Color.Yellow : Color.White;

            var popup = new FloatingText(amount.ToString(), textPos, damageColor);
            GameEngine.FloatingTexts.Add(popup);

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
                DropLoot();                Kill();
            }
        }

        private void DropExperience()
        {
            var gem = new ExperienceGem(this.WorldPosition, Weight);
            GameEngine.AllSprites.Add(gem);
        }

        private void DropLoot()
        {
            double rng = GameEngine.Random.NextDouble();
            if (ChestDropChance > 0 && rng < ChestDropChance)
            {
                var chest = new Chest(this.WorldPosition);
                GameEngine.Items.Add(chest);
                GameEngine.AllSprites.Add(chest);
                GameEngine.Assets.Sounds["menu_confirm"].Play(0.8f, -0.2f, 0f);
                return;
            }

            rng = GameEngine.Random.NextDouble();
            if (rng < 0.01)
            {
                var item = new ItemDrop(this.WorldPosition, ItemType.Magnet);
                GameEngine.Items.Add(item);
                GameEngine.AllSprites.Add(item);
                return;
            }
            if (rng < 0.001)
            {
                var item = new ItemDrop(this.WorldPosition, ItemType.HealthPotion);
                GameEngine.Items.Add(item);
                GameEngine.AllSprites.Add(item);
            }
        }

        public virtual void Move(GameTime gameTime, float dt)
        {
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved)
            {
                WorldVelocity = Vector2.Zero;
                return;
            }

            if (_knockbackVelocity.LengthSquared() > 0.5f)
            {
                WorldVelocity = Vector2.Zero;
                return;
            }

            Vector2 toPlayer = new Vector2(
                GameEngine.Player.WorldPosition.X - WorldPosition.X,
                GameEngine.Player.WorldPosition.Y - WorldPosition.Y
            );
            if (toPlayer != Vector2.Zero) toPlayer.Normalize();

            Vector2 separation = Vector2.Zero;
            int neighbors = 0;
            float separationRadius = 0.8f;
            foreach (var other in GameEngine.AllEnemies)
            {
                if (other == this || other.IsRemoved) continue;

                float distSq = Vector2.DistanceSquared(
                    new Vector2(WorldPosition.X, WorldPosition.Y),
                    new Vector2(other.WorldPosition.X, other.WorldPosition.Y));

                if (distSq < separationRadius * separationRadius)
                {
                    Vector2 push = new Vector2(WorldPosition.X - other.WorldPosition.X, WorldPosition.Y - other.WorldPosition.Y);
                    if (push.LengthSquared() > 0)
                    {
                        push.Normalize();
                        float weight = 1.0f - (MathF.Sqrt(distSq) / separationRadius);
                        separation += push * weight;
                        neighbors++;
                    }
                }
            }

            if (neighbors > 0)
            {
                separation /= neighbors;
                if (separation != Vector2.Zero) separation.Normalize();
            }

            Vector2 finalDir = (toPlayer * 0.7f) + (separation * 0.5f);
            if (finalDir != Vector2.Zero) finalDir.Normalize();

            WorldVelocity = finalDir * Speed;

            UpdateSpriteDirection(finalDir);
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