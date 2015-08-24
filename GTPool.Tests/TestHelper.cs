using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTPool.Tests
{
    public class TestHelper
    {
        public static void ExecuteTestInMta(Action test)
        {
            var objThread = new Thread(new ThreadStart(test));
            objThread.SetApartmentState(ApartmentState.MTA);
            objThread.Start();
            objThread.Join();
        }
    }
}
