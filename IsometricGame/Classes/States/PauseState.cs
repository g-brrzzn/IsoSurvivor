using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;

namespace IsometricGame.States
{
    public class PauseState : GameStateBase
    {
        private List<string> _options = new List<string> { "CONTINUE", "EXIT" };
        private int _selected = 0;

        public override void Start()
        {
            base.Start();
            _selected = 0;
            if (GameEngine.Player != null && !GameEngine.Player.IsRemoved) // Also check IsRemoved for safety
            {
                // Follow player only if they exist and are active
                Game1.Camera.Follow(GameEngine.Player.ScreenPosition);
            }
            else
            {
                // If player is null or removed, what should the camera do?
                // Option 1: Follow the origin (0,0)
                Game1.Camera.Follow(Vector2.Zero);
                Debug.WriteLine("PauseState.Start: Player is null or removed. Camera following origin.");

                // Option 2: Keep the camera where it was (do nothing here)
                // Debug.WriteLine("PauseState.Start: Player is null or removed. Camera position unchanged.");
            }
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            if (input.IsKeyPressed("DOWN"))
            {
                _selected = (_selected + 1) % _options.Count;
                GameEngine.Assets.Sounds["menu_select"].Play();
            }
            if (input.IsKeyPressed("UP"))
            {
                _selected = (_selected - 1 + _options.Count) % _options.Count;
                GameEngine.Assets.Sounds["menu_select"].Play();
            }

            if (input.IsKeyPressed("ESC"))
            {
                IsDone = true;
                NextState = "Game";
            }

            if (input.IsKeyPressed("START"))
            {
                GameEngine.Assets.Sounds["menu_confirm"].Play();
                IsDone = true;
                if (_selected == 0)
                {
                    NextState = "Game";
                }
                else if (_selected == 1)
                {
                    NextState = "ExitConfirm";
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            DrawUtils.DrawMenu(spriteBatch, _options, "PAUSED", _selected);
        }
    }
}