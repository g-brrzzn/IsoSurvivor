// Directory: Classes/States/Editor
// EditorState.cs

using IsometricGame.Classes;     // For Sprite
using IsometricGame.Map;         // For MapData, MapTrigger, TileMappingEntry, MapLayer
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input; // For Keys
using Newtonsoft.Json;             // For saving/loading JSON
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;                   // For saving/loading files
using System.Linq;                 // For FirstOrDefault, OrderBy, Enumerable.Repeat etc.

namespace IsometricGame.States.Editor // Namespace for editor-related state files
{
    /// <summary>
    /// Defines the current editing mode (either placing tiles or managing triggers).
    /// Moved outside the EditorState class for better accessibility.
    /// </summary>
    public enum EditorMode
    {
        Tiles,
        Triggers
    }

    /// <summary>
    /// Manages the state and core logic for the map editor.
    /// Delegates input handling to EditorInputHandler and rendering to EditorRenderer.
    /// </summary>
    public class EditorState : GameStateBase
    {
        // --- Core State ---
        private MapData _currentMapData; // Holds the actual map data being edited
        private string _currentMapFileName = "new_map.json"; // Name of the file being edited
        private EditorMode _currentMode = EditorMode.Tiles; // Current editing mode
        private Vector3 _cursorWorldPos = Vector3.Zero; // Calculated 3D position of the cursor grid cell
        private int _currentZLevel = 0; // The Z-layer currently being viewed/edited

        // --- Visual Representation Cache ---
        // Stores the Sprite objects for visible tiles, keyed by their WorldPosition. Regenerated on map load/change.
        private Dictionary<Vector3, Sprite> _mapSprites = new Dictionary<Vector3, Sprite>();

        // --- Tile Palette State ---
        private List<TileMappingEntry> _tilePalette; // List of available tiles to place
        private int _paletteIndex = 0; // Current index in the _tilePalette
        private int _selectedTileId = 0; // ID of the tile currently selected for placement (0 means erase)
        private TileMappingEntry _selectedTileInfo = null; // Cached info of the selected tile

        // --- Trigger State ---
        private MapTrigger _selectedTrigger = null; // The trigger currently selected for viewing/deletion

        // --- Helper Components ---
        private EditorInputHandler _inputHandler; // Handles all keyboard/mouse input logic
        private EditorRenderer _renderer; // Handles all drawing logic
        private SpriteFont _font; // Font used by the renderer for UI text
        private Texture2D _pixelTexture; // Basic 1x1 texture for drawing shapes/overlays
        private Texture2D _triggerIconTexture; // Optional texture for representing triggers

        /// <summary>
        /// Constructor: Initializes the input handler.
        /// </summary>
        public EditorState()
        {
            _inputHandler = new EditorInputHandler(this); // Pass reference to self
        }

