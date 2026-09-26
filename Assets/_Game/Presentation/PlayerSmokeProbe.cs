#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Opt-in player verification only. No object, file access or UI in a normal launch.
    public sealed class PlayerSmokeProbe : MonoBehaviour
    {
        private FirstEncounter encounter;
        private MeleeActor hero, enemy;
        private Report report;
        private string outputDirectory;
        private string phase = "waiting";
        private float started, phaseStarted;
        private Vector3 heroStart, enemyStart;
        private int frozenHeroHp, frozenEnemyHp, oldEncounterId, pendingCaptures;
        private bool finishing, restarted;
        private FrozenState frozen;
        private readonly HashSet<string> completedCaptures = new HashSet<string>();
        private static readonly string[] RequiredCaptures = { "walk", "impact", "cleared", "paused", "equipment" };
        private readonly HashSet<string> requestedCaptures = new HashSet<string>();
        private readonly List<string> screenshots = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-affixSmokeTest") < 0) return;
            var host = new GameObject("Affix Player Smoke Probe");
            DontDestroyOnLoad(host);
            host.AddComponent<PlayerSmokeProbe>().Begin(args);
        }

        private void Begin(string[] args)
        {
            report = new Report();
            started = Time.realtimeSinceStartup;
            outputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Build", "Reports"));
            Application.logMessageReceived += OnLog;
            try
            {
                int directoryIndex = Array.IndexOf(args, "-affixReportDir");
                if (directoryIndex >= 0)
                {
                    if (directoryIndex + 1 >= args.Length || !Path.IsPathRooted(args[directoryIndex + 1]))
                        throw new ArgumentException("-affixReportDir requires an absolute output directory.");
                    outputDirectory = Path.GetFullPath(args[directoryIndex + 1]);
                }
                Directory.CreateDirectory(outputDirectory);
                WriteReport(); // Invalidate an older PASS before doing any verification.
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        // Observe after both actors and the encounter have finished their Update.
        // Time.timeScale changes do not retroactively zero the current frame's deltaTime.
        private void LateUpdate()
        {
            if (finishing || report == null) return;
            try
            {
                if (Time.realtimeSinceStartup - started > 30)
                    throw new TimeoutException("Player smoke exceeded 30 seconds in phase " + phase);
                Tick();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void Tick()
        {
            if (phase == "waiting" || phase == "restartWaiting")
            {
                var found = FindFirstObjectByType<FirstEncounter>();
                if (found == null || (restarted && found.GetInstanceID() == oldEncounterId)) return;
                if (found.Hero == null || found.Enemy == null || !found.Hero.IsReady || !found.Enemy.IsReady) return;
                encounter = found;
                hero = found.Hero;
                enemy = found.Enemy;
                Require(hero.Hp == hero.MaxHp && enemy.Hp == enemy.MaxHp, "New encounter did not start with full HP.");
                Require(found.Experience == 0 && found.Gold == 0 && found.RewardCollectionCount == 0,
                    "New encounter did not start with zero rewards.");
                Require(hero.AnimationSet.ValidateSet() == null && enemy.AnimationSet.ValidateSet() == null,
                    "Runtime animation data is invalid.");
                Require(!found.IsPaused && !found.EquipmentVisible && Time.timeScale > 0,
                    "New encounter retained pause or equipment overlay state.");
                report.actorsInitialized = true;
                if (restarted) report.restartPauseAndOverlayReset = true;
                if (restarted) { report.restartFullHp = true; report.restartRewardsReset = true; }
                heroStart = hero.transform.position;
                enemyStart = enemy.transform.position;
                hero.Damaged += OnDamaged;
                enemy.Damaged += OnDamaged;
                phase = "combat";
            }
            if (IsPausePhase())
            {
                TickPauseChecks();
                return;
            }
            if (phase == "combat")
            {
                Require(!hero.IsDead, "Hero died before the expected first encounter clear.");
                if (Vector3.Distance(heroStart, hero.transform.position) > 0.15f &&
                    Vector3.Distance(enemyStart, enemy.transform.position) > 0.15f)
                {
                    report.approachObserved = true;
                    CaptureOnce("walk");
                }
                if (!restarted && report.approachObserved && !report.movementPauseStable && pendingCaptures == 0)
                {
                    Require(hero.CurrentClip == ActorClip.Walk && enemy.CurrentClip == ActorClip.Walk,
                        "Movement pause was not reached while both actors walked.");
                    encounter.SetPaused(true);
                    BeginPauseCheck("movementPaused");
                    CaptureOnce("paused");
                    return;
                }
                if (!restarted && report.manualPauseSurvivesOverlay && !report.attackPauseStable &&
                    (hero.IsAttacking || enemy.IsAttacking))
                {
                    encounter.SetPaused(true);
                    BeginPauseCheck("attackPaused");
                    return;
                }
                if (restarted && hero.Hp < hero.MaxHp && enemy.Hp < enemy.MaxHp)
                {
                    report.restartCombatObserved = true;
                    phase = "complete";
                }
                else if (enemy.IsDead)
                {
                    Require(enemy.DeathCount == 1, "Enemy death was not recorded exactly once.");
                    report.enemyDeathObserved = true;
                    frozenHeroHp = hero.Hp;
                    frozenEnemyHp = enemy.Hp;
                    phase = "deathHold";
                    phaseStarted = Time.realtimeSinceStartup;
                }
            }
            if (phase == "deathHold")
            {
                Require(hero.Hp == frozenHeroHp && enemy.Hp == frozenEnemyHp, "Damage continued after enemy death.");
                Require(encounter.Experience == 25 && encounter.Gold == 8 && encounter.RewardCollectionCount == 1,
                    "Expected exactly one automatic 25 XP / 8 gold reward.");
                float elapsed = Time.realtimeSinceStartup - phaseStarted;
                if (elapsed > 1.0f) CaptureOnce("cleared");
                if (elapsed > 1.6f && pendingCaptures == 0)
                {
                    report.noDamageAfterDeath = true;
                    report.rewardCollectedOnce = true;
                    report.firstClearHeroHp = hero.Hp;
                    DetachActors();
                    oldEncounterId = encounter.GetInstanceID();
                    restarted = true;
                    phase = "restartWaiting";
                    encounter.SetPaused(true);
                    encounter.SetEquipmentVisible(true);
                    Require(encounter.IsPaused && encounter.EquipmentVisible, "Could not prepare paused overlay restart.");
                    encounter.RestartEncounter(); // Direct API call shared with the player's button; not a click.
                }
            }
            if (phase == "complete" && pendingCaptures == 0)
            {
                Require(report.approachObserved && report.heroDamageObserved && report.enemyDamageObserved &&
                    report.enemyDeathObserved && report.impactPoseObserved && report.noDamageAfterDeath &&
                    report.rewardCollectedOnce && report.restartFullHp && report.restartRewardsReset &&
                    report.restartCombatObserved && report.restartPauseAndOverlayReset &&
                    report.movementPauseStable && report.attackPauseStable && report.equipmentPauseStable &&
                    report.equipmentCloseResumed && report.manualPauseSurvivesOverlay && report.combatResumedAfterPause,
                    "Required combat or pause observations are missing.");
                foreach (string label in RequiredCaptures)
                    Require(completedCaptures.Contains(label), "Required screenshot is missing: " + label);
                Finish(true, "");
            }
        }

        private bool IsPausePhase() => phase == "movementPaused" || phase == "equipmentPaused" ||
            phase == "manualWithEquipment" || phase == "manualAfterEquipment" || phase == "attackPaused";

        private void BeginPauseCheck(string next)
        {
            Require(encounter.IsPaused && Time.timeScale == 0, "Pause did not stop simulation time.");
            phase = next;
            phaseStarted = Time.realtimeSinceStartup;
            frozen = new FrozenState(hero, enemy, encounter);
        }

        private void TickPauseChecks()
        {
            Require(encounter.IsPaused && Time.timeScale == 0, "Pause ended unexpectedly in " + phase);
            frozen.RequireUnchanged(hero, enemy, encounter);
            if (Time.realtimeSinceStartup - phaseStarted < 0.35f || pendingCaptures > 0) return;
            if (phase == "movementPaused")
            {
                report.movementPauseStable = true;
                encounter.SetPaused(false);
                Require(!encounter.IsPaused, "Manual resume failed.");
                encounter.SetEquipmentVisible(true);
                Require(encounter.EquipmentVisible, "Equipment overlay did not open.");
                BeginPauseCheck("equipmentPaused");
                CaptureOnce("equipment");
            }
            else if (phase == "equipmentPaused")
            {
                report.equipmentPauseStable = true;
                encounter.SetEquipmentVisible(false);
                Require(!encounter.EquipmentVisible && !encounter.IsPaused && Time.timeScale > 0,
                    "Closing equipment did not restore the previously running state.");
                report.equipmentCloseResumed = true;
                encounter.SetPaused(true);
                encounter.SetEquipmentVisible(true);
                Require(encounter.EquipmentVisible, "Equipment did not open over manual pause.");
                BeginPauseCheck("manualWithEquipment");
            }
            else if (phase == "manualWithEquipment")
            {
                encounter.SetEquipmentVisible(false);
                Require(!encounter.EquipmentVisible && encounter.IsPaused,
                    "Closing equipment unexpectedly removed manual pause.");
                BeginPauseCheck("manualAfterEquipment");
            }
            else
            {
                if (phase == "manualAfterEquipment") report.manualPauseSurvivesOverlay = true;
                if (phase == "attackPaused") report.attackPauseStable = true;
                encounter.SetPaused(false);
                Require(!encounter.IsPaused && Time.timeScale > 0, "Combat did not resume after pause.");
                phase = "combat";
            }
        }

        private sealed class FrozenState
        {
            private readonly Vector3 heroPosition, enemyPosition;
            private readonly int heroHp, enemyHp, experience, gold, collections;
            private readonly double heroAttack, enemyAttack, elapsed;
            private readonly Sprite heroSprite, enemySprite;
            public FrozenState(MeleeActor hero, MeleeActor enemy, FirstEncounter encounter)
            {
                heroPosition = hero.transform.position; enemyPosition = enemy.transform.position;
                heroHp = hero.Hp; enemyHp = enemy.Hp;
                heroAttack = hero.AttackElapsed; enemyAttack = enemy.AttackElapsed;
                heroSprite = hero.GetComponent<SpriteRenderer>().sprite;
                enemySprite = enemy.GetComponent<SpriteRenderer>().sprite;
                experience = encounter.Experience; gold = encounter.Gold; collections = encounter.RewardCollectionCount;
                elapsed = encounter.ElapsedSeconds;
            }
            public void RequireUnchanged(MeleeActor hero, MeleeActor enemy, FirstEncounter encounter)
            {
                Require(hero.transform.position == heroPosition && enemy.transform.position == enemyPosition,
                    "An actor moved while paused.");
                Require(hero.Hp == heroHp && enemy.Hp == enemyHp, "HP changed while paused.");
                Require(hero.AttackElapsed == heroAttack && enemy.AttackElapsed == enemyAttack,
                    "Attack time advanced while paused.");
                Require(hero.GetComponent<SpriteRenderer>().sprite == heroSprite &&
                    enemy.GetComponent<SpriteRenderer>().sprite == enemySprite, "Animation changed while paused.");
                Require(encounter.Experience == experience && encounter.Gold == gold &&
                    encounter.RewardCollectionCount == collections, "Rewards changed while paused.");
                Require(encounter.ElapsedSeconds == elapsed, "Encounter timer advanced while paused.");
            }
        }

        private void OnDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (finishing) return;
            try
            {
                Require(receipt.Accepted, "Damage event did not carry an accepted receipt.");
                Require(!encounter.IsPaused, "Damage was accepted while the encounter was paused.");
                if (actor == hero) report.heroDamageObserved = true;
                if (actor == enemy) report.enemyDamageObserved = true;
                var attacker = actor == hero ? enemy : hero;
                Require(attacker.GetComponent<SpriteRenderer>().sprite == attacker.AnimationSet.ImpactSprite,
                    "Damage did not coincide with the actual impact sprite.");
                report.impactPoseObserved = true;
                if (report.attackPauseStable) report.combatResumedAfterPause = true;
                CaptureOnce("impact");
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void CaptureOnce(string label)
        {
            if (!requestedCaptures.Add(label)) return;
            pendingCaptures++;
            StartCoroutine(CaptureFrame(label));
        }

        private IEnumerator CaptureFrame(string label)
        {
            // The probe changes state in LateUpdate; give the native HUD a complete
            // update/layout cycle before inspecting and capturing its visual tree.
            yield return null;
            yield return new WaitForEndOfFrame();
            if (finishing) yield break;
            Texture2D pixels = null;
            try
            {
                Require(Screen.width > 0 && Screen.height > 0, "Player framebuffer has no size.");
                VerifyNativeHud();
                pixels = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                pixels.Apply();
                string path = Path.Combine(outputDirectory, "player-" + report.runId + "-" + label + ".bmp");
                WriteBitmap(path, pixels);
                screenshots.Add(path);
                completedCaptures.Add(label);
                pendingCaptures--;
            }
            catch (Exception error) { Finish(false, error.ToString()); }
            finally { if (pixels != null) Destroy(pixels); }
        }

        private void VerifyNativeHud()
        {
            var document = encounter.GetComponent<UIDocument>();
            Require(document != null && document.panelSettings != null, "Runtime UIDocument or PanelSettings missing.");
            var root = document.rootVisualElement;
            Require(root != null && root.name == "stitch-hud" && root.panel != null, "Stitch HUD is not attached to a panel.");
            Require(root.worldBound.width > 0 && root.worldBound.height > 0, "Stitch HUD has no layout area.");
            Require(root.Q<Label>("hero-hp-value")?.text == hero.Hp + "\n/ " + hero.MaxHp, "Hero HP label differs from combat state.");
            Require(root.Q<Label>("enemy-hp-value")?.text == enemy.Hp + " / " + enemy.MaxHp + " HP", "Enemy HP label differs from combat state.");
            Require(root.Q<Label>("xp-value")?.text == "획득 경험치  " + encounter.Experience + " XP", "XP label differs from awarded XP.");
            Require(root.Q<Label>("gold-value")?.text == "GOLD  " + encounter.Gold + "     XP  " + encounter.Experience, "Gold label differs from awarded gold.");
            var panel = root.Q("character-panel");
            Require(panel != null && (panel.resolvedStyle.display != DisplayStyle.None) == encounter.EquipmentVisible,
                "Character panel visibility differs from encounter state.");
            Require(root.Q("attack-slot") != null, "Native attack slot missing.");
            report.nativeHudStateVerified = true;
            report.nativeHudCaptureChecks++;
        }

        private static void Require(bool valid, string problem)
        {
            if (!valid) throw new InvalidOperationException(problem);
        }

        // BMP needs no ImageConversion module. Unity's pixel rows start at the
        // bottom left, matching the positive-height BMP bottom-up row order.
        private static void WriteBitmap(string path, Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            int rowBytes = checked(width * 3);
            int stride = checked(rowBytes + 3) & ~3;
            int imageBytes = checked(stride * height);
            Color32[] colors = texture.GetPixels32();
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write((ushort)0x4D42);
                writer.Write(checked(54 + imageBytes));
                writer.Write(0);
                writer.Write(54);
                writer.Write(40);
                writer.Write(width);
                writer.Write(height);
                writer.Write((ushort)1);
                writer.Write((ushort)24);
                writer.Write(0);
                writer.Write(imageBytes);
                writer.Write(2835);
                writer.Write(2835);
                writer.Write(0);
                writer.Write(0);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color32 color = colors[y * width + x];
                        writer.Write(color.b);
                        writer.Write(color.g);
                        writer.Write(color.r);
                    }
                    for (int padding = rowBytes; padding < stride; padding++) writer.Write((byte)0);
                }
            }
        }

        private void OnLog(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                Finish(false, message + "\n" + trace);
        }

        private void DetachActors()
        {
            if (hero != null) hero.Damaged -= OnDamaged;
            if (enemy != null) enemy.Damaged -= OnDamaged;
        }

        private void Finish(bool success, string problem)
        {
            if (finishing) return;
            finishing = true;
            Application.logMessageReceived -= OnLog;
            DetachActors();
            report.result = success ? "PASS" : "FAIL";
            report.problem = problem;
            report.finishedUtc = DateTime.UtcNow.ToString("O");
            report.screenshots = screenshots.ToArray();
            try { Directory.CreateDirectory(outputDirectory); WriteReport(); }
            catch (Exception error) { success = false; Debug.LogError("Cannot write smoke report: " + error); }
            Debug.Log("AFFIX_PLAYER_SMOKE_" + (success ? "PASS" : "FAIL") + " run=" + report.runId);
            Application.Quit(success ? 0 : 1);
        }

        private void WriteReport() => File.WriteAllText(Path.Combine(outputDirectory, "player-smoke.json"), JsonUtility.ToJson(report, true));

        [Serializable]
        private sealed class Report
        {
            public string schema = "affix-player-smoke-v1";
            public string runId = Guid.NewGuid().ToString("N");
            public string startedUtc = DateTime.UtcNow.ToString("O");
            public string finishedUtc = "";
            public string unityVersion = Application.unityVersion;
            public string buildGuid = Application.buildGUID;
            public string result = "RUNNING";
            public string problem = "";
            public string scope = "Actual Windows player combat, pause/overlay state checks and scene restart through shared public APIs.";
            public string interactionMethod = "Direct API calls shared with UI controls; no mouse clicks or keyboard input were generated.";
            public string actualUiClickVerification = "NOT_RUN";
            public string screenshotScope = "Uncompressed 24-bit BMP from end-of-frame ReadPixels framebuffer, including the actual runtime UI Toolkit HUD.";
            public string userVisualApproval = "NOT_APPROVED";
            public bool actorsInitialized, approachObserved, heroDamageObserved, enemyDamageObserved;
            public bool enemyDeathObserved, impactPoseObserved, noDamageAfterDeath, rewardCollectedOnce;
            public bool restartFullHp, restartRewardsReset, restartCombatObserved;
            public bool movementPauseStable, attackPauseStable, equipmentPauseStable, equipmentCloseResumed;
            public bool manualPauseSurvivesOverlay, combatResumedAfterPause, restartPauseAndOverlayReset;
            public bool nativeHudStateVerified;
            public int nativeHudCaptureChecks;
            public int firstClearHeroHp;
            public string[] screenshots = Array.Empty<string>();
        }
    }
}
#endif
