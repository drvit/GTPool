﻿using System.Threading;

namespace GTPool.App
{
    public class CalculateFibonacciDnTp
    {
        public static void Run(int fibonacciCalculations, int fibonacciSeed)
        {
            // One event is used for each Fibonacci object
            var doneEvents = new ManualResetEvent[fibonacciCalculations];
            //var fibArray = new Fibonacci[fibonacciCalculations];

            // Configure and launch threads using ThreadPool:
            //Console.WriteLine("launching {0} fibonacci tasks with seed in {1}", fibonacciCalculations, fibonacciSeed);

            for (var i = 0; i < fibonacciCalculations; i++)
            {
                var f = new Fibonacci(i + fibonacciSeed);
                doneEvents[i] = new ManualResetEvent(false);
                //fibArray[i] = f;
                ThreadPool.QueueUserWorkItem(f.ThreadPoolCalculate, doneEvents[i]);
            }

            // Wait for all threads in pool to calculation...
            WaitHandle.WaitAll(doneEvents);
            //Console.WriteLine("All calculations are complete.");

            // Display the results...
            //for (int i = 0; i < fibonacciCalculations; i++)
            //{
            //    Fibonacci f = fibArray[i];
            //    //Console.WriteLine("Fibonacci({0}) = {1}", f.N, f.FibOfN);
            //}
        }
    }
}