        /// <summary>
        /// Initializes the editor state when activated. Loads assets, sets up the camera,
        /// initializes the tile palette, and loads the initial map.
        /// </summary>
        public override void Start()
        {
            base.Start(); // Resets IsDone and NextState
            _font = GameEngine.Assets.Fonts["captain_32"];
            Game1.Instance.IsMouseVisible = false; // Use custom cursor indicator
            Game1.Camera.SetZoom(1.5f); // Editor-specific zoom

            // Ensure essential textures are available
            if (!GameEngine.Assets.Images.TryGetValue("pixel", out _pixelTexture))
            {
                _pixelTexture = new Texture2D(Game1._graphicsManagerInstance.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
                GameEngine.Assets.Images["pixel"] = _pixelTexture;
                Debug.WriteLine("Warning (Editor): Texture 'pixel' not found. Created fallback.");
            }
            // Optional: Load trigger icon texture here if you have one
            // GameEngine.Assets.Images.TryGetValue("trigger_icon", out _triggerIconTexture);
            if (_triggerIconTexture == null) { _triggerIconTexture = _pixelTexture; }

            // Initialize the renderer now that assets are loaded
            _renderer = new EditorRenderer(_font, _pixelTexture, _triggerIconTexture);

            // Reset editor state variables
            _currentMode = EditorMode.Tiles;
            _selectedTrigger = null;
            _currentZLevel = 0; // Start at ground level

            InitializePalette(); // Load available tiles
            UpdateSelectedTileInfo(); // Set initial selected tile info

            LoadMap(_currentMapFileName); // Load default map or create new
        }

        /// <summary>
        /// Cleans up when the editor state is deactivated.
        /// </summary>
        public override void End()
        {
            Game1.Instance.IsMouseVisible = true; // Restore system cursor
            // Optional: Prompt to save changes
            // Optional: Restore game camera zoom
            // Game1.Camera.SetZoom(2.0f);
        }

        // --- GETTERS (Provide read-only access to state for helper classes) ---
        public MapData GetCurrentMapData() => _currentMapData;
        public string GetCurrentMapFileName() => _currentMapFileName;
        public EditorMode GetCurrentMode() => _currentMode;
        public Vector3 GetCursorWorldPos() => _cursorWorldPos;
        public int GetCurrentZLevel() => _currentZLevel;
        public Dictionary<Vector3, Sprite> GetMapSprites() => _mapSprites;
        public TileMappingEntry GetSelectedTileInfo() => _selectedTileInfo;
        public int GetSelectedTileId() => _selectedTileId;
        public MapTrigger GetSelectedTrigger() => _selectedTrigger;
        public List<MapTrigger> GetCurrentMapTriggers() => _currentMapData?.Triggers;
        // ----------------------------------------------------------------------


        /// <summary>
        /// Main update loop for the editor. Delegates input handling and updates cursor position.
        /// </summary>
        public override void Update(GameTime gameTime, InputManager input)
        {
            // Input handler processes all inputs and calls action methods below
            _inputHandler.HandleInput(input, gameTime);

            // If input handler requested exit, stop processing
            if (IsDone) return;

            // Update the calculated world position of the cursor based on mouse input
            UpdateCursorPosition(input);
        }

        /// <summary>
        /// Calculates the 3D grid position (_cursorWorldPos) corresponding to the mouse's screen position.
        /// </summary>
        private void UpdateCursorPosition(InputManager input)
        {
            Vector2 mouseScreenPos = input.InternalMousePosition;
            Vector2 mouseCameraWorld = Game1.Camera.ScreenToWorld(mouseScreenPos);
            Vector2 mouseIsoWorld = IsoMath.ScreenToWorld(mouseCameraWorld);

            // Cursor's final position snaps to the grid and uses the currently selected Z-level
            _cursorWorldPos = new Vector3(
                MathF.Round(mouseIsoWorld.X),
                MathF.Round(mouseIsoWorld.Y),
                _currentZLevel
            );
        }


        // --- DRAW METHODS (Delegate rendering to the renderer) ---

        /// <summary>
        /// Draws the world elements of the editor (map, triggers, cursor).
        /// Called by Game1.Draw() within a SpriteBatch Begin/End block using the camera matrix.
        /// </summary>
        public void DrawWorld(SpriteBatch spriteBatch)
        {
            _renderer?.DrawEditorWorld(spriteBatch, this); // Pass self to renderer
        }

        /// <summary>
        /// Draws the UI elements of the editor (text info, palette preview).
        /// Called by Game1.Draw() within a SpriteBatch Begin/End block *without* the camera matrix.
        /// </summary>
        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            _renderer?.DrawEditorUI(spriteBatch, this); // Pass self to renderer
        }
        // ----------------------------------------------------------


        // --- ACTION METHODS (Called by EditorInputHandler) ---

        public void RequestExit() { /* TODO: Add confirmation? */ IsDone = true; NextState = "Menu"; }
        public void RequestSaveMap() { SaveMap(_currentMapFileName); }
        public void RequestLoadMap() { LoadMap(_currentMapFileName); }
        public void RequestNewMap() { /* TODO: Add confirmation? */ _currentMapFileName = $"new_map_{DateTime.Now:yyyyMMddHHmmss}.json"; CreateNewMap(); }
        public void SwitchEditorMode() { _currentMode = (_currentMode == EditorMode.Tiles) ? EditorMode.Triggers : EditorMode.Tiles; _selectedTrigger = null; Debug.WriteLine($"Mode switched to: {_currentMode}"); }
        public void SetCurrentZLevel(int z) { if (z >= 0 && z <= Constants.MaxZLevel) { _currentZLevel = z; _selectedTrigger = null; /* Deselect trigger on Z change */ Debug.WriteLine($"Current Z-Level: {_currentZLevel}"); } }
        public void MoveCamera(Vector2 moveAmount) { Game1.Camera.Follow(Game1.Camera.Position + moveAmount); } // Direct camera move
        public void ZoomCamera(float zoomFactor) { Game1.Camera.SetZoom(Game1.Camera.Zoom * zoomFactor); }

        public void SelectNextTileInPalette()
        {
            if (_tilePalette == null || _tilePalette.Count == 0) return;
            _paletteIndex = (_paletteIndex + 1) % _tilePalette.Count;
            _selectedTileId = _tilePalette[_paletteIndex].Id;
            UpdateSelectedTileInfo();
        }
        public void SelectPreviousTileInPalette()
        {
            if (_tilePalette == null || _tilePalette.Count == 0) return;
            _paletteIndex = (_paletteIndex - 1 + _tilePalette.Count) % _tilePalette.Count;
            _selectedTileId = _tilePalette[_paletteIndex].Id;
            UpdateSelectedTileInfo();
        }

