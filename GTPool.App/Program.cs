using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using GTPool.App.ThreadExercises;
using GTPool.Sandbox;
using GTP = GTPool.GenericThreadPool;

namespace GTPool.App
{
    class Program
    {
        static int _minThreads = 2;
        static int _maxThreads = 2;
        static int _idleTime = 100;
        
        private static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.Write("What Program? (/FGTP | /FDNTP | /BM | /EX) [/args]: ");
                var pg = Console.ReadLine();

                if (pg != null && 
                    (pg.StartsWith("/FGTP ") || pg.StartsWith("/BM ") || pg.StartsWith("/EX ")))
                {
                    args = pg.Split(' ');
                }
            }

            ApplicationStart(args);

            if (args.Length > 0)
            {
                var fibonacciCalculations = 10;
                var fibonacciSeed = 30;

                TryGetIntArg(args, "FC", ref fibonacciCalculations);
                TryGetIntArg(args, "FS", ref fibonacciSeed);

                if (args[0].Equals("/FGTP"))
                {
                    var s = Stopwatch.StartNew();
                    CalculateFibonacciGtp.Run(fibonacciCalculations, fibonacciSeed);
                    s.Stop();
                    Console.WriteLine(s.ElapsedMilliseconds);
                }

                if (args[0].Equals("/FDNTP"))
                {
                    var s = Stopwatch.StartNew();
                    CalculateFibonacciDnTp.Run(fibonacciCalculations, fibonacciSeed);
                    s.Stop();
                    Console.WriteLine(s.ElapsedMilliseconds);
                }

                if (args.Length > 0 && args[0].Equals("/EX"))
                {
                    RunExercises.WhatExercise();
                }

                if (args.Length > 0 && args[0].Equals("/BM"))
                {
                    Utils.Log("=================== Benchmark Starting ===================", true);
                    
                    var bmIterations = 5;
                    TryGetIntArg(args, "BMI", ref bmIterations);

                    var targets = new Dictionary<string, Action<int, int>>
                    {
                        {"Generic Thread Pool", CalculateFibonacciGtp.Run},
                        {".Net Thread Pool", CalculateFibonacciDnTp.Run}
                    };

                    Utils.Log(string.Format("/BM /BMI{0} /FC{1} /FS{2} /TI{3} /TA{4} /TT{5}",
                        bmIterations, fibonacciCalculations, fibonacciSeed, _minThreads, _maxThreads, _idleTime), true);

                    foreach(var tgt in targets)
                    {
                        Utils.Log(string.Format(tgt.Key), true);
                        Benchmarck(bmIterations, tgt.Value, fibonacciCalculations, fibonacciSeed);
                    }

                    Utils.Log("--- Finished", true);
                }
            }

            ApplicationEnd();
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

        private static void Benchmarck(int bmIterations, Action<int, int> target, 
            int fibonacciCalculations, int fibonacciSeed)
        {
            var s1 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                var si = Stopwatch.StartNew();
                target.Invoke(fibonacciCalculations, fibonacciSeed);
                si.Stop();
                Console.WriteLine("Time: " + si.ElapsedMilliseconds);
            }
            s1.Stop();
            var s2 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                var si = Stopwatch.StartNew();
                target.Invoke(fibonacciCalculations, fibonacciSeed);
                si.Stop();
                Console.WriteLine("Time: " + si.ElapsedMilliseconds);
            }
            s2.Stop();
            var s3 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                var si = Stopwatch.StartNew();
                target.Invoke(fibonacciCalculations, fibonacciSeed);
                si.Stop();
                Console.WriteLine("Time: " + si.ElapsedMilliseconds);
            }
            s3.Stop();

            var summary = string.Format("Summary: Loop1 {0}, Loop2 {1}, Loop3 {2}",
                s1.ElapsedMilliseconds,
                s2.ElapsedMilliseconds,
                s3.ElapsedMilliseconds);

            Utils.Log(summary, true);

            Console.WriteLine(summary);
        }

        private static void ApplicationStart(string[] args)
        {
            Utils.StopLogging();

            if (args.Length > 0)
            {
                TryGetIntArg(args, "TI", ref _minThreads);
                TryGetIntArg(args, "TA", ref _maxThreads);
                TryGetIntArg(args, "TT", ref _idleTime);

                GTP.Init<GtpAsync>(_minThreads, _maxThreads, _idleTime);
            }
        }

        private static void ApplicationEnd()
        {
            GTP.End(true);
            Utils.WaitLoggingToFinish();
        }

    }
}
