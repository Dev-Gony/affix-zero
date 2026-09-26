using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AffixZero.Presentation
{
    public sealed class FirstEncounter : MonoBehaviour
    {
        [SerializeField] private MeleeActor hero;
        [SerializeField] private MeleeActor enemy;
        private EncounterRewards rewards;
        private bool manuallyPaused;
        private float previousTimeScale;
        private float endedAt = -1;
        public int Experience => rewards == null ? 0 : rewards.Experience;
        public int Gold => rewards == null ? 0 : rewards.Gold;
        public int RewardCollectionCount => rewards == null ? 0 : rewards.CollectionCount;
        public MeleeActor Hero => hero;
        public MeleeActor Enemy => enemy;
        public bool IsPaused => manuallyPaused || EquipmentVisible;
        public bool EquipmentVisible { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public bool HasEnded => (hero != null && hero.IsDead) || (enemy != null && enemy.IsDead);
        public float SecondsSinceEnd => endedAt < 0 ? 0 : Time.unscaledTime - endedAt;

        public void Configure(MeleeActor player, MeleeActor opponent) { hero = player; enemy = opponent; }

        private void Awake()
        {
            previousTimeScale = Time.timeScale;
            var hud = GetComponent<EncounterHud>();
            if (hud == null) hud = gameObject.AddComponent<EncounterHud>();
            hud.Configure(this);
        }

        private void Start()
        {
            rewards = new EncounterRewards(Guid.NewGuid().ToString("N"));
            if (enemy != null) enemy.Damaged += OnEnemyDamaged;
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
            EquipmentVisible = visible;
            ApplyPause();
        }

        private void ApplyPause() => Time.timeScale = IsPaused ? 0 : previousTimeScale;

        private void OnDestroy()
        {
            if (enemy != null) enemy.Damaged -= OnEnemyDamaged;
            Time.timeScale = previousTimeScale;
        }

        private void OnEnemyDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (receipt.Killed && hero != null && !hero.IsDead)
                rewards.TryCollect(rewards.EncounterId, actor.ActorId, actor.DeathCount, 25, 8);
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
            EquipmentVisible = false;
            Time.timeScale = previousTimeScale;
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
