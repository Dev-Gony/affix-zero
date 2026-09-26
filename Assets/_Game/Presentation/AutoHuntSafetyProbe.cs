#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Isolated fault-injection scenario; never part of ordinary startup or baseline performance verification.
    public sealed class AutoHuntSafetyProbe : MonoBehaviour
    {
        private FirstEncounter owner;
        private AutoHuntDirector hunt;
        private MeleeActor hero;
        private Report report;
        private string outputDirectory, mode="deaths", phase="initializing";
        private float started, holdStarted;
        private bool finishing, startedHunt;
        private int injectedLifeId;
        private HoldState hold;
        private readonly HashSet<int> deadLives=new HashSet<int>();
        private readonly List<int> injectedLives=new List<int>();
        private readonly List<string> fixtureIds=new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args=Environment.GetCommandLineArgs();
            if(Array.IndexOf(args,"-affixAutoHuntSafetyTest")<0)return;
            var host=new GameObject("Affix Auto Hunt Safety Probe");DontDestroyOnLoad(host);
            host.AddComponent<AutoHuntSafetyProbe>().Begin(args);
        }
        private void Begin(string[] args)
        {
            report=new Report();started=Time.realtimeSinceStartup;
            outputDirectory=Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,"Build","Reports"));
            Application.logMessageReceived+=OnLog;
            try
            {
                int directoryIndex=Array.IndexOf(args,"-affixReportDir");
                if(directoryIndex>=0)
                {
                    Require(directoryIndex+1<args.Length&&Path.IsPathRooted(args[directoryIndex+1]),"-affixReportDir requires an absolute path.");
                    outputDirectory=Path.GetFullPath(args[directoryIndex+1]);
                }
                int modeIndex=Array.IndexOf(args,"-affixSafetyMode");
                if(modeIndex>=0){Require(modeIndex+1<args.Length,"Missing safety mode.");mode=args[modeIndex+1];}
                Require(mode=="deaths"||mode=="bag-full","Safety mode must be deaths or bag-full.");
                Require(Array.IndexOf(args,"-affixAutoHuntTest")<0&&Array.IndexOf(args,"-affixSmokeTest")<0,
                    "Safety fault injection must run separately from baseline or progression probes.");
                report.mode=mode;
                report.faultInjection=mode=="deaths"
                    ?"After each hero life reset, SetAttackDamage(1) replaces only the actor's attack damage. No direct damage/death calls; real enemy attacks cause both deaths."
                    :"Before the sole start callback, 24 synthetic test:bag:<index> weapons are added through TryCreatePendingLoot/PickUp. No item deletion, currency injection, or damage modification.";
                Directory.CreateDirectory(outputDirectory);WriteReport();
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void LateUpdate()
        {
            if(finishing||report==null)return;
            try
            {
                if(Time.realtimeSinceStartup-started>90)throw new TimeoutException("Safety scenario exceeded 90 real seconds in "+phase);
                Tick();
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void Tick()
        {
            if(!startedHunt)
            {
                owner=FindFirstObjectByType<FirstEncounter>();
                if(owner==null||owner.Hunt==null||!owner.Hunt.Initialized||owner.Hero==null||!owner.Hero.IsReady)return;
                hunt=owner.Hunt;hero=owner.Hero;
                var doc=owner.GetComponent<UIDocument>();
                if(doc==null||doc.rootVisualElement.panel==null||doc.rootVisualElement.worldBound.width<=0)return;
                var startControl=doc.rootVisualElement.Q("autohunt-toggle");
                if(startControl==null||!startControl.enabledInHierarchy||startControl.worldBound.width<=0)return;
                Require(!hunt.Running&&owner.IsPaused&&Time.timeScale==0,"Safety test requires an initialized, stopped player.");
                Require(hero.Hp==hero.MaxHp&&hero.Damage==30&&owner.Progression.TotalDamage==30,"Safety fixture requires fresh baseline actor stats.");
                Require(owner.Progression.Inventory.Count==0&&owner.Progression.PendingLoot==null&&owner.Progression.TotalGold==0&&
                    owner.Progression.TotalExperience==0,"Safety test requires an empty, fresh in-memory profile.");
                hero.Damaged+=OnHeroDamaged;
                if(mode=="bag-full")FillBag();
                DispatchStart();startedHunt=true;phase="observing";
                Require(hunt.Running&&!owner.IsPaused,"Single native start callback failed to start hunting.");
                if(mode=="deaths")InjectWeakAttack();
                report.initialized=true;WriteReport();return;
            }
            Require(owner!=null&&hunt!=null&&hero!=null,"Safety actor/director disappeared.");
            Require(owner.IsPaused?Mathf.Approximately(Time.timeScale,0):Mathf.Approximately(Time.timeScale,1),"Unexpected time acceleration or pause scale.");
            report.realTimeScaleVerified=true;
            if(mode=="deaths")
            {
                if(hunt.Running)InjectWeakAttack();
                Require(hunt.FailedRuns<=2&&hunt.DeathRetries<=1,"The director exceeded the bounded retry policy.");
                Require(hunt.CompletedRuns==0&&hunt.TotalKills==0,"Weak-attack fixture unexpectedly completed a run or killed an enemy.");
                if(!hunt.Running&&phase=="observing")
                {
                    Require(deadLives.Count==2&&hero.IsDead&&hunt.FailedRuns==2&&hunt.ConsecutiveFailures==2&&hunt.DeathRetries==1,
                        "Expected two observed real deaths and exactly one retry before stopping.");
                    Require(owner.IsPaused&&Time.timeScale==0&&hunt.LastFault.Contains("연속 2회"),"Death safety stop did not expose the repeated-death fault.");
                    Require(injectedLives.Count==2,"Weak attack was not injected exactly once for each observed hero life.");
                    report.realDeathsVerified=true;StartHold();
                }
            }
            else
            {
                CheckFixture();Require(hero.Damage==30,"Bag-full test must not change attack damage.");
                Require(hunt.FailedRuns==0&&hunt.DeathRetries==0&&hunt.CompletedRuns==0,"Bag capacity scenario failed or cleared a run before stopping.");
                if(!hunt.Running&&phase=="observing")
                {
                    Require(hunt.TotalKills==1&&owner.Progression.PendingLoot!=null&&owner.Progression.PendingLoot.Id=="loot:ember-steel:first",
                        "Bag-full stop did not preserve the actual first-kill weapon drop.");
                    Require(owner.IsPaused&&Time.timeScale==0&&hunt.LastFault.Contains("가방"),"Bag-full stop did not expose the capacity fault.");
                    Require(hunt.CollectedItems==0&&owner.Progression.TotalExperience==25&&owner.Progression.TotalGold==8,
                        "Bag-full stop lost/duplicated the first kill reward or overwrote an item.");
                    report.pendingLootPreserved=true;StartHold();
                }
            }
            if(phase=="holding")
            {
                CheckHold();
                if(Time.realtimeSinceStartup-holdStarted>=4)
                {
                    report.stoppedHoldSeconds=Time.realtimeSinceStartup-holdStarted;
                    report.noInfiniteRetryVerified=mode=="deaths";
                    report.noItemLossVerified=mode=="bag-full";
                    report.stoppedStateStable=true;Finish(true,null);
                }
            }
        }
        private void FillBag()
        {
            Require(HeroProgression.InventoryCapacity==24,"Safety fixture expects 24 inventory slots.");
            for(int i=0;i<24;i++)
            {
                string id="test:bag:"+i;
                var item=new WeaponItem(id,"Safety fixture "+i,1,0,string.Empty,"AffixGenerated/AttackIcon","Common");
                Require(owner.Progression.TryCreatePendingLoot(item)&&owner.Progression.PickUp(),"Could not construct the explicit bag-capacity fixture.");
                fixtureIds.Add(id);
            }
            Require(owner.Progression.PendingLoot==null,"Fixture unexpectedly left pending loot before gameplay.");
            report.injectedItemCount=fixtureIds.Count;CheckFixture();
        }
        private void CheckFixture()
        {
            Require(owner.Progression.Inventory.Count==24,"Inventory capacity fixture changed size.");
            for(int i=0;i<fixtureIds.Count;i++)
                Require(owner.Progression.Inventory[i].Id==fixtureIds[i]&&owner.Progression.Inventory[i].FlatDamage==1,
                    "An existing fixture item was lost, overwritten or reordered.");
        }
        private void InjectWeakAttack()
        {
            if(hero.ActorId==injectedLifeId)return;
            Require(!hero.IsDead&&hero.Hp==hero.MaxHp,"Damage injection did not occur on a fresh hero life.");
            Require(!hero.IsAttacking,"Fresh-life injection would alter an already prepared attack.");
            hero.SetAttackDamage(1);injectedLifeId=hero.ActorId;injectedLives.Add(injectedLifeId);
            report.attackDamageInjectionCount=injectedLives.Count;
        }
        private void OnHeroDamaged(MeleeActor actor,HitReceipt receipt)
        {
            if(finishing||!receipt.Accepted)return;
            try
            {
                Require(!owner.IsPaused&&hunt.Running,"Hero damage occurred outside running simulation.");
                bool actualEnemy=false;
                foreach(var enemy in hunt.Enemies)
                    if(enemy!=null&&enemy.ActorId==actor.LastAttackerId&&enemy.isActiveAndEnabled){actualEnemy=true;break;}
                Require(actualEnemy,"Safety death damage did not come from an actual enemy actor.");
                report.acceptedEnemyHits++;
                if(receipt.Killed)Require(deadLives.Add(actor.ActorId),"One hero life emitted more than one death receipt.");
                report.observedHeroDeaths=deadLives.Count;
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void StartHold()
        {
            hold=new HoldState {kills=hunt.TotalKills,retries=hunt.DeathRetries,failed=hunt.FailedRuns,runs=hunt.CompletedRuns,loot=hunt.CollectedItems,
                gold=owner.Progression.TotalGold,xp=owner.Progression.TotalExperience,heroId=hero.ActorId,hp=hero.Hp,
                pendingId=owner.Progression.PendingLoot==null?null:owner.Progression.PendingLoot.Id,fault=hunt.LastFault};
            holdStarted=Time.realtimeSinceStartup;phase="holding";report.safetyStopObserved=true;
        }
        private void CheckHold()
        {
            Require(!hunt.Running&&owner.IsPaused&&Time.timeScale==0,"Stopped safety state resumed without user input.");
            Require(hunt.TotalKills==hold.kills&&hunt.DeathRetries==hold.retries&&hunt.FailedRuns==hold.failed&&hunt.CompletedRuns==hold.runs&&
                hunt.CollectedItems==hold.loot&&owner.Progression.TotalGold==hold.gold&&owner.Progression.TotalExperience==hold.xp&&
                hero.ActorId==hold.heroId&&hero.Hp==hold.hp&&hunt.LastFault==hold.fault,"Safety stop progressed, retried or changed rewards during the four-second hold.");
            string pending=owner.Progression.PendingLoot==null?null:owner.Progression.PendingLoot.Id;
            Require(pending==hold.pendingId,"Safety stop discarded/replaced pending loot.");
            if(mode=="bag-full")CheckFixture();
            else Require(owner.Progression.Inventory.Count==0&&owner.Progression.PendingLoot==null,"Death scenario created unexpected inventory changes.");
        }
        private void DispatchStart()
        {
            var element=owner.GetComponent<UIDocument>().rootVisualElement.Q("autohunt-toggle");
            Require(element!=null&&element.enabledInHierarchy&&element.worldBound.width>0&&element.worldBound.height>0,"Native auto-hunt start control missing or disabled.");
            for(var node=element;node!=null;node=node.parent)
                Require(node.resolvedStyle.display!=DisplayStyle.None&&node.resolvedStyle.visibility==Visibility.Visible,"Auto-hunt start control hidden.");
            using(var click=ClickEvent.GetPooled()){click.target=element;element.SendEvent(click);}
            report.startCallbacks++;
        }
        private void OnLog(string message,string trace,LogType type)
        {if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)Finish(false,message+"\n"+trace);}
        private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        private void Finish(bool success,string problem)
        {
            if(finishing)return;finishing=true;Application.logMessageReceived-=OnLog;
            if(hero!=null)hero.Damaged-=OnHeroDamaged;
            report.mode=mode;report.result=success?"PASS":"FAIL";report.status=report.result;report.problem=problem??"";report.phase=phase;
            report.finishedUtc=DateTime.UtcNow.ToString("O");report.elapsedSeconds=Time.realtimeSinceStartup-started;
            report.injectedHeroLifeIds=injectedLives.ToArray();report.fixtureItemIds=fixtureIds.ToArray();
            if(hunt!=null){report.failedRuns=hunt.FailedRuns;report.consecutiveFailures=hunt.ConsecutiveFailures;report.deathRetries=hunt.DeathRetries;
                report.completedRuns=hunt.CompletedRuns;report.totalKills=hunt.TotalKills;report.collectedItems=hunt.CollectedItems;report.directorFault=hunt.LastFault??"";
                report.runningAtFinish=hunt.Running;}
            if(owner!=null){report.gold=owner.Progression.TotalGold;report.experience=owner.Progression.TotalExperience;
                report.inventoryCount=owner.Progression.Inventory.Count;report.pendingLootId=owner.Progression.PendingLoot?.Id??"";report.pausedAtFinish=owner.IsPaused;}
            try{Directory.CreateDirectory(outputDirectory);WriteReport();}
            catch(Exception error){success=false;Debug.LogError("Cannot write auto hunt safety report: "+error);}
            Debug.Log("AFFIX_AUTO_HUNT_SAFETY_"+(success?"PASS":"FAIL")+" mode="+mode+" run="+report.runId);
            Application.Quit(success?0:1);
        }
        private void WriteReport()=>File.WriteAllText(Path.Combine(outputDirectory,"auto-hunt-safety.json"),JsonUtility.ToJson(report,true));
        private sealed class HoldState {public int kills,retries,failed,runs,loot,gold,xp,heroId,hp;public string pendingId,fault;}
        [Serializable] private sealed class Report
        {
            public string schema="affix-auto-hunt-safety-v1",runId=Guid.NewGuid().ToString("N"),startedUtc=DateTime.UtcNow.ToString("O"),finishedUtc="";
            public string unityVersion=Application.unityVersion,buildGuid=Application.buildGUID,mode="deaths",result="RUNNING",status="RUNNING",problem="",phase="initializing",directorFault="",pendingLootId="";
            public string scope="Isolated 1x safety fault injection. This is not baseline performance, normal balance, long-duration stability or user approval evidence.";
            public string faultInjection="",interactionMethod="One native UI Toolkit ClickEvent start; no physical input, no direct death/damage calls, no time acceleration.";
            public string actualUiClickVerification="NOT_RUN",userVisualApproval="NOT_APPROVED";
            public bool initialized,realTimeScaleVerified,realDeathsVerified,pendingLootPreserved,safetyStopObserved,stoppedStateStable,noInfiniteRetryVerified,noItemLossVerified,runningAtFinish,pausedAtFinish;
            public int startCallbacks,attackDamageInjectionCount,acceptedEnemyHits,observedHeroDeaths,injectedItemCount,failedRuns,consecutiveFailures,deathRetries,completedRuns,totalKills,collectedItems,gold,experience,inventoryCount;
            public float elapsedSeconds,stoppedHoldSeconds;
            public int[] injectedHeroLifeIds=Array.Empty<int>();public string[] fixtureItemIds=Array.Empty<string>();
        }
    }
}
#endif
