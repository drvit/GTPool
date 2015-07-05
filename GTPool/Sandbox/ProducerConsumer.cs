using System;
using System.Collections;
using System.Diagnostics;
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
                new object[] {"Fusca", 6000}, 
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
                Thread.Sleep(1000);
                pman.AddJob(job, new object[] { t, string.Empty });
            }

            Action<string> job2 = (wt) =>
            {
                Console.WriteLine("I have got " + cars.Count() + " cars - WT: " + wt);
            };

            pman.AddJob(job2, new object[] {string.Empty});


            Thread.Sleep(30000);

            Action<string> job3 = (wt) =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("I have got an other car that is not in the list. :) - WT: " + wt);
            };

            pman.AddJob(job3, new object[] { string.Empty });


            Action<string> job4 = (wt) =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("My other car is a MERCEDES. - WT: " + wt);
            };

            pman.AddJob(job4, new object[] { string.Empty });

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
        private readonly int _minIdleTime;
        private int _numberOfActiveThreads;
        private int _threadId;
        private bool _isThreadCreation;

        public ProducerConsumer()
            : this(2, 10, 10000, 1)
        {
        }

        public ProducerConsumer(int minThreads, int maxThreads, int idleTime, int minIdleTime)
        {
            _maxThreads = maxThreads;
            _idleTime = idleTime;
            _minIdleTime = minIdleTime;
            _minThreads = minThreads;
            _threadId = new Random().Next(10, 99);

            LoadThreadQueue(maxThreads);
        }

        private void LoadThreadQueue(int maxThreads, string wt)
        {
            Console.WriteLine("LoadThreadQueue - WT " + wt);
            LoadThreadQueue(maxThreads);
        }

        private void LoadThreadQueue(int maxThreads)
        {
            IsThreadCreation = true;

            while (NumberOfActiveThreads < maxThreads)
            {
                try
                {
                    NumberOfActiveThreads++;
                    var threadName = NextThreadName;

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

            IsThreadCreation = false;
        }

        private void JobInvoker(object threadName)
        {
            while (true)
            {
                var job = Consume();

                if (job == null)
                {
                    if (!IsMinNumberOfActiveThreads && !IsThreadCreation)
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

        private string NextThreadName
        {
            get
            {
                lock (_counterLock)
                {
                    _threadId++;
                    return "__thread_" + _threadId + "__";
                }
            }
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

        private bool IsThreadCreation
        {
            get
            {
                lock (_counterLock)
                {
                    return _isThreadCreation;
                }
            }
            set
            {
                lock (_counterLock)
                {
                    _isThreadCreation = value;
                }
            }
        }

        private bool IsMinNumberOfActiveThreads
        {
            get { return NumberOfActiveThreads == _minThreads; }
        }

        public void Produce(Tuple<Delegate, object[]> job)
        {
            Tuple<Delegate, object[]> createThreadsJob = null;

            // TODO: Must check if all threads are busy and number of active threads are smaller than max number of threads
            // TODO: then create threads on demand
            if (IsMinNumberOfActiveThreads)
                createThreadsJob = new Tuple<Delegate, object[]>(
                    (Action<int, string>)LoadThreadQueue, new object[] { _maxThreads, string.Empty });

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
            var idleTime = !IsThreadCreation && IsMinNumberOfActiveThreads ? _minIdleTime : _idleTime;

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
                    Monitor.Wait(_queueLock, idleTime);

                    if (_queue.Count == 0)
                        return null;
                }

                return _queue.Dequeue() as Tuple<Delegate, object[]>;
            }
        }
    }
}
