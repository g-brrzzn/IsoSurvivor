// Directory: Map
// MapLoader.cs

using IsometricGame.Classes; // Necessário para referenciar a classe Sprite
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
// using IsometricGame.Map; // Não é necessário se MapData, LoadedMapData, etc. estão no mesmo namespace

namespace IsometricGame.Map // Namespace atualizado
{
    public class MapLoader
    {
        public LoadedMapData LoadMapDataFromFile(string filePath)
        {
            // Verifica se o arquivo existe
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"Erro: Arquivo de mapa não encontrado em {filePath}");
                return null; // Retorna null indicando falha
            }

            // Lê o conteúdo do arquivo JSON
            string jsonContent = File.ReadAllText(filePath);
            MapData mapData = null;

            // Tenta desserializar o JSON para o objeto MapData
            try
            {
                mapData = JsonConvert.DeserializeObject<MapData>(jsonContent);
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Erro ao desserializar o mapa {filePath}: {ex.Message}");
                return null; // Retorna null em caso de erro de desserialização
            }


            // Verifica se a desserialização foi bem-sucedida
            if (mapData == null)
            {
                Debug.WriteLine($"Erro: Falha ao desserializar o mapa {filePath} (resultado nulo).");
                return null;
            }

            // Inicializa listas e dicionários para armazenar os dados carregados
            List<Sprite> loadedTileSprites = new List<Sprite>();
            Dictionary<Vector3, Sprite> loadedSolidTiles = new Dictionary<Vector3, Sprite>();
            List<MapTrigger> loadedTriggers = mapData.Triggers ?? new List<MapTrigger>(); // Carrega triggers, garantindo que não seja null

            // Cria um lookup para acesso rápido às informações dos tiles pelo ID
            // Garante que mapData.TileMapping não seja null antes de tentar criar o dicionário
            Dictionary<int, TileMappingEntry> tileLookup = new Dictionary<int, TileMappingEntry>();
            if (mapData.TileMapping != null)
            {
                try
                {
                    tileLookup = mapData.TileMapping.ToDictionary(entry => entry.Id, entry => entry);
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine($"Erro no tileMapping do mapa {filePath}: IDs duplicados? {ex.Message}");
                    // Você pode optar por retornar null aqui ou continuar sem um tileLookup completo
                    return null;
                }
            }
            else
            {
                Debug.WriteLine($"Aviso: tileMapping está nulo ou vazio no mapa {filePath}.");
            }


            // Processa cada camada do mapa
            if (mapData.Layers != null) // Verifica se há camadas
            {
                foreach (var layer in mapData.Layers)
                {
                    if (layer.Data == null) // Pula camadas sem dados
                    {
                        Debug.WriteLine($"Aviso: Camada '{layer.Name}' em {filePath} não possui dados (array 'data' nulo).");
                        continue;
                    }

                    // Processa cada tile na camada
                    for (int i = 0; i < layer.Data.Count; i++)
                    {
                        int tileId = layer.Data[i];
                        if (tileId == 0) continue; // ID 0 representa tile vazio

                        // Calcula as coordenadas X e Y do tile com base no índice e na largura do mapa
                        int x = i % mapData.Width;
                        int y = i / mapData.Width;

                        // Busca as informações do tile (assetName, solid) usando o ID
                        if (tileLookup.TryGetValue(tileId, out TileMappingEntry tileInfo))
                        {
                            // Busca a textura correspondente no AssetManager
                            if (GameEngine.Assets.Images.TryGetValue(tileInfo.AssetName, out Texture2D texture))
                            {
                                // Cria a posição no mundo 3D (X, Y, Z)
                                Vector3 worldPos = new Vector3(x, y, layer.ZLevel);
                                // Cria o sprite do tile
                                var tileSprite = new Sprite(texture, worldPos);

                                // Adiciona o sprite à lista de todos os sprites de tile carregados
                                loadedTileSprites.Add(tileSprite);

                                bool isSolid = false;

                                // Lógica de Solidez:
                                // 1. Prioridade: O que está definido no JSON (`tileInfo.Solid`)
                                if (tileInfo.Solid)
                                {
                                    isSolid = true;
                                }
                                // 2. Se não definido como sólido no JSON, aplicam-se regras da engine:
                                else
                                {
                                    // 2a. Qualquer tile com Z > 0 é sólido por padrão.
                                    if (layer.ZLevel > 0)
                                    {
                                        isSolid = true;
                                        // Opcional: Log para indicar que a engine tornou sólido um tile não marcado no JSON
                                        // Debug.WriteLine($"Aviso: Tile {tileInfo.AssetName} em Z={layer.ZLevel} não marcado como sólido no JSON, mas considerado sólido pela engine (Z>0).");
                                    }
                                    // 2b. No nível Z=0, apenas tiles de água são sólidos por padrão.
                                    else if (layer.ZLevel == 0 && tileInfo.AssetName.Contains("water_"))
                                    {
                                        isSolid = true;
                                        // Opcional: Log para indicar que a engine tornou sólido um tile de água não marcado no JSON
                                        // Debug.WriteLine($"Aviso: Tile de água {tileInfo.AssetName} em Z=0 não marcado como sólido no JSON, mas considerado sólido pela engine (água).");
                                    }
                                    // Tiles em Z=0 que não são água e não estão marcados como solid:true no JSON, `isSolid` permanece `false`.
                                }

                                // Se o tile foi determinado como sólido, adiciona ao dicionário de sólidos
                                if (isSolid)
                                {
                                    loadedSolidTiles[worldPos] = tileSprite;
                                }
                            }
                            else
                            {
                                // Log se a textura definida no JSON não for encontrada no AssetManager
                                Debug.WriteLine($"Aviso: Textura '{tileInfo.AssetName}' (ID: {tileId}) não encontrada no AssetManager para o mapa {filePath}.");
                            }
                        }
                        else
                        {
                            // Log se um ID de tile nos dados da camada não existir no tileMapping
                            Debug.WriteLine($"Aviso: Tile ID {tileId} na camada '{layer.Name}' não encontrado no tileMapping do mapa {filePath}.");
                        }
                    } // Fim do loop de tiles (índice i)
                } // Fim do loop de camadas (layer)
            } // Fim da verificação mapData.Layers != null
            else
            {
                Debug.WriteLine($"Aviso: Nenhuma camada ('layers') encontrada no mapa {filePath}.");
            }

            // Log final indicando sucesso e resumo dos dados carregados
            Debug.WriteLine($"Dados do mapa {filePath} processados. {loadedTileSprites.Count} sprites de tile, {loadedSolidTiles.Count} tiles sólidos, {loadedTriggers.Count} triggers.");

            // Retorna um novo objeto LoadedMapData contendo todas as informações processadas
            return new LoadedMapData(loadedTileSprites, loadedSolidTiles, loadedTriggers, mapData);
        }
    }
}