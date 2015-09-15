namespace GTPool.App
{
    public class CalculateFibonacciSt
    {
        public static void Run(int fibonacciCalculations, int fibonacciSeed)
        {
            // One event is used for each Fibonacci object
            var fibArray = new Fibonacci[fibonacciCalculations];

            // Configure and launch threads using ThreadPool:
            //Console.WriteLine("launching {0} fibonacci tasks with seed in {1}", fibonacciCalculations, fibonacciSeed);

            for (var i = 0; i < fibonacciCalculations; i++)
            {
                var f = new Fibonacci(i + fibonacciSeed);
                fibArray[i] = f;
                f.Calculate();
            }

            // Wait for all threads in pool to calculation...
            //Console.WriteLine("All calculations are complete.");

            // Display the results...
            //for (var i = 0; i < fibonacciCalculations; i++)
            //{
            //    var f = fibArray[i];
            //    //Console.WriteLine("Fibonacci({0}) = {1}", f.N, f.FibOfN);
            //}
        }
    }
}
