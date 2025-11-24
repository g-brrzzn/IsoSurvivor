using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace IsometricGame.Classes
{
    public class WaveDef
    {
        public float StartTime;
        public float EndTime;
        public float SpawnRate;
        public List<Type> EnemyTypes;
    }

    public class WaveManager
    {
        public float Timer { get; private set; } = 0f;
        private List<WaveDef> _waves;
        private float _spawnTimer = 0f;

        public WaveManager()
        {
            InitializeWaves();
        }

        private void InitializeWaves()
        {
            _waves = new List<WaveDef>();
            _waves.Add(new WaveDef
            {
                StartTime = 0,
                EndTime = 30,
                SpawnRate = 1.0f,                EnemyTypes = new List<Type> { typeof(Enemy1) }
            });
            _waves.Add(new WaveDef
            {
                StartTime = 30,
                EndTime = 60,
                SpawnRate = 0.8f,
                EnemyTypes = new List<Type> { typeof(Enemy1), typeof(Bat) }
            });
            _waves.Add(new WaveDef
            {
                StartTime = 60,
                EndTime = 120,
                SpawnRate = 0.5f,                EnemyTypes = new List<Type> { typeof(Enemy1), typeof(Bat), typeof(Enemy1), typeof(Golem) }
            });
            _waves.Add(new WaveDef
            {
                StartTime = 120,
                EndTime = 180,
                SpawnRate = 0.2f,                EnemyTypes = new List<Type> { typeof(Bat), typeof(Bat), typeof(Golem) }            });
            _waves.Add(new WaveDef
            {
                StartTime = 180,
                EndTime = 9999,
                SpawnRate = 0.05f,                EnemyTypes = new List<Type> { typeof(Enemy1), typeof(Bat), typeof(Golem), typeof(Golem) }
            });
        }

        public void Update(GameTime gameTime, Vector3 playerPos)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Timer += dt;
            _spawnTimer -= dt;

            WaveDef currentWave = null;
            foreach (var w in _waves)
            {
                if (Timer >= w.StartTime && Timer < w.EndTime)
                {
                    currentWave = w;
                    break;
                }
            }
            if (currentWave == null && _waves.Count > 0)
                currentWave = _waves[_waves.Count - 1];

            if (currentWave == null) return;

            if (_spawnTimer <= 0)
            {
                _spawnTimer = currentWave.SpawnRate;
                SpawnRandomEnemy(currentWave.EnemyTypes, playerPos);
            }
        }

        private void SpawnRandomEnemy(List<Type> types, Vector3 playerPos)
        {
            if (types.Count == 0) return;

            Type typeToSpawn = types[GameEngine.Random.Next(types.Count)];
            float angle = (float)(GameEngine.Random.NextDouble() * Math.PI * 2);
            float distance = 12.0f + (float)GameEngine.Random.NextDouble() * 6.0f;

            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;
            Vector3 spawnPos = playerPos + new Vector3(offset.X, offset.Y, 0);
            spawnPos.X = MathF.Round(spawnPos.X);
            spawnPos.Y = MathF.Round(spawnPos.Y);
            spawnPos.Z = 0;
            if (!GameEngine.SolidTiles.ContainsKey(spawnPos))
            {
                EnemyBase enemy = null;
                if (typeToSpawn == typeof(Enemy1)) enemy = new Enemy1(spawnPos);
                else if (typeToSpawn == typeof(Bat)) enemy = new Bat(spawnPos);
                else if (typeToSpawn == typeof(Golem)) enemy = new Golem(spawnPos);

                if (enemy != null)
                {
                    GameEngine.AllEnemies.Add(enemy);
                    GameEngine.AllSprites.Add(enemy);
                }
            }
        }
    }
}