        public void PlaceSelectedTileAtCursor() { PlaceTile(_cursorWorldPos, _selectedTileId); }
        public void EraseTileAtCursor() { PlaceTile(_cursorWorldPos, 0); } // Place tile ID 0 to erase

        public void SelectTriggerAtCursor() { SelectTriggerNear(_cursorWorldPos); }
        public void AddTriggerAtCursor() { AddTriggerAt(_cursorWorldPos); }
        public void RemoveSelectedTrigger() { if (_selectedTrigger != null) RemoveTrigger(_selectedTrigger); else Debug.WriteLine("No trigger selected to remove."); }

        // --- END ACTION METHODS ---


        // --- INTERNAL LOGIC METHODS ---

        /// <summary> Loads tile definitions into _tilePalette from a default map file. </summary>
        private void InitializePalette()
        { /* ... (Code from previous answer) ... */
            string sampleMapPath = Path.Combine("Content", "maps", "map1.json");
            _tilePalette = new List<TileMappingEntry>();
            if (File.Exists(sampleMapPath)) { try { string jsonContent = File.ReadAllText(sampleMapPath); var sampleMapData = JsonConvert.DeserializeObject<MapData>(jsonContent); if (sampleMapData?.TileMapping != null) { _tilePalette.AddRange(sampleMapData.TileMapping); } } catch (Exception ex) { Debug.WriteLine($"Erro ao carregar paleta de {sampleMapPath}: {ex.Message}"); } } else { Debug.WriteLine($"Arquivo de mapa de exemplo para paleta não encontrado: {sampleMapPath}"); }
            if (_tilePalette.Count == 0) { _tilePalette.Add(new TileMappingEntry { Id = 1, AssetName = "tile_grass1", Solid = false }); Debug.WriteLine("Paleta vazia. Adicionado tile padrão."); }
            _paletteIndex = 0;
            _selectedTileId = _tilePalette.Count > 0 ? _tilePalette[0].Id : 0;
        }

        /// <summary> Updates _selectedTileInfo based on _selectedTileId, ensuring consistency. </summary>
        private void UpdateSelectedTileInfo()
        { /* ... (Code from previous answer) ... */
            if (_tilePalette == null || _tilePalette.Count == 0) { _selectedTileInfo = null; _selectedTileId = 0; return; }
            _selectedTileInfo = _tilePalette.FirstOrDefault(t => t.Id == _selectedTileId);
            if (_selectedTileInfo == null) { _paletteIndex = 0; _selectedTileId = _tilePalette[0].Id; _selectedTileInfo = _tilePalette[0]; }
            _paletteIndex = _tilePalette.FindIndex(t => t.Id == _selectedTileId); if (_paletteIndex < 0) _paletteIndex = 0;
        }

        /// <summary> Creates a new, empty MapData object in memory. </summary>
        private void CreateNewMap(int width = 30, int height = 30)
        { /* ... (Code from previous answer) ... */
            _currentMapData = new MapData { Width = width, Height = height, TileMapping = new List<TileMappingEntry>(_tilePalette ?? new List<TileMappingEntry>()), Layers = new List<MapLayer>(), Triggers = new List<MapTrigger>() };
            _currentMapData.Layers.Add(new MapLayer { Name = $"Ground (Z=0)", ZLevel = 0, Data = Enumerable.Repeat(0, width * height).ToList() });
            RebuildMapSprites(); _selectedTrigger = null; Debug.WriteLine($"Novo mapa criado ({width}x{height}).");
        }

        /// <summary> Updates the _mapSprites dictionary based on _currentMapData. </summary>
        private void RebuildMapSprites()
        { /* ... (Code from previous answer) ... */
            _mapSprites.Clear(); if (_currentMapData?.Layers == null || _currentMapData.Width <= 0) return;
            Dictionary<int, TileMappingEntry> tileLookup = new Dictionary<int, TileMappingEntry>(); if (_currentMapData.TileMapping != null) { try { tileLookup = _currentMapData.TileMapping.ToDictionary(entry => entry.Id, entry => entry); } catch (ArgumentException ex) { Debug.WriteLine($"IDs duplicados no tileMapping: {ex.Message}"); } }
            foreach (var layer in _currentMapData.Layers.OrderBy(l => l.ZLevel)) { if (layer.Data == null) continue; for (int i = 0; i < layer.Data.Count; i++) { int tileId = layer.Data[i]; if (tileId == 0) continue; int x = i % _currentMapData.Width; int y = i / _currentMapData.Width; Vector3 worldPos = new Vector3(x, y, layer.ZLevel); if (tileLookup.TryGetValue(tileId, out var tileInfo) && GameEngine.Assets.Images.TryGetValue(tileInfo.AssetName, out var texture)) { _mapSprites[worldPos] = new Sprite(texture, worldPos); } } }
            Debug.WriteLine($"Sprites reconstruídos: {_mapSprites.Count} tiles.");
        }

