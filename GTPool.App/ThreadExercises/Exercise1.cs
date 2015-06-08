using System;
using System.Threading;

namespace GTPool.App.ThreadExercises
{
    public class Exercise1
    {
        public static void Run()
        {
            var job = new ThreadStart(ThreadJob1);
            var thread = new Thread(job);
            thread.Start();

            var par1 = "oie";
            var par2 = 47;

            var par3 = par1;
            var par4 = par2;
            
            var thread2 = new Thread(() => ThreadJob2(par3, par4));
            thread2.Start();

            for (var i = 0; i < 15; i++)
            {
                par2++;
                par1 = "oie (" + par2 + ")";
                Console.WriteLine("Main thread: {0}", i);
                Thread.Sleep(1000);
            }

        }

        static void ThreadJob2(string par1, int par2)
        {
            for (var i = 0; i < 25; i++)
            {
                Console.WriteLine("SOME thread: {0}, {1} - {2}", i, par1, par2);
                Thread.Sleep(500);
            }
        }

        static void ThreadJob1()
        {
            for (var i = 0; i < 25; i++)
            {
                Console.WriteLine("Other thread: {0}", i);
                Thread.Sleep(500);
            }
        }
    }
}
