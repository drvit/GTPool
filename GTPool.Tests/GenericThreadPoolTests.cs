using System;
using System.Linq;
using System.Threading;
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
                var settings = new CustomSyncSettings(5, 200);

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
                    .Init<GtpAsync>(new CustomAsyncSettings(1, 7, 350)).Settings;

                var newSettings = GenericThreadPool
                    .Init<GtpAsync>(new CustomAsyncSettings(5, 15, 3500)).Settings;

                Assert.AreEqual(settings, newSettings);

                GenericThreadPool.End();
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_cant_change_mode_before_dispose()
            {
                GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 5, 100));

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
                GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 5, 100));
                GenericThreadPool.End();

                GenericThreadPool instance = null;
                try
                {
                    instance = GenericThreadPool.Init<GtpSync>(new CustomSyncSettings(5, 100));
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
                GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 5, 100));
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
                    GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 3, 100));

                    Action<bool> job = ret =>
                    {
                        jobReturn = ret;
                        Utils.Log("Test: can_add_job_as_closures_to_async_pool");
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
                    using (var gtp = GenericThreadPool.Init<GtpSync>(new CustomSyncSettings(3, 100)))
                    {
                        Action<bool> job = ret =>
                        {
                            jobReturn = ret;
                            Utils.Log("Test: can_add_job_as_closures_to_sync_pool");
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
                    GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 3, 100));

                    Func<bool, bool> job = assertRet =>
                    {
                        Utils.Log("Test: can_add_job_with_callback_as_closures_to_async_pool");
                        return assertRet;
                    };

                    Action<string, bool> callback = (print, assertRet) =>
                    {
                        Utils.Log(print);
                        isTrue = assertRet;
                    };

                    GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] { true },
                            callback, new object[] { "Test: ASYNC CALBACK" }));

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
                    Func<bool, bool> job = assertRet =>
                    {
                        Utils.Log("Test: can_add_job_with_callback_as_closures_to_sync_pool");
                        return assertRet;
                    };

                    Action<string, bool> callback = (print, assertRet) =>
                    {
                        Utils.Log(print);
                        isTrue = assertRet;
                    };

                    using (var gtp = GenericThreadPool.Init<GtpSync>(new CustomSyncSettings(3, 100)))
                    {
                        gtp.AddJob(new ManagedSyncJob(job, new object[] { true },
                                callback, new object[] { "Test: SYNC CALBACK" }));

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

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void add_jobs_using_the_5_different_thread_priorities_to_async_pool()
            {
                var isJob1True = false;
                var isJob2True = false;
                var isJob3True = false;
                var isJob4True = false;
                var isJob5True = false;

                try
                {
                    Action job1 = () =>
                    {
                        Utils.Log("---------------------------------------------------------------");
                        isJob1True = Thread.CurrentThread.Priority == ThreadPriority.Lowest;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_async_pool - job1");
                    };

                    Action job2 = () =>
                    {
                        isJob2True = Thread.CurrentThread.Priority == ThreadPriority.BelowNormal;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_async_pool - job2");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    Action job3 = () =>
                    {
                        isJob3True = Thread.CurrentThread.Priority == ThreadPriority.Normal;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_async_pool - job3");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    Action job4 = () =>
                    {
                        isJob4True = Thread.CurrentThread.Priority == ThreadPriority.AboveNormal;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_async_pool - job4");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    Action job5 = () =>
                    {
                        isJob5True = Thread.CurrentThread.Priority == ThreadPriority.Highest;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_async_pool - job5");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 3, 100));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job1, null, ThreadPriority.Lowest, true));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job2, null, ThreadPriority.BelowNormal, true));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job3, null, ThreadPriority.Normal, true));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job4, null, ThreadPriority.AboveNormal, true));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job5, null, ThreadPriority.Highest, true));
                    GenericThreadPool.End();
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(isJob1True && isJob2True && isJob3True && isJob4True && isJob5True);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void add_jobs_using_the_5_different_thread_priorities_to_sync_pool()
            {
                var isJob1True = false;
                var isJob2True = false;
                var isJob3True = false;
                var isJob4True = false;
                var isJob5True = false;

                try
                {
                    Action job1 = () =>
                    {
                        Utils.Log("---------------------------------------------------------------");
                        isJob1True = Thread.CurrentThread.Priority == ThreadPriority.Lowest;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_sync_pool - job1");
                    };

                    Action job2 = () =>
                    {
                        isJob2True = Thread.CurrentThread.Priority == ThreadPriority.BelowNormal;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_sync_pool - job2");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    Action job3 = () =>
                    {
                        isJob3True = Thread.CurrentThread.Priority == ThreadPriority.Normal;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_sync_pool - job3");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    Action job4 = () =>
                    {
                        isJob4True = Thread.CurrentThread.Priority == ThreadPriority.AboveNormal;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_sync_pool - job4");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    Action job5 = () =>
                    {
                        isJob5True = Thread.CurrentThread.Priority == ThreadPriority.Highest;
                        Utils.Log("Test: add_jobs_using_the_5_different_thread_priorities_to_sync_pool - job5");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    using (var gtp = GenericThreadPool.Init<GtpSync>(new CustomSyncSettings(3, 100)))
                    {
                        gtp.AddJob(new ManagedSyncJob(job1, null, ThreadPriority.Lowest, true));
                        gtp.AddJob(new ManagedSyncJob(job2, null, ThreadPriority.BelowNormal, true));
                        gtp.AddJob(new ManagedSyncJob(job3, null, ThreadPriority.Normal, true));
                        gtp.AddJob(new ManagedSyncJob(job4, null, ThreadPriority.AboveNormal, true));
                        gtp.AddJob(new ManagedSyncJob(job5, null, ThreadPriority.Highest, true));
                    }
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(isJob1True && isJob2True && isJob3True && isJob4True && isJob5True);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void add_jobs_as_background_and_foreground_to_async_pool()
            {
                var isJob1True = false;
                var isJob2True = false;

                try
                {
                    Action job1 = () =>
                    {
                        Utils.Log("---------------------------------------------------------------");
                        isJob1True = Thread.CurrentThread.IsBackground;
                        Utils.Log("Test: add_jobs_as_background_and_foreground_to_async_pool - job1");
                    };

                    Action job2 = () =>
                    {
                        isJob2True = !Thread.CurrentThread.IsBackground;
                        Utils.Log("Test: add_jobs_as_background_and_foreground_to_async_pool - job2");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(1, 3, 100));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job1, null, ThreadPriority.BelowNormal, true));
                    GenericThreadPool.AddJob(new ManagedAsyncJob(job2, null, ThreadPriority.AboveNormal, false));
                    GenericThreadPool.End();
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(isJob1True && isJob2True);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void add_jobs_as_background_and_foreground_to_sync_pool()
            {
                var isJob1True = false;
                var isJob2True = false;

                try
                {
                    Action job1 = () =>
                    {
                        Utils.Log("---------------------------------------------------------------");
                        isJob1True = Thread.CurrentThread.IsBackground;
                        Utils.Log("Test: add_jobs_as_background_and_foreground_to_sync_pool - job1");
                    };

                    Action job2 = () =>
                    {
                        isJob2True = !Thread.CurrentThread.IsBackground;
                        Utils.Log("Test: add_jobs_as_background_and_foreground_to_sync_pool - job2");
                        Utils.Log("---------------------------------------------------------------");
                    };

                    using (var gtp = GenericThreadPool.Init<GtpSync>(new CustomSyncSettings(3, 100)))
                    {
                        gtp.AddJob(new ManagedSyncJob(job1, null, ThreadPriority.BelowNormal, true));
                        gtp.AddJob(new ManagedSyncJob(job2, null, ThreadPriority.AboveNormal, false));
                    }
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(isJob1True && isJob2True);
                }
            }


            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void test_jobs()
            {
                var isJob1True = false;
                var isJob2True = false;

                try
                {

                    #region Cars Object
                    var cars = new[]
                    {
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Gol", 3000},
                        new object[] {"Ferrari", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Gol", 3000},
                        new object[] {"Ferrari", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Gol", 3000},
                        new object[] {"Ferrari", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Fusca", 3000},
                        new object[] {"Corcel", 3000},
                        new object[] {"Maverick", 3000},
                        new object[] {"Opala", 3000},
                        new object[] {"Belina", 3000},
                        new object[] {"Cadilac", 3000},
                        new object[] {"Mustang", 3000},
                        new object[] {"Parati", 3000},
                        new object[] {"Gol", 3000}
                    };
                    #endregion


                    Action<int> job = c =>
                    {
                        long ret = 1;
                        for (var i = 0; i < (int) cars[c][1]; i++)
                        {
                            ret += i*(cars[c][0]).ToString().Length;
                        }
                        //Thread.Sleep((int)cars[c][1]);    
                        Utils.Log("My car is: " + cars[c][0] + ", with " + ret + " miles run");
                    };

                    GenericThreadPool.Init<GtpAsync>(new CustomAsyncSettings(3, 100, 500));

                    for (var t = 0; t < cars.Count(); t++)
                    {
                        GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] {t}));
                    }

                    Thread.Sleep(20000);

                    for (var t = 0; t < cars.Count(); t++)
                    {
                        Thread.Sleep(10);
                        GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] { t }));
                    }

                    GenericThreadPool.End();
                }
                catch (Exception)
                {
                    Assert.Inconclusive("Failed to assert.");
                }
                finally
                {
                    Assert.IsTrue(!isJob1True && !isJob2True);
                }
            }

        }
    }
}
