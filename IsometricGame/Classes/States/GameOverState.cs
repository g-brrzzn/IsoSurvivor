using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace IsometricGame.States
{
    public class GameOverState : GameStateBase
    {
        private List<string> _options = new List<string> { "RESTART", "EXIT" };
        private int _selected = 0;
        private List<Rectangle> _optionRects = new List<Rectangle>();

        public override void Start()
        {
            base.Start();
            _selected = 0;
            Game1.Instance.IsMouseVisible = true;
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            if (input.IsKeyPressed("DOWN")) { _selected = (_selected + 1) % _options.Count; GameEngine.Assets.Sounds["menu_select"]?.Play(); }
            if (input.IsKeyPressed("UP")) { _selected = (_selected - 1 + _options.Count) % _options.Count; GameEngine.Assets.Sounds["menu_select"]?.Play(); }

            Vector2 mousePos = input.InternalMousePosition;
            Point mousePoint = new Point((int)mousePos.X, (int)mousePos.Y);
            for (int i = 0; i < _optionRects.Count; i++)
            {
                if (_optionRects[i].Contains(mousePoint))
                {
                    if (_selected != i) { _selected = i; GameEngine.Assets.Sounds["menu_select"]?.Play(); }
                    if (input.IsLeftMouseButtonPressed()) { ConfirmSelection(); return; }
                }
            }

            if (input.IsKeyPressed("START")) { ConfirmSelection(); }
        }

        private void ConfirmSelection()
        {
            GameEngine.Assets.Sounds["menu_confirm"]?.Play();
            IsDone = true;
            if (_selected == 0) NextState = "Menu";
            else if (_selected == 1) NextState = "ExitConfirm";
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            _optionRects = DrawUtils.DrawMenu(spriteBatch, _options, "GAME OVER", _selected);
        }
    }
}