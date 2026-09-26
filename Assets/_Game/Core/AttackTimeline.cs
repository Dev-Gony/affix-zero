using System;

namespace AffixZero.Core
{
    // A fresh C# implementation. No engine callbacks or file I/O.
    public sealed class AttackTimeline
    {
        public double ImpactTime { get; }
        public double Duration { get; }
        public double Elapsed { get; private set; }
        public long AttackId { get; private set; }
        public int TargetId { get; private set; }
        public bool IsRunning { get; private set; }
        private bool impactConsumed;

        public AttackTimeline(double impactTime, double duration)
        {
            if (!Finite(impactTime) || !Finite(duration) || impactTime < 0 || duration <= impactTime)
                throw new ArgumentOutOfRangeException(nameof(duration));
            ImpactTime = impactTime;
            Duration = duration;
        }

        public bool Begin(int targetId)
        {
            if (targetId <= 0) throw new ArgumentOutOfRangeException(nameof(targetId));
            if (IsRunning) return false;
            AttackId = checked(AttackId + 1);
            TargetId = targetId;
            Elapsed = 0;
            impactConsumed = false;
            IsRunning = true;
            return true;
        }

        // Return an impact once, even when a frame crosses the entire attack.
        // The locked target cannot silently switch to a newly selected enemy.
        public Impact Advance(double seconds, bool lockedTargetAlive, bool lockedTargetInRange)
        {
            if (!Finite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (!IsRunning || seconds == 0) return default;
            // Once impact has resolved, finish the visible recovery even if that hit
            // killed the target. A dead target before impact still cancels the swing.
            if (!lockedTargetAlive && !impactConsumed) { Cancel(); return default; }
            Elapsed = Math.Min(Duration, Elapsed + seconds);
            Impact result = default;
            if (!impactConsumed && Elapsed >= ImpactTime)
            {
                impactConsumed = true;
                if (lockedTargetInRange) result = new Impact(AttackId, TargetId);
            }
            if (Elapsed >= Duration) IsRunning = false;
            return result;
        }

        public void Cancel()
        {
            IsRunning = false;
            impactConsumed = true;
        }

        public int FrameAt(int frameCount, double framesPerSecond)
        {
            if (frameCount <= 0 || !Finite(framesPerSecond) || framesPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(frameCount));
            return (int)Math.Min(frameCount - 1, Math.Floor(Elapsed * framesPerSecond));
        }

        private static bool Finite(double x) => !double.IsNaN(x) && !double.IsInfinity(x);
    }

    public readonly struct Impact
    {
        public long AttackId { get; }
        public int TargetId { get; }
        public bool Occurred => AttackId > 0;
        public Impact(long attackId, int targetId) { AttackId = attackId; TargetId = targetId; }
    }
}
