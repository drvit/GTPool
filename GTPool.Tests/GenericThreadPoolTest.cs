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
    public class GenericThreadPoolTest
    {
        [TestMethod]
        [TestCategory("GenericThreadPool")]
        public void static_instance_exists()
        {
            Assert.IsNotNull(GenericThreadPool<GtpAsync>.Instance);
        }

        [TestMethod]
        [TestCategory("GenericThreadPool")]
        public void initialized_instance_has_correct_settings()
        {
            var settings = new CustomSettings(5, 15, 3000);

            using (var gtp = GenericThreadPool<GtpAsync>.Init(settings))
            {
                Assert.AreEqual(settings, gtp.Settings);
            }
        }

        [TestMethod]
        [TestCategory("GenericThreadPool")]
        public void initialized_instance_cant_change_settings()
        {
            var settings = GenericThreadPool<GtpAsync>.Init().Settings;

            var newSettings = GenericThreadPool<GtpAsync>
                .Init(new CustomSettings(5, 15, 3500)).Settings;

            Assert.AreEqual(settings, newSettings);

            GenericThreadPool<GtpAsync>.Instance.Dispose();
        }

        [TestMethod]
        [TestCategory("GenericThreadPool")]
        public void initialized_instance_cant_change_mode_before_dispose()
        {
            GenericThreadPool<GtpAsync>.Init();

            var modeException = new Exception();

            try
            {
                GenericThreadPool<GtpSync>.Init();
            }
            catch (GtpException ex)
            {
                modeException = ex;
            }
            catch (Exception ex)
            {
                modeException = ex;
            }
            finally
            {
                Assert.AreEqual(modeException.Message, GtpExceptions.IncompatibleGtpMode.ToDescription());
            }
        }

    }
}
