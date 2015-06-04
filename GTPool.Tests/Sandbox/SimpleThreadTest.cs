using System.Collections.Generic;
using GTPool.Sandbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTPool.Tests.Sandbox
{
    [TestClass]
    public class SimpleThreadTest
    {
        [TestMethod] 
        public void TestSimpleThreadClass()
        {
            var threadJobClass = new SimpleThread();
            Assert.IsInstanceOfType(threadJobClass, typeof(SimpleThread));
        }

        [TestMethod]
        public void TestSimpleThreadReturnsResult()
        {
            IDictionary<string, int> results = new Dictionary<string, int>();
            SimpleThread.Main(results);

            Assert.IsNotNull(results);
        }
    }
}
