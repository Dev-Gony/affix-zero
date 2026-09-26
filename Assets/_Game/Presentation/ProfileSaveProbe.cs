#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Explicit two-process disk verification. Normal play never creates this component.
    public sealed class ProfileSaveProbe : MonoBehaviour
    {
        private FirstEncounter owner;
        private Report report;
        private string mode, saveDirectory, reportDirectory, phase = "initializing";
        private float started;
        private int phaseFrame;
        private bool finishing;
        private readonly HashSet<MeleeActor> observed = new HashSet<MeleeActor>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-affixSaveTest") < 0) return;
            var host = new GameObject("Affix Profile Save Probe");
            DontDestroyOnLoad(host);
            host.AddComponent<ProfileSaveProbe>().Begin(args);
        }

        private void Begin(string[] args)
        {
            report = new Report();
            started = Time.realtimeSinceStartup;
            reportDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Build", "Reports"));
            Application.logMessageReceived += OnLog;
            try
            {
                reportDirectory = AbsoluteArgument(args, "-affixReportDir");
                saveDirectory = AbsoluteArgument(args, "-affixSaveDir");
                mode = Argument(args, "-affixSaveMode");
                Require(mode == "write" || mode == "read", "Save mode must be write or read.");
                Require(Array.IndexOf(args, "-affixSmokeTest") < 0 && Array.IndexOf(args, "-affixAutoHuntTest") < 0 &&
                    Array.IndexOf(args, "-affixAutoHuntSafetyTest") < 0, "Save probe cannot run alongside another probe.");
                report.mode = mode;
                report.expectedPath = Path.Combine(saveDirectory, "expected-profile.json");
                Directory.CreateDirectory(reportDirectory);
                WriteReport();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void LateUpdate()
        {
            if (finishing || report == null) return;
            try
            {
                Require(!Input.anyKeyDown && Input.mouseScrollDelta.sqrMagnitude == 0,
                    "Physical keyboard/mouse input invalidated this unattended verification.");
                if (Time.realtimeSinceStartup - started > 90) throw new TimeoutException("Save probe timed out in " + phase);
                Tick();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void Tick()
        {
            if (phase == "initializing")
            {
                owner = FindFirstObjectByType<FirstEncounter>();
                if (owner == null) return;
                Require(owner.Persistence.CanPlay && !owner.Persistence.Ephemeral,
                    "Disk profile unavailable: " + owner.Persistence.Problem);
                Require(string.Equals(Path.GetFullPath(owner.Persistence.FilePath),
                    Path.Combine(saveDirectory, "profile-v1.json"), StringComparison.OrdinalIgnoreCase),
                    "Persistence did not select the isolated test directory.");
                if (owner.Hunt == null || !owner.Hunt.Initialized || !owner.Hero.IsReady || !ControlReady("autohunt-toggle")) return;
                Require(!owner.Hunt.Running && owner.IsPaused && Time.timeScale == 0 && !owner.ManagementVisible,
                    "Loaded profile must start stopped at the dungeon screen.");
                Require(Vector2.Distance(owner.Hero.transform.position, new Vector2(-10, -4)) < .01f,
                    "Loaded hero is not at the entry point.");
                ObserveActors();
                if (mode == "write")
                {
                    Require(owner.Progression.TotalExperience == 0 && owner.Progression.TotalGold == 0 &&
                        owner.Progression.Inventory.Count == 0 && owner.Progression.PendingLoot == null &&
                        owner.Progression.TotalDamage == 30, "Write phase requires a fresh isolated profile.");
                    Require(!File.Exists(report.expectedPath), "Write phase refuses to replace an existing expected-profile artifact.");
                    StartThroughUi();
                    Phase("write-hunting");
                }
                else
                {
                    Require(File.Exists(report.expectedPath), "Expected profile artifact is absent; run write phase first.");
                    string expected = File.ReadAllText(report.expectedPath);
                    Require(ProfilePersistence.Encode(owner.Progression) == expected,
                        "Restored profile differs from the previous process before any mutation.");
                    report.canonicalRestoreMatches = true;
                    RequireStats(150, 40, 3, 1, true);
                    Dispatch("character-tab");
                    Phase("read-hud");
                }
                return;
            }

            Require(owner != null && owner.Hunt != null && owner.Persistence.CanPlay,
                "Owner disappeared or persistence blocked play.");
            Require(string.IsNullOrEmpty(owner.Hunt.LastFault), "Hunt fault: " + owner.Hunt.LastFault);
            Require(!owner.Hero.IsDead && owner.Hunt.DeathRetries == 0, "Save scenario died or retried.");
            Require(owner.IsPaused ? Time.timeScale == 0 : Time.timeScale == 1, "Unexpected time acceleration or pause scale.");
            ObserveActors();

            if (phase == "write-hunting")
            {
                Require(owner.Hunt.Running, "Write hunt stopped early.");
                if (owner.Hunt.TotalKills < 6 || owner.Progression.PendingLoot == null) return;
                owner.Hunt.StopHunt();
                Require(owner.Hunt.TotalKills == 6 && report.enemyDeaths == 6 && report.acceptedHits > 0,
                    "Write phase did not observe exactly six real enemy deaths.");
                Require(owner.Progression.PendingLoot.Id.StartsWith("clear:", StringComparison.Ordinal),
                    "Pending weapon is not the uncollected clear reward.");
                Require(owner.EquipItem(0), "Could not equip the first Ember sword.");
                Require(owner.SpendTalent(TalentId.Fury) && owner.SpendTalent(TalentId.Fury) &&
                    owner.SpendTalent(TalentId.Precision), "Could not spend the earned talent points.");
                Require(owner.EnhanceWeapon(), "Could not enhance equipped weapon using earned gold.");
                RequireStats(150, 40, 3, 1, true);
                Dispatch("character-tab");
                Phase("write-hud");
            }
            else if (phase == "write-hud" || phase == "read-hud")
            {
                if (Time.frameCount <= phaseFrame + 1) return;
                VerifyHudDamage();
                Dispatch("dungeon-tab");
                Require(!owner.ManagementVisible, "Dungeon callback failed to close management.");
                if (mode == "write")
                {
                    FlushAndVerify();
                    string expected = ProfilePersistence.Encode(owner.Progression);
                    byte[] bytes = new System.Text.UTF8Encoding(false).GetBytes(expected);
                    using (var file = new FileStream(report.expectedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    { file.Write(bytes, 0, bytes.Length); file.Flush(true); }
                    Require(File.ReadAllText(report.expectedPath) == expected, "Expected artifact write did not roundtrip.");
                    Finish(true, null);
                }
                else
                {
                    StartThroughUi();
                    Phase("read-hunting");
                }
            }
            else if (phase == "read-hunting")
            {
                Require(owner.Hunt.Running, "Read hunt stopped before resumed combat.");
                if (owner.Progression.PendingLoot == null && owner.Progression.Inventory.Count == 2)
                    report.restoredPendingCollected = true;
                if (owner.Hunt.TotalKills < 2) return;
                owner.Hunt.StopHunt();
                Require(owner.Hunt.TotalKills == 2 && report.enemyDeaths == 2 && report.acceptedHits > 0,
                    "Read phase did not observe two new real enemy deaths.");
                Require(report.restoredPendingCollected, "Restored pending reward was not automatically collected.");
                RequireStats(200, 56, 5, 2, false);
                string before = ProfilePersistence.Encode(owner.Progression);
                string oldToken = ProfilePersistence.Decode(File.ReadAllText(report.expectedPath)).CaptureSnapshot().killTokens[0];
                Require(!owner.Progression.TryRegisterKill(oldToken) && ProfilePersistence.Encode(owner.Progression) == before,
                    "Previously saved kill token issued a duplicate reward.");
                report.oldKillRejected = true;
                FlushAndVerify();
                Finish(true, null);
            }
        }

        private void RequireStats(int xp, int gold, int points, int inventory, bool pending)
        {
            HeroProgression p = owner.Progression;
            Require(p.TotalExperience == xp && p.TotalGold == gold && p.UnspentPoints == points &&
                p.Inventory.Count == inventory && (p.PendingLoot != null) == pending,
                "Unexpected profile balances, inventory or pending loot.");
            Require(p.TotalDamage == 52 && owner.Hero.Damage == 52 && p.FuryRank == 2 && p.PrecisionRank == 1 &&
                p.KeystoneRank == 0 && p.EquippedWeapon.EnhancementRank == 1 && p.EquippedWeapon.Id == "loot:ember-steel:first",
                "Equipped build was not preserved exactly.");
        }

        private void VerifyHudDamage()
        {
            Require(owner.EquipmentVisible, "Equipment screen is not open for HUD verification.");
            VisualElement section = Root.Q("equipment-section");
            Require(section != null && section.worldBound.width > 0, "Equipment stats layout is absent.");
            bool found = false;
            section.Query<Label>().ForEach(label => { if (label.text == "52") found = true; });
            Require(found && owner.Hero.Damage == 52, "Native equipment HUD did not bind attack 52.");
            report.hudDamageMatches = true;
        }

        private void FlushAndVerify()
        {
            Require(owner.SaveProgress(), "Explicit save failed: " + owner.Persistence.Problem);
            Require(File.Exists(owner.Persistence.FilePath), "Saved profile file is absent.");
            Require(ProfilePersistence.Encode(ProfilePersistence.Decode(File.ReadAllText(owner.Persistence.FilePath))) ==
                ProfilePersistence.Encode(owner.Progression), "Disk readback differs from current profile.");
            report.diskRoundtripMatches = true;
        }

        private VisualElement Root => owner.GetComponent<UIDocument>()?.rootVisualElement;
        private bool ControlReady(string name)
        {
            VisualElement root = Root;
            VisualElement element = root?.Q(name);
            return root?.panel != null && element != null && element.enabledInHierarchy && element.worldBound.width > 0;
        }
        private void Dispatch(string name)
        {
            Require(ControlReady(name), "Native UI control is not ready: " + name);
            VisualElement element = Root.Q(name);
            for (VisualElement node = element; node != null; node = node.parent)
                Require(node.resolvedStyle.display != DisplayStyle.None && node.resolvedStyle.visibility == Visibility.Visible,
                    "UI control is hidden: " + name);
            using (var click = ClickEvent.GetPooled()) { click.target = element; element.SendEvent(click); }
            report.uiCallbacks++;
        }
        private void StartThroughUi()
        {
            Dispatch("autohunt-toggle");
            Require(owner.Hunt.Running && !owner.IsPaused && Time.timeScale == 1, "Start callback did not resume 1x hunting.");
            report.startCallbacks++;
        }
        private void ObserveActors()
        {
            Observe(owner.Hero);
            foreach (MeleeActor enemy in owner.Hunt.Enemies) Observe(enemy);
        }
        private void Observe(MeleeActor actor)
        { if (actor != null && observed.Add(actor)) actor.Damaged += OnDamaged; }
        private void OnDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (finishing || !receipt.Accepted) return;
            report.acceptedHits++;
            if (actor == owner.Hero) report.heroHits++;
            else { report.enemyHits++; if (receipt.Killed) report.enemyDeaths++; }
        }
        private void Phase(string value) { phase = value; phaseFrame = Time.frameCount; }
        private static string Argument(string[] args, string key)
        {
            int index = Array.IndexOf(args, key);
            if (index < 0 || index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
                throw new ArgumentException("Missing argument " + key);
            return args[index + 1];
        }
        private static string AbsoluteArgument(string[] args, string key)
        {
            string value = Argument(args, key);
            if (!Path.IsPathRooted(value)) throw new ArgumentException(key + " requires an absolute directory.");
            return Path.GetFullPath(value);
        }
        private static void Require(bool condition, string problem)
        { if (!condition) throw new InvalidOperationException(problem); }
        private void OnLog(string message, string trace, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Finish(false, message + "\n" + trace); }
        private void Finish(bool success, string problem)
        {
            if (finishing) return;
            finishing = true;
            Application.logMessageReceived -= OnLog;
            foreach (MeleeActor actor in observed) if (actor != null) actor.Damaged -= OnDamaged;
            report.result = success ? "PASS" : "FAIL";
            report.problem = problem ?? ""; report.phase = phase;
            report.finishedUtc = DateTime.UtcNow.ToString("O"); report.elapsedSeconds = Time.realtimeSinceStartup - started;
            try
            {
                if (owner != null)
                {
                    report.experience = owner.Progression.TotalExperience; report.gold = owner.Progression.TotalGold;
                    report.damage = owner.Progression.TotalDamage; report.unspentPoints = owner.Progression.UnspentPoints;
                    report.inventoryCount = owner.Progression.Inventory.Count; report.pendingLoot = owner.Progression.PendingLoot != null;
                    report.saveCount = owner.Persistence.SaveCount; report.saveStatus = owner.Persistence.Status;
                    report.filePath = owner.Persistence.FilePath;
                    if (owner.Hunt != null) { report.kills = owner.Hunt.TotalKills; report.completedRuns = owner.Hunt.CompletedRuns; }
                    if (File.Exists(report.filePath))
                        using (var sha = SHA256.Create()) using (var file = File.OpenRead(report.filePath))
                            report.fileSha256 = BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
                }
                Directory.CreateDirectory(reportDirectory); WriteReport();
            }
            catch (Exception error) { success = false; Debug.LogError("Cannot finish save probe report: " + error); }
            Debug.Log("AFFIX_SAVE_PROBE_" + (success ? "PASS" : "FAIL") + " mode=" + mode + " run=" + report.runId);
            Application.Quit(success ? 0 : 1);
        }
        private void WriteReport() => File.WriteAllText(Path.Combine(reportDirectory, "profile-save-" +
            (mode == "write" || mode == "read" ? mode : "invalid") + ".json"), JsonUtility.ToJson(report, true));

        [Serializable] private sealed class Report
        {
            public string schema = "affix-profile-save-probe-v1", runId = Guid.NewGuid().ToString("N"),
                startedUtc = DateTime.UtcNow.ToString("O"), finishedUtc = "", buildGuid = Application.buildGUID,
                unityVersion = Application.unityVersion, mode = "", result = "RUNNING", problem = "", phase = "initializing",
                expectedPath = "", filePath = "", fileSha256 = "", saveStatus = "";
            public string scope = "Separate-process isolated profile save/load, real 1x combat, retained pending loot, native UI Toolkit callback start and HUD data binding.";
            public string interactionMethod = "UI Toolkit ClickEvent callbacks plus authorized FirstEncounter gear/talent/forge APIs; no OS click automation. Physical input invalidates the run.";
            public string actualOsInputVerification = "NOT_RUN", userVisualApproval = "NOT_APPROVED";
            public bool canonicalRestoreMatches, diskRoundtripMatches, hudDamageMatches, restoredPendingCollected, oldKillRejected, pendingLoot;
            public int uiCallbacks, startCallbacks, acceptedHits, heroHits, enemyHits, enemyDeaths, kills, completedRuns,
                experience, gold, damage, unspentPoints, inventoryCount, saveCount;
            public float elapsedSeconds;
        }
    }
}
#endif
