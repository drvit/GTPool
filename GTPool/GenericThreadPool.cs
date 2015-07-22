using System;
using System.CodeDom;
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
        private int _threadId;
        private bool _waiting;
        private bool _disposeThreads;
        
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
            return Init<TMode>(new CustomSettings());
        }

        public static GenericThreadPool Init<TMode>(CustomSettings settings)
            where TMode : IGtpMode, new()
        {
            InitializeInstance<TMode>(settings);
            _instance.InitializeThreadQueue();

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
                queue.Enqueue(job);

                if (!Waiting)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }
        }

        private void InitializeThreadQueue()
        {
            InitializeThreadQueue(!Waiting ? Settings.MinThreads : Settings.MaxThreads);
        }

        private void InitializeThreadQueue(int numberOfThreads)
        {
            while (Threads.Count < numberOfThreads)
            {
                var threadName = NextThreadName;

                Threads.Add(threadName, new ManagedThread(new Thread(JobInvoker)
                {
                    Name = threadName,
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                }));

                Threads[threadName].Start(threadName);
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
                    if (Threads.ContainsKey(tname))
                        Threads[tname].ExecuteJob(job);
                }
                else
                {
                    if (NumberOfRemainingJobs == 0 &&
                        (DisposeThreads || Threads.Count != Settings.MinThreads) &&
                        Threads[tname].Status == ManagedThreadStatus.Retired)
                    {
                        break;
                    }
                }
            }

            Threads.Remove(tname);
        }

        private ManagedJob DequeueJob(string threadName)
        {
            lock (_queueLocker)
            {
                if (!Waiting)
                {
                    if (_queueHighest.Any())
                        return _queueHighest.Dequeue();

                    if (_queueAboveNormal.Any())
                        return _queueAboveNormal.Dequeue();

                    if (_queueNormal.Any())
                        return _queueNormal.Dequeue();

                    if (_queueBelowNormal.Any())
                        return _queueBelowNormal.Dequeue();

                    if (_queueLowest.Any())
                        return _queueLowest.Dequeue();
                }

                Threads[threadName].Wait(_queueLocker, Settings.IdleTime, Waiting);
            }

            return null;
        }


        private Dictionary<string, ManagedThread> Threads
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

            var waiting = Waiting;
            Waiting = false;
            DisposeThreads = true;

            while (Threads.Any(x => x.Value.Status == ManagedThreadStatus.Waiting))
            {
                lock (_queueLocker)
                {
                    Monitor.Pulse(_queueLocker);
                }
            }

            while (NumberOfRemainingJobs > 0 || (Threads.Count > 0))
            {
                Thread.Sleep(1);
            }

            if (GtpMode.DisposeCallback != null)
                GtpMode.DisposeCallback.DynamicInvoke();

            Settings = null;
            GtpMode = null;
            Threads = null;
            Waiting = waiting;
            DisposeThreads = false;
            _instance = null;
        }
    }

    public class CustomSettings
    {
        private const int DefaultMinThreads = 2;
        private const int DefaultMaxThreads = 50;
        private int _minThreads;
        private int _maxThreads;
        private readonly int _idleTime;

        public CustomSettings()
            : this(DefaultMinThreads, DefaultMaxThreads, 5000)
        { }

        public CustomSettings(int minThreads, int maxThreads, int idleTime)
        {
            MinThreads = minThreads;
            MaxThreads = maxThreads;
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
    }

    
}
