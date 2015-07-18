using System;
using System.Threading;

namespace GTPool
{
    public sealed class GenericThreadPool<TMode> : IDisposable
        where TMode : IGtpMode, new()
    {
        static GenericThreadPool() { }
        
        private GenericThreadPool() { }

        private static GenericThreadPool<TMode> _instance = new GenericThreadPool<TMode>();
        private static CustomSettings _settings;
        private static TMode _gtpMode;

        //private static readonly object _queueLocker = new object();
        //private static readonly object _variableLocker = new object();

        public static GenericThreadPool<TMode> Instance
        {
            get { return _instance; }
        }

        public CustomSettings Settings
        {
            get { return _settings ?? (_settings = new CustomSettings()); }
        }

        public TMode GtpMode
        {
            get { return _gtpMode; }
        }

        
        public static GenericThreadPool<TMode> Init()
        {
            return Init(new CustomSettings());
        }

        public static GenericThreadPool<TMode> Init(CustomSettings settings)
        {
            InitializeInstance(settings);
            return _instance;
        }

        private static void InitializeInstance(CustomSettings settings)
        {
            if (_instance == null)
                _instance = new GenericThreadPool<TMode>();

            if (_settings == null)
                _settings = settings;

            _gtpMode = new TMode();
        }



        public void Dispose()
        {
            if (_gtpMode.WithWait)
            {
                // TODO: Stop waiting and execute all jobs and
                // TODO: Wait for all threads to finish working and destroy them all
            }

            _instance = null;
            _settings = null;
            _gtpMode = default(TMode);
        }
    }

    public class CustomSettings
    {
        private readonly int _minThreads;
        private const int DefaultMinThreads = 2;
        private readonly int _maxThreads;
        private const int DefaultMaxThreads = 50;
        private readonly int _idleTime;

        public CustomSettings()
            : this(DefaultMinThreads, DefaultMaxThreads, 5000)
        { }

        public CustomSettings(int minThreads, int maxThreads, int idleTime)
        {
            _minThreads = minThreads > DefaultMaxThreads
                ? DefaultMaxThreads
                : minThreads < DefaultMinThreads ? DefaultMinThreads : minThreads;

            _maxThreads = maxThreads < DefaultMinThreads
                ? DefaultMinThreads
                : maxThreads > DefaultMaxThreads ? DefaultMaxThreads : maxThreads;

            _minThreads = _minThreads > _maxThreads ? _maxThreads : _minThreads;

            _idleTime = idleTime;
        }

        public int MinThreads
        {
            get { return _minThreads; }
        }

        public int MaxThreads
        {
            get { return _maxThreads; }
        }

        public int IdleTime
        {
            get { return _idleTime; }
        }

    }
}
