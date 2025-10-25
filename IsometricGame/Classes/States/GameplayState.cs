// Directory: Classes/States
// GameplayState.cs

using IsometricGame.Classes;
using IsometricGame.Classes.Particles;
using IsometricGame.Map; // Adicionado para usar MapManager, MapTrigger, etc.
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
// using System.Runtime.CompilerServices; // Não parece ser necessário

namespace IsometricGame.States
{
    // NOVO ENUM para controlar o estado da transição
    public enum TransitionState
    {
        Idle,
        FadingOut,
        FadingIn
    }

    public class GameplayState : GameStateBase
    {
        // Variáveis existentes
        private Explosion _hitExplosion;
        private Fall _backgroundFall; // Considerar se o background fall faz sentido em todos os mapas
        private MapManager _mapManager;
        private Texture2D _cursorTexture;

        // --- NOVAS VARIÁVEIS PARA TRANSIÇÃO ---
        private TransitionState _transitionState = TransitionState.Idle;
        private float _fadeAlpha = 0f; // Opacidade (0.0 = transparente, 1.0 = preto opaco)
        private float _fadeSpeed = 1.5f; // Velocidade do fade (maior = mais rápido)
        private bool _isTransitioning = false;
        private MapTrigger _pendingTrigger = null; // Guarda o trigger que iniciou a transição
        private Texture2D _pixelTexture; // Textura 1x1 preta para o fade
        // --- FIM DAS NOVAS VARIÁVEIS ---

        public GameplayState()
        {
            _mapManager = new MapManager(); // Inicializa o MapManager
        }

        public override void Start()
        {
            base.Start();
            // GameEngine.ResetGame(); // Movido para LoadInitialMap e antes de carregar novo mapa em UpdateTransition

            _hitExplosion = new Explosion();
            _backgroundFall = new Fall(300); // Pode precisar ser reconfigurado a cada LoadMap

            // Configurações iniciais da transição
            _transitionState = TransitionState.Idle;
            _fadeAlpha = 0f;
            _isTransitioning = false;
            _pendingTrigger = null;

            // Carrega o mapa inicial (sem fade-in por enquanto, poderia ser adicionado)
            LoadInitialMap("map1.json");

            // Pega texturas necessárias que não mudam com o mapa
            _cursorTexture = GameEngine.Assets.Images["cursor"];
            // Pega a textura do pixel (garanta que ela exista no AssetManager)
            if (!GameEngine.Assets.Images.TryGetValue("pixel", out _pixelTexture))
            {
                // Fallback: Cria um pixel branco se não existir
                _pixelTexture = new Texture2D(Game1._graphicsManagerInstance.GraphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
                GameEngine.Assets.Images["pixel"] = _pixelTexture; // Adiciona ao AssetManager para futuro uso
                Debug.WriteLine("Aviso: Textura 'pixel' não encontrada no AssetManager. Criado pixel branco temporário.");
            }

            Game1.Instance.IsMouseVisible = false;
        }

        // Método separado para carregar o primeiro mapa
        private void LoadInitialMap(string mapFileName)
        {
            GameEngine.ResetGame(); // Limpa TUDO (AllSprites, listas, etc.)
            bool mapLoaded = _mapManager.LoadMap(mapFileName);

            if (!mapLoaded)
            {
                Debug.WriteLine("ERRO CRÍTICO: Mapa inicial não pôde ser carregado!");
                IsDone = true;
                NextState = "Menu";
                return;
            }

            // Define posição inicial (TODO: Ler de MapData.StartPoint?)
            Vector3 playerStartPos = new Vector3(2, 2, 0);
            GameEngine.Player = new Player(playerStartPos);
            // Adiciona jogador APÓS carregar o mapa (pois ResetGame limpa AllSprites)
            GameEngine.AllSprites.Add(GameEngine.Player);

            SpawnEnemies(); // Popula o mapa inicial
        }


        public override void End()
        {
            _mapManager.UnloadCurrentMap(); // Garante que o mapa seja descarregado ao sair do estado
            Game1.Instance.IsMouseVisible = true;
            // Limpa entidades dinâmicas ao sair do estado de jogo completamente
            ClearDynamicEntities();
            if (GameEngine.Player != null)
            {
                GameEngine.Player.Kill(); // Marca o jogador para remoção
                GameEngine.AllSprites.Remove(GameEngine.Player); // Remove da lista principal
                GameEngine.Player = null; // Anula a referência
            }

        }

        // TODO: A lógica de SpawnEnemies talvez precise ler o MapData para
        // determinar onde e quantos inimigos spawnar baseado no mapa atual.
        private void SpawnEnemies()
        {
            // Limpa inimigos restantes de um mapa anterior (caso a transição não tenha limpado por algum motivo)
            GameEngine.AllEnemies.Clear();
            GameEngine.AllSprites.RemoveAll(s => s is EnemyBase);

            Debug.WriteLine($"Spawning inimigos para Nível {GameEngine.Level} no mapa {_mapManager.CurrentMapName}");
            // Lógica simples de spawn atual
            int mapWidth = _mapManager.GetCurrentMapData()?.OriginalMapData?.Width ?? 20; // Usa largura do mapa ou default
            int mapHeight = _mapManager.GetCurrentMapData()?.OriginalMapData?.Height ?? 20;

            for (int i = 0; i < GameEngine.Level * 3; i++)
            {
                // Tenta encontrar uma posição válida
                int attempts = 0;
                Vector3 spawnPos;
                bool positionFound = false;
                while (attempts < 50 && !positionFound) // Limita tentativas para evitar loop infinito
                {
                    float x = GameEngine.Random.Next(0, mapWidth);
                    float y = GameEngine.Random.Next(0, mapHeight);
                    spawnPos = new Vector3(x, y, 0); // Assume Z=0 para spawn

                    // Checa distância do jogador E se a posição não é sólida
                    if ((GameEngine.Player == null || Vector2.Distance(new Vector2(x, y), new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y)) > 5.0f)
                        && !GameEngine.SolidTiles.ContainsKey(spawnPos)
                        && !GameEngine.SolidTiles.ContainsKey(spawnPos + new Vector3(0, 0, 1))) // Checa tile acima também
                    {
                        positionFound = true;
                        SpawnEnemy(typeof(Enemy1), spawnPos);
                    }
                    attempts++;
                }
                if (!positionFound) Debug.WriteLine($"Não foi possível encontrar posição válida para spawn de inimigo {i + 1} após {attempts} tentativas.");

            }
        }

