using System;
using System.Collections.Generic;
using System.Linq;
using IsometricGame.Classes.Items;
using IsometricGame.Classes.Weapons;
using Microsoft.Xna.Framework;

namespace IsometricGame.Classes.Upgrades
{
    public static class UpgradeManager
    {
        private static List<Type> _availableWeapons = new List<Type>
        {
            typeof(SimpleWeapon),
            typeof(OrbitingShield)
        };

        private static List<PassiveType> _availablePassives = new List<PassiveType>
        {
            PassiveType.EmptyTome,
            PassiveType.Spinach,
            PassiveType.Bracer,
            PassiveType.Spellbinder
        };

        public static void Initialize() { }

        public static List<UpgradeOption> GetSmartOptions(Player player, int count)
        {
            List<UpgradeOption> pool = new List<UpgradeOption>();
            foreach (var weapon in player.Weapons)
            {
                if (weapon.Level < 8 && !weapon.Name.Contains("Holy"))
                {
                    pool.Add(new UpgradeOption(
                        $"Upgrade: {weapon.Name}",
                        GetWeaponUpgradeDescription(weapon),
                        Color.Orange,
                        p => weapon.LevelUp()
                    ));
                }
            }
            if (player.Weapons.Count < 4)            {
                foreach (var weaponType in _availableWeapons)
                {
                    if (!player.Weapons.Any(w => w.GetType() == weaponType))
                    {
                        string name = weaponType == typeof(SimpleWeapon) ? "Magic Wand" : "Orbiting Shield";

                        pool.Add(new UpgradeOption(
                            $"New: {name}",
                            GetNewWeaponDescription(weaponType),
                            Color.Gold,
                            p => p.Weapons.Add((WeaponBase)Activator.CreateInstance(weaponType, p))
                        ));
                    }
                }
            }
            foreach (var passiveType in _availablePassives)
            {
                var existing = player.Passives.Find(pass => pass.Type == passiveType);

                if (existing == null)
                {
                    if (player.Passives.Count < 4)
                    {
                        pool.Add(new UpgradeOption(
                            $"Get: {GetPassiveName(passiveType)}",
                            GetPassiveDescription(passiveType, 1),                            Color.Cyan,
                            p => p.AddOrLevelUpPassive(passiveType)
                        ));
                    }
                }
                else if (existing.Level < existing.MaxLevel)
                {
                    pool.Add(new UpgradeOption(
                        $"Upgrade: {existing.Name}",
                        GetPassiveDescription(passiveType, existing.Level + 1),
                        Color.LightBlue,
                        p => p.AddOrLevelUpPassive(passiveType)
                    ));
                }
            }
            if (pool.Count < count)
            {
                pool.Add(new UpgradeOption("Roast Chicken", "Recover 30 Health.", Color.Green, p => p.Heal(30)));
                pool.Add(new UpgradeOption("Gold Coin", "Gain 100 Gold (Score).", Color.Yellow, p => { /* TODO: Score */ }));
            }
            return pool.OrderBy(x => GameEngine.Random.Next()).Take(count).ToList();
        }

        private static string GetWeaponUpgradeDescription(WeaponBase weapon)
        {

            if (weapon is SimpleWeapon)
            {
                int nextLvl = weapon.Level + 1;
                if (nextLvl % 2 == 0) return $"Lvl {nextLvl}: Fires +1 Projectile.";
                else return $"Lvl {nextLvl}: Base Damage +10.";
            }
            else if (weapon is OrbitingShield)
            {
                return $"Lvl {weapon.Level + 1}: Adds +1 Shield & +0.5s Duration.";
            }

            return "Increases overall power.";
        }

        private static string GetNewWeaponDescription(Type weaponType)
        {
            if (weaponType == typeof(SimpleWeapon))
                return "Fires magic missiles at the nearest enemy.";
            if (weaponType == typeof(OrbitingShield))
                return "Creates a rotating shield that damages enemies.";

            return "A new weapon.";
        }

        private static string GetPassiveName(PassiveType type)
        {
            switch (type)
            {
                case PassiveType.EmptyTome: return "Empty Tome";
                case PassiveType.Spinach: return "Spinach";
                case PassiveType.Bracer: return "Bracer";
                case PassiveType.Spellbinder: return "Spellbinder";
                default: return type.ToString();
            }
        }

        private static string GetPassiveDescription(PassiveType type, int level)
        {
            string prefix = $"Lvl {level}: ";
            switch (type)
            {
                case PassiveType.EmptyTome:
                    return prefix + "Reduces Weapon Cooldown by 8%.";
                case PassiveType.Spinach:
                    return prefix + "Increases Damage by 10%.";
                case PassiveType.Bracer:
                    return prefix + "Increases Projectile Speed by 10%.";
                case PassiveType.Spellbinder:
                    return prefix + "Increases Effect Duration by 10%.";
                default:
                    return prefix + "Improves stats.";
            }
        }
    }
}