        /// <summary> Modifies the _currentMapData and _mapSprites based on tile placement/removal. </summary>
        private void PlaceTile(Vector3 worldPos, int tileId)
        { /* ... (Code from previous answer) ... */
            if (_currentMapData == null || _currentMapData.Width <= 0) return;
            if (worldPos.X < 0 || worldPos.X >= _currentMapData.Width || worldPos.Y < 0 || worldPos.Y >= _currentMapData.Height || worldPos.Z < 0 || worldPos.Z > Constants.MaxZLevel) return;
            int x = (int)worldPos.X; int y = (int)worldPos.Y; int z = (int)worldPos.Z;
            MapLayer targetLayer = _currentMapData.Layers?.FirstOrDefault(l => l.ZLevel == z);
            if (targetLayer == null) { if (tileId == 0) return; if (_currentMapData.Layers == null) _currentMapData.Layers = new List<MapLayer>(); targetLayer = new MapLayer { Name = $"Layer (Z={z})", ZLevel = z, Data = Enumerable.Repeat(0, _currentMapData.Width * _currentMapData.Height).ToList() }; _currentMapData.Layers.Add(targetLayer); _currentMapData.Layers.Sort((a, b) => a.ZLevel.CompareTo(b.ZLevel)); Debug.WriteLine($"Criada camada Z={z}"); }
            int expectedSize = _currentMapData.Width * _currentMapData.Height; if (targetLayer.Data == null || targetLayer.Data.Count != expectedSize) { Debug.WriteLine($"Corrigindo array 'Data' para camada Z={z}."); targetLayer.Data = Enumerable.Repeat(0, expectedSize).ToList(); }
            int index = y * _currentMapData.Width + x; if (index < 0 || index >= targetLayer.Data.Count) { Debug.WriteLine($"Índice inválido ({index}) ao colocar tile."); return; }
            if (targetLayer.Data[index] != tileId) { targetLayer.Data[index] = tileId; if (tileId == 0) { _mapSprites.Remove(worldPos); } else { TileMappingEntry newTileInfo = _tilePalette?.FirstOrDefault(t => t.Id == tileId); if (newTileInfo != null && GameEngine.Assets.Images.TryGetValue(newTileInfo.AssetName, out var texture)) { _mapSprites[worldPos] = new Sprite(texture, worldPos); } else { _mapSprites.Remove(worldPos); Debug.WriteLine($"Tile ID {tileId} inválido ou textura '{newTileInfo?.AssetName ?? "N/A"}' não encontrada."); } } }
        }

        /// <summary> Finds and selects the trigger closest to the click position on the current Z-level. </summary>
        private void SelectTriggerNear(Vector3 clickPos)
        { /* ... (Code from previous answer) ... */
            if (_currentMapData?.Triggers == null) return; MapTrigger foundTrigger = null; float closestDistSq = 0.3f * 0.3f;
            foreach (var trigger in _currentMapData.Triggers) { if (Math.Abs(trigger.Position.Z - clickPos.Z) < 0.1f) { float distSq = Vector2.DistanceSquared(new Vector2(trigger.Position.X, trigger.Position.Y), new Vector2(clickPos.X, clickPos.Y)); if (distSq <= closestDistSq) { foundTrigger = trigger; closestDistSq = distSq; } } }
            _selectedTrigger = foundTrigger; Debug.WriteLine($"Trigger selecionado: {(_selectedTrigger?.Id ?? "None")}");
        }