        private void SpawnEnemy(Type enemyType, Vector3 worldPos)
        {
            EnemyBase enemy = null;
            if (enemyType == typeof(Enemy1)) enemy = new Enemy1(worldPos);
            // Adicionar outros tipos de inimigos aqui (else if...)

            if (enemy != null)
            {
                GameEngine.AllEnemies.Add(enemy);
                GameEngine.AllSprites.Add(enemy);
            }
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            // --- Atualiza a lógica de transição PRIMEIRO ---
            UpdateTransition(gameTime);

            // Se estiver em transição (fade ativo), não processa o resto do jogo
            if (_isTransitioning)
            {
                return;
            }
            // --- Fim da lógica de transição ---


            // --- Lógica normal do jogo (só executa se não estiver em transição) ---
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float effectiveDt = dt * Constants.BaseSpeedMultiplier;

            // Input e Checagens básicas
            if (input.IsKeyPressed("ESC"))
            {
                IsDone = true;
                NextState = "Pause";
                return;
            }
            // Verifica se o jogador existe ANTES de tentar usá-lo
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved)
            {
                if (!IsDone) { IsDone = true; NextState = "GameOver"; }
                return;
            }

            // Input do Mouse e Player
            Vector2 mouseInternalPos = input.InternalMousePosition;
            Vector2 isoScreenPos = Game1.Camera.ScreenToWorld(mouseInternalPos);
            Vector2 targetWorldPos = IsoMath.ScreenToWorld(isoScreenPos);
            Vector2 cursorDrawPos = mouseInternalPos;
            GameEngine.TargetWorldPosition = targetWorldPos;
            GameEngine.CursorScreenPosition = cursorDrawPos;
            // Debug.WriteLine($"MouseInternal: {mouseInternalPos} -> IsoScreen(CamInv): {isoScreenPos} -> TargetWorld(IsoInv): {targetWorldPos} -> CursorDraw: {cursorDrawPos}"); // Comentar se poluir muito o log
            GameEngine.Player.GetInput(input);

