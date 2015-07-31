using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTPool.Sandbox
{
    public class Program
    {
        Program()
        {
            var gtp = GenericThreadPool.Init(10);

            GenericThreadPool.Init<GtpSync>(3, 3000);
        }
    }

    public sealed class GenericThreadPool : IDisposable
    {
        private static GenericThreadPool _instance = new GenericThreadPool();
        static GenericThreadPool() { }
        private GenericThreadPool() { }
        private static IGtpMode _gtpMode;

        public static GenericThreadPool Init(int maxThreads)
        {
            return Init<GtpAsync>(1, maxThreads, 300);
        }

        public static GenericThreadPool Init<TMode>(int numberOfThreads, int idleTime)
            where TMode : GtpSync, new()
        {
            
            _gtpMode = new TMode();
            return _instance;
        }

        public static GenericThreadPool Init<TMode>(int minThreads, int maxThreads, int idleTime)
            where TMode : GtpAsync, new()
        {
            _gtpMode = new TMode();
            return _instance;
        }


        public void Dispose()
        {
            if (_gtpMode.GetType() == typeof(GtpSync))
                throw new NotImplementedException();
        }
    }

    public class CustomSettings
    {
        public CustomSettings() { }
    }
    

    public interface IGtpMode
    {
        bool WithWait { get; }

        Delegate DisposeCallback { get; }
    }

    public class GtpSync : IGtpMode
    {
        public bool WithWait
        {
            get { return true; }
        }

        public Delegate DisposeCallback
        {
            get { return null; }
        }
    }

    public class GtpAsync : IGtpMode
    {
        public bool WithWait
        {
            get { return false; }
        }

        public Delegate DisposeCallback
        {
            get { return null; }
        }
    }
}
