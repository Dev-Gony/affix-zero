using System;

namespace AffixZero.Core
{
    // Version 1 persists only the two authored weapon icon resources used by this MVP.
    // Adding another resource or changing reward accounting requires an explicit save-policy review.
    [Serializable]
    public sealed class ProgressionSnapshot
    {
        public int schemaVersion = 1;
        public int totalExperience, totalGold, unspentPoints, furyRank, precisionRank, keystoneRank;
        public bool firstDropWaiting;
        public bool hasPendingLoot;
        public WeaponSnapshot equippedWeapon;
        public WeaponSnapshot[] inventory;
        public WeaponSnapshot pendingLoot;
        public string[] killTokens, issuedItemIds;
    }

    [Serializable]
    public sealed class WeaponSnapshot
    {
        public string id, name, affixName, iconResource, rarity;
        public int flatDamage, affixDamage, enhancementRank;
    }
}
