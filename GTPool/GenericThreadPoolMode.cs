using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GTPool
{
    public abstract class GenericThreadPoolMode
    {
        public abstract bool WithWait { get; }

        public Delegate DisposeCallback { get; set; }

        public object[] DisposeCallbackParams { get; set; }

        public void InvokeDisposeCallback()
        {
            if (DisposeCallback != null)
                DisposeCallback.DynamicInvoke(DisposeCallbackParams);
        }
    }

    public class GtpSync : GenericThreadPoolMode
    {
        public override bool WithWait
        {
            get { return true; }
        }
    }

    public class GtpAsync : GenericThreadPoolMode
    {
        public override bool WithWait
        {
            get { return false; }
        }
    }
}
