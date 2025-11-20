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
        private List<Rectangle> _optionRects = new List<Rectangle>();

        public override void Start()
        {
            base.Start();
            _selected = 0;
            Game1.Instance.IsMouseVisible = true;

            if (GameEngine.Player != null && !GameEngine.Player.IsRemoved)
                Game1.Camera.Follow(GameEngine.Player.ScreenPosition);
            else
                Game1.Camera.Follow(Vector2.Zero);
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            if (input.IsKeyPressed("DOWN")) { _selected = (_selected + 1) % _options.Count; GameEngine.Assets.Sounds["menu_select"].Play(); }
            if (input.IsKeyPressed("UP")) { _selected = (_selected - 1 + _options.Count) % _options.Count; GameEngine.Assets.Sounds["menu_select"].Play(); }

            Vector2 mousePos = input.InternalMousePosition;
            Point mousePoint = new Point((int)mousePos.X, (int)mousePos.Y);
            for (int i = 0; i < _optionRects.Count; i++)
            {
                if (_optionRects[i].Contains(mousePoint))
                {
                    if (_selected != i) { _selected = i; GameEngine.Assets.Sounds["menu_select"].Play(); }
                    if (input.IsLeftMouseButtonPressed()) { ConfirmSelection(); return; }
                }
            }

            if (input.IsKeyPressed("ESC")) { IsDone = true; NextState = "Game"; }
            if (input.IsKeyPressed("START")) { ConfirmSelection(); }
        }

        private void ConfirmSelection()
        {
            GameEngine.Assets.Sounds["menu_confirm"].Play();
            IsDone = true;
            if (_selected == 0) NextState = "Game";
            else if (_selected == 1) NextState = "ExitConfirm";
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            _optionRects = DrawUtils.DrawMenu(spriteBatch, _options, "PAUSED", _selected);
        }
    }
}