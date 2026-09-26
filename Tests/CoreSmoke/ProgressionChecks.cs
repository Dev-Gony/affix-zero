using System;
using System.Collections.Generic;
using AffixZero.Core;

internal static class ProgressionChecks
{
    private static WeaponItem Item(string id, int damage = 1) =>
        new WeaponItem(id, "Test sword", damage, 0, "", "Test/Sword", "Common");

    public static void Run(Action<bool, string> check, Action<Action, string> throws)
    {
        var hero = new HeroProgression();
        check(hero.TotalDamage == 30 && hero.Inventory.Count == 0 && hero.PendingLoot == null &&
            hero.UnspentPoints == 0, "progression starts at real combat attack 30 with empty bag and no points");
        check(!hero.TrySpendPoint(TalentId.Fury) && !hero.TryRegisterKill(null) &&
            !hero.TryRegisterKill("  "), "no unearned point spending or invalid kill tokens");
        check(hero.TryRegisterKill("encounter-1:enemy-1") && hero.UnspentPoints == 1 &&
            hero.PendingLoot.DamageBonus == 16 && hero.Inventory.Count == 0,
            "first kill offers guaranteed affixed sword but does not auto pick up");
        WeaponItem firstDrop = hero.PendingLoot;
        check(firstDrop.FlatDamage == 12 && firstDrop.AffixDamage == 4 &&
            firstDrop.Name == "잿불 강철검" && firstDrop.IconResource == "AffixGenerated/EmberSword",
            "first drop retains authored flat and affix identity");
        check(!hero.TryRegisterKill("encounter-1:enemy-1") && hero.UnspentPoints == 1 &&
            ReferenceEquals(firstDrop, hero.PendingLoot), "duplicate kill cannot mint points or replace pending loot");
        check(hero.TotalExperience == 25 && hero.TotalGold == 8,
            "duplicate kill leaves lifetime experience and gold unchanged");
        check(hero.TryRegisterKill("encounter-2:enemy-1") && hero.UnspentPoints == 2 &&
            ReferenceEquals(firstDrop, hero.PendingLoot), "another kill grants point while previous drop remains pending");
        check(hero.TotalExperience == 50 && hero.TotalGold == 16,
            "distinct encounters accumulate experience and gold in the same run");
        check(hero.PickUp() && !hero.PickUp() && hero.Inventory.Count == 1 && hero.PendingLoot == null,
            "pickup transfers one item exactly once");
        check(hero.CompareDamage(0) == 10 && hero.TotalDamage == 30,
            "comparison predicts damage increase without changing equipment");
        check(hero.Equip(0) && hero.TotalDamage == 40 && hero.Inventory.Count == 1 &&
            hero.Inventory[0].DamageBonus == 6, "equip swaps old sword into same bag slot");
        check(hero.CompareDamage(0) == -10 && hero.Equip(0) && hero.TotalDamage == 30 &&
            hero.Equip(0) && hero.TotalDamage == 40, "repeated re-equip has no stacking or item duplication");
        check(!hero.Equip(-1) && !hero.Equip(1) && !hero.Discard(int.MaxValue) &&
            hero.CompareDamage(-1) == null && hero.CompareDamage(1) == null && hero.TotalDamage == 40,
            "invalid inventory indices leave equipment and bag untouched");
        check(!hero.TryCreatePendingLoot(firstDrop) && !hero.TryCreatePendingLoot(hero.Inventory[0]) &&
            !hero.TryCreatePendingLoot(null), "owned identity and null cannot become another drop");
        bool readOnly = false;
        try { ((IList<WeaponItem>)hero.Inventory).Clear(); }
        catch (NotSupportedException) { readOnly = true; }
        check(readOnly && hero.Inventory.Count == 1, "inventory view cannot mutate internal collection");

        check(!hero.TrySpendPoint(TalentId.Precision) && !hero.TrySpendPoint(TalentId.Keystone) &&
            !hero.TrySpendPoint((TalentId)99) && hero.UnspentPoints == 2,
            "prerequisite and invalid talent failures do not spend points");
        check(hero.TrySpendPoint(TalentId.Fury) && hero.TotalDamage == 43 &&
            !hero.TrySpendPoint(TalentId.Precision) && hero.UnspentPoints == 1,
            "first Fury rank adds three but does not unlock Precision");
        check(hero.TrySpendPoint(TalentId.Fury) && hero.TotalDamage == 46 && hero.FuryRank == 2,
            "second Fury rank reaches cap and six total attack");
        hero.TryRegisterKill("encounter-3:enemy-1");
        check(!hero.TrySpendPoint(TalentId.Fury) && hero.UnspentPoints == 1 &&
            hero.TrySpendPoint(TalentId.Precision) && hero.TotalDamage == 50,
            "capped Fury does not consume point and unlocked Precision adds four");
        hero.TryRegisterKill("encounter-4:enemy-1");
        check(!hero.TrySpendPoint(TalentId.Precision) && hero.TrySpendPoint(TalentId.Keystone) &&
            hero.TotalDamage == 56 && hero.SpentPoints == 4 && hero.UnspentPoints == 0,
            "Keystone completes one branch for sixteen total talent damage");
        hero.TryRegisterKill("encounter-5:enemy-1");
        check(!hero.TrySpendPoint(TalentId.Keystone) && hero.UnspentPoints == 1 && hero.PendingLoot == null,
            "maxed branch preserves surplus point and later kills do not duplicate first sword");
        hero.ResetTalents();
        hero.ResetTalents();
        check(hero.UnspentPoints == 5 && hero.SpentPoints == 0 && hero.TotalDamage == 40 &&
            !hero.TrySpendPoint(TalentId.Precision), "reset refunds exactly spent points once and restores prerequisites");
        check(hero.TotalExperience == 125 && hero.TotalGold == 40,
            "talent refunds leave earned experience and gold unchanged");
        check(hero.TrySpendPoint(TalentId.Fury) && hero.UnspentPoints == 4 && hero.TotalDamage == 43,
            "refunded points can be allocated again");

        var full = new HeroProgression();
        for (int i = 0; i < HeroProgression.InventoryCapacity; i++)
            if (!full.TryCreatePendingLoot(Item("bag:" + i)) || !full.PickUp())
                throw new Exception("Could not prepare full inventory.");
        check(full.TryRegisterKill("full-bag-kill") && !full.PickUp() &&
            full.PendingLoot.DamageBonus == 16 && full.Inventory.Count == 24 && full.UnspentPoints == 1,
            "full bag retains first-kill drop and point without overflow");
        WeaponItem pending = full.PendingLoot;
        check(!full.TryCreatePendingLoot(Item("replacement")) && ReferenceEquals(full.PendingLoot, pending),
            "new drop cannot overwrite pending loot");
        check(full.Equip(0) && full.Inventory.Count == 24 && ReferenceEquals(full.PendingLoot, pending),
            "equipment swap succeeds even when bag is full");
        check(full.Discard(1) && full.PickUp() && full.Inventory.Count == 24 && full.PendingLoot == null &&
            ReferenceEquals(full.Inventory[23], pending), "explicit discard frees one slot for retained drop");
        check(!full.TryCreatePendingLoot(Item("bag:1")), "discarded item identity cannot be reissued");
        var queued = new HeroProgression();
        queued.TryCreatePendingLoot(Item("prior-drop"));
        check(queued.TryRegisterKill("first") && queued.UnspentPoints == 1 &&
            queued.PendingLoot.Id == "prior-drop" && queued.PickUp() && queued.PendingLoot.DamageBonus == 16,
            "first-kill guarantee waits behind an existing drop without rejecting the earned point");
        throws(() => Item("negative", -1), "negative item damage rejected");
        throws(() => Item("overflow", int.MaxValue), "item damage reserves safe room for base and talent bonuses");
        bool badIdRejected = false;
        try { Item(" "); } catch (ArgumentException) { badIdRejected = true; }
        check(badIdRejected, "empty item identity rejected at construction");
        var maximum = new HeroProgression();
        maximum.TryCreatePendingLoot(Item("maximum", int.MaxValue - 46));
        maximum.PickUp();
        maximum.Equip(0);
        check(maximum.TotalDamage == int.MaxValue - 22 && maximum.CompareDamage(0) < 0,
            "largest accepted weapon remains arithmetically safe");

        var forge = new HeroProgression();
        WeaponItem starter = forge.EquippedWeapon;
        check(starter.EnhancementRank == 0 && starter.EnhancementDamage == 0 &&
            forge.EquippedEnhancementCost == 8 && !forge.CanEnhanceEquipped &&
            !forge.TryEnhanceEquipped() && ReferenceEquals(starter, forge.EquippedWeapon) && forge.TotalGold == 0,
            "unaffordable first enhancement leaves weapon and balance unchanged");
        forge.TryRegisterKill("forge:1");
        forge.PickUp();
        forge.Equip(0);
        WeaponItem ember = forge.EquippedWeapon;
        check(forge.CanEnhanceEquipped && forge.TryEnhanceEquipped() && forge.TotalDamage == 42 &&
            forge.TotalGold == 0 && forge.TotalExperience == 25 && forge.UnspentPoints == 1 &&
            forge.EquippedWeapon.EnhancementRank == 1 && forge.EquippedWeapon.EnhancementDamage == 2,
            "eight earned gold buys first ember enhancement for attack 42 without consuming XP or talent points");
        WeaponItem enhanced = forge.EquippedWeapon;
        check(!ReferenceEquals(ember, enhanced) && ember.EnhancementRank == 0 &&
            enhanced.Id == ember.Id && enhanced.Name == ember.Name && enhanced.IconResource == ember.IconResource &&
            enhanced.FlatDamage == ember.FlatDamage && enhanced.AffixDamage == ember.AffixDamage &&
            enhanced.AffixName == ember.AffixName && enhanced.Rarity == ember.Rarity,
            "enhancement creates immutable replacement preserving item identity and authored properties");
        check(!forge.TryRegisterKill("forge:1") && forge.TotalGold == 0 &&
            !forge.TryEnhanceEquipped() && ReferenceEquals(enhanced, forge.EquippedWeapon),
            "duplicate kill cannot replenish spent gold or fund another enhancement");
        check(forge.TrySpendPoint(TalentId.Fury) && forge.TotalDamage == 45,
            "first enhancement and first Fury rank combine additively for attack 45");
        check(forge.CompareDamage(0) == -12 && forge.Equip(0) && forge.TotalDamage == 33 &&
            forge.Inventory[0].EnhancementRank == 1 && forge.Equip(0) && forge.TotalDamage == 45 &&
            ReferenceEquals(enhanced, forge.EquippedWeapon), "bag swaps retain rank and comparison includes enhancement");
        forge.TryRegisterKill("forge:2");
        check(forge.EquippedEnhancementCost == 16 && !forge.CanEnhanceEquipped &&
            !forge.TryEnhanceEquipped() && forge.TotalGold == 8 && ReferenceEquals(enhanced, forge.EquippedWeapon),
            "rank two requires full sixteen gold and does not partially charge");
        forge.TryRegisterKill("forge:3");
        check(forge.TryEnhanceEquipped() && forge.TotalGold == 0 && forge.EquippedWeapon.EnhancementRank == 2 &&
            forge.TotalDamage == 47 && forge.EquippedEnhancementCost == 24,
            "second enhancement consumes sixteen gold and exposes final rank cost");
        forge.TryRegisterKill("forge:4");
        forge.TryRegisterKill("forge:5");
        forge.TryRegisterKill("forge:6");
        check(forge.TryEnhanceEquipped() && forge.TotalGold == 0 && forge.TotalDamage == 49 &&
            forge.EquippedWeapon.EnhancementRank == 3 && forge.EquippedWeapon.EnhancementDamage == 6,
            "third enhancement consumes twenty-four gold and reaches six bonus damage");
        forge.TryRegisterKill("forge:7");
        WeaponItem capped = forge.EquippedWeapon;
        check(forge.EquippedEnhancementCost == 0 && !forge.CanEnhanceEquipped && !forge.TryEnhanceEquipped() &&
            forge.TotalGold == 8 && ReferenceEquals(capped, forge.EquippedWeapon),
            "maximum enhancement rejects even with gold and preserves all state");
        forge.ResetTalents();
        check(forge.TotalDamage == 46 && forge.TotalGold == 8 && forge.TotalExperience == 175 &&
            forge.EquippedWeapon.EnhancementRank == 3, "talent refund does not refund forge costs or remove enhancement");
        throws(() => new WeaponItem("bad-rank", "Sword", 1, 0, "", "Icon", "Common", -1),
            "negative enhancement rank rejected");
        throws(() => new WeaponItem("bad-rank", "Sword", 1, 0, "", "Icon", "Common", 4),
            "enhancement above maximum rejected");
        throws(() => Item("unsafe-upgrade", int.MaxValue - 45),
            "weapon without room for all future enhancements rejected at intake");
        for (int i = 0; i < 6; i++) maximum.TryRegisterKill("boundary:" + i);
        maximum.TrySpendPoint(TalentId.Fury);
        maximum.TrySpendPoint(TalentId.Fury);
        maximum.TrySpendPoint(TalentId.Precision);
        maximum.TrySpendPoint(TalentId.Keystone);
        check(maximum.TryEnhanceEquipped() && maximum.TryEnhanceEquipped() && maximum.TryEnhanceEquipped() &&
            maximum.TotalDamage == int.MaxValue && maximum.TotalGold == 0,
            "largest accepted weapon reaches exact int maximum with full talents and enhancements safely");
    }
}
