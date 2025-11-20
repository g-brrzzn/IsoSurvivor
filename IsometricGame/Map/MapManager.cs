using IsometricGame.Classes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace IsometricGame.Map
{
    public class MapManager
    {
        private MapLoader _mapLoader;
        private List<Sprite> _currentMapSprites;
        private LoadedMapData _currentLoadedMapData;
        public string CurrentMapName { get; private set; }

        public MapManager()
        {
            _mapLoader = new MapLoader();
            _currentMapSprites = new List<Sprite>();
            CurrentMapName = null;
        }

        public bool LoadMap(string mapFileName)
        {
            Debug.WriteLine($"MapManager: Carregando arquivo '{mapFileName}'...");
            UnloadCurrentMap();

            var data = _mapLoader.LoadMapDataFromFile($"Content/maps/{mapFileName}");
            if (data == null) return false;

            ApplyLoadedData(data, mapFileName);
            return true;
        }

        public void LoadMapFromData(MapData mapData, string virtualName)
        {
            Debug.WriteLine($"MapManager: Carregando mapa procedural '{virtualName}'...");
            UnloadCurrentMap();

            List<Sprite> tileSprites = new List<Sprite>();
            Dictionary<Vector3, Sprite> solidTiles = new Dictionary<Vector3, Sprite>();

            var tileLookup = mapData.TileMapping.ToDictionary(t => t.Id, t => t);

            foreach (var layer in mapData.Layers)
            {
                for (int i = 0; i < layer.Data.Count; i++)
                {
                    int tileId = layer.Data[i];
                    if (tileId == 0) continue;

                    int x = i % mapData.Width;
                    int y = i / mapData.Width;
                    Vector3 pos = new Vector3(x, y, layer.ZLevel);

                    if (tileLookup.TryGetValue(tileId, out var tileInfo))
                    {
                        if (GameEngine.Assets.Images.TryGetValue(tileInfo.AssetName, out var tex))
                        {
                            var sprite = new Sprite(tex, pos);
                            tileSprites.Add(sprite);

                            if (tileInfo.Solid) solidTiles[pos] = sprite;
                        }
                    }
                }
            }

            var loadedData = new LoadedMapData(tileSprites, solidTiles, mapData.Triggers, mapData);
            ApplyLoadedData(loadedData, virtualName);
        }

        private void ApplyLoadedData(LoadedMapData data, string name)
        {
            _currentLoadedMapData = data;
            CurrentMapName = name;

            _currentMapSprites.AddRange(data.TileSprites);
            GameEngine.AllSprites.AddRange(_currentMapSprites);

            foreach (var kvp in data.SolidTiles)
            {
                GameEngine.SolidTiles[kvp.Key] = kvp.Value;
            }

            Debug.WriteLine($"Mapa carregado. Total Sprites: {_currentMapSprites.Count}");
        }

        public void UnloadCurrentMap()
        {
            if (CurrentMapName == null || _currentMapSprites.Count == 0) return;

            foreach (var s in _currentMapSprites)
            {
                GameEngine.AllSprites.Remove(s);
            }
            _currentMapSprites.Clear();
            GameEngine.SolidTiles.Clear();
            _currentLoadedMapData = null;
            CurrentMapName = null;
        }

        public LoadedMapData GetCurrentMapData() => _currentLoadedMapData;
        public List<MapTrigger> GetCurrentTriggers() => _currentLoadedMapData?.Triggers ?? new List<MapTrigger>();
    }
}