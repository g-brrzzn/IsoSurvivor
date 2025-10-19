using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricGame.Classes;
using IsometricGame.Classes.Particles;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace IsometricGame.States
{
    public class GameplayState : GameStateBase
    {
        private Explosion _hitExplosion;
        private Fall _backgroundFall;
        private MapGenerator _mapGenerator;

        public override void Start()
        {
            base.Start();
            GameEngine.ResetGame();

            _hitExplosion = new Explosion();
            _backgroundFall = new Fall(300);

            // 1. Gera o mapa PRIMEIRO
            _mapGenerator = new MapGenerator();
            _mapGenerator.GenerateMap(); // Isso irá popular GameEngine.AllSprites com tiles

            // 2. Adiciona o Player
            Vector2 playerPos = new Vector2(10, 10); // Posição inicial no meio do "castelo"
            GameEngine.Player = new Player(playerPos);
            GameEngine.AllSprites.Add(GameEngine.Player); // Adiciona o player DEPOIS dos tiles

            // 3. Adiciona os Inimigos
            SpawnEnemies();
        }

        private void SpawnEnemies()
        {
            for (int i = 0; i < GameEngine.Level * 5; i++)
            {
                float x = GameEngine.Random.Next(-10, 10);
                float y = GameEngine.Random.Next(-10, 10);
                if (Math.Abs(x) < 2 && Math.Abs(y) < 2) continue;
                SpawnEnemy(typeof(Enemy1), new Vector2(x, y));
            }
        }

        private void SpawnEnemy(Type enemyType, Vector2 worldPos)
        {
            EnemyBase enemy = null;
            if (enemyType == typeof(Enemy1)) enemy = new Enemy1(worldPos);

            if (enemy != null)
            {
                GameEngine.AllEnemies.Add(enemy);
                GameEngine.AllSprites.Add(enemy);
            }
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float effectiveDt = dt * Constants.BaseSpeedMultiplier;
            if (input.IsKeyPressed("ESC"))
            {
                IsDone = true;
                NextState = "Pause";
                return;            }
            if (GameEngine.Player == null)
            {
                if (!IsDone)
                {
                    IsDone = true;
                    NextState = "GameOver";
                }
                return;            }
            GameEngine.Player.GetInput(input);
            _hitExplosion.Update(effectiveDt);
            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                if (i < GameEngine.AllSprites.Count)
                {
                    var sprite = GameEngine.AllSprites[i];
                    if (!sprite.IsRemoved)
                    {
                        sprite.Update(gameTime, effectiveDt);
                    }
                }
            }
            HandleCollisions(gameTime);
            CleanupSprites();
            if (GameEngine.Player == null)
            {
                if (!IsDone)
                {
                    IsDone = true;
                    NextState = "GameOver";
                }
                return;            }
            if (GameEngine.AllEnemies.Count == 0)
            {
                GameEngine.Level++;
                SpawnEnemies();
            }
        }
        private void HandleCollisions(GameTime gameTime)
        {
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved) return;
            const float enemyCollisionRadius = 0.8f;
            const float bulletCollisionRadius = 0.3f;
            const float playerCollisionRadius = 0.6f;
            for (int i = GameEngine.AllEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = GameEngine.AllEnemies[i];
                if (enemy.IsRemoved) continue;

                for (int j = GameEngine.PlayerBullets.Count - 1; j >= 0; j--)
                {
                    var bullet = GameEngine.PlayerBullets[j];
                    if (bullet.IsRemoved) continue;

                    if (Vector2.Distance(bullet.WorldPosition, enemy.WorldPosition) < (enemyCollisionRadius + bulletCollisionRadius))
                    {
                        if (enemy.Texture != null)
                            _hitExplosion.Create(enemy.ScreenPosition.X, enemy.ScreenPosition.Y - enemy.Texture.Height / 2f);
                        enemy.Damage(gameTime);
                        bullet.Kill();
                    }
                }
            }
            if (!GameEngine.Player.IsRemoved)
            {
                for (int i = GameEngine.EnemyBullets.Count - 1; i >= 0; i--)
                {
                    var bullet = GameEngine.EnemyBullets[i];
                    if (bullet.IsRemoved) continue;

                    if (Vector2.Distance(bullet.WorldPosition, GameEngine.Player.WorldPosition) < (playerCollisionRadius + bulletCollisionRadius))
                    {
                        GameEngine.Player.TakeDamage(gameTime);
                        bullet.Kill();
                        if (GameEngine.Player.IsRemoved) break;                    }
                }
                if (!GameEngine.Player.IsRemoved)
                {
                    for (int i = GameEngine.AllEnemies.Count - 1; i >= 0; i--)
                    {
                        var enemy = GameEngine.AllEnemies[i];
                        if (enemy.IsRemoved) continue;

                        if (Vector2.Distance(enemy.WorldPosition, GameEngine.Player.WorldPosition) < (enemyCollisionRadius + playerCollisionRadius))
                        {
                            GameEngine.Player.TakeDamage(gameTime);
                            enemy.Kill();
                            GameEngine.ScreenShake = 5;
                            if (GameEngine.Player.IsRemoved) break;                        }
                    }
                }
            }
        }
        private void CleanupSprites()
        {
            GameEngine.AllSprites.RemoveAll(s => s.IsRemoved);
            GameEngine.AllEnemies.RemoveAll(e => e.IsRemoved);
            GameEngine.PlayerBullets.RemoveAll(b => b.IsRemoved);
            GameEngine.EnemyBullets.RemoveAll(b => b.IsRemoved);

            if (GameEngine.Player != null && GameEngine.Player.IsRemoved)
            {
                GameEngine.Player = null;
            }
        }

        public void DrawWorld(SpriteBatch spriteBatch)
        {
            foreach (var sprite in GameEngine.AllSprites)
            {
                if (!sprite.IsRemoved)
                    sprite.Draw(spriteBatch);
            }

            if (GameEngine.Player != null)
                GameEngine.Player.ExplosionEffect.Draw(spriteBatch);

            _hitExplosion.Draw(spriteBatch);
        }
        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // O código de desenhar sprites foi movido para DrawWorld()

            var font = GameEngine.Assets.Fonts["captain_32"];

            // Usa coordenadas de tela
            Vector2 levelPos = new Vector2(Constants.InternalResolution.X - 100, 30);
            Vector2 lifePos = new Vector2(Constants.InternalResolution.X - 100, 60);

            // Usa a nova função DrawTextScreen
            DrawUtils.DrawTextScreen(spriteBatch, $"Level {GameEngine.Level}", font, levelPos, Color.White, 1.0f);
            if (GameEngine.Player != null)
                DrawUtils.DrawTextScreen(spriteBatch, $"Life  {GameEngine.Player.Life}", font, lifePos, Color.White, 1.0f);
            else
                DrawUtils.DrawTextScreen(spriteBatch, "Life  0", font, lifePos, Color.Red, 1.0f);
        }
    }
}