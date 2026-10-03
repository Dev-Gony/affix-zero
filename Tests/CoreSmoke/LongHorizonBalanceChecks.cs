using System;
using AffixZero.Core;

internal static class LongHorizonBalanceChecks
{
    private const int EquivalentSeconds = 3600;
    private const int KillsPerMinute = 38;
    private const int TotalKills = EquivalentSeconds * KillsPerMinute / 60;

    private sealed class Result
    {
        public string Route;
        public HeroProgression Hero;
        public int Drops, Upgrades, Salvaged, SalvageGold, MaximumInventory, WeaponStylesMask;
        public int PointsAt15Minutes, PointsAt30Minutes, FullTreeKill;
    }

    public static void Run(Action<bool, string> check)
    {
        Result arc = Simulate("ARC", WeaponStyle.Sword, 1100000);
        Result quake = Simulate("QUAKE", WeaponStyle.Axe, 2200000);
        Result lance = Simulate("LANCE", WeaponStyle.Staff, 3300000);

        ValidateCommon(arc, WeaponStyle.Sword, check);
        ValidateCommon(quake, WeaponStyle.Axe, check);
        ValidateCommon(lance, WeaponStyle.Staff, check);

        check(arc.Hero.AreaTrajectory == AreaSkillTrajectory.Chain && arc.Hero.AreaSkillMaxTargets == 4 &&
            Math.Abs(arc.Hero.AreaSkillJumpRange - 2.85f) < .0001f,
            "hour-equivalent natural sword build ends on four-target ARC II with 2.85m jumps");
        check(quake.Hero.AreaTrajectory == AreaSkillTrajectory.Quake && quake.Hero.AreaSkillMaxTargets == 24 &&
            quake.Hero.SplashRadius > arc.Hero.SplashRadius,
            "hour-equivalent natural axe build ends on broad QUAKE II with the axe splash tradeoff");
        check(lance.Hero.AreaTrajectory == AreaSkillTrajectory.Pierce && lance.Hero.AreaSkillMaxTargets == 5 &&
            Math.Abs(lance.Hero.AreaSkillPierceReach - 6.2f) < .0001f &&
            Math.Abs(lance.Hero.AreaSkillPierceWidth - .54f) < .0001f,
            "hour-equivalent natural staff build ends on five-target 6.2m by 1.08m LANCE II lane");

        Console.WriteLine("LONG_HORIZON model=deterministic-domain-equivalent realTimeSeconds=0 equivalentSeconds=" + EquivalentSeconds +
            " killsPerMinute=" + KillsPerMinute + " routes=" + Describe(arc) + ";" + Describe(quake) + ";" + Describe(lance));
    }

    private static Result Simulate(string route, WeaponStyle desiredWeapon, int seedBase)
    {
        var result = new Result { Route = route, Hero = new HeroProgression() };
        TalentId[] priorities = desiredWeapon == WeaponStyle.Sword
            ? new[] { TalentId.Fury, TalentId.Precision, TalentId.Keystone, TalentId.Haste, TalentId.Cleave, TalentId.Vitality }
            : desiredWeapon == WeaponStyle.Axe
                ? new[] { TalentId.Vitality, TalentId.Fury, TalentId.Cleave, TalentId.Precision, TalentId.Keystone, TalentId.Haste }
                : new[] { TalentId.Fury, TalentId.Precision, TalentId.Haste, TalentId.Keystone, TalentId.Vitality, TalentId.Cleave };

        for (int kill = 1; kill <= TotalKills; kill++)
        {
            if (!result.Hero.TryRegisterKill("hour:" + route + ":" + kill))
                throw new Exception("Long-horizon kill registration failed at " + route + " " + kill + ".");
            SpendAvailable(result.Hero, priorities);

            if (kill % 24 == 0)
            {
                result.Hero.RegisterDungeonClear();
                if (result.Hero.DungeonClears == 1) result.Hero.TrySetDifficulty(DungeonDifficulty.Veteran);
                else if (result.Hero.DungeonClears == 2) result.Hero.TrySetDifficulty(DungeonDifficulty.Torment);
            }

            DifficultyRule rule = DifficultyTuning.Get(result.Hero.SelectedDifficulty);
            bool elite = kill % rule.EliteStride == 0;
            int floor = Math.Max(1, result.Hero.Level + result.Hero.DungeonClears);
            WeaponItem item = LootGenerator.TryGenerate("hour:item:" + route + ":" + kill, floor, elite,
                seedBase + kill * 37, result.Hero.SelectedDifficulty);
            if (item != null)
            {
                result.Drops++;
                if (item.EquipmentSlot == EquipmentSlot.Weapon)
                    result.WeaponStylesMask |= 1 << (int)item.WeaponStyle;
                if (!result.Hero.TryCreatePendingLoot(item)) throw new Exception("Long-horizon pending loot failed.");
                if (result.Hero.Inventory.Count >= HeroProgression.InventoryCapacity) SalvageWorst(result);
                if (!result.Hero.PickUp()) throw new Exception("Long-horizon pickup failed.");
                result.MaximumInventory = Math.Max(result.MaximumInventory, result.Hero.Inventory.Count);
                int index = result.Hero.Inventory.Count - 1;
                WeaponItem equipped = result.Hero.GetEquipped(item.EquipmentSlot);
                bool eligibleWeapon = item.EquipmentSlot != EquipmentSlot.Weapon || item.WeaponStyle == desiredWeapon;
                if (eligibleWeapon && Score(item) > Score(equipped))
                {
                    if (!result.Hero.Equip(index)) throw new Exception("Long-horizon equip failed.");
                    result.Upgrades++;
                }
                while (result.Hero.Inventory.Count >= 20) SalvageWorst(result);
            }

            if (kill % 120 == 0 && result.Hero.CanEnhanceEquipped) result.Hero.TryEnhanceEquipped();
            if (kill == KillsPerMinute * 15) result.PointsAt15Minutes = result.Hero.SpentPoints;
            if (kill == KillsPerMinute * 30) result.PointsAt30Minutes = result.Hero.SpentPoints;
            if (result.FullTreeKill == 0 && result.Hero.SpentPoints == HeroProgression.TotalTalentCapacity)
                result.FullTreeKill = kill;
        }
        return result;
    }

