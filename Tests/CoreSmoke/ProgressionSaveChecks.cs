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
        var difficult = new HeroProgression(); difficult.TrySetDifficulty(DungeonDifficulty.Torment);
        for (int i = 0; i < 24; i++) difficult.TryRegisterKill("torment:" + i);
        difficult.RegisterDungeonClear();
        var difficultReloaded = HeroProgression.RestoreSnapshot(difficult.CaptureSnapshot());
        check(difficultReloaded.SelectedDifficulty == DungeonDifficulty.Torment && difficultReloaded.DungeonClears == 1 &&
            difficultReloaded.TotalExperience == 912 && difficultReloaded.TotalGold == 288 && difficultReloaded.UnspentPoints == 3,
            "difficulty, clear sequence and scaled rewards persist across save reload");
        var source = new HeroProgression();
        for (int i = 0; i < 6; i++) source.TryRegisterKill("save-kill:" + i);
        // Recreate a valid legacy one-point-per-kill balance, then migrate before spending.
        var legacyBalance = AsLegacy(source.CaptureSnapshot()); legacyBalance.unspentPoints = 6;
        source = HeroProgression.RestoreSnapshot(legacyBalance);
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
        full.TryCreatePendingLoot(Item("full:pending"));
        var loadedFull = HeroProgression.RestoreSnapshot(full.CaptureSnapshot());
        check(loadedFull.Inventory.Count == 24 && loadedFull.PendingLoot.Id == "full:pending" && !loadedFull.PickUp() &&
            loadedFull.Discard(0) && loadedFull.PickUp() && loadedFull.Inventory.Count == 24,
            "full-bag snapshot retains generated pending loot until space is freed");
        var waiting = new HeroProgression();
        waiting.TryCreatePendingLoot(Item("earlier-drop")); waiting.TryRegisterKill("waiting:kill");
        var loadedWaiting = HeroProgression.RestoreSnapshot(waiting.CaptureSnapshot());
        check(loadedWaiting.PendingLoot.Id == "earlier-drop" && loadedWaiting.PickUp() &&
            loadedWaiting.PendingLoot == null && loadedWaiting.TotalExperience == 25,
            "current saves retain explicit pending loot without inventing a deferred drop");

        var previousV3 = source.CaptureSnapshot(); previousV3.schemaVersion = 3; previousV3.totalSalvageGold = 0;
        var migratedV3 = HeroProgression.RestoreSnapshot(previousV3); var normalizedV5 = migratedV3.CaptureSnapshot();
        check(normalizedV5.schemaVersion == 5 && migratedV3.TotalSalvageGold == 0 &&
            migratedV3.TotalGold == source.TotalGold && migratedV3.TotalExperience == source.TotalExperience &&
            migratedV3.EquippedWeapon.Id == source.EquippedWeapon.Id && migratedV3.Inventory.Count == source.Inventory.Count &&
            normalizedV5.killTokens.Length == previousV3.killTokens.Length && normalizedV5.issuedItemIds.Length == previousV3.issuedItemIds.Length &&
            normalizedV5.totalRunesIssued == 0 && normalizedV5.runeStacks.Length == 0,
            "schema-v3 save migrates to v5 with zero salvage/rune provenance and unchanged progression ledgers");

        var socketSource = new HeroProgression();
        var socketItem = new WeaponItem("save:socket-staff", "Saved socket staff", 9, 0, "",
            "AffixUIVisual/Items/magic_sword", "rare", 0, EquipmentSlot.Weapon, WeaponStyle.Staff,
            socketCapacity: 4, openedSocketCount: 2);
        socketSource.TryCreatePendingLoot(socketItem); socketSource.PickUp();
        socketSource.TryGrantRune("ember", 2); socketSource.TryGrantRune("keen"); socketSource.TryGrantRune("bastion");
        socketSource.SocketInventoryItem(0, 0, "ember"); socketSource.SocketInventoryItem(0, 1, "keen");
        var socketRoundTrip = HeroProgression.RestoreSnapshot(JsonSerializer.Deserialize<ProgressionSnapshot>(
            JsonSerializer.Serialize(socketSource.CaptureSnapshot(), options), options));
        check(socketRoundTrip.CaptureSnapshot().schemaVersion == 5 && socketRoundTrip.Inventory[0].SocketCapacity == 4 &&
            socketRoundTrip.Inventory[0].OpenedSocketCount == 2 && socketRoundTrip.Inventory[0].SocketedRunes[0] == "ember" &&
            socketRoundTrip.Inventory[0].SocketedRunes[1] == "keen" && socketRoundTrip.RuneCount("ember") == 1 &&
            socketRoundTrip.RuneCount("bastion") == 1 && socketRoundTrip.TotalRunesIssued == 4,
            "v5 JSON roundtrip preserves capacity, indexed sockets, stack counts and issued-rune conservation");
        Reject(check, socketSource, s => s.inventory[0].socketCapacity = 3, "saved capacity mismatch rejected");
        Reject(check, socketSource, s => s.inventory[0].openedSocketCount = 5, "opened sockets beyond base capacity rejected");
        Reject(check, socketSource, s => s.inventory[0].sockets = new[] {
            new SocketSnapshot { index = 0, runeId = "ember" },
            new SocketSnapshot { index = 0, runeId = "keen" } }, "duplicate saved socket index rejected");
        Reject(check, socketSource, s => s.inventory[0].sockets[0].runeId = "unknown", "unknown saved socket rune rejected");
        Reject(check, socketSource, s => s.inventory[0].sockets[0].runeId = "bastion", "slot-incompatible saved rune rejected");
        Reject(check, socketSource, s => s.inventory[0].sockets = null, "missing v5 socket array rejected");
        Reject(check, socketSource, s => s.runeStacks = null, "missing v5 rune stack array rejected");
        Reject(check, socketSource, s => s.runeStacks = new[] { new RuneStackSnapshot { runeId = "unknown", count = 1 } },
            "unknown saved rune stack rejected");
        Reject(check, socketSource, s => s.totalRunesIssued++, "rune creation through a forged issued total rejected");
        Reject(check, socketSource, s => s.totalRunesIssued = -1, "negative issued rune total rejected");
        Reject(check, socketSource, s => s.schemaVersion = 4, "pre-v5 snapshot cannot smuggle socket state");

        Reject(check, source, s => s.schemaVersion = 6, "unknown snapshot schema rejected", true);
        Reject(check, source, s => s.totalGold = -1, "negative save balance rejected");
        Reject(check, source, s => s.totalSalvageGold = -1, "negative salvage provenance rejected");
        Reject(check, source, s => s.unspentPoints = int.MaxValue, "overflowing or unearned point balance rejected");
        Reject(check, source, s => s.selectedDifficulty = 99, "unknown saved difficulty rejected");
        Reject(check, source, s => s.dungeonClears = -1, "negative saved dungeon clears rejected");
        Reject(check, source, s => s.totalExperience = s.killTokens.Length * 45 + 1, "XP beyond option-bonus and difficulty reward range rejected");
        Reject(check, source, s => s.totalGold = s.killTokens.Length * 17 + 1, "gold beyond option-bonus and difficulty reward range rejected");
        Reject(check, source, s => s.furyRank = 1, "broken Precision prerequisite rejected");
        Reject(check, source, s => s.keystoneRank = 6, "talent rank above cap rejected");
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
        Reject(check, source, s => s.equippedWeapon.enhancementRank = 21, "invalid saved enhancement rank rejected");
        Reject(check, source, s => s.equippedWeapon.flatDamage = int.MaxValue, "overflowing saved weapon rejected");
        Reject(check, source, s => s.equippedWeapon.affixName = "", "missing saved affix identity rejected");
        Reject(check, source, s => s.equippedWeapon.iconResource = "../../Unknown", "unreviewed saved resource path rejected");
        Reject(check, source, s => s.firstDropWaiting = true, "already issued first drop cannot be reserved again");
        Reject(check, waiting, s => s.pendingLoot = null, "tagged pending loot cannot disappear from its payload");
        Reject(check, source, s => s.legacyPointCredit = -1, "negative legacy credit rejected");
        Reject(check, source, s => s.legacyPointCredit = 7, "credit beyond recorded legacy rewards rejected");
        Reject(check, source, s => s.equippedWeapon.equipmentSlot = 1, "armor cannot occupy weapon slot");
        Reject(check, source, s => s.equippedWeapon.weaponStyle = 99, "unknown saved weapon style rejected");
        Reject(check, source, s => { s.hasArmor = true; s.equippedArmor = s.equippedWeapon; }, "duplicate armor identity rejected");
        Reject(check, source, s => { s.hasRelic = true; s.equippedRelic = null; }, "tagged missing relic rejected");
        var salvageSource = new HeroProgression();
        salvageSource.TryRegisterKill("salvage-save:kill");
        salvageSource.TryCreatePendingLoot(new WeaponItem("salvage-save:item", "Saved salvage", 2, 0, "",
            "AffixGenerated/AttackIcon", "rare"));
        salvageSource.PickUp(); int salvageValue = salvageSource.GetSalvageValue(0); salvageSource.Salvage(0);
        var salvageLoaded = HeroProgression.RestoreSnapshot(salvageSource.CaptureSnapshot());
        check(salvageLoaded.TotalSalvageGold == salvageValue && salvageLoaded.TotalGold == 8 + salvageValue &&
            salvageLoaded.Inventory.Count == 0, "v5 roundtrip preserves bounded salvage gold provenance");
        Reject(check, salvageSource, s => s.totalGold = s.killTokens.Length * 17 + s.totalSalvageGold + 1,
            "gold beyond kill rewards and salvage provenance rejected");
        Reject(check, salvageSource, s => s.totalSalvageGold = s.issuedItemIds.Length * 80 + 1,
            "implausible salvage provenance rejected");
        var emptyInline = new HeroProgression().CaptureSnapshot();
        emptyInline.equippedArmor = new WeaponSnapshot(); emptyInline.equippedRelic = new WeaponSnapshot();
        emptyInline.pendingLoot = new WeaponSnapshot();
        var inlineRestored = HeroProgression.RestoreSnapshot(emptyInline);
        check(inlineRestored.EquippedArmor == null && inlineRestored.EquippedRelic == null && inlineRestored.PendingLoot == null,
            "explicit optional tags ignore Unity empty inline objects");
        MigrationAndExpandedRoundTrip(check, options);
        bool nullRejected = false;
        try { HeroProgression.RestoreSnapshot(null); } catch (ArgumentNullException) { nullRejected = true; }
        check(nullRejected && source.TotalDamage == 54 && source.TotalGold == 24 && source.PendingLoot.Id == "pending:clear",
            "null and invalid restore attempts leave the active profile untouched");
    }

    private static void MigrationAndExpandedRoundTrip(Action<bool, string> check, JsonSerializerOptions options)
    {
        var legacy = new HeroProgression();
        for (int i = 0; i < 57; i++) legacy.TryRegisterKill("legacy:" + i);
        var legacySeed=AsLegacy(legacy.CaptureSnapshot());legacySeed.unspentPoints=57;
        legacy=HeroProgression.RestoreSnapshot(legacySeed);
        legacy.PickUp(); legacy.Equip(0); legacy.TryEnhanceEquipped(); legacy.TryEnhanceEquipped(); legacy.TryEnhanceEquipped();
        var old = AsLegacy(legacy.CaptureSnapshot()); old.unspentPoints = 53;
        old.furyRank = 2; old.precisionRank = 1; old.keystoneRank = 1;
        // Anonymous DTO reproduces the original wire fields, with all v2 additions absent.
        Func<WeaponSnapshot, object> oldItem = w => w == null ? null : new
        { w.id, w.name, w.affixName, w.iconResource, w.rarity, w.flatDamage, w.affixDamage, w.enhancementRank };
        string legacyJson = JsonSerializer.Serialize(new
        {
            old.schemaVersion, old.totalExperience, old.totalGold, old.unspentPoints,
            old.furyRank, old.precisionRank, old.keystoneRank, old.firstDropWaiting, old.hasPendingLoot,
            equippedWeapon = oldItem(old.equippedWeapon), inventory = new[] { oldItem(old.inventory[0]) },
            pendingLoot = oldItem(old.pendingLoot), old.killTokens, old.issuedItemIds
        });
        var migrated = HeroProgression.RestoreSnapshot(JsonSerializer.Deserialize<ProgressionSnapshot>(legacyJson, options));
        check(migrated.UnspentPoints == 53 && migrated.SpentPoints == 4 && migrated.LegacyPointCredit == 52 && migrated.Level == 6 &&
            migrated.TotalGold == 408 && migrated.TotalExperience == 1425 && migrated.TotalDamage == 62 &&
            migrated.EquippedWeapon.EnhancementRank == 3 && migrated.EquippedArmor == null && migrated.EquippedRelic == null,
            "actual v1 wire fields migrate all 57 earned points, spent ranks, enhanced gear and balances without invented secondary equipment");
        check(migrated.CaptureSnapshot().schemaVersion == 5 && !migrated.TryRegisterKill("legacy:0"),
            "migration normalizes payload to v5 and retains reward idempotency");
        var pendingLegacy = new HeroProgression(); pendingLegacy.TryRegisterKill("legacy-pending:first");
        var pendingWire = AsLegacy(pendingLegacy.CaptureSnapshot()); pendingWire.unspentPoints = 1;
        var migratedPending = HeroProgression.RestoreSnapshot(pendingWire);
        check(migratedPending.PendingLoot.Id == "loot:ember-steel:first" && migratedPending.PickUp() &&
            !migratedPending.PickUp() && migratedPending.UnspentPoints == 1 && migratedPending.LegacyPointCredit == 1,
            "legacy pending first drop and earned point survive migration exactly once");
        var invalidLegacy = AsLegacy(legacy.CaptureSnapshot());
        invalidLegacy.unspentPoints = 52; invalidLegacy.furyRank = 3; invalidLegacy.precisionRank = 1; invalidLegacy.keystoneRank = 1;
        bool rejectedLegacy = false;
        try { HeroProgression.RestoreSnapshot(invalidLegacy); } catch (ArgumentException) { rejectedLegacy = true; }
        check(rejectedLegacy, "legacy format cannot smuggle ranks that exceeded its original cap");
        migrated.TryRegisterKill("post-migration:0"); migrated.TryRegisterKill("post-migration:1");
        check(migrated.UnspentPoints == 53, "migration credit does not grant a point on every subsequent kill");
        migrated.TryRegisterKill("post-migration:2");
        check(migrated.UnspentPoints == 54 && migrated.Level == 7 && migrated.LegacyPointCredit == 52,
            "first new level after migration grants exactly one additional point");
        migrated.TrySpendPoint(TalentId.Vitality); migrated.TrySpendPoint(TalentId.Cleave); migrated.TrySpendPoint(TalentId.Haste);
        var armor = new WeaponItem("save:armor", "수호 갑옷", 0, 0, "", "AffixGenerated/GearArmor", "Rare", 4,
            EquipmentSlot.Armor, flatDefense: 3, flatHealth: 30);
        var relic = new WeaponItem("save:relic", "가속 유물", 4, 2, "잿불", "AffixGenerated/GearRelic", "Rare", 5,
            EquipmentSlot.Relic, cooldownReductionPercent: 12);
        var staff = new WeaponItem("save:staff", "사원 지팡이", 15, 3, "날카로움", "AffixGenerated/GearStaff", "Rare", 6,
            EquipmentSlot.Weapon, WeaponStyle.Staff);
        foreach (var item in new[] { armor, relic, staff })
        { int index = migrated.Inventory.Count; migrated.TryCreatePendingLoot(item); migrated.PickUp(); migrated.Equip(index); }
        var loaded = HeroProgression.RestoreSnapshot(JsonSerializer.Deserialize<ProgressionSnapshot>(
            JsonSerializer.Serialize(migrated.CaptureSnapshot(), options), options));
        check(loaded.EquippedArmor.Id == armor.Id && loaded.EquippedArmor.EnhancementRank == 4 && loaded.TotalDefense == 9 &&
            loaded.TotalMaxHp == 180 && loaded.EquippedRelic.Id == relic.Id && loaded.EquippedRelic.EnhancementRank == 5 &&
            loaded.EquippedWeapon.WeaponStyle == WeaponStyle.Staff && loaded.IsRanged && loaded.AttackReach == 4.5f,
            "current roundtrip preserves all slots, armor enhancement and ranged style parameters");
        check(loaded.VitalityRank == 1 && loaded.CleaveRank == 1 && loaded.HasteRank == 1 && loaded.LegacyPointCredit == 52 &&
            loaded.TotalDamage == migrated.TotalDamage && loaded.CooldownReductionPercent == 14 &&
            loaded.SplashRadius == migrated.SplashRadius && loaded.AttackSpeedMultiplier == migrated.AttackSpeedMultiplier,
            "current roundtrip preserves new build stats and legacy point credit exactly");
        loaded.ResetTalents(); loaded.ResetTalents();
        check(loaded.UnspentPoints == 58 && loaded.SpentPoints == 0 && loaded.TotalGold == migrated.TotalGold,
            "reset after migration refunds all old and new ranks without minting or deleting points");
        Reject(check, migrated, s => s.equippedArmor.equipmentSlot = 2, "relic tagged in armor slot rejected");
        Reject(check, migrated, s => s.equippedRelic.cooldownReductionPercent = 51, "invalid saved cooldown bonus rejected");
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

    private static ProgressionSnapshot AsLegacy(ProgressionSnapshot snapshot)
    {
        snapshot.schemaVersion=1;
        if(snapshot.killTokens.Length==0||Array.IndexOf(snapshot.issuedItemIds,"loot:ember-steel:first")>=0)return snapshot;
        if(snapshot.hasPendingLoot){snapshot.firstDropWaiting=true;return snapshot;}
        snapshot.hasPendingLoot=true;snapshot.firstDropWaiting=false;
        snapshot.pendingLoot=new WeaponSnapshot{id="loot:ember-steel:first",name="잿불 강철검",affixName="잿불",iconResource="AffixGenerated/EmberSword",rarity="Rare",flatDamage=12,affixDamage=4,equipmentSlot=0,weaponStyle=0,options=Array.Empty<ItemOptionSnapshot>()};
        var ids=new string[snapshot.issuedItemIds.Length+1];Array.Copy(snapshot.issuedItemIds,ids,snapshot.issuedItemIds.Length);ids[ids.Length-1]="loot:ember-steel:first";snapshot.issuedItemIds=ids;
        return snapshot;
    }
}
