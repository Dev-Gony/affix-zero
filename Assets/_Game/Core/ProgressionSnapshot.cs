using System;

namespace AffixZero.Core
{
    // Payload v2 expands equipment and talents; v1 migrates with an explicit legacy point credit.
    // Payload v4 records same-currency salvage provenance without invalidating v1-v3 profiles.
    // Payload v5 adds per-base sockets and a conserved stackable rune inventory.
    // Optional inline items require tags because Unity JsonUtility may produce empty objects.
    [Serializable]
    public sealed class ProgressionSnapshot
    {
        public int schemaVersion = 5;
        public int totalExperience, totalGold, totalSalvageGold, unspentPoints, furyRank, precisionRank, keystoneRank;
        public int vitalityRank, cleaveRank, hasteRank, legacyPointCredit;
        public int selectedDifficulty, dungeonClears;
        public bool hasArmor, hasRelic, hasHelmet, hasGloves, hasBoots, hasRing, hasAmulet;
        public WeaponSnapshot equippedArmor, equippedRelic, equippedHelmet, equippedGloves, equippedBoots, equippedRing, equippedAmulet;
        public bool firstDropWaiting;
        public bool hasPendingLoot;
        public WeaponSnapshot equippedWeapon;
        public WeaponSnapshot[] inventory;
        public WeaponSnapshot pendingLoot;
        public string[] killTokens, issuedItemIds;
        public int totalRunesIssued;
        public RuneStackSnapshot[] runeStacks;
    }

    [Serializable]
    public sealed class WeaponSnapshot
    {
        public string id, name, affixName, iconResource, rarity;
        public int flatDamage, affixDamage, enhancementRank;
        public int equipmentSlot, weaponStyle, flatDefense, flatHealth, cooldownReductionPercent;
        public ItemOptionSnapshot[] options;
        public int socketCapacity, openedSocketCount;
        public SocketSnapshot[] sockets;
    }

    [Serializable]
    public sealed class ItemOptionSnapshot
    {
        public int stat;
        public string name;
        public float value;
    }

    [Serializable]
    public sealed class SocketSnapshot
    {
        public int index;
        public string runeId;
    }

    [Serializable]
    public sealed class RuneStackSnapshot
    {
        public string runeId;
        public int count;
    }
}
