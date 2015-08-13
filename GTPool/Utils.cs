using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GTPool
{
    public class Utils
    {
        #region Log

        private static readonly object _locker = new object();
        private static readonly Queue<string> _logMessageQueue = new Queue<string>();
        private static readonly string _fileName = string.Format(LogPath, DateTime.Today.ToString("yyyy-MM-dd"));
        private static Thread _logger;
        private static bool _stopLogger;
        private static bool _forceLogging;

        public static void Log(string message)
        {
            if (IsDebug)
            {
                if (!_stopLogger)
                {
                    Log(message, false);
                }
            }
        }

        public static void Log(string message, bool forceLogging)
        {
            if (IsDebug)
            {
                _forceLogging = forceLogging;

                if (!_stopLogger || _forceLogging)
                {
                    var thread = Thread.CurrentThread;
                    var threadId = thread.ManagedThreadId.ToString();
                    var logMessage = string.Format("------------------| {0} | {1} | {2} ",
                        HiResDateTime.UtcNow,
                        thread.Name ?? threadId.PadLeft(16 - threadId.Length, ' '),
                        message);

#if DEBUG
                    Trace.WriteLine(logMessage);
#endif
                    AddLogMessage(logMessage);
                    StartLogger();
                }
            }
        }

        public static void StartLogging()
        {
            _stopLogger = false;
        }

        public static void StopLogging()
        {
            _stopLogger = true;
        }

        public static void WaitLoggingToFinish()
        {
            if (IsDebug)
            {
                var any = false;
                while (true)
                {
                    lock (_locker)
                    {
                        if (!_logMessageQueue.Any())
                        {
                            break;
                        }

                        any = true;
                        Thread.Sleep(1);
                    }
                }

                if (any)
                    Thread.Sleep(3000);

                _stopLogger = true;
                _forceLogging = false;
            }
        }

        private static void AddLogMessage(string logMessage)
        {
            new Thread((() =>
            {
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
            while (!_stopLogger || _forceLogging)
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

        #endregion

        #region App Settings

        private static string _logPath;
        public static string LogPath
        {
            get
            {
                if (string.IsNullOrEmpty(_logPath))
                {
                    _logPath = TryGetAppSetting("LogPath", @"C:\Log\GTPool_{0}.txt");
                }

                return _logPath;
            }
        }

        private static bool? _isDebug;
        public static bool IsDebug
        {
            get
            {
                if (_isDebug == null)
                {
                    _isDebug = bool.Parse(TryGetAppSetting("GtpDebug", "false"));
                }

                return _isDebug.Value;
            }
        }

        private static string TryGetAppSetting(string key, string defaultValue)
        {
            string retVal;
            var reader = new AppSettingsReader();

            try
            {
                retVal = (string)reader.GetValue(key, typeof(string));
            }
            catch (InvalidOperationException e)
            {
                retVal = defaultValue;
            }

            return retVal;
        }

        #endregion

        #region Random Number

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

        #endregion

        #region High Resolution Date Time

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

        #endregion
    }
}
