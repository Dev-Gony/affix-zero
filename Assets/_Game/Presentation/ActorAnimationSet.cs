using System;
using UnityEngine;
using AffixZero.Core;

namespace AffixZero.Presentation
{
    public enum ActorClip { Idle, Walk, Attack, Hit, Death }
    public enum ActorFacing { Down, Left, Right, Up }

    [CreateAssetMenu(menuName = "AFFIX/Actor Animation Set")]
    public sealed class ActorAnimationSet : ScriptableObject
    {
        [SerializeField] private Sprite[] idle = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] walk = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] attack = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] hit = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] death = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] attackWeapon = Array.Empty<Sprite>();
        [SerializeField] private bool fourDirections = false;
        [SerializeField, Min(1)] private int idleFramesPerDirection = 1;
        [SerializeField, Min(2)] private int walkFramesPerDirection = 2;
        [SerializeField, Min(3)] private int attackFramesPerDirection = 3;
        [SerializeField, Min(1)] private int hitFramesPerDirection = 1;
        [SerializeField, Min(2)] private int deathFramesPerDirection = 2;
        [SerializeField, Min(1)] private float attackFps = 12;
        [SerializeField, Min(1)] private float movementFps = 10;
        [SerializeField, Min(1)] private float reactionFps = 10;
        [SerializeField, Min(1)] private float deathFps = 10;
        [SerializeField, Min(1)] private int impactFrame = 2;
        [SerializeField] private bool sourceFacesRight = true;
        [SerializeField] private string sourceLicenseRecord = "";

        public bool SourceFacesRight => sourceFacesRight;
        public bool UsesFourDirections => fourDirections;
        public string SourceLicenseRecord => sourceLicenseRecord;
        public Sprite ImpactSprite => ImpactSpriteFor(ActorFacing.Down);
        public double HitDuration => FramesPerDirection(ActorClip.Hit) / (double)reactionFps;
        public double AttackImpactTime => impactFrame / (double)attackFps;
        public double AttackDuration => FramesPerDirection(ActorClip.Attack) / (double)attackFps;
        public Sprite WeaponPreview => HasAttackWeapon ? attackWeapon[0] : null;
        public bool HasAttackWeapon => attackWeapon != null && attackWeapon.Length > 0;

        public string ValidateSet()
        {
            string problem = fourDirections
                ? CheckDirectional(idle, idleFramesPerDirection, 1, "idle")
                    ?? CheckDirectional(walk, walkFramesPerDirection, 2, "walk")
                    ?? CheckDirectional(attack, attackFramesPerDirection, 3, "attack")
                    ?? CheckDirectional(hit, hitFramesPerDirection, 1, "hit")
                    ?? CheckDirectional(death, deathFramesPerDirection, 2, "death")
                : Check(idle, 1, "idle") ?? Check(walk, 2, "walk")
                    ?? Check(attack, 3, "attack") ?? Check(hit, 1, "hit") ?? Check(death, 2, "death");
            if (problem != null) return problem;
            if (HasAttackWeapon)
            {
                if (attackWeapon.Length != attack.Length) return "Weapon frames must match the body attack timeline.";
                problem = Check(attackWeapon, 3, "attack weapon");
                if (problem != null) return problem;
            }
            if (!PositiveFinite(attackFps) || !PositiveFinite(movementFps) || !PositiveFinite(reactionFps) || !PositiveFinite(deathFps))
                return "Animation FPS must be positive and finite.";
            if (impactFrame <= 0 || impactFrame >= FramesPerDirection(ActorClip.Attack) - 1)
                return "Impact needs both anticipation and recovery frames.";
            if (string.IsNullOrWhiteSpace(sourceLicenseRecord)) return "Asset provenance/license record missing.";
            return null;
        }

        public AttackTimeline CreateTimeline()
        {
            string problem = ValidateSet();
            if (problem != null) throw new InvalidOperationException(problem);
            return new AttackTimeline(impactFrame / (double)attackFps,
                FramesPerDirection(ActorClip.Attack) / (double)attackFps);
        }

        public Sprite AttackFrame(AttackTimeline timeline) => AttackFrame(timeline, ActorFacing.Down);

        public Sprite AttackFrame(AttackTimeline timeline, ActorFacing facing)
        {
            if (timeline == null) throw new ArgumentNullException(nameof(timeline));
            int count = FramesPerDirection(ActorClip.Attack);
            return attack[DirectionOffset(facing, count) + timeline.FrameAt(count, attackFps)];
        }

        public Sprite ImpactSpriteFor(ActorFacing facing)
        {
            int count = FramesPerDirection(ActorClip.Attack);
            return attack[DirectionOffset(facing, count) + impactFrame];
        }

        public Sprite WeaponFrame(AttackTimeline timeline, bool showImpact)
        {
            if (!HasAttackWeapon) return null;
            return attackWeapon[showImpact ? impactFrame : timeline.FrameAt(attackWeapon.Length, attackFps)];
        }

        public Sprite Frame(ActorClip clip, double elapsed) => Frame(clip, elapsed, ActorFacing.Down);

        public Sprite Frame(ActorClip clip, double elapsed, ActorFacing facing)
        {
            Sprite[] frames = clip == ActorClip.Idle ? idle : clip == ActorClip.Walk ? walk
                : clip == ActorClip.Hit ? hit : clip == ActorClip.Death ? death : attack;
            double fps = clip == ActorClip.Attack ? attackFps
                : clip == ActorClip.Death ? deathFps : clip == ActorClip.Hit ? reactionFps : movementFps;
            bool loop = clip == ActorClip.Idle || clip == ActorClip.Walk;
            double frame = Math.Floor(Math.Max(0, elapsed) * fps);
            int count = FramesPerDirection(clip);
            int index = loop ? (int)(frame % count) : (int)Math.Min(count - 1, frame);
            return frames[DirectionOffset(facing, count) + index];
        }

        private int FramesPerDirection(ActorClip clip)
        {
            if (!fourDirections)
                return clip == ActorClip.Idle ? idle.Length : clip == ActorClip.Walk ? walk.Length
                    : clip == ActorClip.Hit ? hit.Length : clip == ActorClip.Death ? death.Length : attack.Length;
            return clip == ActorClip.Idle ? idleFramesPerDirection : clip == ActorClip.Walk ? walkFramesPerDirection
                : clip == ActorClip.Hit ? hitFramesPerDirection : clip == ActorClip.Death ? deathFramesPerDirection
                : attackFramesPerDirection;
        }

        private int DirectionOffset(ActorFacing facing, int count) => fourDirections ? (int)facing * count : 0;

        private static bool PositiveFinite(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
        private static string Check(Sprite[] frames, int minimum, string clip)
        {
            if (frames == null || frames.Length < minimum) return clip + ": insufficient frames.";
            foreach (Sprite sprite in frames)
                if (sprite == null || sprite.texture == null) return clip + ": unloaded sprite/texture.";
            if (minimum > 1)
            {
                bool distinct = false;
                for (int i = 1; i < frames.Length; i++)
                    if (frames[i].texture != frames[0].texture || frames[i].rect != frames[0].rect)
                        distinct = true;
                if (!distinct) return clip + ": repeated still image is not an animation.";
            }
            return null; // Pixel contents, pivot and license still require human inspection.
        }

        private static string CheckDirectional(Sprite[] frames, int perDirection, int minimum, string clip)
        {
            if (perDirection < minimum) return clip + ": insufficient frames per direction.";
            if (frames == null || frames.Length != perDirection * 4)
                return clip + ": expected four complete directional strips.";
            for (int direction = 0; direction < 4; direction++)
            {
                Sprite[] strip = new Sprite[perDirection];
                Array.Copy(frames, direction * perDirection, strip, 0, perDirection);
                string problem = Check(strip, minimum, clip + " direction " + direction);
                if (problem != null) return problem;
            }
            return null;
        }
    }
}
