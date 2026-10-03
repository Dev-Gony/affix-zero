using System;
using System.Collections.Generic;

namespace AffixZero.Core
{
    public sealed class RarityRule
    {
        public string Id { get; }
        public string Name { get; }
        public int Index { get; }
        public int MaxAffixes { get; }
        public float StatMultiplier { get; }
        public float BaseWeight { get; }
        public RarityRule(string id, string name, int index, int maxAffixes, float statMultiplier, float baseWeight)
        { Id = id; Name = name; Index = index; MaxAffixes = maxAffixes; StatMultiplier = statMultiplier; BaseWeight = baseWeight; }
    }

    // Port of the last pre-restart loot model at c95b7ba. Weights are normalized after progression modifiers.
    public static class LootGenerator
    {
        private sealed class ItemBase
        {
            public string Id, Name;
            public EquipmentSlot Slot;
            public int Tier, Attack, Defense, Health, Mana;
            public float Speed;
            public WeaponStyle Style;
        }
        private sealed class AffixRule
        {
            public AffixStat Stat; public string Name; public float Minimum, Maximum; public bool Decimal;
        }
        public static readonly IReadOnlyList<RarityRule> Rarities = Array.AsReadOnly(new[]
        {
            new RarityRule("normal","일반",0,0,1f,60f),
            new RarityRule("magic","매직",1,2,1.3f,25f),
            new RarityRule("rare","레어",2,3,1.7f,10f),
            new RarityRule("unique","유니크",3,4,2.2f,4f),
            new RarityRule("legend","전설",4,5,3f,1f),
            new RarityRule("epic","에픽",5,6,4.5f,.2f)
        });
        public static string ConfiguredRollRange(AffixStat stat)
        {
            switch (stat)
            {
                case AffixStat.Attack: return "1-15 x floor";
                case AffixStat.Defense: return "1-10 x floor";
                case AffixStat.Health: return "5-60 x floor";
                case AffixStat.Mana: return "5-40 x floor";
                case AffixStat.Speed: return "5-30% x floor";
                case AffixStat.Critical: return "1-15 x floor";
                case AffixStat.Vampirism: return "1-8 x floor";
                case AffixStat.Experience: return "3-20 x floor";
                case AffixStat.Gold: return "5-30 x floor";
                case AffixStat.Penetration: return "1-10 x floor";
                default: return "configured roll";
            }
        }
        private static readonly ItemBase[] Bases =
        {
            Base("dagger","단검",EquipmentSlot.Weapon,1,attack:4), Base("longsword","장검",EquipmentSlot.Weapon,4,attack:8),
            Base("axe","도끼",EquipmentSlot.Weapon,8,attack:13,style:WeaponStyle.Axe), Base("magic_sword","마검",EquipmentSlot.Weapon,13,attack:20),
            Base("divine_sword","신검",EquipmentSlot.Weapon,20,attack:30),
            Base("leather_hat","가죽 모자",EquipmentSlot.Helmet,1,defense:2), Base("iron_helm","철 투구",EquipmentSlot.Helmet,5,defense:5),
            Base("mithril_helm","미스릴 투구",EquipmentSlot.Helmet,11,defense:9), Base("dragon_helm","용의 투구",EquipmentSlot.Helmet,18,defense:15),
            Base("cloth","천 갑옷",EquipmentSlot.Armor,1,defense:3), Base("leather_armor","가죽 갑옷",EquipmentSlot.Armor,5,defense:7),
            Base("plate_armor","판금 갑옷",EquipmentSlot.Armor,11,defense:12), Base("dragonscale","용린 갑옷",EquipmentSlot.Armor,18,defense:20),
            Base("cloth_gloves","천 장갑",EquipmentSlot.Gloves,1,attack:1,defense:1), Base("leather_gloves","가죽 장갑",EquipmentSlot.Gloves,5,attack:3,defense:2),
            Base("battle_gloves","전투 장갑",EquipmentSlot.Gloves,11,attack:6,defense:4), Base("dragon_gloves","용의 장갑",EquipmentSlot.Gloves,18,attack:10,defense:7),
            Base("sandals","샌들",EquipmentSlot.Boots,1,speed:.05f), Base("leather_boots","가죽 장화",EquipmentSlot.Boots,5,speed:.12f),
            Base("swift_boots","신속 장화",EquipmentSlot.Boots,11,speed:.22f), Base("gale_boots","질풍 장화",EquipmentSlot.Boots,18,speed:.35f),
            Base("copper_ring","구리 반지",EquipmentSlot.Ring,1,health:10), Base("silver_ring","은 반지",EquipmentSlot.Ring,5,health:25),
            Base("gold_ring","금 반지",EquipmentSlot.Ring,11,health:45), Base("diamond_ring","다이아 반지",EquipmentSlot.Ring,18,health:75),
            Base("bone_necklace","뼈 목걸이",EquipmentSlot.Amulet,1,mana:8), Base("crystal_necklace","수정 목걸이",EquipmentSlot.Amulet,5,mana:20),
            Base("ruby_necklace","루비 목걸이",EquipmentSlot.Amulet,11,mana:38), Base("dragon_tear","용의 눈물",EquipmentSlot.Amulet,18,mana:65)
        };
        private static readonly AffixRule[] Affixes =
        {
            Affix(AffixStat.Attack,"힘의",1,15), Affix(AffixStat.Defense,"수호의",1,10),
            Affix(AffixStat.Health,"활력의",5,60), Affix(AffixStat.Mana,"지혜의",5,40),
            Affix(AffixStat.Speed,"신속의",.05f,.3f,true), Affix(AffixStat.Critical,"치명의",1,15),
            Affix(AffixStat.Vampirism,"흡혈의",1,8), Affix(AffixStat.Experience,"경험의",3,20),
            Affix(AffixStat.Gold,"재물의",5,30), Affix(AffixStat.Penetration,"관통의",1,10)
        };

