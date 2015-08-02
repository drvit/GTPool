﻿using System;

namespace GTPool
{
    public class GenericThreadPoolSettings
    {
        private const int DefaultMinThreads = 1;
        private const int DefaultMaxThreads = 200;
        private const int DefaultIdleTime = 500;
        private const int MinIdleTime = 1;
        private const int MaxIdleTime = 1800000;

        public GenericThreadPoolSettings()
            : this(DefaultMinThreads, DefaultMaxThreads, DefaultIdleTime)
        { }

        public GenericThreadPoolSettings(int numberOfThreads, int idleTime)
            : this(DefaultMinThreads, numberOfThreads, idleTime)
        { }

        public GenericThreadPoolSettings(int minThreads, int maxThreads, int idleTime)
        {
            MaxThreads = Math.Min(Math.Max(DefaultMinThreads, maxThreads), DefaultMaxThreads);
            MinThreads = Math.Max(Math.Min(DefaultMaxThreads, minThreads), DefaultMinThreads);
            MinThreads = Math.Min(MinThreads, MaxThreads);

            IdleTime = Math.Min(Math.Max(MinIdleTime, idleTime), MaxIdleTime);
        }

        public int MinThreads { get; private set; }

        public int MaxThreads { get; private set; }

        public int NumberOfThreads
        {
            get { return MaxThreads; }
        }

        public int IdleTime { get; private set; }
    }
}
