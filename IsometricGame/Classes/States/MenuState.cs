using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Diagnostics;
namespace IsometricGame.States
{
    public class MenuState : GameStateBase
    {
        private List<string> _options = new List<string> { "START", "OPTIONS", "EXIT" };
        private int _selected = 0;
        private float _titleOffsetY;

        public override void Start()
        {
            base.Start();
            _selected = 0;
            Debug.WriteLine("MenuState Started.");        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            _titleOffsetY = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2 * Math.PI) * (Constants.InternalResolution.Y * 0.04));

            if (input.IsKeyPressed("DOWN"))
            {
                Debug.WriteLine($"MenuState detected: DOWN");
                _selected = (_selected + 1) % _options.Count;
                GameEngine.Assets.Sounds["menu_select"]?.Play();            }
            if (input.IsKeyPressed("UP"))
            {
                Debug.WriteLine($"MenuState detected: UP");
                _selected = (_selected - 1 + _options.Count) % _options.Count;
                GameEngine.Assets.Sounds["menu_select"]?.Play();
            }

            if (input.IsKeyPressed("START"))
            {
                Debug.WriteLine($"MenuState detected: START");
                GameEngine.Assets.Sounds["menu_confirm"]?.Play();
                IsDone = true;
                if (_selected == 0) NextState = "Game";
                else if (_selected == 1) NextState = "Options";
                else if (_selected == 2) NextState = "Exit";
            }
            if (input.IsKeyPressed("ESC"))
            {
                Debug.WriteLine($"MenuState detected: ESC (mapped to Exit)");
                IsDone = true;
                NextState = "Exit";            }
        }
        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            Vector2 titlePosScreen = new Vector2(Constants.InternalResolution.X / 2f, Constants.InternalResolution.Y * 0.33f + _titleOffsetY);
            Vector2 titlePosWorld = Game1.Camera.ScreenToWorld(titlePosScreen);
            DrawUtils.DrawText(spriteBatch, "Isometric Game Base", GameEngine.Assets.Fonts["captain_80"], titlePosWorld, Constants.TitleYellow1, 1.0f);
            DrawUtils.DrawMenu(spriteBatch, _options, "", _selected);
        }
    }
}