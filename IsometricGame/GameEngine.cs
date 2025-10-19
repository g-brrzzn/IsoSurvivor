using System.Collections.Generic;
using System;
using IsometricGame.Classes;

namespace IsometricGame
{
    public static class GameEngine
    {
        public static Player Player { get; set; }

        public static List<Sprite> AllSprites { get; private set; }
        public static List<EnemyBase> AllEnemies { get; private set; }
        public static List<Bullet> PlayerBullets { get; private set; }
        public static List<Bullet> EnemyBullets { get; private set; }

        public static AssetManager Assets { get; set; }
        public static int Level { get; set; }
        public static int ScreenShake { get; set; }

        public static Random Random { get; private set; }

        public static void Initialize()
        {
            AllSprites = new List<Sprite>();
            AllEnemies = new List<EnemyBase>();
            PlayerBullets = new List<Bullet>();
            EnemyBullets = new List<Bullet>();
            Assets = new AssetManager();
            Random = new Random();
            Level = 1;
            ScreenShake = 0;
        }

        public static void ResetGame()
        {
            AllSprites.Clear();
            AllEnemies.Clear();
            PlayerBullets.Clear();
            EnemyBullets.Clear();
            Player = null;
            Level = 1;
        }
    }
}