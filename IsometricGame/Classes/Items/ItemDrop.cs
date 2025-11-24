using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace IsometricGame.Classes.Items
{
    public enum ItemType
    {
        HealthPotion,
        Magnet,
        Bomb
    }

    public class ItemDrop : Sprite
    {
        public ItemType Type { get; private set; }
        private float _floatTimer;

        public ItemDrop(Vector3 worldPos, ItemType type) : base(null, worldPos)
        {
            Type = type;
            _floatTimer = (float)GameEngine.Random.NextDouble() * 10f;

            switch (type)
            {
                case ItemType.HealthPotion:
                    if (GameEngine.Assets.Images.ContainsKey("item_potion"))
                        UpdateTexture(GameEngine.Assets.Images["item_potion"]);
                    else
                        UpdateTexture(GameEngine.Assets.Images["gem_50"]);
                    break;

                case ItemType.Magnet:
                    if (GameEngine.Assets.Images.ContainsKey("item_magnet"))
                        UpdateTexture(GameEngine.Assets.Images["item_magnet"]);
                    else
                        UpdateTexture(GameEngine.Assets.Images["gem_1"]);
                    break;
            }
            if (Texture != null)
                Origin = new Vector2(Texture.Width / 2f, Texture.Height);
        }

        public override void Update(GameTime gameTime, float dt)
        {
            _floatTimer += dt * 3f;
            float floatOffset = (float)Math.Sin(_floatTimer) * 5f + 20f;
            BaseYOffsetWorld = floatOffset;

            UpdateScreenPosition();
        }

        public void OnPickup(Player player)
        {
            switch (Type)
            {
                case ItemType.HealthPotion:
                    player.Heal(1);
                    if (GameEngine.Assets.Sounds.ContainsKey("menu_select"))
                        GameEngine.Assets.Sounds["menu_select"].Play(1.0f, 0.5f, 0f);

                    GameEngine.FloatingTexts.Add(new IsometricGame.Classes.UI.FloatingText(
                        "+1 HP",
                        WorldPosition + new Vector3(0, 0, 15),
                        Color.LimeGreen,
                        1.0f
                    ));
                    break;

                case ItemType.Magnet:
                    ActivateMagnet();
                    if (GameEngine.Assets.Sounds.ContainsKey("menu_confirm"))
                        GameEngine.Assets.Sounds["menu_confirm"].Play(1.0f, 0.2f, 0f);

                    GameEngine.FloatingTexts.Add(new IsometricGame.Classes.UI.FloatingText(
                        "MAGNET!",
                        WorldPosition + new Vector3(0, 0, 15),
                        Color.Cyan,
                        1.5f
                    ));
                    break;
            }

            Kill();
        }

        private void ActivateMagnet()
        {
            foreach (var gem in GameEngine.Gems)
            {
                if (!gem.IsRemoved)
                {
                    gem.ForceMagnetize();
                }
            }
        }
    }
}