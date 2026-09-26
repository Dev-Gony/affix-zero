using System;
using AffixZero.Core;

internal static class CombatHealthChecks
{
    public static void Run(Action<bool, string> check, Action<Action, string> throws)
    {
        throws(() => new CombatHealth(0, 0), "zero health rejected");
        throws(() => new CombatHealth(10, -1), "negative armor rejected");
        var hp = new CombatHealth(10, 2);
        check(hp.Current == 10 && !hp.IsDead, "health starts alive");
        HitReceipt hit = hp.Receive(1, 1, 5);
        check(hit.Accepted && hit.Damage == 3 && hp.Current == 7, "armor applied once");
        check(!hp.Receive(1, 1, 5).Accepted && hp.Current == 7, "duplicate hit rejected");
        hp.Receive(1, 3, 3);
        check(!hp.Receive(1, 2, 9).Accepted && hp.Current == 6, "late older hit rejected");
        check(hp.Receive(2, 1, 3).Accepted && hp.Current == 5, "same attack number from different actor accepted");
        check(hp.Receive(2, 2, 1).Damage == 1, "minimum positive damage");
        hit = hp.Receive(1, 4, 100);
        check(hit.Killed && hit.Damage == 4 && hp.DeathCount == 1, "overkill clamps to remaining hp");
        check(!hp.Receive(1, 5, 10).Accepted && hp.DeathCount == 1, "dead target cannot die twice");
        throws(() => hp.Receive(0, 1, 1), "invalid attacker rejected");
        throws(() => hp.Receive(1, 0, 1), "invalid attack id rejected");
        throws(() => hp.Receive(1, 1, 0), "zero damage request rejected");
        var paused = new CombatHealth(20, 0);
        var timeline = new AttackTimeline(0.2, 0.5);
        timeline.Begin(2);
        check(!timeline.Advance(0.1, true, true).Occurred && paused.Current == 20, "no damage before impact integration");
        Impact impact = timeline.Advance(0.1, true, true);
        if (impact.Occurred) paused.Receive(1, impact.AttackId, 7);
        check(paused.Current == 13, "timeline and health integrate at impact");
        check(!timeline.Advance(0.3, true, true).Occurred && paused.Current == 13, "recovery produces no extra damage");
    }
}
