using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using GTPool.App.ThreadExercises;
using GTPool.Sandbox;
using GTP = GTPool.GenericThreadPool;
using System.Threading;

namespace GTPool.App
{
    class Program
    {
        static int _minThreads = 1;
        static int _maxThreads = 25;
        static int _idleTime = 100;
        
        [MTAThread]
        private static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.Write("What Program? (/FGTP | /FDNTP | /BM | /EX) [/args]: ");
                var pg = Console.ReadLine();

                if (pg != null && 
                    (pg.StartsWith("/FGTP ") || pg.StartsWith("/BM ") || pg.StartsWith("/EX")))
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
                    Utils.StartLogging();
                    RunExercises.WhatExercise();
                }

                if (args.Length > 0 && args[0].Equals("/BM"))
                {
                    //BmLog("=================== Benchmark Starting ===================");
                    
                    var bmIterations = 5;
                    TryGetIntArg(args, "BMI", ref bmIterations);

                    var targets = new Dictionary<string, Action<int, int>>
                    {
                        {"Single Thread", CalculateFibonacciSt.Run},
                        {".Net Thread Pool", CalculateFibonacciDnTp.Run},
                        {"GTPool", CalculateFibonacciGtp.Run}
                    };

                    //BmLog(string.Format("/BM /BMI{0} /FC{1} /FS{2} /TI{3} /TA{4} /TT{5}",
                    //    bmIterations, fibonacciCalculations, fibonacciSeed, _minThreads, _maxThreads, _idleTime));

                    //BmLog(string.Format("Threading Style, L1, L2, L3, I[{0}]; S[{2}]; C[{1}]", bmIterations, fibonacciSeed, fibonacciCalculations));
                    BmLog("Threading Style, L1, L2, L3, BMI, FSD, FCA");

                    var originalFc = fibonacciCalculations;
                    var originalBMI = bmIterations;

                    Stopwatch sw;
                    var limit = 3000;
                    do
                    {
                        Console.WriteLine("-----------------------------------------");

                        sw = Stopwatch.StartNew();
                        foreach (var tgt in targets)
                        {
                            Benchmarck(tgt.Key, tgt.Value, bmIterations, fibonacciCalculations, fibonacciSeed);
                        }
                        sw.Stop();
                        Console.WriteLine(">>>> Iteration time: " + sw.ElapsedMilliseconds);

                        //if (sw.ElapsedMilliseconds >= limit)
                        //{
                        //    Console.WriteLine("*******************************************");
                        //    Console.WriteLine("What's the new limit, 3000?");
                        //    var newLimit = Console.ReadLine();

                        //    if (!int.TryParse(newLimit, out limit))
                        //        limit = 3000;
                        //}

                        //bmIterations++;
                        //fibonacciCalculations++;

                        //if (fibonacciCalculations < 40)
                        if (bmIterations < 20)
                        {
                            //fibonacciCalculations++;
                            bmIterations++;
                        }
                        else
                        {
                            //fibonacciCalculations = originalFc;
                            //bmIterations++;
                            bmIterations = originalBMI;
                            fibonacciCalculations++;
                        }

                        //} while (sw.ElapsedMilliseconds < limit);
                    } while (fibonacciCalculations < 35);

                    Console.WriteLine("-------------Finished------------");
                    //BmLog("--- Finished");
                    //Thread.Sleep(3000);
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

        private static void Benchmarck(string targetName, Action<int, int> target, int bmIterations, 
            int fibonacciCalculations, int fibonacciSeed)
        {
            var s1 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                //var si = Stopwatch.StartNew();
                target.Invoke(fibonacciCalculations, fibonacciSeed);
                //si.Stop();
                //Console.WriteLine("Time: " + si.ElapsedMilliseconds);
            }
            s1.Stop();
            var s2 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                //var si = Stopwatch.StartNew();
                target.Invoke(fibonacciCalculations, fibonacciSeed);
                //si.Stop();
                //Console.WriteLine("Time: " + si.ElapsedMilliseconds);
            }
            s2.Stop();
            var s3 = Stopwatch.StartNew();
            for (var i = 0; i < bmIterations; i++)
            {
                //var si = Stopwatch.StartNew();
                target.Invoke(fibonacciCalculations, fibonacciSeed);
                //si.Stop();
                //Console.WriteLine("Time: " + si.ElapsedMilliseconds);
            }
            s3.Stop();

            //var summary = string.Format("Summary, Loop1, {0}, Loop2, {1}, Loop3, {2}",
            //    s1.ElapsedMilliseconds,
            //    s2.ElapsedMilliseconds,
            //    s3.ElapsedMilliseconds);

            var summary = string.Format("{0},{1},{2},{3},BMI_{4},FSD_{5},FCA_{6}", 
                targetName, 
                s1.ElapsedMilliseconds, 
                s2.ElapsedMilliseconds,
                s3.ElapsedMilliseconds,
                bmIterations.ToString("00"),
                fibonacciSeed.ToString("00"),
                fibonacciCalculations.ToString("00"));

            BmLog(summary);

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

                //GTP.Init(_minThreads, _maxThreads, _idleTime);

                GTP.Init(minThreads: _minThreads,
                    maxThreads: _maxThreads,
                    idleTime: _idleTime,
                    disposeCallback: (Action)(() =>
                    {
                        Console.WriteLine("Summary: {0} Threads Created; {1} Threads Consumed; {2} Jobs Added; {3} Jobs Processed;",
                            GTP.TotalThreadsCreated, GTP.TotalThreadsUsed, GTP.TotalJobsAdded, GTP.TotalJobsProcessed);
                    }),
                    disposeCallbackParams: null);
            }
        }

        private static readonly string _fileName = string.Format("C:\\Log\\Benchmark_{0}.csv", DateTime.Today.ToString("yyyy-MM-dd"));
        public static void BmLog(string message)
        {
            File.AppendAllText(_fileName, "\n"); 
            File.AppendAllText(_fileName, message);
        }

        private static void ApplicationEnd()
        {
            GTP.Shutdown(true);
            //Utils.WaitLoggingToFinish();
        }

    }
}
