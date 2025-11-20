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

        public static void DrawTextScreen(SpriteBatch spriteBatch, string text, SpriteFont font, Vector2 position, Color color, float depth = 1.0f)
        {
            Vector2 origin = font.MeasureString(text) / 2f;
            spriteBatch.DrawString(font, text, position, color, 0, origin, 1.0f, SpriteEffects.None, depth);
        }

        public static List<Rectangle> DrawMenu(SpriteBatch spriteBatch, List<string> options, string title, int selected)
        {
            List<Rectangle> optionRects = new List<Rectangle>();
            Texture2D pixel = GameEngine.Assets.Images["pixel"];
            SpriteFont fontTitle = GameEngine.Assets.Fonts["captain_80"];
            SpriteFont fontOption = GameEngine.Assets.Fonts["captain_42"];

            Vector2 center = new Vector2(Constants.InternalResolution.X / 2f, Constants.InternalResolution.Y / 2f);

            if (!string.IsNullOrEmpty(title))
            {
                Vector2 titlePos = new Vector2(Constants.InternalResolution.X / 2f, Constants.InternalResolution.Y / 2f - 150);
                DrawTextScreen(spriteBatch, title, fontTitle, titlePos, Constants.TitleYellow1, 1.0f);
            }

            float yGap = 50f;
            Vector2 startPosScreen = new Vector2(center.X, center.Y - (options.Count / 2f * yGap) + 50f);

            for (int i = 0; i < options.Count; i++)
            {
                Color color = (i == selected) ? Constants.GameColor : Color.White;
                Vector2 posScreen = new Vector2(startPosScreen.X, startPosScreen.Y + i * yGap);
                Vector2 textSize = fontOption.MeasureString(options[i]);

                Rectangle rect = new Rectangle(
                    (int)(posScreen.X - textSize.X / 2),
                    (int)(posScreen.Y - textSize.Y / 2),
                    (int)textSize.X,
                    (int)textSize.Y
                );
                optionRects.Add(rect);

                DrawTextScreen(spriteBatch, options[i], fontOption, posScreen, color, 1.0f);

                if (i == selected)
                {
                    float textWidth = textSize.X / 2f;
                    Vector2 leftMarkerPos = new Vector2(posScreen.X - textWidth - 20, posScreen.Y);
                    Vector2 rightMarkerPos = new Vector2(posScreen.X + textWidth + 20, posScreen.Y);

                    DrawTextScreen(spriteBatch, "|", fontOption, leftMarkerPos, color, 1.0f);
                    DrawTextScreen(spriteBatch, "|", fontOption, rightMarkerPos, color, 1.0f);
                }
            }
            return optionRects;
        }
    }
}