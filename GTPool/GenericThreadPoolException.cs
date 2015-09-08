using System;
using System.ComponentModel;

namespace GTPool
{
    public class GenericThreadPoolException : Exception
    {
        public GenericThreadPoolException(GenericThreadPoolExceptionType gtpException)
            : this(gtpException, null, null)
        { }

        public GenericThreadPoolException(GenericThreadPoolExceptionType gtpException, Exception inner)
            : this(gtpException, inner, null)
        { }

        public GenericThreadPoolException(GenericThreadPoolExceptionType gtpException, Exception inner, object[] jobParameters)
            : base(gtpException.ToDescription(), inner)
        {
            ExceptionType = gtpException;
            JobParameters = jobParameters;
        }

        public GenericThreadPoolExceptionType ExceptionType { get; private set; }

        public object[] JobParameters { get; private set; }
    }

    public enum GenericThreadPoolExceptionType
    {
        [Description("Exception thrown by Managed Job.")]
        ManagedJobException,
        [Description("Thread Pool already initialized in a different Mode")]
        IncompatibleGtpMode,
        [Description("Settings have been disposed or not initialized. Use Init() to initialize the configuration settings.")]
        SettingsNotInitialized,
        [Description("Instance is disposed.")]
        InstanceIsDisposed,
        [Description("Work must be provided when creating a managed job.")]
        MissingWork,
        [Description("Wrong GTP Sync Id.")]
        WrongSyncId,
        [Description("Job can't be null.")]
        JobIsNull,
        [Description("WaitAll handler only supports MTA Apartments")]
        WaitHandlerNotInMta
    }
}
