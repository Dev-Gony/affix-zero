using System;
using UnityEngine;

namespace AffixZero.Presentation
{
    public readonly struct BossEncounterProfile
    {
        public readonly string Name;
        public readonly int BaseHp, BaseDamage, Defense;
        public readonly float MoveSpeed, AttackSpeed, Reach, PulseRadius;
        public readonly Color Color;

        public BossEncounterProfile(string name, int hp, int damage, int defense, float moveSpeed,
            float attackSpeed, float reach, float pulseRadius, Color color)
        {
            if (string.IsNullOrWhiteSpace(name) || hp <= 0 || damage <= 0 || defense < 0 ||
                !Finite(moveSpeed) || !Finite(attackSpeed) || !Finite(reach) || !Finite(pulseRadius))
                throw new ArgumentOutOfRangeException(nameof(name));
            Name = name; BaseHp = hp; BaseDamage = damage; Defense = defense;
            MoveSpeed = moveSpeed; AttackSpeed = attackSpeed; Reach = reach; PulseRadius = pulseRadius; Color = color;
        }

        private static bool Finite(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    public static class BossEncounterTuning
    {
        public static BossEncounterProfile Get(DungeonLayoutId layout)
        {
            switch (layout)
            {
                case DungeonLayoutId.Bastion:
                    return new BossEncounterProfile("ASHEN TYRANT", 320, 7, 3, 1.28f, .82f, 1.42f, 3.15f,
                        new Color(1f, .28f, .08f, 1f));
                case DungeonLayoutId.Galleries:
                    return new BossEncounterProfile("DROWNED ARCHON", 300, 6, 2, 1.52f, .94f, 1.32f, 3.35f,
                        new Color(.22f, .68f, 1f, 1f));
                default:
                    return new BossEncounterProfile("ECLIPSE SOVEREIGN", 340, 7, 3, 1.36f, .86f, 1.52f, 3.55f,
                        new Color(1f, .18f, .62f, 1f));
            }
        }
    }

    // A large double-ring seal and crown silhouette make the route-end boss distinct from elites.
    // This is presentation-only geometry; it creates no collider or world-space health display.
    public sealed class BossEncounterMarker : MonoBehaviour
    {
        private MeleeActor actor;
        private BossEncounterProfile profile;
        private LineRenderer seal, crown;
        private Material material;
        private bool active;

        public bool Active => active;
        public string DisplayName => active ? profile.Name : string.Empty;
        public Color AccentColor => active ? profile.Color : Color.white;

        public void Configure(MeleeActor source, BossEncounterProfile value)
        {
            actor = source != null ? source : throw new ArgumentNullException(nameof(source));
            profile = value; active = true; transform.localScale = Vector3.one * 1.48f;
            EnsureRenderers(); seal.enabled = crown.enabled = actor.isActiveAndEnabled && !actor.IsDead;
        }

        public void Clear()
        {
            active = false; transform.localScale = Vector3.one;
            if (seal != null) seal.enabled = false;
            if (crown != null) crown.enabled = false;
        }

        private void LateUpdate()
        {
            if (!active || actor == null || !actor.isActiveAndEnabled || actor.IsDead)
            { if (seal != null) seal.enabled = false; if (crown != null) crown.enabled = false; return; }
            EnsureRenderers(); seal.enabled = crown.enabled = true;
            int points = 24; seal.positionCount = points; float turn = Time.time * .42f;
            for (int i = 0; i < points; i++)
            {
                float angle = turn + Mathf.PI * 2 * i / points;
                float radius = (i & 1) == 0 ? 1.03f : .83f;
                seal.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * .48f + .08f, 0));
            }
            Color pulse = profile.Color; pulse.a = .72f + .18f * Mathf.Abs(Mathf.Sin(Time.time * 3.6f));
            seal.startColor = seal.endColor = pulse;
            crown.positionCount = 5;
            crown.SetPosition(0, new Vector3(-.48f, .74f)); crown.SetPosition(1, new Vector3(-.27f, 1.08f));
            crown.SetPosition(2, new Vector3(0, .82f)); crown.SetPosition(3, new Vector3(.27f, 1.08f));
            crown.SetPosition(4, new Vector3(.48f, .74f));
            crown.startColor = crown.endColor = Color.Lerp(profile.Color, Color.white, .35f);
        }

        private void EnsureRenderers()
        {
            if (seal != null) return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Built-in sprite shader missing for boss marker.");
            material = new Material(shader);
            seal = CreateLine("Boss double-ring seal", true, 2000, .075f);
            crown = CreateLine("Boss crown", false, 2001, .10f);
        }

        private LineRenderer CreateLine(string name, bool loop, int order, float width)
        {
            var host = new GameObject(name, typeof(LineRenderer)); host.transform.SetParent(transform, false);
            var line = host.GetComponent<LineRenderer>(); line.sharedMaterial = material; line.useWorldSpace = false;
            line.loop = loop; line.sortingOrder = order; line.numCapVertices = 3; line.numCornerVertices = 3;
            line.startWidth = line.endWidth = width; return line;
        }

        private void OnDestroy() { if (material != null) Destroy(material); }
    }

