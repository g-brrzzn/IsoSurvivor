// Directory: . (ou Classes/Map)
// MapManager.cs

using IsometricGame.Classes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;

namespace IsometricGame.Map
{
    public class MapManager
    {
        private MapLoader _mapLoader;
        private List<Sprite> _currentMapSprites; // Sprites pertencentes APENAS ao mapa atual
        private LoadedMapData _currentLoadedMapData; // Dados do mapa carregado
        public string CurrentMapName { get; private set; }

        public MapManager()
        {
            _mapLoader = new MapLoader();
            _currentMapSprites = new List<Sprite>();
            CurrentMapName = null;
        }

        public bool LoadMap(string mapFileName)
        {
            Debug.WriteLine($"MapManager: Tentando carregar mapa '{mapFileName}'...");
            // Descarrega o mapa anterior, se houver
            UnloadCurrentMap();

            // Carrega os dados do novo mapa
            _currentLoadedMapData = _mapLoader.LoadMapDataFromFile($"Content/maps/{mapFileName}"); // Assumindo que os mapas estão em Content/maps

            if (_currentLoadedMapData == null)
            {
                Debug.WriteLine($"MapManager: Falha ao carregar dados de '{mapFileName}'.");
                CurrentMapName = null;
                return false;
            }

            CurrentMapName = mapFileName;

            // Adiciona os novos sprites de tile à lista global e à lista local
            _currentMapSprites.AddRange(_currentLoadedMapData.TileSprites);
            GameEngine.AllSprites.AddRange(_currentMapSprites);

            // Adiciona os novos tiles sólidos ao dicionário global
            // (GameEngine.SolidTiles foi limpo em UnloadCurrentMap)
            foreach (var kvp in _currentLoadedMapData.SolidTiles)
            {
                GameEngine.SolidTiles[kvp.Key] = kvp.Value;
            }

            Debug.WriteLine($"MapManager: Mapa '{mapFileName}' carregado. Adicionados {_currentMapSprites.Count} sprites e {GameEngine.SolidTiles.Count} tiles sólidos ao GameEngine.");
            return true;
        }

        public void UnloadCurrentMap()
        {
            if (CurrentMapName == null || _currentMapSprites.Count == 0)
            {
                // Nenhum mapa carregado ou já descarregado
                return;
            }

            Debug.WriteLine($"MapManager: Descarregando mapa '{CurrentMapName}'...");

            // Remove os sprites do mapa atual da lista global
            // É mais eficiente remover assim do que usar RemoveAll com Contains
            int removedCount = 0;
            for (int i = _currentMapSprites.Count - 1; i >= 0; i--)
            {
                if (GameEngine.AllSprites.Remove(_currentMapSprites[i]))
                {
                    removedCount++;
                }
                // Marcar o sprite como removido pode ser útil se outras partes do código
                // ainda tiverem referências a ele, embora para tiles isso seja menos provável.
                // _currentMapSprites[i].Kill();
            }


            Debug.WriteLine($"MapManager: Removidos {removedCount} sprites do mapa de GameEngine.AllSprites.");

            // Limpa a lista local de sprites do mapa
            _currentMapSprites.Clear();

            // Limpa o dicionário global de tiles sólidos
            GameEngine.SolidTiles.Clear();
            Debug.WriteLine($"MapManager: GameEngine.SolidTiles limpo.");


            _currentLoadedMapData = null;
            CurrentMapName = null;
            Debug.WriteLine($"MapManager: Mapa descarregado.");
        }

        // Método auxiliar para obter dados do mapa atual, se necessário
        public LoadedMapData GetCurrentMapData()
        {
            return _currentLoadedMapData;
        }

        // Método helper opcional para acesso direto aos triggers
        public List<MapTrigger> GetCurrentTriggers()
        {
            return _currentLoadedMapData?.Triggers ?? new List<MapTrigger>(); // Retorna lista vazia se não houver mapa
        }
    }
}