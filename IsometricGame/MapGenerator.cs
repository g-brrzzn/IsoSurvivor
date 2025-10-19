using IsometricGame.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsometricGame
{
    public class MapGenerator
    {
        public void GenerateMap()
        {
            Texture2D floorTexture = GameEngine.Assets.Images["tile_floor"];
            Texture2D wallTexture = GameEngine.Assets.Images["tile_wall"];

            // 1. Gerar o Chão (Z=0)
            for (int x = 0; x < Constants.WorldSize.X; x++)
            {
                for (int y = 0; y < Constants.WorldSize.Y; y++)
                {
                    // --- MODIFICAÇÃO: Usa Vector3(x, y, 0) ---
                    var floorTile = new Sprite(floorTexture, new Vector3(x, y, 0));
                    GameEngine.AllSprites.Add(floorTile);
                }
            }

            // 2. Gerar Estruturas (Z=0)
            for (int i = 5; i < 15; i++)
            {
                // --- MODIFICAÇÃO: Usa Vector3 ---
                var wallLeft = new Sprite(wallTexture, new Vector3(5, i, 0));
                GameEngine.AllSprites.Add(wallLeft);
                var wallTop = new Sprite(wallTexture, new Vector3(i, 5, 0));
                GameEngine.AllSprites.Add(wallTop);
            }

            // --- MODIFICAÇÃO: Usa Vector3 ---
            var doorTile = new Sprite(floorTexture, new Vector3(5, 10, 0));
            GameEngine.AllSprites.Add(doorTile);


            // --- ADIÇÃO: Exemplo de Empilhamento ---
            // Cria uma pequena pilha de paredes em (10, 15)
            var baseWall = new Sprite(wallTexture, new Vector3(10, 15, 0));
            var stackedWall1 = new Sprite(wallTexture, new Vector3(10, 15, 1)); // Z=1
            var stackedWall2 = new Sprite(wallTexture, new Vector3(10, 15, 2)); // Z=2
            // Adiciona um chão no topo da pilha
            var topFloor = new Sprite(floorTexture, new Vector3(10, 15, 3)); // Z=3

            GameEngine.AllSprites.Add(baseWall);
            GameEngine.AllSprites.Add(stackedWall1);
            GameEngine.AllSprites.Add(stackedWall2);
            GameEngine.AllSprites.Add(topFloor);
            // --- FIM DA ADIÇÃO ---
        }
    }
}