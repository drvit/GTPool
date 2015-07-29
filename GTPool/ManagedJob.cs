using System;
using System.Diagnostics;
using System.Threading;

namespace GTPool
{
    public class ManagedAsyncJob : ManagedJob
    {
        public ManagedAsyncJob(Delegate work) 
            : base(new GtpAsync(), work)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters) 
            : base(new GtpAsync(), work, parameters)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, ThreadPriority threadPriority,
            bool isBackground) 
            : base(new GtpAsync(), work, parameters, threadPriority, isBackground)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters) 
            : base(new GtpAsync(), work, parameters, callback, callbackParameters)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpAsync(), work, parameters, callback, callbackParameters, threadPriority, isBackground)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, Action<Exception> onerror)
            : base(new GtpAsync(), work, parameters, callback, callbackParameters, onerror)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, Action<Exception> onerror, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpAsync(), work, parameters, callback, callbackParameters, onerror, threadPriority, isBackground)
        {
        }
    }

    public class ManagedSyncJob : ManagedJob
    {
        public ManagedSyncJob(Delegate work) 
            : base(new GtpSync(), work)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters)
            : base(new GtpSync(), work, parameters)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, ThreadPriority threadPriority,
            bool isBackground)
            : base(new GtpSync(), work, parameters, threadPriority, isBackground)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters)
            : base(new GtpSync(), work, parameters, callback, callbackParameters)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpSync(), work, parameters, callback, callbackParameters, threadPriority, isBackground)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, Action<Exception> onerror)
            : base(new GtpSync(), work, parameters, callback, callbackParameters, onerror)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, Action<Exception> onerror, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpSync(), work, parameters, callback, callbackParameters, onerror, threadPriority, isBackground)
        {
        }
    }

    public abstract class ManagedJob
    {
        protected ManagedJob(IGtpMode gtpMode, Delegate work)
            : this(gtpMode, work, null, null, null, null)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters)
            : this(gtpMode, work, parameters, null, null, null)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : this(gtpMode, work, parameters, null, null, null, threadPriority, isBackground)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters)
            : this(gtpMode, work, parameters, callback, callbackParameters, null)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            ThreadPriority threadPriority, bool isBackground)
            : this(gtpMode, work, parameters, callback, callbackParameters, null, threadPriority, isBackground)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            Action<Exception> onerror)
            : this(gtpMode, work, parameters, callback, callbackParameters, onerror, ThreadPriority.Normal, true)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters, 
            Action<Exception> onerror, ThreadPriority threadPriority, bool isBackground)
        {
            ThreadPriority = threadPriority;
            IsBackground = isBackground;
            GtpMode = gtpMode;
            Status = WorkStatus.NotStarted;

            _work = new ManagedJobDelegate(work, parameters);
            _callback = new ManagedJobDelegate(callback, callbackParameters);
            _onerror = new ManagedJobDelegate(onerror);
        }

        private readonly ManagedJobDelegate _work;
        private readonly ManagedJobDelegate _callback;
        private readonly ManagedJobDelegate _onerror;

        public IGtpMode GtpMode { get; private set; }

        public ThreadPriority ThreadPriority { get; private set; }

        public bool IsBackground { get; private set; }

        public string ThreadName { get; private set; }

        public WorkStatus Status { get; private set; }

        public Exception Error { get; private set; }

        public void DoWork(string threadName)
        {
            ThreadName = threadName;

            Status = WorkStatus.InProgress;
            Utils.Log(string.Format("Thread Working {0}", threadName));
            
            object result;
            try
            {
                result = _work.Invoke(_onerror, "Thread Work failed", threadName);
            }
            catch (Exception ex)
            {
                Status = WorkStatus.Failed;
                Error = ex;
                return;
            }

            if (_callback != null)
            {
                if (result != null)
                {
                    if (_callback.Parameters == null)
                    {
                        _callback.Parameters = new[] {result};
                    }
                    else
                    {
                        var cbparams = _callback.Parameters;

                        Array.Resize(ref cbparams, cbparams.Length + 1);
                        cbparams[cbparams.Length - 1] = result;

                        _callback.Parameters = cbparams;
                    }
                }

                try
                {
                    _callback.Invoke(_onerror, "Thread Work Callback failed", threadName);
                }
                catch (Exception ex)
                {
                    Status = WorkStatus.Failed;
                    Error = ex;
                    return;
                }
            }

            Status = WorkStatus.Finished;
            Utils.Log(string.Format("Thread Finished Working {0}" , threadName));
        }
    }


    internal class ManagedJobDelegate
    {
        public ManagedJobDelegate(Delegate jobDelegate)
            : this(jobDelegate, null)
        { }

        public ManagedJobDelegate(Delegate jobDelegate, object[] parameters)
        {
            JobDelegate = jobDelegate;
            Parameters = parameters;
        }

        public Delegate JobDelegate { get; private set; }
        public object[] Parameters { get; set; }

        public object Invoke(ManagedJobDelegate onError, string errorMessage, string threadName)
        {
            if (JobDelegate == null)
                return null;

            try
            {
                return JobDelegate.DynamicInvoke(Parameters);
            }
            catch (Exception ex)
            {
                Utils.Log(string.Format("{0} {1}", errorMessage, threadName));
                Utils.Log(string.Format("Error: {0}", ex.Message));

                if (onError != null)
                {
                    Utils.Log(string.Format("On Error {0}", errorMessage));
                    onError.Invoke(null, errorMessage, threadName);
                }

                throw;
            }
        }
    }

    public enum WorkStatus
    {
        NotStarted,
        InProgress,
        Failed,
        Finished
    }
}