    // The boss periodically locks its melee swing and announces a large ruin pulse. The director
    // consumes the same radius to move the automatic hunter out before this component resolves damage.
    [DefaultExecutionOrder(-20)]
    public sealed class BossAttackPattern : MonoBehaviour
    {
        private MeleeActor actor, hero;
        private BossEncounterProfile profile;
        private LineRenderer outer, inner, spokes;
        private Material material;
        private bool active, telegraphing;
        private float cooldown, telegraph;
        private Vector2 origin;

        public bool Active => active;
        public bool IsTelegraphing => active && telegraphing;
        public float DangerRadius => active ? profile.PulseRadius : 0;
        public Vector2 DangerOrigin => origin;
        public int TelegraphCount { get; private set; }
        public int CastCount { get; private set; }
        public int HitCount { get; private set; }
        public event Action<bool> Resolved;

        public void Configure(MeleeActor source, MeleeActor target, BossEncounterProfile value)
        {
            actor = source != null ? source : throw new ArgumentNullException(nameof(source));
            hero = target != null ? target : throw new ArgumentNullException(nameof(target));
            profile = value; active = true; telegraphing = false; cooldown = 2.2f;
            actor.SetPatternLocked(false); EnsureRenderers(); SetVisible(false);
        }

        public void Clear()
        {
            active = telegraphing = false;
            if (actor != null) actor.SetPatternLocked(false);
            SetVisible(false);
        }

        private void Update()
        {
            if (!active || actor == null || hero == null || actor.IsDead || hero.IsDead || Time.deltaTime <= 0)
            { SetVisible(false); return; }
            if (telegraphing)
            {
                telegraph -= Time.deltaTime; DrawTelegraph();
                if (telegraph <= 0) Resolve();
                return;
            }
            cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
            if (cooldown > 0 || actor.CurrentTarget != hero || actor.IsAttacking) return;
            Vector2 delta = (Vector2)hero.transform.position - (Vector2)actor.transform.position;
            if (delta.sqrMagnitude <= 4.8f * 4.8f && actor.HasLineOfSight(hero.transform.position)) Begin();
        }

        private void Begin()
        {
            telegraphing = true; telegraph = .95f; origin = actor.transform.position; TelegraphCount++;
            actor.SetPatternLocked(true); SetVisible(true); DrawTelegraph();
        }

        private void Resolve()
        {
            telegraphing = false; actor.SetPatternLocked(false); SetVisible(false); CastCount++;
            bool hit = Vector2.Distance(origin, hero.transform.position) <= profile.PulseRadius && actor.HasLineOfSight(hero.transform.position) &&
                actor.ResolvePatternHit(hero, Mathf.Max(1, Mathf.RoundToInt(actor.Damage * .82f)), profile.PulseRadius, true).Accepted;
            if (hit) HitCount++;
            Resolved?.Invoke(hit); cooldown = 3.1f;
        }

        private void DrawTelegraph()
        {
            EnsureRenderers(); SetVisible(true);
            float progress = 1f - Mathf.Clamp01(telegraph / .95f);
            DrawCircle(outer, profile.PulseRadius, 48);
            DrawCircle(inner, Mathf.Lerp(.35f, profile.PulseRadius, progress), 40);
            spokes.positionCount = 10;
            for (int i = 0; i < 5; i++)
            {
                float angle = Time.time * .75f + Mathf.PI * 2 * i / 5;
                Vector2 edge = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * profile.PulseRadius;
                spokes.SetPosition(i * 2, origin); spokes.SetPosition(i * 2 + 1, edge);
            }
            Color color = profile.Color; color.a = .48f + .42f * progress;
            outer.startColor = outer.endColor = color; spokes.startColor = spokes.endColor = color;
            Color hot = Color.Lerp(profile.Color, Color.white, progress); hot.a = .78f;
            inner.startColor = inner.endColor = hot;
        }

        private void DrawCircle(LineRenderer line, float radius, int points)
        {
            line.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float angle = Mathf.PI * 2 * i / points;
                line.SetPosition(i, origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        private void EnsureRenderers()
        {
            if (outer != null) return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Built-in sprite shader missing for boss telegraph.");
            material = new Material(shader);
            outer = CreateLine("Boss ruin pulse boundary", true, .12f);
            inner = CreateLine("Boss ruin pulse countdown", true, .09f);
            spokes = CreateLine("Boss ruin pulse spokes", false, .055f);
        }

        private LineRenderer CreateLine(string name, bool loop, float width)
        {
            var host = new GameObject(name, typeof(LineRenderer)); host.transform.SetParent(transform, false);
            var line = host.GetComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.loop = loop; line.sortingOrder = 1998;
            line.numCapVertices = 3; line.numCornerVertices = 3; line.startWidth = line.endWidth = width;
            return line;
        }

        private void SetVisible(bool value)
        { if (outer != null) outer.enabled = value; if (inner != null) inner.enabled = value; if (spokes != null) spokes.enabled = value; }
        private void OnDisable() { if (actor != null) actor.SetPatternLocked(false); telegraphing = false; SetVisible(false); }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
