using System;
using AffixZero.Core;

internal static class Program
{
    private static int passed;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        passed++;
        Console.WriteLine("PASS " + name);
    }
    private static void Throws(Action operation, string name)
    {
        bool thrown = false;
        try { operation(); } catch (ArgumentOutOfRangeException) { thrown = true; }
        Check(thrown, name);
    }
    private static int Main()
    {
        try
        {
            Throws(() => new AttackTimeline(-1, 1), "negative impact rejected");
            Throws(() => new AttackTimeline(1, 1), "recovery must exist");
            Throws(() => new AttackTimeline(double.NaN, 1), "NaN rejected");
            var attack = new AttackTimeline(0.25, 0.75);
            Check(attack.Begin(7), "attack starts");
            Check(!attack.Begin(8) && attack.TargetId == 7, "target locked during attack");
            Check(!attack.Advance(0.125, true, true).Occurred, "no anticipation damage");
            Check(attack.FrameAt(6, 8) == 1, "visual frame shares timeline");
            Impact impact = attack.Advance(0.125, true, true);
            Check(impact.Occurred && impact.TargetId == 7 && impact.AttackId == 1, "impact at correct time");
            Check(attack.FrameAt(6, 8) == 2, "impact matches frame two");
            Check(!attack.Advance(0.125, true, true).Occurred, "no duplicate hit");
            attack.Advance(1, true, true);
            Check(!attack.IsRunning, "attack recovery completes");
            attack.Begin(8);
            Check(!attack.Advance(0.25, true, false).Occurred, "out-of-range impact misses");
            Check(!attack.Advance(0.1, true, true).Occurred, "miss does not cause late damage");
            attack.Cancel();
            Check(!attack.Advance(1, true, true).Occurred, "cancel prevents ghost hit");
            attack.Begin(9);
            Check(!attack.Advance(0.3, false, true).Occurred && !attack.IsRunning, "dead target cancels");
            attack.Begin(10);
            Check(!attack.Advance(0, true, true).Occurred, "zero delta is a pause");
            Check(attack.Advance(10, true, true).Occurred && !attack.IsRunning, "large frame resolves once");
            Check(!attack.Advance(10, true, true).Occurred, "completed attack stays completed");
            Throws(() => attack.Advance(-0.1, true, true), "negative delta rejected");
            Throws(() => attack.Advance(double.PositiveInfinity, true, true), "infinite delta rejected");
            Throws(() => attack.Begin(0), "invalid target rejected");
            Throws(() => attack.FrameAt(0, 8), "empty frame set rejected");
            Check(attack.FrameAt(6, 8) == 5, "last frame clamped");
            Console.WriteLine("CORE_SMOKE_PASSED checks=" + passed + " scope=pure-CSharp-not-Unity-editor");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine("CORE_SMOKE_FAILED " + error);
            return 1;
        }
    }
}
