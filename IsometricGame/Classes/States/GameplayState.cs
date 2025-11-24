using IsometricGame.Classes;
using IsometricGame.Classes.Items;
using IsometricGame.Classes.Particles;
using IsometricGame.Classes.UI;
using IsometricGame.Classes.Upgrades;
using IsometricGame.Classes.Weapons.Projectiles;
using IsometricGame.Classes.Physics;
using IsometricGame.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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
        private SpatialGrid _spatialGrid;

        private TransitionState _transitionState = TransitionState.Idle;
        private float _fadeAlpha = 0f;
        private float _fadeSpeed = 1.5f;
        private bool _isTransitioning = false;

        public GameplayState()
        {
            _mapManager = new MapManager();
        }

        public override void Start()
        {
            base.Start();
            _hitExplosion = new Explosion();
            _backgroundFall = new Fall(300);
            _spatialGrid = new SpatialGrid(150);
            if (GameEngine.Assets.Images.ContainsKey("cursor"))
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
                GameEngine.CurrentUpgradeOptions = UpgradeManager.GetSmartOptions(GameEngine.Player, 3);
                IsDone = true; NextState = "LevelUp"; return;
            }

            _waveManager.Update(gameTime, GameEngine.Player.WorldPosition);
            _hitExplosion.Update(effectiveDt);
            _spatialGrid.Clear();
            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                var sprite = GameEngine.AllSprites[i];
                if (sprite != null && !sprite.IsRemoved)
                {
                    sprite.Update(gameTime, effectiveDt);

                    if (sprite is EnemyBase enemy)
                    {
                        _spatialGrid.Register(enemy);
                    }
                }
            }
            for (int i = GameEngine.FloatingTexts.Count - 1; i >= 0; i--)
            {
                var text = GameEngine.FloatingTexts[i];
                text.Update(effectiveDt);
                if (text.IsRemoved) GameEngine.FloatingTexts.RemoveAt(i);
            }

            HandleCollisions(gameTime);
            HandleItemPickups();
            CleanupSprites();
        }

        private void HandleCollisions(GameTime gameTime)
        {
            if (GameEngine.Player == null) return;
            Vector2 playerPos2D = new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y);
            var enemiesNearPlayer = _spatialGrid.RetrieveNear(playerPos2D);

            foreach (var enemy in enemiesNearPlayer)
            {
                if (enemy.IsRemoved) continue;

                if (Vector2.DistanceSquared(new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y), playerPos2D) < 0.64f)
                {
                    GameEngine.Player.TakeDamage(gameTime);
                }
            }
            for (int j = GameEngine.PlayerBullets.Count - 1; j >= 0; j--)
            {
                var bullet = GameEngine.PlayerBullets[j];
                if (bullet.IsRemoved) continue;

                var enemiesNearBullet = _spatialGrid.RetrieveNear(new Vector2(bullet.WorldPosition.X, bullet.WorldPosition.Y));

                foreach (var enemy in enemiesNearBullet)
                {
                    if (bullet.IsRemoved) break;
                    if (enemy.IsRemoved || bullet.HitList.Contains(enemy)) continue;

                    float collisionDistSq = MathF.Pow(0.6f * bullet.Scale, 2);

                    if (Vector2.DistanceSquared(new Vector2(bullet.WorldPosition.X, bullet.WorldPosition.Y),
                                                new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y)) < collisionDistSq)
                    {
                        ApplyBulletHit(bullet, enemy, gameTime);
                    }
                }
            }
            for (int k = GameEngine.AllSprites.Count - 1; k >= 0; k--)
            {
                var sprite = GameEngine.AllSprites[k];

                if (sprite is OrbitingProjectile orb && !orb.IsRemoved)
                {
                    Vector2 orbPos2D = new Vector2(orb.WorldPosition.X, orb.WorldPosition.Y);
                    var enemiesNearOrb = _spatialGrid.RetrieveNear(orbPos2D);
                    float orbRadiusSq = 0.8f * 0.8f;

                    foreach (var enemy in enemiesNearOrb)
                    {
                        if (enemy.IsRemoved) continue;

                        if (Vector2.DistanceSquared(orbPos2D, new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y)) < orbRadiusSq)
                        {
                            if (orb.CanHit(enemy, gameTime))
                            {
                                enemy.Damage(gameTime, orb.DamageAmount);
                                Vector2 pushDir = new Vector2(enemy.WorldPosition.X - orb.WorldPosition.X, enemy.WorldPosition.Y - orb.WorldPosition.Y);
                                if (pushDir != Vector2.Zero) pushDir.Normalize();
                                enemy.ApplyKnockback(pushDir * orb.KnockbackPower * 3.0f);
                            }
                        }
                    }
                }
            }
        }

        private void HandleItemPickups()
        {
            if (GameEngine.Player == null) return;

            for (int i = GameEngine.Items.Count - 1; i >= 0; i--)
            {
                var item = GameEngine.Items[i];
                if (item.IsRemoved)
                {
                    GameEngine.Items.RemoveAt(i);
                    continue;
                }

                float distSq = Vector2.DistanceSquared(
                    new Vector2(item.WorldPosition.X, item.WorldPosition.Y),
                    new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y)
                );

                if (distSq < 0.8f * 0.8f)
                {
                    item.OnPickup(GameEngine.Player);
                }
            }
        }

        private void ApplyBulletHit(Bullet bullet, EnemyBase enemy, GameTime gameTime)
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

        private void ApplyAreaKnockback(Vector3 center, float radius, float force)
        {
            Vector2 centerXY = new Vector2(center.X, center.Y);
            var nearby = _spatialGrid.RetrieveNear(centerXY);
            float radiusSq = radius * radius;

            foreach (var enemy in nearby)
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
                    enemy.ApplyKnockback(pushDir * force * factor * 0.5f);
                }
            }
        }

        private void ClearDynamicEntities()
        {
            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                var s = GameEngine.AllSprites[i];
                if (s is EnemyBase || s is Bullet || s is ExperienceGem || s is OrbitingProjectile || s is ItemDrop)
                {
                    s.Kill();
                    GameEngine.AllSprites.RemoveAt(i);
                }
            }
            GameEngine.AllEnemies.Clear();
            GameEngine.PlayerBullets.Clear();
            GameEngine.EnemyBullets.Clear();
            GameEngine.Gems.Clear();
            GameEngine.Items.Clear();
            GameEngine.FloatingTexts.Clear();
        }

        private void CleanupSprites()
        {
            GameEngine.AllSprites.RemoveAll(s => s.IsRemoved);
            GameEngine.AllEnemies.RemoveAll(e => e.IsRemoved);
            GameEngine.PlayerBullets.RemoveAll(b => b.IsRemoved);
            GameEngine.EnemyBullets.RemoveAll(b => b.IsRemoved);
            GameEngine.Gems.RemoveAll(g => g.IsRemoved);
        }

        public void DrawWorld(SpriteBatch spriteBatch)
        {
            int margin = 200;
            Vector2 camPos = Game1.Camera.Position;
            Vector2 screenCenter = new Vector2(Constants.InternalResolution.X / 2, Constants.InternalResolution.Y / 2);
            int count = GameEngine.AllSprites.Count;
            for (int i = 0; i < count; i++)
            {
                var sprite = GameEngine.AllSprites[i];
                if (sprite != null && !sprite.IsRemoved)
                {
                    Vector2 posOnScreen = sprite.ScreenPosition - camPos + screenCenter;

                    if (posOnScreen.X > -margin && posOnScreen.X < Constants.InternalResolution.X + margin &&
                        posOnScreen.Y > -margin && posOnScreen.Y < Constants.InternalResolution.Y + margin)
                    {
                        sprite.Draw(spriteBatch);
                    }
                }
            }

            _hitExplosion.Draw(spriteBatch);

            foreach (var text in GameEngine.FloatingTexts)
            {
                text.Draw(spriteBatch);
            }
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

            Color hpColor = (GameEngine.Player.Life <= 1) ? Color.Red : Color.White;
            DrawUtils.DrawTextScreen(spriteBatch, $"HP: {GameEngine.Player.Life}/{GameEngine.Player.MaxLife}", font, new Vector2(barX + barW + 80, barY - 5), hpColor, 0f);

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