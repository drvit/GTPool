using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTP = GTPool.GenericThreadPool;

namespace GTPool.App
{
    public class CalculateFibonacci
    {
        public static void Run(int numberOfThreads, int fibonacciCalculations, int fibonacciSeed)
        {
            // One event is used for each Fibonacci object
            var doneEvents = new ManualResetEvent[fibonacciCalculations];
            var fibArray = new Fibonacci[fibonacciCalculations];

            // Configure and launch threads using ThreadPool:
            Console.WriteLine("launching {0} tasks...", fibonacciCalculations);

            GTP.Init<GtpAsync>(5, numberOfThreads, 500);

            for (var i = 0; i < fibonacciCalculations; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                var f = new Fibonacci(i + fibonacciSeed, doneEvents[i]);
                fibArray[i] = f;
                GTP.AddJob(new ManagedAsyncJob((Action<object>) f.ThreadPoolCallback, new object[] { i }));
            }

            // Wait for all threads in pool to calculation...
            WaitHandle.WaitAll(doneEvents);
            Console.WriteLine("All calculations are complete.");

            // Display the results...
            for (var i = 0; i < fibonacciCalculations; i++)
            {
                var f = fibArray[i];
                Console.WriteLine("Fibonacci({0}) = {1}", f.N, f.FibOfN);
            }

            GTP.End();
        }
    }
}
