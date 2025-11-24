using IsometricGame.Classes.UI;
using IsometricGame.Classes.Weapons;
using IsometricGame.Classes.Items;using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes.Items
{
    public class Chest : ItemDrop
    {
        public Chest(Vector3 worldPos) : base(worldPos, ItemType.Magnet)
        {
            if (GameEngine.Assets.Images.ContainsKey("chest"))
                UpdateTexture(GameEngine.Assets.Images["chest"]);
            else if (GameEngine.Assets.Images.ContainsKey("tile_dirt2"))
                UpdateTexture(GameEngine.Assets.Images["tile_dirt2"]);            else
                UpdateTexture(GameEngine.Assets.Images["gem_50"]);
        }
        public new void OnPickup(Player player)
        {
            bool evolved = TryEvolveWeapon(player);

            if (evolved)
            {
                GameEngine.FloatingTexts.Add(new FloatingText("EVOLUTION!", WorldPosition + new Vector3(0, 0, 10), Color.Gold, 2.0f));
                if (GameEngine.Assets.Sounds.ContainsKey("menu_confirm"))
                    GameEngine.Assets.Sounds["menu_confirm"].Play(1.0f, 0.5f, 0f);
            }
            else
            {
                player.Heal(player.MaxLife);
                player.AddExperience(50);
                GameEngine.FloatingTexts.Add(new FloatingText("TREASURE!", WorldPosition + new Vector3(0, 0, 10), Color.Yellow, 1.5f));

                if (GameEngine.Assets.Sounds.ContainsKey("menu_select"))
                    GameEngine.Assets.Sounds["menu_select"].Play(1.0f, 0.0f, 0f);
            }

            Kill();
        }

        private bool TryEvolveWeapon(Player player)
        {
            return false;        }
    }
}