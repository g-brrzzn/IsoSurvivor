using Microsoft.Xna.Framework;
using System;

namespace IsometricGame
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        public float Zoom { get; private set; }
        public Matrix Transform { get; private set; }

        private int _viewportWidth;
        private int _viewportHeight;

        public Camera(int viewportWidth, int viewportHeight)
        {
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            Zoom = 1.0f;
            Position = Vector2.Zero;
        }

        private void UpdateMatrix()
        {
            Transform = Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                        Matrix.CreateTranslation(_viewportWidth / 2, _viewportHeight / 2, 0);
        }
        public void Follow(Vector2 targetPosition)
        {
            Position = Vector2.Lerp(Position, targetPosition, 0.1f);
            UpdateMatrix();
        }
        public Matrix GetViewMatrix()
        {
            return Transform;
        }
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(Transform));
        }
    }
}