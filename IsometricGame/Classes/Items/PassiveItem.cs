using System;

namespace IsometricGame.Classes.Items
{
    public enum PassiveType
    {
        EmptyTome,        Spinach,        Bracer,        Spellbinder    }

    public class PassiveItem
    {
        public PassiveType Type { get; private set; }
        public string Name { get; private set; }
        public int Level { get; private set; } = 0;
        public int MaxLevel { get; private set; } = 5;
        private Action<Player> _onLevelUp;

        public PassiveItem(PassiveType type, string name, int maxLevel, Action<Player> onLevelUp)
        {
            Type = type;
            Name = name;
            MaxLevel = maxLevel;
            _onLevelUp = onLevelUp;
        }

        public void LevelUp(Player player)
        {
            if (Level < MaxLevel)
            {
                Level++;
                _onLevelUp?.Invoke(player);
            }
        }
    }
}