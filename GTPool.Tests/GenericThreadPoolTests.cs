using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTPool.Tests
{
    [TestClass]
    public class GenericThreadPoolTests
    {
        [TestClass]
        public class StaticInstance
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void static_instance_exists()
            {
                Assert.IsNotNull(GenericThreadPool.Instance);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_has_correct_settings()
            {
                var settings = new CustomSettings(3, 5, 200);

                using (var gtp = GenericThreadPool.Init<GtpSync>(settings))
                {
                    Assert.AreEqual(settings, gtp.Settings);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_cant_change_settings()
            {
                var settings = GenericThreadPool
                    .Init<GtpAsync>(new CustomSettings(1, 7, 350)).Settings;

                var newSettings = GenericThreadPool
                    .Init<GtpAsync>(new CustomSettings(5, 15, 3500)).Settings;

                Assert.AreEqual(settings, newSettings);

                GenericThreadPool.End();
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_cant_change_mode_before_dispose()
            {
                GenericThreadPool.Init<GtpAsync>(new CustomSettings(1, 5, 100));

                var modeException = new Exception();

                try
                {
                    GenericThreadPool.Init<GtpSync>();
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

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposed_instance_can_be_initialized_with_different_mode()
            {
                GenericThreadPool.Init<GtpAsync>(new CustomSettings(1, 5, 100));
                GenericThreadPool.End();

                GenericThreadPool instance = null;
                try
                {
                    instance = GenericThreadPool.Init<GtpSync>(new CustomSettings(1, 5, 100));
                    Assert.IsInstanceOfType(GenericThreadPool.Instance.GtpMode, typeof(GtpSync));
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    if (instance != null)
                        instance.Dispose();
                    else
                        Assert.Fail();
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposed_instance_is_disposed()
            {
                GenericThreadPool.Init<GtpAsync>(new CustomSettings(1, 5, 100));
                GenericThreadPool.End();

                Assert.IsNull(GenericThreadPool.Instance);
            }
        }

        [TestClass]
        public class AddJob
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_add_job_as_closures_to_async_pool()
            {
                var jobReturn = false;

                try
                {
                    GenericThreadPool.Init<GtpAsync>(new CustomSettings(1, 3, 100));

                    Action<bool, string> job = (ret, tn) =>
                    {
                        jobReturn = ret;
                        Trace.WriteLine("Test: can_add_job_as_closures_to_async_pool - " + tn);
                    };

                    GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] { true }));

                    GenericThreadPool.End();
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(jobReturn);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_add_job_as_closures_to_sync_pool()
            {
                var jobReturn = false;

                try
                {
                    using (var gtp = GenericThreadPool.Init<GtpSync>(new CustomSettings(1, 3, 100)))
                    {
                        Action<bool, string> job = (ret, tn) =>
                        {
                            jobReturn = ret;
                            Trace.WriteLine("Test: can_add_job_as_closures_to_sync_pool - " + tn);
                        };

                        gtp.AddJob(new ManagedSyncJob(job, new object[] { true }));
                    }
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(jobReturn);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_add_job_with_callback_as_closures_to_async_pool()
            {
                var isTrue = false;

                try
                {
                    GenericThreadPool.Init<GtpAsync>(new CustomSettings(1, 3, 100));

                    Func<bool, string, bool> job = (assertRet, tn) =>
                    {
                        Trace.WriteLine("Test: can_add_job_with_callback_as_closures_to_async_pool - " + tn);
                        return assertRet;
                    };

                    Action<string, bool> callback = (print, assertRet) =>
                    {
                        Trace.WriteLine(print);
                        isTrue = assertRet;
                    };

                    GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] { true },
                            callback, new object[] { "------------- Test: ASYNC CALBACK -------------" }));

                    GenericThreadPool.End();
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(isTrue);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_add_job_with_callback_as_closures_to_sync_pool()
            {
                var isTrue = false;

                try
                {
                    Func<bool, string, bool> job = (assertRet, tn) =>
                    {
                        Trace.WriteLine("Test: can_add_job_with_callback_as_closures_to_sync_pool - " + tn);
                        return assertRet;
                    };

                    Action<string, bool> callback = (print, assertRet) =>
                    {
                        Trace.WriteLine(print);
                        isTrue = assertRet;
                    };

                    using (var gtp = GenericThreadPool.Init<GtpSync>(new CustomSettings(1, 3, 100)))
                    {
                        gtp.AddJob(new ManagedSyncJob(job, new object[] { true },
                                callback, new object[] { "------------- Test: SYNC CALBACK -------------" }));

                    }
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(isTrue);
                }
            }
        }
    }
}
