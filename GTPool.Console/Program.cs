using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var job = new ThreadStart(ThreadJob);
            var thread = new Thread(job);
            thread.Start();

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("Main thread: {0}", i);
                Thread.Sleep(1000);
            }
        }

        static void ThreadJob()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Other thread: {0}", i);
                Thread.Sleep(500);
            }
        }
    }
}
