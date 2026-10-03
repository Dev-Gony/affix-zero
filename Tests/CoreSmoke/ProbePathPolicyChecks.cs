using System;
using AffixZero.Core;

internal static class ProbePathPolicyChecks
{
    public static void Run(Action<bool, string> check)
    {
        string production = @"C:\Users\Player\AppData\LocalLow\Dev-Gony\AFFIX ZERO";
        string isolated = @"D:\github\affix-zero-original-temple\Build\TempProfiles\sentinel";
        string[] flags =
        {
            "-affixSaveTest", "-affixNaturalProgressionTest", "-affixPhysicalInputTest",
            "-affixSpeedUiStressTest", "-affixCombatExperienceTest", "-affixUiReferenceTest",
            "-affixAutoHuntTest", "-affixAutoHuntSafetyTest", "-affixSmokeTest"
        };
        foreach (string flag in flags)
        {
            string[] args = { "game.exe", flag, "-affixSaveDir", isolated };
            check(ProbePathPolicy.IsProbe(args) &&
                string.Equals(ProbePathPolicy.RequireSaveDirectory(args, production), isolated, StringComparison.OrdinalIgnoreCase),
                "probe path policy recognizes and isolates " + flag);
        }
        check(!ProbePathPolicy.IsProbe(new[] { "game.exe" }) &&
            ProbePathPolicy.RequireSaveDirectory(new[] { "game.exe" }, production) == null,
            "ordinary player launch retains production persistence policy");
        check(ProbePathPolicy.UsesWritableProfile(new[] { "game.exe", "-affixSpeedUiStressTest" }) &&
            !ProbePathPolicy.UsesWritableProfile(new[] { "game.exe", "-affixUiReferenceTest" }),
            "only disk probes open their isolated profile store");
        Reject(() => ProbePathPolicy.RequireSaveDirectory(new[] { "game.exe", "-affixSmokeTest" }, production), check,
            "probe without explicit save sandbox fails closed");
        Reject(() => ProbePathPolicy.RequireSaveDirectory(new[] { "game.exe", "-affixSmokeTest", "-affixSaveDir", production }, production), check,
            "probe cannot select production profile path");
        Reject(() => ProbePathPolicy.RequireSaveDirectory(new[] { "game.exe", "-affixSmokeTest", "-affixSaveDir", @"C:\temp\probe" }, production), check,
            "probe cannot select a non-D drive");
        Reject(() => ProbePathPolicy.RequireSaveDirectory(new[] { "game.exe", "-affixSmokeTest", "-affixSaveDir", @"D:\" }, production), check,
            "probe cannot select the D drive root");
        Reject(() => ProbePathPolicy.RequireSaveDirectory(new[] { "game.exe", "-affixSmokeTest", "-affixSaveDir", isolated, "-affixSaveDir", isolated + "-two" }, production), check,
            "duplicate save sandbox arguments fail closed");
        check(ProbePathPolicy.IsSameOrChild(isolated + @"\child", isolated) &&
            !ProbePathPolicy.IsSameOrChild(@"D:\other", isolated),
            "sandbox ancestry check rejects sibling paths");
    }

    private static void Reject(Action operation, Action<bool, string> check, string name)
    {
        bool rejected = false;
        try { operation(); }
        catch (ArgumentException) { rejected = true; }
        check(rejected, name);
    }
}
