using System;
using System.Threading;

namespace GTPool.App.ThreadExercises
{
    public class Exercise7
    {
        public static void Run()
        {
            var events = new WaitHandle[10];
            for (var i = 0; i < events.Length; i++)
            {
                events[i] = new ManualResetEvent(false);
                var r = new Runner((ManualResetEvent) events[i], i);
                new Thread(r.Run).Start();
            }

            var index = WaitHandle.WaitAny(events);

            Console.WriteLine("***** The winner is {0} *****", index);

            WaitHandle.WaitAll(events);
            Console.WriteLine("All finished!");
            Console.ReadLine();
        }
    }

    class Runner
    {
        static readonly object RngLock = new object();
        static readonly Random Rng = new Random();

        readonly ManualResetEvent _ev;
        readonly int _id;

        internal Runner(ManualResetEvent ev, int id)
        {
            _ev = ev;
            _id = id;
        }

        internal void Run()
        {
            for (var i = 0; i < 10; i++)
            {
                int sleepTime;
                // Not sure about the thread safety of Random...
                lock (RngLock)
                {
                    sleepTime = Rng.Next(2000);
                }
                Thread.Sleep(sleepTime);
                Console.WriteLine("Runner {0} at stage {1}", _id, i);
            }
            _ev.Set();
        }
    }

    public class Exercise71
    {
        public static void Run()
        {
            for (var i = 0; i < 10; i++)
            {
                var r = new OldRunner(i);
                new Thread(r.Run).Start();
            }

            Console.WriteLine("***** The winner is ??? *****");

            Console.WriteLine("All finished!");
            Console.ReadLine();
        }
    }

    class OldRunner
    {
        static readonly object RngLock = new object();
        static readonly Random Rng = new Random();

        readonly int _id;

        internal OldRunner(int id)
        {
            _id = id;
        }

        internal void Run()
        {
            for (var i = 0; i < 10; i++)
            {
                int sleepTime;
                // Not sure about the thread safety of Random...
                lock (RngLock)
                {
                    sleepTime = Rng.Next(2000);
                }
                Thread.Sleep(sleepTime);
                Console.WriteLine("Runner {0} at stage {1}", _id, i);
            }
        }
    }
}
