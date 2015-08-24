using System;
using System.Threading;

namespace GTPool
{
    public class ManagedJobWaitHandler
    {
        public ManagedJobWaitHandler(ManagedJob job, int groupById = -1)
        {
            Current = job;
            GroupById = groupById;

            if (groupById > -1)
                JobWaitHandle = new ManualResetEventSlim(false);
        }

        public ManagedJob Current { get; set; }

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
