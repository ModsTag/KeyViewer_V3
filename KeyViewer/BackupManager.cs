using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace KeyViewer
{
    public static class BackupManager
    {
        static CancellationTokenSource cts;
        public static void Start()
        {
            if (!Directory.Exists(Path.Combine(Main.Mod.Path, "Backups")))
                Directory.CreateDirectory(Path.Combine(Main.Mod.Path, "Backups"));
            cts = new CancellationTokenSource();
            var current = new Thread(t => BackupThread((CancellationToken)t));
            current.Start(cts.Token);
        }
        public static void Stop()
        {
            cts.Cancel();
            cts = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
        }
        static void BackupThread(CancellationToken cancelTok)
        {
            WriteBackup();
            long time = DateTime.Now.Ticks;
            while (!cancelTok.IsCancellationRequested)
            {
                Thread.Sleep(1000);
                long current = DateTime.Now.Ticks;
                if (current - time > Main.Settings.BackupInterval * 10_000_000)
                {
                    time = current;
                    WriteBackup();
                }
            }
        }
        static void WriteBackup()
        {
            var now = DateTime.Now;
            var path = Path.Combine(Main.Mod.Path, "Backups", $"{now.Year}-{now.Month}-{now.Day} {now.Hour:D2}h{now.Minute:D2}m{now.Second:D2}s Backup.xml");
            using FileStream textWriter = new FileStream(path, FileMode.OpenOrCreate);
            new XmlSerializer(typeof(Settings), default(XmlAttributeOverrides)).Serialize(textWriter, Main.Settings);
        }
    }
}
