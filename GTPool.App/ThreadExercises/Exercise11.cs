using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.App.ThreadExercises
{
    public class Exercise11
    {
        public static void Run()
        {
            Console.WriteLine("Started at {0:HH:mm:ss.fff}", DateTime.Now);
            // Start in three seconds, then fire every one second
            using (var timer = new Timer(Tick, null, 3000, 1000))
            {

                // Wait for 10 seconds
                Thread.Sleep(10000);

                // Then go slow for another 10 seconds
                timer.Change(0, 2000);
                Thread.Sleep(10000);
            }

            // The timer will now have been disposed automatically due to the using
            // statement, so there won't be any other threads running, and we'll quit.
        }

        static void Tick(object state)
        {
            Console.WriteLine("Ticked at {0:HH:mm:ss.fff}", DateTime.Now);
        }
    }
}
