using System.Threading;

namespace GTPool
{
    public class ManagedThread
    {
        private const int Timeout = 30000;
        private const int MaxIdleLifeCycles = 5;
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
            Wait(queuelock, idleTime, false);
        }

        public void Wait(object queuelock, int idleTime, bool withWait)
        {
            Status = ManagedThreadStatus.Waiting;

            if (withWait)
            {
                Monitor.Wait(queuelock);
                _idleLifeCycles = StartIdleLifeCycles;
            }
            else
            {
                if (Monitor.Wait(queuelock, idleTime))
                {
                    _idleLifeCycles = StartIdleLifeCycles;
                }
            }

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

            #if DEBUG
            job.DoWork(Name);
            #else
            job.DoWork();
            #endif

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
}
