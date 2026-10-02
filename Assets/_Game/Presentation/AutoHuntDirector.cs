using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    public enum HuntPhase { Waiting, Exploring, Fighting, Collecting, Returning, Resting, Recovering, Blocked }

    // All monsters exist in one connected dungeon from the start. No arrival-triggered two-enemy sections.
    [DefaultExecutionOrder(-100)]
    public sealed class AutoHuntDirector : MonoBehaviour
    {
        private const int Population = 24;
        private const float AggroRadius = 6.5f;
        private FirstEncounter owner;
        private readonly List<MeleeActor> enemies = new List<MeleeActor>();
        private readonly List<int> roamSteps = new List<int>();
        private readonly List<float> roamAt = new List<float>();
        private MeleeActor hero;
        private AutoAreaSkill areaSkill;
        private AutoRecoverySkill recoverySkill;
        private CombatFeedback feedback;
        private float delay, stalledTime, decisionTime, enemyDecisionTime, recoveryUntil;
        private float lastKillAt = -100;
        private Vector2 lastPosition, progressPosition;
        private int identity = 10, stallAttempts;
        private bool runStarted;
        private HuntPhase phaseBeforeLoot;
        public DungeonWorld World { get; private set; }
        public IReadOnlyList<MeleeActor> Enemies => enemies.AsReadOnly();
        public bool Initialized { get; private set; }
        public bool Running { get; private set; }
        public HuntPhase Phase { get; private set; } = HuntPhase.Waiting;
        public int SectionIndex { get; private set; }
        public int CompletedRuns { get; private set; }
        public int TotalKills { get; private set; }
        public int CollectedItems { get; private set; }
        public int DeathRetries { get; private set; }
        public int FailedRuns { get; private set; }
        public int ConsecutiveFailures { get; private set; }
        public int TargetSelections { get; private set; }
        public int MaxAliveEnemies { get; private set; }
        public int MultiHitAttacks { get; private set; }
        public int MaxKillsPerStrike { get; private set; }
        public int StallRecoveries { get; private set; }
        public int LayoutTransitions { get; private set; }
        public int LayoutsVisitedMask { get; private set; }
        public int EliteKills { get; private set; }
        public int GuardianKills { get; private set; }
        public int DefeatedElitePatternMask { get; private set; }
        public int KillChain { get; private set; }
        public int MaxKillChain { get; private set; }
        public DungeonDifficulty CurrentDifficulty => owner == null ? DungeonDifficulty.Scout : owner.Progression.SelectedDifficulty;
        public DifficultyRule Difficulty => DifficultyTuning.Get(CurrentDifficulty);
        public string CurrentLayoutName => World == null ? "LOADING" : World.LayoutName;
        public bool CanChangeDifficulty => Initialized && !Running && owner != null && owner.Progression.PendingLoot == null;
        public bool ReducedEffects => feedback != null && feedback.ReducedEffects;
        public int AreaCasts => areaSkill == null ? 0 : areaSkill.CastCount;
        public float AreaCooldownRemaining => areaSkill == null ? 0 : areaSkill.CooldownRemaining;
        public float AreaCooldownDuration => areaSkill == null ? 4.5f : areaSkill.CooldownDuration;
        public int RecoveryCasts => recoverySkill == null ? 0 : recoverySkill.CastCount;
        public float RecoveryCooldownRemaining => recoverySkill == null ? 0 : recoverySkill.CooldownRemaining;
        public float RecoveryCooldownDuration => recoverySkill == null ? 14f : recoverySkill.CooldownDuration;
        public string AreaSkillName => "회전 참격";
        public float TravelDistance { get; private set; }
        public string LastFault { get; private set; } = "";
        public int AliveEnemies
        {
            get { int count = 0; foreach (MeleeActor actor in enemies) if (actor.isActiveAndEnabled && !actor.IsDead) count++; return count; }
        }
        public string StateCaption
        {
            get
            {
                if (!Running) return Phase == HuntPhase.Blocked ? LastFault : "상주 던전 대기";
                switch (Phase)
                {
                    case HuntPhase.Exploring: return "넓은 던전 탐색 · 배회 적 추적";
                    case HuntPhase.Fighting: return "상주 몬스터 자동 전투";
                    case HuntPhase.Collecting: return "필드 전리품 회수";
                    case HuntPhase.Resting: return "구역 정화 · 재배치";
                    case HuntPhase.Recovering: return "쓰러짐 · 입구 부활";
                    default: return "자동사냥";
                }
            }
        }

        private void Start()
        {
            owner = GetComponent<FirstEncounter>(); hero = owner.Hero; World = new DungeonWorld(LayoutForClear(owner.Progression.DungeonClears));
            LayoutsVisitedMask |= 1 << (int)World.LayoutId;
            enemies.Add(owner.Enemy);
            for (int i = 1; i < Population; i++)
            {
                GameObject clone = Instantiate(owner.Enemy.gameObject);
                clone.name = "Obsidian Raider " + (i + 1);
                enemies.Add(clone.GetComponent<MeleeActor>());
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                MeleeActor actor = enemies[i]; actor.SetTarget(null); actor.Damaged += OnEnemyDamaged;
                actor.SetNavigation(World.NextWaypoint, World.LineOfSight); actor.ConfigureMoveSpeed(1.55f + (i % 4) * .08f);
                roamSteps.Add(0); roamAt.Add(0);
            }
            hero.SetTarget(null); hero.SetNavigation(World.NextWaypoint, World.LineOfSight); hero.ConfigureMoveSpeed(3.35f);
            hero.ConfigureCleave(enemies, 1.85f); hero.StrikeResolved += OnStrike;
            feedback = gameObject.AddComponent<CombatFeedback>(); feedback.Configure(hero, enemies);
            areaSkill = gameObject.AddComponent<AutoAreaSkill>(); areaSkill.Configure(hero, enemies, this);
            recoverySkill = gameObject.AddComponent<AutoRecoverySkill>(); recoverySkill.Configure(hero, this);
            Camera camera = Camera.main;
            if (camera != null)
            {
                DungeonCameraRig rig = camera.GetComponent<DungeonCameraRig>();
                if (rig == null) rig = camera.gameObject.AddComponent<DungeonCameraRig>();
                rig.Configure(hero.transform);
            }
        }

        private void Update()
        {
            if (World == null) return;
            if (!Initialized)
            {
                if (!hero.IsReady) return;
                foreach (MeleeActor actor in enemies) if (!actor.IsReady) return;
                SpawnPopulation(true);
                Initialized = true; return;
            }
            Vector2 position = hero.transform.position;
            TravelDistance += Vector2.Distance(lastPosition, position); lastPosition = position;
            if (!Running || owner.IsPaused) return;
            if (Time.time - lastKillAt > 2.4f) KillChain = 0;
            SectionIndex = Mathf.Clamp(Mathf.FloorToInt((position.x - DungeonWorld.Origin.x) / 24f), 0, 2);
            UpdateEnemyRoaming(position);
            if (hero.IsDead)
            {
                if (Phase != HuntPhase.Recovering)
                {
                    Phase = HuntPhase.Recovering; delay = 2; ClearTargets(); FailedRuns++; ConsecutiveFailures++;
                    if (ConsecutiveFailures >= 2) { Block("연속 2회 쓰러졌습니다. 장비와 스킬을 정비한 뒤 재개하세요."); return; }
                }
                delay -= Time.deltaTime;
                if (delay <= 0) { DeathRetries++; SpawnPopulation(true); }
                return;
            }
            if (owner.Progression.PendingLoot != null)
            {
                if (Phase != HuntPhase.Collecting) phaseBeforeLoot = Phase;
                Phase = HuntPhase.Collecting; hero.SetTarget(null); hero.SetDestination(owner.LootPosition);
                if (Vector2.Distance(position, owner.LootPosition) < .6f)
                {
                    if (owner.CollectLoot()) { CollectedItems++; hero.SetDestination(null); stalledTime = 0; stallAttempts = 0; }
                    else { Block("가방이 가득 찼습니다. 공간을 비운 뒤 재개하세요."); return; }
                }
                CheckMovement(position); return;
            }
            if (Phase == HuntPhase.Collecting) Phase = phaseBeforeLoot == HuntPhase.Fighting ? HuntPhase.Exploring : phaseBeforeLoot;
            if (Phase == HuntPhase.Resting)
            {
                delay -= Time.deltaTime;
                if (delay <= 0) SpawnPopulation(false);
                return;
            }
            if (AliveEnemies == 0)
            {
                CompletedRuns++; owner.Progression.RegisterDungeonClear(); owner.SaveProgress();
                ConsecutiveFailures = 0; Phase = HuntPhase.Resting; delay = 3;
                hero.SetTarget(null); hero.SetDestination(null); return;
            }
            // A stall escape destination needs a short exclusive movement window. Without this,
            // the null target immediately triggered ChooseTarget and erased the recovery step.
            if(Time.time<recoveryUntil){Phase=HuntPhase.Exploring;return;}
            decisionTime -= Time.deltaTime;
            if (decisionTime <= 0 || hero.CurrentTarget == null || hero.CurrentTarget.IsDead)
            {
                ChooseTarget(); decisionTime = .2f;
            }
            Phase = hero.CurrentTarget == null ? HuntPhase.Exploring : HuntPhase.Fighting;
            bool atAttackRange = hero.CurrentTarget != null &&
                Vector2.Distance(position, hero.CurrentTarget.transform.position) <= hero.AttackReach + .1f &&
                World.LineOfSight(position, hero.CurrentTarget.transform.position);
            if (!hero.IsAttacking && !atAttackRange) CheckMovement(position); else { stalledTime = 0; stallAttempts = 0; progressPosition = position; }
        }

        public void StartHunt()
        {
            if (!Initialized || !owner.CanProgress) return;
            if (owner.Progression.PendingLoot != null && owner.Progression.Inventory.Count >= HeroProgression.InventoryCapacity)
            { LastFault = "가방 공간을 먼저 비우세요."; return; }
            bool resumeAfterSafetyStop = Phase == HuntPhase.Blocked;
            LastFault = ""; Running = true; owner.SetPaused(false);
            // Two consecutive deaths still stop unattended play. Once the player has reviewed
            // gear/talents and explicitly starts again, create a fresh life at the entrance.
            if (resumeAfterSafetyStop) { ConsecutiveFailures = 0; DeathRetries++; SpawnPopulation(true); }
            if (!runStarted) { runStarted = true; owner.BeginAutoRun(); areaSkill.ResetForRun(); recoverySkill.ResetForRun(); }
            if (Phase == HuntPhase.Blocked || Phase == HuntPhase.Waiting) Phase = HuntPhase.Exploring;
            progressPosition = hero.transform.position; stalledTime = 0; stallAttempts = 0;
        }
        public void StopHunt() { Running = false; owner.SetPaused(true); }
        public void ToggleReducedEffects(){if(feedback!=null)feedback.SetReducedEffects(!feedback.ReducedEffects);}

        public bool SetDifficulty(DungeonDifficulty difficulty)
        {
            if (!CanChangeDifficulty || difficulty == CurrentDifficulty || !owner.Progression.TrySetDifficulty(difficulty)) return false;
            DungeonLayoutId before=World.LayoutId;owner.SaveProgress(); RebuildWorld(LayoutForClear(owner.Progression.DungeonClears));
            if(World.LayoutId!=before)LayoutTransitions++;SpawnPopulation(true);
            LastFault = ""; return true;
        }

        private void SpawnPopulation(bool atEntrance)
        {
            if (!atEntrance)
            {
                DungeonLayoutId next = LayoutForClear(owner.Progression.DungeonClears);
                if (World == null || World.LayoutId != next) { RebuildWorld(next); LayoutTransitions++; }
            }
            owner.BeginAutoRun();
            Vector2 heroPosition = atEntrance ? DungeonWorld.Entrance : World.SafePoint(hero.transform.position);
            hero.ResetForEncounter(heroPosition, ++identity, owner.Progression.TotalMaxHp, owner.Progression.TotalDamage);
            owner.RefreshCombatBuild();
            for (int i = 0; i < enemies.Count; i++)
            {
                MeleeActor actor = enemies[i]; actor.gameObject.SetActive(true);
                bool elite = (i + 1) % Difficulty.EliteStride == 0;
                bool guardian = elite && i == Population - 1;
                EliteEncounterMarker marker = actor.GetComponent<EliteEncounterMarker>();
                EliteEncounterProfile profile = default;
                int baseHp = 46, baseDamage = 4, defense = 0;
                float moveSpeed = 1.55f + (i % 4) * .08f, attackSpeed = 1, reach = 1;
                if (elite)
                {
                    profile = EliteEncounterTuning.Get(World.LayoutId, guardian);
                    actor.name = profile.Name + " " + (i + 1);
                    baseHp = profile.BaseHp; baseDamage = profile.BaseDamage; defense = profile.Defense;
                    moveSpeed = profile.MoveSpeed; attackSpeed = profile.AttackSpeed; reach = profile.Reach;
                    if (marker == null) marker = actor.gameObject.AddComponent<EliteEncounterMarker>();
                    marker.Configure(actor, profile, guardian);
                }
                else
                {
                    actor.name = "Obsidian Raider " + (i + 1);
                    if (marker != null) marker.Clear();
                }
                int scaledHp = DifficultyTuning.ScaleEnemyHealth(baseHp, CurrentDifficulty);
                int scaledDamage = DifficultyTuning.ScaleEnemyDamage(baseDamage, CurrentDifficulty);
                actor.ResetForEncounter(World.SafePoint(World.SpawnPoints[i]), ++identity,
                    scaledHp, scaledDamage);
                actor.ApplyCombatBuild(scaledHp, defense, scaledDamage, attackSpeed, reach, 0, 1, false);
                actor.ConfigureMoveSpeed(moveSpeed);
                actor.SetTarget(null); actor.SetDestination(World.PatrolPoint(i, ++roamSteps[i])); roamAt[i] = Time.time + 1 + (i % 5) * .3f;
            }
            MaxAliveEnemies = Math.Max(MaxAliveEnemies, AliveEnemies);
            areaSkill.ResetForRun(); recoverySkill.ResetForRun(); Phase = Running ? HuntPhase.Exploring : HuntPhase.Waiting;
            lastPosition = progressPosition = heroPosition; stalledTime = decisionTime = enemyDecisionTime = recoveryUntil = 0; stallAttempts = 0;
        }

        private void RebuildWorld(DungeonLayoutId layout)
        {
            World?.Dispose(); World = new DungeonWorld(layout); LayoutsVisitedMask |= 1 << (int)layout;
            if (hero != null) hero.SetNavigation(World.NextWaypoint, World.LineOfSight);
            foreach (MeleeActor actor in enemies) if (actor != null) actor.SetNavigation(World.NextWaypoint, World.LineOfSight);
        }

        private static DungeonLayoutId LayoutForClear(int clears) => (DungeonLayoutId)(Math.Abs(clears) % 3);

        private void UpdateEnemyRoaming(Vector2 heroPosition)
        {
            enemyDecisionTime -= Time.deltaTime;
            if (enemyDecisionTime > 0) return;
            enemyDecisionTime = .25f;
            var threatening = new List<MeleeActor>();
            for (int i = 0; i < enemies.Count; i++)
            {
                MeleeActor actor = enemies[i]; if (!actor.isActiveAndEnabled || actor.IsDead) continue;
                float distance = Vector2.Distance(actor.transform.position, heroPosition);
                if (distance <= AggroRadius && World.LineOfSight(actor.transform.position, heroPosition))
                    threatening.Add(actor);
                else
                {
                    actor.SetTarget(null); actor.SetAttacksAllowed(false);
                    // The chosen prey must remain a stable navigation goal. Letting a distant
                    // selected monster keep patrolling can make both endpoints orbit an obstacle.
                    if (actor == hero.CurrentTarget) { actor.SetDestination(null); continue; }
                    if (Time.time >= roamAt[i])
                    {
                        actor.SetDestination(World.PatrolPoint(i, ++roamSteps[i]));
                        roamAt[i] = Time.time + 2.2f + (i % 6) * .45f;
                    }
                }
            }
            threatening.Sort((a, b) => ((Vector2)a.transform.position - heroPosition).sqrMagnitude.CompareTo(((Vector2)b.transform.position - heroPosition).sqrMagnitude));
            for (int i = 0; i < threatening.Count; i++)
            {
                threatening[i].SetDestination(null); threatening[i].SetTarget(hero); threatening[i].SetAttacksAllowed(i<3);
            }
        }

        private void ChooseTarget()
        {
            if (hero.IsAttacking) return;
            var candidates = new List<GridCell>(); var actors = new List<MeleeActor>();
            foreach (MeleeActor actor in enemies)
                if (actor.isActiveAndEnabled && !actor.IsDead)
                { actors.Add(actor); candidates.Add(World.Cell(actor.transform.position)); }
            if (World.Navigation.TryNearestReachable(World.Cell(hero.transform.position), candidates, out int index, out _))
            {
                MeleeActor selected = actors[index]; if (hero.CurrentTarget != selected) TargetSelections++;
                hero.SetTarget(selected); hero.SetDestination(null); owner.FocusEnemy(selected);
            }
            else Block("남은 적에게 도달 가능한 경로가 없습니다.");
        }

        private void CheckMovement(Vector2 position)
        {
            if (Vector2.Distance(position, progressPosition) > .18f)
            { stalledTime = 0; stallAttempts = 0; progressPosition = position; return; }
            stalledTime += Time.deltaTime;
            if (stalledTime <= 5) return;
            stalledTime = 0; stallAttempts++; StallRecoveries++;
            hero.SetTarget(null);
            hero.SetDestination(World.SafePoint(position + new Vector2(stallAttempts % 2 == 0 ? 2 : -2, stallAttempts % 3 - 1)));
            decisionTime = 1.5f;recoveryUntil=Time.time+1.5f;
            if (stallAttempts >= 3) Block("세 차례 경로 복구에 실패해 안전 중지했습니다.");
        }
        private void Block(string reason)
        {
            Debug.LogWarning("Hunt stopped: " + reason + " phase=" + Phase + " hero=" + hero.transform.position);
            LastFault = reason; Phase = HuntPhase.Blocked; StopHunt();
        }
        private void ClearTargets()
        {
            hero.SetTarget(null); hero.SetDestination(null);
            foreach (MeleeActor actor in enemies) { actor.SetTarget(null); actor.SetDestination(null); }
        }
        private void OnStrike(MeleeActor attacker, int hits, int kills, bool area)
        {
            if (hits > 1) MultiHitAttacks++; MaxKillsPerStrike = Math.Max(MaxKillsPerStrike, kills);
        }
        private void OnEnemyDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (!receipt.Killed || hero.IsDead) return;
            if (owner.RegisterDefeat(actor))
            {
                TotalKills++; stalledTime = 0; stallAttempts = 0;
                EliteEncounterMarker marker=actor.GetComponent<EliteEncounterMarker>();
                if(marker!=null&&marker.Active)
                {
                    EliteKills++;if(marker.IsGuardian)GuardianKills++;
                    DefeatedElitePatternMask|=marker.StyleMaskBit;
                }
                KillChain = Time.time - lastKillAt <= 2.4f ? KillChain + 1 : 1;
                MaxKillChain = Math.Max(MaxKillChain, KillChain); lastKillAt = Time.time;
            }
        }
        private void OnDestroy()
        {
            if (hero != null) hero.StrikeResolved -= OnStrike;
            foreach (MeleeActor actor in enemies) if (actor != null) actor.Damaged -= OnEnemyDamaged;
            World?.Dispose();
        }
    }
}
