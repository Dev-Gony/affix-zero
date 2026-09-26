using System;
using System.IO;
using System.Text;

namespace AffixZero.Core
{
    // One instance owns a profile for a process session. The validator owns the
    // format: false means corrupt data; unsupported formats must throw.
    public sealed class AtomicProfileStore : IDisposable
    {
        public const int MaximumPayloadBytes = 2 * 1024 * 1024;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        private readonly object gate = new object();
        private readonly string filePath;
        private readonly Func<string, bool> validate;
        private FileStream processLock;
        private byte[] loadedPrimary, recoveredBackup;
        private bool loaded, recovered, readBlocked, disposed;

        public AtomicProfileStore(string filePath, Func<string, bool> validate)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Profile path is required.", nameof(filePath));
            this.validate = validate ?? throw new ArgumentNullException(nameof(validate));
            this.filePath = Path.GetFullPath(filePath);
            Directory.CreateDirectory(Path.GetDirectoryName(this.filePath));
            // Keep the file after disposal: deleting a lock file creates a race
            // between sessions opening different file handles at the same path.
            processLock = new FileStream(this.filePath + ".lock", FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None);
        }

        public string Load(out string notice)
        {
            lock (gate)
            {
                EnsureUsable();
                notice = string.Empty;
                try
                {
                    byte[] primary = ReadBytes(filePath);
                    if (TryValidate(primary, out string payload))
                    {
                        Remember(primary, null, false);
                        return payload;
                    }

                    byte[] backup = ReadBytes(filePath + ".bak");
                    if (TryValidate(backup, out payload))
                    {
                        Remember(primary, backup, true);
                        notice = "Recovered the last valid backup. The original save is retained until it can be preserved before saving.";
                        return payload;
                    }
                    if (primary == null && backup == null)
                    {
                        Remember(null, null, false);
                        return null;
                    }
                    throw new IOException("No valid primary or backup profile exists. Existing files have been preserved.");
                }
                catch
                {
                    // A caller must not catch a bad load and save a fresh profile
                    // through this instance. Repair/recovery needs a new session.
                    readBlocked = true;
                    throw;
                }
            }
        }

        public void Save(string payload)
        {
            lock (gate)
            {
                EnsureUsable();
                if (payload == null) throw new ArgumentNullException(nameof(payload));
                int length = Utf8.GetByteCount(payload);
                if (length > MaximumPayloadBytes) throw new IOException("Profile exceeds the 2 MiB UTF-8 limit.");
                if (!validate(payload)) throw new IOException("Refusing to save an invalid profile.");
                byte[] bytes = Utf8.GetBytes(payload);
                if (!loaded) Load(out _);

                // The lifetime lock excludes cooperating writers. Also reject
                // external edits instead of overwriting an unexpected file.
                if (!SameBytes(ReadBytes(filePath), loadedPrimary) ||
                    (recovered && !SameBytes(ReadBytes(filePath + ".bak"), recoveredBackup)))
                {
                    readBlocked = true;
                    throw new IOException("Profile files changed after loading. Saving has been blocked.");
                }

                string temporary = filePath + ".tmp";
                PreserveInterruptedTemporary(temporary);
                WriteNewFile(temporary, bytes);
                if (loadedPrimary == null)
                {
                    // Missing primary plus valid backup recovery also takes this
                    // path, leaving that backup untouched.
                    File.Move(temporary, filePath);
                }
                else if (recovered)
                {
                    WriteNewFile(UniquePreservedPath("corrupt"), loadedPrimary);
                    // Never rotate corrupt primary bytes into the valid backup.
                    File.Replace(temporary, filePath, null);
                }
                else
                {
                    File.Replace(temporary, filePath, filePath + ".bak");
                }
                Remember(bytes, null, false);
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (disposed) return;
                disposed = true;
                processLock.Dispose();
                processLock = null;
            }
        }

        private void EnsureUsable()
        {
            if (disposed) throw new ObjectDisposedException(nameof(AtomicProfileStore));
            if (readBlocked) throw new IOException("Profile loading failed or files changed; this session cannot save.");
        }

        private void Remember(byte[] primary, byte[] backup, bool fromBackup)
        {
            loadedPrimary = primary;
            recoveredBackup = backup;
            recovered = fromBackup;
            loaded = true;
        }

        private bool TryValidate(byte[] bytes, out string payload)
        {
            payload = null;
            if (bytes == null) return false;
            try
            {
                int offset = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf ? 3 : 0;
                payload = Utf8.GetString(bytes, offset, bytes.Length - offset);
            }
            catch (DecoderFallbackException) { return false; }
            // In particular, never reinterpret NotSupportedException as corruption.
            return validate(payload);
        }

        private static byte[] ReadBytes(string path)
        {
            FileStream stream;
            try { stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read); }
            catch (FileNotFoundException) { return null; }
            // Access denial, sharing errors, missing directories and other I/O
            // errors are not evidence of a fresh profile or a corrupt payload.
            using (stream)
            {
                if (stream.Length > MaximumPayloadBytes) throw new IOException("Profile file exceeds the 2 MiB limit: " + path);
                byte[] bytes = new byte[(int)stream.Length];
                int count = 0;
                while (count < bytes.Length)
                {
                    int read = stream.Read(bytes, count, bytes.Length - count);
                    if (read == 0) throw new EndOfStreamException("Profile changed while reading: " + path);
                    count += read;
                }
                if (stream.ReadByte() != -1) throw new IOException("Profile grew while reading: " + path);
                return bytes;
            }
        }

        private static void WriteNewFile(string path, byte[] bytes)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        private void PreserveInterruptedTemporary(string temporary)
        {
            FileAttributes attributes;
            try { attributes = File.GetAttributes(temporary); }
            catch (FileNotFoundException) { return; }
            if ((attributes & FileAttributes.Directory) != 0) throw new IOException("Temporary profile path is a directory.");
            File.Move(temporary, UniquePreservedPath("interrupted"));
        }

        private string UniquePreservedPath(string kind) => filePath + "." + kind + "." +
            DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffffffZ") + "." + Guid.NewGuid().ToString("N");

        private static bool SameBytes(byte[] left, byte[] right)
        {
            if (left == null || right == null) return left == right;
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
            return true;
        }
    }
}
