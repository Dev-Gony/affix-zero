using System;
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
        private Func<Vector2, Vector2, Vector2> nextWaypoint;
        private Func<Vector2, Vector2, bool> lineOfSight;
        private Vector2? destination;
        private bool invalidWaypointReported;

        public int Hp => health == null ? maximumHp : health.Current;
        public int MaxHp => maximumHp;
        public int Damage => damage;
        public int Defense => defense;
        public float MoveSpeed => moveSpeed;
        public bool IsDead => health != null && health.IsDead;
        public bool IsReady => health != null;
        public int ActorId => actorId;
        public int DeathCount => health == null ? 0 : health.DeathCount;
        public ActorClip CurrentClip => clip;
        public ActorAnimationSet AnimationSet => animationSet;
        public MeleeActor CurrentTarget => target;
        public bool IsAttacking => timeline != null && timeline.IsRunning;
        public double AttackElapsed => timeline == null || !timeline.IsRunning ? 0 : timeline.Elapsed;
        public event Action<MeleeActor, HitReceipt> Damaged;
        public int LastAttackerId { get; private set; }

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
            // Preserve the attack sequence across reuse so surviving recipients cannot reject new hits as duplicates.
            if (timeline == null) timeline = animationSet.CreateTimeline();
            else timeline.Cancel();
            target = attackTarget = null;
            destination = null;
            swingDamage = 0;
            hurtRemaining = clipTime = 0;
            clip = ActorClip.Idle;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            PrepareRenderer();
            view.flipX = false;
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
            view.sprite = animationSet.Frame(ActorClip.Idle, 0);
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
                Impact impact = timeline.Advance(delta, alive, inRange);
                SetClip(ActorClip.Attack);
                // A skipped render frame still displays the impact pose when applying its hit.
                view.sprite = impact.Occurred ? animationSet.ImpactSprite : animationSet.AttackFrame(timeline);
                RenderWeapon(impact.Occurred);
                if (impact.Occurred) attackTarget.Receive(actorId, impact.AttackId, swingDamage);
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
            if (Math.Abs(toTarget.x) > 0.01f)
                view.flipX = animationSet.SourceFacesRight ? toTarget.x < 0 : toTarget.x > 0;
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
                timeline.Begin(target.actorId);
                // Gear changes apply to the next swing, never to an already prepared hit.
                swingDamage = damage;
                attackTarget = target;
                SetClip(ActorClip.Attack);
                view.sprite = animationSet.AttackFrame(timeline);
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
            if (Math.Abs(motion.x) > 0.0001f)
                view.flipX = animationSet.SourceFacesRight ? motion.x < 0 : motion.x > 0;
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            SetClip(ActorClip.Walk); RenderClip();
        }

        private void Receive(int attacker, long attack, int power)
        {
            if (health == null) return;
            HitReceipt receipt = health.Receive(attacker, attack, power);
            if (!receipt.Accepted) return;
            LastAttackerId=attacker;
            // Ordinary hits carry damage and feedback, not hard crowd control.
            // Keep the committed swing and movement alive under multiple attackers.
            hurtRemaining=receipt.Killed?0:.12;
            if(receipt.Killed)
            {
                timeline.Cancel();attackTarget=null;clipTime=0;view.color=Color.white;
                SetClip(ActorClip.Death);RenderClip();
            }
            Damaged?.Invoke(this, receipt);
        }
        private void SetClip(ActorClip next) { if (clip != next) { clip = next; clipTime = 0; } }
        private void RenderClip()
        {
            view.sprite = animationSet.Frame(clip, clipTime);
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
        private static bool FinitePositive(float x) => x > 0 && !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool FinitePoint(Vector2 point) => !float.IsNaN(point.x) && !float.IsInfinity(point.x)
            && !float.IsNaN(point.y) && !float.IsInfinity(point.y);
    }
}
