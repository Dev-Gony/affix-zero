#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AffixZero.Core;
using UnityEngine;

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

        private void Update()
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
                report.actorsInitialized = true;
                if (restarted) { report.restartFullHp = true; report.restartRewardsReset = true; }
                heroStart = hero.transform.position;
                enemyStart = enemy.transform.position;
                hero.Damaged += OnDamaged;
                enemy.Damaged += OnDamaged;
                phase = "combat";
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
                if (elapsed > 0.5f) CaptureOnce("cleared");
                if (elapsed > 1.25f && pendingCaptures == 0)
                {
                    report.noDamageAfterDeath = true;
                    report.rewardCollectedOnce = true;
                    report.firstClearHeroHp = hero.Hp;
                    DetachActors();
                    oldEncounterId = encounter.GetInstanceID();
                    restarted = true;
                    phase = "restartWaiting";
                    encounter.RestartEncounter(); // Same path as the player's button.
                }
            }
            if (phase == "complete" && pendingCaptures == 0)
            {
                Require(report.approachObserved && report.heroDamageObserved && report.enemyDamageObserved &&
                    report.enemyDeathObserved && report.impactPoseObserved && report.noDamageAfterDeath &&
                    report.rewardCollectedOnce && report.restartFullHp && report.restartRewardsReset &&
                    report.restartCombatObserved && screenshots.Count == 3, "Required observations or screenshots are missing.");
                Finish(true, "");
            }
        }

        private void OnDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (finishing) return;
            try
            {
                Require(receipt.Accepted, "Damage event did not carry an accepted receipt.");
                if (actor == hero) report.heroDamageObserved = true;
                if (actor == enemy) report.enemyDamageObserved = true;
                var attacker = actor == hero ? enemy : hero;
                Require(attacker.GetComponent<SpriteRenderer>().sprite == attacker.AnimationSet.ImpactSprite,
                    "Damage did not coincide with the actual impact sprite.");
                report.impactPoseObserved = true;
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
            yield return new WaitForEndOfFrame();
            if (finishing) yield break;
            Texture2D pixels = null;
            try
            {
                Require(Screen.width > 0 && Screen.height > 0, "Player framebuffer has no size.");
                pixels = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                pixels.Apply();
                string path = Path.Combine(outputDirectory, "player-" + report.runId + "-" + label + ".bmp");
                WriteBitmap(path, pixels);
                screenshots.Add(path);
                pendingCaptures--;
            }
            catch (Exception error) { Finish(false, error.ToString()); }
            finally { if (pixels != null) Destroy(pixels); }
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
            public string scope = "Actual standalone Windows player launch, normal automatic combat, one UI-path scene restart.";
            public string screenshotScope = "Uncompressed 24-bit BMP from end-of-frame ReadPixels framebuffer, including the actual IMGUI HUD.";
            public string userVisualApproval = "NOT_APPROVED";
            public bool actorsInitialized, approachObserved, heroDamageObserved, enemyDamageObserved;
            public bool enemyDeathObserved, impactPoseObserved, noDamageAfterDeath, rewardCollectedOnce;
            public bool restartFullHp, restartRewardsReset, restartCombatObserved;
            public int firstClearHeroHp;
            public string[] screenshots = Array.Empty<string>();
        }
    }
}
#endif
