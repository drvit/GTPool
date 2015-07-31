using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GTPool
{
    public sealed class GenericThreadPool : IDisposable
    {
        private static GenericThreadPool _instance = new GenericThreadPool();
        private Dictionary<string, ManagedThread> _threads;

        private readonly object _queueLocker = new object();
        private readonly object _variableLocker = new object();
        private readonly Queue<ManagedJob> _queueHighest = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueAboveNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueBelowNormal = new Queue<ManagedJob>();
        private readonly Queue<ManagedJob> _queueLowest = new Queue<ManagedJob>();
        private HashSet<int> _cancelledJobs = new HashSet<int>(); 
        private int _threadId;
        private int _jobsCount;
        private bool _waiting;
        private bool _disposeThreads;
        private bool _creatingThreads;
        private bool _stallThreads;

        static GenericThreadPool() { }

        private GenericThreadPool() { }

        public static GenericThreadPool Instance { get { return _instance; } }
        public CustomSettings Settings { get; private set; }
        public IGtpMode GtpMode { get; private set; }

        public static GenericThreadPool Init()
        {
            return Init(new GtpAsync(), new CustomSettings());
        }

        public static GenericThreadPool Init<TMode>(int numberOfThreads, int idleTime)
            where TMode : GtpSync, new()
        {
            return Init(new TMode(), new CustomSettings(numberOfThreads, idleTime));
        }

        public static GenericThreadPool Init<TMode>(int minThreads, int maxThreads, int idleTime)
            where TMode : GtpAsync, new()
        {
            return Init(new TMode(), new CustomSettings(minThreads, maxThreads, idleTime));
        }

        private static GenericThreadPool Init<TMode>(TMode gtpMode, CustomSettings settings)
            where TMode : IGtpMode, new()
        {
            Utils.Log("###############################################");
            Utils.Log("Generic Thread Pool Initialization");

            InitializeInstance(gtpMode, settings);
            _instance.LoadThreadQueue();

            return _instance;
        }

        private static void InitializeInstance<TMode>(TMode gtpMode, CustomSettings settings)
            where TMode : IGtpMode, new()
        {
            if (_instance == null)
            {
                _instance = new GenericThreadPool {DisposeThreads = false};
            }

            if (_instance.GtpMode == null)
            {
                _instance.GtpMode = gtpMode;
                _instance.Waiting = _instance.GtpMode.WithWait;
            }
            else
            {
                if (typeof(TMode) != _instance.GtpMode.GetType())
                    throw new GtpException(GtpExceptions.IncompatibleGtpMode);
            }

            if (_instance.Settings == null)
            {
                _instance.Settings = settings;
            }

            if (_instance.Threads == null)
            {
                _instance.Threads = new Dictionary<string, ManagedThread>();
            }

            _instance.StallThreads = false;
        }

        private void LoadThreadQueue()
        {
            LoadThreadQueue(!Waiting ? Settings.MinThreads : Settings.MaxThreads);
        }

        private void LoadThreadQueue(int numberOfThreads)
        {
            Utils.Log(string.Format("------ Creating {0} threads ------", numberOfThreads));

            while (Threads.Count < Math.Min(Settings.MaxThreads, numberOfThreads))
            {
                if (StallThreads) break;

                var threadName = NextThreadName;

                Threads.Add(threadName, new ManagedThread(new Thread(JobInvoker)
                {
                    Name = threadName,
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                }));

                Threads[threadName].Start(threadName);

                Utils.Log(string.Format("<<<<< Thread created {0} >>>>>> ", threadName));
            }
        }

        private void JobInvoker(object threadName)
        {
            var tname = threadName.ToString();

            while (true)
            {
                var job = DequeueJob(tname);

                if (job != null)
                {
                    Threads[tname].ExecuteJob(job);
                }
                else
                {
                    if (JobsCount == 0 &&
                        (DisposeThreads || Threads.Count != Settings.MinThreads) &&
                        Threads[tname].Status == ManagedThreadStatus.Retired)
                    {
                        break;
                    }
                }
            }

            Threads.Remove(tname);
            Utils.Log(string.Format(">>>>>> Thread destroyed {0} <<<<<<", threadName));
        }

        public static void AddJob(ManagedAsyncJob job)
        {
            ValidateAsyncMode(job);
            
            _instance.AddJobToPriorityQueue(job);
        }

        public void AddJob(ManagedSyncJob job)
        {
            ValidateSyncMode(job);
            
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
                if (_cancelledJobs == null)
                    _cancelledJobs = new HashSet<int>();

                StallThreads = false;
                JobsCount++;

                queue.Enqueue(job);

                if (!Waiting)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }
        }
        
        private ManagedJob DequeueJob(string threadName)
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

                        if (_cancelledJobs.Contains(job.JobId))
                        {
                            _cancelledJobs.Remove(job.JobId);
                            return null;
                        }

                        return job;
                    }

                    _cancelledJobs = null;
                    StallThreads = true;
                }

                Threads[threadName].Wait(_queueLocker, Settings.IdleTime, Waiting);
            }

            return null;
        }

        private void BaunceThreadPool()
        {
            if (CreatingThreads) 
                return;

            bool areThereMoreJobsThanThreads;
            bool areThereThreadsWaiting;

            lock (_variableLocker)
            {
                areThereMoreJobsThanThreads = _jobsCount > _threads.Count(x =>
                    x.Value.Status != ManagedThreadStatus.Working &&
                    x.Value.Status != ManagedThreadStatus.NotStarted);

                areThereThreadsWaiting = _threads.Any(x => x.Value.Status == ManagedThreadStatus.Waiting ||
                                                           x.Value.Status == ManagedThreadStatus.Retired);
            }

            if (areThereMoreJobsThanThreads)
            {
                if (areThereThreadsWaiting)
                {
                    lock (_queueLocker)
                    {
                        Monitor.Pulse(_queueLocker);
                    }
                }
                else
                {
                    LoadThreadQueue(JobsCount);
                }
            }

            CreatingThreads = false;
        }

        public static void CancelJob(ManagedAsyncJob job)
        {
            ValidateAsyncMode(job);

            _instance.CancelJob(job.JobId);
        }

        public void CancelJob(ManagedSyncJob job)
        {
            ValidateSyncMode(job);

            CancelJob(job.JobId);
        }

        private void CancelJob(int jobid)
        {
            if (jobid <= 0)
                return;

            lock (_queueLocker)
            {
                if (!_cancelledJobs.Contains(jobid))
                    _cancelledJobs.Add(jobid);
            }
        }

        private bool StallThreads
        {
            get
            {
                lock (_variableLocker)
                {
                    return _stallThreads;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _stallThreads = value;
                }
            }
        }

        private Dictionary<string, ManagedThread> Threads
        {
            get
            {
                lock (_variableLocker)
                {
                    return _threads;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _threads = value;
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

        private bool DisposeThreads
        {
            get
            {
                lock (_variableLocker)
                {
                    return _disposeThreads;
                }
            }
            set
            {
                lock (_variableLocker)
                {
                    _disposeThreads = value;
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

        private static void ValidateAsyncMode(ManagedAsyncJob job)
        {
            if (_instance == null)
                throw new GtpException(GtpExceptions.InstanceIsDisposed);

            if (_instance.GtpMode.WithWait != job.GtpMode.WithWait)
                throw new GtpException(GtpExceptions.IncompatibleGtpMode);

            if (_instance.Settings == null)
                throw new GtpException(GtpExceptions.SettingsNotInitialized);
        }

        private void ValidateSyncMode(ManagedSyncJob job)
        {
            if (GtpMode.WithWait != job.GtpMode.WithWait)
                throw new GtpException(GtpExceptions.IncompatibleGtpMode);
        }

        public static void End()
        {
            if (_instance == null)
                throw new GtpException(GtpExceptions.InstanceIsDisposed);

            if (_instance.GtpMode.WithWait)
                throw new GtpException(GtpExceptions.IncompatibleGtpMode);

            _instance.InternalDispose();
        }

        public void Dispose()
        {
            if (!GtpMode.WithWait)
                throw new GtpException(GtpExceptions.IncompatibleGtpMode);

            InternalDispose();
        }

        private void InternalDispose()
        {
            // TODO: Add abort jobs and kill all threads

            Waiting = false;
            DisposeThreads = true;

            while (Threads.Any(x => x.Value.Status == ManagedThreadStatus.Waiting))
            {
                lock (_queueLocker)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }

            while (JobsCount > 0 || (Threads.Count > 0))
            {
                Thread.Sleep(1);
            }

            if (GtpMode.DisposeCallback != null)
                GtpMode.DisposeCallback.DynamicInvoke();

            Settings = null;
            GtpMode = null;
            Threads = null;
            _instance = null;

            Utils.Log("Generic Thread Pool Disposed");
            Utils.Log("###############################################");
        }
    }

    public class CustomSettings
    {
        private const int DefaultMinThreads = 2;
        private const int DefaultMaxThreads = 200;
        private const int DefaultIdleTime = 500;

        public CustomSettings()
            : this(DefaultMinThreads, DefaultMaxThreads, DefaultIdleTime)
        { }

        public CustomSettings(int numberOfThreads, int idleTime)
            : this(DefaultMinThreads, numberOfThreads, idleTime)
        { }

        public CustomSettings(int minThreads, int maxThreads, int idleTime)
        {
            MaxThreads = Math.Min(Math.Max(DefaultMinThreads, maxThreads), DefaultMaxThreads);
            MinThreads = Math.Max(Math.Min(DefaultMaxThreads, minThreads), DefaultMinThreads);
            MinThreads = Math.Min(MinThreads, MaxThreads);

            IdleTime = idleTime;
        }

        public int MinThreads { get; private set; }

        public int MaxThreads { get; private set; }

        public int NumberOfThreads
        {
            get { return MaxThreads; }
        }

        public int IdleTime { get; private set; }
    }
}
