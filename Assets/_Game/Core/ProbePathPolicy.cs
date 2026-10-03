using System;
using System.IO;

namespace AffixZero.Core
{
    // Fail-closed policy shared by every standalone verification probe.
    public static class ProbePathPolicy
    {
        private static readonly string[] ProbeFlags =
        {
            "-affixSaveTest", "-affixNaturalProgressionTest", "-affixPhysicalInputTest",
            "-affixSpeedUiStressTest", "-affixCombatExperienceTest", "-affixUiReferenceTest",
            "-affixAutoHuntTest", "-affixAutoHuntSafetyTest", "-affixSmokeTest"
        };

        public static bool IsProbe(string[] args)
        {
            if (args == null) return false;
            foreach (string flag in ProbeFlags)
                if (Array.IndexOf(args, flag) >= 0) return true;
            return false;
        }

        public static bool UsesWritableProfile(string[] args) =>
            Has(args, "-affixSaveTest") || Has(args, "-affixNaturalProgressionTest") ||
            Has(args, "-affixPhysicalInputTest") || Has(args, "-affixSpeedUiStressTest");

        public static string RequireSaveDirectory(string[] args, string productionDirectory)
        {
            if (!IsProbe(args)) return null;
            string directory = RequireDDriveDirectory(args, "-affixSaveDir");
            string production = Normalize(productionDirectory);
            if (!string.IsNullOrEmpty(production) && IsSameOrChild(directory, production))
                throw new ArgumentException("Probe save directory cannot be the player's save directory or its child.");
            return directory;
        }

        public static string RequireDDriveDirectory(string[] args, string key)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            int found = -1;
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], key, StringComparison.Ordinal)) continue;
                if (found >= 0) throw new ArgumentException("Duplicate " + key + " is not allowed.");
                found = i;
            }
            if (found < 0 || found + 1 >= args.Length || string.IsNullOrWhiteSpace(args[found + 1]))
                throw new ArgumentException("Probe verification requires an explicit " + key + ".");
            if (!Path.IsPathRooted(args[found + 1]))
                throw new ArgumentException(key + " requires an absolute path.");
            string directory = Normalize(args[found + 1]);
            string root = Path.GetPathRoot(directory);
            if (!string.Equals(root, "D:\\", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(key + " must be an isolated D: path.");
            if (string.Equals(directory, root.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(key + " cannot target the D: root.");
            return directory;
        }

        public static bool IsSameOrChild(string candidate, string parent)
        {
            string child = Normalize(candidate);
            string root = Normalize(parent);
            if (string.IsNullOrEmpty(child) || string.IsNullOrEmpty(root)) return false;
            if (string.Equals(child, root, StringComparison.OrdinalIgnoreCase)) return true;
            return child.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Has(string[] args, string flag) => args != null && Array.IndexOf(args, flag) >= 0;
        private static string Normalize(string path) => string.IsNullOrWhiteSpace(path)
            ? ""
            : Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
