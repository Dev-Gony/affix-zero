using System;
using AffixZero.Core;

internal static class EncounterRewardsChecks
{
    public static void Run(Action<bool, string> check, Action<Action, string> throws)
    {
        var rewards = new EncounterRewards("encounter-a");
        check(rewards.TryCollect("encounter-a", 2, 1, 25, 8), "valid death receipt credits reward");
        check(rewards.Experience == 25 && rewards.Gold == 8 && rewards.CollectionCount == 1,
            "collected reward totals match receipt");
        check(!rewards.TryCollect("encounter-a", 2, 1, 250, 80) && rewards.Experience == 25 && rewards.Gold == 8,
            "duplicate death cannot mint additional reward");
        check(!rewards.TryCollect("encounter-old", 3, 1, 25, 8) && rewards.CollectionCount == 1,
            "late reward from old encounter rejected");
        check(rewards.TryCollect("encounter-a", 3, 1, 5, 2) && rewards.Experience == 30 && rewards.Gold == 10,
            "separate enemy death has independent reward identity");
        var restarted = new EncounterRewards("encounter-b");
        check(restarted.Experience == 0 && restarted.Gold == 0 && restarted.CollectionCount == 0,
            "restart resets transient reward balance");
        check(!restarted.TryCollect("encounter-a", 2, 1, 25, 8), "old death cannot credit restarted encounter");
        check(restarted.TryCollect("encounter-b", 2, 1, 25, 8), "new encounter can collect same actor identity once");
        throws(() => rewards.TryCollect("encounter-a", 0, 1, 25, 8), "invalid reward actor rejected");
        throws(() => rewards.TryCollect("encounter-a", 2, 0, 25, 8), "invalid death identity rejected");
        throws(() => rewards.TryCollect("encounter-a", 2, 2, -1, 8), "negative reward rejected");
        var saturated = new EncounterRewards("overflow");
        saturated.TryCollect("overflow", 2, 1, 1, int.MaxValue);
        bool overflow = false;
        try { saturated.TryCollect("overflow", 3, 1, 2, 1); } catch (OverflowException) { overflow = true; }
        check(overflow && saturated.Experience == 1 && saturated.Gold == int.MaxValue && saturated.CollectionCount == 1,
            "overflow cannot partially credit or consume receipt");
        check(saturated.TryCollect("overflow", 3, 1, 2, 0) && saturated.Experience == 3,
            "unconsumed receipt remains collectable after failed transaction");
    }
}
