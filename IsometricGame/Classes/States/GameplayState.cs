using IsometricGame.Classes;
using IsometricGame.Classes.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IsometricGame.States
{
    public class GameplayState : GameStateBase
    {
        private Explosion _hitExplosion;
        private Fall _backgroundFall;
        private MapLoader _mapLoader;
        private Texture2D _cursorTexture;

        public override void Start()
        {
            base.Start();
            GameEngine.ResetGame();

            _hitExplosion = new Explosion();
            _backgroundFall = new Fall(300);

            _mapLoader = new MapLoader();
            _mapLoader.LoadMap("Content/maps/map1.json");

            Vector3 playerStartPos = new Vector3(2, 2, 0);
            GameEngine.Player = new Player(playerStartPos);
            GameEngine.AllSprites.Add(GameEngine.Player);

            SpawnEnemies();

            _cursorTexture = GameEngine.Assets.Images["cursor"];
            Game1.Instance.IsMouseVisible = false;
        }

        public override void End()
        {
            Game1.Instance.IsMouseVisible = true;        }

        private void SpawnEnemies()
        {
            for (int i = 0; i < GameEngine.Level * 3; i++)            {
                float x = GameEngine.Random.Next(0, 20);
                float y = GameEngine.Random.Next(0, 20);

                if (Vector2.Distance(new Vector2(x, y), new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y)) < 5.0f)
                {
                    i--;                    continue;
                }

                SpawnEnemy(typeof(Enemy1), new Vector3(x, y, 0));            }
        }

        private void SpawnEnemy(Type enemyType, Vector3 worldPos)
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
                return;
            }
            if (GameEngine.Player == null)
            {
                if (!IsDone)
                {
                    IsDone = true;
                    NextState = "GameOver";
                }
                return;
            }

            Vector2 mouseInternalPos = input.InternalMousePosition;

            Vector2 isoScreenPos = Game1.Camera.ScreenToWorld(mouseInternalPos);

            Vector2 targetWorldPos = IsoMath.ScreenToWorld(isoScreenPos);

            Vector2 cursorDrawPos = mouseInternalPos;

            GameEngine.TargetWorldPosition = targetWorldPos;
            GameEngine.CursorScreenPosition = cursorDrawPos;

            Debug.WriteLine($"MouseInternal: {mouseInternalPos} -> IsoScreen(CamInv): {isoScreenPos} -> TargetWorld(IsoInv): {targetWorldPos} -> CursorDraw: {cursorDrawPos}");

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
                return;
            }
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
                Vector2 enemyPosXY = new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y);

                for (int j = GameEngine.PlayerBullets.Count - 1; j >= 0; j--)
                {
                    var bullet = GameEngine.PlayerBullets[j];
                    if (bullet.IsRemoved) continue;

                    Vector2 bulletPosXY = new Vector2(bullet.WorldPosition.X, bullet.WorldPosition.Y);

                    if (Vector2.Distance(bulletPosXY, enemyPosXY) < (enemyCollisionRadius + bulletCollisionRadius))
                    {
                        if (enemy.Texture != null)
                            _hitExplosion.Create(enemy.ScreenPosition.X, enemy.ScreenPosition.Y - enemy.Origin.Y);                        enemy.Damage(gameTime);
                        bullet.Kill();
                    }
                }
            }
            if (!GameEngine.Player.IsRemoved)
            {
                Vector2 playerPosXY = new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y);

                for (int i = GameEngine.EnemyBullets.Count - 1; i >= 0; i--)
                {
                    var bullet = GameEngine.EnemyBullets[i];
                    if (bullet.IsRemoved) continue;

                    Vector2 bulletPosXY = new Vector2(bullet.WorldPosition.X, bullet.WorldPosition.Y);

                    if (Vector2.Distance(bulletPosXY, playerPosXY) < (playerCollisionRadius + bulletCollisionRadius))
                    {
                        GameEngine.Player.TakeDamage(gameTime);
                        bullet.Kill();
                        if (GameEngine.Player.IsRemoved) break;
                    }
                }
                if (!GameEngine.Player.IsRemoved)
                {
                    for (int i = GameEngine.AllEnemies.Count - 1; i >= 0; i--)
                    {
                        var enemy = GameEngine.AllEnemies[i];
                        if (enemy.IsRemoved) continue;

                        Vector2 enemyPosXY = new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y);

                        if (Vector2.Distance(enemyPosXY, playerPosXY) < (enemyCollisionRadius + playerCollisionRadius))
                        {
                            GameEngine.Player.TakeDamage(gameTime);
                            enemy.Kill();
                            GameEngine.ScreenShake = 5;
                            if (GameEngine.Player.IsRemoved) break;
                        }
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

            var font = GameEngine.Assets.Fonts["captain_32"];

            Vector2 levelPos = new Vector2(Constants.InternalResolution.X - 100, 30);
            Vector2 lifePos = new Vector2(Constants.InternalResolution.X - 100, 60);

            DrawUtils.DrawTextScreen(spriteBatch, $"Level {GameEngine.Level}", font, levelPos, Color.White, 1.0f);
            if (GameEngine.Player != null)
                DrawUtils.DrawTextScreen(spriteBatch, $"Life  {GameEngine.Player.Life}", font, lifePos, Color.White, 1.0f);
            else
                DrawUtils.DrawTextScreen(spriteBatch, "Life  0", font, lifePos, Color.Red, 1.0f);

            if (_cursorTexture != null)
            {
                Vector2 cursorPos = GameEngine.CursorScreenPosition;
                Vector2 origin = new Vector2(_cursorTexture.Width / 2f, _cursorTexture.Height / 2f);

                spriteBatch.Draw(
                    _cursorTexture,
                    cursorPos,                    null,
                    Color.White,
                    0f,
                    origin,
                    1.0f,                    SpriteEffects.None,
                    0.0f                );
            }
        }
    }
}