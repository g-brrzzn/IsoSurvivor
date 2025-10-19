using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System;using System.Diagnostics;
namespace IsometricGame.States
{
    public class OptionsState : GameStateBase
    {
        private List<string> _options = new List<string>();
        private int _selected = 0;

        private int _currentResIndex;
        private bool _currentShowFps;
        private bool _currentFullscreen;

        private const string OptRes = "RESOLUTION";
        private const string OptFps = "SHOW FPS";
        private const string OptFs = "FULLSCREEN";
        private const string OptApply = "APPLY";
        private const string OptBack = "BACK";
        public OptionsState() { }

        public override void Start()
        {
            base.Start();
            _selected = 0;

            _currentResIndex = Constants.Resolutions.ToList().IndexOf(Constants.WindowSize);
            if (_currentResIndex < 0) _currentResIndex = 0;

            _currentShowFps = Constants.ShowFPS;
            _currentFullscreen = Constants.SetFullscreen;
            UpdateOptionsText();        }

        private void UpdateOptionsText()
        {
            _options.Clear();
            _options.Add($"{OptRes} - {Constants.Resolutions[_currentResIndex].X}x{Constants.Resolutions[_currentResIndex].Y}");
            _options.Add($"{OptFps}: {(_currentShowFps ? "ON" : "OFF")}");
            _options.Add($"{OptFs}: {(_currentFullscreen ? "ON" : "OFF")}");
            _options.Add(OptApply);
            _options.Add(OptBack);
        }


        public override void Update(GameTime gameTime, InputManager input)
        {
            bool selectionChanged = false;
            if (input.IsKeyPressed("DOWN"))
            {
                _selected = (_selected + 1) % _options.Count;
                selectionChanged = true;
            }
            if (input.IsKeyPressed("UP"))
            {
                _selected = (_selected - 1 + _options.Count) % _options.Count;
                selectionChanged = true;
            }

            if (selectionChanged)
            {
                GameEngine.Assets.Sounds["menu_select"]?.Play();
            }


            if (input.IsKeyPressed("ESC"))
            {
                IsDone = true;
                NextState = "Menu";
                return;            }
            if (_options == null || _options.Count == 0)
            {
                Debug.WriteLine("Error: Options list is empty in OptionsState.Update!");
                UpdateOptionsText();
                if (_options.Count == 0) return;            }
            _selected = Math.Clamp(_selected, 0, _options.Count - 1);
            string currentOptionText = _options[_selected];
            string currentOptionAction = currentOptionText.Split(' ')[0];
            if (currentOptionAction == OptRes)
            {
                bool optionTextChanged = false;
                if (input.IsKeyPressed("RIGHT"))
                {
                    _currentResIndex = (_currentResIndex + 1) % Constants.Resolutions.Length;
                    optionTextChanged = true;
                    selectionChanged = true;                }
                if (input.IsKeyPressed("LEFT"))
                {
                    _currentResIndex = (_currentResIndex - 1 + Constants.Resolutions.Length) % Constants.Resolutions.Length;
                    optionTextChanged = true;
                    selectionChanged = true;                }
                if (optionTextChanged) UpdateOptionsText();                if (selectionChanged && !input.IsKeyPressed("UP") && !input.IsKeyPressed("DOWN"))                    GameEngine.Assets.Sounds["menu_select"]?.Play();
            }

            if (input.IsKeyPressed("START"))
            {
                GameEngine.Assets.Sounds["menu_confirm"]?.Play();

                switch (currentOptionAction)                {
                    case OptFps:
                        _currentShowFps = !_currentShowFps;
                        UpdateOptionsText();                        break;

                    case OptFs:
                        _currentFullscreen = !_currentFullscreen;
                        UpdateOptionsText();                        break;

                    case OptApply:
                        Game1.ApplySettings(Constants.Resolutions[_currentResIndex], _currentFullscreen);
                        Constants.ShowFPS = _currentShowFps;
                        break;

                    case OptBack:
                        IsDone = true;
                        NextState = "Menu";
                        break;
                    case OptRes:
                        break;
                }
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            DrawUtils.DrawMenu(spriteBatch, _options, "OPTIONS", _selected);
        }
    }
}