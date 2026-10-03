using System;
using System.Collections.Generic;
using AffixZero.Core;

internal static class SocketChecks
{
    public static void Run(Action<bool, string> check)
    {
        var expected = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            {"dagger",2},{"longsword",3},{"axe",3},{"magic_sword",4},{"divine_sword",3},
            {"leather_hat",1},{"iron_helm",2},{"mithril_helm",2},{"dragon_helm",3},
            {"cloth",2},{"leather_armor",3},{"plate_armor",3},{"dragonscale",4},
            {"cloth_gloves",0},{"leather_gloves",1},{"battle_gloves",1},{"dragon_gloves",2},
            {"sandals",0},{"leather_boots",1},{"swift_boots",1},{"gale_boots",2},
            {"copper_ring",0},{"silver_ring",1},{"gold_ring",1},{"diamond_ring",2},
            {"bone_necklace",0},{"crystal_necklace",1},{"ruby_necklace",1},{"dragon_tear",2}
        };
        bool exact = SocketCatalog.Profiles.Count == 29;
        foreach (SocketProfile profile in SocketCatalog.Profiles)
            exact &= expected.TryGetValue(profile.BaseId, out int capacity) && capacity == profile.Capacity;
        check(exact && SocketCatalog.CapacityFor("AffixUIVisual/Items/magic_sword", EquipmentSlot.Weapon) == 4 &&
            SocketCatalog.CapacityFor("AffixUIVisual/Items/magic_sword", EquipmentSlot.Armor) == 0,
            "all 29 base capacities use one slot-checked registry and exceed the rejected universal cap");

        var runeIds = new HashSet<string>(StringComparer.Ordinal);
        bool runeCatalogValid = SocketCatalog.Runes.Count == 10;
        foreach (RuneDefinition rune in SocketCatalog.Runes)
            runeCatalogValid &= runeIds.Add(rune.Id) && rune.Value > 0 && rune.Slots.Count > 0;
        check(runeCatalogValid, "ten original runes map to existing stats and explicit compatible slots");

        bool generatedBounds = true, sawFourCapacity = false, zeroStayedZero = true;
        foreach (KeyValuePair<string, int> pair in expected)
        {
            WeaponItem item = LootGenerator.GenerateGuaranteedBase("socket-base:" + pair.Key, pair.Key, "rare", 20,
                3100 + pair.Value + pair.Key.Length);
            generatedBounds &= item.SocketCapacity == pair.Value && item.OpenedSocketCount >= (pair.Value > 0 ? 1 : 0) &&
                item.OpenedSocketCount <= item.SocketCapacity && item.SocketedRunes.Count == item.OpenedSocketCount;
            sawFourCapacity |= item.SocketCapacity == 4;
            zeroStayedZero &= pair.Value != 0 || item.OpenedSocketCount == 0;
        }
        check(generatedBounds && sawFourCapacity && zeroStayedZero,
            "generated loot separates immutable capacity from bounded seeded opened sockets");

        var hero = new HeroProgression();
        var staff = new WeaponItem("socket:staff", "Socket staff", 10, 0, "", "AffixUIVisual/Items/magic_sword",
            "rare", 0, EquipmentSlot.Weapon, WeaponStyle.Staff, options: null,
            socketCapacity: 4, openedSocketCount: 2);
        check(hero.TryCreatePendingLoot(staff) && hero.PickUp() &&
            hero.TryGrantRune("ember", 2) && hero.TryGrantRune("bastion") && hero.TryGrantRune("keen"),
            "socket fixture owns one item and conserved stackable runes");
        check(!hero.SocketInventoryItem(0, 0, "bastion") && hero.RuneCount("bastion") == 1 &&
            !hero.SocketInventoryItem(0, 2, "ember"),
            "incompatible and unopened socket insertion fail without consuming a rune");
        check(hero.SocketInventoryItem(0, 0, "ember") && hero.RuneCount("ember") == 1 &&
            hero.Inventory[0].FilledSocketCount == 1 && hero.Inventory[0].DamageBonus == 13,
            "insertion consumes one stack and contributes its existing stat");
        check(!hero.SocketInventoryItem(0, 0, "keen") && hero.RuneCount("keen") == 1 &&
            hero.Inventory[0].SocketedRunes[0] == "ember",
            "filled socket replacement cannot destroy or consume either rune");
        check(hero.Equip(0) && hero.TotalDamage == 37 && hero.EquippedWeapon.SocketedRunes[0] == "ember",
            "equipping a socketed item applies rune stats exactly once");
        hero.TryRegisterKill("socket:gold");
        check(hero.TryEnhanceEquipped() && hero.EquippedWeapon.EnhancementRank == 1 &&
            hero.EquippedWeapon.SocketedRunes[0] == "ember" && hero.TotalDamage == 39,
            "enhancement preserves socket ordering and combines enhancement with rune power");
        check(hero.UnsocketEquippedItem(EquipmentSlot.Weapon, 0) && hero.RuneCount("ember") == 2 &&
            hero.EquippedWeapon.FilledSocketCount == 0 && hero.TotalDamage == 36,
            "removal returns the rune intact and immediately removes its combat contribution");
        check(hero.SocketEquippedItem(EquipmentSlot.Weapon, 0, "ember") &&
            hero.SocketEquippedItem(EquipmentSlot.Weapon, 1, "keen") && hero.TotalDamage == 39 &&
            hero.CriticalChance == 9,
            "two compatible runes aggregate independent existing stats");

        var replacement = new WeaponItem("socket:replacement", "Replacement sword", 1, 0, "",
            "AffixGenerated/AttackIcon", "normal");
        int replacementIndex = hero.Inventory.Count;
        check(hero.TryCreatePendingLoot(replacement) && hero.PickUp() && hero.Equip(replacementIndex),
            "socketed equipment can be swapped back into the bounded bag");
        int socketedIndex = replacementIndex;
        int salvageGold = hero.GetSalvageValue(socketedIndex);
        check(hero.Salvage(socketedIndex) && hero.LastReturnedRuneCount == 2 &&
            hero.RuneCount("ember") == 2 && hero.RuneCount("keen") == 1 &&
            hero.LastSalvageGold == salvageGold && hero.TotalRunesIssued == 4,
            "salvage ReturnAll moves both runes and gold atomically without changing issued total");

        var rollback = new HeroProgression();
        var dagger = new WeaponItem("socket:rollback", "Rollback dagger", 2, 0, "",
            "AffixUIVisual/Items/dagger", "normal", socketCapacity: 2, openedSocketCount: 1);
        rollback.TryCreatePendingLoot(dagger); rollback.PickUp();
        check(rollback.TryGrantRune("ember", SocketCatalog.MaxRuneStack) &&
            rollback.SocketInventoryItem(0, 0, "ember") && rollback.TryGrantRune("ember") &&
            rollback.RuneCount("ember") == SocketCatalog.MaxRuneStack,
            "rollback fixture reaches a legal full stack with one additional socketed rune");
        WeaponItem before = rollback.Inventory[0];
        check(!rollback.UnsocketInventoryItem(0, 0) && ReferenceEquals(before, rollback.Inventory[0]) &&
            rollback.Inventory[0].SocketedRunes[0] == "ember" &&
            rollback.RuneCount("ember") == SocketCatalog.MaxRuneStack,
            "full-stack removal rejects the entire transaction without rune loss or item replacement");
    }
}
