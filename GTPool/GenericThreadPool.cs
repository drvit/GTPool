using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GTPool
{
    /// -------------------------------------------------------------------------------
    /// TODO: From the proposal
    /// 1. Dequeue tasks depending on the priority (Thread Prioriry)	-- done
    /// 2. Define an Interface to add tasks (ITask)	            -- done (not an interface, but a class)
    /// 3. Thread Pool Manager dequeue a task and execute       -- done
    /// 4. Make the Thread Pool static (singleton?)	            -- done
    /// 5. Define the size of the pool of threads               -- done
    /// 6. Allow changing the size of the pool of threads       -- done (can change only on Init())
    /// 7. Queue tasks in Thread Pool as closures               -- done
    /// 8. Add a callback function to the task                  -- done
    /// 9. Set priority to execute the task	                    -- done
    /// 10. Allow to abort the task if it hasn't been executed	-- done
    /// 11. Benchmark the execution	                            -- in progress
    /// 12. Unit test the Thread Pool	                        -- done
    /// 13. Create a Demo Application	                        -- in progress
    /// 14. BONUS: On Error Callback method                     -- done
    /// 15. BONUS: Cancell All Tasks (Jobs)                     -- done
    /// 16. BONUS: Callback method on GTP Dispose               -- done
    /// 17. BONUS: GTP also works in a Synchronous mode         -- done (two modes, sync or async, one has to be disposed before initializing the other)
    public sealed class GenericThreadPool : IDisposable
    {
        #region Private Variables

        private static GenericThreadPool _current = new GenericThreadPool();
        private ConcurrentDictionary<string, ManagedThread> _threads;
        private readonly object _queueLocker = new object();
        private readonly object _variableLocker = new object();
        private readonly Queue<ManagedJob> _queueHighest = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueAboveNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueBelowNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueLowest = new Queue<ManagedJob>();
        private HashSet<int> _ignoredJobs = new HashSet<int>();
        private int _threadId;
        private int _jobsCount;
        private bool _waiting;
        private bool _disposingThreads;
        private bool _creatingThreads;
        private bool _stopThreadCreation;
        private int _busyThreadsCount;
        private int _waitingThreadsCount;

        #endregion

        #region Constructors
        
        static GenericThreadPool() { }

        private GenericThreadPool() { }

        #endregion

        #region Public Properties

        public static GenericThreadPool Current { get { return _current; } }
        public GenericThreadPoolSettings Settings { get; private set; }
        public GenericThreadPoolMode GtpMode { get; private set; }

        #endregion

        #region Initialize Generic Thread Pool

        public static GenericThreadPool Init()
        {
            return Init<GtpAsync>(new GenericThreadPoolSettings(), null, null);
        }

        public static GenericThreadPool Init<TMode>(int numberOfThreads, int idleTime)
            where TMode : GtpSync, new()
        {
            return Init<TMode>(new GenericThreadPoolSettings(numberOfThreads, idleTime), null, null);
        }

        public static GenericThreadPool Init<TMode>(int numberOfThreads, int idleTime,
            Delegate disposeCallback, object[] disposeCallbackParams)
            where TMode : GtpSync, new()
        {
            return Init<TMode>(new GenericThreadPoolSettings(numberOfThreads, idleTime), 
                disposeCallback, disposeCallbackParams);
        }

        public static GenericThreadPool Init<TMode>(int minThreads, int maxThreads, int idleTime)
            where TMode : GtpAsync, new()
        {
            return Init<TMode>(new GenericThreadPoolSettings(minThreads, maxThreads, idleTime), null, null);
        }

        public static GenericThreadPool Init<TMode>(int minThreads, int maxThreads, int idleTime,
            Delegate disposeCallback, object[] disposeCallbackParams)
            where TMode : GtpAsync, new()
        {
            return Init<TMode>(new GenericThreadPoolSettings(minThreads, maxThreads, idleTime), 
                disposeCallback, disposeCallbackParams);
        }

        private static GenericThreadPool Init<TMode>(GenericThreadPoolSettings settings, 
            Delegate disposeCallback, object[] disposeCallbackParams)
            where TMode : GenericThreadPoolMode, new()
        {
            Utils.Log("###############################################");
            Utils.Log("Generic Thread Pool Initialization");

            InitializeInstance<TMode>(settings, disposeCallback, disposeCallbackParams);
            _current.LoadThreadQueue();

            return _current;
        }

        private static void InitializeInstance<TMode>(GenericThreadPoolSettings settings,
            Delegate disposeCallback, object[] disposeCallbackParams)
            where TMode : GenericThreadPoolMode, new()
        {
            if (_current == null)
            {
                _current = new GenericThreadPool { DisposingThreads = false };
            }

            if (_current.GtpMode == null)
            {
                _current.GtpMode = new TMode
                {
                    DisposeCallback = disposeCallback,
                    DisposeCallbackParams = disposeCallbackParams
                };

                _current.Waiting = _current.GtpMode.WithWait;
            }
            else
            {
                if (typeof(TMode) != _current.GtpMode.GetType())
                    throw new GenericThreadPoolException(
                        GenericThreadPoolExceptionType.IncompatibleGtpMode);
            }

            if (_current.Settings == null)
            {
                _current.Settings = settings;
            }

            if (_current._threads == null)
            {
                var numProcs = Environment.ProcessorCount;
                var concurrencyLevel = numProcs * 2;

                _current._threads = new ConcurrentDictionary<string, ManagedThread>(
                    concurrencyLevel, _current.Settings.MaxThreads);
            }

            if (_current._ignoredJobs == null)
            {
                _current._ignoredJobs = new HashSet<int>();
            }

            _current.StopThreadCreation = false;
        }

        #endregion

        #region Handle Managed Threads

        private void LoadThreadQueue()
        {
            LoadThreadQueue(!Waiting ? Settings.MinThreads : Settings.MaxThreads);
        }

        private void LoadThreadQueue(int numberOfThreads)
        {
            numberOfThreads = Math.Min(Settings.MaxThreads, numberOfThreads);

            Utils.Log(string.Format("====== Creating {0} threads ======", numberOfThreads));

            while (_threads.Count < numberOfThreads)
            {
                if (StopThreadCreation) break;

                var threadName = NextThreadName;

                _threads.TryAdd(threadName, new ManagedThread(new Thread(JobInvoker)
                {
                    Name = threadName,
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                }));

                _threads[threadName].Start();

                lock (_variableLocker)
                {
                    _busyThreadsCount++;
                }

                Utils.Log(string.Format("<<<<< Thread {0} created >>>>>>", threadName));
            }
        }

        private void JobInvoker()
        {
            var tname = CurrentThreadName;

            while (true)
            {
                var job = DequeueJob();

                if (job != null)
                {
                    _threads[tname].ExecuteJob(job);
                }
                else
                {
                    if (JobsCount == 0 &&
                        (DisposingThreads || _threads.Count != Settings.MinThreads) &&
                        _threads[tname].Status == ManagedThreadStatus.Retired)
                    {
                        lock (_variableLocker)
                        {
                            _waitingThreadsCount--;
                        }

                        break;
                    }
                }
            }

            ManagedThread mt;
            _threads.TryRemove(tname, out mt);

            Utils.Log(">>>>>> Thread destroyed <<<<<<");
        }

        #endregion

        #region Enqueue Managed Job

        public static void AddJob(ManagedAsyncJob job)
        {
            ValidateAsyncJob(job);

            _current.AddJobToPriorityQueue(job);
        }

        public void AddJob(ManagedSyncJob job)
        {
            ValidateSyncJob(job);

            AddJobToPriorityQueue(job);
        }

        private void AddJobToPriorityQueue(ManagedJob job)
        {
            switch (job.ThreadPriority)
            {
                case ThreadPriority.Highest:
                    EnqueueJob(_queueHighest, job);
                    break;
                case ThreadPriority.AboveNormal:
                    EnqueueJob(_queueAboveNormal, job);
                    break;
                case ThreadPriority.BelowNormal:
                    EnqueueJob(_queueBelowNormal, job);
                    break;
                case ThreadPriority.Lowest:
                    EnqueueJob(_queueLowest, job);
                    break;
                default:
                    EnqueueJob(_queueNormal, job);
                    break;
            }
        }

        private void EnqueueJob(Queue<ManagedJob> queue, ManagedJob job)
        {
            lock (_queueLocker)
            {
                StopThreadCreation = false;
                JobsCount++;

                queue.Enqueue(job);
                Utils.Log(string.Format("++++++ Job added to GTP JobId: {0} ++++++", job.JobId));

                if (!Waiting)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }
        }

        #endregion

        #region Dequeue Managed Job

        private ManagedJob DequeueJob()
        {
            BaunceThreadPool();

            lock (_queueLocker)
            {
                if (!Waiting)
                {
                    ManagedJob job = null;

                    if (_queueHighest.Any())
                        job = _queueHighest.Dequeue();

                    else if (_queueAboveNormal.Any())
                        job = _queueAboveNormal.Dequeue();

                    else if (_queueNormal.Any())
                        job = _queueNormal.Dequeue();

                    else if (_queueBelowNormal.Any())
                        job = _queueBelowNormal.Dequeue();

                    else if (_queueLowest.Any())
                        job = _queueLowest.Dequeue();

                    if (job != null)
                    {
                        JobsCount--;

                        if (_ignoredJobs.Contains(job.JobId))
                        {
                            _ignoredJobs.Remove(job.JobId);
                            Utils.Log(string.Format("------ Ignored Job {0} ------", job.JobId));
                            return null;
                        }

                        return job;
                    }

                    _ignoredJobs.Clear();
                    StopThreadCreation = true;
                }
            }

            lock (_variableLocker)
            {
                _busyThreadsCount--;
                _waitingThreadsCount++;
            }

            _threads[CurrentThreadName].Wait(_queueLocker, Settings.IdleTime, Waiting);

            lock (_variableLocker)
            {
                _busyThreadsCount++;
                _waitingThreadsCount--;
            }

            return null;
        }

        private void BaunceThreadPool()
        {
            if (CreatingThreads)
                return;

            bool areThereMoreJobsThanThreads;
            bool areThereWaitingThreads;

            lock (_variableLocker)
            {
                areThereMoreJobsThanThreads = _jobsCount > _busyThreadsCount;
                areThereWaitingThreads = _waitingThreadsCount > 0;
            }

            if (areThereMoreJobsThanThreads)
            {
                if (areThereWaitingThreads)
                {
                    lock (_queueLocker)
                    {
                        Monitor.PulseAll(_queueLocker);
                        Utils.Log("(((((( PulseAll ))))))");
                    }
                }
                else if (!StopThreadCreation)
                {
                    LoadThreadQueue(JobsCount);
                }
            }

            CreatingThreads = false;
        }

        #endregion

        #region Cancel Managed Jobs

        public static void CancelJob(ManagedAsyncJob job)
        {
            ValidateAsyncJob(job);

            if (job.JobId <= 0)
                return;

            lock (_current._queueLocker)
            {
                if (!_current._ignoredJobs.Contains(job.JobId))
                    _current._ignoredJobs.Add(job.JobId);
            }
        }

        public static void CancellAllJobs()
        {
            lock (_current._queueLocker)
            {
                _current._queueHighest.Clear();
                _current._queueAboveNormal.Clear();
                _current._queueNormal.Clear();
                _current._queueBelowNormal.Clear();
                _current._queueLowest.Clear();
                _current.JobsCount = 0;
            }
        }

        #endregion

        #region Private Properties

        private bool StopThreadCreation
        {
            get
            {
                lock (_variableLocker)
                {
                    return _stopThreadCreation;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _stopThreadCreation = value;
                }
            }
        }

        private static string CurrentThreadName
        {
            get { return Thread.CurrentThread.Name ?? string.Empty; }
        }

        private string NextThreadName
        {
            get
            {
                lock (_variableLocker)
                {
                    _threadId++;
                    return "__thread_" + _threadId.ToString("D3") + "__";
                }
            }
        }

        private bool Waiting
        {
            get
            {
                lock (_variableLocker)
                {
                    return GtpMode.WithWait && _waiting;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _waiting = value;
                }
            }
        }

        private int JobsCount
        {
            get
            {
                lock (_variableLocker)
                {
                    return _jobsCount;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _jobsCount = value;
                }
            }
        }

        private bool DisposingThreads
        {
            get
            {
                lock (_variableLocker)
                {
                    return _disposingThreads;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _disposingThreads = value;
                }
            }
        }

        private bool CreatingThreads
        {
            get
            {
                lock (_variableLocker)
                {
                    if (_creatingThreads == false)
                    {
                        _creatingThreads = true;
                        return false;
                    }

                    return true;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _creatingThreads = value;
                }
            }
        }

        #endregion

        #region Managed Job Validation

        private static void ValidateAsyncJob(ManagedAsyncJob job)
        {
            if (job == null)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.JobIsNull);

            if (_current == null)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.InstanceIsDisposed);

            if (_current.GtpMode.WithWait != job.GtpMode.WithWait)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);

            if (_current.Settings == null)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.SettingsNotInitialized);
        }

        private void ValidateSyncJob(ManagedSyncJob job)
        {
            if (job == null)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.JobIsNull);

            if (GtpMode.WithWait != job.GtpMode.WithWait)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);
        }

        #endregion

        #region IDisposable

        public static void End()
        {
            if (_current == null)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.InstanceIsDisposed);

            if (_current.GtpMode.WithWait)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);

            _current.InternalDispose();
        }

        public void Dispose()
        {
            if (!GtpMode.WithWait)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);

            InternalDispose();
        }

        private void InternalDispose()
        {
            Waiting = false;
            DisposingThreads = true;

            while (_waitingThreadsCount > 0)
            {
                lock (_queueLocker)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }

            while (JobsCount > 0 || (_threads.Count > 0))
            {
                Thread.Sleep(1);
            }

            GtpMode.InvokeDisposeCallback();

            Settings = null;
            GtpMode = null;
            _ignoredJobs = null;
            _current = null;
            _threads = null;
            _busyThreadsCount = _waitingThreadsCount = 0;

            Utils.Log("Generic Thread Pool Disposed");
            Utils.Log("###############################################");
        }

        #endregion
    }
}
