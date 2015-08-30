using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTP = GTPool.GenericThreadPool;

namespace GTPool.App.ThreadExercises
{
    public class Exercise13
    {
        public static void Run()
        {
            GTP.AddJob(new ManagedJob((Action<int, int, string>) Task, new object[] {100, 200, null}));
            GTP.AddJob(new ManagedJob((Action<int, int, string>) Task, new object[] {200, 130, null}));
            GTP.AddJob(new ManagedJob((Action<int, int, string>) Task, new object[] {300, 160, null}));

            Task(400, 230, "Main Thread   ");
            //Console.WriteLine("Main Thread | {0}", );

            Thread.Sleep(5000);
        }

        private static void Task(int seed, int sleepTime, string threadName)
        {
            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("{0} | result = {1}",
                    threadName ?? Thread.CurrentThread.Name,
                    seed + i);

                Thread.Sleep(sleepTime);
            }
        }
    }
}
