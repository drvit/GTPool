using System.Threading;

namespace GTPool
{
    public interface IManagedJob
    {
        int JobId { get; }
        
        ThreadPriority ThreadPriority { get; }
        
        bool IsBackground { get; }
        
        WorkStatus Status { get; }
        
        GenericThreadPoolException Error { get; }

        void DoWork();
    }
}