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
            var cars = new [] 
            {
                new object[] {"Fusca", 6000}, 
                new object[] {"Corcel", 7000}, 
                new object[] {"Maverick", 1000}, 
                new object[] {"Opala", 4000},
                new object[] {"Belina", 3000},
                new object[] {"Cadilac", 6000}, 
                new object[] {"Mustang", 7000}, 
                new object[] {"Parati", 1000}, 
                new object[] {"Gol", 4000},
                new object[] {"Ferrari", 3000}
            };

            Action<int, string> job = (c, wt) =>
            {
                Thread.Sleep((int)cars[c][1]);
                Console.WriteLine("My car is: " + cars[c][0] + " - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            var pman = new ThreadPoolManager(false);
            for (var t = 0; t < cars.Count(); t++)
            {
                //Thread.Sleep(1000);
                pman.AddJob(job, new object[] { t, string.Empty });
            }

            Action<string> job2 = (wt) =>
            {
                Console.WriteLine("I have got " + cars.Count() + " cars - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            pman.AddJob(job2, new object[] {string.Empty});


            Action<string> job4 = wt =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("My other car is a MERCEDES. - WT: " + wt + " : " + HiResDateTime.UtcNow);
            };

            pman.AddJob(job4, new object[] { string.Empty });

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
    /// 6. Callback function    
    /// 7. Configuration Initializer interface and class
    /// 8. Set job priority     -- Done
    /// 9. 
    public class ThreadPool
    {
        private readonly object _queueLock = new object();
        private readonly object _counterLock = new object();
        private readonly Queue<ManagedJob> _queue = new Queue<ManagedJob>();
        private readonly int _minThreads;
        private const int DefaultMinThreads = 2;
        private readonly int _maxThreads;
        private const int DefaultMaxThreads = 25;
        private readonly int _idleTime;
        private bool _withWait;
        private int _threadId;
        private bool _isThreadCreation;
        private Dictionary<string, ManagedThread> _threads; 

        public ThreadPool()
            : this(false)
        { }

        public ThreadPool(bool synchronous)
            : this(0, 10, 15000, synchronous)
        { }

        public ThreadPool(int minThreads, int maxThreads, int idleTime, bool synchronous)
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
            _withWait = synchronous;

            Threads = new Dictionary<string, ManagedThread>();
            LoadThreadQueue(_withWait ? _minThreads : _maxThreads);
        }

        public Dictionary<string, ManagedThread> Threads
        {
            get
            {
                lock (_queueLock)
                {
                    return _threads;
                }
            }
            set
            {
                lock (_queueLock)
                {
                    _threads = value;
                }
            }
        }

        public void StopWaiting()
        {
            if (!WithWait)
                return;

            WithWait = false;

            lock (_queueLock)
            {
                for(var i = 0; i < _queue.Count; i++)
                {
                    Monitor.Pulse(_queueLock);
                }
            }

            while (IsThereAnyJob || Threads.Any(x => x.Value.Status == ManagedThreadStatus.Running))
            {
                Thread.Sleep(1);
            }

            WithWait = true;
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
            while (true)
            {
                var job = Consume(threadName.ToString());

                if (job != null)
                {
                    var thread = Threads[threadName.ToString()].SafeThread;
                    thread.IsBackground = job.IsBackground;
                    thread.Priority = job.ThreadPriority;
                    
                    //Console.WriteLine("Working Thread " + threadName);
                    job.Parameters[job.Parameters.Length - 1] = threadName;
                    job.Target.DynamicInvoke(job.Parameters);

                    thread.IsBackground = true;
                    thread.Priority = ThreadPriority.Normal;
                }
                else
                {
                    //if (!IsMinNumberOfActiveThreads && !IsThreadCreation && !WithWait && !IsThereJob)
                    if (Threads[threadName.ToString()].Status == ManagedThreadStatus.Terminated)
                        break;
                }
            }

            Threads.Remove(threadName.ToString());
            Console.WriteLine("Thread ended " + threadName + " : " + HiResDateTime.UtcNow);
        }

        private ManagedJob ShouldCreateMoreThreads()
        {
            // TODO: Must check if all threads are busy and number of active threads are smaller than max number of threads
            // TODO: then create threads on demand
            if (IsMinNumberOfActiveThreads && IsThereAnyJob && !IsThreadCreation)
            {
                IsThreadCreation = true;

                //var numberOfThreads = Math.Min((int)Math.Round(_minThreads * 2.5, 0, MidpointRounding.AwayFromZero), _maxThreads);
                var numberOfThreads = _maxThreads;

                return new ManagedJob(
                    (Action<int, string>)LoadThreadQueue, new object[] { numberOfThreads, string.Empty });
            }

            return null;
        }

        public void Produce(ManagedJob job)
        {
            var createThreads = ShouldCreateMoreThreads();

            lock (_queueLock)
            {
                if (createThreads != null)
                {
                    _queue.Enqueue(createThreads);
                    Monitor.Pulse(_queueLock);
                }

                _queue.Enqueue(job);

                if (!WithWait)
                {
                    Monitor.Pulse(_queueLock);
                }
            }
        }

        private ManagedJob Consume(string threadName)
        {
            lock (_queueLock)
            {
                //TODO: Continue from here

                //_queue.Where(x => x.)
                if (_queue.Count > 0 && !WithWait)
                {
                    return _queue.Dequeue();
                }

                Threads[threadName].Wait(_queueLock, _idleTime);
            }

            return null;
        }

        #region Private Properties

        private bool WithWait
        {
            get
            {
                lock (_queueLock)
                {
                    return _withWait;
                }
            }
            set
            {
                lock (_queueLock)
                {
                    _withWait = value;
                }
            }
        }

        private bool IsThereAnyJob
        {
            get
            {
                lock (_queueLock)
                {
                    return _queue.Count > 0;
                }
            }
        }

        private string NextThreadName
        {
            get
            {
                lock (_counterLock)
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
                lock (_counterLock)
                {
                    return _isThreadCreation;
                }
            }
            set
            {
                lock (_counterLock)
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
        public ManagedJob(Delegate target, object[] parameters)
            : this(target, parameters, ThreadPriority.Normal, true)
        { }

        public ManagedJob(Delegate target, object[] parameters, 
            ThreadPriority threadPriority, bool isBackground)
        {
            ThreadPriority = threadPriority;
            IsBackground = isBackground;
            Target = target;
            Parameters = parameters;
        }

        public Delegate Target { get; private set; }

        public object[] Parameters { get; private set; }

        public ThreadPriority ThreadPriority { get; private set; }

        public bool IsBackground { get; private set; }
    }

    public class ManagedThread
    {
        private const int Timeout = 30000;
        private const int MaxIdleLifeCycles = 3;
        private const int StartIdleLifeCycles = 1;
        private int _idleLifeCycles;
        
        public ManagedThread(Thread thread)
        {
            SafeThread = thread;
        }

        public Thread SafeThread { get; private set; }
        public ManagedThreadStatus Status { get; private set; }

        public void Start()
        {
            Status = ManagedThreadStatus.Running;
            _idleLifeCycles = StartIdleLifeCycles;
            SafeThread.Start();
        }

        public void Start(object param)
        {
            Status = ManagedThreadStatus.Running;
            _idleLifeCycles = StartIdleLifeCycles;
            SafeThread.Start(param);
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
                ? ManagedThreadStatus.Running 
                : ManagedThreadStatus.Terminated;
        }
    }

    public enum ManagedThreadStatus
    {
        Terminated,
        Waiting,
        Running
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
