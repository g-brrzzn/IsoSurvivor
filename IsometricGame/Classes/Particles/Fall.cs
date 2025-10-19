using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;using System.Collections.Generic;

namespace IsometricGame.Classes.Particles
{
    public struct Drop
    {
        public Vector2 Position;
        public Vector2 Velocity;        public int Radius;
        public Color Color;
    }

    public class Fall
    {
        private List<Drop> _drops = new List<Drop>();
        private const float _maxFallSpeed = 150f;
        public Fall(int amount)
        {
            int width = Constants.InternalResolution.X;
            int height = Constants.InternalResolution.Y;
            var rand = GameEngine.Random;

            _drops.Clear();            for (int i = 0; i < amount; i++)
            {
                int radius = rand.Next(1, 4);
                float initialSpeedY = (float)rand.NextDouble() * 40f + 10f;
                _drops.Add(new Drop
                {
                    Position = new Vector2((float)rand.NextDouble() * width, (float)rand.NextDouble() * height),
                    Velocity = new Vector2((float)(rand.NextDouble() - 0.5) * 10f, initialSpeedY),                    Radius = radius,
                    Color = new Color(70, 70, 70)
                });
            }
        }
        public void Update(float gravity = 90f, float wind = 15f, float dt = 1f / 60f)        {
            if (dt <= 0f) return;
            int width = Constants.InternalResolution.X;
            int height = Constants.InternalResolution.Y;

            for (int i = 0; i < _drops.Count; i++)
            {
                var drop = _drops[i];
                drop.Velocity.Y += gravity * dt;
                drop.Velocity.Y = Math.Min(drop.Velocity.Y, _maxFallSpeed);                drop.Velocity.X = MathHelper.Lerp(drop.Velocity.X, wind, 0.01f);                drop.Position += drop.Velocity * dt;
                if (drop.Position.Y - drop.Radius > height)
                {
                    ResetDrop(ref drop, width, height);
                }
                else if (drop.Position.Y + drop.Radius < 0)
                {
                    drop.Position.Y = height + drop.Radius;
                }
                if (drop.Position.X - drop.Radius > width) drop.Position.X = -drop.Radius;
                else if (drop.Position.X + drop.Radius < 0) drop.Position.X = width + drop.Radius;


                _drops[i] = drop;
            }
        }
        private void ResetDrop(ref Drop drop, int width, int height)
        {
            var rand = GameEngine.Random;
            drop.Position = new Vector2((float)rand.NextDouble() * width, -drop.Radius - (float)rand.NextDouble() * 20f);
            float initialSpeedY = (float)rand.NextDouble() * 40f + 10f;            drop.Velocity = new Vector2((float)(rand.NextDouble() - 0.5) * 10f, initialSpeedY);
            drop.Radius = rand.Next(1, 4);
        }
        public void Draw(SpriteBatch spriteBatch, Color? overrideColor = null)
        {
            if (Explosion.PixelTexture == null) return;

            for (int i = 0; i < _drops.Count; i++)
            {
                var drop = _drops[i];
                Color c = overrideColor ?? drop.Color;
                byte alpha = (byte)MathHelper.Clamp(120 + drop.Radius * 40, 60, 255);
                var drawColor = new Color(c.R, c.G, c.B, alpha);
                float scale = drop.Radius * 2f;
                float depth = 0.0f;

                spriteBatch.Draw(
                    Explosion.PixelTexture,
                    drop.Position,
                    null,
                    drawColor,
                    0f,
                    new Vector2(0.5f, 0.5f),                    scale,
                    SpriteEffects.None,
                    depth                );
            }
        }
    }
}