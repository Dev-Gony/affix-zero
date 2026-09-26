using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    public enum HuntPhase { Waiting, Exploring, Fighting, Collecting, Returning, Resting, Recovering, Blocked }

    // Decides where to go; MeleeActor still owns movement animation and the real attack timeline.
    [DefaultExecutionOrder(-100)]
    public sealed class AutoHuntDirector : MonoBehaviour
    {
        private static readonly Vector2 Entrance=new Vector2(-10,-4);
        private static readonly Vector2[] Areas={new Vector2(-9,0),new Vector2(8,0),new Vector2(8,-5)};
        private FirstEncounter owner;
        private readonly List<MeleeActor> enemies=new List<MeleeActor>();
        private MeleeActor hero;
        private float delay, stalledTime, decisionTime;
        private Vector2 lastPosition, progressPosition;
        private int identity=10;
        private bool runStarted, clearDropOffered;
        private HuntPhase afterLoot, beforeBlocked;
        public DungeonWorld World { get; private set; }
        public IReadOnlyList<MeleeActor> Enemies=>enemies.AsReadOnly();
        public bool Initialized { get; private set; }
        public bool Running { get; private set; }
        public HuntPhase Phase { get; private set; }=HuntPhase.Waiting;
        public int SectionIndex { get; private set; }
        public int CompletedRuns { get; private set; }
        public int TotalKills { get; private set; }
        public int CollectedItems { get; private set; }
        public int DeathRetries { get; private set; }
        public int FailedRuns { get; private set; }
        public int ConsecutiveFailures { get; private set; }
        public int TargetSelections { get; private set; }
        public float TravelDistance { get; private set; }
        public string LastFault { get; private set; }="";
        public string StateCaption
        {
            get
            {
                if(!Running)return Phase==HuntPhase.Blocked?LastFault:"자동사냥 대기";
                switch(Phase)
                {
                    case HuntPhase.Exploring:return "다음 구역 탐색";
                    case HuntPhase.Fighting:return "적 탐색 · 자동 전투";
                    case HuntPhase.Collecting:return "전리품 자동 회수";
                    case HuntPhase.Returning:return "입구로 귀환";
                    case HuntPhase.Resting:return "클리어 · 재정비";
                    case HuntPhase.Recovering:return "쓰러짐 · 재도전 준비";
                    default:return "자동사냥";
                }
            }
        }

        private void Start()
        {
            owner=GetComponent<FirstEncounter>();hero=owner.Hero;
            World=new DungeonWorld();
            enemies.Add(owner.Enemy);
            var clone=Instantiate(owner.Enemy.gameObject);
            clone.name="Orc Scout";enemies.Add(clone.GetComponent<MeleeActor>());
            foreach(var actor in enemies){actor.SetTarget(null);actor.Damaged+=OnEnemyDamaged;actor.SetNavigation(World.NextWaypoint,World.LineOfSight);}
            hero.SetTarget(null);hero.SetNavigation(World.NextWaypoint,World.LineOfSight);
            hero.ConfigureMoveSpeed(3.2f);
            foreach(var actor in enemies)actor.ConfigureMoveSpeed(1.8f);
        }
        private void Update()
        {
            if(World==null)return;
            if(!Initialized)
            {
                if(!hero.IsReady)return;
                foreach(var actor in enemies)if(!actor.IsReady)return;
                hero.ResetForEncounter(Entrance,++identity,120,owner.Progression.TotalDamage);
                foreach(var actor in enemies)actor.gameObject.SetActive(false);
                lastPosition=progressPosition=hero.transform.position;
                Initialized=true;return;
            }
            Vector2 position=hero.transform.position;
            TravelDistance+=Vector2.Distance(lastPosition,position);lastPosition=position;
            if(!Running||owner.IsPaused)return;
            if(hero.IsDead)
            {
                if(Phase!=HuntPhase.Recovering)
                {
                    Phase=HuntPhase.Recovering;delay=2;ClearTargets();FailedRuns++;ConsecutiveFailures++;
                    if(ConsecutiveFailures>=2){Block("연속 2회 쓰러졌습니다. 장비를 정비한 뒤 재개하세요.");return;}
                }
                delay-=Time.deltaTime;
                if(delay<=0){DeathRetries++;BeginRun();}
                return;
            }
            if(owner.Progression.PendingLoot!=null)
            {
                if(Phase!=HuntPhase.Collecting)afterLoot=Phase;
                Phase=HuntPhase.Collecting;hero.SetTarget(null);hero.SetDestination(owner.LootPosition);
                if(Vector2.Distance(position,owner.LootPosition)<.55f)
                {
                    if(owner.CollectLoot()){CollectedItems++;hero.SetDestination(null);stalledTime=0;}
                    else {Block("가방이 가득 찼습니다. 공간을 비운 뒤 재개하세요.");return;}
                }
                CheckMovement(position);return;
            }
            if(Phase==HuntPhase.Collecting)Phase=afterLoot;
            if(Phase==HuntPhase.Resting)
            {delay-=Time.deltaTime;if(delay<=0)BeginRun();return;}
            if(Phase==HuntPhase.Exploring)
            {
                hero.SetTarget(null);hero.SetDestination(Areas[SectionIndex]);
                if(Vector2.Distance(position,Areas[SectionIndex])<.4f){SpawnArea();Phase=HuntPhase.Fighting;stalledTime=0;}
                else CheckMovement(position);
                return;
            }
            if(Phase==HuntPhase.Fighting)
            {
                if(AllDefeated())
                {
                    hero.SetTarget(null);hero.SetDestination(null);
                    if(SectionIndex<Areas.Length-1){SectionIndex++;Phase=HuntPhase.Exploring;}
                    else Phase=HuntPhase.Returning;
                    stalledTime=0;return;
                }
                decisionTime-=Time.deltaTime;
                if(decisionTime<=0){ChooseTarget();decisionTime=.2f;}
                foreach(var actor in enemies)
                    if(actor.isActiveAndEnabled&&!actor.IsDead)actor.SetTarget(Vector2.Distance(actor.transform.position,position)<5?hero:null);
                bool atMelee=hero.CurrentTarget!=null&&Vector2.Distance(position,hero.CurrentTarget.transform.position)<1.15f&&World.LineOfSight(position,hero.CurrentTarget.transform.position);
                if(!hero.IsAttacking&&!atMelee)CheckMovement(position);else stalledTime=0;
                return;
            }
            if(Phase==HuntPhase.Returning)
            {
                hero.SetTarget(null);hero.SetDestination(Entrance);
                if(Vector2.Distance(position,Entrance)<.4f)
                {
                    if(!clearDropOffered)
                    {
                        clearDropOffered=true;
                        owner.OfferClearLoot(CompletedRuns,Entrance);
                        return;
                    }
                    CompletedRuns++;ConsecutiveFailures=0;Phase=HuntPhase.Resting;delay=1;hero.SetDestination(null);ClearTargets();stalledTime=0;
                }
                else CheckMovement(position);
            }
        }

        public void StartHunt()
        {
            if(!Initialized || !owner.CanProgress)return;
            if(owner.Progression.PendingLoot!=null&&owner.Progression.Inventory.Count>=HeroProgression.InventoryCapacity)
            {LastFault="가방 공간을 먼저 비우세요.";return;}
            LastFault="";Running=true;owner.SetPaused(false);
            if(!runStarted){BeginRun();return;}
            if(Phase==HuntPhase.Blocked){Phase=beforeBlocked;ConsecutiveFailures=0;}
            progressPosition=hero.transform.position;stalledTime=0;
        }
        public void StopHunt(){Running=false;owner.SetPaused(true);}
        private void BeginRun()
        {
            owner.BeginAutoRun();runStarted=true;clearDropOffered=false;SectionIndex=0;Phase=HuntPhase.Exploring;
            ClearTargets();foreach(var actor in enemies)actor.gameObject.SetActive(false);
            hero.ResetForEncounter(Entrance,++identity,120,owner.Progression.TotalDamage);
            lastPosition=progressPosition=Entrance;stalledTime=0;decisionTime=0;
        }
        private void SpawnArea()
        {
            for(int i=0;i<enemies.Count;i++)
            {
                var actor=enemies[i];actor.gameObject.SetActive(true);
                Vector2 spawn=Areas[SectionIndex]+new Vector2(i==0?-1.7f:1.7f,1.7f);
                actor.ResetForEncounter(spawn,++identity,54,6);actor.SetTarget(hero);
            }
            hero.SetDestination(null);ChooseTarget();
        }
        private void ChooseTarget()
        {
            if(hero.IsAttacking)return;
            var candidates=new List<GridCell>();var actors=new List<MeleeActor>();
            foreach(var actor in enemies)if(actor.isActiveAndEnabled&&!actor.IsDead){actors.Add(actor);candidates.Add(World.Cell(actor.transform.position));}
            if(World.Navigation.TryNearestReachable(World.Cell(hero.transform.position),candidates,out int index,out _))
            {
                var selected=actors[index];if(hero.CurrentTarget!=selected)TargetSelections++;
                hero.SetTarget(selected);hero.SetDestination(null);owner.FocusEnemy(selected);
            }
            else Block("도달 가능한 적이 없습니다. 사냥을 중지했습니다.");
        }
        private bool AllDefeated(){foreach(var actor in enemies)if(actor.isActiveAndEnabled&&!actor.IsDead)return false;return true;}
        private void CheckMovement(Vector2 position)
        {
            if(Vector2.Distance(position,progressPosition)>.15f){stalledTime=0;progressPosition=position;return;}
            stalledTime+=Time.deltaTime;
            // Attack preparation and hurt reactions can stop motion legitimately for short intervals.
            if(stalledTime>6)Block("이동 경로를 확인할 수 없어 사냥을 중지했습니다.");
        }
        private void Block(string reason)
        {
            Debug.LogWarning("Hunt stopped: "+reason+" phase="+Phase+" hero="+hero.transform.position+" target="+(hero.CurrentTarget==null?"none":hero.CurrentTarget.transform.position.ToString()));
            LastFault=reason;if(Phase!=HuntPhase.Blocked)beforeBlocked=Phase;Phase=HuntPhase.Blocked;StopHunt();
        }
        private void ClearTargets(){hero.SetTarget(null);hero.SetDestination(null);foreach(var actor in enemies){actor.SetTarget(null);actor.SetDestination(null);}}
        private void OnEnemyDamaged(MeleeActor actor,HitReceipt receipt)
        {
            if(!receipt.Killed||hero.IsDead)return;
            if(owner.RegisterDefeat(actor)){TotalKills++;stalledTime=0;}
        }
        private void OnDestroy(){foreach(var actor in enemies)if(actor!=null)actor.Damaged-=OnEnemyDamaged;World?.Dispose();}
    }
}
