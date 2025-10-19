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

            // 1. Gerar o Chão
            // Itera por todo o tamanho do mundo definido em Constants
            for (int x = 0; x < Constants.WorldSize.X; x++)
            {
                for (int y = 0; y < Constants.WorldSize.Y; y++)
                {
                    // Cria um novo sprite para o tile de chão
                    var floorTile = new Sprite(floorTexture, new Vector2(x, y));

                    // Adiciona o tile à lista global de sprites para ser renderizado
                    GameEngine.AllSprites.Add(floorTile);
                }
            }

            // 2. Gerar Estruturas (Ex: paredes de um "castelo")
            // Isso demonstra como adicionar estruturas "em cima" do chão
            // O sistema de profundidade (GetDepth) fará o resto.
            for (int i = 5; i < 15; i++)
            {
                // Parede esquerda
                var wallLeft = new Sprite(wallTexture, new Vector2(5, i));
                GameEngine.AllSprites.Add(wallLeft);

                // Parede do fundo
                var wallTop = new Sprite(wallTexture, new Vector2(i, 5));
                GameEngine.AllSprites.Add(wallTop);
            }

            // Adiciona uma "porta"
            var doorTile = new Sprite(floorTexture, new Vector2(5, 10)); // Re-adiciona um tile de chão
            GameEngine.AllSprites.Add(doorTile);
        }
    }
}