        /// <summary> Adds a new trigger with default properties at the specified world position. </summary>
        private void AddTriggerAt(Vector3 worldPos)
        { /* ... (Code from previous answer) ... */
            if (_currentMapData == null || _currentMapData.Width <= 0) return; if (_currentMapData.Triggers == null) _currentMapData.Triggers = new List<MapTrigger>(); if (worldPos.X < 0 || worldPos.X >= _currentMapData.Width || worldPos.Y < 0 || worldPos.Y >= _currentMapData.Height) return;

            // --- CORREÇÃO AQUI ---
            // Antes: Id = $"Trig_{worldPos.X:0}_{worldPos.Y:0}_{worldPos.Z:0}"
            // Corrigido: Usa um contador simples para um ID mais limpo.
            var newTrigger = new MapTrigger
            {
                Id = $"Trigger_{_currentMapData.Triggers.Count + 1}",
                Position = worldPos,
                TargetMap = "changeme.json",
                TargetPosition = Vector3.Zero,
                Radius = 0.5f
            };
            // --- FIM DA CORREÇÃO ---

            _currentMapData.Triggers.Add(newTrigger); _selectedTrigger = newTrigger; Debug.WriteLine($"Novo trigger adicionado em {worldPos}. ID: {newTrigger.Id}");
        }

        /// <summary> Removes the specified trigger from the map data. </summary>
        private void RemoveTrigger(MapTrigger triggerToRemove)
        { /* ... (Code from previous answer) ... */
            if (_currentMapData?.Triggers == null || triggerToRemove == null) return; bool removed = _currentMapData.Triggers.Remove(triggerToRemove); if (removed) { Debug.WriteLine($"Trigger '{triggerToRemove.Id ?? "(sem ID)"}' removido."); if (_selectedTrigger == triggerToRemove) _selectedTrigger = null; } else { Debug.WriteLine($"Falha ao remover trigger '{triggerToRemove.Id ?? "(sem ID)"}'."); }
        }

        /// <summary> Saves the current map data (_currentMapData) to the specified JSON file. </summary>
        private void SaveMap(string fileName)
        { /* ... (Code from previous answer) ... */
            if (_currentMapData == null) { Debug.WriteLine("SaveMap: Nenhum dado de mapa para salvar."); return; }
            _currentMapData.TileMapping = new List<TileMappingEntry>(_tilePalette ?? new List<TileMappingEntry>()); string directory = Path.Combine("Content", "maps"); string filePath = Path.Combine(directory, fileName); Debug.WriteLine($"Tentando salvar mapa em: {filePath}"); try { Directory.CreateDirectory(directory); string jsonContent = JsonConvert.SerializeObject(_currentMapData, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }); File.WriteAllText(filePath, jsonContent); Debug.WriteLine($"Mapa salvo com sucesso."); } catch (Exception ex) { Debug.WriteLine($"Erro ao salvar mapa em {filePath}: {ex.Message}"); }
        }

        /// <summary> Loads map data from the specified JSON file, or creates a new map if loading fails. </summary>
        private void LoadMap(string fileName)
        { /* ... (Code from previous answer, minor tweaks) ... */
            string filePath = Path.Combine("Content", "maps", fileName); Debug.WriteLine($"Tentando carregar mapa: {filePath}");
            if (!File.Exists(filePath)) { Debug.WriteLine("Arquivo não encontrado. Criando novo mapa padrão."); _currentMapFileName = fileName; CreateNewMap(); return; }
            try { string jsonContent = File.ReadAllText(filePath); var loadedData = JsonConvert.DeserializeObject<MapData>(jsonContent); if (loadedData == null) { Debug.WriteLine($"Falha ao desserializar {filePath}. Criando novo mapa."); _currentMapFileName = fileName; CreateNewMap(); } else { _currentMapData = loadedData; _currentMapFileName = fileName; if (_currentMapData.Layers == null) _currentMapData.Layers = new List<MapLayer>(); if (_currentMapData.Triggers == null) _currentMapData.Triggers = new List<MapTrigger>(); if (_currentMapData.TileMapping == null) _currentMapData.TileMapping = new List<TileMappingEntry>(); if (_currentMapData.TileMapping.Count > 0) { _tilePalette = new List<TileMappingEntry>(_currentMapData.TileMapping); Debug.WriteLine($"Paleta atualizada com base no mapa ({_tilePalette.Count} tiles)."); } else { Debug.WriteLine("Mapa sem tileMapping. Mantendo paleta anterior."); _currentMapData.TileMapping = new List<TileMappingEntry>(_tilePalette ?? new List<TileMappingEntry>()); } UpdateSelectedTileInfo(); RebuildMapSprites(); _selectedTrigger = null; Debug.WriteLine($"Mapa {fileName} carregado. {_currentMapData.Layers.Count} camadas, {_currentMapData.Triggers.Count} triggers."); } } catch (Exception ex) { Debug.WriteLine($"Erro ao carregar mapa {filePath}: {ex.Message}. Criando novo mapa."); _currentMapFileName = fileName; CreateNewMap(); }
        }

        // --- END INTERNAL LOGIC METHODS ---

    } // Fim da classe EditorState
}