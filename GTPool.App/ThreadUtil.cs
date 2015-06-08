using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTPool.App
{
    public class ThreadUtil
    {

        /// <summary>
        /// Delegate to wrap another delegate and its arguments
        /// </summary>
        delegate void DelegateWrapper(Delegate d, object[] args);

        /// <summary>
        /// An instance of DelegateWrapper which calls InvokeWrappedDelegate,
        /// which in turn calls the DynamicInvoke method of the wrapped
        /// delegate.
        /// </summary>
        static readonly DelegateWrapper WrapperInstance = InvokeWrappedDelegate;

        /// <summary>
        /// Callback used to call <code>EndInvoke</code> on the asynchronously
        /// invoked DelegateWrapper.
        /// </summary>
        static readonly AsyncCallback Callback = EndWrapperInvoke;

        /// <summary>
        /// Executes the specified delegate with the specified arguments
        /// asynchronously on a thread pool thread.
        /// </summary>
        public static void FireAndForget(Delegate d, params object[] args)
        {
            // Invoke the wrapper asynchronously, which will then
            // execute the wrapped delegate synchronously (in the
            // thread pool thread)
            WrapperInstance.BeginInvoke(d, args, Callback, null);
        }

        /// <summary>
        /// Invokes the wrapped delegate synchronously
        /// </summary>
        static void InvokeWrappedDelegate(Delegate d, object[] args)
        {
            d.DynamicInvoke(args);
        }

        /// <summary>
        /// Calls EndInvoke on the wrapper and Close on the resulting WaitHandle
        /// to prevent resource leaks.
        /// </summary>
        static void EndWrapperInvoke(IAsyncResult ar)
        {
            WrapperInstance.EndInvoke(ar);
            ar.AsyncWaitHandle.Close();
        }
    }
}
