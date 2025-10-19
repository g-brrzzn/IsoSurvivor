using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace IsometricGame.Classes
{
    public class Sprite
    {
        public Texture2D Texture { get; set; }
        public Vector2 WorldPosition { get; set; }
        public Vector2 ScreenPosition { get; protected set; }
        public Vector2 WorldVelocity { get; set; }

        public bool IsRemoved { get; set; } = false;
        public Vector2 Origin { get; protected set; }

        public Sprite(Texture2D texture, Vector2 worldPosition)
        {
            Texture = texture;
            WorldPosition = worldPosition;
            WorldVelocity = Vector2.Zero;
            UpdateScreenPosition();        }

        protected void UpdateTexture(Texture2D newTexture)
        {
            if (newTexture != null)
            {
                Texture = newTexture;
                Origin = new Vector2(Texture.Width / 2f, Texture.Height);
            }
        }

        protected void UpdateScreenPosition()
        {
            ScreenPosition = IsoMath.WorldToScreen(WorldPosition);
        }
        public virtual void Update(GameTime gameTime, float dt)
        {
            WorldPosition += WorldVelocity * dt;
            UpdateScreenPosition();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null && !IsRemoved)
            {
                float depth = IsoMath.GetDepth(WorldPosition);
                spriteBatch.Draw(Texture,
                                 ScreenPosition,
                                 null,
                                 Color.White,
                                 0f,
                                 Origin,
                                 1.0f,                                 SpriteEffects.None,
                                 depth);            }
        }

        public void Kill() => IsRemoved = true;
    }
}