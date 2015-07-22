using System;
using System.Threading;

namespace GTPool
{
    public class ManagedAsyncJob : ManagedJob
    {
        public ManagedAsyncJob(Delegate work, object[] parameters)
            : base(new GtpAsync(), work, parameters)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpAsync(), work, parameters, threadPriority, isBackground)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters)
            : base(new GtpAsync(), work, parameters, callBack, callBackParameters)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpAsync(), work, parameters, callBack, callBackParameters, threadPriority, isBackground)
        {
        }
    }

    public class ManagedSyncJob : ManagedJob
    {
        public ManagedSyncJob(Delegate work, object[] parameters) 
            : base(new GtpSync(), work, parameters)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpSync(), work, parameters, threadPriority, isBackground)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters)
            : base(new GtpSync(), work, parameters, callBack, callBackParameters)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpSync(), work, parameters, callBack, callBackParameters, threadPriority, isBackground)
        {
        }
    }

    public abstract class ManagedJob
    {
        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters)
            : this(gtpMode, work, parameters, null, null)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : this(gtpMode, work, parameters, null, null, threadPriority, isBackground)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters)
            : this(gtpMode, work, parameters, callBack, callBackParameters, ThreadPriority.Normal, true)
        { }

        protected ManagedJob(IGtpMode gtpMode, Delegate work, object[] parameters, Delegate callBack, object[] callBackParameters,
            ThreadPriority threadPriority, bool isBackground)
        {
            ThreadPriority = threadPriority;
            IsBackground = isBackground;
            GtpMode = gtpMode;

            _work = work;
            _parameters = parameters;
            _callback = callBack;
            _callbackParameters = callBackParameters;
        }

        private readonly Delegate _work;
        private object[] _parameters;
        private readonly Delegate _callback;
        private object[] _callbackParameters;

        public IGtpMode GtpMode { get; private set; }

        public ThreadPriority ThreadPriority { get; private set; }

        public bool IsBackground { get; private set; }

        public void DoWork(string threadName)
        {
            if (_parameters == null)
            {
                _parameters = new object[] {threadName};
            }
            else
            {
                Array.Resize(ref _parameters, _parameters.Length + 1);
                _parameters[_parameters.Length - 1] = threadName;
            }

            DoWork();
        }

        public void DoWork()
        {
            var result = _work.DynamicInvoke(_parameters);

            if (_callback != null)
            {
                if (result != null)
                {
                    if (_callbackParameters == null)
                    {
                        _callbackParameters = new[] {result};
                    }
                    else
                    {
                        Array.Resize(ref _callbackParameters, _callbackParameters.Length + 1);
                        _callbackParameters[_callbackParameters.Length - 1] = result;
                    }
                }

                _callback.DynamicInvoke(_callbackParameters);
            }
        }
    }
}
