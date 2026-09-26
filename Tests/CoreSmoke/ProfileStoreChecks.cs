using System;
using System.IO;
using AffixZero.Core;

internal static class ProfileStoreChecks
{
    // Retain uniquely named fixtures; never recursively delete an OS temp folder.
    public static void Run(Action<bool, string> check)
    {
        string root = Path.Combine(Path.GetTempPath(), "AffixProfileStoreChecks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Console.WriteLine("PROFILE_STORE_FIXTURES " + root);
        string normal = Path.Combine(root, "normal.json");
        using (var store = new AtomicProfileStore(normal, Valid))
        {
            check(store.Load(out string notice) == null && notice == "", "missing profile is new without recovery notice");
            store.Save("v1|first");
            check(File.ReadAllText(normal) == "v1|first" && !File.Exists(normal + ".bak"), "first atomic save creates primary only");
            store.Save("v1|second");
            check(File.ReadAllText(normal) == "v1|second" && File.ReadAllText(normal + ".bak") == "v1|first", "replacement keeps previous valid generation");
            Expect<IOException>(() => store.Save("corrupt"), check, "invalid payload cannot overwrite primary");
            Expect<NotSupportedException>(() => store.Save("v2|future"), check, "unsupported save payload is not rewritten");
            check(File.ReadAllText(normal) == "v1|second" && File.ReadAllText(normal + ".bak") == "v1|first", "rejected payloads preserve both generations");
            Expect<IOException>(() => { using (var other = new AtomicProfileStore(normal, Valid)) { } }, check, "second writer cannot acquire lifetime process lock");
        }
        using (var store = new AtomicProfileStore(normal, Valid))
            check(store.Load(out string notice) == "v1|second" && notice == "", "reopened session loads primary and releases prior lock");

        string corrupt = Path.Combine(root, "corrupt.json");
        File.WriteAllText(corrupt, "broken original"); File.WriteAllText(corrupt + ".bak", "v1|last-good");
        using (var store = new AtomicProfileStore(corrupt, Valid))
        {
            check(store.Load(out string notice) == "v1|last-good" && notice.Length > 0, "corrupt primary recovers valid backup with notice");
            check(File.ReadAllText(corrupt) == "broken original", "load recovery does not change corrupt original");
            store.Save("v1|recovered-progress");
            string[] preserved = Directory.GetFiles(root, "corrupt.json.corrupt.*");
            check(preserved.Length == 1 && File.ReadAllText(preserved[0]) == "broken original", "recovery save quarantines exact corrupt original");
            check(File.ReadAllText(corrupt) == "v1|recovered-progress" && File.ReadAllText(corrupt + ".bak") == "v1|last-good", "recovery save protects previous valid backup");
            store.Save("v1|next");
            check(File.ReadAllText(corrupt + ".bak") == "v1|recovered-progress", "normal backup rotation resumes after recovery");
        }

        string missing = Path.Combine(root, "missing-primary.json"); File.WriteAllText(missing + ".bak", "v1|backup-only");
        using (var store = new AtomicProfileStore(missing, Valid))
        {
            check(store.Load(out string notice) == "v1|backup-only" && notice.Length > 0, "missing primary recovers existing valid backup");
            store.Save("v1|restored");
            check(File.ReadAllText(missing + ".bak") == "v1|backup-only", "backup-only recovery preserves backup on first save");
        }

        string invalid = Path.Combine(root, "both-invalid.json"); File.WriteAllText(invalid, "bad primary"); File.WriteAllText(invalid + ".bak", "bad backup");
        using (var store = new AtomicProfileStore(invalid, Valid))
        {
            Expect<IOException>(() => store.Load(out _), check, "two invalid generations fail closed");
            Expect<IOException>(() => store.Save("v1|fresh"), check, "failed load cannot be followed by fresh profile overwrite");
        }
        check(File.ReadAllText(invalid) == "bad primary" && File.ReadAllText(invalid + ".bak") == "bad backup", "failed recovery retains both invalid files");
        using (var store = new AtomicProfileStore(invalid, Valid))
            Expect<IOException>(() => store.Save("v1|fresh"), check, "save before load still refuses corrupt existing profile");

        string future = Path.Combine(root, "future.json"); File.WriteAllText(future, "v2|future-primary"); File.WriteAllText(future + ".bak", "v1|old");
        using (var store = new AtomicProfileStore(future, Valid))
        {
            Expect<NotSupportedException>(() => store.Load(out _), check, "unsupported primary never falls back to older backup");
            Expect<IOException>(() => store.Save("v1|fresh"), check, "unsupported load cannot be overwritten in the same session");
        }
        check(File.ReadAllText(future) == "v2|future-primary" && File.ReadAllText(future + ".bak") == "v1|old", "unsupported schema preserves files");
        string futureBackup = Path.Combine(root, "future-backup.json"); File.WriteAllText(futureBackup, "broken"); File.WriteAllText(futureBackup + ".bak", "v2|future-backup");
        using (var store = new AtomicProfileStore(futureBackup, Valid))
            Expect<NotSupportedException>(() => store.Load(out _), check, "unsupported backup fails closed during recovery");

        string locked = Path.Combine(root, "read-locked.json"); File.WriteAllText(locked, "v1|locked"); File.WriteAllText(locked + ".bak", "v1|old");
        using (var block = new FileStream(locked, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        using (var store = new AtomicProfileStore(locked, Valid))
            Expect<IOException>(() => store.Load(out _), check, "unreadable primary is not mistaken for corruption or absence");

        string writeFailure = Path.Combine(root, "write-failure.json");
        using (var store = new AtomicProfileStore(writeFailure, Valid))
        {
            store.Save("v1|first"); store.Save("v1|second");
            Directory.CreateDirectory(writeFailure + ".tmp");
            Expect<IOException>(() => store.Save("v1|third"), check, "temporary path failure rejects save");
            check(File.ReadAllText(writeFailure) == "v1|second" && File.ReadAllText(writeFailure + ".bak") == "v1|first", "failed write preserves primary and backup");
        }
        string replaceFailure = Path.Combine(root, "replace-failure.json");
        using (var store = new AtomicProfileStore(replaceFailure, Valid))
        {
            store.Save("v1|committed"); Directory.CreateDirectory(replaceFailure + ".bak");
            bool failed = false;
            try { store.Save("v1|uncommitted"); }
            catch (IOException) { failed = true; }
            catch (UnauthorizedAccessException) { failed = true; }
            check(failed, "backup destination failure rejects atomic replacement");
            check(File.ReadAllText(replaceFailure) == "v1|committed" && Directory.Exists(replaceFailure + ".bak"), "replacement failure retains committed primary and obstructing path");
            check(File.ReadAllText(replaceFailure + ".tmp") == "v1|uncommitted", "flushed uncommitted payload remains available after replacement failure");
        }
        string interrupted = Path.Combine(root, "interrupted.json");
        using (var store = new AtomicProfileStore(interrupted, Valid))
        {
            store.Save("v1|committed"); File.WriteAllText(interrupted + ".tmp", "partial old write");
            store.Save("v1|after-interruption");
            string[] oldTemporary = Directory.GetFiles(root, "interrupted.json.interrupted.*");
            check(oldTemporary.Length == 1 && File.ReadAllText(oldTemporary[0]) == "partial old write", "interrupted temporary is preserved instead of deleted or trusted");
            check(File.ReadAllText(interrupted) == "v1|after-interruption" && File.ReadAllText(interrupted + ".bak") == "v1|committed", "save succeeds after preserving interrupted temporary");
        }

        string external = Path.Combine(root, "external-edit.json"); File.WriteAllText(external, "v1|original");
        using (var store = new AtomicProfileStore(external, Valid))
        {
            store.Load(out _); File.WriteAllText(external, "external corruption");
            Expect<IOException>(() => store.Save("v1|new"), check, "external edit after load blocks overwrite");
            check(File.ReadAllText(external) == "external corruption", "unrecognized external edit is retained");
        }
        string changedBackup = Path.Combine(root, "changed-backup.json"); File.WriteAllText(changedBackup, "corrupt"); File.WriteAllText(changedBackup + ".bak", "v1|good");
        using (var store = new AtomicProfileStore(changedBackup, Valid))
        {
            store.Load(out _); File.WriteAllText(changedBackup + ".bak", "external backup");
            Expect<IOException>(() => store.Save("v1|recovered"), check, "backup edit during recovery blocks primary replacement");
            check(File.ReadAllText(changedBackup) == "corrupt" && File.ReadAllText(changedBackup + ".bak") == "external backup", "recovery conflict preserves both external files");
        }

        string limits = Path.Combine(root, "limits.json");
        using (var store = new AtomicProfileStore(limits, Valid))
        {
            store.Save("v1|small");
            Expect<IOException>(() => store.Save("v1|" + new string('x', AtomicProfileStore.MaximumPayloadBytes)), check, "oversized payload rejected before any write");
            Expect<IOException>(() => store.Save("v1|" + new string('\ud55c', AtomicProfileStore.MaximumPayloadBytes / 2)), check, "size cap counts UTF-8 bytes rather than characters");
            check(File.ReadAllText(limits) == "v1|small", "size rejection preserves committed profile");
        }
        string oversized = Path.Combine(root, "oversized.json"); File.WriteAllText(oversized, new string('x', AtomicProfileStore.MaximumPayloadBytes + 1)); File.WriteAllText(oversized + ".bak", "v1|old");
        using (var store = new AtomicProfileStore(oversized, Valid))
            Expect<IOException>(() => store.Load(out _), check, "oversized file fails closed without allocating unlimited data");
        string encoding = Path.Combine(root, "bad-utf8.json"); File.WriteAllBytes(encoding, new byte[] { 0xc3, 0x28 }); File.WriteAllText(encoding + ".bak", "v1|utf8-backup");
        using (var store = new AtomicProfileStore(encoding, Valid))
            check(store.Load(out _) == "v1|utf8-backup", "invalid UTF-8 is recoverable corruption");
        var disposed = new AtomicProfileStore(Path.Combine(root, "disposed.json"), Valid); disposed.Dispose(); disposed.Dispose();
        Expect<ObjectDisposedException>(() => disposed.Load(out _), check, "disposed store rejects load");
        Expect<ObjectDisposedException>(() => disposed.Save("v1|data"), check, "disposed store rejects save");
    }

    private static bool Valid(string payload)
    {
        if (payload.StartsWith("v2|", StringComparison.Ordinal)) throw new NotSupportedException("Future schema.");
        return payload.StartsWith("v1|", StringComparison.Ordinal);
    }

    private static void Expect<T>(Action operation, Action<bool, string> check, string name) where T : Exception
    {
        bool caught = false;
        try { operation(); } catch (T) { caught = true; }
        check(caught, name);
    }
}
