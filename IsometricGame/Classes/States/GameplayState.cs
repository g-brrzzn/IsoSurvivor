using IsometricGame.Classes;
using IsometricGame.Classes.Particles;
using IsometricGame.Classes.Upgrades;
using IsometricGame.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace IsometricGame.States
{
    public enum TransitionState { Idle, FadingOut, FadingIn }

    public class GameplayState : GameStateBase
    {
        private Explosion _hitExplosion;
        private Fall _backgroundFall;
        private MapManager _mapManager;
        private WaveManager _waveManager;
        private Texture2D _cursorTexture;
        private Texture2D _pixelTexture;

        private TransitionState _transitionState = TransitionState.Idle;
        private float _fadeAlpha = 0f;
        private float _fadeSpeed = 1.5f;
        private bool _isTransitioning = false;
        private MapTrigger _pendingTrigger = null;

        private Rectangle _cameraViewBounds;

        public GameplayState()
        {
            _mapManager = new MapManager();
        }

        public override void Start()
        {
            base.Start();
            _hitExplosion = new Explosion();
            _backgroundFall = new Fall(300);
            _cursorTexture = GameEngine.Assets.Images["cursor"];
            if (GameEngine.Assets.Images.ContainsKey("pixel"))
                _pixelTexture = GameEngine.Assets.Images["pixel"];

            Game1.Instance.IsMouseVisible = false;

            if (GameEngine.Player != null && !GameEngine.Player.IsRemoved)
            {
                if (GameEngine.Player.Experience >= GameEngine.Player.ExperienceToNextLevel)
                {
                    GameEngine.Player.ConfirmLevelUp();
                }
                return;
            }

            _transitionState = TransitionState.Idle;
            _fadeAlpha = 0f;
            _waveManager = new WaveManager();

            GenerateAndLoadArena();
        }

        private void GenerateAndLoadArena()
        {
            GameEngine.ResetGame();

            var procData = MapGenerator.GenerateProceduralMap(80, 80);
            _mapManager.LoadMapFromData(procData, "Procedural_Arena");

            GameEngine.Player = new Player(new Vector3(40, 40, 0));
            GameEngine.AllSprites.Add(GameEngine.Player);
        }

        public override void End()
        {
            if (NextState == "Pause" || NextState == "LevelUp") return;
            _mapManager.UnloadCurrentMap();
            Game1.Instance.IsMouseVisible = true;
            ClearDynamicEntities();
            GameEngine.Player = null;
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            UpdateTransition(gameTime);
            if (_isTransitioning) return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float effectiveDt = dt * Constants.BaseSpeedMultiplier;

            if (input.IsKeyPressed("ESC")) { IsDone = true; NextState = "Pause"; return; }

            if (GameEngine.Player == null || GameEngine.Player.IsRemoved)
            {
                if (!IsDone) { IsDone = true; NextState = "GameOver"; }
                return;
            }

            GameEngine.Player.GetInput(input);

            if (GameEngine.Player.Experience >= GameEngine.Player.ExperienceToNextLevel)
            {
                GameEngine.CurrentUpgradeOptions = UpgradeManager.GetRandomOptions(3);
                IsDone = true; NextState = "LevelUp"; return;
            }

            _waveManager.Update(gameTime, GameEngine.Player.WorldPosition);

            _hitExplosion.Update(effectiveDt);

            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                var sprite = GameEngine.AllSprites[i];
                if (sprite != null && !sprite.IsRemoved)
                    sprite.Update(gameTime, effectiveDt);
            }

            HandleCollisions(gameTime);
            CleanupSprites();
            CheckForMapTransition();
        }

        private void HandleCollisions(GameTime gameTime)
        {
            if (GameEngine.Player == null) return;

            for (int i = GameEngine.AllEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = GameEngine.AllEnemies[i];
                if (enemy.IsRemoved) continue;

                if (Vector2.DistanceSquared(new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y),
                                            new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y)) < 0.8f)
                {
                    GameEngine.Player.TakeDamage(gameTime);
                }

                for (int j = GameEngine.PlayerBullets.Count - 1; j >= 0; j--)
                {
                    var bullet = GameEngine.PlayerBullets[j];
                    if (bullet.IsRemoved) continue;
                    if (bullet.HitList.Contains(enemy)) continue;

                    float collisionDistSq = MathF.Pow(0.6f * bullet.Scale, 2);

                    if (Vector2.DistanceSquared(new Vector2(bullet.WorldPosition.X, bullet.WorldPosition.Y),
                                                new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y)) < collisionDistSq)
                    {
                        if (enemy.Texture != null)
                            _hitExplosion.Create(enemy.ScreenPosition.X, enemy.ScreenPosition.Y - enemy.Origin.Y);

                        enemy.Damage(gameTime, bullet.DamageAmount);

                        Vector2 pushDir = new Vector2(bullet.WorldVelocity.X, bullet.WorldVelocity.Y);
                        if (pushDir != Vector2.Zero) pushDir.Normalize();

                        float totalForce = 5.0f + bullet.KnockbackPower;
                        enemy.ApplyKnockback(pushDir * totalForce);

                        float radius = 2.5f + (bullet.KnockbackPower * 0.5f);
                        ApplyAreaKnockback(enemy.WorldPosition, radius, totalForce);

                        bullet.HitList.Add(enemy);
                        bullet.PiercingLeft--;
                        if (bullet.PiercingLeft < 0) bullet.Kill();
                    }
                }
            }
        }

        private void ApplyAreaKnockback(Vector3 center, float radius, float force)
        {
            float radiusSq = radius * radius;
            Vector2 centerXY = new Vector2(center.X, center.Y);

            foreach (var enemy in GameEngine.AllEnemies)
            {
                if (enemy.IsRemoved) continue;
                Vector2 enemyPos = new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y);
                float distSq = Vector2.DistanceSquared(centerXY, enemyPos);

                if (distSq < radiusSq)
                {
                    Vector2 pushDir = enemyPos - centerXY;
                    if (pushDir == Vector2.Zero) pushDir = new Vector2(1, 0);
                    else pushDir.Normalize();

                    float dist = MathF.Sqrt(distSq);
                    float factor = 1.0f - (dist / radius);
                    enemy.ApplyKnockback(pushDir * force * factor);
                }
            }
        }

        private void UpdateTransition(GameTime gameTime) { /* Mantido igual ao anterior */ }
        private void CheckForMapTransition() { /* Mantido igual ao anterior */ }
        private void ClearDynamicEntities()
        {
            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                var s = GameEngine.AllSprites[i];
                if (s is EnemyBase || s is Bullet || s is ExperienceGem)
                {
                    s.Kill();
                    GameEngine.AllSprites.RemoveAt(i);
                }
            }
            GameEngine.AllEnemies.Clear();
            GameEngine.PlayerBullets.Clear();
            GameEngine.EnemyBullets.Clear();
        }
        private void CleanupSprites()
        {
            GameEngine.AllSprites.RemoveAll(s => s.IsRemoved);
            GameEngine.AllEnemies.RemoveAll(e => e.IsRemoved);
            GameEngine.PlayerBullets.RemoveAll(b => b.IsRemoved);
            GameEngine.EnemyBullets.RemoveAll(b => b.IsRemoved);
        }

        public void DrawWorld(SpriteBatch spriteBatch)
        {


            int margin = 100;            _cameraViewBounds = new Rectangle(
                -margin,
                -margin,
                Constants.InternalResolution.X + (margin * 2),
                Constants.InternalResolution.Y + (margin * 2)
            );



            Vector2 camPos = Game1.Camera.Position;
            Vector2 screenCenter = new Vector2(Constants.InternalResolution.X / 2, Constants.InternalResolution.Y / 2);

            foreach (var sprite in GameEngine.AllSprites)
            {
                if (sprite != null && !sprite.IsRemoved)
                {
                    Vector2 posOnScreen = sprite.ScreenPosition - camPos + screenCenter;

                    if (posOnScreen.X > -200 && posOnScreen.X < Constants.InternalResolution.X + 200 &&
                        posOnScreen.Y > -200 && posOnScreen.Y < Constants.InternalResolution.Y + 200)
                    {
                        sprite.Draw(spriteBatch);
                    }
                }
            }

            _hitExplosion.Draw(spriteBatch);
        }

        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            DrawHUD(spriteBatch);
            if (_cursorTexture != null && !_isTransitioning)
            {
                Vector2 mousePos = Game1.InputManagerInstance.InternalMousePosition;
                spriteBatch.Draw(_cursorTexture, mousePos, Color.White);
            }
        }

        private void DrawHUD(SpriteBatch spriteBatch)
        {
            if (GameEngine.Player == null) return;

            int screenW = Constants.InternalResolution.X;
            int barW = 600;
            int barH = 20;
            int barX = (screenW - barW) / 2;
            int barY = 20;

            if (_pixelTexture != null)
            {
                spriteBatch.Draw(_pixelTexture, new Rectangle(barX, barY, barW, barH), Color.Black * 0.6f);
                float pct = (float)GameEngine.Player.Experience / (float)GameEngine.Player.ExperienceToNextLevel;
                pct = MathHelper.Clamp(pct, 0f, 1f);
                spriteBatch.Draw(_pixelTexture, new Rectangle(barX, barY, (int)(barW * pct), barH), Color.Cyan);
            }

            var font = GameEngine.Assets.Fonts["captain_32"];
            string lvlText = $"LVL {GameEngine.Player.Level}";
            DrawUtils.DrawTextScreen(spriteBatch, lvlText, font, new Vector2(barX - 80, barY - 5), Color.White, 0f);
            DrawUtils.DrawTextScreen(spriteBatch, $"HP: {GameEngine.Player.Life}/{GameEngine.Player.MaxLife}", font, new Vector2(barX + barW + 20, barY - 5), Color.Red, 0f);

            if (_waveManager != null)
            {
                TimeSpan t = TimeSpan.FromSeconds(_waveManager.Timer);
                string timeStr = $"{t.Minutes:D2}:{t.Seconds:D2}";
                DrawUtils.DrawTextScreen(spriteBatch, timeStr, font, new Vector2(screenW / 2, barY + 30), Color.Gold, 0f);
            }
        }

        public void DrawTransitionOverlay(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (_fadeAlpha > 0f && _pixelTexture != null)
            {
                spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, Constants.InternalResolution.X, Constants.InternalResolution.Y), Color.Black * _fadeAlpha);
            }
        }
    }
}