using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;using System.Linq;
namespace IsometricGame
{
    public class InputManager
    {
        private KeyboardState _currentKeyState, _previousKeyState;

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

        public void Update()
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();
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
    }
}