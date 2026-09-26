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
    // Isolated layout/reference capture, not an unattended balance benchmark.
    public sealed class ReferenceUiProbe : MonoBehaviour
    {
        private FirstEncounter owner;
        private Report report;
        private string outputDirectory, phase = "initializing";
        private float started;
        private int phaseFrame, pointsBeforeSelection, damageBeforeEquip, damageBeforeForge, goldBeforeForge;
        private bool finishing;
        private readonly HashSet<MeleeActor> observed = new HashSet<MeleeActor>();
        private readonly HashSet<string> captures = new HashSet<string>();
        private readonly List<FrameReport> frames = new List<FrameReport>();
        private readonly List<string> fixtureItems = new List<string>();
        private readonly List<string> callbacks = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-affixUiReferenceTest") < 0) return;
            var host = new GameObject("Affix UI Reference Probe");
            DontDestroyOnLoad(host);
            host.AddComponent<ReferenceUiProbe>().Begin(args);
        }

        private void Begin(string[] args)
        {
            report = new Report(); started = Time.realtimeSinceStartup;
            outputDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "Build", "Reports"));
            Application.logMessageReceived += OnLog;
            try
            {
                int index = Array.IndexOf(args, "-affixReportDir");
                Require(index >= 0 && index + 1 < args.Length && Path.IsPathRooted(args[index + 1]),
                    "UI reference verification requires an absolute -affixReportDir.");
                outputDirectory = Path.GetFullPath(args[index + 1]);
                foreach (string other in new[] { "-affixSaveTest", "-affixSmokeTest", "-affixAutoHuntTest", "-affixAutoHuntSafetyTest" })
                    Require(Array.IndexOf(args, other) < 0, "UI reference fixture cannot run with another probe: " + other);
                Directory.CreateDirectory(outputDirectory); WriteReport();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void LateUpdate()
        {
            if (finishing || report == null) return;
            try
            {
                Require(!Input.anyKeyDown && Input.mouseScrollDelta.sqrMagnitude == 0,
                    "Physical input invalidated this programmatic UI verification.");
                if (Time.realtimeSinceStartup - started > 60) throw new TimeoutException("UI reference probe timed out in " + phase);
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
                AssertIsolation();
                if (owner.Hunt == null || !owner.Hunt.Initialized || !owner.Hero.IsReady || !Ready("autohunt-toggle")) return;
                Require(owner.Progression.TotalDamage == 30 && owner.Progression.TotalGold == 0 &&
                    owner.Progression.TotalExperience == 0 && owner.Progression.Inventory.Count == 0,
                    "UI reference fixture requires a fresh temporary profile.");
                Require(!owner.Hunt.Running && owner.IsPaused, "Reference startup must be stopped.");
                foreach (var actor in owner.Hunt.Enemies)
                    if (observed.Add(actor)) actor.Damaged += OnEnemyDamaged;
                Dispatch("autohunt-toggle"); Require(owner.Hunt.Running && !owner.IsPaused, "Native start callback failed.");
                Phase("battle"); return;
            }
            AssertIsolation();
            Require(string.IsNullOrEmpty(owner.Hunt.LastFault), "Director fault during reference capture: " + owner.Hunt.LastFault);
            Require(owner.IsPaused ? Time.timeScale == 0 : Time.timeScale == 1, "Unexpected time scaling.");
            if (Time.frameCount - phaseFrame < 2) return;
            HeroProgression p = owner.Progression;
            switch (phase)
            {
                case "battle":
                    if (report.actualHeroHits == 0) return;
                    Capture("battle"); Phase("battle-capture"); break;
                case "battle-capture":
                    if (!captures.Contains("battle")) return;
                    Dispatch("autohunt-toggle"); Require(!owner.Hunt.Running && owner.IsPaused, "Native stop callback failed.");
                    SeedFixture();
                    Dispatch("character-tab"); Phase("equipment-open"); break;
                case "equipment-open":
                    Require(owner.Screen == ManagementScreen.Equipment, "Equipment tab did not open.");
                    damageBeforeEquip = p.TotalDamage; Dispatch("inventory-slot-0"); Phase("equipment-selected"); break;
                case "equipment-selected":
                    Require(Text("selected-item-name").Contains(p.Inventory[0].Name), "Selected item name is stale.");
                    Require(p.TotalDamage == damageBeforeEquip, "Selecting an inventory slot changed combat damage.");
                    string selectedId = p.Inventory[0].Id; int delta = p.CompareDamage(0).Value;
                    Dispatch("equip-button");
                    Require(p.EquippedWeapon.Id == selectedId && p.TotalDamage == damageBeforeEquip + delta && owner.Hero.Damage == p.TotalDamage,
                        "Native equip callback did not apply the real selected item.");
                    report.equipVerified = true; Dispatch("inventory-slot-1"); Phase("equipment-compare"); break;
                case "equipment-compare":
                    Require(Text("selected-item-name").Contains(p.Inventory[1].Name) && !string.IsNullOrWhiteSpace(Text("comparison-delta")),
                        "Comparison selection is absent from equipment UI.");
                    Capture("equipment"); Phase("equipment-capture"); break;
                case "equipment-capture":
                    if (!captures.Contains("equipment")) return;
                    Dispatch("talents-tab"); Phase("talents-open"); break;
                case "talents-open":
                    Require(owner.Screen == ManagementScreen.Talents, "Talent tab did not open.");
                    pointsBeforeSelection = p.UnspentPoints; Dispatch("talent-node-precision"); Phase("talents-locked"); break;
                case "talents-locked":
                    Require(p.UnspentPoints == pointsBeforeSelection && !Element("talent-invest").enabledInHierarchy && Text("talent-detail-name").Contains("정밀"),
                        "Locked node selection spent points or enabled investment.");
                    report.lockedSelectionVerified = true; Dispatch("talent-node-fury"); Phase("talents-fury"); break;
                case "talents-fury":
                    Dispatch("talent-invest"); Require(p.FuryRank == 1 && p.UnspentPoints == pointsBeforeSelection - 1, "First Fury investment failed.");
                    Phase("talents-fury-two"); break;
                case "talents-fury-two":
                    Dispatch("talent-invest"); Require(p.FuryRank == 2 && p.UnspentPoints == pointsBeforeSelection - 2, "Second Fury investment failed.");
                    Dispatch("talent-node-precision"); Phase("talents-precision"); break;
                case "talents-precision":
                    Dispatch("talent-invest"); Require(p.PrecisionRank == 1 && p.UnspentPoints == pointsBeforeSelection - 3 && owner.Hero.Damage == p.TotalDamage,
                        "Precision investment did not update real progression and actor damage.");
                    report.talentVerified = true; Dispatch("talent-node-keystone"); Phase("talents-detail"); break;
                case "talents-detail":
                    Require(Text("talent-detail-name").Contains("숙련") && Element("talent-invest").enabledInHierarchy,
                        "Unlocked keystone detail is not displayed.");
                    Capture("talents"); Phase("talents-capture"); break;
                case "talents-capture":
                    if (!captures.Contains("talents")) return;
                    Dispatch("forge-tab"); Phase("forge-open"); break;
                case "forge-open":
                    Require(owner.Screen == ManagementScreen.Forge, "Forge tab did not open.");
                    damageBeforeForge = p.TotalDamage; goldBeforeForge = p.TotalGold; int cost = p.EquippedEnhancementCost;
                    Dispatch("forge-enhance");
                    Require(p.EquippedWeapon.EnhancementRank == 1 && p.TotalDamage == damageBeforeForge + 2 &&
                        p.TotalGold == goldBeforeForge - cost && owner.Hero.Damage == p.TotalDamage,
                        "Native enhancement did not charge exact gold and apply real damage.");
                    report.forgeVerified = true; Phase("forge-enhanced"); break;
                case "forge-enhanced":
                    Require(Text("forge-item-name").Contains(p.EquippedWeapon.Name) && Text("forge-wallet").Contains(p.TotalGold.ToString()),
                        "Forge result labels are stale.");
                    Capture("forge"); Phase("forge-capture"); break;
                case "forge-capture":
                    if (!captures.Contains("forge")) return;
                    Require(captures.Count == 4 && report.equipVerified && report.talentVerified && report.forgeVerified,
                        "Required native screens or mutations are incomplete.");
                    Finish(true, null); break;
            }
        }

        private void SeedFixture()
        {
            HeroProgression p = owner.Progression;
            report.realKillsBeforeFixture = owner.Hunt.TotalKills;
            report.experienceBeforeFixture = p.TotalExperience; report.goldBeforeFixture = p.TotalGold;
            for (int i = 0; i < 6; i++)
                Require(p.TryRegisterKill("reference-ui:" + report.runId + ":kill:" + i), "Fixture kill token was rejected.");
            if (p.PendingLoot != null) Require(owner.CollectLoot(), "Guaranteed fixture weapon was not collected.");
            for (int i = 0; i < 8; i++)
            {
                string id = "reference-ui:" + report.runId + ":weapon:" + i;
                var weapon = new WeaponItem(id, new[] { "날카로운 강철검", "묵직한 장검", "잿불 수호검", "훈련용 단검" }[i % 4],
                    10 + i, 2 + i % 3, new[] { "날카로움", "묵직함", "잿불" }[i % 3],
                    i % 2 == 0 ? "AffixGenerated/EmberSword" : "AffixGenerated/AttackIcon", i % 2 == 0 ? "Rare" : "Common");
                Require(p.TryCreatePendingLoot(weapon) && owner.CollectLoot(), "Fixture weapon intake failed."); fixtureItems.Add(id);
            }
            Require(p.TotalExperience == report.experienceBeforeFixture + 150 && p.TotalGold == report.goldBeforeFixture + 48 &&
                p.Inventory.Count == 9 && p.EquippedWeapon.Id == "equipped:starting-sword" && p.TotalDamage == 30,
                "Fixture intake changed unexpected state.");
            report.fixtureApplied = true;
        }

        private void OnEnemyDamaged(MeleeActor actor, HitReceipt receipt)
        {
            if (receipt.Accepted && owner != null && actor.LastAttackerId == owner.Hero.ActorId) report.actualHeroHits++;
        }
        private void AssertIsolation()
        {
            Require(owner.Persistence.Ephemeral && owner.Persistence.CanPlay && string.IsNullOrEmpty(owner.Persistence.FilePath) &&
                owner.Persistence.SaveCount == 0, "Reference fixture must never open or modify a real profile store.");
            report.ephemeralVerified = true;
        }
        private void Phase(string value) { phase = value; phaseFrame = Time.frameCount; }
        private VisualElement Root => owner.GetComponent<UIDocument>()?.rootVisualElement;
        private VisualElement Element(string name)
        {
            VisualElement value = Root?.Q(name); Require(value != null, "Missing native element: " + name); return value;
        }
        private string Text(string name)
        {
            var label = Element(name) as Label; Require(label != null, "Named value is not a Label: " + name); return label.text ?? "";
        }
        private bool Ready(string name)
        {
            var value = Root?.Q(name);
            return value != null && value.panel != null && value.enabledInHierarchy && value.worldBound.width > 0;
        }
        private static bool Visible(VisualElement value)
        {
            for (var node = value; node != null; node = node.parent)
                if (node.resolvedStyle.display == DisplayStyle.None || node.resolvedStyle.visibility != Visibility.Visible) return false;
            return true;
        }
        private void Dispatch(string name)
        {
            var element = Element(name);
            Require(element.enabledInHierarchy && element.panel != null && Visible(element) && element.worldBound.width > 0 && element.worldBound.height > 0,
                "Native control is disabled, hidden or has no layout: " + name);
            using (var click = ClickEvent.GetPooled()) { click.target = element; element.SendEvent(click); }
            callbacks.Add(name);
        }

        private void Capture(string label) { StartCoroutine(CaptureFrame(label)); }
        private IEnumerator CaptureFrame(string label)
        {
            yield return null; yield return new WaitForEndOfFrame();
            if (finishing) yield break;
            Texture2D texture = null;
            try
            {
                AssertIsolation();
                Require(Root != null && Root.name == "stitch-hud" && Root.panel != null, "Native HUD is missing.");
                Require(Text("hero-hp-value").Contains(owner.Hero.Hp.ToString()) && Text("gold-value").Contains(owner.Progression.TotalGold.ToString()),
                    "HUD values differ from runtime state.");
                FrameReport frame = CheckLayout(label);
                texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); texture.Apply();
                frame.imagePath = Path.Combine(outputDirectory, "ui-" + report.runId + "-" + label + ".bmp");
                WriteBitmap(frame.imagePath, texture); frames.Add(frame); captures.Add(label);
                WriteReport();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
            finally { if (texture != null) Destroy(texture); }
        }

        private FrameReport CheckLayout(string screen)
        {
            var p = owner.Progression;
            var frame = new FrameReport { screen = screen, pixelWidth = Screen.width, pixelHeight = Screen.height,
                panelWidth = Root.worldBound.width, panelHeight = Root.worldBound.height, hp = owner.Hero.Hp,
                experience = p.TotalExperience, gold = p.TotalGold, damage = p.TotalDamage, actorDamage = owner.Hero.Damage,
                points = p.UnspentPoints, fury = p.FuryRank, precision = p.PrecisionRank, keystone = p.KeystoneRank,
                enhancement = p.EquippedWeapon.EnhancementRank, equippedId = p.EquippedWeapon.Id, inventoryCount = p.Inventory.Count,
                running = owner.Hunt.Running, paused = owner.IsPaused };
            var bounds = new List<BoundReport>();
            Rect panel = Root.worldBound;
            var top = Element("top-navigation"); var bottom = Element("bottom-hud");
            Record(top, panel, bounds); Record(bottom, panel, bounds); NoOverlap(top, bottom);
            string[] nav = { "dungeon-tab", "character-tab", "talents-tab", "forge-tab", "autohunt-toggle", "pause-button", "gold-value" };
            CheckGroup(nav, top.worldBound, bounds, true);
            CheckGroup(new[] { "hero-hp-value", "attack-slot", "xp-value" }, bottom.worldBound, bounds, true);
            string active = screen == "equipment" ? "character-panel" : screen == "talents" ? "talent-screen" : screen == "forge" ? "forge-screen" : null;
            foreach (string name in new[] { "character-panel", "talent-screen", "forge-screen" })
                Require(Visible(Element(name)) == (name == active), "Management visibility differs from selected screen: " + name);
            if (active != null)
            {
                var management = Element(active); Record(management, panel, bounds); NoOverlap(management, top); NoOverlap(management, bottom);
                if (screen == "equipment")
                {
                    var controls = new List<string> { "equip-button", "close-character" };
                    for (int i = 0; i < 24; i++) controls.Add("inventory-slot-" + i);
                    CheckGroup(controls.ToArray(), management.worldBound, bounds, true);
                    CheckGroup(new[] { "selected-item-name", "comparison-delta" }, management.worldBound, bounds, false);
                    frame.selectedItem = Text("selected-item-name"); frame.comparison = Text("comparison-delta");
                    frame.comparisonDamage = p.CompareDamage(1).Value;
                }
                else if (screen == "talents")
                {
                    CheckGroup(new[] { "talent-node-fury", "talent-node-precision", "talent-node-keystone", "talent-invest", "talent-screen-reset", "talent-screen-close" }, management.worldBound, bounds, true);
                    CheckGroup(new[] { "talent-detail-name", "talent-detail-effect", "talent-screen-points" }, management.worldBound, bounds, false);
                    frame.selectedTalent = Text("talent-detail-name");
                }
                else
                {
                    CheckGroup(new[] { "forge-enhance", "forge-close", "forge-equipment" }, management.worldBound, bounds, true);
                    CheckGroup(new[] { "forge-item-name", "forge-attack-damage", "forge-cost", "forge-wallet" }, management.worldBound, bounds, false);
                    frame.forgePreview = Text("forge-attack-damage"); frame.forgeCost = Text("forge-cost");
                }
            }
            else CheckGroup(new[] { "enemy-health", "minimap" }, panel, bounds, true);
            frame.bounds = bounds.ToArray(); frame.layoutVerified = true; return frame;
        }
        private void CheckGroup(string[] names, Rect container, List<BoundReport> records, bool nonOverlapping)
        {
            for (int i = 0; i < names.Length; i++)
            {
                var element = Element(names[i]); Record(element, container, records);
                if (nonOverlapping) for (int j = 0; j < i; j++) NoOverlap(element, Element(names[j]));
            }
        }
        private static void Record(VisualElement element, Rect container, List<BoundReport> records)
        {
            Rect r = element.worldBound;
            Require(Visible(element) && r.width > 0 && r.height > 0 && !float.IsNaN(r.x) && !float.IsNaN(r.y), "Missing visible bounds: " + element.name);
            Require(r.xMin >= container.xMin - 1 && r.yMin >= container.yMin - 1 && r.xMax <= container.xMax + 1 && r.yMax <= container.yMax + 1,
                "Important element overflows its screen region: " + element.name + " " + r + " container=" + container);
            records.Add(new BoundReport { name = element.name, x = r.x, y = r.y, width = r.width, height = r.height, enabled = element.enabledInHierarchy });
        }
        private static void NoOverlap(VisualElement a, VisualElement b)
        {
            Rect x = a.worldBound, y = b.worldBound;
            float width = Mathf.Min(x.xMax, y.xMax) - Mathf.Max(x.xMin, y.xMin);
            float height = Mathf.Min(x.yMax, y.yMax) - Mathf.Max(x.yMin, y.yMin);
            Require(width <= 1 || height <= 1, "Important controls overlap: " + a.name + " / " + b.name);
        }

        private static void WriteBitmap(string path, Texture2D texture)
        {
            int width = texture.width, height = texture.height, rowBytes = checked(width * 3), stride = checked(rowBytes + 3) & ~3, imageBytes = checked(stride * height);
            Color32[] colors = texture.GetPixels32();
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write((ushort)0x4D42); writer.Write(checked(54 + imageBytes)); writer.Write(0); writer.Write(54); writer.Write(40);
                writer.Write(width); writer.Write(height); writer.Write((ushort)1); writer.Write((ushort)24); writer.Write(0); writer.Write(imageBytes);
                writer.Write(2835); writer.Write(2835); writer.Write(0); writer.Write(0);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++) { Color32 c = colors[y * width + x]; writer.Write(c.b); writer.Write(c.g); writer.Write(c.r); }
                    for (int pad = rowBytes; pad < stride; pad++) writer.Write((byte)0);
                }
            }
        }
        private void OnLog(string message, string trace, LogType type)
        { if (!finishing && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) Finish(false, message + "\n" + trace); }
        private void Finish(bool success, string problem)
        {
            if (finishing) return; finishing = true;
            Application.logMessageReceived -= OnLog;
            foreach (var actor in observed) if (actor != null) actor.Damaged -= OnEnemyDamaged;
            report.result = report.status = success ? "PASS" : "FAIL"; report.problem = problem ?? "";
            report.finishedUtc = DateTime.UtcNow.ToString("O");
            try { WriteReport(); }
            catch (Exception error) { Debug.LogError("UI reference report write failed: " + error); success = false; }
            Debug.Log("AFFIX_UI_REFERENCE_" + (success ? "PASS" : "FAIL") + " run=" + report.runId);
            Application.Quit(success ? 0 : 1);
        }
        private void WriteReport()
        {
            report.phase = phase; report.elapsedSeconds = Time.realtimeSinceStartup - started;
            report.frames = frames.ToArray(); report.fixtureItemIds = fixtureItems.ToArray(); report.nativeCallbacks = callbacks.ToArray();
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "ui-reference-probe.json"), JsonUtility.ToJson(report, true));
        }
        private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        [Serializable] private sealed class Report
        {
            public string schema = "affix-ui-reference-v1", runId = Guid.NewGuid().ToString("N"), startedUtc = DateTime.UtcNow.ToString("O"), finishedUtc = "";
            public string buildGuid = Application.buildGUID, unityVersion = Application.unityVersion, result = "RUNNING", status = "RUNNING", problem = "", phase;
            public string scope = "Actual Windows framebuffer and native UI Toolkit callbacks. Geometry checks cover named important controls only; font glyph clipping and artistic/reference fidelity require visual review.";
            public string fixture = "After a real first attack and native Stop callback: six unique Core kill tokens grant 150 XP, 48 gold and six points; guaranteed first drop plus eight authored test weapons enter the temporary inventory through public APIs. Native equip, three talent investments and one gold-funded enhancement modify real progression. Not a balance, reward-rate or unattended-farming benchmark.";
            public string actualPhysicalInput = "NOT_RUN; external key/mouse-button/scroll input rejects the run", userVisualApproval = "NOT_APPROVED";
            public bool ephemeralVerified, fixtureApplied, equipVerified, lockedSelectionVerified, talentVerified, forgeVerified;
            public int actualHeroHits, realKillsBeforeFixture, experienceBeforeFixture, goldBeforeFixture;
            public float elapsedSeconds;
            public string[] fixtureItemIds, nativeCallbacks;
            public FrameReport[] frames;
        }
        [Serializable] private sealed class FrameReport
        {
            public string screen, imagePath, equippedId, selectedItem, comparison, selectedTalent, forgePreview, forgeCost;
            public int pixelWidth, pixelHeight, hp, experience, gold, damage, actorDamage, points, fury, precision, keystone, enhancement, inventoryCount, comparisonDamage;
            public float panelWidth, panelHeight;
            public bool running, paused, layoutVerified;
            public BoundReport[] bounds;
        }
        [Serializable] private sealed class BoundReport
        { public string name; public float x, y, width, height; public bool enabled; }
    }
}
#endif
