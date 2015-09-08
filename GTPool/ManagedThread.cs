using System.Threading;

namespace GTPool
{
    public class ManagedThread
    {
        private const int Timeout = 30000;
        private readonly Thread _instance;
        private readonly bool _defaultIsBackground;
        private readonly ThreadPriority _defaultThreadPriority;

        public ManagedThread(Thread thread)
        {
            _instance = thread;
            _defaultThreadPriority = thread.Priority;
            _defaultIsBackground = thread.IsBackground;
        }

        public void Start()
        {
            _instance.Start();
        }

        public void Start(object param)
        {
            _instance.Start(param);
        }

        public void Wait(object queuelock)
        {
            Wait(queuelock, Timeout);
        }

        public void Wait(object queuelock, int idleTime)
        {
            Wait(queuelock, idleTime, false);
        }

        public void Wait(object queuelock, int idleTime, bool withWait)
        {
            if (withWait)
            {
                lock (queuelock)
                {
                    Monitor.Wait(queuelock);
                }
            }
            else
            {
                lock (queuelock)
                {
                    Monitor.Wait(queuelock, idleTime);
                }
            }
        }

        public void ExecuteJob(ManagedJobWaitHandler job)
        {
            _instance.IsBackground = job.Current.IsBackground;
            _instance.Priority = job.Current.ThreadPriority;

            job.DoWork();

            _instance.IsBackground = _defaultIsBackground;
            _instance.Priority = _defaultThreadPriority;

        }
    }
}
