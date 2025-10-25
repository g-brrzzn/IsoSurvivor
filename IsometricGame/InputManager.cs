// Directory: .
// InputManager.cs

using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq; // Usado para SequenceEqual no debug comentado
using Microsoft.Xna.Framework;
using System; // Usado para Math

namespace IsometricGame
{
    public class InputManager
    {
        // Estados dos dispositivos de entrada
        private KeyboardState _currentKeyState, _previousKeyState;
        private MouseState _currentMouseState, _previousMouseState;

        // Variáveis para conversão de coordenadas de tela e roda do mouse
        private Rectangle _renderDestination;
        private Point _internalResolution;
        private int _previousScrollWheelValue = 0;

        // Propriedades públicas para acesso externo
        public Vector2 InternalMousePosition { get; private set; } // Posição do mouse dentro da resolução interna
        public int ScrollWheelDelta { get; private set; } // Mudança no valor da roda do mouse desde o último frame

        // Mapeamento de ações (strings) para teclas físicas
        // Facilita a reconfiguração de controles e torna o código mais legível
        private static readonly Dictionary<string, Keys[]> _controls = new Dictionary<string, Keys[]>
        {
            // Movimento
            { "UP", new[] { Keys.W, Keys.Up } },
            { "DOWN", new[] { Keys.S, Keys.Down } },
            { "LEFT", new[] { Keys.A, Keys.Left } },
            { "RIGHT", new[] { Keys.D, Keys.Right } },

            // Ações de Jogo
            { "FIRE", new[] { Keys.Space } }, // Disparar no jogo
            { "START", new[] { Keys.Enter, Keys.Space } }, // Confirmar em menus, às vezes usado como ação principal

            // Ações Gerais
            { "ESC", new[] { Keys.Escape } }, // Pausar, Voltar, Sair

            // Controles do Editor
            { "ZOOM_IN", new[] { Keys.OemPlus, Keys.Add } },
            { "ZOOM_OUT", new[] { Keys.OemMinus, Keys.Subtract } },
            { "NEXT_TILE", new[] { Keys.PageDown, Keys.E } },
            { "PREV_TILE", new[] { Keys.PageUp, Keys.Q } },
            { "SAVE_MODIFIER", new[] { Keys.LeftControl, Keys.RightControl } },
            { "SAVE_ACTION", new[] { Keys.S } },
            { "LOAD_MODIFIER", new[] { Keys.LeftControl, Keys.RightControl } },
            { "LOAD_ACTION", new[] { Keys.L } },
            { "NEW_MODIFIER", new[] { Keys.LeftControl, Keys.RightControl } },
            { "NEW_ACTION", new[] { Keys.N } },
            { "SWITCH_MODE", new[] { Keys.Tab } },        // <-- ADICIONADO
            { "DELETE_TRIGGER", new[] { Keys.Delete } }, // <-- ADICIONADO

            // Seleção de Camada Z (Teclas numéricas 0-9)
            { "D0", new[] { Keys.D0, Keys.NumPad0 } },
            { "D1", new[] { Keys.D1, Keys.NumPad1 } },
            { "D2", new[] { Keys.D2, Keys.NumPad2 } },
            { "D3", new[] { Keys.D3, Keys.NumPad3 } },
            { "D4", new[] { Keys.D4, Keys.NumPad4 } },
            { "D5", new[] { Keys.D5, Keys.NumPad5 } },
            { "D6", new[] { Keys.D6, Keys.NumPad6 } },
            { "D7", new[] { Keys.D7, Keys.NumPad7 } },
            { "D8", new[] { Keys.D8, Keys.NumPad8 } },
            { "D9", new[] { Keys.D9, Keys.NumPad9 } },
        };

        /// <summary>
        /// Define os parâmetros necessários para converter a posição do mouse na tela
        /// para a posição dentro da resolução interna do jogo.
        /// </summary>
        /// <param name="renderDestination">O retângulo onde o RenderTarget é desenhado na tela.</param>
        /// <param name="internalResolution">A resolução interna do jogo (tamanho do RenderTarget).</param>
        public void SetScreenConversion(Rectangle renderDestination, Point internalResolution)
        {
            _renderDestination = renderDestination;
            _internalResolution = internalResolution;
        }

