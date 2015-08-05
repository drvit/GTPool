using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using GTPool.App.ThreadExercises;
using GTPool.Sandbox;

namespace GTPool.App
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.Write("What Program? (/FGTP | /BM | /EX) [/args]: ");
                var pg = Console.ReadLine();

                if (pg != null && 
                    (pg.StartsWith("/FGTP ") || pg.StartsWith("/BM ") || pg.StartsWith("/EX ")))
                {
                    args = pg.Split(' ');
                }
            }

            if (args.Length > 0)
            {
                if (args[0].Equals("/FGTP"))
                {
                    CalculateFibonacciUsingGtp(args.Where(x => !x.StartsWith("/FGTP")).ToList());
                }

                if (args.Length > 0 && args[0].Equals("/EX"))
                {
                    RunExercises.WhatExercise();
                }

                if (args.Length > 0 && args[0].Equals("/BM"))
                {
                    //Utils.StopLogging();

                    var bmIterations = 5;
                    var targetName = string.Empty;
                    TryGetIntArg(args, "BMI", ref bmIterations);

                    Action<IList<string>> target = null;

                    if (args.Contains("/FGTP"))
                    {
                        targetName = "CalculateFibonacciUsingGtp";
                        target = CalculateFibonacciUsingGtp;
                    }

                    if (target != null)
                    {
                        Console.WriteLine("---Benchmarking {0}", targetName);
                        Benchmarck(bmIterations, target, args);
                    }
                }

                Utils.WaitLoggingToFinish();
            }
        }

        private static void TryGetIntArg(IEnumerable<string> args, string argName, ref int value)
        {
            var tempVal = value;
            argName = "/" + argName;
            
            foreach (var arg in args)
            {
                if (arg.StartsWith(argName))
                {
                    if (!int.TryParse(arg.Replace(argName, ""), out value))
                        value = tempVal;
                    break;
                }
            }
        }

        private static void Benchmarck(int bmIterations, Action<IList<string>> target, IList<string> args)
        {
            Utils.Log("=================== Benchmark Starting ===================");
            var s1 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                target.Invoke(args);
            }
            s1.Stop();
            var s2 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                target.Invoke(args);
            }
            s2.Stop();
            var s3 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                target.Invoke(args);
            }
            s3.Stop();

            var summary = string.Format("Summary: Loop1 {0}, Loop2 {1}, Loop3 {2}",
                s1.ElapsedMilliseconds,
                s2.ElapsedMilliseconds,
                s3.ElapsedMilliseconds);

            Utils.Log(summary);

            Console.WriteLine(summary);
        }

        // /FGTP /F15 /S35 /TI2 /TA5
        private static void CalculateFibonacciUsingGtp(IList<string> args)
        {
            var minThreads = 2;
            var maxThreads = 2;
            var fibonacciCalculations = 10;
            var fibonacciSeed = 30;

            TryGetIntArg(args, "TI", ref minThreads);
            TryGetIntArg(args, "TA", ref maxThreads);
            TryGetIntArg(args, "F", ref fibonacciCalculations);
            TryGetIntArg(args, "S", ref fibonacciSeed);

            CalculateFibonacci.Run(minThreads, maxThreads, fibonacciCalculations, fibonacciSeed);
        }
    }
}
