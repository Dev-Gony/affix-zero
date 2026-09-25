using System;
using UnityEngine;
using AffixZero.Core;

namespace AffixZero.Presentation
{
    [CreateAssetMenu(menuName = "AFFIX/Actor Animation Set")]
    public sealed class ActorAnimationSet : ScriptableObject
    {
        [SerializeField] private Sprite[] idle = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] walk = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] attack = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] hit = Array.Empty<Sprite>();
        [SerializeField] private Sprite[] death = Array.Empty<Sprite>();
        [SerializeField, Min(1)] private float attackFps = 12;
        [SerializeField, Min(1)] private int impactFrame = 2;
        [SerializeField] private string sourceLicenseRecord = "";

        public string ValidateSet()
        {
            string problem = Check(idle, 1, "idle") ?? Check(walk, 2, "walk")
                ?? Check(attack, 3, "attack") ?? Check(hit, 1, "hit") ?? Check(death, 2, "death");
            if (problem != null) return problem;
            if (float.IsNaN(attackFps) || float.IsInfinity(attackFps) || attackFps <= 0)
                return "Attack FPS must be positive and finite.";
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
            string problem = ValidateSet();
            if (problem != null) throw new InvalidOperationException(problem);
            return attack[timeline.FrameAt(attack.Length, attackFps)];
        }

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
            return null;
        }
    }
}
