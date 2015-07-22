using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GTPool
{
    public class GtpException : Exception
    {
        public GtpException(GtpExceptions gtpException)
            : base(gtpException.ToDescription())
        {
        }

        public GtpException(GtpExceptions gtpException, Exception inner)
            : base(gtpException.ToDescription(), inner)
        {
        }
    }

    public enum GtpExceptions
    {
        [Description("Thread Pool already initialized in a different Mode")]
        IncompatibleGtpMode,
        [Description("Settings have been disposed or not initialized. Use Init() to initialize the configuration settings.")]
        SettingsNotInitialized,
        [Description("Instance is disposed.")]
        InstanceIsDisposed
    }
}
