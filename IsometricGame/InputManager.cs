using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IsometricGame
{
    public class InputManager
    {
        private KeyboardState _currentKeyState, _previousKeyState;
        private MouseState _currentMouseState, _previousMouseState;
        private Rectangle _renderDestination;
        private Point _internalResolution;
        public Vector2 InternalMousePosition { get; private set; }


        private static readonly Dictionary<string, Keys[]> _controls = new Dictionary<string, Keys[]>
    {
      { "UP", new[] { Keys.W, Keys.Up } },
      { "DOWN", new[] { Keys.S, Keys.Down } },
      { "LEFT", new[] { Keys.A, Keys.Left } },
      { "RIGHT", new[] { Keys.D, Keys.Right } },
      { "FIRE", new[] { Keys.Space } },
      { "START", new[] { Keys.Enter, Keys.Space } },
      { "ESC", new[] { Keys.Escape } }
    };

        public void SetScreenConversion(Rectangle renderDestination, Point internalResolution)
        {
            _renderDestination = renderDestination;
            _internalResolution = internalResolution;
        }

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            if (_renderDestination.Width > 0 && _renderDestination.Height > 0)
            {
                float mouseInRenderX = _currentMouseState.X - _renderDestination.X;
                float mouseInRenderY = _currentMouseState.Y - _renderDestination.Y;

                float internalX = mouseInRenderX / (float)_renderDestination.Width * _internalResolution.X;
                float internalY = mouseInRenderY / (float)_renderDestination.Height * _internalResolution.Y;

                InternalMousePosition = new Vector2(internalX, internalY);
            }
            else
            {
                InternalMousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
            }

            var pressedKeys = _currentKeyState.GetPressedKeys();
            if (pressedKeys.Length > 0)
            {
                Debug.WriteLine($"InputManager.Update - Current Keys: {string.Join(", ", pressedKeys)}");
            }
        }

        public bool IsKeyDown(string action)
        {
            if (!_controls.ContainsKey(action))
            {
                Debug.WriteLine($"Warning: Input action '{action}' not found in controls map.");
                return false;
            }

            foreach (var key in _controls[action])
            {
                if (_currentKeyState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        public bool IsKeyPressed(string action)
        {
            if (!_controls.ContainsKey(action))
            {
                return false;
            }

            foreach (var key in _controls[action])
            {
                if (_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                {
                    Debug.WriteLine($"InputManager.IsKeyPressed TRUE for action: {action}, key: {key}");
                    return true;
                }
            }
            return false;
        }

        public bool IsLeftMouseButtonDown()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool IsLeftMouseButtonPressed()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _previousMouseState.LeftButton == ButtonState.Released;
        }
    }
}