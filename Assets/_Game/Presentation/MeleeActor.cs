using System;
using System.Collections.Generic;
using UnityEngine;
using AffixZero.Core;

namespace AffixZero.Presentation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class MeleeActor : MonoBehaviour
    {
        [SerializeField] private ActorAnimationSet animationSet;
        [SerializeField] private MeleeActor target;
        [SerializeField] private int actorId = 1;
        [SerializeField, Min(1)] private int maximumHp = 120;
        [SerializeField, Min(1)] private int damage = 24;
        [SerializeField, Min(0)] private int defense = 2;
        [SerializeField, Min(0.01f)] private float moveSpeed = 1.6f;
        [SerializeField, Min(0.01f)] private float reach = 1.0f;
        private SpriteRenderer view;
        private SpriteRenderer weaponView;
        private AttackTimeline timeline;
        private CombatHealth health;
        private MeleeActor attackTarget;
        private int swingDamage;
        private double clipTime;
        private double hurtRemaining;
        private ActorClip clip = ActorClip.Idle;
        private ActorFacing facing = ActorFacing.Down;
        private Func<Vector2, Vector2, Vector2> nextWaypoint;
        private Func<Vector2, Vector2, bool> lineOfSight;
        private Vector2? destination;
        private bool invalidWaypointReported;
        private IReadOnlyList<MeleeActor> cleaveTargets;
        private float cleaveRadius=1.85f;
        private long damageSequence;
        private Vector2 swingDirection;
        private bool attacksAllowed=true;
        private float attackSpeedMultiplier=1,cleaveFraction=1;
        private bool ranged;
        private int criticalChance,vampirismPercent,penetration;

        public int Hp => health == null ? maximumHp : health.Current;
        public int MaxHp => maximumHp;
        public int Damage => damage;
        public int Defense => defense;
        public float MoveSpeed => moveSpeed;
        public bool IsDead => health != null && health.IsDead;
        public bool IsReady => health != null;
        public int ActorId => actorId;
        public int DeathCount => health == null ? 0 : health.DeathCount;
        public int Heal(int amount) => health == null || amount <= 0 ? 0 : health.Heal(amount);
        public ActorClip CurrentClip => clip;
        public ActorFacing CurrentFacing => facing;
        public ActorAnimationSet AnimationSet => animationSet;
        public MeleeActor CurrentTarget => target;
        public bool IsAttacking => timeline != null && timeline.IsRunning;
        public double AttackElapsed => timeline == null || !timeline.IsRunning ? 0 : timeline.Elapsed;
        public event Action<MeleeActor, HitReceipt> Damaged;
        public int LastAttackerId { get; private set; }
        public long LastAttackId { get; private set; }
        public int LastHitRawDamage { get; private set; }
        public int LastHitHpBefore {get;private set;}
        public Vector2 LastHitOrigin { get; private set; }
        public Vector2 LastImpactPosition { get; private set; }
        public float LastHitRadius { get; private set; }
        public bool LastHitIsArea { get; private set; }
        public bool LastHitCritical { get; private set; }
        public bool LastHitKilled { get; private set; }
        public float AttackSpeedMultiplier=>attackSpeedMultiplier;
        public float AttackReach=>reach;
        public bool IsRanged=>ranged;
        public float CleaveRadius=>cleaveRadius;
        public Vector2 LastStrikeDirection=>swingDirection;
        public Vector2 LastStrikePoint {get;private set;}
        public event Action<MeleeActor,int,int,bool> StrikeResolved;
        public event Action<MeleeActor,MeleeActor> AttackStarted;

        public void ConfigureCleave(IReadOnlyList<MeleeActor> victims,float radius)
        {if(!FinitePositive(radius))throw new ArgumentOutOfRangeException(nameof(radius));cleaveTargets=victims;cleaveRadius=Mathf.Max(cleaveRadius,radius);}
        public void SetAttacksAllowed(bool value){attacksAllowed=value;}
        public void ApplyCombatBuild(int maxHp,int armor,int power,float speedMultiplier,float attackReach,
            float splashRadius,float splashFraction,bool isRanged,int critical=0,int vampirism=0,int armorPenetration=0)
        {
            if(maxHp<=0||armor<0||power<=0||!FinitePositive(speedMultiplier)||!FinitePositive(attackReach)||
                float.IsNaN(splashRadius)||float.IsInfinity(splashRadius)||splashRadius<0||
                float.IsNaN(splashFraction)||float.IsInfinity(splashFraction)||splashFraction<0||
                critical<0||critical>100||vampirism<0||vampirism>100||armorPenetration<0)
                throw new ArgumentOutOfRangeException(nameof(maxHp));
            maximumHp=maxHp;defense=armor;damage=power;attackSpeedMultiplier=speedMultiplier;reach=attackReach;ranged=isRanged;
            criticalChance=critical;vampirismPercent=vampirism;penetration=armorPenetration;
            cleaveRadius=isRanged?0:Mathf.Max(attackReach>1.15f?2.5f:1.85f,splashRadius);
            cleaveFraction=isRanged?0:Mathf.Max(.85f,Mathf.Min(1,splashFraction));
            health?.Reconfigure(maxHp,armor);
        }

        public void Configure(ActorAnimationSet set, int id, int hp, int attackDamage)
        { animationSet = set; actorId = id; maximumHp = hp; damage = attackDamage; }
        public void SetTarget(MeleeActor other) { target = other; }
        public void SetNavigation(Func<Vector2, Vector2, Vector2> waypointProvider,
            Func<Vector2, Vector2, bool> visibilityProvider)
        {
            nextWaypoint = waypointProvider;
            lineOfSight = visibilityProvider;
            invalidWaypointReported = false;
        }
        // A living combat target has priority; exploration resumes when it is gone.
        public void SetDestination(Vector2? value)
        {
            if (value.HasValue && !FinitePoint(value.Value)) throw new ArgumentOutOfRangeException(nameof(value));
            destination = value;
        }
        public void ConfigureMoveSpeed(float value)
        {
            if (!FinitePositive(value)) throw new ArgumentOutOfRangeException(nameof(value));
            moveSpeed = value;
        }
        public void ResetForEncounter(Vector2 position, int id, int hp, int attackDamage)
        {
            if (!FinitePoint(position)) throw new ArgumentOutOfRangeException(nameof(position));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (hp <= 0) throw new ArgumentOutOfRangeException(nameof(hp));
            if (attackDamage <= 0) throw new ArgumentOutOfRangeException(nameof(attackDamage));
            string problem = animationSet == null ? "Animation set is missing." : animationSet.ValidateSet();
            if (problem != null) throw new InvalidOperationException(problem);
            actorId = id; maximumHp = hp; damage = attackDamage;
            health = new CombatHealth(maximumHp, defense);
            LastAttackerId=0;
            LastAttackId=0;LastHitCritical=false;LastHitKilled=false;attacksAllowed=true;
            // Preserve the attack sequence across reuse so surviving recipients cannot reject new hits as duplicates.
            if (timeline == null) timeline = animationSet.CreateTimeline();
            else timeline.Cancel();
            target = attackTarget = null;
            destination = null;
            swingDamage = 0;
            hurtRemaining = clipTime = 0;
            clip = ActorClip.Idle;
            facing = ActorFacing.Down;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            PrepareRenderer();
            view.flipX = false;
            view.color=Color.white;
            view.sortingOrder = -(int)Math.Round(position.y * 100);
            RenderClip();
        }
        public void SetAttackDamage(int value)
        {
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            damage = value;
        }

        private void OnDisable()
        {
            timeline?.Cancel();
            attackTarget = null;
        }

        private void Start()
        {
            string problem = animationSet == null ? "Animation set is missing." : animationSet.ValidateSet();
            if (problem != null || actorId <= 0 || maximumHp <= 0 || damage <= 0 || defense < 0
                || !FinitePositive(moveSpeed) || !FinitePositive(reach))
            { Debug.LogError("AFFIX actor setup failed: " + (problem ?? "Invalid actor values."), this); enabled = false; return; }
            PrepareRenderer();
            if (timeline == null) timeline = animationSet.CreateTimeline();
            if (health == null) health = new CombatHealth(maximumHp, defense);
            view.sprite = animationSet.Frame(ActorClip.Idle, 0, facing);
        }

        private void PrepareRenderer()
        {
            if (view == null) view = GetComponent<SpriteRenderer>();
            if (animationSet.HasAttackWeapon && weaponView == null)
            {
                var weapon = new GameObject("Attack Weapon", typeof(SpriteRenderer));
                weapon.transform.SetParent(transform, false);
                weaponView = weapon.GetComponent<SpriteRenderer>();
                weaponView.enabled = false;
            }
        }

        private void Update()
        {
            double delta = Time.deltaTime;
            if (delta <= 0 || health == null) return;
            clipTime += delta;
            view.sortingOrder = -(int)Math.Round(transform.position.y * 100);
            if (IsDead) { SetClip(ActorClip.Death); RenderClip(); return; }
            hurtRemaining=Math.Max(0,hurtRemaining-delta);
            view.color=hurtRemaining>0?new Color(1f,.55f,.45f,1f):Color.white;
            if (timeline.IsRunning)
            {
                // Keep the actual object as well as its ID locked throughout the swing.
                // Selecting a new target cannot redirect damage already in preparation.
                bool alive = attackTarget != null && attackTarget.isActiveAndEnabled
                    && attackTarget.health != null && !attackTarget.IsDead && attackTarget.actorId == timeline.TargetId;
                bool inRange = alive && Vector2.Distance(transform.position, attackTarget.transform.position) <= reach + 0.05f
                    && CanSee(attackTarget.transform.position);
                Impact impact = timeline.Advance(delta*attackSpeedMultiplier, alive, inRange);
                SetClip(ActorClip.Attack);
                // A skipped render frame still displays the impact pose when applying its hit.
                view.sprite = impact.Occurred ? animationSet.ImpactSpriteFor(facing) : animationSet.AttackFrame(timeline, facing);
                RenderWeapon(impact.Occurred);
                if (impact.Occurred) ResolveMeleeImpact();
                if (!timeline.IsRunning) attackTarget = null;
                return;
            }
            if (target == null || !target.isActiveAndEnabled || target.health == null || target.IsDead)
            {
                if (destination.HasValue) MoveToward(destination.Value, (float)delta);
                else { SetClip(hurtRemaining>0?ActorClip.Hit:ActorClip.Idle); RenderClip(); }
                return;
            }

            Vector2 toTarget = target.transform.position - transform.position;
            // Preserve attack direction until recovery finishes.
            Face(toTarget);
            bool visible = CanSee(target.transform.position);
            if (toTarget.magnitude > reach || !visible)
            {
                // Path providers receive the true goal cell; without navigation retain the original standoff movement.
                Vector2 goal = nextWaypoint == null && visible
                    ? (Vector2)target.transform.position - toTarget.normalized * reach
                    : (Vector2)target.transform.position;
                MoveToward(goal, (float)delta);
            }
            else
            {
                if(!attacksAllowed){SetClip(ActorClip.Idle);RenderClip();return;}
                timeline.Begin(target.actorId);
                // Gear changes apply to the next swing, never to an already prepared hit.
                swingDamage = damage;
                attackTarget = target;
                swingDirection=toTarget.sqrMagnitude>.0001f?toTarget.normalized:Vector2.right;
                AttackStarted?.Invoke(this,target);
                SetClip(ActorClip.Attack);
                view.sprite = animationSet.AttackFrame(timeline, facing);
                RenderWeapon(false);
            }
        }

        private bool CanSee(Vector2 point) => lineOfSight == null || lineOfSight(transform.position, point);

        private void MoveToward(Vector2 goal, float delta)
        {
            Vector2 current = transform.position;
            Vector2 waypoint = nextWaypoint == null ? goal : nextWaypoint(current, goal);
            if (!FinitePoint(waypoint))
            {
                if (!invalidWaypointReported) { Debug.LogError("Navigation returned a non-finite waypoint.", this); invalidWaypointReported = true; }
                SetClip(ActorClip.Idle); RenderClip(); return;
            }
            Vector2 position = Vector2.MoveTowards(current, waypoint, moveSpeed * delta);
            Vector2 motion = position - current;
            if (motion.sqrMagnitude <= 0.00000001f)
            { SetClip(ActorClip.Idle); RenderClip(); return; }
            Face(motion);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            SetClip(ActorClip.Walk); RenderClip();
        }

        private void ResolveMeleeImpact()
        {
            long receiptId=++damageSequence;
            Vector2 origin=transform.position;
            LastStrikePoint=attackTarget==null?origin:(Vector2)attackTarget.transform.position;
            int hits=0,kills=0,totalApplied=0;
            float radius=cleaveTargets==null||ranged?reach+.05f:Mathf.Max(reach+.05f,cleaveRadius);
            if(attackTarget!=null)
            {
                int power=PowerAgainst(attackTarget,swingDamage,out bool critical);
                HitReceipt receipt=attackTarget.Receive(actorId,receiptId,power,origin,radius,false,critical);
                if(receipt.Accepted){hits++;totalApplied+=receipt.Damage;if(receipt.Killed)kills++;}
            }
            if(cleaveTargets!=null&&!ranged)
                foreach(MeleeActor victim in cleaveTargets)
                {
                    if(victim==null||victim==attackTarget||victim==this||!victim.isActiveAndEnabled||victim.IsDead)continue;
                    Vector2 offset=(Vector2)victim.transform.position-origin;
                    if(offset.sqrMagnitude>radius*radius||Vector2.Dot(offset.normalized,swingDirection)<-.15f||!CanSee(victim.transform.position))continue;
                    int splashPower=Math.Max(1,(int)Math.Min(int.MaxValue,(double)swingDamage*cleaveFraction));
                    int power=PowerAgainst(victim,splashPower,out bool critical);
                    HitReceipt receipt=victim.Receive(actorId,receiptId,power,origin,radius,false,critical);
                    if(receipt.Accepted){hits++;totalApplied+=receipt.Damage;if(receipt.Killed)kills++;}
                }
            if(vampirismPercent>0&&totalApplied>0)health.Heal(Math.Max(1,totalApplied*vampirismPercent/100));
            StrikeResolved?.Invoke(this,hits,kills,false);
        }

        public int CastAreaStrike(IReadOnlyList<MeleeActor> victims,float radius,int power)
        {
            if(victims==null)throw new ArgumentNullException(nameof(victims));
            if(!FinitePositive(radius)||power<=0)throw new ArgumentOutOfRangeException(nameof(radius));
            if(!isActiveAndEnabled||IsDead||health==null||Time.deltaTime<=0)return 0;
            long receiptId=++damageSequence;
            Vector2 origin=transform.position;
            LastHitRadius=radius;LastHitIsArea=true;LastStrikePoint=origin;
            int hits=0,kills=0,totalApplied=0;
            foreach(MeleeActor victim in victims)
            {
                if(victim==null||victim==this||!victim.isActiveAndEnabled||victim.IsDead||
                    Vector2.Distance(origin,victim.transform.position)>radius||!CanSee(victim.transform.position))continue;
                int resolvedPower=PowerAgainst(victim,power,out bool critical);
                HitReceipt receipt=victim.Receive(actorId,receiptId,resolvedPower,origin,radius,true,critical);
                if(receipt.Accepted){hits++;totalApplied+=receipt.Damage;if(receipt.Killed)kills++;}
            }
            if(vampirismPercent>0&&totalApplied>0)health.Heal(Math.Max(1,totalApplied*vampirismPercent/100));
            StrikeResolved?.Invoke(this,hits,kills,true);
            return hits;
        }

        private int PowerAgainst(MeleeActor victim,int basePower,out bool critical)
        {
            critical=criticalChance>0&&Math.Abs((damageSequence*37+actorId*13)%100)<criticalChance;
            long result=critical?(long)basePower*2:basePower;
            result+=Math.Min(penetration,victim==null?0:victim.Defense);
            return (int)Math.Min(int.MaxValue,result);
        }

        private HitReceipt Receive(int attacker, long attack, int power,Vector2 origin,float radius,bool area,bool critical)
        {
            if (health == null) return default;
            int before=health.Current;
            HitReceipt receipt = health.Receive(attacker, attack, power);
            if (!receipt.Accepted) return receipt;
            LastAttackerId=attacker;
            HitFxBurst.Spawn(transform.position + Vector3.up * .55f,
                view == null ? 1 : view.sortingOrder + 2,critical,receipt.Killed,area);
            LastAttackId=attack;LastHitRawDamage=power;LastHitOrigin=origin;LastImpactPosition=transform.position;
            LastHitHpBefore=before;
            LastHitRadius=radius;LastHitIsArea=area;
            LastHitCritical=critical;LastHitKilled=receipt.Killed;
            // Ordinary hits carry damage and feedback, not hard crowd control.
            // Keep the committed swing and movement alive under multiple attackers.
            hurtRemaining=receipt.Killed?0:critical?.18:.12;
            // Enemy recoil is bounded by the same swept LOS used for navigation, never a stun.
            if(cleaveTargets==null&&lineOfSight!=null)
            {
                Vector2 from=transform.position;
                Vector2 recoil=(from-origin).normalized*(area?.22f:critical?.17f:.11f);
                if(lineOfSight(from,from+recoil))transform.position=new Vector3(from.x+recoil.x,from.y+recoil.y,transform.position.z);
            }
            if(receipt.Killed)
            {
                timeline.Cancel();attackTarget=null;clipTime=0;view.color=Color.white;
                SetClip(ActorClip.Death);RenderClip();
            }
            Damaged?.Invoke(this, receipt);
            return receipt;
        }
        private void SetClip(ActorClip next) { if (clip != next) { clip = next; clipTime = 0; } }
        private void RenderClip()
        {
            view.sprite = animationSet.Frame(clip, clipTime, facing);
            if (weaponView != null) weaponView.enabled = false;
        }
        private void RenderWeapon(bool showImpact)
        {
            if (weaponView == null) return;
            weaponView.enabled = true;
            weaponView.sprite = animationSet.WeaponFrame(timeline, showImpact);
            weaponView.flipX = view.flipX;
            weaponView.sortingLayerID = view.sortingLayerID;
            weaponView.sortingOrder = view.sortingOrder + 1;
        }
        private void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude <= .00000001f) return;
            if (animationSet.UsesFourDirections)
            {
                facing = Math.Abs(direction.x) > Math.Abs(direction.y)
                    ? (direction.x < 0 ? ActorFacing.Left : ActorFacing.Right)
                    : (direction.y < 0 ? ActorFacing.Down : ActorFacing.Up);
                view.flipX = false;
            }
            else if (Math.Abs(direction.x) > .0001f)
            {
                view.flipX = animationSet.SourceFacesRight ? direction.x < 0 : direction.x > 0;
            }
        }
        private static bool FinitePositive(float x) => x > 0 && !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool FinitePoint(Vector2 point) => !float.IsNaN(point.x) && !float.IsInfinity(point.x)
            && !float.IsNaN(point.y) && !float.IsInfinity(point.y);
    }
}
