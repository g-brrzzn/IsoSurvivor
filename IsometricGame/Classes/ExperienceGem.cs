using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace IsometricGame.Classes
{
    public class ExperienceGem : Sprite
    {
        public int Value { get; private set; }
        private float _magnetSpeed = 0f;
        private const float _acceleration = 15f;
        private const float _maxSpeed = 600f;        private bool _isMagnetized = false;
        private float _floatTimer = 0f;

        public ExperienceGem(Vector3 worldPos, int value) : base(null, worldPos)
        {
            Value = value;

            string textureName = "gem_1";
            if (value >= 20)
            {
                textureName = "gem_50";
            }
            else if (value >= 5)
            {
                textureName = "gem_10";
            }

            if (GameEngine.Assets.Images.ContainsKey(textureName))
                UpdateTexture(GameEngine.Assets.Images[textureName]);
            else if (GameEngine.Assets.Images.ContainsKey("gem_1"))
                UpdateTexture(GameEngine.Assets.Images["gem_1"]);
            else
                UpdateTexture(GameEngine.Assets.Images["bullet_player"]);

            BaseYOffsetWorld = 10f;

            _floatTimer = (float)GameEngine.Random.NextDouble() * 10f;
        }
        public void ForceMagnetize()
        {
            _isMagnetized = true;
            _magnetSpeed = 100f;        }

        public override void Update(GameTime gameTime, float dt)
        {
            base.Update(gameTime, dt);

            if (GameEngine.Player == null || GameEngine.Player.IsRemoved) return;

            _floatTimer += dt * 5f;
            if (!_isMagnetized)
            {
                BaseYOffsetWorld = (float)Math.Sin(_floatTimer) * 3f + 5f;
            }

            float distToPlayerSq = Vector2.DistanceSquared(
                new Vector2(WorldPosition.X, WorldPosition.Y),
                new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y));

            float magnetRadiusSq = GameEngine.Player.MagnetRange * GameEngine.Player.MagnetRange;

            if (distToPlayerSq < magnetRadiusSq || _isMagnetized)
            {
                _isMagnetized = true;
                _magnetSpeed += _acceleration * dt * 60f;
                _magnetSpeed = Math.Min(_magnetSpeed, _maxSpeed);

                Vector2 direction = new Vector2(
                    GameEngine.Player.WorldPosition.X - WorldPosition.X,
                    GameEngine.Player.WorldPosition.Y - WorldPosition.Y
                );

                if (direction != Vector2.Zero) direction.Normalize();

                WorldPosition += new Vector3(direction.X, direction.Y, 0) * _magnetSpeed * dt * 0.05f;
                BaseYOffsetWorld = MathHelper.Lerp(BaseYOffsetWorld, 15f, dt * 10);
                if (distToPlayerSq < 0.5f * 0.5f)
                {
                    bool leveledUp = GameEngine.Player.AddExperience(Value);

                    if (Value >= 10)
                        GameEngine.Assets.Sounds["menu_select"].Play(0.6f, 0.8f, 0f);
                    else
                        GameEngine.Assets.Sounds["menu_select"].Play(0.3f, 0.5f, 0f);

                    Kill();
                }
            }
        }
    }
}