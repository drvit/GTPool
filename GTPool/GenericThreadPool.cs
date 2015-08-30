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
    /// 11. Benchmark the execution	                            -- done
    /// 12. Unit test the Thread Pool	                        -- done
    /// 13. Create a Demo Application	                        -- done
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
        private readonly Queue<ManagedJobWaitHandler> _queueHighest = new Queue<ManagedJobWaitHandler>();
        private readonly Queue<ManagedJobWaitHandler> _queueAboveNormal = new Queue<ManagedJobWaitHandler>();
        private readonly Queue<ManagedJobWaitHandler> _queueNormal = new Queue<ManagedJobWaitHandler>();
        private readonly Queue<ManagedJobWaitHandler> _queueBelowNormal = new Queue<ManagedJobWaitHandler>();
        private readonly Queue<ManagedJobWaitHandler> _queueLowest = new Queue<ManagedJobWaitHandler>();
        private HashSet<int> _ignoredJobs;
        private static HashSet<string> _threadsUsed;
        private static HashSet<int> _jobsProcessed;
        private int _threadId;
        private int _monitorFailureCount;
        private int _jobsCount;
        private bool _waiting;
        private bool _disposingThreads;
        private int _waitingThreadsCount;
        private bool _gtpMonitorWaiting;
        private Delegate _disposeCallback;
        private object[] _disposeCallbackParams;
        private Dictionary<int, IList<WaitHandle>> _waitHandlers;

        #endregion

        #region Constructors
        
        static GenericThreadPool() { }

        private GenericThreadPool() { }

        #endregion

        #region Public Properties

        public static GenericThreadPool Current { get { return _current; } }
        public static GenericThreadPoolSettings Settings { get; private set; }

        public static bool IsInitialized { get { return Settings != null; } }
        public static int TotalThreadsCreated { get; private set; }
        public static int TotalThreadsUsed { get { return _threadsUsed != null ? _threadsUsed.Count : 0; } }
        public static int TotalJobsAdded { get; private set; }
        public static int TotalJobsProcessed { get { return _jobsProcessed != null ? _jobsProcessed.Count : 0; } }

        #endregion

        #region Initialize Generic Thread Pool

        public static void Init()
        {
            Init(new GenericThreadPoolSettings(), null, null);
        }

        public static void Init(int minThreads, int maxThreads)
        {
            Init(new GenericThreadPoolSettings(minThreads, maxThreads), null, null);
        }

        public static void Init(int minThreads, int maxThreads, int idleTime)
        {
            Init(new GenericThreadPoolSettings(minThreads, maxThreads, idleTime), null, null);
        }

        public static void Init(int minThreads, int maxThreads, int idleTime,
            Delegate disposeCallback, object[] disposeCallbackParams)
        {
            Init(new GenericThreadPoolSettings(minThreads, maxThreads, idleTime),
                disposeCallback, disposeCallbackParams);
        }

        private static void Init(GenericThreadPoolSettings settings,
            Delegate disposeCallback, object[] disposeCallbackParams)
        {
            if (InitializeInstance(settings, disposeCallback, disposeCallbackParams))
            {
                Utils.Log("###############################################");
                Utils.Log("Generic Thread Pool Initialization");

                _current.LoadThreadPool(settings.MinThreads);
                _current.CreateMonitor();
            }
        }

        private static bool InitializeInstance(GenericThreadPoolSettings settings,
            Delegate disposeCallback, object[] disposeCallbackParams)
        {
            if (Settings == null)
            {
                Settings = settings;
                _current.DisposingThreads = false;
                _current.Waiting = true;
                _current._ignoredJobs = new HashSet<int>();
                _current._threadId = 0;
                _current._disposeCallback = disposeCallback;
                _current._disposeCallbackParams = disposeCallbackParams;
                _current._waitHandlers = new Dictionary<int, IList<WaitHandle>>();

                ResetTotals();

                var numProcs = Environment.ProcessorCount;
                var concurrencyLevel = numProcs * 2;

                _current._threads = new ConcurrentDictionary<string, ManagedThread>(
                    concurrencyLevel, Settings.MaxThreads);

                return true;
            }

            return false;
        }

        public static void ResetTotals()
        {
            lock (_current._variableLocker)
            {
                TotalThreadsCreated = 0;
                _threadsUsed = new HashSet<string>();
                TotalJobsAdded = 0;
                _jobsProcessed = new HashSet<int>();
            }
        }

        #endregion

        #region Handle Managed Threads

        private void LoadThreadPool(int numberOfThreads)
        {
            numberOfThreads = Math.Min(Settings.MaxThreads, numberOfThreads);

            if (_threads.Count < numberOfThreads)
            {
                Utils.Log(string.Format("====== Attempting to create {0} threads ======", numberOfThreads));

                while (_threads.Count < numberOfThreads && (JobsCount > 0 || Waiting))
                {
                    var threadName = NextThreadName;

                    _threads.TryAdd(threadName, new ManagedThread(new Thread(Consumer)
                    {
                        Name = threadName,
                        IsBackground = true,
                        Priority = ThreadPriority.Normal
                    }));

                    _threads[threadName].Start();
                    TotalThreadsCreated++;

                    Utils.Log(string.Format("<<<<< Thread {0} created >>>>>>", threadName));
                }
            }
        }

        private void Consumer()
        {
            var tname = CurrentThreadName;

            while (true)
            {
                var jobwh = DequeueJob();

                if (jobwh != null)
                {
                    lock (_variableLocker)
                    {
                        if (!_threadsUsed.Contains(tname))
                            _threadsUsed.Add(tname);

                        if (!_jobsProcessed.Contains(jobwh.Current.JobId))
                            _jobsProcessed.Add(jobwh.Current.JobId);
                    }

                    _threads[tname].ExecuteJob(jobwh);
                }
                else
                {
                    lock (_variableLocker)
                    {
                        _waitingThreadsCount++;
                    }

                    Utils.Log("Thread going to sleep");

                    _threads[tname].Wait(_queueLocker, Settings.IdleTime, 
                        Waiting && _threads.Count <= Settings.MinThreads);

                    Utils.Log("Thread woke up");

                    lock (_variableLocker)
                    {
                        _waitingThreadsCount--;
                    }

                    if (JobsCount == 0 && (DisposingThreads || _threads.Count > Settings.MinThreads))
                    {
                        break;
                    }
                }
            }

            ManagedThread mt;
            _threads.TryRemove(tname, out mt);

            Utils.Log(">>>>>> Thread destroyed <<<<<<");
        }

        public static void WaitAllJobs(int groupedById)
        {
            WaitAllJobs(groupedById, false);
        }

        private static void WaitAllJobs(int groupedById, bool silently)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.MTA)
            {
                if (silently)
                    return;

                throw new GenericThreadPoolException(GenericThreadPoolExceptionType.WaitHandlerNotInMta);
            }

            IList<WaitHandle> groupedJobsHandlers;

            if (_current._waitHandlers.TryGetValue(groupedById, out groupedJobsHandlers)
                && groupedJobsHandlers != null)
            {
                Utils.Log(string.Format("?????? Waiting all jobs grouped by id {0} to finish", groupedById));

                WaitHandle.WaitAll(groupedJobsHandlers.ToArray(), 30000);
                _current._waitHandlers.Remove(groupedById);

                Utils.Log(string.Format("!!!!!! Waiting all jobs grouped by id {0} have finished", groupedById));
            }
        }

        #endregion

        #region Enqueue Managed Job

        public static void AddJob(IManagedJob job)
        {
            AddJob(job, -1);
        }

        public static void AddJob(IManagedJob job, int groupById)
        {
            ValidateJob(job);

            var jobwh = new ManagedJobWaitHandler(job, groupById);

            _current.AddJobToPriorityQueue(jobwh);
        }

        private void AddJobToPriorityQueue(ManagedJobWaitHandler jobwh)
        {
            Queue<ManagedJobWaitHandler> queue;

            switch (jobwh.Current.ThreadPriority)
            {
                case ThreadPriority.Highest:
                    queue = _queueHighest;
                    break;
                case ThreadPriority.AboveNormal:
                    queue = _queueAboveNormal;
                    break;
                case ThreadPriority.BelowNormal:
                    queue = _queueBelowNormal;
                    break;
                case ThreadPriority.Lowest:
                    queue = _queueLowest;
                    break;
                default:
                    queue = _queueNormal;
                    break;
            }

            EnqueueJob(queue, jobwh);
        }

        private void EnqueueJob(Queue<ManagedJobWaitHandler> queue, ManagedJobWaitHandler jobwh)
        {
            CheckGtpMonitorIsRunning();

            lock (_queueLocker)
            {
                lock (_variableLocker)
                {
                    TotalJobsAdded++;
                    _jobsCount++;

                    if (jobwh.JobWaitHandle != null)
                    {
                        if (!_waitHandlers.ContainsKey(jobwh.GroupById))
                        {
                            _waitHandlers = new Dictionary<int, IList<WaitHandle>>
                            {
                                {jobwh.GroupById, new List<WaitHandle>()}
                            };
                        }

                        _waitHandlers[jobwh.GroupById].Add(jobwh.JobWaitHandle.WaitHandle);
                    }
                }

                queue.Enqueue(jobwh);
                Utils.Log(string.Format("++++++ Job added to GTP JobId: {0} ++++++", jobwh.Current.JobId));

                Waiting = false;
                Monitor.Pulse(_queueLocker);
            }
        }

        #endregion

        #region Dequeue Managed Job

        private ManagedJobWaitHandler DequeueJob()
        {
            if (!Waiting)
            {
                lock (_queueLocker)
                {
                    var jobwh = DequeueJobWithHighestPriority();

                    if (jobwh != null)
                    {
                        lock (_variableLocker)
                        {
                            _jobsCount--;
                        }

                        if (_ignoredJobs.Contains(jobwh.Current.JobId))
                        {
                            _ignoredJobs.Remove(jobwh.Current.JobId);
                            Utils.Log(string.Format("------ Ignored Job {0} ------", jobwh.Current.JobId));
                            return null;
                        }

                        return jobwh;
                    }

                    _ignoredJobs.Clear();
                }
            }

            return null;
        }

        private ManagedJobWaitHandler DequeueJobWithHighestPriority()
        {
            ManagedJobWaitHandler jobwh = null;
            if (_queueHighest.Any())
                jobwh = _queueHighest.Dequeue();

            else if (_queueAboveNormal.Any())
                jobwh = _queueAboveNormal.Dequeue();

            else if (_queueNormal.Any())
                jobwh = _queueNormal.Dequeue();

            else if (_queueBelowNormal.Any())
                jobwh = _queueBelowNormal.Dequeue();

            else if (_queueLowest.Any())
                jobwh = _queueLowest.Dequeue();

            return jobwh;
        }

        #endregion

        #region GTP Thread Monitor

        private void CreateMonitor()
        {
            var gtpMonitorThread = new Thread(new ThreadStart((Action) (() =>
            {
                try
                {
                    GtpMonitor();
                }
                catch (Exception ex)
                {
                    Utils.Log(string.Format("GenericThreadPool Thread Monitor failed with exception: {0}", ex.Message));

                    if (_monitorFailureCount > 1)
                    {
                        Shutdown();
                        return;
                    }

                    _monitorFailureCount++;
                    Utils.Log(string.Format("GenericThreadPool will attempt to restart the GTP Thread Monitor ({0}/2)...", _monitorFailureCount));

                    CreateMonitor();
                }
            })));

            gtpMonitorThread.Name = "__thread_gtpMonitor__";
            gtpMonitorThread.Start();
        }

        private void CheckGtpMonitorIsRunning()
        {
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
                            Monitor.Pulse(_queueLocker);
                            Utils.Log("(((((( Pulse ))))))");
                        }
                    }
                    else
                    {
                        LoadThreadPool(JobsCount);
                    }
                }
                else
                {
                    lock (_gtpMonitorLocker)
                    {
                        Waiting = true;
                        _gtpMonitorWaiting = true;
                        Monitor.Wait(_gtpMonitorLocker);
                    }
                }
            }

            Utils.Log(":::::: Shutting down GTP Monitor ::::::");
        }

        #endregion

        #region Cancel Managed Jobs

        public static void CancelJob(IManagedJob job)
        {
            ValidateJob(job);

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

        private static void ValidateJob(IManagedJob job)
        {
            if (job == null)
                throw new GenericThreadPoolException(GenericThreadPoolExceptionType.JobIsNull);

            if (Settings == null)
                throw new GenericThreadPoolException(GenericThreadPoolExceptionType.SettingsNotInitialized);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Shutdown(false);
        }

        public static void Shutdown()
        {
            Shutdown(false);
        }

        public static void Shutdown(bool silently)
        {
            _current.InternalDispose(silently);
        }

        private void InternalDispose(bool silently = false)
        {
            Utils.Log("...... Disposing ......");

            if (Settings == null)
            {
                if (!silently)
                {
                    throw new GenericThreadPoolException(GenericThreadPoolExceptionType.InstanceIsDisposed);
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

                foreach (var ev in _waitHandlers)
                {
                    WaitAllJobs(ev.Key, true);
                }

                while (_threads.Count > 0 || JobsCount > 0)
                {
                    Thread.Sleep(1);
                }

                if (_disposeCallback != null)
                    _disposeCallback.DynamicInvoke(_disposeCallbackParams);
            }

            _ignoredJobs = null;
            _threads = null;
            Settings = null;

            Utils.Log("Generic Thread Pool Disposed");
            Utils.Log(string.Format("Summary: {0} Threads Created; {1} Threads Consumed; {2} Jobs Added; {3} Jobs Processed;", TotalThreadsCreated, TotalThreadsUsed, TotalJobsAdded, TotalJobsProcessed));
            Utils.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
        }

        #endregion
    }
}
