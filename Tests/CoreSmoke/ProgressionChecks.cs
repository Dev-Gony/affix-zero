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
        maximum.TryCreatePendingLoot(Item("maximum", int.MaxValue - 40));
        maximum.PickUp();
        maximum.Equip(0);
        check(maximum.TotalDamage == int.MaxValue - 16 && maximum.CompareDamage(0) < 0,
            "largest accepted weapon remains arithmetically safe");
    }
}
