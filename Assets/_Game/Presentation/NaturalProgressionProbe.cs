#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Long-running product-flow observation. It uses normal drop rolls and an isolated disk profile.
    public sealed class NaturalProgressionProbe : MonoBehaviour
    {
        private float observationSeconds = 1200f;
        private FirstEncounter owner;
        private AutoHuntDirector hunt;
        private Report report;
        private string mode, reportDirectory, saveDirectory, expectedPath, captureDirectory, phase = "initializing";
        private float started, nextCheckpoint, nextCaptureAt;
        private bool finishing, busy;
        private int readStartKills;
        private readonly HashSet<string> processedItems = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<string> dropRecords = new List<string>();
        private readonly List<string> captures = new List<string>();
        private readonly int[] captureCounts = new int[3];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-affixNaturalProgressionTest") < 0) return;
            var host = new GameObject("Affix Natural Progression Probe");
            DontDestroyOnLoad(host);
            host.AddComponent<NaturalProgressionProbe>().Begin(args);
        }

        private void Begin(string[] args)
        {
            started = Time.realtimeSinceStartup; nextCheckpoint = 30;
            report = new Report(); Application.logMessageReceived += OnLog;
            try
            {
                mode = Argument(args, "-affixNaturalMode");
                Require(mode == "observe" || mode == "read", "Natural mode must be observe or read.");
                reportDirectory = AbsoluteArgument(args, "-affixReportDir");
                saveDirectory = AbsoluteArgument(args, "-affixSaveDir");
                string seconds=OptionalArgument(args,"-affixObservationSeconds");
                if(!string.IsNullOrEmpty(seconds))
                    Require(float.TryParse(seconds,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out observationSeconds)&&observationSeconds>=120,
                        "Observation seconds must be at least 120.");
                string capture=OptionalArgument(args,"-affixCaptureDir");
                if(!string.IsNullOrEmpty(capture))
                {Require(Path.IsPathRooted(capture),"-affixCaptureDir requires an absolute path.");captureDirectory=Path.GetFullPath(capture);Directory.CreateDirectory(captureDirectory);}
                expectedPath = Path.Combine(saveDirectory, "expected-natural-profile.json");
                Require(Array.IndexOf(args, "-affixAutoHuntTest") < 0,
                    "Natural progression must not enable guaranteed QA drops.");
                report.mode = mode; report.normalDropRolls = true;
                Directory.CreateDirectory(reportDirectory); WriteReport();
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void LateUpdate()
        {
            if (finishing || report == null) return;
            try
            {
                if (Time.realtimeSinceStartup - started > (mode == "observe" ? observationSeconds+120 : 150))
                    throw new TimeoutException("Natural progression timed out in " + phase);
                Tick();
                if (Time.realtimeSinceStartup - started >= nextCheckpoint)
                { nextCheckpoint += 30; Snapshot(); WriteReport(); }
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private void Tick()
        {
            if (phase == "initializing")
            {
                owner = FindFirstObjectByType<FirstEncounter>();
                if (owner == null || owner.Hunt == null || !owner.Hunt.Initialized || owner.Hero == null || !owner.Hero.IsReady || !Ready("autohunt-toggle")) return;
                hunt = owner.Hunt;
                Require(owner.Persistence.CanPlay && !owner.Persistence.Ephemeral,
                    "Natural profile is not writable: " + owner.Persistence.Problem);
                Require(string.Equals(Path.GetFullPath(owner.Persistence.FilePath), Path.Combine(saveDirectory, "profile-v1.json"), StringComparison.OrdinalIgnoreCase),
                    "Natural progression escaped its isolated save directory.");
                Require(!hunt.Running && owner.IsPaused && Time.timeScale == 0, "Profile did not start stopped at 1x-ready state.");
                if (mode == "observe")
                {
                    Require(owner.Progression.TotalExperience == 0 && owner.Progression.TotalGold == 0 && owner.Progression.Inventory.Count == 0,
                        "Observe mode requires a fresh profile.");
                    report.initialDamage = owner.Progression.TotalDamage; report.initialHealth = owner.Progression.TotalMaxHp;
                    Dispatch("autohunt-toggle"); report.startClicks++;
                    Require(hunt.Running && Time.timeScale == 1, "Native start callback did not begin 1x play.");
                    phase = "observing";
                }
                else
                {
                    Require(File.Exists(expectedPath), "Expected natural profile is missing.");
                    string expected = File.ReadAllText(expectedPath);
                    Require(ProfilePersistence.Encode(owner.Progression) == expected,
                        "Restarted profile differs before continued play.");
                    report.restoreMatched = true; readStartKills = hunt.TotalKills;
                    Dispatch("autohunt-toggle"); report.startClicks++;
                    phase = "read-continuation";
                }
                return;
            }

            Require(owner != null && hunt != null && owner.Persistence.CanPlay, "Runtime progression owner disappeared or became unavailable.");
            Require(owner.IsPaused ? Time.timeScale == 0 : Time.timeScale == 1, "Natural run changed simulation speed.");
            DiscoverDrops();
            CaptureVarietyFrame();

            if (busy) return;
            if (mode == "observe" && TryDifficultyTransition()) return;
            if (TryBeginGrowthAction()) return;

            if (!hunt.Running)
            {
                Require(hunt.Phase == HuntPhase.Blocked,
                    "Hunt stopped outside its explicit safety state: " + hunt.LastFault);
                Dispatch("autohunt-toggle"); report.startClicks++; report.safetyRestarts++;
                Require(hunt.Running && !owner.Hero.IsDead,
                    "Explicit post-safety-stop start did not create a fresh entrance state.");
            }

            float elapsed = Time.realtimeSinceStartup - started;
            if (mode == "observe" && elapsed >= observationSeconds)
            {
                hunt.StopHunt();
                Require(owner.SaveProgress(), "Final natural-profile save failed: " + owner.Persistence.Problem);
                string canonical = ProfilePersistence.Encode(owner.Progression);
                File.WriteAllText(expectedPath, canonical);
                Require(File.ReadAllText(owner.Persistence.FilePath) == canonical && File.ReadAllText(expectedPath) == canonical,
                    "Natural profile did not round-trip to disk.");
                report.diskRoundtripMatched = true;
                Require(hunt.TotalKills >= 10, "Twenty-minute observation did not reach ten kills.");
                Require(hunt.CollectedItems > 0, "Normal drop rolls produced no collectible progression in twenty minutes.");
                Require(report.talentInvestments > 0, "Natural XP produced no invested talent point.");
                Require(hunt.LayoutsVisitedMask == 7 && hunt.LayoutTransitions >= 2,
                    "Natural run did not traverse all three connected layout topologies.");
                Require(owner.Progression.SelectedDifficulty == DungeonDifficulty.Torment && report.difficultyTransitions == 2,
                    "Natural run did not safely select and retain all three difficulties.");
                Require(owner.Progression.SpentPoints < HeroProgression.TotalTalentCapacity,
                    "Twenty-minute progression still exhausted the complete talent tree.");
                Require(owner.Progression.TotalDamage > report.initialDamage || owner.Progression.TotalMaxHp > report.initialHealth || owner.Progression.TotalDefense > 2,
                    "Natural progression did not improve any combat stat.");
                Finish(true, null);
            }
            else if (mode == "read" && hunt.TotalKills - readStartKills >= 2)
            {
                hunt.StopHunt();
                Require(owner.SaveProgress(), "Continued profile save failed: " + owner.Persistence.Problem);
                report.continuedAfterRestart = true; report.diskRoundtripMatched = true;
                Finish(true, null);
            }
        }

        private void CaptureVarietyFrame()
        {
            if(string.IsNullOrEmpty(captureDirectory)||hunt==null||hunt.World==null||!hunt.Running||owner.ManagementVisible||
                Time.realtimeSinceStartup<nextCaptureAt)return;
            int layout=(int)hunt.World.LayoutId;
            if(layout<0||layout>=captureCounts.Length||captureCounts[layout]>=16)return;
            nextCaptureAt=Time.realtimeSinceStartup+.25f;
            string path=Path.Combine(captureDirectory,"layout-"+layout+"-"+captureCounts[layout].ToString("00")+".bmp");
            captureCounts[layout]++;captures.Add(path);StartCoroutine(CaptureFrame(path));
        }

        private IEnumerator CaptureFrame(string path)
        {
            yield return new WaitForEndOfFrame();Texture2D pixels=null;
            try
            {
                pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();WriteBitmap(path,pixels);
            }
            finally{if(pixels!=null)Destroy(pixels);}
        }
        private static void WriteBitmap(string path,Texture2D texture)
        {
            int width=texture.width,height=texture.height,rowBytes=checked(width*3),stride=checked(rowBytes+3)&~3,imageBytes=checked(stride*height);
            Color32[] colors=texture.GetPixels32();
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write((ushort)0x4D42);writer.Write(checked(54+imageBytes));writer.Write(0);writer.Write(54);writer.Write(40);
                writer.Write(width);writer.Write(height);writer.Write((ushort)1);writer.Write((ushort)24);writer.Write(0);writer.Write(imageBytes);
                writer.Write(2835);writer.Write(2835);writer.Write(0);writer.Write(0);
                for(int y=0;y<height;y++)
                {
                    for(int x=0;x<width;x++){Color32 c=colors[y*width+x];writer.Write(c.b);writer.Write(c.g);writer.Write(c.r);}
                    for(int pad=rowBytes;pad<stride;pad++)writer.Write((byte)0);
                }
            }
        }

        private bool TryDifficultyTransition()
        {
            DungeonDifficulty current=owner.Progression.SelectedDifficulty;
            DungeonDifficulty next=current==DungeonDifficulty.Scout&&hunt.CompletedRuns>=1?DungeonDifficulty.Veteran:
                current==DungeonDifficulty.Veteran&&hunt.CompletedRuns>=2?DungeonDifficulty.Torment:current;
            if(next==current||owner.Progression.PendingLoot!=null)return false;
            StartCoroutine(DifficultyFlow(next));return true;
        }

        private IEnumerator DifficultyFlow(DungeonDifficulty next)
        {
            busy=true;hunt.StopHunt();Require(hunt.CanChangeDifficulty,"Difficulty selection was not safe after stopping the hunt.");
            string control="difficulty-"+next.ToString().ToLowerInvariant();yield return WaitUntilReady(control);
            Dispatch(control);yield return null;
            Require(owner.Progression.SelectedDifficulty==next&&!hunt.Running,
                "Native difficulty callback did not apply a stopped-state selection.");
            report.difficultyTransitions++;Dispatch("autohunt-toggle");report.startClicks++;
            Require(hunt.Running&&Time.timeScale==1,"Difficulty re-entry did not resume 1x automatic hunting.");
            busy=false;
        }

        private void DiscoverDrops()
        {
            foreach (WeaponItem item in owner.Progression.Inventory)
            {
                if (processedItems.Contains(item.Id)) continue;
                processedItems.Add(item.Id);
                dropRecords.Add(item.Id + "|" + item.Rarity + "|" + item.EquipmentSlot + "|score=" + Score(item));
                report.naturalItemsSeen++;
            }
        }

        private bool TryBeginGrowthAction()
        {
            HeroProgression p = owner.Progression;
            for (int i = 0; i < p.Inventory.Count; i++)
            {
                WeaponItem item = p.Inventory[i]; WeaponItem current = p.GetEquipped(item.EquipmentSlot);
                if (Score(item) > Score(current))
                { StartCoroutine(EquipFlow(item.Id, item.EquipmentSlot)); return true; }
            }
            if (p.Inventory.Count >= 20)
            {
                int worst = 0;
                for (int i = 1; i < p.Inventory.Count; i++)
                    if (Score(p.Inventory[i]) < Score(p.Inventory[worst])) worst = i;
                StartCoroutine(DiscardFlow(p.Inventory[worst].Id)); return true;
            }
            if (p.UnspentPoints > 0 && p.SpentPoints < HeroProgression.TotalTalentCapacity)
            {
                TalentId next = p.VitalityRank < 20 ? TalentId.Vitality : p.FuryRank < 20 ? TalentId.Fury :
                    p.PrecisionRank < 10 ? TalentId.Precision : p.KeystoneRank < 5 ? TalentId.Keystone :
                    p.CleaveRank < 10 ? TalentId.Cleave : TalentId.Haste;
                StartCoroutine(TalentFlow(next)); return true;
            }
            if (report.enhancements < 3 && p.CanEnhanceEquipped)
            { StartCoroutine(ForgeFlow()); return true; }
            return false;
        }

        private IEnumerator EquipFlow(string id, EquipmentSlot slot)
        {
            busy = true; WeaponItem before = owner.Progression.GetEquipped(slot); int beforeScore = Score(before);
            Dispatch("character-tab"); yield return WaitUntilReady("inventory-slot-0");
            int index = IndexOf(id); Require(index >= 0, "Natural item disappeared before comparison.");
            Dispatch("inventory-slot-" + index); yield return WaitUntilReady("equip-button");
            Dispatch("equip-button"); yield return null;
            WeaponItem equipped = owner.Progression.GetEquipped(slot);
            Require(equipped != null && equipped.Id == id && Score(equipped) > beforeScore,
                "Native equipment comparison/equip flow did not apply the selected upgrade.");
            report.equipmentComparisons++; report.equipmentUpgrades++;
            processedItems.Add(id); if (before != null) processedItems.Add(before.Id);
            Dispatch("dungeon-tab"); busy = false;
        }

        private IEnumerator TalentFlow(TalentId talent)
        {
            busy = true; int before = owner.Progression.GetTalentRank(talent);
            Dispatch("talents-tab"); yield return WaitUntilReady("talent-node-" + talent.ToString().ToLowerInvariant());
            Dispatch("talent-node-" + talent.ToString().ToLowerInvariant()); yield return null;
            Dispatch("talent-invest"); yield return null;
            Require(owner.Progression.GetTalentRank(talent) == before + 1,
                "Native talent selection/investment did not apply " + talent + ".");
            report.talentInvestments++; Dispatch("dungeon-tab"); busy = false;
        }

        private IEnumerator DiscardFlow(string id)
        {
            busy = true; int before = owner.Progression.Inventory.Count;
            Dispatch("character-tab"); yield return WaitUntilReady("inventory-slot-0");
            int index = IndexOf(id); Require(index >= 0, "Discard candidate disappeared before comparison.");
            Dispatch("inventory-slot-" + index); yield return WaitUntilReady("salvage-button");
            Dispatch("salvage-button"); yield return null; Dispatch("salvage-button"); yield return null;
            Require(owner.Progression.Inventory.Count == before - 1 && IndexOf(id) < 0,
                "Native two-step discard did not free one bag slot.");
            report.discardedItems++; Dispatch("dungeon-tab"); busy = false;
        }

        private IEnumerator ForgeFlow()
        {
            busy = true; int before = owner.Progression.EquippedWeapon.EnhancementRank;
            Dispatch("forge-tab"); yield return WaitUntilReady("forge-enhance");
            Dispatch("forge-enhance"); yield return null;
            Require(owner.Progression.EquippedWeapon.EnhancementRank == before + 1,
                "Native forge action did not enhance the equipped weapon.");
            report.enhancements++; Dispatch("dungeon-tab"); busy = false;
        }

        private int IndexOf(string id)
        { for (int i = 0; i < owner.Progression.Inventory.Count; i++) if (owner.Progression.Inventory[i].Id == id) return i; return -1; }
        private static int Score(WeaponItem item) => item == null ? 0 : item.DamageBonus * 4 + item.DefenseBonus * 12 +
            item.HealthBonus / 4 + item.VampirismPercent * 4 + item.CriticalChance * 2 + item.Penetration + item.CooldownReductionPercent;

        private VisualElement Root => owner.GetComponent<UIDocument>()?.rootVisualElement;
        private bool Ready(string name)
        { VisualElement e = Root?.Q(name); return e != null && e.panel != null && e.enabledInHierarchy && e.worldBound.width > 0; }
        private IEnumerator WaitUntilReady(string name)
        {
            for (int i = 0; i < 30 && !Ready(name); i++) yield return new WaitForSecondsRealtime(.05f);
            Require(Ready(name), "Native control did not become ready: " + name);
        }
        private void Dispatch(string name)
        {
            VisualElement element = Root?.Q(name); Require(element != null && element.panel != null && element.enabledInHierarchy,
                "Native control unavailable: " + name);
            for (VisualElement node = element; node != null; node = node.parent)
                Require(node.resolvedStyle.display != DisplayStyle.None && node.resolvedStyle.visibility == Visibility.Visible,
                    "Native control hidden: " + name);
            using (var click = ClickEvent.GetPooled()) { click.target = element; element.SendEvent(click); }
            report.uiCallbacks++;
        }

        private void Snapshot()
        {
            if (owner == null || hunt == null) return;
            HeroProgression p = owner.Progression;
            report.elapsedSeconds = Time.realtimeSinceStartup - started; report.kills = hunt.TotalKills;
            report.completedRuns = hunt.CompletedRuns; report.failedRuns = hunt.FailedRuns; report.deathRetries = hunt.DeathRetries;
            report.collectedItems = hunt.CollectedItems; report.experience = p.TotalExperience; report.gold = p.TotalGold;
            report.damage = p.TotalDamage; report.health = p.TotalMaxHp; report.defense = p.TotalDefense;
            report.inventoryCount = p.Inventory.Count; report.spentPoints = p.SpentPoints; report.unspentPoints = p.UnspentPoints;
            report.fury = p.FuryRank; report.precision = p.PrecisionRank; report.keystone = p.KeystoneRank;
            report.vitality = p.VitalityRank; report.cleave = p.CleaveRank; report.haste = p.HasteRank;
            report.areaCasts = hunt.AreaCasts; report.recoveryCasts = hunt.RecoveryCasts;
            report.layoutTransitions=hunt.LayoutTransitions;report.layoutsVisitedMask=hunt.LayoutsVisitedMask;
            report.maxKillChain=hunt.MaxKillChain;report.selectedDifficulty=(int)p.SelectedDifficulty;report.dungeonClears=p.DungeonClears;
            report.dropRecords = dropRecords.ToArray(); report.saveCount = owner.Persistence.SaveCount;
            report.captureFrames=captures.ToArray();
        }

        private void OnLog(string message, string trace, LogType type)
        { if (!finishing && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) Finish(false, message + "\n" + trace); }
        private void Finish(bool success, string problem)
        {
            if (finishing) return; finishing = true; Application.logMessageReceived -= OnLog;
            Snapshot(); report.result = report.status = success ? "PASS" : "FAIL"; report.problem = problem ?? "";
            report.phase = phase; report.finishedUtc = DateTime.UtcNow.ToString("O");
            if (owner != null && File.Exists(owner.Persistence.FilePath))
                using (var sha = SHA256.Create()) using (var stream = File.OpenRead(owner.Persistence.FilePath))
                    report.profileSha256 = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            try { WriteReport(); } catch (Exception error) { Debug.LogError("Natural report write failed: " + error); success = false; }
            Debug.Log("AFFIX_NATURAL_PROGRESSION_" + (success ? "PASS" : "FAIL") + " mode=" + mode + " run=" + report.runId);
            Application.Quit(success ? 0 : 1);
        }
        private void WriteReport() => File.WriteAllText(Path.Combine(reportDirectory, "natural-progression-" + mode + ".json"), JsonUtility.ToJson(report, true));
        private static string Argument(string[] args, string key)
        { int i = Array.IndexOf(args, key); if (i < 0 || i + 1 >= args.Length) throw new ArgumentException("Missing " + key); return args[i + 1]; }
        private static string OptionalArgument(string[] args,string key)
        {int i=Array.IndexOf(args,key);return i<0||i+1>=args.Length?null:args[i+1];}
        private static string AbsoluteArgument(string[] args, string key)
        { string value = Argument(args, key); if (!Path.IsPathRooted(value)) throw new ArgumentException(key + " requires an absolute path."); return Path.GetFullPath(value); }
        private static void Require(bool value, string problem) { if (!value) throw new InvalidOperationException(problem); }

        [Serializable] private sealed class Report
        {
            public string schema = "affix-natural-progression-v1", runId = Guid.NewGuid().ToString("N"), mode = "", result = "RUNNING", status = "RUNNING",
                problem = "", phase = "initializing", startedUtc = DateTime.UtcNow.ToString("O"), finishedUtc = "", unityVersion = Application.unityVersion,
                buildGuid = Application.buildGUID, profileSha256 = "";
            public string scope = "Fresh isolated profile, normal production drop rolls, 1x twenty-minute observation, native UI Toolkit equipment/talent/forge actions, safety-stop review/restart, disk save and separate-process continuation.";
            public string actualPhysicalInput = "NOT_RUN; this probe uses native UI Toolkit callbacks, not OS mouse or keyboard input.";
            public bool normalDropRolls, restoreMatched, diskRoundtripMatched, continuedAfterRestart;
            public float elapsedSeconds; public int initialDamage, initialHealth, kills, completedRuns, failedRuns, deathRetries, collectedItems,
                naturalItemsSeen, experience, gold, damage, health, defense, inventoryCount, spentPoints, unspentPoints,
                fury, precision, keystone, vitality, cleave, haste, areaCasts, recoveryCasts, equipmentComparisons, equipmentUpgrades,
                talentInvestments, enhancements, discardedItems, safetyRestarts, startClicks, uiCallbacks, saveCount;
            public int difficultyTransitions,layoutTransitions,layoutsVisitedMask,maxKillChain,selectedDifficulty,dungeonClears;
            public string[] dropRecords;
            public string[] captureFrames;
        }
    }
}
#endif
