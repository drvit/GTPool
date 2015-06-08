using System;
using System.Collections;
using System.Threading;

namespace GTPool.App
{
    public class Exercise6
    {
        static ProducerConsumer _queue;

        public static void Run()
        {
            _queue = new ProducerConsumer();
            new Thread(ConsumerJob).Start();

            var rng = new Random(0);
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine("Producing {0}", i);
                _queue.Produce(i);
                Thread.Sleep(rng.Next(1000));
            }
        }

        static void ConsumerJob()
        {
            // Make sure we get a different random seed from the
            // first thread
            var rng = new Random(1);
            // We happen to know we've only got 10 
            // items to receive
            for (var i = 0; i < 10; i++)
            {
                var o = _queue.Consume();
                Console.WriteLine("\t\t\t\tConsuming {0}", o);
                Thread.Sleep(rng.Next(1000));
            }
        }
    }

    public class ProducerConsumer
    {
        readonly object _listLock = new object();
        readonly Queue _queue = new Queue();

        public void Produce(object o)
        {
            lock (_listLock)
            {
                _queue.Enqueue(o);

                // We always need to pulse, even if the queue wasn't
                // empty before. Otherwise, if we add several items
                // in quick succession, we may only pulse once, waking
                // a single thread up, even if there are multiple threads
                // waiting for items.            
                Monitor.Pulse(_listLock);
            }
        }

        public object Consume()
        {
            lock (_listLock)
            {
                // If the queue is empty, wait for an item to be added
                // Note that this is a while loop, as we may be pulsed
                // but not wake up before another thread has come in and
                // consumed the newly added object. In that case, we'll
                // have to wait for another pulse.
                while (_queue.Count == 0)
                {
                    // one thread calls Wait, which makes it block 
                    // until another thread calls Pulse or PulseAll

                    // This releases listLock, only reacquiring it
                    // after being woken up by a call to Pulse
                    Monitor.Wait(_listLock);
                }
                return _queue.Dequeue();
            }
        }
    }
}
