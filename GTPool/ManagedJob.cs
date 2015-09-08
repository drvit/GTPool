using System;
using System.Threading;

namespace GTPool
{
    public class ManagedJob : IManagedJob
    {
        #region Constructors

        public ManagedJob(Delegate work)
            : this(work, null, null, null, null)
        { }

        public ManagedJob(Delegate work, object[] parameters)
            : this(work, parameters, null, null, null)
        { }

        public ManagedJob(Delegate work, object[] parameters, Action<GenericThreadPoolException> onError)
            : this(work, parameters, null, null, onError)
        { }

        public ManagedJob(Delegate work, object[] parameters, Action<GenericThreadPoolException> onError, ThreadPriority threadPriority,
            bool isBackground)
            : this(work, parameters, null, null, onError, threadPriority, isBackground)
        { }

        public ManagedJob(Delegate work, object[] parameters, ThreadPriority threadPriority, bool isBackground)
            : this(work, parameters, null, null, null, threadPriority, isBackground)
        { }

        public ManagedJob(Delegate work, object[] parameters, Delegate callback, object[] callbackParameters)
            : this(work, parameters, callback, callbackParameters, null)
        { }

        public ManagedJob(Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            ThreadPriority threadPriority, bool isBackground)
            : this(work, parameters, callback, callbackParameters, null, threadPriority, isBackground)
        { }

        public ManagedJob(Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            Action<GenericThreadPoolException> onError)
            : this(work, parameters, callback, callbackParameters, onError, ThreadPriority.Normal, true)
        { }

        public ManagedJob(Delegate work, object[] parameters, Delegate callback, object[] callbackParameters,
            Action<GenericThreadPoolException> onError, ThreadPriority threadPriority, bool isBackground)
        {
            ThreadPriority = threadPriority;
            IsBackground = isBackground;
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

        #endregion

        #region Private variables

        private readonly ManagedJobDelegate _work;
        private readonly ManagedJobDelegate _callback;
        private readonly Action<GenericThreadPoolException> _onError;

        #endregion

        public int JobId { get; private set; }

        public ThreadPriority ThreadPriority { get; private set; }

        public bool IsBackground { get; private set; }

        public WorkStatus Status { get; private set; }

        public GenericThreadPoolException Error { get; private set; }

        public void DoWork()
        {
            Status = WorkStatus.InProgress;
            //Utils.Log("Thread Working");
            
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
                    _callback.Parameters = AddParameter(result, _callback.Parameters);

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

            //Utils.Log("Thread Finished Working");
        }

        private static object[] AddParameter(object param, object[] parameters)
        {
            if (param != null)
            {
                if (parameters == null)
                {
                    return new[] { param };
                }

                var cbparams = parameters;

                Array.Resize(ref cbparams, cbparams.Length + 1);
                cbparams[cbparams.Length - 1] = param;

                return cbparams;
            }

            return null;
        }
    }

    internal class ManagedJobDelegate
    {
        internal ManagedJobDelegate(Delegate target)
            : this(target, null)
        { }

        internal ManagedJobDelegate(Delegate target, object[] parameters)
        {
            Target = target;
            Parameters = parameters;
        }

        internal Delegate Target { get; private set; }
        internal object[] Parameters { get; set; }

        internal object Invoke(Action<GenericThreadPoolException> onError, string errorMessage)
        {
            if (Target == null)
                return null;

            try
            {
                return Target.DynamicInvoke(Parameters);
            }
            catch (Exception ex)
            {
                var targetException = new GenericThreadPoolException(
                    GenericThreadPoolExceptionType.ManagedJobException, ex.InnerException ?? ex, Parameters);

                //Utils.Log(string.Format("{0}", errorMessage));
                //Utils.Log(string.Format("Error: {0}", targetException.InnerException));

                if (onError != null)
                {
                    //Utils.Log(string.Format("On Error {0}", errorMessage));

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
