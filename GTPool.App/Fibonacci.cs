using System.Threading;
namespace GTPool.App
{
    public class Fibonacci
    {
        public Fibonacci(int n)
        {
            N = n;
        }

        // Wrapper method for use with thread pool.
        public void ThreadPoolCalculate(object manualResetEvent)
        {
            var doneEvent = manualResetEvent as ManualResetEvent;
            if (doneEvent != null)
            { 
                Calculate();
                doneEvent.Set();
            }
        }

        public void Calculate()
        {
            FibOfN = CalculateFibonacciNumber(N);
        }

        // Recursive method that calculates the Nth Fibonacci number.
        private static int CalculateFibonacciNumber(int n)
        {
            if (n <= 1)
            {
                return n;
            }

            return CalculateFibonacciNumber(n - 1) + CalculateFibonacciNumber(n - 2);
        }

        private int N { get; set; }
        public int FibOfN { get; private set; }
    }
}
