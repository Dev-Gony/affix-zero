using System;
using System.Collections.Generic;
using AffixZero.Core;

internal static class ProgressionChecks
{
    private static WeaponItem Item(string id, int damage = 1) =>
        new WeaponItem(id, "Test sword", damage, 0, "", "AffixGenerated/AttackIcon", "Common");
    private static void Kills(HeroProgression hero, int count, string prefix)
    { for (int i = 0; i < count; i++) if (!hero.TryRegisterKill(prefix + i)) throw new Exception("Duplicate test kill."); }
    private static void EquipNew(HeroProgression hero, WeaponItem item)
    {
        int index = hero.Inventory.Count;
        if (!hero.TryCreatePendingLoot(item) || !hero.PickUp() || !hero.Equip(index)) throw new Exception("Equipment fixture failed.");
    }
    public static void Run(Action<bool, string> check, Action<Action, string> throws)
    {
        var hero = new HeroProgression();
        check(hero.TotalDamage == 30 && hero.TotalMaxHp == 120 && hero.TotalDefense == 2 && hero.Level == 1 &&
            hero.Inventory.Count == 0 && hero.PendingLoot == null && hero.UnspentPoints == 0 && hero.LegacyPointCredit == 0,
            "new profile retains baseline combat and earns no free talent points");
        check(hero.ActiveEvolutionCount == 0 && hero.AreaSkillName == "AREA" && hero.AreaTrajectory==AreaSkillTrajectory.Radial && hero.RecoverySkillName == "HEAL" &&
            hero.AreaSkillRadius == 3.2f && hero.AreaSkillMinimumTargets == 2 && hero.AreaSkillDamageMultiplier == 1.5f &&
            hero.RecoveryThresholdPercent == 45 && hero.RecoveryHealPercent == 20,
            "fresh profile keeps the original area and recovery behavior before milestone investment");
        check(hero.SelectedDifficulty==DungeonDifficulty.Scout&&hero.DungeonClears==0&&
            !hero.TrySetDifficulty((DungeonDifficulty)99)&&hero.TrySetDifficulty(DungeonDifficulty.Veteran),
            "new profile starts Scout and rejects invalid difficulty selections");
        hero.TrySetDifficulty(DungeonDifficulty.Scout);
        check(hero.EquippedArmor == null && hero.EquippedRelic == null && hero.AttackReach == 1.05f &&
            hero.AttackSpeedMultiplier == 1f && !hero.IsRanged && hero.SplashRadius == 0,
            "starter sword has actual melee parameters and secondary slots start empty");
        check(!hero.TrySpendPoint(TalentId.Fury) && !hero.TryRegisterKill(null) && !hero.TryRegisterKill("  "),
            "unearned spending and empty reward identities are rejected");
        check(hero.TryRegisterKill("first") && hero.UnspentPoints == 0 && hero.TotalExperience == 25 && hero.TotalGold == 8,
            "first kill grants XP and gold but no instant talent point");
        check(hero.PendingLoot == null, "normal rewards do not mint a fixed onboarding weapon");
        WeaponItem first = new WeaponItem("loot:first", "Authored test sword", 12, 4,
            "Flame", "AffixGenerated/EmberSword", "rare");
        check(hero.TryCreatePendingLoot(first), "generated loot enters the explicit pending pickup flow");
        check(first.DamageBonus == 16 && first.FlatDamage == 12 && first.AffixDamage == 4 &&
            first.Name == "Authored test sword" && first.IconResource == "AffixGenerated/EmberSword",
            "first drop preserves authored affix and exact damage");
        check(!hero.TryRegisterKill("first") && hero.TotalExperience == 25 && hero.TotalGold == 8 &&
            ReferenceEquals(first, hero.PendingLoot), "duplicate reward cannot change balances or pending identity");
        Kills(hero, 8, "level-one:");
        check(hero.TotalExperience == 225 && hero.Level == 1 && hero.UnspentPoints == 0 && ReferenceEquals(first, hero.PendingLoot),
            "nine kills remain below the first level boundary and retain pending loot");
        hero.TryRegisterKill("tenth");
        check(hero.TotalExperience == 250 && hero.Level == 2 && hero.UnspentPoints == 1 && hero.TotalGold == 80,
            "exactly ten kills earn the first point at 250 XP");
        check(hero.PickUp() && !hero.PickUp() && hero.Inventory.Count == 1 && hero.PendingLoot == null,
            "pickup transfers an identity exactly once");
        check(hero.CompareDamage(0) == 10 && hero.TotalDamage == 30, "comparison does not equip");
        check(hero.Equip(0) && hero.TotalDamage == 40 && hero.Inventory[0].DamageBonus == 6,
            "weapon replacement returns old equipment to the same bag slot");
        check(hero.CompareDamage(0) == -10 && hero.Equip(0) && hero.TotalDamage == 30 && hero.Equip(0) && hero.TotalDamage == 40,
            "repeated swap never stacks bonuses");
        check(!hero.Equip(-1) && !hero.Equip(1) && !hero.Discard(int.MaxValue) && hero.CompareDamage(-1) == null,
            "invalid indices leave bag and equipment untouched");
        check(!hero.TryCreatePendingLoot(first) && !hero.TryCreatePendingLoot(hero.Inventory[0]) && !hero.TryCreatePendingLoot(null),
            "owned identities cannot be reissued");
        bool readOnly = false;
        try { ((IList<WeaponItem>)hero.Inventory).Clear(); } catch (NotSupportedException) { readOnly = true; }
        check(readOnly && hero.Inventory.Count == 1, "public inventory view cannot mutate its backing collection");
        check(!hero.TrySpendPoint(TalentId.Precision) && !hero.TrySpendPoint(TalentId.Keystone) &&
            !hero.TrySpendPoint(TalentId.Cleave) && !hero.TrySpendPoint(TalentId.Haste) && !hero.TrySpendPoint((TalentId)99) &&
            hero.UnspentPoints == 1, "locked and invalid talent requests preserve the available point");
        check(hero.TrySpendPoint(TalentId.Fury) && hero.TotalDamage == 43 && !hero.CanSpendPoint(TalentId.Precision),
            "first Fury gives three attack and does not unlock Precision");
        Kills(hero, 10, "level-two:");
        check(hero.TrySpendPoint(TalentId.Fury) && hero.FuryRank == 2 && hero.TotalDamage == 46,
            "second earned Fury rank keeps original prerequisite compatibility");
        Kills(hero, 10, "level-three:");
        check(hero.CanSpendPoint(TalentId.Fury) && hero.CanSpendPoint(TalentId.Cleave) &&
            hero.TrySpendPoint(TalentId.Precision) && hero.TotalDamage == 50,
            "Fury continues beyond old cap while Precision and Cleave unlock");
        Kills(hero, 10, "level-four:");
        check(hero.TrySpendPoint(TalentId.Keystone) && hero.TotalDamage == 56 && hero.SpentPoints == 4,
            "original four-rank path preserves its sixteen-damage effect");
        Kills(hero, 10, "level-five:");
        check(hero.CanSpendPoint(TalentId.Keystone) && hero.CanSpendPoint(TalentId.Haste) && hero.PendingLoot == null,
            "later ranks remain available without duplicating the first drop");
        hero.ResetTalents(); hero.ResetTalents();
        check(hero.UnspentPoints == 5 && hero.SpentPoints == 0 && hero.TotalDamage == 40 &&
            hero.TotalExperience == 1250 && hero.TotalGold == 400 && !hero.CanSpendPoint(TalentId.Precision),
            "reset refunds each allocated point once and preserves earned XP and currency");
        check(hero.TrySpendPoint(TalentId.Vitality) && hero.TotalMaxHp == 130 && hero.TotalDamage == 40,
            "survivability is an independent real build choice");

        var full = new HeroProgression();
        for (int i = 0; i < 24; i++) { full.TryCreatePendingLoot(Item("bag:" + i)); full.PickUp(); }
        WeaponItem pending = Item("pending", 16);
        check(full.TryRegisterKill("full") && full.TryCreatePendingLoot(pending) && !full.PickUp() && full.Inventory.Count == 24 &&
            ReferenceEquals(full.PendingLoot, pending) && full.UnspentPoints == 0, "full bag preserves pending generated loot and normal level pacing");
        check(!full.TryCreatePendingLoot(Item("replacement")) && ReferenceEquals(pending, full.PendingLoot), "pending loot cannot be overwritten");
        check(full.Equip(0) && full.Inventory.Count == 24 && ReferenceEquals(pending, full.PendingLoot), "full-bag weapon swap needs no extra slot");
        check(full.Discard(1) && full.PickUp() && full.Inventory.Count == 24 && ReferenceEquals(full.Inventory[23], pending),
            "discard frees exactly one slot for preserved pending loot");
        check(!full.TryCreatePendingLoot(Item("bag:1")), "discarded identity remains issued");
        var queued = new HeroProgression(); queued.TryCreatePendingLoot(Item("prior"));
        check(queued.TryRegisterKill("first") && queued.PendingLoot.Id == "prior" && queued.PickUp() && queued.PendingLoot == null,
            "combat rewards never overwrite or secretly queue behind existing loot");

        var gear = new HeroProgression();
        var armor = new WeaponItem("armor", "갑옷", 0, 0, "", "AffixGenerated/GearArmor", "Rare", 0, EquipmentSlot.Armor,
            flatDefense: 3, flatHealth: 40);
        EquipNew(gear, armor);
        check(gear.EquippedArmor == armor && gear.Inventory.Count == 0 && gear.TotalDefense == 5 && gear.TotalMaxHp == 160 && gear.TotalDamage == 30,
            "empty armor slot removes bag entry and changes health and defense only");
        var relic = new WeaponItem("relic", "유물", 3, 2, "격노", "AffixGenerated/GearRelic", "Rare", 0, EquipmentSlot.Relic,
            cooldownReductionPercent: 10);
        EquipNew(gear, relic);
        check(gear.EquippedRelic == relic && gear.TotalDamage == 35 && Math.Abs(gear.AttackSpeedMultiplier - 1f / .9f) < .0001f,
            "relic damage and cooldown affect actual combat parameters");
        EquipNew(gear, new WeaponItem("axe", "도끼", 14, 0, "", "AffixGenerated/GearAxe", "Common", 0,
            EquipmentSlot.Weapon, WeaponStyle.Axe));
        check(gear.AttackReach == 1.3f && gear.SplashRadius == 1.1f && gear.SplashDamageFraction == .5f && !gear.IsRanged &&
            Math.Abs(gear.AttackSpeedMultiplier - .8f / .9f) < .0001f, "axe gains reach and area damage with slower base attack");
        EquipNew(gear, new WeaponItem("staff", "지팡이", 10, 0, "", "AffixGenerated/GearStaff", "Common", 0,
            EquipmentSlot.Weapon, WeaponStyle.Staff));
        check(gear.IsRanged && gear.AttackReach == 4.5f && gear.SplashRadius == 0 && Math.Abs(gear.AttackSpeedMultiplier - 1f) < .0001f,
            "staff uses ranged reach and its own attack timing");
        var oldArmor = gear.EquippedArmor;
        EquipNew(gear, new WeaponItem("armor-two", "방벽", 0, 0, "", "AffixGenerated/GearArmor", "Rare", 0,
            EquipmentSlot.Armor, flatDefense: 5, flatHealth: 20));
        check(gear.TotalDefense == 7 && gear.TotalMaxHp == 140 && gear.Inventory[2] == oldArmor && gear.EquippedWeapon.WeaponStyle == WeaponStyle.Staff,
            "replacing armor swaps only its matching slot and removes old bonuses");
        check(gear.GetEquipped((EquipmentSlot)99) == null && !gear.CanEnhance((EquipmentSlot)99) &&
            !gear.TryEnhance(EquipmentSlot.Relic), "invalid or unaffordable enhancement cannot mutate equipment");

        check(HeroProgression.EarnedTalentPointsForExperience(2500)==10&&
            HeroProgression.EarnedTalentPointsForExperience(10500)==30&&
            HeroProgression.EarnedTalentPointsForExperience(23500)==50&&
            HeroProgression.EarnedTalentPointsForExperience(25570)==52&&
            HeroProgression.EarnedTalentPointsForExperience(48500)==75,
            "talent curve keeps early choices quick and stretches late specialization across sessions");
        var talents = new HeroProgression(); Kills(talents, 2000, "talent:");
        int capSum = 0;
        foreach (var definition in HeroProgression.TalentDefinitions)
        {
            capSum += definition.MaxRank;
            for (int rank = 0; rank < definition.MaxRank; rank++)
                if (!talents.CanSpendPoint(definition.Id) || !talents.TrySpendPoint(definition.Id)) throw new Exception("Talent rank unexpectedly unavailable.");
            int before = talents.UnspentPoints;
            check(talents.GetTalentRank(definition.Id) == definition.MaxRank && !talents.TrySpendPoint(definition.Id) &&
                talents.UnspentPoints == before, "talent cap rejects spending without consuming points: " + definition.Id);
        }
        check(capSum == 75 && capSum == HeroProgression.TotalTalentCapacity && talents.SpentPoints == 75 && talents.UnspentPoints == 1,
            "six-node build has 75 allocated ranks and preserves surplus level points");
        check(talents.TotalDamage == 160 && talents.TotalMaxHp == 320 && Math.Abs(talents.SplashRadius - .8f) < .0001f &&
            talents.SplashDamageFraction == .25f && talents.AttackSpeedMultiplier == 1.25f,
            "full tree modifies damage, survivability, area and timing independently");
        check(talents.ActiveEvolutionCount == 12 && talents.CriticalChance == 15 && talents.Penetration == 12 &&
            talents.AreaSkillName == "ARC II" && talents.AreaTrajectory == AreaSkillTrajectory.Chain && talents.AreaSkillMaxTargets == 4 &&
            Math.Abs(talents.AreaSkillJumpRange-2.85f)<.0001f && talents.AreaSkillRadius == 4.1f && talents.AreaSkillMinimumTargets == 1 &&
            talents.AreaSkillDamageMultiplier == 1.8f && talents.AreaSkillArmingDelay == 1.8f &&
            talents.RecoverySkillName == "SURGE" && talents.RecoveryThresholdPercent == 55 &&
            talents.RecoveryHealPercent == 30 && talents.RecoveryArmingDelay == 6f,
            "all six talent branches unlock two real combat evolution milestones");
        EquipNew(talents,new WeaponItem("route:axe","Route axe",12,0,"","AffixGenerated/GearAxe","Rare",0,
            EquipmentSlot.Weapon,WeaponStyle.Axe));
        check(talents.AreaTrajectory==AreaSkillTrajectory.Quake&&talents.AreaSkillName=="QUAKE II"&&talents.AreaSkillMaxTargets==24,
            "equipping an axe selects the broad short-range QUAKE route without new save state");
        EquipNew(talents,new WeaponItem("route:staff","Route staff",12,0,"","AffixGenerated/GearStaff","Rare",0,
            EquipmentSlot.Weapon,WeaponStyle.Staff));
        check(talents.AreaTrajectory==AreaSkillTrajectory.Pierce&&talents.AreaSkillName=="LANCE II"&&talents.AreaSkillMaxTargets==5&&
            Math.Abs(talents.AreaSkillPierceReach-6.2f)<.0001f&&Math.Abs(talents.AreaSkillPierceWidth-.54f)<.0001f,
            "equipping a staff selects the long narrow LANCE route with bounded targets");
        EquipNew(talents,new WeaponItem("route:sword","Route sword",6,0,"","AffixGenerated/AttackIcon","Rare"));
        check(talents.AreaTrajectory==AreaSkillTrajectory.Chain&&talents.AreaSkillName=="ARC II",
            "equipping a sword returns to the focused ARC route immediately");
        talents.PickUp();
        EquipNew(talents, new WeaponItem("cooldown-relic", "유물", 0, 0, "", "AffixGenerated/GearRelic", "Rare", 0,
            EquipmentSlot.Relic, cooldownReductionPercent: 50));
        check(talents.CooldownReductionPercent == 50 && talents.AttackSpeedMultiplier == 2f, "combined cooldown reduction is capped safely at fifty percent");
        talents.ResetTalents(); talents.ResetTalents();
        check(talents.UnspentPoints == 76 && talents.TotalMaxHp == 120 && talents.SplashRadius == 0 && talents.TotalDamage == 30 &&
            talents.ActiveEvolutionCount == 0 && talents.CriticalChance == 5 && talents.Penetration == 0 &&
            talents.AreaSkillName == "AREA" && talents.AreaTrajectory==AreaSkillTrajectory.Radial && talents.RecoverySkillName == "HEAL",
            "reset clears all six rank effects while retaining equipped relic stats");

        var forge = new HeroProgression(); WeaponItem starter = forge.EquippedWeapon;
        check(forge.EquippedEnhancementCost == 8 && !forge.TryEnhanceEquipped() && ReferenceEquals(starter, forge.EquippedWeapon),
            "unaffordable enhancement leaves immutable item unchanged");
        forge.TryRegisterKill("forge:first");
        forge.TryCreatePendingLoot(new WeaponItem("forge:ember","Ember blade",12,4,"Flame","AffixGenerated/EmberSword","rare"));
        forge.PickUp(); forge.Equip(0); WeaponItem ember = forge.EquippedWeapon;
        check(forge.TryEnhanceEquipped() && forge.TotalDamage == 42 && forge.TotalGold == 0 && forge.UnspentPoints == 0,
            "first enhancement spends eight gold without changing point pacing");
        WeaponItem enhanced = forge.EquippedWeapon;
        check(!ReferenceEquals(ember, enhanced) && ember.EnhancementRank == 0 && enhanced.Id == ember.Id &&
            enhanced.AffixName == ember.AffixName && enhanced.AffixDamage == 4 && enhanced.IconResource == ember.IconResource,
            "immutable enhancement preserves identity, icon and affix");
        check(!forge.TryRegisterKill("forge:first") && !forge.TryEnhanceEquipped() && ReferenceEquals(enhanced, forge.EquippedWeapon),
            "duplicate reward cannot fund an enhancement");
        check(forge.CompareDamage(0) == -12 && forge.Equip(0) && forge.Inventory[0].EnhancementRank == 1 &&
            forge.Equip(0) && ReferenceEquals(enhanced, forge.EquippedWeapon), "swap and comparison retain enhancement ranks");
        Kills(forge, 1, "forge:second:");
        check(forge.EquippedEnhancementCost == 16 && !forge.TryEnhanceEquipped() && forge.TotalGold == 8,
            "insufficient balance never partially charges a rank");
        Kills(forge, 1, "forge:third:");
        check(forge.TryEnhanceEquipped() && forge.TotalGold == 0 && forge.EquippedEnhancementCost == 24,
            "second legacy enhancement cost remains sixteen");
        Kills(forge, 3, "forge:fourth:");
        check(forge.TryEnhanceEquipped() && forge.TotalDamage == 46 && forge.EquippedEnhancementCost == 40,
            "rank three now progresses to the escalating fourth rank");
        Kills(forge, 3000, "forge:fund:");
        int goldBefore = forge.TotalGold, costSum = 0, previousCost = 24;
        for (int rank = 3; rank < 20; rank++)
        {
            int cost = forge.EquippedEnhancementCost;
            if (cost <= previousCost || !forge.TryEnhanceEquipped()) throw new Exception("Enhancement cost/progression failed.");
            costSum += cost; previousCost = cost;
        }
        check(forge.EquippedWeapon.EnhancementRank == 20 && forge.TotalDamage == 80 && forge.TotalGold == goldBefore - costSum,
            "twenty ranks apply forty damage and charge every increasingly expensive purchase exactly once");
        WeaponItem capped = forge.EquippedWeapon;
        check(forge.EquippedEnhancementCost == 0 && !forge.TryEnhanceEquipped() && ReferenceEquals(capped, forge.EquippedWeapon),
            "rank twenty cap rejects further enhancement even when funded");
        forge.ResetTalents(); check(forge.TotalGold == goldBefore - costSum && forge.EquippedWeapon.EnhancementRank == 20,
            "talent reset cannot refund gold or remove item enhancements");
        Kills(gear, 10, "gear:fund:");
        check(gear.TryEnhance(EquipmentSlot.Armor) && gear.TotalDefense == 8 && gear.TotalMaxHp == 145 &&
            gear.EquippedArmor.EnhancementDamage == 0, "armor enhancement improves defense and health instead of weapon damage");
        check(gear.EquippedArmor.NextEnhancementDamage == 0 && gear.EquippedArmor.NextEnhancementDefense == 1 &&
            gear.EquippedArmor.NextEnhancementHealth == 5 && gear.EquippedWeapon.NextEnhancementDamage == 2 &&
            capped.NextEnhancementDamage == 0, "UI next-upgrade values match slot-specific rules and the cap");
        int beforeDamage = gear.TotalDamage;
        check(gear.TryEnhance(EquipmentSlot.Relic) && gear.TotalDamage == beforeDamage + 2 && gear.EquippedRelic.CooldownReductionPercent == 10,
            "relic enhancement preserves cooldown affix and adds real damage");

        throws(() => Item("negative", -1), "negative item damage rejected");
        throws(() => Item("overflow", int.MaxValue), "unsafe item base damage rejected");
        throws(() => new WeaponItem("bad", "Bad", 1, 0, "", "Icon", "Common", -1), "negative rank rejected");
        throws(() => new WeaponItem("bad", "Bad", 1, 0, "", "Icon", "Common", 21), "rank above twenty rejected");
        throws(() => new WeaponItem("bad", "Bad", 1, 0, "", "Icon", "Common", equipmentSlot: (EquipmentSlot)99), "invalid equipment enum rejected");
        throws(() => new WeaponItem("bad", "Bad", 1, 0, "", "Icon", "Common", weaponStyle: (WeaponStyle)99), "invalid weapon style rejected");
        throws(() => new WeaponItem("bad", "Bad", 0, 0, "", "Icon", "Common", flatHealth: -1), "negative health bonus rejected");
        throws(() => new WeaponItem("bad", "Bad", 0, 0, "", "Icon", "Common", cooldownReductionPercent: 51), "invalid cooldown bonus rejected");
        bool badId = false; try { Item(" "); } catch (ArgumentException) { badId = true; }
        check(badId, "blank item identity rejected");
        var maximum = new HeroProgression(); EquipNew(maximum, Item("maximum", int.MaxValue - 46));
        check(maximum.TotalDamage == int.MaxValue - 22 && maximum.CompareDamage(0) < 0, "largest legacy-accepted item remains usable");
        EquipNew(maximum, new WeaponItem("maximum-relic", "Overflow guard", 100, 0, "", "AffixGenerated/GearRelic", "Rare", 0, EquipmentSlot.Relic));
        check(maximum.TotalDamage == int.MaxValue, "new multi-slot damage saturates safely rather than overflowing legacy high-value items");

        var health = new CombatHealth(120, 0); health.Receive(1, 1, 20); health.Reconfigure(160, 3);
        check(health.Current == 100 && health.Maximum == 160 && health.Defense == 3 && !health.Receive(1, 1, 20).Accepted,
            "gear change preserves absolute HP and attack deduplication without healing");
        health.Reconfigure(80, 0); health.Reconfigure(160, 0);
        check(health.Current == 80, "lower-max clamp followed by re-equip cannot heal");
        health.Receive(1, 2, 100); health.Reconfigure(200, 5);
        check(health.IsDead && health.Current == 0 && health.DeathCount == 1, "gear changes cannot revive dead actors or reset death count");
        throws(() => health.Reconfigure(0, 0), "invalid maximum rejected before changing health");
        throws(() => health.Reconfigure(200, -1), "invalid defense rejected before changing health");
    }
}
