using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace IsometricGame.States
{
    public class ExitConfirmState : GameStateBase
    {
        private List<string> _options = new List<string> { "EXIT TO MAIN-MENU", "EXIT TO DESKTOP", "BACK" };
        private int _selected = 0;
        private string _previousState = "Menu";
        public override void Start()
        {
            base.Start();
            _selected = 0;
            _previousState = (GameEngine.Player != null) ? "Pause" : "Menu";
            if (_previousState == "Pause")
                Game1.Camera.Follow(GameEngine.Player.ScreenPosition);
            else
                Game1.Camera.Follow(Vector2.Zero);
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
                NextState = _previousState;            }

            if (input.IsKeyPressed("START"))
            {
                GameEngine.Assets.Sounds["menu_confirm"].Play();
                IsDone = true;
                if (_selected == 0)
                {
                    NextState = "Menu";
                }
                else if (_selected == 1)
                {
                    NextState = "Exit";
                }
                else if (_selected == 2)
                {
                    NextState = _previousState;                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            DrawUtils.DrawMenu(spriteBatch, _options, "EXIT", _selected);
        }
    }
}