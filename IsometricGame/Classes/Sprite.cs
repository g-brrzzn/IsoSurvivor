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
            WorldPosition = worldPosition;
            WorldVelocity = Vector2.Zero;

            // Esta é a mudança principal:
            // Precisamos chamar UpdateTexture para definir a Texture E a Origin.
            // A Origin correta (base da imagem) é essencial para o sorting isométrico.
            UpdateTexture(texture);

            UpdateScreenPosition();
        }

        protected void UpdateTexture(Texture2D newTexture)
        {
            if (newTexture != null)
            {
                Texture = newTexture;
                Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
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
                // --- INÍCIO DA MODIFICAÇÃO ---
                // Arredonda as coordenadas X e Y para o inteiro mais próximo
                // Isso "trava" o sprite na grade de pixels, evitando micro-desalinhamentos.
                Vector2 drawPosition = new Vector2(
                    MathF.Round(ScreenPosition.X),
                    MathF.Round(ScreenPosition.Y)
                );
                // --- FIM DA MODIFICAÇÃO ---

                float depth = IsoMath.GetDepth(WorldPosition);

                spriteBatch.Draw(Texture,
                                 // Usa a posição arredondada
                                 drawPosition,
                                 null,
                                 Color.White,
                                 0f,
                                 Origin,
                                 1.0f,
                                 SpriteEffects.None,
                                 depth);
            }
        }

        public void Kill() => IsRemoved = true;
    }
}