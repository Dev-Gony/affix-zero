using System;
using System.Collections.Generic;

namespace AffixZero.Core
{
    public enum DungeonDifficulty { Scout = 0, Veteran = 1, Torment = 2 }

    public sealed class DifficultyRule
    {
        public DungeonDifficulty Id { get; }
        public string Name { get; }
        public float EnemyHealthMultiplier { get; }
        public float EnemyDamageMultiplier { get; }
        public float RewardMultiplier { get; }
        public float DropMultiplier { get; }
        public float RarityProgressionBonus { get; }
        public int EliteStride { get; }

        internal DifficultyRule(DungeonDifficulty id, string name, float health, float damage,
            float reward, float drop, float rarityProgressionBonus, int eliteStride)
        {
            Id = id; Name = name; EnemyHealthMultiplier = health; EnemyDamageMultiplier = damage;
            RewardMultiplier = reward; DropMultiplier = drop; RarityProgressionBonus = rarityProgressionBonus;
            EliteStride = eliteStride;
        }
    }

    public static class DifficultyTuning
    {
        private static readonly IReadOnlyList<DifficultyRule> rules = Array.AsReadOnly(new[]
        {
            new DifficultyRule(DungeonDifficulty.Scout, "Scout", 1f, 1f, 1f, 1f, 0f, 8),
            new DifficultyRule(DungeonDifficulty.Veteran, "Veteran", 1.45f, 1.25f, 1.2f, 1.25f, .12f, 6),
            new DifficultyRule(DungeonDifficulty.Torment, "Torment", 2.05f, 1.6f, 1.5f, 1.6f, .28f, 4)
        });

        public static IReadOnlyList<DifficultyRule> Rules => rules;

        public static DifficultyRule Get(DungeonDifficulty difficulty)
        {
            if (!Enum.IsDefined(typeof(DungeonDifficulty), difficulty))
                throw new ArgumentOutOfRangeException(nameof(difficulty));
            return rules[(int)difficulty];
        }

        public static int ScaleEnemyHealth(int baseValue, DungeonDifficulty difficulty) => Scale(baseValue, Get(difficulty).EnemyHealthMultiplier);
        public static int ScaleEnemyDamage(int baseValue, DungeonDifficulty difficulty) => Scale(baseValue, Get(difficulty).EnemyDamageMultiplier);
        public static int ScaleReward(int baseValue, DungeonDifficulty difficulty) => Scale(baseValue, Get(difficulty).RewardMultiplier);

        private static int Scale(int value, float multiplier)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            return Math.Max(1, (int)Math.Round(value * multiplier, MidpointRounding.AwayFromZero));
        }
    }
}
