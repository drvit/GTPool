using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool
{
    public class Utils
    {
        public static void Log(string message)
        {
#if DEBUG
            Trace.WriteLine(string.Format("------------------| {0} | {1} ", HiResDateTime.UtcNow, message));
#endif
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
