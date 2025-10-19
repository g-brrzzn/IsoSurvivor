using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace IsometricGame.States
{
    public static class DrawUtils
    {
        public static void DrawText(SpriteBatch spriteBatch, string text, SpriteFont font, Vector2 position, Color color, float depth = 1.0f)
        {
            Vector2 origin = font.MeasureString(text) / 2f;
            spriteBatch.DrawString(font, text, position, color, 0, origin, 1.0f, SpriteEffects.None, depth);
        }

        // Nova função para desenhar texto em coordenadas de TELA
        public static void DrawTextScreen(SpriteBatch spriteBatch, string text, SpriteFont font, Vector2 position, Color color, float depth = 1.0f)
        {
            Vector2 origin = font.MeasureString(text) / 2f;
            spriteBatch.DrawString(font, text, position, color, 0, origin, 1.0f, SpriteEffects.None, depth);
        }

        public static void DrawVerticalGradient(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Color color1, Color color2)
        {
            Texture2D pixel = GameEngine.Assets.Images["pixel"];
            int width = Constants.InternalResolution.X;
            int height = Constants.InternalResolution.Y;
            Vector2 cameraTopLeft = Game1.Camera.ScreenToWorld(Vector2.Zero);

            for (int y = 0; y < height; y++)
            {
                float amount = (float)y / (float)height;
                Color lerpedColor = Color.Lerp(color1, color2, amount);
                spriteBatch.Draw(pixel, new Rectangle((int)cameraTopLeft.X, (int)cameraTopLeft.Y + y, width, 1), lerpedColor);
            }
        }

        public static void DrawMenu(SpriteBatch spriteBatch, List<string> options, string title, int selected)
        {
            Texture2D pixel = GameEngine.Assets.Images["pixel"];
            SpriteFont fontTitle = GameEngine.Assets.Fonts["captain_80"];
            SpriteFont fontOption = GameEngine.Assets.Fonts["captain_42"];

            // Não converte mais para "mundo", usa o centro da tela interna
            Vector2 center = new Vector2(Constants.InternalResolution.X / 2f, Constants.InternalResolution.Y / 2f);

            if (!string.IsNullOrEmpty(title))
            {
                // Usa coordenadas de tela
                Vector2 titlePos = new Vector2(Constants.InternalResolution.X / 2f, Constants.InternalResolution.Y / 2f - 150);
                // Usa a nova função DrawTextScreen
                DrawTextScreen(spriteBatch, title, fontTitle, titlePos, Constants.TitleYellow1, 1.0f);
            }

            float yGap = 50f;
            // Usa o centro da tela
            Vector2 startPosScreen = new Vector2(center.X, center.Y - (options.Count / 2f * yGap) + 50f);

            for (int i = 0; i < options.Count; i++)
            {
                Color color = (i == selected) ? Constants.GameColor : Color.White;
                Vector2 posScreen = new Vector2(startPosScreen.X, startPosScreen.Y + i * yGap);

                // Usa a nova função DrawTextScreen
                DrawTextScreen(spriteBatch, options[i], fontOption, posScreen, color, 1.0f);

                if (i == selected)
                {
                    float textWidth = fontOption.MeasureString(options[i]).X / 2f;
                    // Usa coordenadas de tela
                    Vector2 leftMarkerPos = new Vector2(posScreen.X - textWidth - 20, posScreen.Y);
                    Vector2 rightMarkerPos = new Vector2(posScreen.X + textWidth + 20, posScreen.Y);

                    // Usa a nova função DrawTextScreen
                    DrawTextScreen(spriteBatch, "|", fontOption, leftMarkerPos, color, 1.0f);
                    DrawTextScreen(spriteBatch, "|", fontOption, rightMarkerPos, color, 1.0f);
                }
            }
            // --- FIM DA MODIFICAÇÃO ---
        }
    }
}