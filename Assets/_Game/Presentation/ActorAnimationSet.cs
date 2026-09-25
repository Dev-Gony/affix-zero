using System;
using UnityEngine;
using AffixZero.Core;

namespace AffixZero.Presentation
{
    public enum ActorClip { Idle, Walk, Attack, Hit, Death }

    [CreateAssetMenu(menuName = "AFFIX/Actor Animation Set")]
    public sealed class ActorAnimationSet : ScriptableObject
    {
        [SerializeField] private Sprite[] idle = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] walk = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] attack = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] hit = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] death = Array.Empty<Sprite>();
        [SerializeField, Min(1)] private float attackFps = 12;
        [SerializeField, Min(1)] private float movementFps = 10;
        [SerializeField, Min(1)] private float reactionFps = 10;
        [SerializeField, Min(1)] private int impactFrame = 2;
        [SerializeField] private bool sourceFacesRight = true;
        [SerializeField] private string sourceLicenseRecord = "";

        public bool SourceFacesRight => sourceFacesRight;
        public string SourceLicenseRecord => sourceLicenseRecord;
        public Sprite ImpactSprite => attack[impactFrame];
        public double HitDuration => hit.Length / (double)reactionFps;

        public string ValidateSet()
        {
            string problem = Check(idle, 1, "idle") ?? Check(walk, 2, "walk")
                ?? Check(attack, 3, "attack") ?? Check(hit, 1, "hit") ?? Check(death, 2, "death");
            if (problem != null) return problem;
            if (!PositiveFinite(attackFps) || !PositiveFinite(movementFps) || !PositiveFinite(reactionFps))
                return "Animation FPS must be positive and finite.";
            if (impactFrame <= 0 || impactFrame >= attack.Length - 1)
                return "Impact needs both anticipation and recovery frames.";
            if (string.IsNullOrWhiteSpace(sourceLicenseRecord)) return "Asset provenance/license record missing.";
            return null;
        }

        public AttackTimeline CreateTimeline()
        {
            string problem = ValidateSet();
            if (problem != null) throw new InvalidOperationException(problem);
            return new AttackTimeline(impactFrame / (double)attackFps, attack.Length / (double)attackFps);
        }

        public Sprite AttackFrame(AttackTimeline timeline)
        {
            if (timeline == null) throw new ArgumentNullException(nameof(timeline));
            return attack[timeline.FrameAt(attack.Length, attackFps)];
        }

        public Sprite Frame(ActorClip clip, double elapsed)
        {
            Sprite[] frames = clip == ActorClip.Idle ? idle : clip == ActorClip.Walk ? walk
                : clip == ActorClip.Hit ? hit : clip == ActorClip.Death ? death : attack;
            double fps = clip == ActorClip.Attack ? attackFps
                : (clip == ActorClip.Hit || clip == ActorClip.Death) ? reactionFps : movementFps;
            bool loop = clip == ActorClip.Idle || clip == ActorClip.Walk;
            double frame = Math.Floor(Math.Max(0, elapsed) * fps);
            int index = loop ? (int)(frame % frames.Length) : (int)Math.Min(frames.Length - 1, frame);
            return frames[index];
        }

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
    }
}
