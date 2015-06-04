using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.App.ThreadExercises
{
    public class Exercise5
    {
        static int _count;
        static readonly object CountLock = new object();

        public static void Run()
        {
            var job = new ThreadStart(ThreadJob);
            var thread = new Thread(job);
            thread.Start();

            for (var i = 0; i < 5; i++)
            {
                lock (CountLock)
                {
                    var tmp = _count;
                    Console.WriteLine("Read count={0}", tmp);
                    Thread.Sleep(50);
                    tmp++;
                    Console.WriteLine("Incremented tmp to {0}", tmp);
                    Thread.Sleep(20);
                    _count = tmp;
                    Console.WriteLine("Written count={0}", tmp);
                }
                Thread.Sleep(30);
            }

            thread.Join();
            Console.WriteLine("Final count: {0}", _count);
        }

        private static void ThreadJob()
        {
            for (var i = 0; i < 5; i++)
            {
                lock (CountLock)
                {
                    var tmp = _count;
                    Console.WriteLine("\t\t\t\tRead count={0}", tmp);
                    Thread.Sleep(20);
                    tmp++;
                    Console.WriteLine("\t\t\t\tIncremented tmp to {0}", tmp);
                    Thread.Sleep(10);
                    _count = tmp;
                    Console.WriteLine("\t\t\t\tWritten count={0}", tmp);
                }
                Thread.Sleep(40);
            }
        }
    }
}
