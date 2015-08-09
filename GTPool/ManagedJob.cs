using System;
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

        public ManagedAsyncJob(Delegate work, object[] parameters, Action<GenericThreadPoolException> onError)
            : base(new GtpAsync(), work, parameters, onError)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Action<GenericThreadPoolException> onError, ThreadPriority threadPriority,
            bool isBackground)
            : base(new GtpAsync(), work, parameters, onError, threadPriority, isBackground)
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
            object[] callbackParameters, Action<GenericThreadPoolException> onerror)
            : base(new GtpAsync(), work, parameters, callback, callbackParameters, onerror)
        {
        }

        public ManagedAsyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, Action<GenericThreadPoolException> onerror, ThreadPriority threadPriority, bool isBackground)
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

        public ManagedSyncJob(Delegate work, object[] parameters, Action<GenericThreadPoolException> onError)
            : base(new GtpSync(), work, parameters, onError)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Action<GenericThreadPoolException> onError, ThreadPriority threadPriority,
            bool isBackground)
            : base(new GtpSync(), work, parameters, onError, threadPriority, isBackground)
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
            object[] callbackParameters, Action<GenericThreadPoolException> onerror)
            : base(new GtpSync(), work, parameters, callback, callbackParameters, onerror)
        {
        }

        public ManagedSyncJob(Delegate work, object[] parameters, Delegate callback,
            object[] callbackParameters, Action<GenericThreadPoolException> onerror, ThreadPriority threadPriority, bool isBackground)
            : base(new GtpSync(), work, parameters, callback, callbackParameters, onerror, threadPriority, isBackground)
        {
        }
    }

    public abstract class ManagedJob
    {
        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work)
            : this(gtpMode, work, null, null, null, null)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters)
            : this(gtpMode, work, parameters, null, null, null)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, Action<GenericThreadPoolException> onError)
            : this(gtpMode, work, parameters, null, null, onError)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, Action<GenericThreadPoolException> onError, ThreadPriority threadPriority, bool isBackground)
            : this(gtpMode, work, parameters, null, null, onError, threadPriority, isBackground)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : this(gtpMode, work, parameters, null, null, null, threadPriority, isBackground)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters)
            : this(gtpMode, work, parameters, callback, callbackParameters, null)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            ThreadPriority threadPriority, bool isBackground)
            : this(gtpMode, work, parameters, callback, callbackParameters, null, threadPriority, isBackground)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            Action<GenericThreadPoolException> onError)
            : this(gtpMode, work, parameters, callback, callbackParameters, onError, ThreadPriority.Normal, true)
        { }

        protected ManagedJob(GenericThreadPoolMode gtpMode, Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            Action<GenericThreadPoolException> onError, ThreadPriority threadPriority, bool isBackground)
        {
            ThreadPriority = threadPriority;
            IsBackground = isBackground;
            GtpMode = gtpMode;
            Status = WorkStatus.NotStarted;
            JobId = Utils.GenerateUniqueNumber();

            if (work != null)
                _work = new ManagedJobDelegate(work, parameters);
            else
                throw new GenericThreadPoolException(GenericThreadPoolExceptionType.MissingWork);

            if (callback != null)
                _callback = new ManagedJobDelegate(callback, callbackParameters);

            _onError = onError;
        }

        private readonly ManagedJobDelegate _work;
        private readonly ManagedJobDelegate _callback;
        private readonly Action<GenericThreadPoolException> _onError;

        public int JobId { get; private set; }

        public GenericThreadPoolMode GtpMode { get; private set; }

        public ThreadPriority ThreadPriority { get; private set; }

        public bool IsBackground { get; private set; }

        public WorkStatus Status { get; private set; }

        public GenericThreadPoolException Error { get; private set; }

        public void DoWork()
        {
            Status = WorkStatus.InProgress;
            Utils.Log("Thread Working");
            
            object result = null;

            try
            {
                result = _work.Invoke(_onError, "Thread Work failed");
            }
            catch (GenericThreadPoolException ex)
            {
                Error = ex;
            }
            catch (Exception ex)
            {
                Error = new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.ManagedJobException, ex, _work.Parameters);
            }

            if (Error == null)
            {
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
                        _callback.Invoke(_onError, "Thread Work Callback failed");
                    }
                    catch (GenericThreadPoolException ex)
                    {
                        Error = ex;
                    }
                    catch (Exception ex)
                    {
                        Error = new GenericThreadPoolException(
                            GenericThreadPoolExceptionType.ManagedJobException, ex, _callback.Parameters);
                    }
                }
            }

            Status = Error == null 
                ? WorkStatus.Failed
                : WorkStatus.Finished;

            Utils.Log("Thread Finished Working");
        }
    }

    internal class ManagedJobDelegate
    {
        internal ManagedJobDelegate(Delegate jobDelegate)
            : this(jobDelegate, null)
        { }

        internal ManagedJobDelegate(Delegate jobDelegate, object[] parameters)
        {
            JobDelegate = jobDelegate;
            Parameters = parameters;
        }

        internal Delegate JobDelegate { get; private set; }
        internal object[] Parameters { get; set; }

        internal object Invoke(Action<GenericThreadPoolException> onError, string errorMessage)
        {
            if (JobDelegate == null)
                return null;

            try
            {
                return JobDelegate.DynamicInvoke(Parameters);
            }
            catch (Exception ex)
            {
                var targetException = new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.ManagedJobException, ex.InnerException ?? ex, Parameters);

                Utils.Log(string.Format("{0}", errorMessage));
                Utils.Log(string.Format("Error: {0}", targetException.Message));

                if (onError != null)
                {
                    Utils.Log(string.Format("On Error {0}", errorMessage));

                    var mjd = new ManagedJobDelegate(onError, new object[] { targetException });
                    mjd.Invoke(null, errorMessage);

                    return null;
                }

                throw targetException;
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
