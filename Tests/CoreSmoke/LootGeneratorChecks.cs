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
        check(LootGenerator.FieldDropChancePercent(1)==.8f&&LootGenerator.FieldDropChancePercent(999)==2f&&LootGenerator.EliteDropChancePercent(1)==18f&&LootGenerator.EliteDropChancePercent(999)==30f,"field and elite drop rates clamp to documented bounds");
        float[] low=LootGenerator.RarityProbabilities(1),high=LootGenerator.RarityProbabilities(100);
        float lowSum=0,highSum=0;foreach(float p in low)lowSum+=p;foreach(float p in high)highSum+=p;
        check(Math.Abs(lowSum-1)<.0001f&&Math.Abs(highSum-1)<.0001f&&high[5]>low[5]&&high[0]<low[0],"rarity weights normalize and progression shifts odds upward");
        var slots=new HashSet<EquipmentSlot>();bool bounded=true,hasOptions=false;
        for(int seed=0;seed<1000;seed++)
        {
            WeaponItem item=LootGenerator.GenerateGuaranteed("generated:"+seed,20,seed);slots.Add(item.EquipmentSlot);
            bounded&=item.Options.Count<=8;hasOptions|=item.Options.Count>0;
        }
        check(slots.SetEquals(new[]{EquipmentSlot.Weapon,EquipmentSlot.Helmet,EquipmentSlot.Armor,EquipmentSlot.Gloves,EquipmentSlot.Boots,EquipmentSlot.Ring,EquipmentSlot.Amulet}),"generated pool reaches all seven historical equipment slots");
        check(bounded&&hasOptions,"generated items keep base stats plus affixes within the serialized option ceiling");
        WeaponItem a=LootGenerator.GenerateGuaranteed("same",18,4242),b=LootGenerator.GenerateGuaranteed("same",18,4242);
        check(a.Name==b.Name&&a.Rarity==b.Rarity&&a.EquipmentSlot==b.EquipmentSlot&&a.DamageBonus==b.DamageBonus&&a.Options.Count==b.Options.Count,"seeded loot generation is deterministic for save and QA replay");
    }
}