            // Update de Efeitos e Sprites
            _hitExplosion.Update(effectiveDt);
            // Atualiza todos os sprites (jogador, inimigos, balas) - Tiles não têm lógica de Update
            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                // Checagem extra de segurança para evitar erros se a lista for modificada durante o loop (embora CleanupSprites deva cuidar disso)
                if (i < GameEngine.AllSprites.Count)
                {
                    var sprite = GameEngine.AllSprites[i];
                    // Verifica se o sprite não é nulo e não foi removido antes de chamar Update
                    if (sprite != null && !sprite.IsRemoved)
                    {
                        sprite.Update(gameTime, effectiveDt);
                    }
                }
            }

            // Colisões e Limpeza
            HandleCollisions(gameTime);
            CleanupSprites(); // Remove entidades marcadas com IsRemoved = true

            // Checa morte do jogador PÓS-colisão
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved)
            {
                if (!IsDone) { IsDone = true; NextState = "GameOver"; }
                return;
            }

            // Checa ativação de triggers de mapa (só se não estiver já em transição)
            CheckForMapTransition();

            // Lógica de Nível/Spawn (verifica se todos inimigos foram derrotados)
            if (!GameEngine.AllEnemies.Any(e => !e.IsRemoved)) // Checa se não há inimigos ativos
            {
                // TODO: Adicionar lógica de "próximo nível" ou objetivo do mapa aqui.
                // Por enquanto, apenas spawna mais inimigos no mesmo mapa.
                // GameEngine.Level++; // Pode fazer sentido incrementar o nível
                Debug.WriteLine("Todos os inimigos derrotados! Spawning próxima onda...");
                SpawnEnemies(); // Spawna nova onda (ou lógica de próximo nível)
            }
            // --- Fim da lógica normal do jogo ---
        }

        // Atualiza a animação e lógica do fade
        private void UpdateTransition(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_transitionState == TransitionState.FadingOut)
            {
                // Aumenta a opacidade
                _fadeAlpha += _fadeSpeed * elapsedSeconds;
                if (_fadeAlpha >= 1.0f)
                {
                    _fadeAlpha = 1.0f; // Garante opacidade total (preto)

                    // --- EXECUTA A TROCA DE MAPA QUANDO A TELA ESTÁ PRETA ---
                    Debug.WriteLine("Fade Out completo. Iniciando troca de mapa...");
                    ClearDynamicEntities(); // Limpa inimigos/balas do mapa antigo

                    // Carrega o novo mapa usando os dados guardados em _pendingTrigger
                    bool loaded = _mapManager.LoadMap(_pendingTrigger.TargetMap);

                    if (loaded)
                    {
                        // Reposiciona o jogador (verifica se ele ainda existe)
                        if (GameEngine.Player != null && !GameEngine.Player.IsRemoved)
                        {
                            GameEngine.Player.WorldPosition = _pendingTrigger.TargetPosition;
                            GameEngine.Player.WorldVelocity = Vector2.Zero; // Para o jogador
                            GameEngine.Player.UpdateScreenPosition(); // Atualiza posição na tela imediatamente
                            Debug.WriteLine($"Player reposicionado para {_pendingTrigger.TargetPosition} no mapa '{_pendingTrigger.TargetMap}'");

                            // Garante que o jogador esteja na lista após limpeza e recarga (se ResetGame não o removeu)
                            if (!GameEngine.AllSprites.Contains(GameEngine.Player))
                            {
                                Debug.WriteLine("Adicionando jogador de volta a AllSprites após transição.");
                                GameEngine.AllSprites.Add(GameEngine.Player);
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Jogador não existe ou foi removido durante a transição, não será reposicionado.");
                        }

                        // Spawna inimigos para o novo mapa
                        SpawnEnemies(); // TODO: Ler dados de spawn do novo mapa
                    }
                    else
                    {
                        // Falha ao carregar! Volta ao menu como segurança.
                        Debug.WriteLine($"ERRO FATAL: Falha ao carregar mapa '{_pendingTrigger.TargetMap}'. Indo para o Menu.");
                        IsDone = true; // Sinaliza para Game1 trocar de estado
                        NextState = "Menu";
                        _transitionState = TransitionState.Idle; // Reseta estado da transição
                        _isTransitioning = false;
                        _pendingTrigger = null;
                        _fadeAlpha = 0f; // Remove o fade para não ficar tela preta no menu
                        return; // Sai imediatamente do UpdateTransition
                    }
                    // --- FIM DA TROCA DE MAPA ---

                    _pendingTrigger = null; // Limpa o trigger pendente
                    _transitionState = TransitionState.FadingIn; // Começa a clarear a tela
                    Debug.WriteLine("Troca de mapa concluída. Iniciando Fade In.");
                }
            }
            else if (_transitionState == TransitionState.FadingIn)
            {
                // Diminui a opacidade
                _fadeAlpha -= _fadeSpeed * elapsedSeconds;
                if (_fadeAlpha <= 0.0f)
                {
                    _fadeAlpha = 0.0f; // Garante transparência total
                    _transitionState = TransitionState.Idle; // Transição concluída
                    _isTransitioning = false; // Libera a lógica normal do jogo no próximo Update
                    Debug.WriteLine("Fade In completo. Transição finalizada.");
                }
            }
            // Se _transitionState == TransitionState.Idle, não faz nada aqui.
        }


        // Verifica se o jogador está sobre algum trigger
        private void CheckForMapTransition()
        {
            // Só checa se não estiver já em transição e se o jogador existir e estiver ativo
            if (_isTransitioning || GameEngine.Player == null || GameEngine.Player.IsRemoved) return;

            Vector3 playerPos = GameEngine.Player.WorldPosition;
            List<MapTrigger> triggers = _mapManager.GetCurrentTriggers();

            if (triggers == null || triggers.Count == 0) return; // Sai se não há triggers no mapa atual

            foreach (var trigger in triggers)
            {
                if (string.IsNullOrEmpty(trigger.TargetMap)) continue; // Pula triggers sem destino

                // Calcula a distância no plano XY (ignora Z para ativação)
                float distanceSq = Vector2.DistanceSquared(
                    new Vector2(playerPos.X, playerPos.Y),
                    new Vector2(trigger.Position.X, trigger.Position.Y)
                );

                // Usa o raio definido no trigger (ou o default de 0.5f)
                float triggerRadiusSq = trigger.Radius * trigger.Radius;

                // Compara distância ao quadrado para evitar raiz quadrada
                if (distanceSq <= triggerRadiusSq)
                {
                    // Verifica também se o jogador está no mesmo nível Z do trigger
                    // (ou uma pequena margem, se necessário)
                    if (Math.Abs(playerPos.Z - trigger.Position.Z) < 0.1f) // Comparação de float com margem
                    {
                        Debug.WriteLine($"Player ativou trigger '{trigger.Id ?? "N/A"}' para mapa '{trigger.TargetMap}'");
                        InitiateMapTransition(trigger); // Inicia o processo de fade out
                        return; // Importante: Sai após ativar o PRIMEIRO trigger encontrado
                    }
                }
            }
        }

        // Inicia o processo de transição (começa o fade out)
        private void InitiateMapTransition(MapTrigger trigger)
        {
            if (_isTransitioning) return; // Segurança extra: não inicia outra transição

            Debug.WriteLine("Iniciando Fade Out para transição...");
            _isTransitioning = true; // Bloqueia a lógica normal do jogo em Update()
            _pendingTrigger = trigger; // Guarda os dados do trigger para usar quando a tela estiver preta
            _transitionState = TransitionState.FadingOut;
            _fadeAlpha = Math.Max(0f, _fadeAlpha); // Garante que começa do zero ou de onde parou um fade-in interrompido
        }

        // Remove inimigos e balas antes de trocar de mapa
        private void ClearDynamicEntities()
        {
            Debug.WriteLine("Limpando entidades dinâmicas (Inimigos, Balas)...");
            int enemyCount = 0;
            int bulletCount = 0;

            // Itera de trás para frente para remover da lista AllSprites
            for (int i = GameEngine.AllSprites.Count - 1; i >= 0; i--)
            {
                var sprite = GameEngine.AllSprites[i];
                if (sprite is EnemyBase || sprite is Bullet)
                {
                    sprite.Kill(); // Marca como removido (importante se houver outras referências)
                    GameEngine.AllSprites.RemoveAt(i);
                    if (sprite is EnemyBase) enemyCount++;
                    else bulletCount++;
                }
            }

            // Limpa as listas específicas (redundante se AllSprites é a fonte principal, mas seguro)
            GameEngine.AllEnemies.Clear();
            GameEngine.PlayerBullets.Clear();
            GameEngine.EnemyBullets.Clear();

            Debug.WriteLine($"Removidas {enemyCount} inimigos e {bulletCount} balas.");
        }

        // Lógica de colisão (permanece a mesma)
        private void HandleCollisions(GameTime gameTime)
        {
            if (GameEngine.Player == null || GameEngine.Player.IsRemoved) return;
            const float enemyCollisionRadius = 0.8f;
            const float bulletCollisionRadius = 0.3f;
            const float playerCollisionRadius = 0.6f; // Usado na colisão inimigo-jogador e bala-jogador

            // Colisão: Balas do Jogador -> Inimigos
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

                    if (Vector2.DistanceSquared(bulletPosXY, enemyPosXY) < MathF.Pow(enemyCollisionRadius + bulletCollisionRadius, 2))
                    {
                        if (enemy.Texture != null)
                            _hitExplosion.Create(enemy.ScreenPosition.X, enemy.ScreenPosition.Y - enemy.Origin.Y);
                        enemy.Damage(gameTime);
                        bullet.Kill(); // Marcar bala para remoção
                    }
                }
            }

            // Colisões envolvendo o Jogador (só se ele não foi removido ainda)
            if (!GameEngine.Player.IsRemoved)
            {
                Vector2 playerPosXY = new Vector2(GameEngine.Player.WorldPosition.X, GameEngine.Player.WorldPosition.Y);

                // Colisão: Balas Inimigas -> Jogador
                for (int i = GameEngine.EnemyBullets.Count - 1; i >= 0; i--)
                {
                    var bullet = GameEngine.EnemyBullets[i];
                    if (bullet.IsRemoved) continue;
                    Vector2 bulletPosXY = new Vector2(bullet.WorldPosition.X, bullet.WorldPosition.Y);

                    if (Vector2.DistanceSquared(bulletPosXY, playerPosXY) < MathF.Pow(playerCollisionRadius + bulletCollisionRadius, 2))
                    {
                        GameEngine.Player.TakeDamage(gameTime);
                        bullet.Kill();
                        if (GameEngine.Player.IsRemoved) break; // Para se o jogador morreu
                    }
                }

                // Colisão: Inimigos -> Jogador (só se jogador ainda vivo)
                if (!GameEngine.Player.IsRemoved)
                {
                    for (int i = GameEngine.AllEnemies.Count - 1; i >= 0; i--)
                    {
                        var enemy = GameEngine.AllEnemies[i];
                        if (enemy.IsRemoved) continue;
                        Vector2 enemyPosXY = new Vector2(enemy.WorldPosition.X, enemy.WorldPosition.Y);

                        if (Vector2.DistanceSquared(enemyPosXY, playerPosXY) < MathF.Pow(enemyCollisionRadius + playerCollisionRadius, 2))
                        {
                            GameEngine.Player.TakeDamage(gameTime);
                            enemy.Kill(); // Inimigo também morre na colisão (ou sofre dano?)
                            GameEngine.ScreenShake = 5;
                            if (GameEngine.Player.IsRemoved) break; // Para se o jogador morreu
                        }
                    }
                }
            }
        }

        // Remove sprites marcados como IsRemoved das listas principais
        private void CleanupSprites()
        {
            // Remove da lista principal AllSprites primeiro, exceto o jogador
            GameEngine.AllSprites.RemoveAll(s => s != null && s.IsRemoved && s != GameEngine.Player);

            // Remove das listas específicas (pode ser redundante se AllSprites é a fonte mestre)
            GameEngine.AllEnemies.RemoveAll(e => e.IsRemoved);
            GameEngine.PlayerBullets.RemoveAll(b => b.IsRemoved);
            GameEngine.EnemyBullets.RemoveAll(b => b.IsRemoved);

            // Checagem final se o jogador foi marcado como removido (ex: TakeDamage)
            if (GameEngine.Player != null && GameEngine.Player.IsRemoved)
            {
                // Não remove de AllSprites aqui, apenas anula a referência principal
                GameEngine.Player = null;
                Debug.WriteLine("Jogador removido (vida <= 0).");
            }
        }

        // Desenha o mundo do jogo (tiles, jogador, inimigos, balas) - Chamado pelo Game1.Draw com a matriz da câmera
        public void DrawWorld(SpriteBatch spriteBatch)
        {
            // Desenha todos os sprites (tiles inclusos) que não foram removidos
            // A ordenação BackToFront do SpriteBatch cuida da profundidade
            foreach (var sprite in GameEngine.AllSprites)
            {
                // Verificação extra de nulidade
                if (sprite != null && !sprite.IsRemoved)
                    sprite.Draw(spriteBatch);
            }

            // Desenha efeitos de partículas que podem precisar ser desenhados separadamente ou em outra camada
            // (Atualmente, Player.Draw e EnemyBase.Draw já chamam Draw de suas explosões)
            // if (GameEngine.Player != null)
            //     GameEngine.Player.ExplosionEffect.Draw(spriteBatch);
            _hitExplosion.Draw(spriteBatch); // Explosões de acerto genéricas
        }

        // Desenha a UI do GameplayState (vida, nível, cursor) - Chamado pelo Game1.Draw SEM a matriz da câmera
        public override void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            var font = GameEngine.Assets.Fonts["captain_32"];
            Vector2 levelPos = new Vector2(Constants.InternalResolution.X - 100, 30);
            Vector2 lifePos = new Vector2(Constants.InternalResolution.X - 100, 60);

            // Usa DrawTextScreen que calcula a origem para centralizar
            DrawUtils.DrawTextScreen(spriteBatch, $"Level {GameEngine.Level}", font, levelPos, Color.White, 0.1f); // Depth < 1 para ficar na frente
            if (GameEngine.Player != null && !GameEngine.Player.IsRemoved)
                DrawUtils.DrawTextScreen(spriteBatch, $"Life  {GameEngine.Player.Life}", font, lifePos, Color.White, 0.1f);
            else
                DrawUtils.DrawTextScreen(spriteBatch, "Life  0", font, lifePos, Color.Red, 0.1f);

            // Desenha o cursor se não estiver em transição
            if (_cursorTexture != null && !_isTransitioning)
            {
                Vector2 cursorPos = GameEngine.CursorScreenPosition;
                Vector2 origin = new Vector2(_cursorTexture.Width / 2f, _cursorTexture.Height / 2f);
                // Desenha com depth 0 (o mais na frente possível nesta camada UI)
                spriteBatch.Draw(
                    _cursorTexture, cursorPos, null, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0.0f
                );
            }
        }

        // Desenha o overlay de fade - Chamado pelo Game1.Draw DEPOIS de tudo, SEM câmera
        public void DrawTransitionOverlay(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            // Só desenha se houver alguma opacidade
            if (_fadeAlpha > 0.001f) // Usa uma pequena tolerância para evitar desenho desnecessário
            {
                // Garante que alpha esteja entre 0 e 1
                float clampedAlpha = MathHelper.Clamp(_fadeAlpha, 0f, 1f);
                Color fadeColor = Color.Black * clampedAlpha; // Cor preta com a opacidade atual

                // Retângulo cobrindo toda a resolução interna
                Rectangle screenRectangle = new Rectangle(0, 0, Constants.InternalResolution.X, Constants.InternalResolution.Y);

                // Desenha usando a textura _pixelTexture (1x1) esticada
                // Usa depth 0.0f para garantir que fique na frente de toda a UI desenhada antes no mesmo batch
                if (_pixelTexture != null) // Verifica se a textura foi carregada/criada
                {
                    spriteBatch.Draw(_pixelTexture, screenRectangle, null, fadeColor, 0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                }
                else
                {
                    Debug.WriteLine("Erro: _pixelTexture é nula em DrawTransitionOverlay!");
                }
            }
        }

    } // Fim da classe GameplayState
}