using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine.TestTools;

namespace KeyViewer
{
    public static class KPSCalculatorSync
    {
        static bool awake;
        static Profile Profile;
        static Queue<long> keyqueue;
        static long time;
        static int kps;
        static int max;
        static double avg;
        static long avg_n;

        public static int Kps => kps;
        public static int Max => max;
        public static double Avg => Math.Round(avg, 4);

        public static void Start(Profile profile)
        {
            Profile = profile;
            time = Stopwatch.GetTimestamp();
            awake = true;
            if (keyqueue is null)
                keyqueue = new Queue<long>(1024);
            else
                keyqueue.Clear();
        }
        public static void Stop()
        {
            awake = false;
        }
        public static void Press()
        {
            keyqueue.Enqueue(Stopwatch.GetTimestamp() + Stopwatch.Frequency);
        }
        public static void Update()
        {
            if (!awake)
            {
                Main.Logger.Log("Why Not Awake???");
                return;
            }

            long current_time = Stopwatch.GetTimestamp();
            if ((current_time - time) / (Stopwatch.Frequency / 1000) >= Profile.KPSUpdateRateMs)
            {
                while (keyqueue.Count > 0)
                {
                    if (current_time > keyqueue.Peek())
                    {
                        keyqueue.Dequeue();
                    }
                    break;
                }
                kps = keyqueue.Count;
                max = Math.Max(max, kps);

                avg = Profile.KPSUpdateRateMs > 100 ? (avg + kps) / 2 : (avg * 3 + kps) / 4;
                if (avg < 0.1)
                    avg = 0;
                // if (kps != 0)
                // {
                //     avg = (avg * avg_n + kps) / (avg_n + 1.0);
                //     avg_n += 1L;
                // }

                if ((current_time - time) / (Stopwatch.Frequency / 1000) - Profile.KPSUpdateRateMs > Profile.KPSUpdateRateMs)
                    time = current_time;
                else
                    time += Profile.KPSUpdateRateMs * (Stopwatch.Frequency / 1000);
            }
        }

    }
    /* csnb shit code
    public static class KPSCalculator
    {
        static Profile Profile;
        static Thread CalculatingThread;
        static int PressCount;
        public static int Kps;
        public static int Max;
        public static double Average;
        public static void Start(Profile profile)
        {
            try
            {
                Profile = profile;
                if (CalculatingThread == null)
                    (CalculatingThread = GetCalculateThread()).Start();
            }
            catch { }
        }
        public static void Stop()
        {
            try { CalculatingThread.Abort(); }
            catch { }
        }
        public static void Press() => PressCount++;
        static Thread GetCalculateThread()
        {
            return new Thread(() =>
            {
                try
                {
                    LinkedList<int> timePoints = new LinkedList<int>();
                    int prev = 0, total = 0;
                    long n = 0;
                    Stopwatch watch = Stopwatch.StartNew();
                    while (true)
                    {
                        if (watch.ElapsedMilliseconds >= Profile.KPSUpdateRateMs)
                        {
                            int temp = PressCount;
                            PressCount = 0;
                            int kps = temp;
                            foreach (int i in timePoints)
                                kps += i;
                            Max = Math.Max(kps, Max);
                            if (kps != 0)
                            {
                                Average = (Average * n + kps) / (n + 1.0);
                                n += 1L;
                                total += temp;
                            }
                            prev = kps;
                            timePoints.AddFirst(temp);
                            if (timePoints.Count >= 1000 / Profile.KPSUpdateRateMs)
                                timePoints.RemoveLast();
                            Kps = kps;
                            watch.Restart();
                            Thread.Sleep(Math.Max(Profile.KPSUpdateRateMs - 1, 0));
                        }
                    }
                }
                finally { CalculatingThread = null; }
            });
        }
    }
    */
}
