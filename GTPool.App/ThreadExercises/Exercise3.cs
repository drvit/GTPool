using System;
using System.Threading;

namespace GTPool.App.ThreadExercises
{
    class Exercise3
    {
        static int _count2;

        public static void Run()
        {
            var job = new ThreadStart(ThreadJob);
            var thread = new Thread(job);
            thread.Start();

            for (var i = 0; i < 5; i++)
            {
                var tmp = _count2;
                Console.WriteLine("Read count={0}", tmp);
                Thread.Sleep(50);
                tmp++;
                Console.WriteLine("Incremented tmp to {0}", tmp);
                Thread.Sleep(20);
                _count2 = tmp;
                Console.WriteLine("Written count={0}", tmp);
                Thread.Sleep(30);
            }

            thread.Join();
            Console.WriteLine("Final count: {0}", _count2);
        }

        static void ThreadJob()
        {
            for (var i = 0; i < 5; i++)
            {
                var tmp = _count2;
                Console.WriteLine("\t\t\t\tRead count={0}", tmp);
                Thread.Sleep(20);
                tmp++;
                Console.WriteLine("\t\t\t\tIncremented tmp to {0}", tmp);
                Thread.Sleep(10);
                _count2 = tmp;
                Console.WriteLine("\t\t\t\tWritten count={0}", tmp);
                Thread.Sleep(40);
            }
        }
    }
}
