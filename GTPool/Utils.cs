using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GTPool
{
    public class Utils
    {
        public static void Log(string message)
        {
#if DEBUG
            var thread = Thread.CurrentThread;
            var threadId = thread.ManagedThreadId.ToString();

            Trace.WriteLine(string.Format("------------------| {0} | {1} | {2} ", 
                HiResDateTime.UtcNow,
                thread.Name ?? threadId.PadLeft(16 - threadId.Length, ' '),
                message));
#endif
        }

        private static readonly Random _random = new Random();
        private static readonly HashSet<int> _randomList = new HashSet<int>();
        public static int GenerateUniqueNumber()
        {
            int newNumber;

            do
            {
                newNumber = _random.Next(1000, 999999);
            } while (_randomList.Contains(newNumber));

            _randomList.Add(newNumber);

            return newNumber;
        }

        public class HiResDateTime
        {
            private static long _lastTimeStamp = DateTime.UtcNow.Ticks;

            public static long UtcNowTicks
            {
                get
                {
                    long orig, newval;
                    do
                    {
                        orig = _lastTimeStamp;
                        var now = DateTime.UtcNow.Ticks;
                        newval = Math.Max(now, orig + 1);
                    } while (Interlocked.CompareExchange
                        (ref _lastTimeStamp, newval, orig) != orig);

                    return newval;
                }
            }

            public static string UtcNow
            {
                get
                {
                    //return new DateTime(newval, DateTimeKind.Utc).ToString("o");
                    return new DateTime(UtcNowTicks, DateTimeKind.Utc).ToString("HH:mm.ss:fff");
                }
            }

        }
    }
}
