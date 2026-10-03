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
    // Bounded adversarial verification for speed/UI/save interactions. This is not a balance benchmark.
    public sealed class SpeedUiStressProbe : MonoBehaviour
    {
        private FirstEncounter owner;
        private AutoHuntDirector hunt;
        private Report report;
        private string mode,reportDirectory,saveDirectory,captureDirectory,expectedPath,phase="initializing";
        private float started;
        private bool running,finishing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args=Environment.GetCommandLineArgs();
            if(Array.IndexOf(args,"-affixSpeedUiStressTest")<0)return;
            var host=new GameObject("Affix Speed UI Stress Probe");DontDestroyOnLoad(host);
            host.AddComponent<SpeedUiStressProbe>().Begin(args);
        }

        private void Begin(string[] args)
        {
            started=Time.realtimeSinceStartup;report=new Report();Application.logMessageReceived+=OnLog;
            try
            {
                mode=Argument(args,"-affixSpeedUiStressMode");Require(mode=="observe"||mode=="read","Stress mode must be observe or read.");
                reportDirectory=ProbePathPolicy.RequireDDriveDirectory(args,"-affixReportDir");
                saveDirectory=ProbePathPolicy.RequireSaveDirectory(args,Application.persistentDataPath);
                captureDirectory=ProbePathPolicy.RequireDDriveDirectory(args,"-affixCaptureDir");expectedPath=Path.Combine(saveDirectory,"expected-speed-ui-profile.json");
                Directory.CreateDirectory(reportDirectory);Directory.CreateDirectory(captureDirectory);report.mode=mode;WriteReport();
            }
            catch(Exception error){Finish(false,error.ToString());}
        }

        private void LateUpdate()
        {
            if(finishing||report==null)return;
            try
            {
                if(Time.realtimeSinceStartup-started>(mode=="observe"?75:35))throw new TimeoutException("Speed/UI stress timed out in "+phase);
                if(running)return;
                owner=FindFirstObjectByType<FirstEncounter>();
                if(owner==null||owner.Hunt==null||!owner.Hunt.Initialized||owner.Hero==null||!owner.Hero.IsReady||!Ready("autohunt-toggle"))return;
                string expectedProfile=Path.Combine(saveDirectory,"profile-v1.json");
                Require(owner.Persistence.CanPlay&&!owner.Persistence.Ephemeral&&
                    string.Equals(Path.GetFullPath(owner.Persistence.FilePath),expectedProfile,StringComparison.OrdinalIgnoreCase),
                    "Speed/UI stress persistence escaped its explicit D: save sandbox.");
                hunt=owner.Hunt;running=true;
                if(mode=="observe")StartCoroutine(RunObserve());else StartCoroutine(RunRead());
            }
            catch(Exception error){Finish(false,error.ToString());}
        }

        private IEnumerator RunObserve()
        {
            phase="legacy-load";report.loadedSchema=DiskSchema();Require(report.loadedSchema==3,"Observe stress requires the copied schema-v3 production profile.");
            Require(owner.Progression.PrecisionRank>0,"Stress profile needs naturally earned Precision for a production critical roll.");
            Require(owner.SimulationSpeed==1&&owner.IsPaused&&Time.timeScale==0,"New process did not start stopped and 1x-ready.");
            ProgressionSnapshot initial=owner.Progression.CaptureSnapshot();report.initialKillTokens=initial.killTokens.Length;
            report.initialIssuedItems=initial.issuedItemIds.Length;report.initialInventory=owner.Progression.Inventory.Count;
            Dispatch("autohunt-toggle");Require(hunt.Running&&!owner.IsPaused&&Time.timeScale==1,"Stress hunt did not begin at 1x.");

            phase="critical-1x";int criticalBefore=hunt.CriticalFeedbackCount;
            yield return WaitFor(()=>hunt.CriticalFeedbackCount>criticalBefore,18,"a production critical feedback event at 1x");
            Require(owner.SimulationSpeed==1&&Time.timeScale==1,"Critical feedback was not observed at 1x.");
            report.criticalFeedbackAt1x=true;report.criticalFeedbackCount=hunt.CriticalFeedbackCount;
            yield return Capture("critical-1x");

            phase="attack-speed-switch";float attackDeadline=Time.realtimeSinceStartup+12;
            while(!owner.Hero.IsAttacking&&Time.realtimeSinceStartup<attackDeadline)yield return null;
            Require(owner.Hero.IsAttacking,"Timed out waiting for an active hero attack.");
            Dispatch("speed-4x");Dispatch("speed-2x");Dispatch("speed-1x");
            Require(owner.SimulationSpeed==1&&Time.timeScale==1,"Repeated attack-time speed switches did not restore 1x.");
            report.attackSpeedSwitch=true;report.speedSwitches+=3;

            phase="panel-pause";Dispatch("character-tab");yield return null;
            Require(owner.Screen==ManagementScreen.Equipment&&hunt.Running&&Time.timeScale==1,"Equipment panel did not stay live over combat.");
            Dispatch("pause-button");Require(owner.Screen==ManagementScreen.Equipment&&owner.IsPaused&&Time.timeScale==0,
                "Pause button closed the panel or failed to stop simulation.");
            Dispatch("speed-4x");Dispatch("speed-2x");
            Require(owner.Screen==ManagementScreen.Equipment&&owner.SimulationSpeed==2&&Time.timeScale==0,
                "Paused speed selection resumed simulation or closed the panel.");
            Dispatch("pause-button");Require(owner.Screen==ManagementScreen.Equipment&&!owner.IsPaused&&Time.timeScale==2,
                "Panel-open resume did not restore the selected 2x speed.");
            Dispatch("speed-1x");report.panelPauseResume=true;report.speedSwitches+=3;yield return null;

            phase="equipment-layout";int weaponIndex=BestWeaponIndex();Require(weaponIndex>=0,"Stress profile has no inventory weapon for swap coverage.");
            Dispatch("inventory-slot-"+weaponIndex);yield return null;Validate720Equipment(weaponIndex);yield return Capture("equipment-live");

            phase="equipment-swap";string selectedId=owner.Progression.Inventory[weaponIndex].Id;int inventoryBefore=owner.Progression.Inventory.Count;
            int tokensBefore=owner.Progression.CaptureSnapshot().killTokens.Length;Dispatch("equip-button");
            Require(hunt.Running&&owner.Progression.EquippedWeapon.Id==selectedId&&owner.Progression.Inventory.Count==inventoryBefore&&
                owner.Progression.CaptureSnapshot().killTokens.Length==tokensBefore,"Live equipment swap lost inventory or reward state.");
            report.equipmentSwap=true;

            phase="salvage-live";Dispatch("pause-button");Require(hunt.Running&&owner.IsPaused&&Time.timeScale==0,"Transaction pause stopped the hunt or simulation kept advancing.");
            int salvageIndex=WorstSalvageIndex();Require(salvageIndex>=0,"Stress profile has no salvage candidate.");
            string salvageId=owner.Progression.Inventory[salvageIndex].Id,salvageName=KoreanDisplay.ItemName(owner.Progression.Inventory[salvageIndex]);
            string fallbackName=owner.Progression.Inventory.Count<=1?"":KoreanDisplay.ItemName(owner.Progression.Inventory[
                salvageIndex==owner.Progression.Inventory.Count-1?salvageIndex-1:salvageIndex+1]);
            int salvageValue=owner.Progression.GetSalvageValue(salvageIndex);
            Dispatch("inventory-slot-"+salvageIndex);yield return WaitFor(()=>Text("selected-item-name").Contains(salvageName),2,"stable salvage selection");
            Dispatch("salvage-button");yield return WaitFor(()=>Text("salvage-button").Contains("확인"),2,"salvage confirmation state");
            int goldBefore=owner.Progression.TotalGold,salvageBefore=owner.Progression.TotalSalvageGold;
            inventoryBefore=owner.Progression.Inventory.Count;tokensBefore=owner.Progression.CaptureSnapshot().killTokens.Length;
            Dispatch("salvage-button");yield return null;
            Require(IndexOf(salvageId)<0&&owner.Progression.Inventory.Count==inventoryBefore-1&&owner.Progression.TotalGold==goldBefore+salvageValue&&
                owner.Progression.TotalSalvageGold==salvageBefore+salvageValue&&owner.Progression.CaptureSnapshot().killTokens.Length==tokensBefore,
                "Live salvage did not remove exactly one item and credit exact provenance.");
            Require(owner.Progression.Inventory.Count==0?!Visible(Element("item-comparison")):Text("selected-item-name").Contains(fallbackName),
                "Salvage did not move selection to the remaining adjacent item.");report.salvageSelectionFallback=true;
            Dispatch("pause-button");Require(hunt.Running&&!owner.IsPaused&&Time.timeScale==1,"Transaction resume did not restore the active 1x hunt.");
            report.salvageWhileRunning=true;report.salvageValue=salvageValue;

            phase="forge-live";Dispatch("forge-tab");yield return null;Require(owner.Progression.CanEnhanceEquipped,"Stress weapon cannot be enhanced.");
            Dispatch("pause-button");Require(hunt.Running&&owner.IsPaused&&Time.timeScale==0,"Forge transaction pause stopped the hunt or simulation kept advancing.");
            int rankBefore=owner.Progression.EquippedWeapon.EnhancementRank,cost=owner.Progression.EquippedEnhancementCost;
            goldBefore=owner.Progression.TotalGold;tokensBefore=owner.Progression.CaptureSnapshot().killTokens.Length;Dispatch("forge-enhance");
            Require(hunt.Running&&owner.Progression.EquippedWeapon.EnhancementRank==rankBefore+1&&owner.Progression.TotalGold==goldBefore-cost&&
                owner.Progression.CaptureSnapshot().killTokens.Length==tokensBefore,"Live forge action charged or mutated reward state incorrectly.");
            Dispatch("pause-button");Require(hunt.Running&&!owner.IsPaused&&Time.timeScale==1,"Forge transaction resume did not restore the active 1x hunt.");
            report.forgeWhileRunning=true;Dispatch("character-tab");yield return null;

            phase="natural-drop-with-panel";int collectedBefore=hunt.CollectedItems;Dispatch("speed-4x");report.speedSwitches++;
            yield return WaitFor(()=>hunt.CollectedItems>collectedBefore,22,"a normal production drop collected while equipment stayed open");
            Require(owner.Screen==ManagementScreen.Equipment&&hunt.Running,"Natural drop collection closed the equipment panel or stopped the hunt.");
            report.dropCollectedWithPanel=true;report.collectedDuringPanel=hunt.CollectedItems-collectedBefore;
            Dispatch("pause-button");Require(hunt.Running&&owner.IsPaused&&Time.timeScale==0,"Post-drop UI audit did not pause the active hunt.");
            int postDropInventory=owner.Progression.Inventory.Count;
            yield return WaitFor(()=>Text("bag-count").Contains(postDropInventory.ToString()),2,"post-drop inventory label refresh");
            while(owner.Progression.Inventory.Count>HeroProgression.InventoryCapacity-4)
            {
                salvageIndex=WorstSalvageIndex();string id=owner.Progression.Inventory[salvageIndex].Id;Dispatch("inventory-slot-"+salvageIndex);yield return null;
                Dispatch("salvage-button");yield return null;Dispatch("salvage-button");yield return null;Require(IndexOf(id)<0,"Post-drop salvage did not reopen bag space.");
            }
            Dispatch("pause-button");Require(hunt.Running&&!owner.IsPaused,"Post-drop UI audit did not resume the active hunt.");
            Dispatch("dungeon-tab");Dispatch("speed-1x");report.speedSwitches++;

            phase="clear-transition-speed";int clearBefore=hunt.CompletedRuns;
            yield return WaitFor(()=>hunt.CompletedRuns>clearBefore,24,"the next dungeon clear");
            Require(hunt.Phase==HuntPhase.Resting,"Clear counter advanced outside the resting transition.");
            int completedAtRest=hunt.CompletedRuns,clearsAtRest=owner.Progression.DungeonClears;
            Dispatch("speed-4x");Dispatch("speed-2x");Dispatch("speed-1x");report.speedSwitches+=3;
            Require(hunt.Phase==HuntPhase.Resting&&hunt.CompletedRuns==completedAtRest&&owner.Progression.DungeonClears==clearsAtRest,
                "Speed switching duplicated or skipped the clear reward during rest.");
            yield return WaitFor(()=>hunt.Phase!=HuntPhase.Resting,3,"rest transition completion");
            Require(hunt.CompletedRuns==completedAtRest&&owner.Progression.DungeonClears==clearsAtRest,
                "Rest completion duplicated the clear reward.");report.clearTransitionSpeedSwitch=true;

            phase="telegraph-speed-switch";yield return WaitFor(AnyTelegraphing,18,"an elite attack telegraph");
            Require(AnyTelegraphing(),"Telegraph ended before the first speed switch.");
            Dispatch("speed-4x");Dispatch("speed-2x");Dispatch("speed-1x");report.speedSwitches+=3;
            Require(owner.SimulationSpeed==1&&Time.timeScale==1,"Telegraph-time speed switches did not restore 1x.");report.telegraphSpeedSwitch=true;

            phase="ledger-save";ProgressionSnapshot final=owner.Progression.CaptureSnapshot();
            Require(final.killTokens.Length-report.initialKillTokens==hunt.TotalKills,"Runtime kills and persisted reward tokens diverged.");
            Require(Unique(final.killTokens)&&Unique(final.issuedItemIds),"Reward or issued-item ledger contains duplicates.");
            HeroProgression restored=HeroProgression.RestoreSnapshot(final);
            Require(restored.TotalGold==owner.Progression.TotalGold&&restored.TotalSalvageGold==owner.Progression.TotalSalvageGold&&
                restored.Inventory.Count==owner.Progression.Inventory.Count&&restored.DungeonClears==owner.Progression.DungeonClears,
                "Current snapshot did not round-trip after live UI/drop mutations.");
            report.ledgerRoundtrip=true;report.kills=hunt.TotalKills;report.killTokensAdded=final.killTokens.Length-report.initialKillTokens;
            report.finalInventory=owner.Progression.Inventory.Count;report.finalIssuedItems=final.issuedItemIds.Length;
            Dispatch("speed-4x");report.speedSwitches++;Require(owner.SimulationSpeed==4,"Final session speed selection failed.");
            hunt.StopHunt();Require(owner.SaveProgress(),"Final stress save failed: "+owner.Persistence.Problem);
            report.savedSchema=DiskSchema();Require(report.savedSchema==5,"Schema-v3 profile did not save as schema v5.");report.migrationVerified=true;
            string canonical=ProfilePersistence.Encode(owner.Progression);File.WriteAllText(expectedPath,canonical);
            Require(File.ReadAllText(owner.Persistence.FilePath)==canonical,"Saved stress profile differs from canonical encoding.");
            report.diskRoundtrip=true;Finish(true,null);
        }

        private IEnumerator RunRead()
        {
            phase="session-reset-read";Require(File.Exists(expectedPath),"Expected stress profile is missing.");
            report.loadedSchema=DiskSchema();Require(report.loadedSchema==4,"Continued stress profile is not schema v4.");
            Require(ProfilePersistence.Encode(owner.Progression)==File.ReadAllText(expectedPath),"Restarted stress profile differs from saved canonical state.");
            Require(owner.SimulationSpeed==1&&owner.IsPaused&&Time.timeScale==0,"Saved 4x session speed leaked into the next process.");
            report.sessionSpeedReset=true;Dispatch("autohunt-toggle");Require(hunt.Running&&Time.timeScale==1,"Restart did not resume at 1x.");
            yield return WaitFor(()=>hunt.AcceptedFeedbackCount>0,12,"continued combat feedback after restart");
            hunt.StopHunt();Require(owner.SaveProgress(),"Continued stress save failed.");report.continuedAfterRestart=true;report.diskRoundtrip=true;
            Finish(true,null);
        }

        private bool AnyTelegraphing()
        {
            foreach(MeleeActor actor in hunt.Enemies){EliteAttackPattern pattern=actor==null?null:actor.GetComponent<EliteAttackPattern>();if(pattern!=null&&pattern.IsTelegraphing)return true;}
            return false;
        }

        private int BestWeaponIndex()
        {
            int best=-1,score=int.MinValue;for(int i=0;i<owner.Progression.Inventory.Count;i++){WeaponItem item=owner.Progression.Inventory[i];
                if(item.EquipmentSlot!=EquipmentSlot.Weapon)continue;int value=Score(item);if(value>score){score=value;best=i;}}return best;
        }
        private int WorstSalvageIndex()
        {
            int worst=-1,score=int.MaxValue;for(int i=0;i<owner.Progression.Inventory.Count;i++){int value=Score(owner.Progression.Inventory[i]);
                if(value<score){score=value;worst=i;}}return worst;
        }
        private int IndexOf(string id){for(int i=0;i<owner.Progression.Inventory.Count;i++)if(owner.Progression.Inventory[i].Id==id)return i;return -1;}
        private static int Score(WeaponItem item)=>item==null?0:item.DamageBonus*4+item.DefenseBonus*12+item.HealthBonus/4+
            item.VampirismPercent*4+item.CriticalChance*2+item.Penetration+item.CooldownReductionPercent;

        private void Validate720Equipment(int selectedIndex)
        {
            Require(Screen.width==1280&&Screen.height==720,"Stress layout run must use a 1280x720 framebuffer.");
            VisualElement root=Root,panel=Element("character-panel");Require(root!=null&&panel!=null&&Visible(panel),"Equipment panel is not visible.");
            foreach(string name in new[]{"equipped-paper-doll","inventory-grid","item-comparison","inventory-slot-23","selected-item-name","equip-button","salvage-button","speed-1x","speed-2x","speed-4x"})
            {VisualElement element=Element(name);Require(element!=null&&Within(element.worldBound,root.worldBound),"720p control is clipped: "+name);}
            VisualElement slot=Element("inventory-slot-"+selectedIndex);Require(!string.IsNullOrWhiteSpace(slot.tooltip)&&slot.tooltip.Contains("기본"),
                "Selected 720p equipment tooltip does not expose base/rolled detail.");
            Require(!string.IsNullOrWhiteSpace(Text("selected-item-name")),"720p comparison name is empty.");report.layout720Verified=true;
        }
        private static bool Within(Rect child,Rect parent)=>child.xMin>=parent.xMin-.5f&&child.yMin>=parent.yMin-.5f&&child.xMax<=parent.xMax+.5f&&child.yMax<=parent.yMax+.5f;
        private static bool Unique(string[] values){var set=new HashSet<string>(StringComparer.Ordinal);foreach(string value in values)if(string.IsNullOrWhiteSpace(value)||!set.Add(value))return false;return true;}

        private IEnumerator WaitFor(Func<bool> condition,float seconds,string description)
        {
            float deadline=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<deadline)yield return null;
            Require(condition(),"Timed out waiting for "+description+".");
        }
        private IEnumerator Capture(string label)
        {
            yield return new WaitForEndOfFrame();Texture2D pixels=null;string path=Path.Combine(captureDirectory,"speed-ui-stress-"+label+".bmp");
            try{pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();WriteBitmap(path,pixels);report.captureFrames.Add(path);}
            finally{if(pixels!=null)Destroy(pixels);}
        }
        private static void WriteBitmap(string path,Texture2D texture)
        {
            int width=texture.width,height=texture.height,rowBytes=checked(width*3),stride=checked(rowBytes+3)&~3,imageBytes=checked(stride*height);Color32[] colors=texture.GetPixels32();
            using(var writer=new BinaryWriter(File.Create(path))){writer.Write((ushort)0x4D42);writer.Write(checked(54+imageBytes));writer.Write(0);writer.Write(54);writer.Write(40);
                writer.Write(width);writer.Write(height);writer.Write((ushort)1);writer.Write((ushort)24);writer.Write(0);writer.Write(imageBytes);writer.Write(2835);writer.Write(2835);writer.Write(0);writer.Write(0);
                for(int y=0;y<height;y++){for(int x=0;x<width;x++){Color32 c=colors[y*width+x];writer.Write(c.b);writer.Write(c.g);writer.Write(c.r);}for(int pad=rowBytes;pad<stride;pad++)writer.Write((byte)0);}}
        }

        private VisualElement Root=>owner.GetComponent<UIDocument>()?.rootVisualElement;
        private VisualElement Element(string name)=>Root?.Q(name);
        private string Text(string name)=>Element(name)?.Q<Label>()?.text??(Element(name) as Label)?.text??"";
        private bool Ready(string name){VisualElement e=Element(name);return e!=null&&e.panel!=null&&e.enabledInHierarchy&&e.worldBound.width>0;}
        private static bool Visible(VisualElement e)=>e!=null&&e.resolvedStyle.display!=DisplayStyle.None&&e.resolvedStyle.visibility==Visibility.Visible;
        private void Dispatch(string name)
        {
            VisualElement element=Element(name);Require(element!=null&&element.panel!=null&&element.enabledInHierarchy,"Native control unavailable: "+name);
            for(VisualElement node=element;node!=null;node=node.parent)Require(Visible(node),"Native control hidden: "+name);
            using(var click=ClickEvent.GetPooled()){click.target=element;element.SendEvent(click);}report.uiCallbacks++;
        }

        private int DiskSchema()
        {
            Envelope envelope=JsonUtility.FromJson<Envelope>(File.ReadAllText(owner.Persistence.FilePath));
            Require(envelope!=null&&envelope.format=="AFFIX_PROFILE"&&!string.IsNullOrEmpty(envelope.payload),"Profile envelope is invalid.");
            ProgressionSnapshot snapshot=JsonUtility.FromJson<ProgressionSnapshot>(envelope.payload);Require(snapshot!=null,"Profile payload is invalid.");return snapshot.schemaVersion;
        }
        private void OnLog(string message,string trace,LogType type){if(!finishing&&(type==LogType.Error||type==LogType.Exception||type==LogType.Assert))Finish(false,message+"\n"+trace);}
        private void Finish(bool success,string problem)
        {
            if(finishing)return;finishing=true;Application.logMessageReceived-=OnLog;
            if(owner!=null){report.simulatedGameplaySeconds=owner.ElapsedSeconds;report.wallClockGameplaySeconds=owner.WallClockSeconds;report.finalSimulationSpeed=owner.SimulationSpeed;}
            report.elapsedWallSeconds=Time.realtimeSinceStartup-started;report.result=report.status=success?"PASS":"FAIL";report.problem=problem??"";report.phase=phase;report.finishedUtc=DateTime.UtcNow.ToString("O");
            try{WriteReport();}catch(Exception error){Debug.LogError("Speed/UI stress report write failed: "+error);success=false;}
            Debug.Log("AFFIX_SPEED_UI_STRESS_"+(success?"PASS":"FAIL")+" mode="+mode+" run="+report.runId);Application.Quit(success?0:1);
        }
        private void WriteReport()=>File.WriteAllText(Path.Combine(reportDirectory,"speed-ui-stress-"+mode+".json"),JsonUtility.ToJson(report,true));
        private static string Argument(string[] args,string key){int i=Array.IndexOf(args,key);if(i<0||i+1>=args.Length)throw new ArgumentException("Missing "+key);return args[i+1];}
        private static void Require(bool value,string problem){if(!value)throw new InvalidOperationException(problem);}

        [Serializable] private sealed class Envelope{public string format,payload;}
        [Serializable] private sealed class Report
        {
            public string schema="affix-speed-ui-stress-v1",runId=Guid.NewGuid().ToString("N"),mode="",result="RUNNING",status="RUNNING",problem="",phase="initializing",
                startedUtc=DateTime.UtcNow.ToString("O"),finishedUtc="",unityVersion=Application.unityVersion,buildGuid=Application.buildGUID;
            public string scope="Bounded Windows verification of repeated session-speed changes, panel-open pause/resume, live equipment/salvage/forge with normal drops, 720p bounds, reward ledgers, schema-v3 migration, and next-process speed reset.";
            public string actualPhysicalInput="NOT_RUN; UI actions use native UI Toolkit callbacks.";
            public int loadedSchema,savedSchema,initialKillTokens,initialIssuedItems,initialInventory,finalInventory,finalIssuedItems,kills,killTokensAdded,
                speedSwitches,criticalFeedbackCount,salvageValue,collectedDuringPanel,uiCallbacks;
            public float elapsedWallSeconds,simulatedGameplaySeconds,wallClockGameplaySeconds,finalSimulationSpeed;
            public bool criticalFeedbackAt1x,attackSpeedSwitch,panelPauseResume,layout720Verified,equipmentSwap,salvageWhileRunning,salvageSelectionFallback,forgeWhileRunning,
                dropCollectedWithPanel,clearTransitionSpeedSwitch,telegraphSpeedSwitch,ledgerRoundtrip,migrationVerified,diskRoundtrip,sessionSpeedReset,continuedAfterRestart;
            public List<string> captureFrames=new List<string>();
        }
    }
}
#endif
