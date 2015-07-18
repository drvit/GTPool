using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTPool.Tests
{
    [TestClass]
    [Category("GenericThreadPool")]
    public class GenericThreadPoolTest
    {
        [TestMethod]
        public void gtp_static_instance_exists()
        {
            Assert.IsNotNull(GenericThreadPool.Instance);
        }
    }
}
