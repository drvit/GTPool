using System.Threading;

namespace GTPool
{
    public class ManagedJobWaitHandler
    {
        public ManagedJobWaitHandler(IManagedJob job)
            : this(job, -1)
        { }

        public ManagedJobWaitHandler(IManagedJob job, int groupById)
        {
            Current = job;
            GroupById = groupById;

            if (groupById > -1)
                JobWaitHandle = new ManualResetEventSlim(false);
        }

        public IManagedJob Current { get; set; }

        public int GroupById { get; set; }

        public ManualResetEventSlim JobWaitHandle { get; set; }

        public void DoWork()
        {
            try
            {
                Current.DoWork();
            }
            finally
            {
                if (JobWaitHandle != null)
                    JobWaitHandle.Set();
            }
        }
    }
}
