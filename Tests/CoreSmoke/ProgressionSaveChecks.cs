using System;
using System.Text.Json;
using AffixZero.Core;

internal static class ProgressionSaveChecks
{
    private static WeaponItem Item(string id) => new WeaponItem(id, "사원의 검", 12, 3,
        "날카로움", "AffixGenerated/EmberSword", "Rare");

    public static void Run(Action<bool, string> check)
    {
        var fresh = HeroProgression.RestoreSnapshot(new HeroProgression().CaptureSnapshot());
        check(fresh.TotalDamage == 30 && fresh.TotalGold == 0 && fresh.TotalExperience == 0 && fresh.PendingLoot == null,
            "fresh snapshot restores without inventing a first drop or rewards");
        var source = new HeroProgression();
        for (int i = 0; i < 6; i++) source.TryRegisterKill("save-kill:" + i);
        source.PickUp(); source.Equip(0);
        source.TryEnhanceEquipped(); source.TryEnhanceEquipped();
        source.TrySpendPoint(TalentId.Fury); source.TrySpendPoint(TalentId.Fury); source.TrySpendPoint(TalentId.Precision);
        source.TryCreatePendingLoot(Item("pending:clear"));
        var options = new JsonSerializerOptions { IncludeFields = true };
        // Exercises the public-field DTO wire shape; actual Unity JsonUtility is a separate runtime check.
        string json = JsonSerializer.Serialize(source.CaptureSnapshot(), options);
        var dto = JsonSerializer.Deserialize<ProgressionSnapshot>(json, options);
        var restored = HeroProgression.RestoreSnapshot(dto);
        check(restored.TotalExperience == 150 && restored.TotalGold == 24 && restored.UnspentPoints == 3 &&
            restored.FuryRank == 2 && restored.PrecisionRank == 1 && restored.KeystoneRank == 0 && restored.TotalDamage == 54,
            "public-field JSON roundtrip preserves earned balances, spent forge gold, talents and total attack");
        check(restored.EquippedWeapon.Id == source.EquippedWeapon.Id && restored.EquippedWeapon.Name == "잿불 강철검" &&
            restored.EquippedWeapon.FlatDamage == 12 && restored.EquippedWeapon.AffixDamage == 4 &&
            restored.EquippedWeapon.AffixName == "잿불" && restored.EquippedWeapon.EnhancementRank == 2 &&
            restored.EquippedWeapon.IconResource == "AffixGenerated/EmberSword" && restored.EquippedWeapon.Rarity == "Rare" &&
            restored.Inventory.Count == 1 && restored.PendingLoot.Id == "pending:clear",
            "roundtrip preserves weapon identity, affix, enhancement, bag and pending drop");
        check(!restored.TryRegisterKill("save-kill:0") && restored.TotalGold == 24 && restored.TotalExperience == 150 &&
            !restored.TryCreatePendingLoot(Item("pending:clear")), "restored ledgers reject already rewarded kills and items");
        check(restored.PickUp() && restored.Equip(0) && restored.Inventory[0].EnhancementRank == 2 &&
            restored.Equip(0) && restored.TotalDamage == 54, "loaded pending drop and enhanced equipment remain functional");
        dto.equippedWeapon.flatDamage = 1;
        dto.inventory[0].name = "Changed DTO";
        dto.pendingLoot.id = "Changed DTO";
        dto.killTokens[0] = "Changed DTO";
        check(restored.EquippedWeapon.FlatDamage == 12 && restored.Inventory[0].Name != "Changed DTO" &&
            !restored.TryRegisterKill("save-kill:0"), "mutating deserialized DTO does not alias restored profile");
        ProgressionSnapshot detached = source.CaptureSnapshot();
        detached.equippedWeapon.enhancementRank = 0;
        detached.inventory[0] = null;
        detached.issuedItemIds[0] = "Changed DTO";
        check(source.EquippedWeapon.EnhancementRank == 2 && source.Inventory[0] != null &&
            source.CaptureSnapshot().issuedItemIds[0] != "Changed DTO", "capture provides detached objects and ledger arrays");
        restored.Discard(1);
        var discarded = HeroProgression.RestoreSnapshot(restored.CaptureSnapshot());
        check(!discarded.TryCreatePendingLoot(Item("pending:clear")), "discarded item identity remains spent across restore");

        var full = new HeroProgression();
        for (int i = 0; i < 24; i++) { full.TryCreatePendingLoot(Item("full:" + i)); full.PickUp(); }
        full.TryRegisterKill("full:kill");
        var loadedFull = HeroProgression.RestoreSnapshot(full.CaptureSnapshot());
        check(loadedFull.Inventory.Count == 24 && loadedFull.PendingLoot.DamageBonus == 16 && !loadedFull.PickUp() &&
            loadedFull.Discard(0) && loadedFull.PickUp() && loadedFull.Inventory.Count == 24,
            "full-bag snapshot retains guaranteed pending loot until space is freed");
        var waiting = new HeroProgression();
        waiting.TryCreatePendingLoot(Item("earlier-drop")); waiting.TryRegisterKill("waiting:kill");
        var loadedWaiting = HeroProgression.RestoreSnapshot(waiting.CaptureSnapshot());
        check(loadedWaiting.PendingLoot.Id == "earlier-drop" && loadedWaiting.PickUp() &&
            loadedWaiting.PendingLoot.Id == "loot:ember-steel:first" && loadedWaiting.PickUp() &&
            loadedWaiting.PendingLoot == null && loadedWaiting.TotalExperience == 25,
            "deferred first drop survives restore and is offered exactly once after earlier pickup");

        Reject(check, source, s => s.schemaVersion = 2, "unknown snapshot schema rejected", true);
        Reject(check, source, s => s.totalGold = -1, "negative save balance rejected");
        Reject(check, source, s => s.unspentPoints = int.MaxValue, "overflowing or unearned point balance rejected");
        Reject(check, source, s => s.totalExperience++, "XP inconsistent with kill ledger rejected");
        Reject(check, source, s => s.totalGold = 56, "gold exceeding recorded earnings rejected");
        Reject(check, source, s => s.furyRank = 1, "broken Precision prerequisite rejected");
        Reject(check, source, s => s.keystoneRank = 2, "talent rank above cap rejected");
        Reject(check, source, s => s.inventory = new WeaponSnapshot[25], "oversized saved bag rejected");
        Reject(check, source, s => s.inventory = null, "missing inventory rejected");
        Reject(check, source, s => s.equippedWeapon = null, "missing equipped weapon rejected");
        Reject(check, source, s => s.inventory[0] = null, "null item within saved bag rejected");
        Reject(check, source, s => s.pendingLoot = s.equippedWeapon, "duplicate identity across equipment and pending rejected");
        Reject(check, source, s => s.issuedItemIds = new[] { "equipped:starting-sword", "loot:ember-steel:first" },
            "owned identity absent from issued ledger rejected");
        Reject(check, source, s => s.killTokens[0] = s.killTokens[1], "duplicate saved kill tokens rejected");
        Reject(check, source, s => s.killTokens[0] = " ", "blank saved kill token rejected");
        Reject(check, source, s => s.issuedItemIds[0] = s.issuedItemIds[1], "duplicate issued item IDs rejected");
        Reject(check, source, s => s.killTokens = null, "missing kill ledger rejected");
        Reject(check, source, s => s.equippedWeapon.enhancementRank = 4, "invalid saved enhancement rank rejected");
        Reject(check, source, s => s.equippedWeapon.flatDamage = int.MaxValue, "overflowing saved weapon rejected");
        Reject(check, source, s => s.equippedWeapon.affixName = "", "missing saved affix identity rejected");
        Reject(check, source, s => s.equippedWeapon.iconResource = "../../Unknown", "unreviewed saved resource path rejected");
        Reject(check, source, s => s.firstDropWaiting = true, "already issued first drop cannot be reserved again");
        Reject(check, waiting, s => s.pendingLoot = null, "deferred first drop cannot lose its preceding pending item");
        bool nullRejected = false;
        try { HeroProgression.RestoreSnapshot(null); } catch (ArgumentNullException) { nullRejected = true; }
        check(nullRejected && source.TotalDamage == 54 && source.TotalGold == 24 && source.PendingLoot.Id == "pending:clear",
            "null and invalid restore attempts leave the active profile untouched");
    }

    private static void Reject(Action<bool, string> check, HeroProgression source,
        Action<ProgressionSnapshot> corrupt, string name, bool unsupported = false)
    {
        ProgressionSnapshot snapshot = source.CaptureSnapshot();
        corrupt(snapshot);
        bool rejected = false;
        try { HeroProgression.RestoreSnapshot(snapshot); }
        catch (NotSupportedException) { rejected = unsupported; }
        catch (ArgumentException) { rejected = !unsupported; }
        check(rejected, name);
    }
}
