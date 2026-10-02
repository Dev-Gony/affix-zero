using System;
using UnityEngine;

namespace AffixZero.Presentation
{
    public enum EliteEncounterStyle { None = 0, EmberBulwark = 1, GalleryStalker = 2, RitualReaver = 3 }

    public readonly struct EliteEncounterProfile
    {
        public readonly EliteEncounterStyle Style;
        public readonly string Name;
        public readonly int BaseHp, BaseDamage, Defense;
        public readonly float MoveSpeed, AttackSpeed, Reach;
        public readonly Color Color;

        public EliteEncounterProfile(EliteEncounterStyle style, string name, int hp, int damage, int defense,
            float moveSpeed, float attackSpeed, float reach, Color color)
        {
            if (style == EliteEncounterStyle.None || string.IsNullOrWhiteSpace(name) || hp <= 0 || damage <= 0 || defense < 0 ||
                !Finite(moveSpeed) || !Finite(attackSpeed) || !Finite(reach)) throw new ArgumentOutOfRangeException(nameof(style));
            Style = style; Name = name; BaseHp = hp; BaseDamage = damage; Defense = defense;
            MoveSpeed = moveSpeed; AttackSpeed = attackSpeed; Reach = reach; Color = color;
        }

        private static bool Finite(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    // Layout-specific elite identities. Values are original AFFIX tuning, not recovered commercial data.
    public static class EliteEncounterTuning
    {
        public static EliteEncounterProfile Get(DungeonLayoutId layout, bool guardian)
        {
            switch (layout)
            {
                case DungeonLayoutId.Bastion:
                    return guardian
                        ? new EliteEncounterProfile(EliteEncounterStyle.EmberBulwark, "Bastion Warden Elite", 138, 10, 5, 1.25f, .72f, 1.30f, new Color(1f, .42f, .12f, 1f))
                        : new EliteEncounterProfile(EliteEncounterStyle.EmberBulwark, "Ember Bulwark Elite", 82, 8, 3, 1.35f, .78f, 1.15f, new Color(1f, .48f, .16f, 1f));
                case DungeonLayoutId.Galleries:
                    return guardian
                        ? new EliteEncounterProfile(EliteEncounterStyle.GalleryStalker, "Gallery Huntsmaster Elite", 112, 8, 2, 2.35f, 1.50f, 1.00f, new Color(.28f, .72f, 1f, 1f))
                        : new EliteEncounterProfile(EliteEncounterStyle.GalleryStalker, "Gallery Stalker Elite", 68, 6, 1, 2.15f, 1.38f, .90f, new Color(.38f, .78f, 1f, 1f));
                default:
                    return guardian
                        ? new EliteEncounterProfile(EliteEncounterStyle.RitualReaver, "Crucible Hierophant Elite", 126, 9, 3, 1.70f, .92f, 1.65f, new Color(1f, .28f, .62f, 1f))
                        : new EliteEncounterProfile(EliteEncounterStyle.RitualReaver, "Ritual Reaver Elite", 74, 7, 1, 1.65f, 1.02f, 1.45f, new Color(1f, .36f, .68f, 1f));
            }
        }
    }

    // A small procedural floor sigil identifies elite archetype and guardian rank without new raster art.
    public sealed class EliteEncounterMarker : MonoBehaviour
    {
        private MeleeActor actor;
        private LineRenderer line;
        private Material material;
        private EliteEncounterProfile profile;
        private bool guardian, active;

        public bool Active => active;
        public bool IsGuardian => active && guardian;
        public EliteEncounterStyle Style => active ? profile.Style : EliteEncounterStyle.None;
        public int StyleMaskBit => active ? 1 << ((int)profile.Style - 1) : 0;
        public Color TelegraphColor => active ? profile.Color : new Color(1f, .18f, .12f, .85f);

        public void Configure(MeleeActor source, EliteEncounterProfile value, bool isGuardian)
        {
            actor = source != null ? source : throw new ArgumentNullException(nameof(source));
            profile = value; guardian = isGuardian; active = true;
            EnsureRenderer(); line.enabled = actor.isActiveAndEnabled && !actor.IsDead;
        }

        public void Clear()
        {
            active = guardian = false;
            if (line != null) line.enabled = false;
        }

        private void EnsureRenderer()
        {
            if (line != null) return;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Built-in sprite shader missing for elite marker.");
            material = new Material(shader);
            var host = new GameObject("Elite encounter sigil", typeof(LineRenderer));
            host.transform.SetParent(transform, false);
            line = host.GetComponent<LineRenderer>(); line.sharedMaterial = material; line.useWorldSpace = false;
            line.loop = true; line.sortingOrder = 1900; line.numCapVertices = 2; line.numCornerVertices = 2;
        }

        private void LateUpdate()
        {
            if (!active || actor == null || !actor.isActiveAndEnabled || actor.IsDead)
            { if (line != null) line.enabled = false; return; }
            EnsureRenderer(); line.enabled = true;
            int points = profile.Style == EliteEncounterStyle.EmberBulwark ? 4 : profile.Style == EliteEncounterStyle.GalleryStalker ? 6 : 20;
            line.positionCount = points;
            float radius = (guardian ? .86f : .62f) + Mathf.Sin(Time.time * (guardian ? 3.2f : 4.4f)) * .04f;
            float rotation = Time.time * (profile.Style == EliteEncounterStyle.GalleryStalker ? -1.4f : .7f);
            for (int i = 0; i < points; i++)
            {
                float angle = rotation + Mathf.PI * 2 * i / points;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * .48f + .08f, 0));
            }
            Color color = profile.Color; color.a = guardian ? .92f : .68f;
            line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = guardian ? .085f : .055f;
        }

        private void OnDestroy()
        { if (material != null) Destroy(material); }
    }
}