        /// <summary>
        /// Atualiza os estados dos dispositivos de entrada (teclado e mouse).
        /// Deve ser chamado uma vez por frame, no início do método Update do jogo principal.
        /// </summary>
        public void Update()
        {
            // Guarda os estados anteriores
            _previousKeyState = _currentKeyState;
            _previousMouseState = _currentMouseState;

            // Obtém os estados atuais
            _currentKeyState = Keyboard.GetState();
            _currentMouseState = Mouse.GetState();

            // Calcula a mudança na roda do mouse
            ScrollWheelDelta = _currentMouseState.ScrollWheelValue - _previousScrollWheelValue;
            _previousScrollWheelValue = _currentMouseState.ScrollWheelValue; // Atualiza para o próximo frame

            // Calcula a posição do mouse dentro da resolução interna do jogo
            if (_renderDestination.Width > 0 && _renderDestination.Height > 0 && _internalResolution.X > 0 && _internalResolution.Y > 0)
            {
                // Posição do mouse relativa ao canto superior esquerdo do retângulo de renderização
                float mouseInRenderX = _currentMouseState.X - _renderDestination.X;
                float mouseInRenderY = _currentMouseState.Y - _renderDestination.Y;

                // Garante que a posição relativa esteja dentro dos limites 0 a Width/Height do retângulo
                mouseInRenderX = MathHelper.Clamp(mouseInRenderX, 0, _renderDestination.Width);
                mouseInRenderY = MathHelper.Clamp(mouseInRenderY, 0, _renderDestination.Height);


                // Converte a posição relativa para a escala da resolução interna
                float internalX = (mouseInRenderX / _renderDestination.Width) * _internalResolution.X;
                float internalY = (mouseInRenderY / _renderDestination.Height) * _internalResolution.Y;


                InternalMousePosition = new Vector2(internalX, internalY);
            }
            else
            {
                // Fallback: Usa as coordenadas brutas do mouse se a conversão não for possível
                InternalMousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
            }

            // --- Bloco de Debug (Opcional - pode ser comentado) ---
            /*
            var pressedKeys = _currentKeyState.GetPressedKeys();
            var prevPressedKeys = _previousKeyState.GetPressedKeys();
            // Só imprime se as teclas pressionadas mudaram para evitar spam no log
            if (!pressedKeys.SequenceEqual(prevPressedKeys))
            {
                 if (pressedKeys.Length > 0)
                      Debug.WriteLine($"InputManager.Update - Current Keys: {string.Join(", ", pressedKeys)}");
                 else
                      Debug.WriteLine("InputManager.Update - No keys pressed.");
            }
            // Debug do mouse
            if(_currentMouseState != _previousMouseState) {
                 Debug.WriteLine($"Mouse State: Pos({_currentMouseState.X},{_currentMouseState.Y}) Internal({InternalMousePosition.X:F0},{InternalMousePosition.Y:F0}) L:{_currentMouseState.LeftButton} R:{_currentMouseState.RightButton} Scroll:{_currentMouseState.ScrollWheelValue} Delta:{ScrollWheelDelta}");
            }
            */
            // --- Fim do Bloco de Debug ---
        }

        /// <summary>
        /// Verifica se alguma das teclas mapeadas para a ação especificada está atualmente pressionada.
        /// </summary>
        /// <param name="action">O nome da ação (ex: "UP", "FIRE").</param>
        /// <returns>True se alguma tecla da ação estiver pressionada, False caso contrário.</returns>
        public bool IsKeyDown(string action)
        {
            if (!_controls.ContainsKey(action))
            {
                Debug.WriteLine($"Warning: Input action '{action}' not found in controls map.");
                return false;
            }

            // Verifica cada tecla mapeada para a ação
            foreach (var key in _controls[action])
            {
                if (_currentKeyState.IsKeyDown(key))
                    return true; // Retorna true na primeira tecla encontrada
            }
            return false; // Nenhuma tecla da ação está pressionada
        }

        /// <summary>
        /// Verifica se alguma das teclas mapeadas para a ação especificada foi pressionada
        /// *neste exato frame* (estava solta no frame anterior e está pressionada agora).
        /// </summary>
        /// <param name="action">O nome da ação (ex: "START", "ESC").</param>
        /// <returns>True se a ação foi iniciada neste frame, False caso contrário.</returns>
        public bool IsKeyPressed(string action)
        {
            if (!_controls.ContainsKey(action))
            {
                // Não loga warning aqui para não poluir se checar ações opcionais
                return false;
            }

            // Verifica cada tecla mapeada para a ação
            foreach (var key in _controls[action])
            {
                // Verifica se a tecla está pressionada AGORA e estava solta ANTES
                if (_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyUp(key))
                {
                    // Debug.WriteLine($"InputManager.IsKeyPressed TRUE for action: {action}, key: {key}"); // Opcional
                    return true; // Retorna true na primeira tecla encontrada que satisfaz a condição
                }
            }
            return false; // Nenhuma tecla da ação foi pressionada neste frame
        }

        /// <summary>
        /// Verifica se o botão esquerdo do mouse está atualmente pressionado.
        /// </summary>
        public bool IsLeftMouseButtonDown()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Verifica se o botão direito do mouse está atualmente pressionado.
        /// </summary>
        public bool IsRightMouseButtonDown()
        {
            return _currentMouseState.RightButton == ButtonState.Pressed;
        }

        /// <summary>
        /// Verifica se o botão esquerdo do mouse foi pressionado *neste exato frame*.
        /// </summary>
        public bool IsLeftMouseButtonPressed()
        {
            return _currentMouseState.LeftButton == ButtonState.Pressed &&
                   _previousMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Verifica se o botão direito do mouse foi pressionado *neste exato frame*.
        /// </summary>
        public bool IsRightMouseButtonPressed()
        {
            return _currentMouseState.RightButton == ButtonState.Pressed &&
                   _previousMouseState.RightButton == ButtonState.Released;
        }
    }
}