        public static float FieldDropChancePercent(int floor, int rebirths = 0) =>
            FieldDropChancePercent(floor, DungeonDifficulty.Scout, rebirths);
        public static float FieldDropChancePercent(int floor, DungeonDifficulty difficulty, int rebirths = 0)
        {
            float baseline = Clamp(.8f + Math.Max(0, floor - 1) * .012f + Math.Max(0, rebirths) * .08f, .8f, 2f);
            return Clamp(baseline * DifficultyTuning.Get(difficulty).DropMultiplier, .8f, 3.2f);
        }
        public static float EliteDropChancePercent(int floor, int rebirths = 0) =>
            EliteDropChancePercent(floor, DungeonDifficulty.Scout, rebirths);
        public static float EliteDropChancePercent(int floor, DungeonDifficulty difficulty, int rebirths = 0)
        {
            float baseline = Clamp(18f + Math.Max(0, floor - 6) * .10f + Math.Max(0, rebirths) * .8f, 18f, 30f);
            return Clamp(baseline * DifficultyTuning.Get(difficulty).DropMultiplier, 18f, 48f);
        }
        public static float[] RarityProbabilities(int floor, int rebirths = 0) =>
            RarityProbabilities(floor, DungeonDifficulty.Scout, rebirths);
        public static float[] RarityProbabilities(int floor, DungeonDifficulty difficulty, int rebirths = 0)
        {
            float progression = Clamp(Math.Max(0, floor - 1) * .004f + Math.Max(0, rebirths) * .02f +
                DifficultyTuning.Get(difficulty).RarityProgressionBonus, 0, .6f);
            var values = new float[Rarities.Count]; float total = 0;
            for (int i = 0; i < values.Length; i++)
            {
                float modifier = i == 0 ? Math.Max(.78f, 1f - progression * .25f) :
                    i == 1 ? 1f : i == 2 ? 1f + progression * .4f :
                    i == 3 ? .35f + progression * .25f : .05f + progression * .05f;
                values[i] = Rarities[i].BaseWeight * modifier; total += values[i];
            }
            for (int i = 0; i < values.Length; i++) values[i] /= total;
            return values;
        }
        public static WeaponItem TryGenerate(string identity, int floor, bool elite, int seed, int rebirths = 0)
            => TryGenerate(identity, floor, elite, seed, DungeonDifficulty.Scout, rebirths);
        public static WeaponItem TryGenerate(string identity, int floor, bool elite, int seed,
            DungeonDifficulty difficulty, int rebirths = 0)
        {
            var random = new Random(seed);
            float chance = elite ? EliteDropChancePercent(floor, difficulty, rebirths) : FieldDropChancePercent(floor, difficulty, rebirths);
            if (random.NextDouble() * 100 >= chance) return null;
            return Generate(identity, floor, rebirths, difficulty, random, null, null);
        }
        public static WeaponItem GenerateGuaranteed(string identity, int floor, int seed, int rebirths = 0) =>
            Generate(identity, floor, rebirths, DungeonDifficulty.Scout, new Random(seed), null, null);
        public static WeaponItem GenerateGuaranteed(string identity, int floor, int seed, DungeonDifficulty difficulty, int rebirths = 0) =>
            Generate(identity, floor, rebirths, difficulty, new Random(seed), null, null);
        // Audit hook for seeded distribution tests and comparison UI fixtures. Runtime drops still
        // choose from the full eligible pool; this only fixes base and rarity, never option values.
        public static WeaponItem GenerateGuaranteedBase(string identity, string baseId, string rarityId, int floor, int seed)
        {
            ItemBase basis = Array.Find(Bases, item => item.Id == baseId);
            RarityRule rarity = null;
            foreach (RarityRule candidate in Rarities) if (candidate.Id == rarityId) rarity = candidate;
            if (basis == null || rarity == null || basis.Tier > floor) throw new ArgumentOutOfRangeException(nameof(baseId));
            return Generate(identity, floor, 0, DungeonDifficulty.Scout, new Random(seed), basis, rarity);
        }
        private static WeaponItem Generate(string identity, int floor, int rebirths, DungeonDifficulty difficulty,
            Random random, ItemBase forcedBase, RarityRule forcedRarity)
        {
            if (string.IsNullOrWhiteSpace(identity) || floor < 1 || rebirths < 0) throw new ArgumentOutOfRangeException(nameof(floor));
            RarityRule rarity = forcedRarity ?? RollRarity(floor, rebirths, difficulty, random);
            var eligible = new List<ItemBase>();
            foreach (ItemBase item in Bases) if (item.Tier <= floor) eligible.Add(item);
            ItemBase basis = forcedBase ?? eligible[random.Next(eligible.Count)];
            float floorScale = 1f + (floor - 1) * .15f;
            int attack = Round(basis.Attack * floorScale * rarity.StatMultiplier);
            int defense = Round(basis.Defense * floorScale * rarity.StatMultiplier);
            int health = Round(basis.Health * floorScale * rarity.StatMultiplier);
            var options = new List<ItemOption>();
            if (basis.Mana > 0) options.Add(new ItemOption(AffixStat.Mana, "기본 마나", Round(basis.Mana * floorScale * rarity.StatMultiplier)));
            if (basis.Speed > 0) options.Add(new ItemOption(AffixStat.Speed, "기본 속도", basis.Speed * floorScale * rarity.StatMultiplier));
            var candidates = new List<AffixRule>(Affixes);
            int count = rarity.MaxAffixes == 0 ? 0 : random.Next(1, rarity.MaxAffixes + 1);
            float affixScale = 1f + (floor - 1) * .03f;
            for (int i = 0; i < count; i++)
            {
                int chosen = random.Next(candidates.Count); AffixRule rule = candidates[chosen]; candidates.RemoveAt(chosen);
                float value = (float)(rule.Minimum + random.NextDouble() * (rule.Maximum - rule.Minimum)) * affixScale;
                value = rule.Decimal ? (float)Math.Round(value, 2) : Round(value);
                options.Add(new ItemOption(rule.Stat, rule.Name, value));
            }
            string prefix = count == 0 ? "" : options[options.Count - count].Name + " ";
            string icon = IconFor(basis);
            string affixText = count == 0 ? "" : count + "개 옵션";
            return new WeaponItem(identity, prefix + basis.Name, attack, 0, affixText, icon,
                rarity.Id, 0, basis.Slot, basis.Style, defense, health, 0, options);
        }
        private static RarityRule RollRarity(int floor, int rebirths, DungeonDifficulty difficulty, Random random)
        {
            float[] probabilities = RarityProbabilities(floor, difficulty, rebirths);
            double roll = random.NextDouble(), cursor = 0;
            for (int i = 0; i < probabilities.Length; i++)
            { cursor += probabilities[i]; if (roll <= cursor) return Rarities[i]; }
            return Rarities[Rarities.Count - 1];
        }
        private static string IconFor(ItemBase item)
        {
            if (item.Slot == EquipmentSlot.Weapon)
                return item.Style == WeaponStyle.Axe ? "AffixGenerated/GearAxe" :
                    item.Style == WeaponStyle.Staff ? "AffixGenerated/GearStaff" : "AffixGenerated/EmberSword";
            switch (item.Slot)
            {
                case EquipmentSlot.Helmet: return "AffixUIVisual/GearHelmet";
                case EquipmentSlot.Armor: return "AffixGenerated/GearArmor";
                case EquipmentSlot.Gloves: return "AffixUIVisual/GearGloves";
                case EquipmentSlot.Boots: return "AffixUIVisual/GearBoots";
                case EquipmentSlot.Ring: return "AffixUIVisual/GearRing";
                case EquipmentSlot.Amulet: return "AffixUIVisual/GearAmulet";
                case EquipmentSlot.Relic: return "AffixGenerated/GearRelic";
                default: return "AffixGenerated/GearArmor";
            }
        }
        private static ItemBase Base(string id, string name, EquipmentSlot slot, int tier, int attack = 0,
            int defense = 0, int health = 0, int mana = 0, float speed = 0, WeaponStyle style = WeaponStyle.Sword) =>
            new ItemBase { Id = id, Name = id == "magic_sword" ? "Arcane Staff" : name, Slot = slot, Tier = tier, Attack = attack, Defense = defense,
                Health = health, Mana = mana, Speed = speed, Style = id == "magic_sword" ? WeaponStyle.Staff : style };
        private static AffixRule Affix(AffixStat stat, string name, float minimum, float maximum, bool decimalValue = false) =>
            new AffixRule { Stat = stat, Name = name, Minimum = minimum, Maximum = maximum, Decimal = decimalValue };
        private static int Round(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
        private static float Clamp(float value, float minimum, float maximum) => Math.Max(minimum, Math.Min(maximum, value));
    }
}
