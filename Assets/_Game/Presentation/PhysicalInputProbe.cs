#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.IO;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Observes real Windows mouse/key input. It never dispatches synthetic UI Toolkit events.
    public sealed class PhysicalInputProbe : MonoBehaviour
    {
        private FirstEncounter owner; private Report report; private string reportDirectory, phase="initializing", equippedBefore;
        private float started; private int forgeRankBefore; private bool finishing;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            string[] args=Environment.GetCommandLineArgs(); if(Array.IndexOf(args,"-affixPhysicalInputTest")<0)return;
            var host=new GameObject("Affix Physical Input Probe");DontDestroyOnLoad(host);host.AddComponent<PhysicalInputProbe>().Begin(args);
        }
        private void Begin(string[] args)
        {
            started=Time.realtimeSinceStartup;report=new Report();Application.logMessageReceived+=OnLog;
            try{reportDirectory=AbsoluteArgument(args,"-affixReportDir");Directory.CreateDirectory(reportDirectory);WriteReport();}
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void LateUpdate()
        {
            if(finishing||report==null)return;
            try
            {
                if(Time.realtimeSinceStartup-started>150)throw new TimeoutException("Physical input timed out in "+phase);
                Tick();
            }
            catch(Exception error){Finish(false,error.ToString());}
        }
        private void Tick()
        {
            if(phase=="initializing")
            {
                owner=FindFirstObjectByType<FirstEncounter>();
                if(owner==null||owner.Hunt==null||!owner.Hunt.Initialized||owner.Hero==null||!owner.Hero.IsReady||Root?.panel==null)return;
                Require(owner.Persistence.CanPlay&&!owner.Persistence.Ephemeral,"Physical test profile is not isolated writable state.");
                Require(owner.Progression.Inventory.Count>0&&owner.Progression.SpentPoints>0&&owner.Progression.TotalGold>=owner.Progression.EquippedEnhancementCost,
                    "Physical test requires the copied natural-progression profile with bag, talents and forge gold.");
                equippedBefore=EquippedIds();forgeRankBefore=owner.Progression.EquippedWeapon.EnhancementRank;
                Phase("await-start-click");return;
            }
            if(phase=="await-start-click"&&owner.Hunt.Running){report.mouseStart=true;Phase("await-i-key");return;}
            if(phase=="await-i-key"&&Input.GetKeyDown(KeyCode.I)&&owner.Screen==ManagementScreen.Equipment)
            {report.keyboardEquipment=true;Phase("await-equipment-clicks");return;}
            if(phase=="await-equipment-clicks"&&EquippedIds()!=equippedBefore)
            {report.mouseEquipment=true;Phase("await-k-key");return;}
            if(phase=="await-k-key"&&Input.GetKeyDown(KeyCode.K)&&owner.Screen==ManagementScreen.Talents)
            {report.keyboardTalents=true;Phase("await-reset-click");return;}
            if(phase=="await-reset-click"&&owner.Progression.SpentPoints==0&&owner.Progression.UnspentPoints>0)
            {report.mouseTalentReset=true;Phase("await-talent-clicks");return;}
            if(phase=="await-talent-clicks"&&owner.Progression.VitalityRank==1&&owner.Progression.SpentPoints==1)
            {report.mouseTalentInvest=true;Phase("await-f-key");return;}
            if(phase=="await-f-key"&&Input.GetKeyDown(KeyCode.F)&&owner.Screen==ManagementScreen.Forge)
            {report.keyboardForge=true;forgeRankBefore=owner.Progression.EquippedWeapon.EnhancementRank;Phase("await-forge-click");return;}
            if(phase=="await-forge-click"&&owner.Progression.EquippedWeapon.EnhancementRank==forgeRankBefore+1)
            {report.mouseForge=true;Phase("await-escape-pause");return;}
            if(phase=="await-escape-pause"&&Input.GetKeyDown(KeyCode.Escape)&&owner.IsPaused)
            {report.keyboardPause=true;Phase("await-escape-resume");return;}
            if(phase=="await-escape-resume"&&Input.GetKeyDown(KeyCode.Escape)&&!owner.IsPaused)
            {
                report.keyboardResume=true;Require(owner.SaveProgress(),"Physical input state save failed: "+owner.Persistence.Problem);
                Require(report.mouseStart&&report.keyboardEquipment&&report.mouseEquipment&&report.keyboardTalents&&report.mouseTalentReset&&
                    report.mouseTalentInvest&&report.keyboardForge&&report.mouseForge&&report.keyboardPause&&report.keyboardResume,
                    "Physical input evidence is incomplete.");Finish(true,null);
            }
        }
        private VisualElement Root=>owner?.GetComponent<UIDocument>()?.rootVisualElement;
        private string EquippedIds()
        {
            HeroProgression p=owner.Progression;return string.Join("|",new[]{p.EquippedWeapon?.Id,p.EquippedArmor?.Id,p.EquippedRelic?.Id,
                p.EquippedHelmet?.Id,p.EquippedGloves?.Id,p.EquippedBoots?.Id,p.EquippedRing?.Id,p.EquippedAmulet?.Id});
        }
        private void Phase(string value){phase=value;report.phase=value;WriteReport();}
        private void OnLog(string message,string trace,LogType type)
        {if(!finishing&&(type==LogType.Error||type==LogType.Exception||type==LogType.Assert))Finish(false,message+"\n"+trace);}
        private void Finish(bool success,string problem)
        {
            if(finishing)return;finishing=true;Application.logMessageReceived-=OnLog;report.result=report.status=success?"PASS":"FAIL";
            report.problem=problem??"";report.phase=phase;report.elapsedSeconds=Time.realtimeSinceStartup-started;
            report.finishedUtc=DateTime.UtcNow.ToString("O");if(owner!=null){report.saveCount=owner.Persistence.SaveCount;report.finalDamage=owner.Progression.TotalDamage;
                report.finalHealth=owner.Progression.TotalMaxHp;report.finalForgeRank=owner.Progression.EquippedWeapon.EnhancementRank;}
            try{WriteReport();}catch(Exception error){Debug.LogError("Physical input report failed: "+error);success=false;}
            Debug.Log("AFFIX_PHYSICAL_INPUT_"+(success?"PASS":"FAIL")+" run="+report.runId);Application.Quit(success?0:1);
        }
        private void WriteReport()=>File.WriteAllText(Path.Combine(reportDirectory,"physical-input.json"),JsonUtility.ToJson(report,true));
        private static string AbsoluteArgument(string[] args,string key)
        {int i=Array.IndexOf(args,key);if(i<0||i+1>=args.Length||!Path.IsPathRooted(args[i+1]))throw new ArgumentException(key+" requires an absolute path.");return Path.GetFullPath(args[i+1]);}
        private static void Require(bool value,string problem){if(!value)throw new InvalidOperationException(problem);}
        [Serializable] private sealed class Report
        {
            public string schema="affix-physical-input-v1",runId=Guid.NewGuid().ToString("N"),result="RUNNING",status="RUNNING",problem="",phase="initializing",
                startedUtc=DateTime.UtcNow.ToString("O"),finishedUtc="",unityVersion=Application.unityVersion,buildGuid=Application.buildGUID;
            public string scope="Real Windows mouse clicks and keyboard I/K/F/Escape events against the native player window; copied isolated natural-progression profile; no UI Toolkit event dispatch.";
            public bool mouseStart,keyboardEquipment,mouseEquipment,keyboardTalents,mouseTalentReset,mouseTalentInvest,keyboardForge,mouseForge,keyboardPause,keyboardResume;
            public float elapsedSeconds;public int saveCount,finalDamage,finalHealth,finalForgeRank;
        }
    }
}
#endif
