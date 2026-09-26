using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AffixZero.Core
{
    public enum TalentId { Fury, Precision, Keystone }

    public sealed class WeaponItem
    {
        public const int MaxEnhancementRank = 3;
        public string Id { get; }
        public string Name { get; }
        public int FlatDamage { get; }
        public int AffixDamage { get; }
        public string AffixName { get; }
        public string IconResource { get; }
        public string Rarity { get; }
        public int EnhancementRank { get; }
        public int EnhancementDamage => EnhancementRank * 2;
        public int DamageBonus => FlatDamage + AffixDamage + EnhancementDamage;

        public WeaponItem(string id, string name, int flatDamage, int affixDamage,
            string affixName, string iconResource, string rarity, int enhancementRank = 0)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(iconResource) || string.IsNullOrWhiteSpace(rarity))
                throw new ArgumentException("Item identity, name, icon and rarity are required.");
            if (enhancementRank < 0 || enhancementRank > MaxEnhancementRank)
                throw new ArgumentOutOfRangeException(nameof(enhancementRank));
            // Reserve base attack, all talents and all enhancement ranks before intake.
            if (flatDamage < 0 || affixDamage < 0 || (long)flatDamage + affixDamage > int.MaxValue - 46)
                throw new ArgumentOutOfRangeException(nameof(flatDamage));
            if (affixDamage > 0 && string.IsNullOrWhiteSpace(affixName))
                throw new ArgumentException("A damage affix requires a name.", nameof(affixName));
            Id = id;
            Name = name;
            FlatDamage = flatDamage;
            AffixDamage = affixDamage;
            AffixName = affixName ?? string.Empty;
            IconResource = iconResource;
            Rarity = rarity;
            EnhancementRank = enhancementRank;
        }
    }

    // In-memory run state only. The presentation layer owns its lifetime across scenes.
    public sealed class HeroProgression
    {
        public const int InventoryCapacity = 24;
        public const int BaseDamage = 24;
        private const string FirstDropId = "loot:ember-steel:first";
        private readonly List<WeaponItem> inventory = new List<WeaponItem>();
        private readonly HashSet<string> killTokens = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> issuedItemIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly ReadOnlyCollection<WeaponItem> inventoryView;
        private bool firstDropWaiting;

        public IReadOnlyList<WeaponItem> Inventory => inventoryView;
        public WeaponItem EquippedWeapon { get; private set; }
        public WeaponItem PendingLoot { get; private set; }
        public int UnspentPoints { get; private set; }
        public int TotalExperience { get; private set; }
        // Available run gold after purchases, not lifetime gross earnings.
        public int TotalGold { get; private set; }
        public int FuryRank { get; private set; }
        public int PrecisionRank { get; private set; }
        public int KeystoneRank { get; private set; }
        public int SpentPoints => FuryRank + PrecisionRank + KeystoneRank;
        public int TalentDamage => FuryRank * 3 + PrecisionRank * 4 + KeystoneRank * 6;
        public int TotalDamage => BaseDamage + EquippedWeapon.DamageBonus + TalentDamage;
        public int EquippedEnhancementCost => EquippedWeapon.EnhancementRank >= WeaponItem.MaxEnhancementRank
            ? 0 : (EquippedWeapon.EnhancementRank + 1) * 8;
        public bool CanEnhanceEquipped => EquippedEnhancementCost > 0 && TotalGold >= EquippedEnhancementCost;

        public HeroProgression()
        {
            inventoryView = inventory.AsReadOnly();
            EquippedWeapon = new WeaponItem("equipped:starting-sword", "훈련용 검", 6, 0,
                string.Empty, "AffixGenerated/AttackIcon", "Common");
            issuedItemIds.Add(EquippedWeapon.Id);
        }

        public bool TryRegisterKill(string uniqueToken)
        {
            if (string.IsNullOrWhiteSpace(uniqueToken) || killTokens.Contains(uniqueToken) ||
                (long)UnspentPoints + SpentPoints >= int.MaxValue ||
                TotalExperience > int.MaxValue - 25 || TotalGold > int.MaxValue - 8) return false;
            bool firstKill = killTokens.Count == 0;
            if (firstKill)
            {
                firstDropWaiting = true;
                OfferFirstDrop();
            }
            killTokens.Add(uniqueToken);
            UnspentPoints++;
            TotalExperience += 25;
            TotalGold += 8;
            return true;
        }

        // Explicit intake for subsequent authored drops; never overwrite an uncollected drop.
        public bool TryCreatePendingLoot(WeaponItem item)
        {
            if (item == null || PendingLoot != null || issuedItemIds.Contains(item.Id) || item.Id == FirstDropId)
                return false;
            PendingLoot = item;
            issuedItemIds.Add(item.Id);
            return true;
        }

        public bool PickUp()
        {
            if (PendingLoot == null || inventory.Count >= InventoryCapacity) return false;
            inventory.Add(PendingLoot);
            PendingLoot = null;
            OfferFirstDrop();
            return true;
        }

        private void OfferFirstDrop()
        {
            if (!firstDropWaiting || PendingLoot != null) return;
            PendingLoot = new WeaponItem(FirstDropId, "잿불 강철검", 12, 4,
                "잿불", "AffixGenerated/EmberSword", "Rare");
            issuedItemIds.Add(PendingLoot.Id);
            firstDropWaiting = false;
        }

        public bool Equip(int index)
        {
            if (!ValidIndex(index)) return false;
            WeaponItem previous = EquippedWeapon;
            EquippedWeapon = inventory[index];
            inventory[index] = previous;
            return true;
        }

        public bool TryEnhanceEquipped()
        {
            if (!CanEnhanceEquipped) return false;
            WeaponItem current = EquippedWeapon;
            var enhanced = new WeaponItem(current.Id, current.Name, current.FlatDamage, current.AffixDamage,
                current.AffixName, current.IconResource, current.Rarity, current.EnhancementRank + 1);
            int cost = EquippedEnhancementCost;
            TotalGold -= cost;
            EquippedWeapon = enhanced;
            return true;
        }

        // Explicit discard has no currency reward. Pending loot is retained until picked up.
        public bool Discard(int index)
        {
            if (!ValidIndex(index)) return false;
            inventory.RemoveAt(index);
            return true;
        }

        public int? CompareDamage(int index) => ValidIndex(index)
            ? (int?)(inventory[index].DamageBonus - EquippedWeapon.DamageBonus) : null;

        public bool TrySpendPoint(TalentId talent)
        {
            if (UnspentPoints <= 0) return false;
            switch (talent)
            {
                case TalentId.Fury:
                    if (FuryRank >= 2) return false;
                    FuryRank++;
                    break;
                case TalentId.Precision:
                    if (FuryRank < 2 || PrecisionRank >= 1) return false;
                    PrecisionRank++;
                    break;
                case TalentId.Keystone:
                    if (PrecisionRank < 1 || KeystoneRank >= 1) return false;
                    KeystoneRank++;
                    break;
                default: return false;
            }
            UnspentPoints--;
            return true;
        }

        public void ResetTalents()
        {
            UnspentPoints += SpentPoints;
            FuryRank = PrecisionRank = KeystoneRank = 0;
        }

        private bool ValidIndex(int index) => index >= 0 && index < inventory.Count;
    }
}
