#if UNITY_EDITOR
using System;
using System.IO;
using AffixZero.Core;
using AffixZero.Presentation;
using UnityEditor;
using UnityEngine;

namespace AffixZero.Editor
{
    public static class ProfileSerializationVerification
    {
        public static void RunAndBuild()
        {
            try
            {
                var fresh = new HeroProgression(); RoundTrip(fresh);
                fresh.TryRegisterKill("serialization/first"); RoundTrip(fresh);
                fresh.PickUp(); fresh.Equip(0); fresh.TryEnhanceEquipped();
                for (int i = 0; i < 19; i++) fresh.TryRegisterKill("serialization/level/" + i);
                if (!fresh.TrySpendPoint(TalentId.Fury) || !fresh.TrySpendPoint(TalentId.Vitality))
                    throw new InvalidOperationException("Level points were not available at 500 XP.");
                RoundTrip(fresh);
                EquipNew(fresh, new WeaponItem("serialization/armor", "수호 갑옷", 0, 0, "", "AffixGenerated/GearArmor", "Rare", 4,
                    EquipmentSlot.Armor, flatDefense: 3, flatHealth: 30));
                EquipNew(fresh, new WeaponItem("serialization/relic", "가속 유물", 4, 2, "격노", "AffixGenerated/GearRelic", "Rare", 5,
                    EquipmentSlot.Relic, cooldownReductionPercent: 12));
                EquipNew(fresh, new WeaponItem("serialization/staff", "사원 지팡이", 15, 3, "날카로움", "AffixGenerated/GearStaff", "Rare", 6,
                    EquipmentSlot.Weapon, WeaponStyle.Staff));
                var expanded = RoundTrip(fresh);
                if (!expanded.IsRanged || expanded.TotalDefense != 9 || expanded.TotalMaxHp != 180 || expanded.CooldownReductionPercent != 12)
                    throw new InvalidOperationException("Expanded equipment lost real combat parameters.");
                var legacy = CreateLegacyFixture();
                var migrated = HeroProgression.RestoreSnapshot(JsonUtility.FromJson<ProgressionSnapshot>(JsonUtility.ToJson(legacy)));
                if (migrated.LegacyPointCredit != 6 || migrated.UnspentPoints != 2 || migrated.SpentPoints != 4 ||
                    migrated.TotalGold != 24 || migrated.TotalDamage != 60 || migrated.EquippedArmor != null || migrated.EquippedRelic != null)
                    throw new InvalidOperationException("Legacy progression migration lost state.");
                RoundTrip(migrated);
                bool unsupported = false;
                try { ProfilePersistence.Decode("{\"format\":\"AFFIX_PROFILE\",\"schemaVersion\":2}"); }
                catch (NotSupportedException) { unsupported = true; }
                if (!unsupported) throw new InvalidOperationException("Future envelope schema was treated as ordinary corruption.");
                Directory.CreateDirectory("Build/Reports");
                File.WriteAllText("Build/Reports/profile-serialization.json",
                    "{\"result\":\"PASS\",\"checks\":6,\"payloadSchema\":2,\"envelopeSchema\":1,\"scope\":\"Actual Unity JsonUtility fresh, pending, enhanced, all-slot/style and exact legacy-v1 wire migration roundtrips; future envelope rejection\"}");
            }
            catch (Exception error) { Debug.LogError(error); EditorApplication.Exit(1); return; }
            EncounterVerification.BuildWindows();
        }
        private static void EquipNew(HeroProgression profile, WeaponItem item)
        {
            int index = profile.Inventory.Count;
            if (!profile.TryCreatePendingLoot(item) || !profile.PickUp() || !profile.Equip(index))
                throw new InvalidOperationException("Serialization fixture equipment intake failed.");
        }
        private static HeroProgression RoundTrip(HeroProgression profile)
        {
            string json = ProfilePersistence.Encode(profile);
            var restored = ProfilePersistence.Decode(json);
            if (ProfilePersistence.Encode(restored) != json) throw new InvalidOperationException("Unity profile roundtrip changed state.");
            return restored;
        }
        private static LegacyProgression CreateLegacyFixture()
        {
            var source = new HeroProgression();
            for (int i = 0; i < 6; i++) source.TryRegisterKill("serialization/legacy/" + i);
            source.PickUp(); source.Equip(0); source.TryEnhanceEquipped(); source.TryEnhanceEquipped();
            var snapshot = source.CaptureSnapshot();
            return new LegacyProgression
            {
                schemaVersion = 1, totalExperience = 150, totalGold = 24, unspentPoints = 2,
                furyRank = 2, precisionRank = 1, keystoneRank = 1,
                equippedWeapon = LegacyWeapon.From(snapshot.equippedWeapon),
                inventory = new[] { LegacyWeapon.From(snapshot.inventory[0]) },
                killTokens = snapshot.killTokens, issuedItemIds = snapshot.issuedItemIds
            };
        }
        // Exact old wire fields: missing v2 fields must deserialize safely, rather than relying on v2 initializers.
        [Serializable] private sealed class LegacyProgression
        {
            public int schemaVersion, totalExperience, totalGold, unspentPoints, furyRank, precisionRank, keystoneRank;
            public bool firstDropWaiting = false, hasPendingLoot = false;
            public LegacyWeapon equippedWeapon, pendingLoot = null;
            public LegacyWeapon[] inventory;
            public string[] killTokens, issuedItemIds;
        }
        [Serializable] private sealed class LegacyWeapon
        {
            public string id, name, affixName, iconResource, rarity;
            public int flatDamage, affixDamage, enhancementRank;
            public static LegacyWeapon From(WeaponSnapshot source) => new LegacyWeapon
            {
                id = source.id, name = source.name, affixName = source.affixName, iconResource = source.iconResource,
                rarity = source.rarity, flatDamage = source.flatDamage, affixDamage = source.affixDamage, enhancementRank = source.enhancementRank
            };
        }
    }
}
#endif
