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
                var fresh=new HeroProgression();RoundTrip(fresh);
                fresh.TryRegisterKill("serialization/first");RoundTrip(fresh);
                fresh.PickUp();fresh.Equip(0);fresh.TrySpendPoint(TalentId.Fury);fresh.TryEnhanceEquipped();RoundTrip(fresh);
                bool unsupported=false;
                try{ProfilePersistence.Decode("{\"format\":\"AFFIX_PROFILE\",\"schemaVersion\":2}");}
                catch(NotSupportedException){unsupported=true;}
                if(!unsupported)throw new InvalidOperationException("Future schema was treated as ordinary corruption.");
                Directory.CreateDirectory("Build/Reports");
                File.WriteAllText("Build/Reports/profile-serialization.json","{\"result\":\"PASS\",\"checks\":4,\"scope\":\"Actual Unity JsonUtility fresh, pending and equipped/enhanced roundtrips plus future format rejection\"}");
            }
            catch(Exception error){Debug.LogError(error);EditorApplication.Exit(1);return;}
            EncounterVerification.BuildWindows();
        }
        private static void RoundTrip(HeroProgression profile)
        {
            string json=ProfilePersistence.Encode(profile);
            var restored=ProfilePersistence.Decode(json);
            if(ProfilePersistence.Encode(restored)!=json)throw new InvalidOperationException("Unity profile roundtrip changed state.");
        }
    }
}
#endif
