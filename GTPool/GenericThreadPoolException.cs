using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GTPool
{
    public class GenericThreadPoolException : Exception
    {
        public GenericThreadPoolException(GenericThreadPoolExceptionType gtpException)
            : base(gtpException.ToDescription())
        {
        }

        public GenericThreadPoolException(GenericThreadPoolExceptionType gtpException, Exception inner)
            : base(gtpException.ToDescription(), inner)
        {
        }
    }

    public enum GenericThreadPoolExceptionType
    {
        [Description("Thread Pool already initialized in a different Mode")]
        IncompatibleGtpMode,
        [Description("Settings have been disposed or not initialized. Use Init() to initialize the configuration settings.")]
        SettingsNotInitialized,
        [Description("Instance is disposed.")]
        InstanceIsDisposed,
        [Description("Work must be provided when creating a managed job.")]
        MissingWork,
        [Description("Job can't be null.")]
        JobIsNull
    }
}
