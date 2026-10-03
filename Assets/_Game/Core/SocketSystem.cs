using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AffixZero.Core
{
    public sealed class SocketProfile
    {
        public string BaseId { get; }
        public EquipmentSlot Slot { get; }
        public int Capacity { get; }
        internal SocketProfile(string baseId, EquipmentSlot slot, int capacity)
        {
            BaseId = baseId; Slot = slot; Capacity = capacity;
        }
    }

    public sealed class RuneDefinition
    {
        private readonly HashSet<EquipmentSlot> slots;
        public string Id { get; }
        public string Name { get; }
        public string Mark { get; }
        public AffixStat Stat { get; }
        public float Value { get; }
        public IReadOnlyCollection<EquipmentSlot> Slots { get; }
        internal RuneDefinition(string id, string name, string mark, AffixStat stat, float value,
            params EquipmentSlot[] compatibleSlots)
        {
            Id = id; Name = name; Mark = mark; Stat = stat; Value = value;
            slots = new HashSet<EquipmentSlot>(compatibleSlots);
            Slots = new ReadOnlyCollection<EquipmentSlot>(new List<EquipmentSlot>(compatibleSlots));
        }
        public bool Supports(EquipmentSlot slot) => slots.Contains(slot);
    }

    public sealed class RuneStack
    {
        public RuneDefinition Rune { get; }
        public int Count { get; }
        internal RuneStack(RuneDefinition rune, int count) { Rune = rune; Count = count; }
    }

    public static class SocketCatalog
    {
        public const int MaxRuneStack = 999;
        private const string ItemIconPrefix = "AffixUIVisual/Items/";
        private static readonly ReadOnlyCollection<SocketProfile> profiles = Array.AsReadOnly(new[]
        {
            Profile("dagger",EquipmentSlot.Weapon,2), Profile("longsword",EquipmentSlot.Weapon,3),
            Profile("axe",EquipmentSlot.Weapon,3), Profile("magic_sword",EquipmentSlot.Weapon,4),
            Profile("divine_sword",EquipmentSlot.Weapon,3),
            Profile("leather_hat",EquipmentSlot.Helmet,1), Profile("iron_helm",EquipmentSlot.Helmet,2),
            Profile("mithril_helm",EquipmentSlot.Helmet,2), Profile("dragon_helm",EquipmentSlot.Helmet,3),
            Profile("cloth",EquipmentSlot.Armor,2), Profile("leather_armor",EquipmentSlot.Armor,3),
            Profile("plate_armor",EquipmentSlot.Armor,3), Profile("dragonscale",EquipmentSlot.Armor,4),
            Profile("cloth_gloves",EquipmentSlot.Gloves,0), Profile("leather_gloves",EquipmentSlot.Gloves,1),
            Profile("battle_gloves",EquipmentSlot.Gloves,1), Profile("dragon_gloves",EquipmentSlot.Gloves,2),
            Profile("sandals",EquipmentSlot.Boots,0), Profile("leather_boots",EquipmentSlot.Boots,1),
            Profile("swift_boots",EquipmentSlot.Boots,1), Profile("gale_boots",EquipmentSlot.Boots,2),
            Profile("copper_ring",EquipmentSlot.Ring,0), Profile("silver_ring",EquipmentSlot.Ring,1),
            Profile("gold_ring",EquipmentSlot.Ring,1), Profile("diamond_ring",EquipmentSlot.Ring,2),
            Profile("bone_necklace",EquipmentSlot.Amulet,0), Profile("crystal_necklace",EquipmentSlot.Amulet,1),
            Profile("ruby_necklace",EquipmentSlot.Amulet,1), Profile("dragon_tear",EquipmentSlot.Amulet,2)
        });
        private static readonly Dictionary<string, SocketProfile> profileById = BuildProfiles();
        private static readonly ReadOnlyCollection<RuneDefinition> runes = Array.AsReadOnly(new[]
        {
            Rune("ember","Ember Rune","◆",AffixStat.Attack,3,EquipmentSlot.Weapon,EquipmentSlot.Gloves,EquipmentSlot.Ring),
            Rune("bastion","Bastion Rune","⬢",AffixStat.Defense,3,EquipmentSlot.Helmet,EquipmentSlot.Armor,EquipmentSlot.Gloves,EquipmentSlot.Boots),
            Rune("vital","Vital Rune","✦",AffixStat.Health,20,EquipmentSlot.Helmet,EquipmentSlot.Armor,EquipmentSlot.Ring,EquipmentSlot.Amulet),
            Rune("aether","Aether Rune","◇",AffixStat.Mana,12,EquipmentSlot.Weapon,EquipmentSlot.Ring,EquipmentSlot.Amulet),
            Rune("gale","Gale Rune","➶",AffixStat.Speed,.04f,EquipmentSlot.Weapon,EquipmentSlot.Gloves,EquipmentSlot.Boots,EquipmentSlot.Amulet),
            Rune("keen","Keen Rune","✧",AffixStat.Critical,4,EquipmentSlot.Weapon,EquipmentSlot.Gloves,EquipmentSlot.Ring,EquipmentSlot.Amulet),
            Rune("blood","Blood Rune","◈",AffixStat.Vampirism,2,EquipmentSlot.Weapon,EquipmentSlot.Ring,EquipmentSlot.Amulet),
            Rune("scholar","Scholar Rune","✺",AffixStat.Experience,5,EquipmentSlot.Helmet,EquipmentSlot.Ring,EquipmentSlot.Amulet),
            Rune("fortune","Fortune Rune","❖",AffixStat.Gold,7,EquipmentSlot.Boots,EquipmentSlot.Ring,EquipmentSlot.Amulet),
            Rune("piercing","Piercing Rune","◬",AffixStat.Penetration,4,EquipmentSlot.Weapon,EquipmentSlot.Gloves,EquipmentSlot.Amulet)
        });
        private static readonly Dictionary<string, RuneDefinition> runeById = BuildRunes();
        public static IReadOnlyList<SocketProfile> Profiles => profiles;
        public static IReadOnlyList<RuneDefinition> Runes => runes;

        public static SocketProfile ProfileFor(string iconResource, EquipmentSlot slot)
        {
            if (string.IsNullOrEmpty(iconResource) || !iconResource.StartsWith(ItemIconPrefix, StringComparison.Ordinal)) return null;
            string id = iconResource.Substring(ItemIconPrefix.Length);
            return profileById.TryGetValue(id, out SocketProfile profile) && profile.Slot == slot ? profile : null;
        }
        public static int CapacityFor(string iconResource, EquipmentSlot slot)
        {
            SocketProfile profile = ProfileFor(iconResource, slot);
            return profile == null ? 0 : profile.Capacity;
        }
        public static RuneDefinition GetRune(string id) =>
            !string.IsNullOrEmpty(id) && runeById.TryGetValue(id, out RuneDefinition rune) ? rune : null;
        public static RuneDefinition RuneForSeed(int seed)
        {
            int index = seed == int.MinValue ? 0 : Math.Abs(seed) % runes.Count;
            return runes[index];
        }
        public static string DescribeValue(RuneDefinition rune)
        {
            if (rune == null) return "";
            return rune.Stat == AffixStat.Speed
                ? "+" + (rune.Value * 100f).ToString("0") + "% " + rune.Stat.ToString().ToUpperInvariant()
                : "+" + rune.Value.ToString("0.#") + " " + rune.Stat.ToString().ToUpperInvariant();
        }

        private static Dictionary<string, SocketProfile> BuildProfiles()
        {
            var result = new Dictionary<string, SocketProfile>(StringComparer.Ordinal);
            foreach (SocketProfile profile in profiles) result.Add(profile.BaseId, profile);
            return result;
        }
        private static Dictionary<string, RuneDefinition> BuildRunes()
        {
            var result = new Dictionary<string, RuneDefinition>(StringComparer.Ordinal);
            foreach (RuneDefinition rune in runes) result.Add(rune.Id, rune);
            return result;
        }
        private static SocketProfile Profile(string id, EquipmentSlot slot, int capacity) =>
            new SocketProfile(id, slot, capacity);
        private static RuneDefinition Rune(string id, string name, string mark, AffixStat stat, float value,
            params EquipmentSlot[] slots) => new RuneDefinition(id, name, mark, stat, value, slots);
    }
}
