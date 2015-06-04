using System.Threading;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var job = new ThreadStart(ThreadJob);
            var thread = new Thread(job);
            thread.Start();

            for (int i = 0; i < 5; i++)
            {
                System.Console.WriteLine("Main thread: {0}", i);
                Thread.Sleep(1000);
            }
        }

        static void ThreadJob()
        {
            for (int i = 0; i < 10; i++)
            {
                TestConsoleApp.WriteLine("Other thread: {0}", i);
                Thread.Sleep(500);
            }
        }
    }
}
