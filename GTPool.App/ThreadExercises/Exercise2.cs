using System;
using System.Threading;

namespace GTPool.App.ThreadExercises
{
    public class Exercise2
    {
        static int _count1;

        public static void Run()
        {
            var job = new ThreadStart(ThreadJob3);
            var thread = new Thread(job);
            thread.Start();

            for (var i = 0; i < 5; i++)
            {
                _count1++;
            }

            // pauses the main thread until the other thread has completed
            thread.Join();
            Console.WriteLine("Final count: {0}", _count1);
        }

        static void ThreadJob3()
        {
            for (var i = 0; i < 5; i++)
            {
                _count1++;
            }
        }
    }
}
