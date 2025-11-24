using Microsoft.Xna.Framework;

namespace IsometricGame.Classes.Weapons
{
    public class MachineGunWand : SimpleWeapon
    {
        public MachineGunWand(Player owner) : base(owner)
        {
            Name = "Holy Wand";            
            BaseCooldown = 0.1f;            
            BaseDamage = 1;        
        }
    }
}