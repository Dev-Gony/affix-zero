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
        private static HeroProgression sessionProgression;
        private GameObject lootView;
        private Sprite lootSprite;
        public HeroProgression Progression => sessionProgression ?? (sessionProgression = new HeroProgression());
        public string ProgressionNotice { get; private set; } = "";
        public int Experience => rewards == null ? 0 : rewards.Experience;
        public int Gold => rewards == null ? 0 : rewards.Gold;
        public int RewardCollectionCount => rewards == null ? 0 : rewards.CollectionCount;
        public MeleeActor Hero => hero;
        public MeleeActor Enemy => enemy;
        public bool IsPaused => manuallyPaused || ManagementVisible;
        public ManagementScreen Screen { get; private set; }
        public bool EquipmentVisible => Screen == ManagementScreen.Equipment;
        public bool ManagementVisible => Screen != ManagementScreen.None;
        public float ElapsedSeconds { get; private set; }
        public bool HasEnded => (hero != null && hero.IsDead) || (enemy != null && enemy.IsDead);
        public float SecondsSinceEnd => endedAt < 0 ? 0 : Time.unscaledTime - endedAt;

        public void Configure(MeleeActor player, MeleeActor opponent) { hero = player; enemy = opponent; }

        private void Awake()
        {
            previousTimeScale = Time.timeScale;
            ApplyBuild();
            var hud = GetComponent<EncounterHud>();
            if (hud == null) hud = gameObject.AddComponent<EncounterHud>();
            hud.Configure(this);
        }

        private void Start()
        {
            rewards = new EncounterRewards(Guid.NewGuid().ToString("N"));
            if (enemy != null) enemy.Damaged += OnEnemyDamaged;
            if (Progression.PendingLoot != null) ShowLoot();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BeginSession() { sessionProgression = null; }

        private void ApplyBuild()
        {
            if (hero != null) hero.SetAttackDamage(Progression.TotalDamage);
        }

        public bool CollectLoot()
        {
            string name = Progression.PendingLoot?.Name;
            if (!Progression.PickUp())
            {
                ProgressionNotice = Progression.PendingLoot == null ? "회수할 전리품이 없습니다." : "가방이 가득 찼습니다. 먼저 공간을 비우세요.";
                return false;
            }
            ProgressionNotice = name + " 획득 · 가방에서 비교할 수 있습니다.";
            ClearLootView();
            if (Progression.PendingLoot != null) ShowLoot();
            return true;
        }

        public bool EquipItem(int index)
        {
            if (!Progression.Equip(index)) { ProgressionNotice = "장착할 아이템을 선택하세요."; return false; }
            ApplyBuild();
            ProgressionNotice = Progression.EquippedWeapon.Name + " 장착 · 다음 공격부터 적용";
            return true;
        }

        public bool DiscardItem(int index)
        {
            if (!Progression.Discard(index)) { ProgressionNotice = "버릴 아이템을 선택하세요."; return false; }
            ProgressionNotice = "선택한 아이템을 버렸습니다.";
            return true;
        }

        public bool SpendTalent(TalentId talent)
        {
            if (!Progression.TrySpendPoint(talent)) { ProgressionNotice = "남은 포인트와 선행 특성을 확인하세요."; return false; }
            ApplyBuild();
            ProgressionNotice = "특성 적용 · 다음 공격부터 피해 증가";
            return true;
        }

        public void ResetTalents()
        {
            Progression.ResetTalents();
            ApplyBuild();
            ProgressionNotice = "특성을 초기화하고 사용한 포인트를 돌려받았습니다.";
        }

        public bool EnhanceWeapon()
        {
            if (!Progression.TryEnhanceEquipped())
            {
                ProgressionNotice = Progression.EquippedWeapon.EnhancementRank >= WeaponItem.MaxEnhancementRank
                    ? "최대 강화 단계입니다." : "강화에 필요한 골드가 부족합니다.";
                return false;
            }
            ApplyBuild();
            ProgressionNotice = "+" + Progression.EquippedWeapon.EnhancementRank + " 강화 완료 · 다음 공격부터 피해 +2";
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
            if (!HasEnded && hero != null && enemy != null && hero.IsReady && enemy.IsReady)
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
            if (enemy != null) enemy.Damaged -= OnEnemyDamaged;
            ClearLootView();
            Time.timeScale = previousTimeScale;
        }

        private void OnEnemyDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (receipt.Killed && hero != null && !hero.IsDead &&
                rewards.TryCollect(rewards.EncounterId, actor.ActorId, actor.DeathCount, 25, 8))
            {
                Progression.TryRegisterKill(rewards.EncounterId + "/" + actor.ActorId + "/" + actor.DeathCount);
                ProgressionNotice = Progression.PendingLoot != null ? "전리품 발견 · 회수 후 장비를 비교하세요." : "특성 포인트 +1";
                ShowLoot();
            }
        }

        private void ShowLoot()
        {
            if (lootView != null || Progression.PendingLoot == null) return;
            var texture = Resources.Load<Texture2D>(Progression.PendingLoot.IconResource);
            if (texture == null) { Debug.LogError("Loot icon resource is missing.", this); return; }
            lootSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 24);
            lootView = new GameObject("Dropped Weapon", typeof(SpriteRenderer));
            lootView.transform.position = (enemy != null ? enemy.transform.position : transform.position) + Vector3.up * .65f;
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
