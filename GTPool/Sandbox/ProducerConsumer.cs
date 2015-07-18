using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace GTPool.Sandbox
{
    public class Exercise12
    {
        public static void Run()
        {
            var pman = new ThreadPoolManager(true);

            Func<string, string, string> job0 = (prase, wt) =>
            {
                Console.WriteLine(prase + " " + wt);
                return wt;
            };

            Action<string, string> job0Callback = (s, c) =>
            {
                Console.WriteLine(s + " " + c);
            };

            pman.AddJob(job0, new object[] { "Começando os trabalhos", string.Empty }, 
                job0Callback, new object[] { "Começando os trabalhos with a callback ", string.Empty });
            
            var cars = new [] 
            {
                //new object[] {"Fusca", 3000}, 
                //new object[] {"Corcel", 3000}, 
                //new object[] {"Maverick", 3000}, 
                //new object[] {"Opala", 3000},
                //new object[] {"Belina", 3000},
                //new object[] {"Cadilac", 3000}, 
                //new object[] {"Mustang", 3000}, 
                //new object[] {"Parati", 3000}, 
                //new object[] {"Gol", 3000},
                //new object[] {"Ferrari", 3000},
                //new object[] {"Fusca", 3000}, 
                //new object[] {"Corcel", 3000}, 
                //new object[] {"Maverick", 3000}, 
                //new object[] {"Opala", 3000},
                //new object[] {"Belina", 3000},
                //new object[] {"Cadilac", 3000}, 
                //new object[] {"Mustang", 3000}, 
                //new object[] {"Parati", 3000}, 
                //new object[] {"Fusca", 3000}, 
                //new object[] {"Corcel", 3000}, 
                //new object[] {"Maverick", 3000}, 
                //new object[] {"Opala", 3000},
                //new object[] {"Belina", 3000},
                //new object[] {"Cadilac", 3000}, 
                //new object[] {"Mustang", 3000}, 
                //new object[] {"Parati", 3000}, 
                //new object[] {"Fusca", 3000}, 
                //new object[] {"Corcel", 3000}, 
                //new object[] {"Maverick", 3000}, 
                //new object[] {"Opala", 3000},
                //new object[] {"Belina", 3000},
                //new object[] {"Cadilac", 3000}, 
                //new object[] {"Mustang", 3000}, 
                //new object[] {"Parati", 3000}, 
                //new object[] {"Gol", 3000},
                //new object[] {"Ferrari", 3000},
                //new object[] {"Fusca", 3000}, 
                //new object[] {"Corcel", 3000}, 
                //new object[] {"Maverick", 3000}, 
                //new object[] {"Opala", 3000},
                //new object[] {"Belina", 3000},
                //new object[] {"Cadilac", 3000}, 
                //new object[] {"Mustang", 3000}, 
                new object[] {"Parati", 3000}, 
                new object[] {"Fusca", 3000}, 
                new object[] {"Corcel", 3000}, 
                new object[] {"Maverick", 3000}, 
                new object[] {"Opala", 3000},
                new object[] {"Belina", 3000},
                new object[] {"Cadilac", 3000}, 
                new object[] {"Mustang", 3000}, 
                new object[] {"Parati", 3000}, 
                new object[] {"Fusca", 3000}, 
                new object[] {"Corcel", 3000}, 
                new object[] {"Maverick", 3000}, 
                new object[] {"Opala", 3000},
                new object[] {"Belina", 3000},
                new object[] {"Cadilac", 3000}, 
                new object[] {"Mustang", 3000}, 
                new object[] {"Parati", 3000}, 
                new object[] {"Gol", 3000},
                new object[] {"Ferrari", 3000},
                new object[] {"Fusca", 3000}, 
                new object[] {"Corcel", 3000}, 
                new object[] {"Maverick", 3000}, 
                new object[] {"Opala", 3000},
                new object[] {"Belina", 3000},
                new object[] {"Cadilac", 3000}, 
                new object[] {"Mustang", 3000}, 
                new object[] {"Parati", 3000}, 
                new object[] {"Fusca", 3000}, 
                new object[] {"Corcel", 3000}, 
                new object[] {"Maverick", 3000}, 
                new object[] {"Opala", 3000},
                new object[] {"Belina", 3000},
                new object[] {"Cadilac", 3000}, 
                new object[] {"Mustang", 3000}, 
                new object[] {"Parati", 3000}, 
                new object[] {"Gol", 3000}
            };

            Action<int, string> job = (c, wt) =>
            {
                Thread.Sleep((int)cars[c][1]);
                Console.WriteLine("My car is: " + cars[c][0] + " - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            for (var t = 0; t < cars.Count(); t++)
            {
                //Thread.Sleep(1000);
                pman.AddJob(job, new object[] { t, string.Empty });
            }

            Action<string> job2 = (wt) =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("I have got " + cars.Count() + " cars - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            pman.AddJob(job2, new object[] {string.Empty}, ThreadPriority.AboveNormal, false);


            Action<string> job4 = wt =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("My other car is a MERCEDES. - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            pman.AddJob(job4, new object[] { string.Empty }, ThreadPriority.Highest, true);

            pman.ExecuteEnqueuedJobs();

            //Thread.Sleep(30000);

            Action<string> job3 = (wt) =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("I have got an other car that is not in the list. :) - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            pman.AddJob(job3, new object[] { string.Empty }, ThreadPriority.Highest, false);

            pman.ExecuteEnqueuedJobs();
        }
    }
    
    public class ThreadPoolManager
    {
        public ThreadPoolManager()
            : this(false)
        { }

        public ThreadPoolManager(bool withWait)
        {
            _producerConsumer = new ThreadPool(withWait);
        }

        private readonly ThreadPool _producerConsumer;
        
        public void AddJob(Delegate job)
        {
            AddJob(job, null);
        }

        public void AddJob(Delegate job, object[] parameters)
        {
            AddJob(new ManagedJob(job, parameters));
        }

        public void AddJob(Delegate job, object[] parameters, ThreadPriority threadPriority, bool isBackground)
        {
            AddJob(new ManagedJob(job, parameters, threadPriority, isBackground));
        }

        public void AddJob(Delegate job, object[] parameters, Delegate callback, object[] callbackParameters)
        {
            AddJob(new ManagedJob(job, parameters, callback, callbackParameters));
        }

        public void AddJob(Delegate job, object[] parameters, Delegate callback, object[] callbackParameters, ThreadPriority threadPriority, bool isBackground)
        {
            AddJob(new ManagedJob(job, parameters, callback, callbackParameters, threadPriority, isBackground));
        }

        private void AddJob(ManagedJob job)
        {
            _producerConsumer.Produce(job);
        }

        public void ExecuteEnqueuedJobs()
        {
            _producerConsumer.StopWaiting();
        }
    }


    /// -------------------------------------------------------------------------------
    /// TODO: 
    /// 1. Create a job interface and class
    /// 2. Delay execution of a job (Synchronous)     -- Done
    /// 3. Group jobs 
    /// 4. Cancel job
    /// 5. Wait for all jobs to finish  -- Done
    /// 6. Callback function    -- Done
    /// 7. Configuration Initializer interface and class
    /// 8. Set job priority     -- Done
    /// 9. Add unit tests
    /// -------------------------------------------------------------------------------
    /// TODO: From the proposal
    /// 1. Dequeue tasks depending on the priority (Thread Prioriry)	-- done
    /// 2. Define an Interface to add tasks (ITask)	
    /// 3. Thread Pool Manager dequeue a task and execute       -- done
    /// 4. Make the Thread Pool static (singleton?)	
    /// 5. Define the size of the pool of threads               -- done
    /// 6. Allow changing the size of the pool of threads       -- done
    /// 7. Queue tasks in Thread Pool as closures               -- done
    /// 8. Add a callback function to the task                  -- done
    /// 9. Set priority to execute the task	                    -- done
    /// 10. Allow to abort the task if it hasn't been executed yet	
    /// 11. Benchmark the execution	
    /// 12. Unit test the Thread Pool	
    /// 13. Create a Demo Application	                        -- in progress
    public class ThreadPool
    {
        private readonly object _queueLocker = new object();
        private readonly object _variableLocker = new object();
        private readonly Queue<ManagedJob> _queueHighest = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueAboveNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueBelowNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueLowest = new Queue<ManagedJob>();
        private readonly int _minThreads;
        private const int DefaultMinThreads = 2;
        private readonly int _maxThreads;
        private const int DefaultMaxThreads = 50;
        private readonly int _idleTime;
        private bool _withWait;
        private int _threadId;
        private bool _isThreadCreation;
        private Dictionary<string, ManagedThread> _threads; 

        public ThreadPool()
            : this(false)
        { }

        public ThreadPool(bool asynchronous)
            : this(0, 50, 5000, asynchronous)
        { }

        public ThreadPool(int minThreads, int maxThreads, int idleTime, bool asynchronous)
        {
            _minThreads = minThreads > DefaultMaxThreads
                ? DefaultMaxThreads
                : minThreads < DefaultMinThreads ? DefaultMinThreads : minThreads;

            _maxThreads = maxThreads < DefaultMinThreads
                ? DefaultMinThreads
                : maxThreads > DefaultMaxThreads ? DefaultMaxThreads : maxThreads;

            _minThreads = _minThreads > _maxThreads ? _maxThreads : _minThreads;

            _idleTime = idleTime;
            _threadId = 0;
            _withWait = asynchronous;

            Threads = new Dictionary<string, ManagedThread>();
            LoadThreadQueue(_withWait ? _minThreads : _maxThreads);
        }

        public Dictionary<string, ManagedThread> Threads
        {
            get
            {
                lock (_queueLocker)
                {
                    return _threads;
                }
            }
            set
            {
                lock (_queueLocker)
                {
                    _threads = value;
                }
            }
        }

        public void StopWaiting()
        {
            if (!Waiting)
                return;

            Waiting = false;

            lock (_queueLocker)
            {
                Monitor.PulseAll(_queueLocker);
            }

            while (NumberOfRemainingJobs > 0 || Threads.Any(x => x.Value.Status == ManagedThreadStatus.Working))
            {
                Thread.Sleep(1);
            }

            Waiting = true;
        }

        public void Produce(ManagedJob job)
        {
            //HandleThreadsInitialization();

            switch (job.ThreadPriority)
            {
                case ThreadPriority.Highest:
                    AddJobToQueue(_queueHighest, job);
                    break;
                case ThreadPriority.AboveNormal:
                    AddJobToQueue(_queueAboveNormal, job);
                    break;
                case ThreadPriority.BelowNormal:
                    AddJobToQueue(_queueBelowNormal, job);
                    break;
                case ThreadPriority.Lowest:
                    AddJobToQueue(_queueLowest, job);
                    break;
                default:
                    AddJobToQueue(_queueNormal, job);
                    break;
            }
        }

        private void AddJobToQueue(Queue<ManagedJob> queue, ManagedJob job)
        {
            lock (_queueLocker)
            {
                queue.Enqueue(job);

                if (!Waiting)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }
        }
        
        private void LoadThreadQueue(int numberOfThreads, string wt)
        {
            Console.WriteLine("LoadThreadQueue - WT " + wt + " : " + HiResDateTime.UtcNow);
            LoadThreadQueue(numberOfThreads);
        }

        private void LoadThreadQueue(int numberOfThreads)
        {
            IsThreadCreation = true;

            while (Threads.Count < numberOfThreads)
            {
                var threadName = NextThreadName;

                Threads.Add(threadName, new ManagedThread(new Thread(JobInvoker)
                {
                    Name = threadName,
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                }));

                Console.WriteLine("Thread created " + threadName + " : " + HiResDateTime.UtcNow);
                Threads[threadName].Start(threadName);
            }

            IsThreadCreation = false;
        }

        private void JobInvoker(object threadName)
        {
            var tname = threadName.ToString();

            while (true)
            {
                var job = Consume(tname);

                if (job != null)
                {
                    if (Threads.ContainsKey(tname))
                        Threads[tname].ExecuteJob(job);
                }
                else
                {
                    if (NumberOfRemainingJobs == 0 && !IsMinNumberOfActiveThreads
                        && Threads[tname].Status == ManagedThreadStatus.Retired)
                    {
                        break;
                    }
                }
            }

            Threads.Remove(tname);
            Console.WriteLine("Thread ended " + tname + " : " + HiResDateTime.UtcNow);
        }

        private ManagedJob Consume(string threadName)
        {

            // TODO: STILL it doesn't work!
            lock (_variableLocker)
            {
                if (!_isThreadCreation)
                {
                    _isThreadCreation = true;

                    ManagedJob job;
                    if (InitializeNewThreads(out job))
                    {
                        return job;
                    }

                    _isThreadCreation = false;
                }
            }

            lock (_queueLocker)
            {
                if (!Waiting)
                {
                    if (_queueHighest.Count > 0)
                        return _queueHighest.Dequeue();
                    
                    if (_queueAboveNormal.Count > 0)
                        return _queueAboveNormal.Dequeue();

                    if (_queueNormal.Count > 0)
                        return _queueNormal.Dequeue();

                    if (_queueBelowNormal.Count > 0)
                        return _queueBelowNormal.Dequeue();

                    if (_queueLowest.Count > 0)
                        return _queueLowest.Dequeue();
                }
                
                Threads[threadName].Wait(_queueLocker, _idleTime);
            }

            return null;
        }

        // TODO: FIX THIS 
        private bool InitializeNewThreads(out ManagedJob job)
        {
            lock (_queueLocker)
            {
                job = null;

                var isThereJobs = _queueHighest.Count + _queueAboveNormal.Count + _queueNormal.Count +
                                  _queueBelowNormal.Count + _queueLowest.Count > 0;

                if (_withWait || !isThereJobs)
                    return false;

                if (_threads.Any(x => x.Value.Status == ManagedThreadStatus.Waiting ||
                                      x.Value.Status == ManagedThreadStatus.Retired))
                {
                    Monitor.Pulse(_queueLocker);
                }
                else
                {
                    if (_threads.Count(x => x.Value.Status == ManagedThreadStatus.Ready) < NumberOfRemainingJobs)
                    {
                        var threadsNeeded =
                            Math.Min((int) Math.Round(_threads.Count*1.5, 0, MidpointRounding.AwayFromZero), _maxThreads);

                        job = new ManagedJob((Action<int, string>) LoadThreadQueue,
                            new object[] {threadsNeeded, string.Empty}, ThreadPriority.Highest, true);
                    }
                }

                return job != null;
            }

            //var threadsNeeded = Math.Min((int)Math.Round(Threads.Count * 1.5, 0, MidpointRounding.AwayFromZero), _maxThreads);

            //if (NumberOfRemainingJobs > Threads.Count(x => x.Value.Status == ManagedThreadStatus.Waiting))
            //{
            //    IsThreadCreation = true;    // TODO: Remove this!

            //    lock (_threadLocker)
            //    {
            //        _queueThread.Enqueue(new ManagedJob((Action<int, string>)LoadThreadQueue,
            //            new object[] { threadsNeeded, string.Empty }, ThreadPriority.Highest, true));

            //        //Monitor.Pulse(_threadLocker);
            //    }
            //}
        }

        #region Private Properties

        private bool Waiting
        {
            get
            {
                lock (_queueLocker)
                {
                    return _withWait;
                }
            }
            set
            {
                lock (_queueLocker)
                {
                    _withWait = value;
                }
            }
        }

        private int NumberOfRemainingJobs
        {
            get
            {
                lock (_queueLocker)
                {
                    return _queueHighest.Count + _queueAboveNormal.Count + _queueNormal.Count + 
                           _queueBelowNormal.Count + _queueLowest.Count;
                }
            }
        }

        private string NextThreadName
        {
            get
            {
                lock (_variableLocker)
                {
                    _threadId++;
                    return "__thread_" + _threadId.ToString("D2") + "__";
                }
            }
        }

        private bool IsThreadCreation
        {
            get
            {
                lock (_variableLocker)
                {
                    return _isThreadCreation;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _isThreadCreation = value;
                }
            }
        }

        private bool IsMinNumberOfActiveThreads
        {
            get { return Threads.Count == _minThreads; }
        }

        #endregion
    }

    public class ManagedJob
    {
        public ManagedJob(Delegate work, object[] parameters)
            : this(work, parameters, null, null)
        { }

        public ManagedJob(Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : this(work, parameters, null, null, threadPriority, isBackground)
        { }

        public ManagedJob(Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters)
            : this(work, parameters, callBack, callBackParameters, ThreadPriority.Normal, true)
        { }

        public ManagedJob(Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters, 
            ThreadPriority threadPriority, bool isBackground)
        {
            ThreadPriority = threadPriority;
            IsBackground = isBackground;

            _work = work;
            _parameters = parameters;
            _callback = callBack;
            _callbackParameters = callBackParameters;
        }

        private readonly Delegate _work;
        private object[] _parameters;
        private readonly Delegate _callback;
        private object[] _callbackParameters;

        public ThreadPriority ThreadPriority { get; private set; }

        public bool IsBackground { get; private set; }



        public void DoWork(string threadName)
        {
            if (_parameters == null)
                _parameters = new object[] {threadName};
            else
                _parameters[_parameters.Length - 1] = threadName;

            var result = _work.DynamicInvoke(_parameters);

            if (_callback != null)
            {
                if (result != null)
                {
                    if (_callbackParameters == null)
                        _callbackParameters = new[] {result};
                    else
                        _callbackParameters[_callbackParameters.Length - 1] = result;
                }

                _callback.DynamicInvoke(_callbackParameters);
            }
        }
    }

    public class ManagedThread
    {
        private const int Timeout = 30000;
        private const int MaxIdleLifeCycles = 4;
        private const int StartIdleLifeCycles = 0;
        private readonly Thread _instance;
        private readonly bool _defaultIsBackground;
        private readonly ThreadPriority _defaultThreadPriority;
        private int _idleLifeCycles;
        
        public ManagedThread(Thread thread)
        {
            _instance = thread;
            _defaultThreadPriority = thread.Priority;
            _defaultIsBackground = thread.IsBackground;

            Name = thread.Name;
            Status = ManagedThreadStatus.NotStarted;
        }

        public string Name { get; private set; }
        public ManagedThreadStatus Status { get; private set; }

        public void Start()
        {
            Start(null);
        }

        public void Start(object param)
        {
            Status = ManagedThreadStatus.Ready;
            _idleLifeCycles = StartIdleLifeCycles;
            _instance.Start(param);
        }

        public void Wait(object queuelock)
        {
            _idleLifeCycles = MaxIdleLifeCycles;
            Wait(queuelock, Timeout);
        }

        public void Wait(object queuelock, int idleTime)
        {
            Status = ManagedThreadStatus.Waiting;

            if (Monitor.Wait(queuelock, idleTime))
                _idleLifeCycles = StartIdleLifeCycles;

            _idleLifeCycles++;

            Status = _idleLifeCycles <= MaxIdleLifeCycles 
                ? ManagedThreadStatus.Ready 
                : ManagedThreadStatus.Retired;
        }

        public void ExecuteJob(ManagedJob job)
        {
            _instance.IsBackground = job.IsBackground;
            _instance.Priority = job.ThreadPriority;

            Status = ManagedThreadStatus.Working;
         
            job.DoWork(Name);

            _instance.IsBackground = _defaultIsBackground;
            _instance.Priority = _defaultThreadPriority;
            
            Status = ManagedThreadStatus.Ready;
        }
    }

    public enum ManagedThreadStatus
    {
        Retired,
        Waiting,
        Ready,
        Working,
        NotStarted
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
