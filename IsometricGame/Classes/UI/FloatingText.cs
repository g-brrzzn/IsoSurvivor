using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace IsometricGame.Classes.UI
{
    public class FloatingText
    {
        public Vector3 WorldPosition { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public bool IsRemoved { get; private set; } = false;

        private string _text;
        private Color _color;
        private SpriteFont _font;
        private float _lifeTime;
        private float _maxLifeTime;
        private Vector3 _velocity;
        private float _scale;

        public FloatingText(string text, Vector3 worldPos, Color color, float duration = 0.8f)
        {
            _text = text;
            WorldPosition = worldPos;
            _color = color;
            _maxLifeTime = duration;
            _lifeTime = duration;
            _font = GameEngine.Assets.Fonts["captain_32"];            _velocity = new Vector3(0, 0, 15f);            _scale = 1.0f;

            UpdateScreenPosition();
        }

        public void Update(float dt)
        {
            _lifeTime -= dt;
            if (_lifeTime <= 0)
            {
                IsRemoved = true;
                return;
            }
            WorldPosition += _velocity * dt;
            _velocity += new Vector3(0, 0, -20f) * dt;

            UpdateScreenPosition();
        }

        private void UpdateScreenPosition()
        {
            ScreenPosition = IsoMath.WorldToScreen(WorldPosition);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsRemoved) return;

            float alpha = MathHelper.Clamp(_lifeTime / (_maxLifeTime * 0.5f), 0f, 1f);
            Color finalColor = _color * alpha;
            Vector2 origin = _font.MeasureString(_text) / 2;
            spriteBatch.DrawString(_font, _text, ScreenPosition + new Vector2(2, 2), Color.Black * alpha, 0f, origin, _scale, SpriteEffects.None, 1f);
            spriteBatch.DrawString(_font, _text, ScreenPosition, finalColor, 0f, origin, _scale, SpriteEffects.None, 1f);
        }
    }
}