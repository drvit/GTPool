using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.App.ThreadExercises
{
    public class Exercise8
    {
        public static void Run()
        {
            var job = new ThreadStart(ThreadJob);
            var thread = new Thread(job);
            thread.Start();

            // Let the thread start running
            Thread.Sleep(2000);

            // Now tell it to stop counting
            Stop = true;
        }

        static readonly object StopLock = new object();

        static bool _stop;
        static bool Stop
        {
            get
            {
                lock (StopLock)
                {
                    return _stop;
                }
            }
            set
            {
                lock (StopLock)
                {
                    _stop = value;
                }
            }
        }

        static void ThreadJob()
        {
            var count = 0;
            while (!Stop)
            {
                Console.WriteLine("Extra thread: count {0}", count);
                Thread.Sleep(100);
                count++;
            }
        }
    }
}
