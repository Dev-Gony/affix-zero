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
    // Explicit player flag only. The normal player never creates this observer.
    public sealed class AutoHuntSmokeProbe : MonoBehaviour
    {
        private FirstEncounter owner;
        private AutoHuntDirector hunt;
        private MeleeActor hero;
        private Report report;
        private string outputDirectory, phase="initializing";
        private float started, phaseStarted, managementTime, managementTravel;
        private int managementHp, managementHits, pendingCaptures;
        private bool finishing;
        private readonly HashSet<MeleeActor> subscribed=new HashSet<MeleeActor>();
        private readonly HashSet<int> deadLives=new HashSet<int>();
        private readonly HashSet<int> sections=new HashSet<int>();
        private readonly HashSet<string> requested=new HashSet<string>(), completed=new HashSet<string>();
        private readonly List<string> screenshots=new List<string>();
        private readonly List<FrozenActor> frozen=new List<FrozenActor>();
        private int frozenKills, frozenLoot, frozenRuns;
        private float frozenTravel;
        private static readonly string[] RequiredCaptures={"exploration","combat","loot","run-complete","management","paused"};

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args=Environment.GetCommandLineArgs();
            if(Array.IndexOf(args,"-affixAutoHuntTest")<0)return;
            var host=new GameObject("Affix Auto Hunt Smoke Probe");DontDestroyOnLoad(host);
            host.AddComponent<AutoHuntSmokeProbe>().Begin(args);
        }
        private void Begin(string[] args)
        {
            report=new Report();started=Time.realtimeSinceStartup;
            outputDirectory=Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,"Build","Reports"));
            Application.logMessageReceived+=OnLog;
            try
            {
                int index=Array.IndexOf(args,"-affixReportDir");
                if(index>=0)
                {
                    if(index+1>=args.Length||!Path.IsPathRooted(args[index+1]))throw new ArgumentException("-affixReportDir requires an absolute path.");
                    outputDirectory=Path.GetFullPath(args[index+1]);
                }
                Directory.CreateDirectory(outputDirectory);WriteReport();
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void LateUpdate()
        {
            if(finishing||report==null)return;
            try
            {
                if(Time.realtimeSinceStartup-started>240)throw new TimeoutException("Auto hunt exceeded 240 real seconds in "+phase);
                Tick();
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void Tick()
        {
            if(phase=="initializing")
            {
                owner=FindFirstObjectByType<FirstEncounter>();
                if(owner==null||owner.Hunt==null||!owner.Hunt.Initialized||owner.Hero==null||!owner.Hero.IsReady)return;
                hunt=owner.Hunt;hero=owner.Hero;
                var document=owner.GetComponent<UIDocument>();
                if(document==null||document.rootVisualElement.panel==null||document.rootVisualElement.worldBound.width<=0)return;
                // Director readiness is set in Update; HUD enables Start in LateUpdate.
                // Wait for the actual control to finish its next layout/state cycle.
                var startControl=document.rootVisualElement.Q("autohunt-toggle");
                if(startControl==null||!startControl.enabledInHierarchy||startControl.worldBound.width<=0)return;
                Require(!hunt.Running&&owner.IsPaused&&Time.timeScale==0,"Automatic hunt did not start in explicit stopped/paused state.");
                Require(hero.Damage==30,"Fresh-profile baseline damage must be 30 for this smoke scenario.");
                Require(hero.Hp==hero.MaxHp,"Initial hero HP is not full.");
                SubscribeActors();
                report.initialized=true;
                Dispatch("autohunt-toggle");report.startCallbacks++;
                Require(hunt.Running,"Start callback did not start automatic hunt.");
                Phase("hunting");return;
            }
            Require(owner!=null&&hunt!=null&&hero!=null,"Automatic director or owner disappeared.");
            Require(string.IsNullOrEmpty(hunt.LastFault),"Director fault: "+hunt.LastFault);
            Require(hunt.Running,"Hunt stopped before three unattended runs.");
            Require(owner.IsPaused?Mathf.Approximately(Time.timeScale,0):Mathf.Approximately(Time.timeScale,1),"Time acceleration or unexpected simulation scale detected.");
            SubscribeActors();CheckWorld();
            Require(hunt.SectionIndex>=0&&hunt.SectionIndex<3,"Unexpected section index.");
            sections.Add(hunt.SectionIndex);
            if(hunt.TravelDistance>0.5f&&hero.CurrentClip==ActorClip.Walk)Capture("exploration");
            if(report.acceptedHits>0)Capture("combat");
            if(hunt.CollectedItems>0)Capture("loot");
            if(hunt.CompletedRuns>0)Capture("run-complete");

            if(phase=="hunting"&&!report.managementKeepsHunting&&report.acceptedHits>0&&completed.Contains("combat")&&!hero.IsDead)
            {
                Dispatch("character-tab");
                Require(owner.Screen==ManagementScreen.Equipment&&!owner.IsPaused,"Equipment screen paused the automatic hunt.");
                managementTime=Time.time;managementTravel=hunt.TravelDistance;managementHp=hero.Hp;managementHits=report.acceptedHits;
                Capture("management");Phase("equipment-running");
            }
            else if(phase=="equipment-running"&&Age>=0.7f)
            {
                Require(owner.Screen==ManagementScreen.Equipment&&!owner.IsPaused&&hunt.Running,"Hunt did not remain active during equipment management.");
                Require(Time.time-managementTime>=0.55f,"Simulation time stopped inside management.");
                Require(hunt.TravelDistance>managementTravel+0.001f||hero.Hp!=managementHp||report.acceptedHits>managementHits,
                    "No actual path movement or combat change during the management observation.");
                report.managementKeepsHunting=true;
                Dispatch("talents-tab");Require(owner.Screen==ManagementScreen.Talents&&!owner.IsPaused,"Talent switch paused hunting.");
                Phase("talents-running");
            }
            else if(phase=="talents-running"&&Age>=0.12f)
            {
                Dispatch("forge-tab");Require(owner.Screen==ManagementScreen.Forge&&!owner.IsPaused,"Forge switch paused hunting.");
                Phase("forge-running");
            }
            else if(phase=="forge-running"&&Age>=0.12f)
            {
                Dispatch("dungeon-tab");Require(!owner.ManagementVisible&&!owner.IsPaused,"Closing management stopped hunting.");
                report.managementSwitchesVerified=true;
                Dispatch("pause-button");
                Require(owner.IsPaused&&Time.timeScale==0,"Manual pause callback did not pause.");
                Phase("pause-arming");
            }
            else if(phase=="pause-arming")
            {
                // The director samples last frame's movement before actor Update. Let that final
                // pre-pause distance be accounted for before starting the frozen-state interval.
                Require(owner.IsPaused&&Time.timeScale==0,"Manual pause did not persist to the next frame.");
                Freeze();Capture("paused");Phase("manual-pause");
            }
            else if(phase=="manual-pause")
            {
                CheckFrozen();
                if(Age>=0.4f)
                {
                    report.manualPauseVerified=true;
                    Dispatch("pause-button");Require(!owner.IsPaused&&hunt.Running,"Manual resume did not retain automatic hunt.");
                    Phase("resumed");
                }
            }
            else if(phase=="resumed"&&Age>=0.15f)
            {
                Require(Time.timeScale==1&&hunt.Running,"Manual resume failed to restore 1x simulation.");
                report.manualResumeVerified=true;Phase("observe-cycles");
            }

            if(hunt.CompletedRuns>=3&&phase=="observe-cycles"&&pendingCaptures==0)
            {
                Require(hunt.FailedRuns==0&&hunt.DeathRetries==0,"Baseline farming died or retried; this is not a stable idle loop.");
                Require(hunt.TotalKills>=18&&deadLives.Count>=18,"Three cycles did not produce at least 18 observed enemy deaths.");
                Require(hunt.CollectedItems>=3,"Automatic loot collection did not occur across the required cycles.");
                Require(sections.Count==3,"Not all three sections were observed.");
                Require(hunt.World.DetourQueries>0,"No real obstacle detour was requested during the three cycles.");
                Require(report.startCallbacks==1,"Hunt needed more than one start callback.");
                Require(report.heroHitObserved&&report.enemyHitObserved&&report.impactPoseChecks>0&&report.lineOfSightChecks>0,
                    "Actual mutual combat/impact/LOS evidence is incomplete.");
                Require(hunt.TotalKills==deadLives.Count,"Kill count differs from unique observed enemy deaths.");
                Require(owner.Progression.TotalExperience==hunt.TotalKills*25&&owner.Progression.TotalGold==hunt.TotalKills*8,
                    "Automatic kill rewards differ: kills="+hunt.TotalKills+" XP="+owner.Progression.TotalExperience+" gold="+owner.Progression.TotalGold+" damage="+owner.Progression.TotalDamage+" enhancement="+owner.Progression.EquippedWeapon.EnhancementRank);
                Require(owner.Progression.Inventory.Count==hunt.CollectedItems,"Automatic item intake count differs from the retained inventory.");
                foreach(string capture in RequiredCaptures)Require(completed.Contains(capture),"Missing framebuffer capture: "+capture);
                Finish(true,null);
            }
        }
        private float Age=>Time.realtimeSinceStartup-phaseStarted;
        private void Phase(string value){phase=value;phaseStarted=Time.realtimeSinceStartup;}
        private void SubscribeActors()
        {
            Subscribe(hero);
            foreach(var enemy in hunt.Enemies)if(enemy!=null)Subscribe(enemy);
        }
        private void Subscribe(MeleeActor actor)
        {if(actor!=null&&subscribed.Add(actor))actor.Damaged+=OnDamaged;}
        private void CheckWorld()
        {
            Require(hunt.World!=null,"Dungeon world missing after initialization.");
            foreach(var actor in subscribed)
            {
                if(actor==null||!actor.isActiveAndEnabled||!actor.IsReady)continue;
                Require(hunt.World.IsWalkable(actor.transform.position),"Actor entered blocked terrain: "+actor.ActorId+" at "+actor.transform.position);
                report.walkabilityChecks++;
            }
        }
        private void OnDamaged(MeleeActor defender,HitReceipt receipt)
        {
            if(finishing||!receipt.Accepted)return;
            try
            {
                Require(!owner.IsPaused,"Damage occurred while manually paused.");
                MeleeActor attacker=null;
                foreach(var candidate in subscribed)
                    if(candidate!=null&&candidate.ActorId==defender.LastAttackerId){attacker=candidate;break;}
                Require(attacker!=null,"Accepted damage has no identifiable attacker.");
                Require(hunt.World.LineOfSight(attacker.transform.position,defender.transform.position),"A hit crossed a blocking dungeon wall.");
                report.lineOfSightChecks++;
                var renderer=attacker.GetComponent<SpriteRenderer>();
                Require(renderer!=null&&attacker.AnimationSet!=null&&renderer.sprite==attacker.AnimationSet.ImpactSprite,
                    "Damage was applied without the real attack impact sprite.");
                report.impactPoseChecks++;report.acceptedHits++;
                if(defender==hero)report.heroHitObserved=true;
                else
                {
                    report.enemyHitObserved=true;
                    if(receipt.Killed)Require(deadLives.Add(defender.ActorId),"One enemy life produced more than one death receipt.");
                }
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void Freeze()
        {
            frozen.Clear();
            foreach(var actor in subscribed)
                if(actor!=null&&actor.isActiveAndEnabled)frozen.Add(new FrozenActor {actor=actor,id=actor.ActorId,hp=actor.Hp,position=actor.transform.position,
                    elapsed=actor.AttackElapsed,sprite=actor.GetComponent<SpriteRenderer>().sprite});
            frozenKills=hunt.TotalKills;frozenLoot=hunt.CollectedItems;frozenRuns=hunt.CompletedRuns;frozenTravel=hunt.TravelDistance;
        }
        private void CheckFrozen()
        {
            Require(owner.IsPaused&&Time.timeScale==0,"Manual pause ended prematurely.");
            foreach(var state in frozen)
                Require(state.actor!=null&&state.actor.ActorId==state.id&&state.actor.Hp==state.hp&&state.actor.transform.position==state.position&&
                    state.actor.AttackElapsed==state.elapsed&&state.actor.GetComponent<SpriteRenderer>().sprite==state.sprite,
                    "Actor state changed during the manual pause interval.");
            Require(hunt.TotalKills==frozenKills&&hunt.CollectedItems==frozenLoot&&hunt.CompletedRuns==frozenRuns&&hunt.TravelDistance==frozenTravel,
                "Director progressed while manually paused.");
        }
        private void Dispatch(string name)
        {
            var document=owner.GetComponent<UIDocument>();Require(document!=null,"Runtime UIDocument missing.");
            var element=document.rootVisualElement.Q(name);
            Require(element!=null&&element.enabledInHierarchy&&element.worldBound.width>0&&element.worldBound.height>0,"UI control absent or disabled: "+name);
            for(var node=element;node!=null;node=node.parent)
                Require(node.resolvedStyle.display!=DisplayStyle.None&&node.resolvedStyle.visibility==Visibility.Visible,"UI control or ancestor hidden: "+name);
            using(var click=ClickEvent.GetPooled()){click.target=element;element.SendEvent(click);}
            report.uiCallbacksDispatched++;
        }
        private void Capture(string label)
        {
            if(!requested.Add(label))return;
            pendingCaptures++;StartCoroutine(CaptureFrame(label));
        }
        private IEnumerator CaptureFrame(string label)
        {
            yield return null;yield return new WaitForEndOfFrame();
            if(finishing)yield break;
            Texture2D pixels=null;
            try
            {
                var document=owner.GetComponent<UIDocument>();var root=document==null?null:document.rootVisualElement;
                Require(root!=null&&root.name=="stitch-hud"&&root.panel!=null&&root.worldBound.width>0,"HUD absent from actual frame.");
                Require(root.Q<Label>("hero-hp-value")?.text==hero.Hp+"\n/ "+hero.MaxHp,"HUD health binding is stale.");
                Require(Screen.width>0&&Screen.height>0,"Player framebuffer has no size.");
                pixels=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);pixels.Apply();
                string path=Path.Combine(outputDirectory,"auto-"+report.runId+"-"+label+".bmp");WriteBitmap(path,pixels);
                screenshots.Add(path);completed.Add(label);pendingCaptures--;
            }
            catch(Exception error){Finish(false,error.ToString());}
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
        private void OnLog(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)Finish(false,message+"\n"+trace);}
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private void Finish(bool success,string problem)
        {
            if(finishing)return;finishing=true;
            Application.logMessageReceived-=OnLog;
            foreach(var actor in subscribed)if(actor!=null)actor.Damaged-=OnDamaged;
            report.result=success?"PASS":"FAIL";report.status=report.result;report.problem=problem??"";report.phase=phase;
            report.finishedUtc=DateTime.UtcNow.ToString("O");report.elapsedSeconds=Time.realtimeSinceStartup-started;
            report.screenshots=screenshots.ToArray();report.sectionsVisited=sections.Count;report.uniqueDeaths=deadLives.Count;
            if(owner!=null){report.experience=owner.Progression.TotalExperience;report.gold=owner.Progression.TotalGold;
                report.totalDamage=owner.Progression.TotalDamage;report.enhancementRank=owner.Progression.EquippedWeapon.EnhancementRank;
                report.spentTalentPoints=owner.Progression.SpentPoints;report.equippedItemId=owner.Progression.EquippedWeapon.Id;
                report.inventoryCount=owner.Progression.Inventory.Count;}
            if(hunt!=null){report.completedRuns=hunt.CompletedRuns;report.totalKills=hunt.TotalKills;report.collectedItems=hunt.CollectedItems;
                report.deathRetries=hunt.DeathRetries;report.travelDistance=hunt.TravelDistance;report.directorFault=hunt.LastFault??"";
                report.detourQueries=hunt.World==null?0:hunt.World.DetourQueries;report.targetSelections=hunt.TargetSelections;}
            try{Directory.CreateDirectory(outputDirectory);WriteReport();}
            catch(Exception error){success=false;Debug.LogError("Cannot write auto hunt report: "+error);}
            Debug.Log("AFFIX_AUTO_HUNT_SMOKE_"+(success?"PASS":"FAIL")+" run="+report.runId);
            Application.Quit(success?0:1);
        }
        private void WriteReport()=>File.WriteAllText(Path.Combine(outputDirectory,"auto-hunt-smoke.json"),JsonUtility.ToJson(report,true));
        private sealed class FrozenActor {public MeleeActor actor;public int id,hp;public Vector3 position;public double elapsed;public Sprite sprite;}
        [Serializable] private sealed class Report
        {
            public string schema="affix-auto-hunt-smoke-v1",runId=Guid.NewGuid().ToString("N"),startedUtc=DateTime.UtcNow.ToString("O"),finishedUtc="";
            public string unityVersion=Application.unityVersion,buildGuid=Application.buildGUID,result="RUNNING",status="RUNNING",problem="",phase="initializing",directorFault="";
            public string scope="Actual 1x Windows automatic dungeon cycles, actor walkability, accepted-hit LOS and impact sprites, automatic loot, management without pause, manual pause/resume, framebuffer HUD captures.";
            public string interactionMethod="One native UI Toolkit start ClickEvent plus management/pause callbacks. No physical mouse/keyboard, no stat modification or time acceleration.";
            public string actualUiClickVerification="NOT_RUN",userVisualApproval="NOT_APPROVED",screenshotScope="24-bit BMP from actual end-of-frame ReadPixels with native runtime HUD.";
            public bool initialized,heroHitObserved,enemyHitObserved,managementKeepsHunting,managementSwitchesVerified,manualPauseVerified,manualResumeVerified;
            public int startCallbacks,uiCallbacksDispatched,acceptedHits,lineOfSightChecks,impactPoseChecks,walkabilityChecks,completedRuns,totalKills,collectedItems,deathRetries,sectionsVisited,uniqueDeaths,detourQueries,targetSelections;
            public float elapsedSeconds,travelDistance;
            public int experience,gold,totalDamage,enhancementRank,spentTalentPoints,inventoryCount;
            public string equippedItemId="";
            public string[] screenshots=Array.Empty<string>();
        }
    }
}
#endif
