using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AffixZero.Core
{
    public enum TalentId { Fury, Precision, Keystone, Vitality, Cleave, Haste }
    // Numeric values 0-2 retain compatibility with the first expanded Unity draft.
    public enum EquipmentSlot { Weapon = 0, Armor = 1, Relic = 2, Helmet = 3, Gloves = 4, Boots = 5, Ring = 6, Amulet = 7 }
    public enum WeaponStyle { Sword, Axe, Staff }
    public enum AreaSkillTrajectory { Radial, Chain, Quake, Pierce }
    public enum AffixStat { Attack, Defense, Health, Mana, Speed, Critical, Vampirism, Experience, Gold, Penetration }

    public sealed class ItemOption
    {
        public AffixStat Stat { get; }
        public string Name { get; }
        public float Value { get; }
        public ItemOption(AffixStat stat, string name, float value)
        {
            if (!Enum.IsDefined(typeof(AffixStat), stat) || string.IsNullOrWhiteSpace(name) ||
                value <= 0 || float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
            Stat = stat; Name = name; Value = value;
        }
    }

    public sealed class TalentDefinition
    {
        public TalentId Id { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaxRank { get; }
        public TalentId? Prerequisite { get; }
        public int RequiredRank { get; }
        internal TalentDefinition(TalentId id, string name, string description, int cap,
            TalentId? prerequisite = null, int requiredRank = 0)
        { Id = id; Name = name; Description = description; MaxRank = cap;
          Prerequisite = prerequisite; RequiredRank = requiredRank; }
    }

    public sealed class WeaponItem
    {
        public const int MaxEnhancementRank = 20;
        public string Id { get; }
        public string Name { get; }
        public int FlatDamage { get; }
        public int AffixDamage { get; }
        public string AffixName { get; }
        public string IconResource { get; }
        public string Rarity { get; }
        public int EnhancementRank { get; }
        public EquipmentSlot EquipmentSlot { get; }
        public WeaponStyle WeaponStyle { get; }
        public int FlatDefense { get; }
        public int FlatHealth { get; }
        public int BaseCooldownReductionPercent { get; }
        public IReadOnlyList<ItemOption> Options { get; }
        private bool DefensiveSlot => EquipmentSlot == EquipmentSlot.Armor || EquipmentSlot == EquipmentSlot.Helmet ||
            EquipmentSlot == EquipmentSlot.Gloves || EquipmentSlot == EquipmentSlot.Boots;
        private float Option(AffixStat stat) { float total = 0; foreach (ItemOption option in Options) if (option.Stat == stat) total += option.Value; return total; }
        public int EnhancementDamage => DefensiveSlot ? 0 : EnhancementRank * 2;
        public int DamageBonus => FlatDamage + AffixDamage + MathfRound(Option(AffixStat.Attack)) + EnhancementDamage;
        public int DefenseBonus => FlatDefense + MathfRound(Option(AffixStat.Defense)) + (DefensiveSlot ? EnhancementRank : 0);
        public int HealthBonus => FlatHealth + MathfRound(Option(AffixStat.Health)) + (DefensiveSlot ? EnhancementRank * 5 : 0);
        public int ManaBonus => MathfRound(Option(AffixStat.Mana));
        public float SpeedBonus => Option(AffixStat.Speed);
        public int CriticalChance => MathfRound(Option(AffixStat.Critical));
        public int VampirismPercent => MathfRound(Option(AffixStat.Vampirism));
        public int ExperienceBonusPercent => MathfRound(Option(AffixStat.Experience));
        public int GoldBonusPercent => MathfRound(Option(AffixStat.Gold));
        public int Penetration => MathfRound(Option(AffixStat.Penetration));
        public int CooldownReductionPercent => BaseCooldownReductionPercent + MathfRound(SpeedBonus * 100f);
        public int NextEnhancementDamage => EnhancementRank >= MaxEnhancementRank || DefensiveSlot ? 0 : 2;
        public int NextEnhancementDefense => EnhancementRank < MaxEnhancementRank && DefensiveSlot ? 1 : 0;
        public int NextEnhancementHealth => EnhancementRank < MaxEnhancementRank && DefensiveSlot ? 5 : 0;
        public string EnhancementDescription => EnhancementRank >= MaxEnhancementRank ? "최대 강화 단계" :
            DefensiveSlot ? "방어력 +1 · 최대 체력 +5" : "공격력 +2";
        private static int MathfRound(float value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

        public WeaponItem(string id, string name, int flatDamage, int affixDamage,
            string affixName, string iconResource, string rarity, int enhancementRank = 0,
            EquipmentSlot equipmentSlot = EquipmentSlot.Weapon, WeaponStyle weaponStyle = WeaponStyle.Sword,
            int flatDefense = 0, int flatHealth = 0, int cooldownReductionPercent = 0,
            IEnumerable<ItemOption> options = null)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(iconResource) || string.IsNullOrWhiteSpace(rarity))
                throw new ArgumentException("Item identity, name, icon and rarity are required.");
            if (enhancementRank < 0 || enhancementRank > MaxEnhancementRank)
                throw new ArgumentOutOfRangeException(nameof(enhancementRank));
            // Preserve the legacy intake boundary; aggregate stats use saturating long arithmetic.
            if (flatDamage < 0 || affixDamage < 0 || (long)flatDamage + affixDamage > int.MaxValue - 46)
                throw new ArgumentOutOfRangeException(nameof(flatDamage));
            if (affixDamage > 0 && string.IsNullOrWhiteSpace(affixName))
                throw new ArgumentException("A damage affix requires a name.", nameof(affixName));
            if (!Enum.IsDefined(typeof(EquipmentSlot), equipmentSlot) || !Enum.IsDefined(typeof(WeaponStyle), weaponStyle) ||
                (equipmentSlot != EquipmentSlot.Weapon && weaponStyle != WeaponStyle.Sword))
                throw new ArgumentOutOfRangeException(nameof(equipmentSlot));
            if (flatDefense < 0 || flatDefense > int.MaxValue - 20 || flatHealth < 0 ||
                flatHealth > int.MaxValue - 100 || cooldownReductionPercent < 0 || cooldownReductionPercent > 50)
                throw new ArgumentOutOfRangeException(nameof(flatDefense));
            EquipmentSlot = equipmentSlot; WeaponStyle = weaponStyle; FlatDefense = flatDefense;
            FlatHealth = flatHealth; BaseCooldownReductionPercent = cooldownReductionPercent;
            var optionList = options == null ? new List<ItemOption>() : new List<ItemOption>(options);
            // Six random affixes may sit beside authored base stats on the same item.
            if (optionList.Count > 8 || optionList.Contains(null)) throw new ArgumentException("An item supports at most eight non-null options.", nameof(options));
            Options = optionList.AsReadOnly();
            Id = id;
            Name = name;
            FlatDamage = flatDamage;
            AffixDamage = affixDamage;
            AffixName = affixName ?? string.Empty;
            IconResource = iconResource;
            Rarity = rarity;
            EnhancementRank = enhancementRank;
        }
    }

    // The presentation layer owns lifetime and file I/O; snapshots contain only domain state.
    public sealed class HeroProgression
    {
        public const int InventoryCapacity = 24;
        public const int BaseDamage = 24;
        public const int ExperiencePerLevel = 250;
        public const int TotalTalentCapacity = 75;
        private static readonly ReadOnlyCollection<TalentDefinition> talentDefinitions = Array.AsReadOnly(new[]
        {
            new TalentDefinition(TalentId.Fury, "격노", "랭크마다 공격력 +3", 20),
            new TalentDefinition(TalentId.Precision, "정밀", "랭크마다 공격력 +4", 10, TalentId.Fury, 2),
            new TalentDefinition(TalentId.Keystone, "숙련", "랭크마다 공격력 +6", 5, TalentId.Precision, 1),
            new TalentDefinition(TalentId.Vitality, "강인함", "랭크마다 최대 체력 +10", 20),
            new TalentDefinition(TalentId.Cleave, "휩쓸기", "랭크마다 광역 반경 +0.08, 주변 피해 +2.5%", 10, TalentId.Fury, 2),
            new TalentDefinition(TalentId.Haste, "가속", "랭크마다 공격 대기 시간 2% 감소", 10, TalentId.Precision, 1)
        });
        public static IReadOnlyList<TalentDefinition> TalentDefinitions => talentDefinitions;
        private int legacyPointCredit;
        private const string FirstDropId = "loot:ember-steel:first";
        private readonly List<WeaponItem> inventory = new List<WeaponItem>();
        private readonly HashSet<string> killTokens = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> issuedItemIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly ReadOnlyCollection<WeaponItem> inventoryView;
        private bool firstDropWaiting;

        public IReadOnlyList<WeaponItem> Inventory => inventoryView;
        public WeaponItem EquippedWeapon { get; private set; }
        public WeaponItem EquippedArmor { get; private set; }
        public WeaponItem EquippedRelic { get; private set; }
        public WeaponItem EquippedHelmet { get; private set; }
        public WeaponItem EquippedGloves { get; private set; }
        public WeaponItem EquippedBoots { get; private set; }
        public WeaponItem EquippedRing { get; private set; }
        public WeaponItem EquippedAmulet { get; private set; }
        public WeaponItem PendingLoot { get; private set; }
        public int UnspentPoints { get; private set; }
        public int TotalExperience { get; private set; }
        // Available run gold after purchases, not lifetime gross earnings.
        public int TotalGold { get; private set; }
        public int TotalSalvageGold { get; private set; }
        public int LastSalvageGold { get; private set; }
        public int FuryRank { get; private set; }
        public int PrecisionRank { get; private set; }
        public int KeystoneRank { get; private set; }
        public int VitalityRank { get; private set; }
        public int CleaveRank { get; private set; }
        public int HasteRank { get; private set; }
        public DungeonDifficulty SelectedDifficulty { get; private set; }
        public int DungeonClears { get; private set; }
        public int LegacyPointCredit => legacyPointCredit;
        public int Level => 1 + TotalExperience / ExperiencePerLevel;
        public int SpentPoints => FuryRank + PrecisionRank + KeystoneRank + VitalityRank + CleaveRank + HasteRank;
        public int TalentDamage => FuryRank * 3 + PrecisionRank * 4 + KeystoneRank * 6;
        public int FuryEvolutionTier => EvolutionTier(FuryRank, 5, 15);
        public int PrecisionEvolutionTier => EvolutionTier(PrecisionRank, 3, 8);
        public int KeystoneEvolutionTier => EvolutionTier(KeystoneRank, 2, 5);
        public int VitalityEvolutionTier => EvolutionTier(VitalityRank, 5, 15);
        public int CleaveEvolutionTier => EvolutionTier(CleaveRank, 3, 8);
        public int HasteEvolutionTier => EvolutionTier(HasteRank, 3, 8);
        public int ActiveEvolutionCount => FuryEvolutionTier + PrecisionEvolutionTier + KeystoneEvolutionTier +
            VitalityEvolutionTier + CleaveEvolutionTier + HasteEvolutionTier;
        private IEnumerable<WeaponItem> EquippedItems
        {
            get
            {
                yield return EquippedWeapon;
                if (EquippedHelmet != null) yield return EquippedHelmet;
                if (EquippedArmor != null) yield return EquippedArmor;
                if (EquippedGloves != null) yield return EquippedGloves;
                if (EquippedBoots != null) yield return EquippedBoots;
                if (EquippedRing != null) yield return EquippedRing;
                if (EquippedAmulet != null) yield return EquippedAmulet;
                if (EquippedRelic != null) yield return EquippedRelic;
            }
        }
        private int Sum(Func<WeaponItem, int> read) { long total = 0; foreach (WeaponItem item in EquippedItems) total += read(item); return Saturate(total); }
        public int TotalDamage => Saturate((long)BaseDamage + Sum(item => item.DamageBonus) + TalentDamage);
        public int TotalMaxHp => Saturate(120L + VitalityRank * 10 + Sum(item => item.HealthBonus));
        public int TotalDefense => Saturate(2L + Sum(item => item.DefenseBonus));
        public int TotalMana => Saturate(30L + Sum(item => item.ManaBonus));
        public int CriticalChance => Math.Min(75, 5 + PrecisionEvolutionTier * 5 + Sum(item => item.CriticalChance));
        public int VampirismPercent => Math.Min(40, Sum(item => item.VampirismPercent));
        public int ExperienceBonusPercent => Math.Min(20, Sum(item => item.ExperienceBonusPercent));
        public int GoldBonusPercent => Math.Min(30, Sum(item => item.GoldBonusPercent));
        public int Penetration => Math.Min(50, (KeystoneEvolutionTier == 0 ? 0 : KeystoneEvolutionTier == 1 ? 5 : 12) + Sum(item => item.Penetration));
        public int CooldownReductionPercent => Math.Min(50, HasteRank * 2 + Sum(item => item.CooldownReductionPercent));
        public float AttackSpeedMultiplier => (EquippedWeapon.WeaponStyle == WeaponStyle.Axe ? .8f :
            EquippedWeapon.WeaponStyle == WeaponStyle.Staff ? .9f : 1f) / (1f - CooldownReductionPercent / 100f);
        public float AttackReach => EquippedWeapon.WeaponStyle == WeaponStyle.Staff ? 4.5f :
            EquippedWeapon.WeaponStyle == WeaponStyle.Axe ? 1.3f : 1.05f;
        public float SplashRadius => (EquippedWeapon.WeaponStyle == WeaponStyle.Axe ? 1.1f : 0f) + CleaveRank * .08f;
        public float SplashDamageFraction => (EquippedWeapon.WeaponStyle == WeaponStyle.Axe ? .5f : 0f) + CleaveRank * .025f;
        public float AreaSkillDamageMultiplier => 1.5f + FuryEvolutionTier * .15f;
        public float AreaSkillRadius => 3.2f + (CleaveEvolutionTier == 0 ? 0 : CleaveEvolutionTier == 1 ? .4f : .9f);
        public int AreaSkillMinimumTargets => HasteEvolutionTier > 0 ? 1 : 2;
        public float AreaSkillArmingDelay => HasteEvolutionTier == 0 ? 2.6f : HasteEvolutionTier == 1 ? 2.2f : 1.8f;
        public AreaSkillTrajectory AreaTrajectory => FuryEvolutionTier == 0 ? AreaSkillTrajectory.Radial :
            EquippedWeapon.WeaponStyle == WeaponStyle.Staff ? AreaSkillTrajectory.Pierce :
            EquippedWeapon.WeaponStyle == WeaponStyle.Axe ? AreaSkillTrajectory.Quake : AreaSkillTrajectory.Chain;
        public int AreaSkillMaxTargets => AreaTrajectory == AreaSkillTrajectory.Chain ? 2 + FuryEvolutionTier :
            AreaTrajectory == AreaSkillTrajectory.Pierce ? 3 + FuryEvolutionTier : 24;
        public float AreaSkillJumpRange => 2.15f + FuryEvolutionTier * .35f;
        public float AreaSkillPierceReach => 4.8f + FuryEvolutionTier * .7f;
        public float AreaSkillPierceWidth => .36f + FuryEvolutionTier * .09f;
        public int RecoveryThresholdPercent => 45 + VitalityEvolutionTier * 5;
        public int RecoveryHealPercent => 20 + VitalityEvolutionTier * 5;
        public float RecoveryArmingDelay => HasteEvolutionTier == 0 ? 8f : HasteEvolutionTier == 1 ? 7f : 6f;
        public string AreaSkillName => AreaTrajectory == AreaSkillTrajectory.Chain ? (FuryEvolutionTier == 2 ? "ARC II" : "ARC") :
            AreaTrajectory == AreaSkillTrajectory.Pierce ? (FuryEvolutionTier == 2 ? "LANCE II" : "LANCE") :
            AreaTrajectory == AreaSkillTrajectory.Quake ? (FuryEvolutionTier == 2 ? "QUAKE II" : "QUAKE") :
            CleaveEvolutionTier == 2 ? "TEMPEST" : CleaveEvolutionTier == 1 ? "WHIRL" :
            FuryEvolutionTier == 2 ? "DEVASTATE" : FuryEvolutionTier == 1 ? "REND" :
            HasteEvolutionTier > 0 ? "QUICKCAST" : "AREA";
        public string RecoverySkillName => VitalityEvolutionTier == 2 ? "SURGE" : VitalityEvolutionTier == 1 ? "WIND" : "HEAL";
        public bool IsRanged => EquippedWeapon.WeaponStyle == WeaponStyle.Staff;
        public int LastExperienceReward { get; private set; }
        public int LastGoldReward { get; private set; }
        public int EquippedEnhancementCost => GetEnhancementCost(EquipmentSlot.Weapon);
        public bool CanEnhanceEquipped => CanEnhance(EquipmentSlot.Weapon);
        private static int Saturate(long value) => (int)Math.Min(int.MaxValue, value);
        private static int EvolutionTier(int rank, int first, int second) => rank >= second ? 2 : rank >= first ? 1 : 0;

        public static TalentDefinition GetTalentDefinition(TalentId talent) =>
            (int)talent >= 0 && (int)talent < talentDefinitions.Count ? talentDefinitions[(int)talent] : null;
        public int GetTalentRank(TalentId talent)
        {
            switch (talent)
            {
                case TalentId.Fury: return FuryRank;
                case TalentId.Precision: return PrecisionRank;
                case TalentId.Keystone: return KeystoneRank;
                case TalentId.Vitality: return VitalityRank;
                case TalentId.Cleave: return CleaveRank;
                case TalentId.Haste: return HasteRank;
                default: return 0;
            }
        }
        public bool CanSpendPoint(TalentId talent)
        {
            var definition = GetTalentDefinition(talent);
            return definition != null && UnspentPoints > 0 && GetTalentRank(talent) < definition.MaxRank &&
                (!definition.Prerequisite.HasValue || GetTalentRank(definition.Prerequisite.Value) >= definition.RequiredRank);
        }

        public HeroProgression()
        {
            inventoryView = inventory.AsReadOnly();
            EquippedWeapon = new WeaponItem("equipped:starting-sword", "훈련용 검", 6, 0,
                string.Empty, "AffixGenerated/AttackIcon", "Common");
            issuedItemIds.Add(EquippedWeapon.Id);
        }

        // Early points arrive quickly so a fresh run makes choices immediately. Later bands slow
        // down to preserve build decisions across sessions instead of completing all 75 ranks in
        // one twenty-minute farm. Level display remains the historical 250-XP cadence.
        public static int EarnedTalentPointsForExperience(int experience)
        {
            if (experience < 0) throw new ArgumentOutOfRangeException(nameof(experience));
            if (experience < 2500) return experience / 250;
            if (experience < 10500) return 10 + (experience - 2500) / 400;
            if (experience < 23500) return 30 + (experience - 10500) / 650;
            if (experience < 48500) return 50 + (experience - 23500) / 1000;
            return 75 + (experience - 48500) / 1500;
        }

        // XP and the historical level display continue after the tree is complete, but a new
        // profile never accumulates points that have no legal destination. Compatibility credit
        // is included before clamping so migrated players retain every usable rank.
        public static int SpendableTalentPointsForExperience(int experience, int compatibilityCredit = 0)
        {
            if (compatibilityCredit < 0) throw new ArgumentOutOfRangeException(nameof(compatibilityCredit));
            long points = (long)EarnedTalentPointsForExperience(experience) + compatibilityCredit;
            return (int)Math.Min(TotalTalentCapacity, points);
        }

        public bool TrySetDifficulty(DungeonDifficulty difficulty)
        {
            if (!Enum.IsDefined(typeof(DungeonDifficulty), difficulty)) return false;
            SelectedDifficulty = difficulty;
            return true;
        }

        public void RegisterDungeonClear()
        {
            if (DungeonClears == int.MaxValue) throw new InvalidOperationException("Dungeon clear count overflow.");
            DungeonClears++;
        }

        public bool TryRegisterKill(string uniqueToken)
        {
            if (string.IsNullOrWhiteSpace(uniqueToken) || killTokens.Contains(uniqueToken) ||
                (long)UnspentPoints + SpentPoints >= int.MaxValue ||
                TotalExperience > int.MaxValue - 45 || TotalGold > int.MaxValue - 17) return false;
            killTokens.Add(uniqueToken);
            int previousTalentPoints = SpendableTalentPointsForExperience(TotalExperience, legacyPointCredit);
            LastExperienceReward = DifficultyTuning.ScaleReward(25 + 25 * ExperienceBonusPercent / 100, SelectedDifficulty);
            LastGoldReward = DifficultyTuning.ScaleReward(8 + 8 * GoldBonusPercent / 100, SelectedDifficulty);
            TotalExperience += LastExperienceReward;
            UnspentPoints += SpendableTalentPointsForExperience(TotalExperience, legacyPointCredit) - previousTalentPoints;
            TotalGold += LastGoldReward;
            return true;
        }

        // Explicit intake for subsequent authored drops; never overwrite an uncollected drop.
        public bool TryCreatePendingLoot(WeaponItem item)
        {
            if (item == null || PendingLoot != null || issuedItemIds.Contains(item.Id) || item.Id == FirstDropId)
                return false;
            PendingLoot = item;
            issuedItemIds.Add(item.Id);
            return true;
        }

        public bool PickUp()
        {
            if (PendingLoot == null || inventory.Count >= InventoryCapacity) return false;
            inventory.Add(PendingLoot);
            PendingLoot = null;
            // Only migrated v1 profiles can carry this deferred onboarding reward.
            OfferFirstDrop();
            return true;
        }

        private void OfferFirstDrop()
        {
            if (!firstDropWaiting || PendingLoot != null) return;
            PendingLoot = new WeaponItem(FirstDropId, "잿불 강철검", 12, 4,
                "잿불", "AffixGenerated/EmberSword", "Rare");
            issuedItemIds.Add(PendingLoot.Id);
            firstDropWaiting = false;
        }

        public WeaponItem GetEquipped(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: return EquippedWeapon;
                case EquipmentSlot.Armor: return EquippedArmor;
                case EquipmentSlot.Relic: return EquippedRelic;
                case EquipmentSlot.Helmet: return EquippedHelmet;
                case EquipmentSlot.Gloves: return EquippedGloves;
                case EquipmentSlot.Boots: return EquippedBoots;
                case EquipmentSlot.Ring: return EquippedRing;
                case EquipmentSlot.Amulet: return EquippedAmulet;
                default: return null;
            }
        }
        private void SetEquipped(EquipmentSlot slot, WeaponItem item)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: EquippedWeapon = item; break;
                case EquipmentSlot.Armor: EquippedArmor = item; break;
                case EquipmentSlot.Relic: EquippedRelic = item; break;
                case EquipmentSlot.Helmet: EquippedHelmet = item; break;
                case EquipmentSlot.Gloves: EquippedGloves = item; break;
                case EquipmentSlot.Boots: EquippedBoots = item; break;
                case EquipmentSlot.Ring: EquippedRing = item; break;
                case EquipmentSlot.Amulet: EquippedAmulet = item; break;
            }
        }
        public bool Equip(int index)
        {
            if (!ValidIndex(index)) return false;
            WeaponItem item = inventory[index];
            WeaponItem previous = GetEquipped(item.EquipmentSlot);
            SetEquipped(item.EquipmentSlot, item);
            if (previous == null) inventory.RemoveAt(index); else inventory[index] = previous;
            return true;
        }
        public static int EnhancementCostForRank(int currentRank)
        {
            if (currentRank < 0 || currentRank >= WeaponItem.MaxEnhancementRank) return 0;
            int escalation = Math.Max(0, currentRank - 2);
            return (currentRank + 1 + escalation * escalation) * 8;
        }
        public int GetEnhancementCost(EquipmentSlot slot)
        {
            var item = GetEquipped(slot);
            return item == null ? 0 : EnhancementCostForRank(item.EnhancementRank);
        }
        public bool CanEnhance(EquipmentSlot slot) => GetEnhancementCost(slot) > 0 && TotalGold >= GetEnhancementCost(slot);
        public bool TryEnhanceEquipped() => TryEnhance(EquipmentSlot.Weapon);
        public bool TryEnhance(EquipmentSlot slot)
        {
            if (!CanEnhance(slot)) return false;
            WeaponItem current = GetEquipped(slot);
            var enhanced = new WeaponItem(current.Id, current.Name, current.FlatDamage, current.AffixDamage,
                current.AffixName, current.IconResource, current.Rarity, current.EnhancementRank + 1,
                current.EquipmentSlot, current.WeaponStyle, current.FlatDefense, current.FlatHealth,
                current.BaseCooldownReductionPercent, current.Options);
            TotalGold -= GetEnhancementCost(slot);
            SetEquipped(slot, enhanced);
            return true;
        }

        // Explicit discard has no currency reward. Pending loot is retained until picked up.
        public bool Discard(int index)
        {
            if (!ValidIndex(index)) return false;
            inventory.RemoveAt(index);
            return true;
        }

        public static int SalvageValue(WeaponItem item)
        {
            if (item == null) return 0;
            string rarity = (item.Rarity ?? string.Empty).Trim().ToLowerInvariant();
            int tier = rarity == "epic" ? 5 : rarity == "legend" ? 4 : rarity == "unique" ? 3 :
                rarity == "rare" ? 2 : rarity == "magic" ? 1 : 0;
            return 4 + tier * 4 + item.Options.Count * 2 + item.EnhancementRank * 2;
        }

        public int GetSalvageValue(int index) => ValidIndex(index) ? SalvageValue(inventory[index]) : 0;

        public bool Salvage(int index)
        {
            if (!ValidIndex(index)) return false;
            int value = SalvageValue(inventory[index]);
            if (value <= 0 || TotalGold > int.MaxValue - value || TotalSalvageGold > int.MaxValue - value) return false;
            inventory.RemoveAt(index);
            TotalGold += value;
            TotalSalvageGold += value;
            LastSalvageGold = value;
            return true;
        }

        public int? CompareDamage(int index) => ValidIndex(index)
            ? (int?)(inventory[index].DamageBonus - (GetEquipped(inventory[index].EquipmentSlot)?.DamageBonus ?? 0)) : null;

        public bool TrySpendPoint(TalentId talent)
        {
            if (!CanSpendPoint(talent)) return false;
            switch (talent)
            {
                case TalentId.Fury: FuryRank++; break;
                case TalentId.Precision: PrecisionRank++; break;
                case TalentId.Keystone: KeystoneRank++; break;
                case TalentId.Vitality: VitalityRank++; break;
                case TalentId.Cleave: CleaveRank++; break;
                case TalentId.Haste: HasteRank++; break;
                default: return false;
            }
            UnspentPoints--;
            return true;
        }
        public void ResetTalents()
        {
            UnspentPoints += SpentPoints;
            FuryRank = PrecisionRank = KeystoneRank = VitalityRank = CleaveRank = HasteRank = 0;
        }

        public ProgressionSnapshot CaptureSnapshot()
        {
            var items = new WeaponSnapshot[inventory.Count];
            for (int i = 0; i < items.Length; i++) items[i] = CaptureWeapon(inventory[i]);
            var tokens = new List<string>(killTokens);
            var ids = new List<string>(issuedItemIds);
            tokens.Sort(StringComparer.Ordinal);
            ids.Sort(StringComparer.Ordinal);
            return new ProgressionSnapshot
            {
                totalExperience = TotalExperience, totalGold = TotalGold, totalSalvageGold = TotalSalvageGold, unspentPoints = UnspentPoints,
                furyRank = FuryRank, precisionRank = PrecisionRank, keystoneRank = KeystoneRank,
                vitalityRank = VitalityRank, cleaveRank = CleaveRank, hasteRank = HasteRank, legacyPointCredit = legacyPointCredit,
                selectedDifficulty = (int)SelectedDifficulty, dungeonClears = DungeonClears,
                hasArmor = EquippedArmor != null, hasRelic = EquippedRelic != null,
                equippedArmor = CaptureWeapon(EquippedArmor), equippedRelic = CaptureWeapon(EquippedRelic),
                hasHelmet = EquippedHelmet != null, hasGloves = EquippedGloves != null, hasBoots = EquippedBoots != null,
                hasRing = EquippedRing != null, hasAmulet = EquippedAmulet != null,
                equippedHelmet = CaptureWeapon(EquippedHelmet), equippedGloves = CaptureWeapon(EquippedGloves),
                equippedBoots = CaptureWeapon(EquippedBoots), equippedRing = CaptureWeapon(EquippedRing),
                equippedAmulet = CaptureWeapon(EquippedAmulet),
                firstDropWaiting = firstDropWaiting, hasPendingLoot = PendingLoot != null, equippedWeapon = CaptureWeapon(EquippedWeapon),
                inventory = items, pendingLoot = CaptureWeapon(PendingLoot),
                killTokens = tokens.ToArray(), issuedItemIds = ids.ToArray()
            };
        }

        public static HeroProgression RestoreSnapshot(ProgressionSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (snapshot.schemaVersion != 1 && snapshot.schemaVersion != 2 && snapshot.schemaVersion != 3 && snapshot.schemaVersion != 4)
                throw new NotSupportedException("Unsupported progression snapshot version.");
            bool legacy = snapshot.schemaVersion == 1;
            bool current = snapshot.schemaVersion >= 3;
            bool salvageCurrent = snapshot.schemaVersion >= 4;
            int[] ranks = { snapshot.furyRank, snapshot.precisionRank, snapshot.keystoneRank,
                snapshot.vitalityRank, snapshot.cleaveRank, snapshot.hasteRank };
            int[] legacyCaps = { 2, 1, 1, 0, 0, 0 };
            if (snapshot.totalExperience < 0 || snapshot.totalGold < 0 || snapshot.totalSalvageGold < 0 ||
                (!salvageCurrent && snapshot.totalSalvageGold != 0) || snapshot.unspentPoints < 0 || snapshot.dungeonClears < 0 ||
                (current && !Enum.IsDefined(typeof(DungeonDifficulty), snapshot.selectedDifficulty)) ||
                (!current && (snapshot.selectedDifficulty != 0 || snapshot.dungeonClears != 0)) ||
                snapshot.legacyPointCredit < 0 || (legacy && (snapshot.hasArmor || snapshot.hasRelic || snapshot.hasHelmet ||
                snapshot.hasGloves || snapshot.hasBoots || snapshot.hasRing || snapshot.hasAmulet || snapshot.legacyPointCredit != 0)))
                throw new ArgumentException("Invalid balances or legacy fields.", nameof(snapshot));
            long spent = 0;
            for (int i = 0; i < ranks.Length; i++)
            {
                var definition = talentDefinitions[i];
                if (ranks[i] < 0 || ranks[i] > (legacy ? legacyCaps[i] : definition.MaxRank) ||
                    (ranks[i] > 0 && definition.Prerequisite.HasValue && ranks[(int)definition.Prerequisite.Value] < definition.RequiredRank))
                    throw new ArgumentException("Invalid talent rank or prerequisite.", nameof(snapshot));
                spent += ranks[i];
            }
            if (snapshot.inventory == null || snapshot.inventory.Length > InventoryCapacity || snapshot.equippedWeapon == null)
                throw new ArgumentException("An equipped weapon and a bounded inventory are required.", nameof(snapshot));

            HashSet<string> tokens = RestoreIdentifiers(snapshot.killTokens);
            HashSet<string> ids = RestoreIdentifiers(snapshot.issuedItemIds);
            long earnedPoints = snapshot.unspentPoints + spent;
            long normalPoints = EarnedTalentPointsForExperience(snapshot.totalExperience);
            long credit = current ? snapshot.legacyPointCredit : earnedPoints - normalPoints;
            long historicalPointBalance = normalPoints + credit;
            long cappedPointBalance = Math.Min(TotalTalentCapacity, historicalPointBalance);
            long minimumExperience = (long)tokens.Count * 25;
            long maximumExperience = (long)tokens.Count * (current ? 45 : legacy ? 25 : 30);
            long maximumGold = (long)tokens.Count * (current ? 17 : legacy ? 8 : 11);
            if (credit < 0 || credit > tokens.Count - normalPoints ||
                (earnedPoints != historicalPointBalance && earnedPoints != cappedPointBalance) || snapshot.dungeonClears > tokens.Count ||
                (legacy ? snapshot.totalExperience != minimumExperience : snapshot.totalExperience < minimumExperience || snapshot.totalExperience > maximumExperience) ||
                snapshot.totalSalvageGold > (long)ids.Count * 80 || snapshot.totalGold > maximumGold + snapshot.totalSalvageGold ||
                (legacy && snapshot.totalGold % 8 != 0))
                throw new ArgumentException("Reward balances disagree with the kill ledger.", nameof(snapshot));
            bool invalidLegacyFirstDrop = legacy && ((tokens.Count == 0 && (snapshot.firstDropWaiting || ids.Contains(FirstDropId))) ||
                (tokens.Count > 0 && snapshot.firstDropWaiting == ids.Contains(FirstDropId)) ||
                (snapshot.firstDropWaiting && !snapshot.hasPendingLoot));
            if (!ids.Contains("equipped:starting-sword") || invalidLegacyFirstDrop || (!legacy && snapshot.firstDropWaiting))
                throw new ArgumentException("Invalid first-drop or issued-item history.", nameof(snapshot));

            var ownedIds = new HashSet<string>(StringComparer.Ordinal);
            WeaponItem equipped = RestoreWeapon(snapshot.equippedWeapon, ids, ownedIds, legacy);
            WeaponItem armor = snapshot.hasArmor ? RestoreWeapon(snapshot.equippedArmor, ids, ownedIds, legacy) : null;
            WeaponItem relic = snapshot.hasRelic ? RestoreWeapon(snapshot.equippedRelic, ids, ownedIds, legacy) : null;
            WeaponItem helmet = snapshot.hasHelmet ? RestoreWeapon(snapshot.equippedHelmet, ids, ownedIds, legacy) : null;
            WeaponItem gloves = snapshot.hasGloves ? RestoreWeapon(snapshot.equippedGloves, ids, ownedIds, legacy) : null;
            WeaponItem boots = snapshot.hasBoots ? RestoreWeapon(snapshot.equippedBoots, ids, ownedIds, legacy) : null;
            WeaponItem ring = snapshot.hasRing ? RestoreWeapon(snapshot.equippedRing, ids, ownedIds, legacy) : null;
            WeaponItem amulet = snapshot.hasAmulet ? RestoreWeapon(snapshot.equippedAmulet, ids, ownedIds, legacy) : null;
            if (equipped.EquipmentSlot != EquipmentSlot.Weapon ||
                (armor != null && armor.EquipmentSlot != EquipmentSlot.Armor) ||
                (relic != null && relic.EquipmentSlot != EquipmentSlot.Relic) ||
                (helmet != null && helmet.EquipmentSlot != EquipmentSlot.Helmet) ||
                (gloves != null && gloves.EquipmentSlot != EquipmentSlot.Gloves) ||
                (boots != null && boots.EquipmentSlot != EquipmentSlot.Boots) ||
                (ring != null && ring.EquipmentSlot != EquipmentSlot.Ring) ||
                (amulet != null && amulet.EquipmentSlot != EquipmentSlot.Amulet))
                throw new ArgumentException("Item is in the wrong equipment slot.");
            var items = new List<WeaponItem>();
            foreach (WeaponSnapshot item in snapshot.inventory) items.Add(RestoreWeapon(item, ids, ownedIds, legacy));
            // Unity serializes inline null classes as empty objects; the explicit tag owns optionality.
            WeaponItem pending = snapshot.hasPendingLoot ? RestoreWeapon(snapshot.pendingLoot, ids, ownedIds, legacy) : null;

            var restored = new HeroProgression
            {
                TotalExperience = snapshot.totalExperience, TotalGold = snapshot.totalGold, TotalSalvageGold = snapshot.totalSalvageGold,
                UnspentPoints = snapshot.unspentPoints, FuryRank = snapshot.furyRank,
                PrecisionRank = snapshot.precisionRank, KeystoneRank = snapshot.keystoneRank,
                VitalityRank = snapshot.vitalityRank, CleaveRank = snapshot.cleaveRank, HasteRank = snapshot.hasteRank,
                legacyPointCredit = (int)credit, EquippedArmor = armor, EquippedRelic = relic,
                SelectedDifficulty = current ? (DungeonDifficulty)snapshot.selectedDifficulty : DungeonDifficulty.Scout,
                DungeonClears = current ? snapshot.dungeonClears : 0,
                EquippedHelmet = helmet, EquippedGloves = gloves, EquippedBoots = boots,
                EquippedRing = ring, EquippedAmulet = amulet,
                firstDropWaiting = snapshot.firstDropWaiting, EquippedWeapon = equipped, PendingLoot = pending
            };
            restored.inventory.AddRange(items);
            restored.killTokens.UnionWith(tokens);
            restored.issuedItemIds.Clear();
            restored.issuedItemIds.UnionWith(ids);
            return restored;
        }

        private static HashSet<string> RestoreIdentifiers(string[] values)
        {
            if (values == null) throw new ArgumentException("Identifier ledger is required.");
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values)
                if (string.IsNullOrWhiteSpace(value) || !result.Add(value))
                    throw new ArgumentException("Identifier ledger contains an empty or duplicate entry.");
            return result;
        }

        private static WeaponItem RestoreWeapon(WeaponSnapshot item, HashSet<string> issued, HashSet<string> owned, bool legacy)
        {
            if (item == null) throw new ArgumentException("Inventory cannot contain a null weapon.");
            if (legacy && (item.enhancementRank > 3 || item.equipmentSlot != 0 || item.weaponStyle != 0 ||
                item.flatDefense != 0 || item.flatHealth != 0 || item.cooldownReductionPercent != 0))
                throw new ArgumentException("Version-1 item contains unsupported fields.");
            switch (item.iconResource)
            {
                case "AffixGenerated/AttackIcon": case "AffixGenerated/EmberSword":
                case "AffixGenerated/PowerRune": case "AffixGenerated/PrecisionRune": case "AffixGenerated/VeteranRune":
                case "AffixGenerated/GearAxe": case "AffixGenerated/GearStaff":
                case "AffixGenerated/GearArmor": case "AffixGenerated/GearRelic":
                case "AffixUIVisual/GearHelmet": case "AffixUIVisual/GearGloves":
                case "AffixUIVisual/GearBoots": case "AffixUIVisual/GearRing":
                case "AffixUIVisual/GearAmulet": break;
                default: throw new ArgumentException("Item icon is not an authored resource.");
            }
            var options = new List<ItemOption>();
            if (item.options != null)
                foreach (ItemOptionSnapshot option in item.options)
                {
                    if (option == null || !Enum.IsDefined(typeof(AffixStat), option.stat))
                        throw new ArgumentException("Saved item option is invalid.");
                    options.Add(new ItemOption((AffixStat)option.stat, option.name, option.value));
                }
            var weapon = new WeaponItem(item.id, item.name, item.flatDamage, item.affixDamage,
                item.affixName, item.iconResource, item.rarity, item.enhancementRank,
                (EquipmentSlot)item.equipmentSlot, (WeaponStyle)item.weaponStyle, item.flatDefense, item.flatHealth,
                item.cooldownReductionPercent, options);
            if (!issued.Contains(weapon.Id) || !owned.Add(weapon.Id))
                throw new ArgumentException("Owned weapon identity is unissued or duplicated.");
            return weapon;
        }

        private static WeaponSnapshot CaptureWeapon(WeaponItem item) => item == null ? null : new WeaponSnapshot
        {
            id = item.Id, name = item.Name, flatDamage = item.FlatDamage, affixDamage = item.AffixDamage,
            affixName = item.AffixName, iconResource = item.IconResource, rarity = item.Rarity,
            enhancementRank = item.EnhancementRank, equipmentSlot = (int)item.EquipmentSlot, weaponStyle = (int)item.WeaponStyle,
            flatDefense = item.FlatDefense, flatHealth = item.FlatHealth, cooldownReductionPercent = item.BaseCooldownReductionPercent,
            options = CaptureOptions(item.Options)
        };

        private static ItemOptionSnapshot[] CaptureOptions(IReadOnlyList<ItemOption> options)
        {
            var result = new ItemOptionSnapshot[options.Count];
            for (int i = 0; i < result.Length; i++) result[i] = new ItemOptionSnapshot
            { stat = (int)options[i].Stat, name = options[i].Name, value = options[i].Value };
            return result;
        }

        private bool ValidIndex(int index) => index >= 0 && index < inventory.Count;
    }
}
