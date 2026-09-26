using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    // One disk writer per process/session. Scene reloads retain the same profile and file lock.
    public sealed class ProfilePersistence : IDisposable
    {
        private AtomicProfileStore store;
        private bool loadBlocked;
        public HeroProgression Profile { get; private set; } = new HeroProgression();
        public bool Ephemeral { get; private set; }
        public bool CanPlay => string.IsNullOrEmpty(Problem);
        public bool CanRetry => !loadBlocked && store != null;
        public string Problem { get; private set; } = "";
        public string Status { get; private set; } = "저장 준비";
        public string FilePath { get; private set; } = "";
        public string LoadNotice { get; private set; } = "";
        public int SaveCount { get; private set; }

        public ProfilePersistence()
        {
            try
            {
                string[] args=Environment.GetCommandLineArgs();
                bool saveTest=Array.IndexOf(args,"-affixSaveTest")>=0;
                // UI fixtures never open a save, even when incompatible test flags are supplied.
                Ephemeral=Array.IndexOf(args,"-affixUiReferenceTest")>=0 || (!saveTest &&
                    (Array.IndexOf(args,"-affixAutoHuntTest")>=0 || Array.IndexOf(args,"-affixAutoHuntSafetyTest")>=0 ||
                    Array.IndexOf(args,"-affixSmokeTest")>=0));
                // Editor verification also stays separate from the player's persistent character.
#if UNITY_EDITOR
                Ephemeral=true;
#endif
                if(Ephemeral){Status="검증용 임시 프로필";return;}
                string directory=Application.persistentDataPath;
                if(saveTest)
                {
                    int index=Array.IndexOf(args,"-affixSaveDir");
                    if(index<0 || index+1>=args.Length || !Path.IsPathRooted(args[index+1]))
                        throw new ArgumentException("Save verification requires an absolute -affixSaveDir.");
                    directory=Path.GetFullPath(args[index+1]);
                    string production=Path.GetFullPath(Application.persistentDataPath).TrimEnd(Path.DirectorySeparatorChar);
                    if(directory.TrimEnd(Path.DirectorySeparatorChar).Equals(production,StringComparison.OrdinalIgnoreCase))
                        throw new ArgumentException("Save verification cannot use the player's save directory.");
                }
                FilePath=Path.Combine(directory,"profile-v1.json");
                store=new AtomicProfileStore(FilePath,Validate);
                string json=store.Load(out string notice);LoadNotice=notice??"";
                if(json!=null){Profile=Decode(json);Status=string.IsNullOrEmpty(notice)?"저장 불러옴":"백업 복원 · 원본 보존";}
                else Status="새 영웅 · 자동 저장";
            }
            catch(Exception error)
            {
                loadBlocked=true;Problem=error.Message;Status="저장을 불러오지 못했습니다 · 파일 보존";
                Debug.LogWarning("Profile load blocked: "+error.Message);
            }
        }

        public bool Save()
        {
            if(Ephemeral)return true;
            if(loadBlocked)return false;
            try
            {
                store.Save(Encode(Profile));SaveCount++;Problem="";
                Status="자동 저장 완료 · "+DateTime.Now.ToString("HH:mm:ss");return true;
            }
            catch(Exception error)
            {Problem=error.Message;Status="저장 실패 · 사냥 중지 / 재시도";Debug.LogWarning("Profile save failed: "+error.Message);return false;}
        }
        public bool Retry()=>CanRetry && Save();
        public void Dispose(){store?.Dispose();store=null;}

        public static string Encode(HeroProgression profile)
        {
            string payload=JsonUtility.ToJson(profile.CaptureSnapshot());
            return JsonUtility.ToJson(new Envelope{format="AFFIX_PROFILE",schemaVersion=1,payload=payload,sha256=Hash(payload)},true);
        }
        public static HeroProgression Decode(string json)
        {
            Envelope data=JsonUtility.FromJson<Envelope>(json);
            if(data==null || data.format!="AFFIX_PROFILE")throw new InvalidDataException("Invalid profile envelope.");
            if(data.schemaVersion!=1)throw new NotSupportedException("Unsupported profile format version.");
            if(string.IsNullOrEmpty(data.payload) || string.IsNullOrEmpty(data.sha256))throw new InvalidDataException("Incomplete profile envelope.");
            if(!string.Equals(data.sha256,Hash(data.payload),StringComparison.Ordinal))throw new InvalidDataException("Profile checksum differs.");
            return HeroProgression.RestoreSnapshot(JsonUtility.FromJson<ProgressionSnapshot>(data.payload));
        }
        private static bool Validate(string json)
        {
            try{Decode(json);return true;}
            catch(NotSupportedException){throw;}
            catch(ArgumentException){return false;}
            catch(InvalidDataException){return false;}
            catch(InvalidOperationException){return false;}
        }
        private static string Hash(string value)
        {
            using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Replace("-","").ToLowerInvariant();
        }
        [Serializable] private sealed class Envelope
        {public string format,payload,sha256;public int schemaVersion;}
    }
}
