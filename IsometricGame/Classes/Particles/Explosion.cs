using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace IsometricGame.Classes.Particles
{
    public class Explosion
    {
        public static Texture2D PixelTexture { get; set; }

        private List<Particle> _particles = new List<Particle>();

        public void Create(float x, float y,
                           Color? color = null,
                           int count = 80,
                           float eRange = 50,
                           float speed = 400.0f,                           float rangeVariation = 0.4f)
        {
            Vector2 origin = new Vector2(x, y);
            Color partColor = color ?? Constants.TitleYellow1;

            for (int i = 0; i < count; i++)
            {
                float angle = (float)(GameEngine.Random.NextDouble() * 2 * Math.PI);
                float magnitude = (float)(GameEngine.Random.NextDouble() * 0.8 + 0.2) * speed;                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * magnitude;
                float randomFactor = 1.0f + (float)(GameEngine.Random.NextDouble() * 2 - 1) * rangeVariation;
                float particleRange = eRange * randomFactor;

                Particle p = new Particle(origin, origin, vel, partColor, particleRange);
                _particles.Add(p);
            }
        }
        public void Update(float dt)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Position += p.Velocity * dt;                _particles[i] = p;

                if (Vector2.DistanceSquared(p.Position, p.Origin) > (p.MaxRange * p.MaxRange))
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (PixelTexture == null) return;

            foreach (var p in _particles)
            {
                spriteBatch.Draw(PixelTexture, p.Position, null, p.Color, 0f,
                                 new Vector2(0.5f, 0.5f), 6f, SpriteEffects.None, 1.0f);
            }
        }
    }
}