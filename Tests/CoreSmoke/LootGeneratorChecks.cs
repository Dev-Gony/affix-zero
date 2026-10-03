using System;
using System.Collections.Generic;
using AffixZero.Core;

internal static class LootGeneratorChecks
{
    public static void Run(Action<bool,string> check)
    {
        var rules=LootGenerator.Rarities;
        check(rules.Count==6&&rules[0].Id=="normal"&&rules[5].Id=="epic","loot exposes the documented six rarity tiers");
        check(rules[0].BaseWeight==60f&&rules[1].BaseWeight==25f&&rules[2].BaseWeight==10f&&rules[3].BaseWeight==4f&&rules[4].BaseWeight==1f&&rules[5].BaseWeight==.2f,"rarity base weights match the recovered production model");
        check(rules[0].MaxAffixes==0&&rules[5].MaxAffixes==6&&rules[5].StatMultiplier==4.5f,"rarity affix caps and top multiplier are explicit");
        check(LootGenerator.ConfiguredRollRange(AffixStat.Attack)=="1-15 x floor"&&
            LootGenerator.ConfiguredRollRange(AffixStat.Speed)=="5-30% x floor",
            "equipment tooltip roll bands come from the configured loot affix rules");
        check(LootGenerator.FieldDropChancePercent(1)==.8f&&LootGenerator.FieldDropChancePercent(999)==2f&&LootGenerator.EliteDropChancePercent(1)==18f&&LootGenerator.EliteDropChancePercent(999)==30f,"field and elite drop rates clamp to documented bounds");
        check(Math.Abs(LootGenerator.FieldDropChancePercent(20,DungeonDifficulty.Scout)-1.028f)<.0001f&&
            Math.Abs(LootGenerator.FieldDropChancePercent(20,DungeonDifficulty.Veteran)-1.285f)<.0001f&&
            Math.Abs(LootGenerator.FieldDropChancePercent(20,DungeonDifficulty.Torment)-1.6448f)<.0001f&&
            LootGenerator.EliteDropChancePercent(999,DungeonDifficulty.Torment)==48f,
            "difficulty exposes bounded field and elite drop multipliers");
        check(DifficultyTuning.Get(DungeonDifficulty.Scout).EliteStride==8&&
            DifficultyTuning.ScaleEnemyHealth(100,DungeonDifficulty.Veteran)==145&&
            DifficultyTuning.ScaleEnemyDamage(10,DungeonDifficulty.Torment)==16&&
            DifficultyTuning.ScaleReward(25,DungeonDifficulty.Torment)==38,
            "difficulty changes encounter composition, pressure and rewards explicitly");
        float[] low=LootGenerator.RarityProbabilities(1),high=LootGenerator.RarityProbabilities(100);
        float lowSum=0,highSum=0;foreach(float p in low)lowSum+=p;foreach(float p in high)highSum+=p;
        check(Math.Abs(lowSum-1)<.0001f&&Math.Abs(highSum-1)<.0001f&&high[5]>low[5]&&high[0]<low[0],"rarity weights normalize and progression shifts odds upward");
        var slots=new HashSet<EquipmentSlot>();var weaponStyles=new HashSet<WeaponStyle>();bool bounded=true,hasOptions=false,staffIconCorrect=false;
        for(int seed=0;seed<1000;seed++)
        {
            WeaponItem item=LootGenerator.GenerateGuaranteed("generated:"+seed,20,seed);slots.Add(item.EquipmentSlot);
            if(item.EquipmentSlot==EquipmentSlot.Weapon){weaponStyles.Add(item.WeaponStyle);staffIconCorrect|=item.WeaponStyle==WeaponStyle.Staff&&item.IconResource=="AffixGenerated/GearStaff";}
            bounded&=item.Options.Count<=8;hasOptions|=item.Options.Count>0;
        }
        check(slots.SetEquals(new[]{EquipmentSlot.Weapon,EquipmentSlot.Helmet,EquipmentSlot.Armor,EquipmentSlot.Gloves,EquipmentSlot.Boots,EquipmentSlot.Ring,EquipmentSlot.Amulet}),"generated pool reaches all seven historical equipment slots");
        check(weaponStyles.SetEquals(new[]{WeaponStyle.Sword,WeaponStyle.Axe,WeaponStyle.Staff})&&staffIconCorrect,
            "the unchanged natural weapon pool now reaches ARC, QUAKE and LANCE with matching icons");
        check(bounded&&hasOptions,"generated items keep base stats plus affixes within the serialized option ceiling");
        WeaponItem a=LootGenerator.GenerateGuaranteed("same",18,4242),b=LootGenerator.GenerateGuaranteed("same",18,4242);
        check(a.Name==b.Name&&a.Rarity==b.Rarity&&a.EquipmentSlot==b.EquipmentSlot&&a.DamageBonus==b.DamageBonus&&a.Options.Count==b.Options.Count,"seeded loot generation is deterministic for save and QA replay");

        var signatures=new HashSet<string>();bool noDuplicateStats=true,requestedBaseRetained=true;float minimumAttack=float.MaxValue,maximumAttack=0;
        for(int seed=0;seed<300;seed++)
        {
            WeaponItem roll=LootGenerator.GenerateGuaranteedBase("same-base:"+seed,"longsword","rare",12,seed);
            var stats=new HashSet<AffixStat>();string signature="";
            foreach(ItemOption option in roll.Options)
            {noDuplicateStats&=stats.Add(option.Stat);signature+=option.Stat+":"+option.Value+"|";if(option.Stat==AffixStat.Attack){minimumAttack=Math.Min(minimumAttack,option.Value);maximumAttack=Math.Max(maximumAttack,option.Value);}}
            signatures.Add(signature);
            requestedBaseRetained&=roll.EquipmentSlot==EquipmentSlot.Weapon&&roll.Rarity=="rare";
        }
        check(requestedBaseRetained,"forced same-base audit retains requested base and rarity");
        check(noDuplicateStats&&signatures.Count>80,"same base and rarity produce varied legal affix combinations without duplicate stats");
        check(maximumAttack>minimumAttack,"same-base attack affix values vary numerically across seeded rolls");

        int[] observed=new int[rules.Count];const int samples=100000;
        for(int seed=0;seed<samples;seed++)
        {
            WeaponItem roll=LootGenerator.GenerateGuaranteed("distribution:"+seed,20,seed,DungeonDifficulty.Veteran);
            for(int i=0;i<rules.Count;i++)if(rules[i].Id==roll.Rarity){observed[i]++;break;}
        }
        float[] expected=LootGenerator.RarityProbabilities(20,DungeonDifficulty.Veteran);
        bool distributionClose=true;for(int i=0;i<observed.Length;i++)distributionClose&=Math.Abs(observed[i]/(float)samples-expected[i])<.006f;
        check(distributionClose,"seeded rarity sample tracks normalized configured probabilities within 0.6 percentage points");
        Console.WriteLine("LOOT_VARIANCE same_base=longsword rarity=rare samples=300 signatures="+signatures.Count+
            " attack_affix_min="+(minimumAttack==float.MaxValue?0:minimumAttack)+" attack_affix_max="+maximumAttack);
        Console.WriteLine("LOOT_DISTRIBUTION floor=20 difficulty=Veteran samples="+samples+
            " expected="+string.Join(",",Array.ConvertAll(expected,p=>p.ToString("0.000000")))+
            " observed="+string.Join(",",Array.ConvertAll(observed,n=>(n/(float)samples).ToString("0.000000")))+
            " expected_field_kills_per_drop="+(100f/LootGenerator.FieldDropChancePercent(20,DungeonDifficulty.Veteran)).ToString("0.00"));
    }
}
