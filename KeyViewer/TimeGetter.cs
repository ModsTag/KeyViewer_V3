using System;
using System.Diagnostics;

namespace KeyViewer
{
    internal static class TimeGetter
    {
        static TimeGetter()
        {
            long t;
            long f = DateTime.Now.Ticks;
            do
            {
                _start = DateTime.Now.Ticks;
                _delta = Stopwatch.GetTimestamp(); // ~100ns usage
                t = DateTime.Now.Ticks;
            }
            while (_start - f == 0 || t != _start);
            _delta -= 1;
        }

        private static readonly long _start;
        private static readonly long _delta;

        public static long GetTime()
        {
            return _start + (Stopwatch.GetTimestamp() - _delta);
        }
    }
}
