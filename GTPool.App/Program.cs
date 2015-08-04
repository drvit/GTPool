using System;
using GTPool.App.ThreadExercises;
using GTPool.Sandbox;

namespace GTPool.App
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args != null && (args.Length > 0 && args[0].Equals("/F")))
            {
                var numberOfThreads = 10;
                if (args.Length >= 2)
                    int.TryParse(args[1], out numberOfThreads);

                var fibonacciCalculations = 10;
                if (args.Length >= 3)
                    int.TryParse(args[2], out fibonacciCalculations);

                var fibonacciSeed = 35;
                if (args.Length >= 4)
                    int.TryParse(args[3], out fibonacciSeed);

                CalculateFibonacci.Run(numberOfThreads, fibonacciCalculations, fibonacciSeed);
            }

            if (args != null && (args.Length > 0 && args[0].Equals("/E")))
            {
                RunExercises.WhatExercise();
            }

            Utils.LoggerWaitToFinish();
        }
    }
}
