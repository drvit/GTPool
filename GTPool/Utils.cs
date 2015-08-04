using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GTPool
{
    public class Utils
    {
        private static readonly object _locker = new object();
        private static readonly Queue<string> _logMessageQueue = new Queue<string>();
        private static readonly string _fileName = string.Format(@"C:\Log\GTPool_{0}.txt", DateTime.Today.ToString("yyyy-MM-dd"));
        private static Thread _logger;
        private static bool _stopLogger;

        public static void Log(string message)
        {
            var thread = Thread.CurrentThread;
            var threadId = thread.ManagedThreadId.ToString();
            var logMessage = string.Format("------------------| {0} | {1} | {2} ",
                HiResDateTime.UtcNow,
                thread.Name ?? threadId.PadLeft(16 - threadId.Length, ' '),
                message);

#if DEBUG
            Trace.WriteLine(logMessage);
#else
            AddLogMessage(logMessage);
            StartLogger();
#endif
        }

        public static void LoggerStop()
        {
            _stopLogger = true;
        }

        public static void LoggerWaitToFinish()
        {
            while (true)
            {
                lock (_locker)
                {
                    if (!_logMessageQueue.Any())
                        break;

                    Thread.Sleep(1);
                }
            }

            Thread.Sleep(3000);
        }

        private static void AddLogMessage(string logMessage)
        {
            new Thread((() =>
            {
                _stopLogger = false;

                lock (_locker)
                {
                    _logMessageQueue.Enqueue(logMessage);
                }
            })).Start();
        }

        // very slow to add the message...
        //static async void AddLogMessage(string logMessage)
        //{
        //    await Task.Run(() =>
        //    {
        //        _stopLogger = false;

        //        lock (_locker)
        //        {
        //            _logMessageQueue.Enqueue(logMessage);
        //        }
        //    });
        //}

        private static void StartLogger()
        {
            if (_logger == null)
            {
                _logger = new Thread(LogMessages)
                {
                    Name = "__MsgLogger__",
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };

                _logger.Start();
            }
            else
            {
                lock (_locker)
                {
                    Monitor.Pulse(_locker);
                }
            }
        }

        private static void LogMessages()
        {
            const string dummy = "!##^log?msg~##!";
            while (!_stopLogger)
            {
                var logmsg = dummy;
                lock (_locker)
                {
                    if (_logMessageQueue.Any())
                    {
                        logmsg = _logMessageQueue.Dequeue();
                    }
                    else
                    {
                        Monitor.Wait(_locker, 3000);
                    }
                }

                if (logmsg != dummy)
                {
                    File.AppendAllText(_fileName, logmsg + "\n");

                    //using (var tw = File.CreateText(@"C:\Log\GTPool.txt"))
                    //{
                    //    tw.WriteLine(logmsg);
                    //}
                }
            }
        }


        private static readonly Random _random = new Random();
        private static readonly HashSet<int> _randomList = new HashSet<int>();
        public static int GenerateUniqueNumber()
        {
            int newNumber;

            do
            {
                newNumber = _random.Next(1000, 999999);
            } while (_randomList.Contains(newNumber));

            _randomList.Add(newNumber);

            return newNumber;
        }

        public class HiResDateTime
        {
            private static long _lastTimeStamp = DateTime.UtcNow.Ticks;

            public static long UtcNowTicks
            {
                get
                {
                    long orig, newval;
                    do
                    {
                        orig = _lastTimeStamp;
                        var now = DateTime.UtcNow.Ticks;
                        newval = Math.Max(now, orig + 1);
                    } while (Interlocked.CompareExchange
                        (ref _lastTimeStamp, newval, orig) != orig);

                    return newval;
                }
            }

            public static string UtcNow
            {
                get
                {
                    //return new DateTime(newval, DateTimeKind.Utc).ToString("o");
                    return new DateTime(UtcNowTicks, DateTimeKind.Utc).ToString("HH:mm.ss:fff");
                }
            }

        }
    }
}
