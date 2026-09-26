using System;
using System.Collections.Generic;
using System.IO;
using AffixZero.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AffixZero.Editor
{
    // Batch invocation must omit -quit: the editor remains alive through Play and domain reloads.
    [InitializeOnLoad]
    public static class EncounterVerification
    {
        private const string ScenePath = "Assets/_Game/Scenes/FirstEncounter.unity";
        private const string ReportPath = "Build/Reports/encounter-verification.json";
        private const string Key = "Affix.EncounterVerification.";
        private static Report report;
        private static MeleeActor hero;
        private static MeleeActor enemy;
        private static double phaseStarted;
        private static Vector3 heroStart;
        private static Vector3 enemyStart;
        private static int frozenHeroHp;
        private static int frozenEnemyHp;
        private static int pass;
        private static string phase;
        private static readonly HashSet<string> frames = new HashSet<string>();

        [Serializable]
        private sealed class Report
        {
            public string schema = "affix-encounter-verification-v1";
            public string recordedUtc;
            public string editorVersion;
            public string result = "RUNNING";
            public string problem = "";
            public string scene = ScenePath;
            public string userVisualApproval = "NOT_RUN";
            public string screenshotScope = "Camera.Render world only; IMGUI HUD is not included.";
            public string testScope = "Real Play Start/Update; controlled idle prelude then normal 1v1; scene reload restart.";
            public bool actorsInitialized;
            public bool approachObserved;
            public bool heroDamageObserved;
            public bool enemyDamageObserved;
            public bool enemyDeathObserved;
            public bool noDamageAfterDeath;
            public bool restartInitialHp;
            public bool restartCombatObserved;
            public bool impactPoseObserved;
            public bool rewardCollectedOnce;
            public bool restartRewardsReset;
            public string[] observedFrames = Array.Empty<string>();
            public string[] screenshots = Array.Empty<string>();
        }

        static EncounterVerification()
        {
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += PlayStateChanged;
            Application.logMessageReceived += OnLog;
            SceneManager.sceneLoaded += SceneLoaded;
            if (SessionState.GetBool(Key + "active", false))
            {
                report = JsonUtility.FromJson<Report>(SessionState.GetString(Key + "report", "{}"));
                phase = "waitingForActors";
                phaseStarted = EditorApplication.timeSinceStartup;
            }
            if (SessionState.GetBool(Key + "finishing", false) && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.delayCall += ExitIfBatch;
        }

        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Run verification from Edit mode.");
            frames.Clear();
            pass = 0;
            hero = null;
            enemy = null;
            report = new Report { recordedUtc = DateTime.UtcNow.ToString("O"), editorVersion = Application.unityVersion };
            Directory.CreateDirectory("Build/Reports");
            SessionState.SetBool(Key + "finishing", false);
            SessionState.SetBool(Key + "active", true);
            SessionState.SetString(Key + "deadline", DateTime.UtcNow.AddSeconds(90).Ticks.ToString());
            Persist();
            try
            {
                ValidateScene();
                phase = "waitingForActors";
                phaseStarted = EditorApplication.timeSinceStartup;
                EditorApplication.EnterPlaymode();
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }

        private static void ValidateScene()
        {
            if (Application.unityVersion != "6000.3.24f1")
                throw new InvalidOperationException("Unexpected Unity version: " + Application.unityVersion);
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Licensed FirstEncounter scene is absent.", ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var actors = UnityEngine.Object.FindObjectsByType<MeleeActor>(FindObjectsSortMode.None);
            if (actors.Length != 2) throw new InvalidOperationException("Expected exactly two actors.");
            foreach (var actor in actors)
            {
                if (actor.AnimationSet == null) throw new InvalidOperationException(actor.name + " has no animation set.");
                string issue = actor.AnimationSet.ValidateSet();
                if (issue != null) throw new InvalidOperationException(actor.name + ": " + issue);
            }
            if (Camera.main == null) throw new InvalidOperationException("Main Camera is missing.");
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!SessionState.GetBool(Key + "active", false) || !Application.isPlaying || scene.path != ScenePath) return;
            BindActors();
            phase = "waitingForActors";
            if (hero != null) hero.SetTarget(null);
            if (enemy != null) enemy.SetTarget(null);
        }

        private static void BindActors()
        {
            var encounter = UnityEngine.Object.FindFirstObjectByType<FirstEncounter>();
            if (encounter == null) return;
            hero = encounter.Hero;
            enemy = encounter.Enemy;
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Key + "active", false)) return;
            try
            {
                if (DateTime.UtcNow.Ticks > long.Parse(SessionState.GetString(Key + "deadline", "0")))
                    throw new TimeoutException("Play verification exceeded 90 seconds in phase " + phase);
                if (!EditorApplication.isPlaying || EditorApplication.isPaused) return;
                // LoadScene completes on a later player-loop frame; never inspect the old defeated actors.
                if (phase == "restartLoading") return;
                if (phase == "waitingForActors")
                {
                    BindActors();
                    if (hero == null || enemy == null || !hero.IsReady || !enemy.IsReady) return;
                    hero.SetTarget(null);
                    enemy.SetTarget(null);
                    hero.Damaged -= OnDamaged;
                    enemy.Damaged -= OnDamaged;
                    hero.Damaged += OnDamaged;
                    enemy.Damaged += OnDamaged;
                    if (hero.Hp != hero.MaxHp || enemy.Hp != enemy.MaxHp)
                        throw new InvalidOperationException("Actors did not start with full HP.");
                    report.actorsInitialized = true;
                    if (pass == 1)
                    {
                        report.restartInitialHp = true;
                        var restarted = UnityEngine.Object.FindFirstObjectByType<FirstEncounter>();
                        if (restarted.Experience != 0 || restarted.Gold != 0 || restarted.RewardCollectionCount != 0)
                            throw new InvalidOperationException("Scene restart retained old rewards.");
                        report.restartRewardsReset = true;
                    }
                    heroStart = hero.transform.position;
                    enemyStart = enemy.transform.position;
                    phase = "idle";
                    phaseStarted = EditorApplication.timeSinceStartup;
                }
                Observe(hero, "hero");
                Observe(enemy, "enemy");
                double elapsed = EditorApplication.timeSinceStartup - phaseStarted;
                if (phase == "idle" && elapsed >= 0.65)
                {
                    hero.SetTarget(enemy);
                    enemy.SetTarget(hero);
                    phase = "combat";
                    phaseStarted = EditorApplication.timeSinceStartup;
                }
                if (phase == "combat")
                {
                    if (Vector3.Distance(heroStart, hero.transform.position) > 0.15f &&
                        Vector3.Distance(enemyStart, enemy.transform.position) > 0.15f)
                    {
                        report.approachObserved = true;
                        CaptureOnce("walk");
                    }
                    report.heroDamageObserved |= hero.Hp < hero.MaxHp;
                    report.enemyDamageObserved |= enemy.Hp < enemy.MaxHp;
                    if (pass == 1 && hero.Hp < hero.MaxHp && enemy.Hp < enemy.MaxHp)
                    {
                        report.restartCombatObserved = true;
                        RequireAcceptance();
                        Finish(true, "");
                        return;
                    }
                    if (hero.IsDead) throw new InvalidOperationException("Hero died before expected enemy clear.");
                    if (enemy.IsDead)
                    {
                        report.enemyDeathObserved = true;
                        frozenHeroHp = hero.Hp;
                        frozenEnemyHp = enemy.Hp;
                        phase = "deathHold";
                        phaseStarted = EditorApplication.timeSinceStartup;
                    }
                }
                if (phase == "deathHold")
                {
                    elapsed = EditorApplication.timeSinceStartup - phaseStarted;
                    if (hero.Hp != frozenHeroHp || enemy.Hp != frozenEnemyHp)
                        throw new InvalidOperationException("HP changed after enemy death.");
                    if (elapsed > 0.75) CaptureOnce("death");
                    if (elapsed > 1.75)
                    {
                        var cleared = UnityEngine.Object.FindFirstObjectByType<FirstEncounter>();
                        if (cleared.Experience != 25 || cleared.Gold != 8 || cleared.RewardCollectionCount != 1)
                            throw new InvalidOperationException("Expected exactly one 25 XP / 8 gold reward after enemy death.");
                        report.rewardCollectedOnce = true;
                        report.noDamageAfterDeath = true;
                        pass = 1;
                        hero.Damaged -= OnDamaged;
                        enemy.Damaged -= OnDamaged;
                        phase = "restartLoading";
                        // Observe the same reload used by the player's Restart button.
                        UnityEngine.Object.FindFirstObjectByType<FirstEncounter>().RestartEncounter();
                    }
                }
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }

        private static void Observe(MeleeActor actor, string role)
        {
            var sprite = actor.GetComponent<SpriteRenderer>().sprite;
            if (sprite == null) throw new InvalidOperationException(role + " rendered a null sprite.");
            frames.Add(role + "/" + actor.CurrentClip + "/" + sprite.name + "/" + sprite.rect);
        }

        private static void OnDamaged(MeleeActor actor, AffixZero.Core.HitReceipt receipt)
        {
            if (!SessionState.GetBool(Key + "active", false)) return;
            try
            {
                Observe(actor, actor == hero ? "hero" : "enemy");
                var attacker = actor == hero ? enemy : hero;
                if (attacker.GetComponent<SpriteRenderer>().sprite == attacker.AnimationSet.ImpactSprite)
                    report.impactPoseObserved = true;
                else throw new InvalidOperationException("Accepted damage was not rendered at the attack impact sprite.");
                CaptureOnce("impact");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }

        private static void RequireAcceptance()
        {
            if (!report.actorsInitialized || !report.approachObserved || !report.heroDamageObserved ||
                !report.enemyDamageObserved || !report.enemyDeathObserved || !report.noDamageAfterDeath ||
                !report.restartInitialHp || !report.restartCombatObserved || !report.impactPoseObserved ||
                !report.rewardCollectedOnce || !report.restartRewardsReset)
                throw new InvalidOperationException("One or more encounter acceptance observations are missing.");
            foreach (string role in new[] { "hero", "enemy" })
            {
                RequireFrames(role, ActorClip.Idle, 1);
                RequireFrames(role, ActorClip.Walk, 2);
                RequireFrames(role, ActorClip.Attack, 2);
                RequireFrames(role, ActorClip.Hit, 1);
            }
            RequireFrames("enemy", ActorClip.Death, 2);
        }

        private static void RequireFrames(string role, ActorClip clip, int minimum)
        {
            int count = 0;
            foreach (string frame in frames)
                if (frame.StartsWith(role + "/" + clip + "/", StringComparison.Ordinal)) count++;
            if (count < minimum) throw new InvalidOperationException(role + " " + clip + ": observed " + count + " distinct sprites, expected " + minimum);
        }

        private static void CaptureOnce(string name)
        {
            string path = "Build/Reports/encounter-" + name + ".png";
            if (Array.IndexOf(report.screenshots, path) >= 0) return;
            var camera = Camera.main;
            if (camera == null) throw new InvalidOperationException("Cannot capture: Main Camera is absent.");
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var target = new RenderTexture(1280, 720, 24);
            var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                var paths = new List<string>(report.screenshots) { path };
                report.screenshots = paths.ToArray();
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(pixels);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void OnLog(string message, string trace, LogType type)
        {
            if (SessionState.GetBool(Key + "active", false) &&
                (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                Finish(false, message + "\n" + trace);
        }

        private static void Persist() => SessionState.SetString(Key + "report", JsonUtility.ToJson(report));

        private static void Finish(bool success, string problem)
        {
            if (!SessionState.GetBool(Key + "active", false)) return;
            SessionState.SetBool(Key + "active", false);
            if (hero != null) hero.Damaged -= OnDamaged;
            if (enemy != null) enemy.Damaged -= OnDamaged;
            SessionState.SetBool(Key + "finishing", true);
            SessionState.SetInt(Key + "exit", success ? 0 : 1);
            report.result = success ? "PASS" : "FAIL";
            report.problem = problem;
            report.observedFrames = new List<string>(frames).ToArray();
            Array.Sort(report.observedFrames, StringComparer.Ordinal);
            Directory.CreateDirectory("Build/Reports");
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            Debug.Log("AFFIX_ENCOUNTER_" + report.result + ": " + ReportPath + (success ? "" : "\n" + problem));
            if (EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.ExitPlaymode();
            else ExitIfBatch();
        }

        private static void PlayStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode) return;
            if (SessionState.GetBool(Key + "active", false)) Finish(false, "Play mode exited before verification completed.");
            if (SessionState.GetBool(Key + "finishing", false)) ExitIfBatch();
        }

        private static void ExitIfBatch()
        {
            int code = SessionState.GetInt(Key + "exit", 1);
            SessionState.SetBool(Key + "finishing", false);
            if (Application.isBatchMode) EditorApplication.Exit(code);
        }

        public static void BuildWindows()
        {
            Directory.CreateDirectory("Build/Reports");
            try
            {
                ValidateScene();
                Directory.CreateDirectory("Build/Windows");
                var build = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { ScenePath }, locationPathName = "Build/Windows/AffixZero.exe",
                    target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
                });
                bool success = build.summary.result == BuildResult.Succeeded;
                File.WriteAllText("Build/Reports/windows-build.json", JsonUtility.ToJson(new BuildReportData {
                    result = build.summary.result.ToString(), errors = (int)build.summary.totalErrors,
                    warnings = (int)build.summary.totalWarnings, output = build.summary.outputPath
                }, true));
                if (!success) throw new InvalidOperationException("Windows build failed: " + build.summary.result);
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                File.WriteAllText("Build/Reports/windows-build.json", JsonUtility.ToJson(new BuildReportData { result = "FAIL", problem = ex.ToString() }, true));
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        [Serializable]
        private sealed class BuildReportData
        {
            public string recordedUtc = DateTime.UtcNow.ToString("O");
            public string result;
            public string problem = "";
            public int errors;
            public int warnings;
            public string output;
            public string playerLaunched = "NOT_RUN";
            public string userVisualApproval = "NOT_RUN";
        }
    }
}
