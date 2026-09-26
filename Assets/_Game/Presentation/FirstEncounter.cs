using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AffixZero.Presentation
{
    public enum ManagementScreen { None, Equipment, Talents, Forge }

    public sealed class FirstEncounter : MonoBehaviour
    {
        [SerializeField] private MeleeActor hero;
        [SerializeField] private MeleeActor enemy;
        private EncounterRewards rewards;
        private bool manuallyPaused;
        private float previousTimeScale;
        private float endedAt = -1;
        private static ProfilePersistence persistence;
        private GameObject lootView;
        private Sprite lootSprite;
        public Vector2 LootPosition { get; private set; } = new Vector2(-10,-4);
        public AutoHuntDirector Hunt { get; private set; }
        public ProfilePersistence Persistence => persistence ?? (persistence = new ProfilePersistence());
        public HeroProgression Progression => Persistence.Profile;
        public bool CanProgress => Persistence.CanPlay;
        public string ProgressionNotice { get; private set; } = "";
        public int Experience => rewards == null ? 0 : rewards.Experience;
        public int Gold => rewards == null ? 0 : rewards.Gold;
        public int RewardCollectionCount => rewards == null ? 0 : rewards.CollectionCount;
        public MeleeActor Hero => hero;
        public MeleeActor Enemy => enemy;
        public bool IsPaused => manuallyPaused || !CanProgress || (Hunt!=null && !Hunt.Running);
        public ManagementScreen Screen { get; private set; }
        public bool EquipmentVisible => Screen == ManagementScreen.Equipment;
        public bool ManagementVisible => Screen != ManagementScreen.None;
        public float ElapsedSeconds { get; private set; }
        public bool HasEnded => hero != null && hero.IsDead;
        public float SecondsSinceEnd => endedAt < 0 ? 0 : Time.unscaledTime - endedAt;

        public void Configure(MeleeActor player, MeleeActor opponent) { hero = player; enemy = opponent; }

        private void Awake()
        {
            previousTimeScale = Time.timeScale;
            ApplyBuild();
            hero.SetTarget(null);enemy.SetTarget(null);
            Hunt=gameObject.AddComponent<AutoHuntDirector>();
            SetPaused(true);
            var hud = GetComponent<EncounterHud>();
            if (hud == null) hud = gameObject.AddComponent<EncounterHud>();
            hud.Configure(this);
        }

        private void Start()
        {
            rewards = new EncounterRewards(Guid.NewGuid().ToString("N"));
            if (Progression.PendingLoot != null) ShowLoot();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BeginSession() { persistence?.Dispose(); persistence = null; }

        private void OnApplicationQuit(){if(persistence!=null){if(persistence.CanPlay)persistence.Save();persistence.Dispose();}}
        private void OnApplicationPause(bool paused){if(paused && persistence!=null && persistence.CanPlay)SaveProgress();}
        public bool SaveProgress()
        {
            bool saved=Persistence.Save();
            if(!saved){Hunt?.StopHunt();ApplyPause();}
            return saved;
        }
        public void RetrySave(){Persistence.Retry();ApplyPause();}

        private void ApplyBuild()
        {
            if (hero != null) hero.SetAttackDamage(Progression.TotalDamage);
        }

        public bool CollectLoot()
        {
            if(!CanProgress)return false;
            string name = Progression.PendingLoot?.Name;
            if (!Progression.PickUp())
            {
                ProgressionNotice = Progression.PendingLoot == null ? "회수할 전리품이 없습니다." : "가방이 가득 찼습니다. 먼저 공간을 비우세요.";
                return false;
            }
            ProgressionNotice = name + " 획득 · 가방에서 비교할 수 있습니다.";
            ClearLootView();
            if (Progression.PendingLoot != null) ShowLoot();
            SaveProgress();
            return true;
        }

        public bool EquipItem(int index)
        {
            if(!CanProgress)return false;
            if (!Progression.Equip(index)) { ProgressionNotice = "장착할 아이템을 선택하세요."; return false; }
            ApplyBuild();
            ProgressionNotice = Progression.EquippedWeapon.Name + " 장착 · 다음 공격부터 적용";
            SaveProgress();
            return true;
        }

        public bool DiscardItem(int index)
        {
            if(!CanProgress)return false;
            if (!Progression.Discard(index)) { ProgressionNotice = "버릴 아이템을 선택하세요."; return false; }
            ProgressionNotice = "선택한 아이템을 버렸습니다.";
            SaveProgress();
            return true;
        }

        public bool SpendTalent(TalentId talent)
        {
            if(!CanProgress)return false;
            if (!Progression.TrySpendPoint(talent)) { ProgressionNotice = "남은 포인트와 선행 특성을 확인하세요."; return false; }
            ApplyBuild();
            ProgressionNotice = "특성 적용 · 다음 공격부터 피해 증가";
            SaveProgress();
            return true;
        }

        public void ResetTalents()
        {
            if(!CanProgress)return;
            Progression.ResetTalents();
            ApplyBuild();
            ProgressionNotice = "특성을 초기화하고 사용한 포인트를 돌려받았습니다.";
            SaveProgress();
        }

        public bool EnhanceWeapon()
        {
            if(!CanProgress)return false;
            if (!Progression.TryEnhanceEquipped())
            {
                ProgressionNotice = Progression.EquippedWeapon.EnhancementRank >= WeaponItem.MaxEnhancementRank
                    ? "최대 강화 단계입니다." : "강화에 필요한 골드가 부족합니다.";
                return false;
            }
            ApplyBuild();
            ProgressionNotice = "+" + Progression.EquippedWeapon.EnhancementRank + " 강화 완료 · 다음 공격부터 피해 +2";
            SaveProgress();
            return true;
        }

        public bool NextEncounter()
        {
            if (!HasEnded) { ProgressionNotice = "전투가 끝난 뒤 이동할 수 있습니다."; return false; }
            if (Progression.PendingLoot != null) { ProgressionNotice = "전리품을 먼저 회수하세요."; return false; }
            RestartEncounter();
            return true;
        }

        private void Update()
        {
            if (Hunt!=null && Hunt.Running && !HasEnded && hero != null && hero.IsReady)
                ElapsedSeconds += Time.deltaTime;
            if (HasEnded && endedAt < 0) endedAt = Time.unscaledTime;
        }

        public void SetPaused(bool paused)
        {
            manuallyPaused = paused;
            ApplyPause();
        }

        public void SetEquipmentVisible(bool visible)
        {
            ShowManagement(visible ? ManagementScreen.Equipment : ManagementScreen.None);
        }

        public void ShowManagement(ManagementScreen screen)
        {
            if (screen < ManagementScreen.None || screen > ManagementScreen.Forge) return;
            Screen = screen;
            ApplyPause();
        }

        private void ApplyPause() => Time.timeScale = IsPaused ? 0 : previousTimeScale;

        private void OnDestroy()
        {
            ClearLootView();
            Time.timeScale = previousTimeScale;
        }

        public void FocusEnemy(MeleeActor actor) { if(actor!=null)enemy=actor; }

        public void BeginAutoRun()
        {
            rewards=new EncounterRewards(Guid.NewGuid().ToString("N"));
            endedAt=-1;
            // A failed run never deletes an uncollected weapon. Recover it at the entrance.
            if(Progression.PendingLoot!=null){LootPosition=new Vector2(-10,-4);ClearLootView();ShowLoot();}
        }

        public bool RegisterDefeat(MeleeActor actor)
        {
            if(!CanProgress)return false;
            if (actor.IsDead && hero != null && !hero.IsDead &&
                rewards.TryCollect(rewards.EncounterId, actor.ActorId, actor.DeathCount, 25, 8))
            {
                if(!Progression.TryRegisterKill(rewards.EncounterId + "/" + actor.ActorId + "/" + actor.DeathCount))return false;
                ProgressionNotice = Progression.PendingLoot != null ? "전리품 발견 · 자동 회수 중" : "특성 포인트 +1";
                if(lootView==null)LootPosition=actor.transform.position;
                ShowLoot();
                SaveProgress();
                return true;
            }
            return false;
        }

        public void OfferClearLoot(int completedRuns,Vector2 position)
        {
            if(!CanProgress)return;
            int roll=completedRuns%3;
            var item=new WeaponItem("clear:"+rewards.EncounterId,"사원의 강철검",11+roll,2+roll,
                roll==0?"날카로움":roll==1?"잿불":"묵직함","AffixGenerated/EmberSword","Rare");
            if(Progression.TryCreatePendingLoot(item))
            {LootPosition=position;ProgressionNotice="던전 보상 발견 · 자동 회수 중";ShowLoot();SaveProgress();}
        }

        private void ShowLoot()
        {
            if (lootView != null || Progression.PendingLoot == null) return;
            var texture = Resources.Load<Texture2D>(Progression.PendingLoot.IconResource);
            if (texture == null) { Debug.LogError("Loot icon resource is missing.", this); return; }
            lootSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 24);
            lootView = new GameObject("Dropped Weapon", typeof(SpriteRenderer));
            lootView.transform.position = (Vector3)LootPosition + Vector3.up * .3f;
            var renderer = lootView.GetComponent<SpriteRenderer>();
            renderer.sprite = lootSprite;
            renderer.sortingOrder = 250;
        }

        private void ClearLootView()
        {
            if (lootView != null) Destroy(lootView);
            if (lootSprite != null) Destroy(lootSprite);
            lootView = null; lootSprite = null;
        }

        public void RestartEncounter()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex < 0)
            {
                Debug.LogError("Add the encounter scene to Build Settings before restarting.", this);
                return;
            }
            manuallyPaused = false;
            Screen = ManagementScreen.None;
            Time.timeScale = previousTimeScale;
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
