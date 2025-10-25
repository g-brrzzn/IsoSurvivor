using IsometricGame.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IsometricGame.Map
{
    public class MapGenerator
    {
        public void GenerateMap()
        {
            Texture2D floorTexture = GameEngine.Assets.Images["tile_floor"];
            Texture2D wallTexture = GameEngine.Assets.Images["tile_wall"];

            for (int x = 0; x < Constants.WorldSize.X; x++)
            {
                for (int y = 0; y < Constants.WorldSize.Y; y++)
                {
                    var floorTile = new Sprite(floorTexture, new Vector3(x, y, 0));
                    GameEngine.AllSprites.Add(floorTile);
                }
            }

            for (int i = 5; i < 15; i++)
            {
                var wallLeft = new Sprite(wallTexture, new Vector3(5, i, 0));
                GameEngine.AllSprites.Add(wallLeft);
                var wallTop = new Sprite(wallTexture, new Vector3(i, 5, 0));
                GameEngine.AllSprites.Add(wallTop);
            }

            var doorTile = new Sprite(floorTexture, new Vector3(5, 10, 0));
            GameEngine.AllSprites.Add(doorTile);


            var baseWall = new Sprite(wallTexture, new Vector3(10, 15, 0));
            var stackedWall1 = new Sprite(wallTexture, new Vector3(10, 15, 1));            var stackedWall2 = new Sprite(wallTexture, new Vector3(10, 15, 2));            var topFloor = new Sprite(floorTexture, new Vector3(10, 15, 3));
            GameEngine.AllSprites.Add(baseWall);
            GameEngine.AllSprites.Add(stackedWall1);
            GameEngine.AllSprites.Add(stackedWall2);
            GameEngine.AllSprites.Add(topFloor);
        }
    }
}