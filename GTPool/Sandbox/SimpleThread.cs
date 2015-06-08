using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.Sandbox
{
    public class SimpleThread
    {
        public static void Main(IDictionary<string, int> results)
        {
            results = new Dictionary<string, int>();

            var thread = new Thread(() => ThreadJob(results));
            thread.Start(results);

            for (var i = 0; i < 5; i++)
            {
                results.Add("Main", i);
                Thread.Sleep(1000);
            }
        }
        
        public static void ThreadJob(IDictionary<string, int> results)
        {
            for (var i = 0; i < 10; i++)
            {
                results.Add("Other", i);
                Thread.Sleep(500);
            }
        }
    }
}
