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
                var numberOfThreads = 5;

                if (args.Length >= 2)
                    int.TryParse(args[1], out numberOfThreads);

                CalculateFibonacci.Run(numberOfThreads);
            }

            if (args != null && (args.Length > 0 && args[0].Equals("/E")))
            {
                RunExercises.WhatExercise();
            }
        }
    }
}
