using System.Threading;

namespace GTPool.App
{
    public class Fibonacci
    {
        private readonly ManualResetEvent _doneEvent;

        public Fibonacci(int n)
        {
            N = n;
            _doneEvent = null;
        }

        public Fibonacci(int n, ManualResetEvent doneEvent)
        {
            N = n;
            _doneEvent = doneEvent;
        }

        // Wrapper method for use with thread pool.
        public void ThreadPoolCallback(object threadContext)
        {
            var threadIndex = (int)threadContext;
            //Console.WriteLine("thread {0} started...", threadIndex);
            FibOfN = Calculate(N);
            //Console.WriteLine("thread {0} result calculated...", threadIndex);

            if (_doneEvent != null)
                _doneEvent.Set();
        }

        // Recursive method that calculates the Nth Fibonacci number.
        public int Calculate(int n)
        {
            if (n <= 1)
            {
                return n;
            }

            return Calculate(n - 1) + Calculate(n - 2);
        }

        public int N { get; private set; }
        public int FibOfN { get; private set; }
    }
}
