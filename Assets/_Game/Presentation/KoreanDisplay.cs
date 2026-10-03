using System;
using System.Collections.Generic;
using AffixZero.Core;

namespace AffixZero.Presentation
{
    // Korean is the current product language. Stable IDs, enum names and save payloads stay unchanged
    // so a future English catalog can replace this presentation-only mapping without a migration.
    public static class KoreanDisplay
    {
        private const string ItemPrefix = "AffixUIVisual/Items/";
        private static readonly Dictionary<string, string> ItemNames = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            {"dagger","단검"},{"longsword","장검"},{"axe","도끼"},{"magic_sword","비전 지팡이"},{"divine_sword","신검"},
            {"leather_hat","가죽 모자"},{"iron_helm","철 투구"},{"mithril_helm","미스릴 투구"},{"dragon_helm","용의 투구"},
            {"cloth","천 갑옷"},{"leather_armor","가죽 갑옷"},{"plate_armor","판금 갑옷"},{"dragonscale","용린 갑옷"},
            {"cloth_gloves","천 장갑"},{"leather_gloves","가죽 장갑"},{"battle_gloves","전투 장갑"},{"dragon_gloves","용의 장갑"},
            {"sandals","샌들"},{"leather_boots","가죽 장화"},{"swift_boots","신속 장화"},{"gale_boots","질풍 장화"},
            {"copper_ring","구리 반지"},{"silver_ring","은 반지"},{"gold_ring","금 반지"},{"diamond_ring","다이아 반지"},
            {"bone_necklace","뼈 목걸이"},{"crystal_necklace","수정 목걸이"},{"ruby_necklace","루비 목걸이"},{"dragon_tear","용의 눈물"}
        };

        private static readonly Dictionary<string, string> RuneNames = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            {"ember","불씨 룬"},{"bastion","보루 룬"},{"vital","생명 룬"},{"aether","에테르 룬"},{"gale","질풍 룬"},
            {"keen","예리함 룬"},{"blood","피의 룬"},{"scholar","현자의 룬"},{"fortune","행운 룬"},{"piercing","관통 룬"}
        };

        public static string Difficulty(DungeonDifficulty value)
        {
            switch (value)
            {
                case DungeonDifficulty.Veteran: return "숙련";
                case DungeonDifficulty.Torment: return "고행";
                default: return "정찰";
            }
        }

        public static string Layout(DungeonLayoutId value)
        {
            switch (value)
            {
                case DungeonLayoutId.Galleries: return "분단 회랑";
                case DungeonLayoutId.Crucible: return "의식의 도가니";
                default: return "잿불 요새";
            }
        }

        public static string VisualIdentity(DungeonLayoutId value)
        {
            switch (value)
            {
                case DungeonLayoutId.Galleries: return "침수 기록고";
                case DungeonLayoutId.Crucible: return "일식 성소";
                default: return "대장간 성채";
            }
        }

        public static string Boss(DungeonLayoutId value)
        {
            switch (value)
            {
                case DungeonLayoutId.Galleries: return "익사한 집정관";
                case DungeonLayoutId.Crucible: return "일식의 군주";
                default: return "잿빛 폭군";
            }
        }

        public static string ItemName(WeaponItem item)
        {
            if (item == null) return string.Empty;
            if (!string.IsNullOrEmpty(item.IconResource) && item.IconResource.StartsWith(ItemPrefix, StringComparison.Ordinal))
            {
                string id = item.IconResource.Substring(ItemPrefix.Length);
                if (ItemNames.TryGetValue(id, out string name)) return name;
            }
            return item.Name;
        }

        public static string Rarity(string id)
        {
            switch ((id ?? string.Empty).ToLowerInvariant())
            {
                case "magic": return "매직";
                case "rare": return "레어";
                case "unique": return "유니크";
                case "legend": return "전설";
                case "epic": return "에픽";
                default: return "일반";
            }
        }

        public static string Slot(EquipmentSlot value)
        {
            switch (value)
            {
                case EquipmentSlot.Weapon: return "무기";
                case EquipmentSlot.Armor: return "방어구";
                case EquipmentSlot.Relic: return "유물";
                case EquipmentSlot.Helmet: return "투구";
                case EquipmentSlot.Gloves: return "장갑";
                case EquipmentSlot.Boots: return "장화";
                case EquipmentSlot.Ring: return "반지";
                case EquipmentSlot.Amulet: return "목걸이";
                default: return "장비";
            }
        }

        public static string Stat(AffixStat value)
        {
            switch (value)
            {
                case AffixStat.Attack: return "공격력";
                case AffixStat.Defense: return "방어력";
                case AffixStat.Health: return "최대 체력";
                case AffixStat.Mana: return "마나";
                case AffixStat.Speed: return "공격 속도";
                case AffixStat.Critical: return "치명타 확률";
                case AffixStat.Vampirism: return "흡혈";
                case AffixStat.Experience: return "경험치 획득";
                case AffixStat.Gold: return "골드 획득";
                case AffixStat.Penetration: return "방어 관통";
                default: return value.ToString();
            }
        }

        public static string RuneName(string id)
        {
            return id != null && RuneNames.TryGetValue(id, out string name) ? name : "알 수 없는 룬";
        }

        public static string RuneEffect(RuneDefinition rune)
        {
            if (rune == null) return string.Empty;
            string value = rune.Stat == AffixStat.Speed ? (rune.Value * 100f).ToString("0") + "%" : rune.Value.ToString("0.#");
            return Stat(rune.Stat) + " +" + value;
        }

        public static string Skill(string value)
        {
            switch (value ?? string.Empty)
            {
                case "ARC": return "연쇄 참격";
                case "ARC II": return "연쇄 참격 II";
                case "LANCE": return "관통 창";
                case "LANCE II": return "관통 창 II";
                case "QUAKE": return "충격파";
                case "QUAKE II": return "충격파 II";
                case "TEMPEST": return "폭풍";
                case "WHIRL": return "회오리";
                case "DEVASTATE": return "분쇄";
                case "REND": return "열상";
                case "QUICKCAST": return "속전";
                case "AREA": return "범위 참격";
                case "SURGE": return "생명 쇄도";
                case "WIND": return "생명의 바람";
                case "HEAL": return "자동 회복";
                default: return value ?? string.Empty;
            }
        }
    }
}