    private static void SpendAvailable(HeroProgression hero, TalentId[] priorities)
    {
        bool spent;
        do
        {
            spent = false;
            foreach (TalentId talent in priorities)
                if (hero.TrySpendPoint(talent)) { spent = true; break; }
        } while (spent);
    }

    private static void SalvageWorst(Result result)
    {
        int worst = 0;
        for (int i = 1; i < result.Hero.Inventory.Count; i++)
            if (Score(result.Hero.Inventory[i]) < Score(result.Hero.Inventory[worst])) worst = i;
        int value = result.Hero.GetSalvageValue(worst);
        if (value <= 0 || !result.Hero.Salvage(worst)) throw new Exception("Long-horizon salvage failed.");
        result.Salvaged++; result.SalvageGold += value;
    }

    private static int Score(WeaponItem item) => item == null ? 0 : item.DamageBonus * 4 + item.DefenseBonus * 12 +
        item.HealthBonus / 4 + item.VampirismPercent * 4 + item.CriticalChance * 2 + item.Penetration + item.CooldownReductionPercent;

    private static void ValidateCommon(Result result, WeaponStyle expectedWeapon, Action<bool, string> check)
    {
        HeroProgression restored = HeroProgression.RestoreSnapshot(result.Hero.CaptureSnapshot());
        check(result.Hero.DungeonClears == TotalKills / 24 && result.Hero.SelectedDifficulty == DungeonDifficulty.Torment,
            result.Route + " hour-equivalent model reaches and retains production Torment progression");
        check(result.PointsAt15Minutes > 0 && result.PointsAt15Minutes < HeroProgression.TotalTalentCapacity &&
            result.PointsAt30Minutes < HeroProgression.TotalTalentCapacity && result.FullTreeKill > KillsPerMinute * 30 &&
            result.FullTreeKill < TotalKills,
            result.Route + " has meaningful partial builds at 15/30 minutes and completes later within the hour");
        check(result.Hero.SpentPoints == HeroProgression.TotalTalentCapacity && result.Hero.UnspentPoints == 0,
            result.Route + " stops at 75 useful ranks without accumulating dead points");
        check(result.Drops > 100 && result.Upgrades > 0 && result.Salvaged > 0 && result.SalvageGold == result.Hero.TotalSalvageGold &&
            result.MaximumInventory <= 20 && result.Hero.Inventory.Count < 20,
            result.Route + " converts inferior natural loot into bounded existing gold while keeping the bag operable");
        check(result.WeaponStylesMask == 7 && result.Hero.EquippedWeapon.WeaponStyle == expectedWeapon,
            result.Route + " sees all three natural weapon routes and equips its selected route without a fixture");
        check(restored.TotalExperience == result.Hero.TotalExperience && restored.TotalGold == result.Hero.TotalGold &&
            restored.TotalSalvageGold == result.Hero.TotalSalvageGold && restored.SpentPoints == result.Hero.SpentPoints &&
            restored.UnspentPoints == result.Hero.UnspentPoints && restored.Inventory.Count == result.Hero.Inventory.Count &&
            restored.EquippedWeapon.WeaponStyle == expectedWeapon,
            result.Route + " hour-equivalent economy and build survive exact snapshot restore");
    }

    private static string Describe(Result result) => result.Route + "{kills=" + TotalKills + ",clears=" + result.Hero.DungeonClears +
        ",xp=" + result.Hero.TotalExperience + ",spent=" + result.Hero.SpentPoints + ",unspent=" + result.Hero.UnspentPoints +
        ",fullTreeKill=" + result.FullTreeKill + ",drops=" + result.Drops + ",upgrades=" + result.Upgrades +
        ",salvaged=" + result.Salvaged + ",salvageGold=" + result.SalvageGold + ",bag=" + result.Hero.Inventory.Count +
        ",maxBag=" + result.MaximumInventory + ",styles=" + result.WeaponStylesMask + "}";
}
