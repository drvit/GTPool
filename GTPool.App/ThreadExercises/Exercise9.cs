using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.App.ThreadExercises
{
    public class Exercise9
    {
        delegate int TestDelegate(string parameter, int anyNumber);

        public static void Run()
        {
            var d = new TestDelegate(PrintOut);

            d.BeginInvoke("Hello", 4, Callback, d);

            // Give the callback time to execute - otherwise the app
            // may terminate before it is called
            Thread.Sleep(1000);
        }

        static int PrintOut(string parameter, int anyNumber)
        {
            Console.WriteLine(parameter + " - " + anyNumber);
            return 5;
        }

        static void Callback(IAsyncResult ar)
        {
            var d = (TestDelegate)ar.AsyncState;
            Console.WriteLine("Delegate returned {0}", d.EndInvoke(ar));
        }
    }

    public class Exercise91
    {
        delegate int TestDelegate(string parameter);

        public static void Run()
        {
            var d = new TestDelegate(PrintOut);

            var ar = d.BeginInvoke("Hello", null, null);

            Console.WriteLine("Main thread continuing to execute...");

            var result = d.EndInvoke(ar);

            Console.WriteLine("Delegate returned {0}", result);
        }

        static int PrintOut(string parameter)
        {
            Console.WriteLine(parameter);
            return 5;
        }
    }
}
