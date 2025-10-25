// Directory: Classes/States (ou Editor?)
// EditorInputHandler.cs

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace IsometricGame.States.Editor
{
    public class EditorInputHandler
    {
        private EditorState _editorState; // Referência ao estado para chamar ações

        public EditorInputHandler(EditorState editorState)
        {
            _editorState = editorState;
        }

        public void HandleInput(InputManager input, GameTime gameTime)
        {
            // --- Input de Saída ---
            if (input.IsKeyPressed("ESC"))
            {
                _editorState.RequestExit(); // Chama método no EditorState
                return; // Sai cedo se está tentando sair
            }

            // --- Input de Câmera ---
            HandleCameraInput(input, gameTime);

            // --- Trocar Modo ---
            if (input.IsKeyPressed("SWITCH_MODE"))
            {
                _editorState.SwitchEditorMode(); // Chama método no EditorState
            }

            // --- Input Específico do Modo ---
            if (_editorState.GetCurrentMode() == EditorMode.Tiles)
            {
                HandleTileInput(input);
            }
            else // EditorMode.Triggers
            {
                HandleTriggerInput(input);
            }

            // --- Controles Comuns (Salvar, Carregar, Novo) ---
            if (input.IsKeyDown("SAVE_MODIFIER") && input.IsKeyPressed("SAVE_ACTION")) { _editorState.RequestSaveMap(); }
            if (input.IsKeyDown("LOAD_MODIFIER") && input.IsKeyPressed("LOAD_ACTION")) { _editorState.RequestLoadMap(); }
            if (input.IsKeyDown("NEW_MODIFIER") && input.IsKeyPressed("NEW_ACTION")) { _editorState.RequestNewMap(); }
        }


        private void HandleCameraInput(InputManager input, GameTime gameTime)
        {
            // Panning
            // --- Acessa Zoom pela instância da câmera ---
            float camSpeed = 250f * (float)gameTime.ElapsedGameTime.TotalSeconds / Game1.Camera.Zoom;
            Vector2 camMove = Vector2.Zero;
            if (input.IsKeyDown("LEFT")) camMove.X -= camSpeed;
            if (input.IsKeyDown("RIGHT")) camMove.X += camSpeed;
            if (input.IsKeyDown("UP")) camMove.Y -= camSpeed;
            if (input.IsKeyDown("DOWN")) camMove.Y += camSpeed;
            if (camMove != Vector2.Zero)
            {
                // Chama método público no EditorState para mover a câmera
                _editorState.MoveCamera(camMove);
            }

            // Zoom
            if (input.IsKeyPressed("ZOOM_IN") || input.ScrollWheelDelta > 0)
                _editorState.ZoomCamera(1.15f); // Chama método público
            if (input.IsKeyPressed("ZOOM_OUT") || input.ScrollWheelDelta < 0)
                _editorState.ZoomCamera(1 / 1.15f); // Chama método público
        }

        private void HandleTileInput(InputManager input)
        {
            // Seleção de Tile na Paleta
            if (input.IsKeyPressed("NEXT_TILE")) { _editorState.SelectNextTileInPalette(); }
            if (input.IsKeyPressed("PREV_TILE")) { _editorState.SelectPreviousTileInPalette(); }

            // Seleção de Camada Z
            Keys[] numberKeys = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            for (int z = 0; z < numberKeys.Length; z++) { if (input.IsKeyPressed(numberKeys[z].ToString()) && z <= Constants.MaxZLevel) { _editorState.SetCurrentZLevel(z); break; } }

            // Colocar Tile
            if (input.IsLeftMouseButtonDown()) { _editorState.PlaceSelectedTileAtCursor(); }

            // Remover Tile
            if (input.IsRightMouseButtonDown()) { _editorState.EraseTileAtCursor(); }
        }

        private void HandleTriggerInput(InputManager input)
        {
            // Selecionar Trigger
            if (input.IsLeftMouseButtonPressed()) { _editorState.SelectTriggerAtCursor(); }

            // Adicionar Novo Trigger
            if (input.IsRightMouseButtonPressed()) { _editorState.AddTriggerAtCursor(); }

            // Remover Trigger Selecionado
            if (input.IsKeyPressed("DELETE_TRIGGER")) { _editorState.RemoveSelectedTrigger(); } // Só chama se houver um selecionado (EditorState verifica)

            // Seleção de Camada Z (mesma lógica do modo tile)
            Keys[] numberKeys = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            for (int z = 0; z < numberKeys.Length; z++) { if (input.IsKeyPressed(numberKeys[z].ToString()) && z <= Constants.MaxZLevel) { _editorState.SetCurrentZLevel(z); break; } }
        }
    }
}