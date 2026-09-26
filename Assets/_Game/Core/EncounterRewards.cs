using System;
using System.Collections.Generic;

namespace AffixZero.Core
{
    // Transient rewards for one encounter. Presentation lifetime cannot mint another reward.
    public sealed class EncounterRewards
    {
        public string EncounterId { get; }
        public int Experience { get; private set; }
        public int Gold { get; private set; }
        public int CollectionCount => collected.Count;
        private readonly HashSet<(int actor, long death)> collected = new HashSet<(int, long)>();

        public EncounterRewards(string encounterId)
        {
            if (string.IsNullOrWhiteSpace(encounterId)) throw new ArgumentException("Encounter ID required.", nameof(encounterId));
            EncounterId = encounterId;
        }

        public bool TryCollect(string encounterId, int actorId, long deathId, int experience, int gold)
        {
            if (actorId <= 0) throw new ArgumentOutOfRangeException(nameof(actorId));
            if (deathId <= 0) throw new ArgumentOutOfRangeException(nameof(deathId));
            if (experience < 0 || gold < 0) throw new ArgumentOutOfRangeException(nameof(experience));
            var key = (actorId, deathId);
            if (encounterId != EncounterId || collected.Contains(key)) return false;
            int nextExperience = checked(Experience + experience);
            int nextGold = checked(Gold + gold);
            collected.Add(key);
            Experience = nextExperience;
            Gold = nextGold;
            return true;
        }
    }
}
