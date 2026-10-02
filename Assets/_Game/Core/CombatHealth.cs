using System;
using System.Collections.Generic;

namespace AffixZero.Core
{
    // One instance per actor per encounter. No rendering or save dependencies.
    public sealed class CombatHealth
    {
        public int Current { get; private set; }
        public int Maximum { get; private set; }
        public int Defense { get; private set; }
        public bool IsDead => Current == 0;
        public int DeathCount { get; private set; }
        private readonly Dictionary<int, long> lastHits = new Dictionary<int, long>();

        public CombatHealth(int maximum, int defense)
        {
            if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense));
            Maximum = maximum;
            Current = maximum;
            Defense = defense;
        }

        // Equipment changes never heal or revive; hit deduplication survives the change.
        public void Reconfigure(int maximum, int defense)
        {
            if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense));
            Maximum = maximum;
            Defense = defense;
            Current = Math.Min(Current, maximum);
        }

        public int Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (IsDead || amount == 0) return 0;
            int restored = Math.Min(amount, Maximum - Current);
            Current += restored;
            return restored;
        }

        public HitReceipt Receive(int attackerId, long attackId, int rawDamage)
        {
            if (attackerId <= 0) throw new ArgumentOutOfRangeException(nameof(attackerId));
            if (attackId <= 0) throw new ArgumentOutOfRangeException(nameof(attackId));
            if (rawDamage <= 0) throw new ArgumentOutOfRangeException(nameof(rawDamage));
            if (IsDead || (lastHits.TryGetValue(attackerId, out long last) && attackId <= last))
                return default;
            lastHits[attackerId] = attackId;
            int applied = Math.Min(Current, Math.Max(1, rawDamage - Defense));
            Current -= applied;
            if (IsDead) DeathCount++;
            return new HitReceipt(true, applied, IsDead);
        }
    }

    public readonly struct HitReceipt
    {
        public bool Accepted { get; }
        public int Damage { get; }
        public bool Killed { get; }
        public HitReceipt(bool accepted, int damage, bool killed)
        { Accepted = accepted; Damage = damage; Killed = killed; }
    }
}
