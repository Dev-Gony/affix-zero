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
        private AttackTimeline timeline;
        private CombatHealth health;
        private double clipTime;
        private double hurtRemaining;
        private ActorClip clip = ActorClip.Idle;

        public int Hp => health == null ? maximumHp : health.Current;
        public int MaxHp => maximumHp;
        public bool IsDead => health != null && health.IsDead;
        public event Action<MeleeActor, HitReceipt> Damaged;

        public void Configure(ActorAnimationSet set, int id, int hp, int attackDamage)
        { animationSet = set; actorId = id; maximumHp = hp; damage = attackDamage; }
        public void SetTarget(MeleeActor other) { target = other; }

        private void Start()
        {
            string problem = animationSet == null ? "Animation set is missing." : animationSet.ValidateSet();
            if (problem != null || actorId <= 0 || maximumHp <= 0 || damage <= 0 || defense < 0
                || !FinitePositive(moveSpeed) || !FinitePositive(reach))
            { Debug.LogError("AFFIX actor setup failed: " + (problem ?? "Invalid actor values."), this); enabled = false; return; }
            view = GetComponent<SpriteRenderer>();
            timeline = animationSet.CreateTimeline();
            health = new CombatHealth(maximumHp, defense);
            view.sprite = animationSet.Frame(ActorClip.Idle, 0);
        }

        private void Update()
        {
            double delta = Time.deltaTime;
            if (delta <= 0 || health == null) return;
            clipTime += delta;
            view.sortingOrder = -(int)Math.Round(transform.position.y * 100);
            if (IsDead) { SetClip(ActorClip.Death); RenderClip(); return; }
            if (hurtRemaining > 0)
            { hurtRemaining = Math.Max(0, hurtRemaining - delta); SetClip(ActorClip.Hit); RenderClip(); return; }
            if (target == null || target.health == null || target.IsDead)
            { timeline.Cancel(); SetClip(ActorClip.Idle); RenderClip(); return; }

            Vector2 toTarget = target.transform.position - transform.position;
            // Preserve attack direction until recovery finishes.
            if (!timeline.IsRunning && Math.Abs(toTarget.x) > 0.01f)
                view.flipX = animationSet.SourceFacesRight ? toTarget.x < 0 : toTarget.x > 0;
            if (timeline.IsRunning)
            {
                bool sameTarget = target.actorId == timeline.TargetId;
                Impact impact = timeline.Advance(delta, sameTarget && !target.IsDead, toTarget.magnitude <= reach + 0.05f);
                SetClip(ActorClip.Attack);
                // A skipped render frame still displays the impact pose when applying its hit.
                view.sprite = impact.Occurred ? animationSet.ImpactSprite : animationSet.AttackFrame(timeline);
                if (impact.Occurred) target.Receive(actorId, impact.AttackId, damage);
                return;
            }
            if (toTarget.magnitude > reach)
            {
                Vector2 destination = (Vector2)target.transform.position - toTarget.normalized * reach;
                transform.position = Vector2.MoveTowards(transform.position, destination, moveSpeed * (float)delta);
                SetClip(ActorClip.Walk); RenderClip();
            }
            else
            {
                timeline.Begin(target.actorId);
                SetClip(ActorClip.Attack);
                view.sprite = animationSet.AttackFrame(timeline);
            }
        }

        private void Receive(int attacker, long attack, int power)
        {
            if (health == null) return;
            HitReceipt receipt = health.Receive(attacker, attack, power);
            if (!receipt.Accepted) return;
            timeline.Cancel();
            hurtRemaining = receipt.Killed ? 0 : animationSet.HitDuration;
            clipTime = 0;
            SetClip(receipt.Killed ? ActorClip.Death : ActorClip.Hit);
            RenderClip();
            Damaged?.Invoke(this, receipt);
        }
        private void SetClip(ActorClip next) { if (clip != next) { clip = next; clipTime = 0; } }
        private void RenderClip() { view.sprite = animationSet.Frame(clip, clipTime); }
        private static bool FinitePositive(float x) => x > 0 && !float.IsNaN(x) && !float.IsInfinity(x);
    }
}
