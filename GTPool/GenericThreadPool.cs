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

        private static readonly GenericThreadPool _current = new GenericThreadPool();
        private ConcurrentDictionary<string, ManagedThread> _threads;
        private readonly object _queueLocker = new object();
        private readonly object _variableLocker = new object();
        private readonly object _gtpMonitorLocker = new object();
        private readonly Queue<ManagedJob> _queueHighest = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueAboveNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueBelowNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueLowest = new Queue<ManagedJob>();
        private HashSet<int> _ignoredJobs;
        private HashSet<string> _threadsUsed;
        private HashSet<int> _jobsProcessed;
        private int _threadId;
        private int _monitorFailureCount;
        private int _jobsCount;
        private bool _waiting;
        private bool _disposingThreads;
        private int _waitingThreadsCount;
        private bool _gtpMonitorWaiting;
        private int _totalThreadsCreated;
        private int _totalJobsAdded;

        #endregion

        #region Constructors
        
        static GenericThreadPool() { }

        private GenericThreadPool() { }

        #endregion

        #region Public Properties

        public static GenericThreadPool Current { get { return _current; } }
        public GenericThreadPoolSettings Settings { get; private set; }
        public GenericThreadPoolMode GtpMode { get; private set; }

        public static int TotalThreadsCreated { get { return _current._totalThreadsCreated; } }
        public static int TotalThreadsUsed { get { return _current._threadsUsed != null ? _current._threadsUsed.Count() : 0; } }
        public static int TotalJobsAdded { get { return _current._totalJobsAdded; } }
        public static int TotalJobsProcessed { get { return _current._jobsProcessed != null ? _current._jobsProcessed.Count() : 0; } }

        #endregion

        #region Initialize Generic Thread Pool

        public static GenericThreadPool Init()
        {
            return Init<GtpAsync>(new GenericThreadPoolSettings(), null, null);
        }

        public static GenericThreadPool Init<TMode>(int numberOfThreads)
            where TMode : GtpSync, new()
        {
            return Init<TMode>(new GenericThreadPoolSettings(numberOfThreads), null, null);
        }

        public static GenericThreadPool Init<TMode>(int numberOfThreads, Delegate disposeCallback, 
            object[] disposeCallbackParams)
            where TMode : GtpSync, new()
        {
            return Init<TMode>(new GenericThreadPoolSettings(numberOfThreads), disposeCallback, 
                disposeCallbackParams);
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
            if (InitializeInstance<TMode>(settings, disposeCallback, disposeCallbackParams))
            {
                Utils.Log("###############################################");
                Utils.Log("Generic Thread Pool Initialization");

                _current.LoadThreadQueue();

                if (!_current.GtpMode.WithWait)
                    _current.CreateMonitor();
            }

            return _current;
        }

        private static bool InitializeInstance<TMode>(GenericThreadPoolSettings settings,
            Delegate disposeCallback, object[] disposeCallbackParams)
            where TMode : GenericThreadPoolMode, new()
        {
            if (_current.Settings == null)
            {
                _current.Settings = settings;
                _current.DisposingThreads = false;
                _current.Waiting = true;
                _current._ignoredJobs = new HashSet<int>();
                _current._threadId = 0;

                ResetTotals();

                _current.GtpMode = new TMode
                {
                    DisposeCallback = disposeCallback,
                    DisposeCallbackParams = disposeCallbackParams
                };

                var numProcs = Environment.ProcessorCount;
                var concurrencyLevel = numProcs*2;

                _current._threads = new ConcurrentDictionary<string, ManagedThread>(
                    concurrencyLevel, _current.Settings.MaxThreads);

                return true;
            }
            
            if (typeof (TMode) != _current.GtpMode.GetType())
            {
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);
            }

            return false;
        }

        public static void ResetTotals()
        {
            lock (_current._variableLocker)
            {
                _current._totalThreadsCreated = 0;
                _current._threadsUsed = new HashSet<string>();
                _current._totalJobsAdded = 0;
                _current._jobsProcessed = new HashSet<int>();
            }
        }

        #endregion

        #region Handle Managed Threads

        private void LoadThreadQueue()
        {
            LoadThreadQueue(GtpMode.WithWait ? Settings.MaxThreads : Settings.MinThreads);
        }

        private void LoadThreadQueue(int numberOfThreads)
        {
            numberOfThreads = Math.Min(Settings.MaxThreads, numberOfThreads);

            if (_threads.Count < numberOfThreads)
            {
                Utils.Log(string.Format("====== Attempting to create {0} threads ======", numberOfThreads));

                while (true)
                {
                    if ((JobsCount == 0 && !Waiting) || (_threads.Count == numberOfThreads))
                    {
                        break;
                    }

                    var threadName = NextThreadName;

                    _threads.TryAdd(threadName, new ManagedThread(new Thread(JobInvoker)
                    {
                        Name = threadName,
                        IsBackground = true,
                        Priority = ThreadPriority.Normal
                    }));

                    _threads[threadName].Start();
                    _totalThreadsCreated++;

                    Utils.Log(string.Format("<<<<< Thread {0} created >>>>>>", threadName));
                }
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
                    lock (_variableLocker)
                    {
                        if (!_threadsUsed.Contains(tname))
                            _threadsUsed.Add(tname);

                        if (!_jobsProcessed.Contains(job.JobId))
                            _jobsProcessed.Add(job.JobId);
                    }
                    _threads[tname].ExecuteJob(job);
                }
                else
                {
                    //if (_threads.Count > Settings.MinThreads)
                    //{
                        lock (_variableLocker)
                        {
                            _waitingThreadsCount++;
                        }

                        Utils.Log("Thread going to sleep");
                        _threads[CurrentThreadName].Wait(_queueLocker, Settings.IdleTime, Waiting);
                        Utils.Log("Thread woke up");

                        lock (_variableLocker)
                        {
                            _waitingThreadsCount--;
                        }
                    //}

                    if (JobsCount == 0 && (DisposingThreads || 
                        (!GtpMode.WithWait && _threads.Count > Settings.MinThreads)))
                    {
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
            CheckGtpMonitorIsRunning();

            lock (_queueLocker)
            {
                lock (_variableLocker)
                {
                    _totalJobsAdded++;
                    _jobsCount++;
                }

                queue.Enqueue(job);
                Utils.Log(string.Format("++++++ Job added to GTP JobId: {0} ++++++", job.JobId));

                if (!GtpMode.WithWait)
                {
                    Waiting = false;
                    Monitor.Pulse(_queueLocker);
                }
            }
        }

        #endregion

        #region Dequeue Managed Job

        private ManagedJob DequeueJob()
        {
            if (!Waiting)
            {
                lock (_queueLocker)
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
                        lock (_variableLocker)
                        {
                            _jobsCount--;
                        }

                        if (_ignoredJobs.Contains(job.JobId))
                        {
                            _ignoredJobs.Remove(job.JobId);
                            Utils.Log(string.Format("------ Ignored Job {0} ------", job.JobId));
                            return null;
                        }

                        return job;
                    }

                    _ignoredJobs.Clear();
                }
            }

            return null;
        }

        #endregion
        
        #region GTP Thread Monitor

        private void CreateMonitor()
        {
            AddJobToPriorityQueue(new ManagedAsyncJob((Action)GtpMonitor, null, null, null, ex =>
            {
                Utils.Log(string.Format("GenericThreadPool Thread Monitor failed with exception: {0}", ex.Message));

                if (_monitorFailureCount > 1)
                {
                    Dispose();
                    return;
                }

                _monitorFailureCount++;
                Utils.Log(string.Format("GenericThreadPool will attempt to restart the GTP Thread Monitor ({0}/2)...",
                    _monitorFailureCount));

                CreateMonitor();

            }, ThreadPriority.Normal, true));
        }

        private void CheckGtpMonitorIsRunning()
        {
            if (GtpMode.WithWait)
                return;

            if (!_gtpMonitorWaiting)
                return;

            lock (_gtpMonitorLocker)
            {
                if (!_gtpMonitorWaiting)
                    return;

                _gtpMonitorWaiting = false;
                Monitor.Pulse(_gtpMonitorLocker);
            }
        }

        private void GtpMonitor()
        {
            Utils.Log(":::::: Starting GTP Monitor ::::::");

            while (!DisposingThreads)
            {
                if (JobsCount > 0)
                {
                    if (WaitingThreadsCount > 0)
                    {
                        lock (_queueLocker)
                        {
                            Monitor.PulseAll(_queueLocker);
                            Utils.Log("(((((( PulseAll ))))))");
                        }
                    }
                    else
                    {
                        LoadThreadQueue(JobsCount);
                    }
                }
                else
                {
                    lock (_gtpMonitorLocker)
                    {
                        _gtpMonitorWaiting = true;
                        Monitor.Wait(_gtpMonitorLocker);
                    }
                }
            }

            Utils.Log(":::::: Shutting down GTP Monitor ::::::");
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
                    return _waiting;
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

        private int WaitingThreadsCount
        {
            get
            {
                lock (_variableLocker)
                {
                    return _waitingThreadsCount;
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
            End(false);
        }

        public static void End(bool silently)
        {
            if (!silently && !_current.GtpMode.WithWait)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);

            _current.InternalDispose(silently);
        }

        public void Dispose()
        {
            Dispose(false);
        }

        public void Dispose(bool silently)
        {
            if (!silently && !GtpMode.WithWait)
                throw new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.IncompatibleGtpMode);

            InternalDispose(silently);
        }

        private void InternalDispose(bool silently = false)
        {
            Utils.Log("...... Disposing ......");

            if (GtpMode == null || Settings == null)
            {
                if (!silently)
                {
                    throw new GenericThreadPoolException(
                        GenericThreadPoolExceptionType.SettingsNotInitialized);
                }

                ResetTotals();
            }
            else
            {
                Waiting = false;
                DisposingThreads = true;

                CheckGtpMonitorIsRunning();

                while (WaitingThreadsCount > 0)
                {
                    lock (_queueLocker)
                    {
                        Monitor.Pulse(_queueLocker);
                    }
                }

                while (_threads.Count > 0 || JobsCount > 0)
                {
                    Thread.Sleep(1);
                }

                GtpMode.InvokeDisposeCallback();
            }

            Settings = null;
            GtpMode = null;
            _ignoredJobs = null;
            _threadsUsed = null;
            _jobsProcessed = null;
            _threads = null;

            Utils.Log("Generic Thread Pool Disposed");
            Utils.Log(string.Format("Summary: {0} Threads Created; {1} Threads Consummed; {2} Jobs Added; {3} Jobs Processed;",
                TotalThreadsCreated, TotalThreadsUsed, TotalJobsAdded, TotalJobsProcessed));
            Utils.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        #endregion
    }
}
