using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Markup;

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
        private int _threadId;
        private bool _waiting;
        private bool _disposeThreads;
        private bool _creatingThreads;
        private int _jobsCount;
        private bool _stallThreads;

        static GenericThreadPool() { }
        
        private GenericThreadPool() { }

        public static GenericThreadPool Instance
        {
            get { return _instance; }
        }

        public CustomSettings Settings { get; private set; }

        public IGtpMode GtpMode { get; private set; }

        public static GenericThreadPool Init()
        {
            return Init<GtpAsync>();
        }
        
        public static GenericThreadPool Init<TMode>()
            where TMode : IGtpMode, new()
        {
            if (typeof(TMode) == typeof(GtpAsync))
                return Init<TMode>(new CustomAsyncSettings());

            return Init<TMode>(new CustomSyncSettings());
        }

        public static GenericThreadPool Init<TMode>(CustomSettings settings)
            where TMode : IGtpMode, new()
        {
            Utils.Log("###############################################");
            Utils.Log("Generic Thread Pool Initialization");

            InitializeInstance<TMode>(settings);
            _instance.LoadThreadQueue();

            return _instance;
        }

        private static void InitializeInstance<TMode>(CustomSettings settings)
            where TMode : IGtpMode, new()
        {
            if (_instance == null)
            {
                _instance = new GenericThreadPool {DisposeThreads = false};
            }

            if (_instance.GtpMode == null)
            {
                _instance.GtpMode = new TMode();
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

        public static void AddJob(ManagedAsyncJob job)
        {
            if (_instance == null)
                throw new GtpException(GtpExceptions.InstanceIsDisposed);

            if (_instance.GtpMode.WithWait != job.GtpMode.WithWait)
                throw new GtpException(GtpExceptions.IncompatibleGtpMode);

            if (_instance.Settings == null)
                throw new GtpException(GtpExceptions.SettingsNotInitialized);

            _instance.AddJobToPriorityQueue(job);
        }

        public void AddJob(ManagedSyncJob job)
        {
            if (GtpMode.WithWait != job.GtpMode.WithWait)
                throw new GtpException(GtpExceptions.IncompatibleGtpMode);

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
                StallThreads = false;
                JobsCount++;

                queue.Enqueue(job);

                if (!Waiting)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }
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

        private ManagedJob DequeueJob(string threadName)
        {
            BaunceThreadPool();

            lock (_queueLocker)
            {
                if (!Waiting)
                {
                    ManagedJob ret = null;

                    if (_queueHighest.Any())
                        ret = _queueHighest.Dequeue();

                    else if (_queueAboveNormal.Any())
                        ret = _queueAboveNormal.Dequeue();

                    else if (_queueNormal.Any())
                        ret = _queueNormal.Dequeue();

                    else if (_queueBelowNormal.Any())
                        ret = _queueBelowNormal.Dequeue();

                    else if (_queueLowest.Any())
                        ret = _queueLowest.Dequeue();

                    if (ret != null)
                    {
                        JobsCount--;
                        return ret;
                    }

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
                _creatingThreads = true;

                areThereMoreJobsThanThreads = _jobsCount > _threads.Count(x =>
                    x.Value.Status != ManagedThreadStatus.Working &&
                    x.Value.Status != ManagedThreadStatus.NotStarted);

                areThereThreadsWaiting = Threads.Any(x => x.Value.Status == ManagedThreadStatus.Waiting ||
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
                    return _creatingThreads;
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

    public class CustomSyncSettings : CustomSettings
    {
        public CustomSyncSettings() : base(new GtpSync())
        {
        }

        public CustomSyncSettings(int numberOfThreads, int idleTime)
            : base(new GtpSync(), 1, numberOfThreads, idleTime)
        {
        }
    }

    public class CustomAsyncSettings : CustomSettings
    {
        public CustomAsyncSettings() : base(new GtpAsync())
        {
        }

        public CustomAsyncSettings(int minThreads, int maxThreads, int idleTime) 
            : base(new GtpAsync(), minThreads, maxThreads, idleTime)
        {
        }
    }

    public abstract class CustomSettings
    {
        private const int DefaultMinThreads = 2;
        private const int DefaultMaxThreads = 200;
        private int _minThreads;
        private int _maxThreads;
        private readonly int _idleTime;

        protected CustomSettings(IGtpMode gtpMode)
            : this(gtpMode, DefaultMinThreads, DefaultMaxThreads, 5000)
        { }

        protected CustomSettings(IGtpMode gtpMode, int minThreads, int maxThreads, int idleTime)
        {
            MinThreads = minThreads;
            MaxThreads = maxThreads;
            GtpMode = gtpMode;
            _idleTime = idleTime;
        }

        public int MinThreads
        {
            get { return _minThreads > _maxThreads ? _maxThreads : _minThreads; }
            private set
            {
                _minThreads = value > DefaultMaxThreads
                    ? DefaultMaxThreads
                    : value < DefaultMinThreads ? DefaultMinThreads : value;
            }
        }

        public int MaxThreads
        {
            get { return _maxThreads; }
            private set
            {
                _maxThreads = value < DefaultMinThreads
                    ? DefaultMinThreads
                    : value > DefaultMaxThreads ? DefaultMaxThreads : value;
            }
        }

        public int IdleTime
        {
            get { return _idleTime; }
        }

        public IGtpMode GtpMode { get; private set; }

    }

    
}
