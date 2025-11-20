using System;
using System.Collections.Generic;
using System.Linq;
using IsometricGame.Classes;
using Microsoft.Xna.Framework;

namespace IsometricGame.Map
{
    public static class MapGenerator
    {
        public static MapData GenerateProceduralMap(int width, int height)
        {
            var mapData = new MapData
            {
                Width = width,
                Height = height,
                TileMapping = new List<TileMappingEntry>(),
                Layers = new List<MapLayer>(),
                Triggers = new List<MapTrigger>()
            };

            mapData.TileMapping.Add(new TileMappingEntry { Id = 1, AssetName = "tile_grass1", Solid = false });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 2, AssetName = "tile_grass2", Solid = false });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 3, AssetName = "tile_grass3", Solid = false });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 4, AssetName = "tile_dirt1", Solid = false });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 5, AssetName = "tile_dirt2", Solid = false });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 6, AssetName = "tile_dirt3", Solid = false });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 7, AssetName = "water_tile1", Solid = true });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 8, AssetName = "water_tile2", Solid = true });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 9, AssetName = "water_tile3", Solid = true });
            mapData.TileMapping.Add(new TileMappingEntry { Id = 10, AssetName = "tile_wall", Solid = true });

            var groundLayer = new MapLayer
            {
                Name = "Ground",
                ZLevel = 0,
                Data = new List<int>(new int[width * height])
            };

            for (int i = 0; i < groundLayer.Data.Count; i++)
            {
                int rng = GameEngine.Random.Next(0, 100);
                if (rng < 70) groundLayer.Data[i] = 1;                else if (rng < 90) groundLayer.Data[i] = 2;                else groundLayer.Data[i] = 3;            }

            int dirtPatches = (width * height) / 80;            for (int i = 0; i < dirtPatches; i++)
            {
                int px = GameEngine.Random.Next(0, width);
                int py = GameEngine.Random.Next(0, height);
                int radius = GameEngine.Random.Next(3, 6);

                for (int y = py - radius; y <= py + radius; y++)
                {
                    for (int x = px - radius; x <= px + radius; x++)
                    {
                        if (x >= 1 && x < width - 1 && y >= 1 && y < height - 1)
                        {
                            float dist = Vector2.Distance(new Vector2(px, py), new Vector2(x, y));
                            if (dist < radius - (GameEngine.Random.NextDouble() * 0.5))
                            {
                                int dirtId = GameEngine.Random.Next(4, 7);
                                groundLayer.Data[y * width + x] = dirtId;
                            }
                        }
                    }
                }
            }

            int lakes = 5;            for (int i = 0; i < lakes; i++)
            {
                int lx = GameEngine.Random.Next(10, width - 10);
                int ly = GameEngine.Random.Next(10, height - 10);

                if (Vector2.Distance(new Vector2(lx, ly), new Vector2(width / 2, height / 2)) < 15) continue;

                int blobs = GameEngine.Random.Next(3, 6);
                for (int b = 0; b < blobs; b++)
                {
                    int blobX = lx + GameEngine.Random.Next(-4, 5);
                    int blobY = ly + GameEngine.Random.Next(-4, 5);
                    int radius = GameEngine.Random.Next(3, 7);

                    for (int y = blobY - radius; y <= blobY + radius; y++)
                    {
                        for (int x = blobX - radius; x <= blobX + radius; x++)
                        {
                            if (x >= 1 && x < width - 1 && y >= 1 && y < height - 1)
                            {
                                if (Vector2.Distance(new Vector2(blobX, blobY), new Vector2(x, y)) < radius)
                                {
                                    int waterId = GameEngine.Random.Next(7, 10);
                                    groundLayer.Data[y * width + x] = waterId;
                                }
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                groundLayer.Data[0 * width + x] = 10;                groundLayer.Data[(height - 1) * width + x] = 10;            }
            for (int y = 0; y < height; y++)
            {
                groundLayer.Data[y * width + 0] = 10;                groundLayer.Data[y * width + (width - 1)] = 10;            }

            mapData.Layers.Add(groundLayer);
            return mapData;
        }
    }
}