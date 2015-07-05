using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GTPool.Sandbox
{
    public class Exercise12
    {
        public static void Run()
        {
            var cars = new [] 
            {
                new object[] {"Fusca", 15000}, 
                new object[] {"Corcel", 7000}, 
                new object[] {"Maverick", 1000}, 
                new object[] {"Opala", 4000},
                new object[] {"Belina", 3000}
            };

            Action<int, string> job = (c, wt) =>
            {
                Thread.Sleep((int)cars[c][1]);
                Console.WriteLine("My car is: " + cars[c][0] + " - WT: " + wt);
            };

            var pman = new PoolManager();
            for (var t = 0; t < cars.Count(); t++)
            {
                pman.AddJob(job, new object[] { t, string.Empty });
            }

            Action<string> job2 = (wt) =>
            {
                Console.WriteLine("I have got " + cars.Count() + " cars - WT: " + wt);
            };

            pman.AddJob(job2, new object[] {string.Empty});

            
        }
    }
    
    public class PoolManager
    {
        public PoolManager()
        {
            _queueJ = new ProducerConsumer();
        }

        private readonly ProducerConsumer _queueJ;
        
        private readonly object _threadsLock = new object();
        
        public void AddJob(Delegate job)
        {
            AddJob(job, null);
        }

        public void AddJob(Delegate job, object[] parameters)
        {
            AddJob(new Tuple<Delegate, object[]>(job, parameters));
        }

        private void AddJob(Tuple<Delegate, object[]> job)
        {
            lock (_threadsLock)
            {
                _queueJ.Produce(job);
            }
        }


    }


    /// -------------------------------------------------------------------------------
    public class ProducerConsumer
    {
        private readonly object _queueLock = new object();
        private readonly object _counterLock = new object();
        private readonly Queue _queue = new Queue();
        private readonly int _minThreads;
        private readonly int _maxThreads;
        private readonly int _idleTime;
        private int _numberOfActiveThreads;
        private static readonly Random ThreadId = new Random();

        public ProducerConsumer()
            : this(1, 10, 10000)
        {
        }

        public ProducerConsumer(int minThreads, int maxThreads, int idleTime)
        {
            _maxThreads = maxThreads;
            _idleTime = idleTime;
            _minThreads = minThreads;

            LoadThreadQueue(minThreads);
        }

        private void LoadThreadQueue(int maxThreads, string wt)
        {
            Console.WriteLine("LoadThreadQueue - WT " + wt);
            LoadThreadQueue(maxThreads);
        }

        private void LoadThreadQueue(int maxThreads)
        {
            while (NumberOfActiveThreads < maxThreads)
            {
                try
                {
                    NumberOfActiveThreads++;

                    var threadName = "__thread_" + ThreadId.Next(10000, 99999) + "__";

                    var thread = new Thread(JobInvoker)
                    {
                        Name = threadName,
                        IsBackground = true,
                        Priority = ThreadPriority.Normal
                    };

                    Console.WriteLine("Thread created " + threadName);
                    thread.Start(threadName);
                }
                catch
                {
                    NumberOfActiveThreads--;
                }
            }
        }

        private void JobInvoker(object threadName)
        {
            while (true)
            {
                var job = Consume();

                if (job == null)
                {
                    if (NumberOfActiveThreads > _minThreads)
                        break;
                }
                else
                {
                    //Console.WriteLine("Working Thread " + threadName);
                    job.Item2[job.Item2.Length - 1] = threadName;
                    job.Item1.DynamicInvoke(job.Item2);
                }
            }

            NumberOfActiveThreads--;
            Console.WriteLine("Thread ended " + threadName);
        }

        private int NumberOfActiveThreads
        {
            get
            {
                lock (_counterLock)
                {
                    return _numberOfActiveThreads;
                }
            }
            set
            {
                lock (_counterLock)
                {
                    _numberOfActiveThreads = value;
                }
            }
        }

        public void Produce(Tuple<Delegate, object[]> job)
        {
            Tuple<Delegate, object[]> createThreadsJob = null;

            if (NumberOfActiveThreads == _minThreads)
                createThreadsJob = new Tuple<Delegate, object[]>(
                    (Action<int, string>) LoadThreadQueue, new object[] {_maxThreads, string.Empty});

            lock (_queueLock)
            {
                if (createThreadsJob != null)
                    _queue.Enqueue(createThreadsJob);

                _queue.Enqueue(job);

                // We always need to pulse, even if the queue wasn't
                // empty before. Otherwise, if we add several items
                // in quick succession, we may only pulse once, waking
                // a single thread up, even if there are multiple threads
                // waiting for items.            
                Monitor.Pulse(_queueLock);
            }
        }

        public Tuple<Delegate, object[]> Consume()
        {
            lock (_queueLock)
            {
                // If the queue is empty, wait for an item to be added
                // Note that this is a while loop, as we may be pulsed
                // but not wake up before another thread has come in and
                // consumed the newly added object. In that case, we'll
                // have to wait for another pulse.
                if (_queue.Count == 0)
                {
                    // one thread calls Wait, which makes it block 
                    // until another thread calls Pulse or PulseAll

                    // This releases listLock, only reacquiring it
                    // after being woken up by a call to Pulse
                    Monitor.Wait(_queueLock, _idleTime);

                    if (_queue.Count == 0)
                        return null;
                }

                return _queue.Dequeue() as Tuple<Delegate, object[]>;
            }
        }
    }
}
