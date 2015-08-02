using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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
                GenericThreadPool.Init();
                Assert.IsNotNull(GenericThreadPool.Current);
                GenericThreadPool.End();
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_has_correct_settings()
            {
                const int numberOfThreads = 5;
                const int idleTime = 200;

                using (var gtp = GenericThreadPool.Init<GtpSync>(numberOfThreads, idleTime))
                {
                    Assert.IsTrue(gtp.Settings.NumberOfThreads == numberOfThreads && gtp.Settings.IdleTime == idleTime);
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_cant_change_settings()
            {
                const int minThreads = 1;
                const int maxThreads = 7;
                const int idleTime = 350;

                const int newMinThreads = 5;
                const int newMaxThreads = 15;
                const int newIdleTime = 560;

                var gtp = GenericThreadPool
                    .Init<GtpAsync>(minThreads, maxThreads, idleTime);

                var newGtp = GenericThreadPool
                    .Init<GtpAsync>(newMinThreads, newMaxThreads, newIdleTime);

                Assert.IsTrue(
                    gtp.Settings.MinThreads == newGtp.Settings.MinThreads && newGtp.Settings.MinThreads != newMinThreads &&
                    gtp.Settings.MaxThreads == newGtp.Settings.MaxThreads && newGtp.Settings.MaxThreads != newMaxThreads &&
                    gtp.Settings.IdleTime == newGtp.Settings.IdleTime && newGtp.Settings.IdleTime != newIdleTime);

                GenericThreadPool.End();
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_cant_change_mode_before_dispose()
            {
                GenericThreadPool.Init<GtpAsync>(1, 5, 100);

                var modeException = new Exception();

                try
                {
                    GenericThreadPool.Init<GtpSync>(5, 300);
                }
                catch (GenericThreadPoolException ex)
                {
                    modeException = ex;
                }
                catch (Exception ex)
                {
                    modeException = ex;
                }
                finally
                {
                    Assert.AreEqual(modeException.Message,
                        GenericThreadPoolExceptionType.IncompatibleGtpMode.ToDescription());
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposed_instance_can_be_initialized_with_different_mode()
            {
                GenericThreadPool.Init<GtpAsync>(1, 5, 100);
                GenericThreadPool.End();

                GenericThreadPool instance = null;
                try
                {
                    instance = GenericThreadPool.Init<GtpSync>(5, 100);
                    Assert.IsInstanceOfType(GenericThreadPool.Current.GtpMode, typeof (GtpSync));
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
                GenericThreadPool.Init<GtpAsync>(1, 5, 100);
                GenericThreadPool.End();

                Assert.IsNull(GenericThreadPool.Current);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposing_async_pool_can_call_dispose_callback()
            {
                var jobReturn = false;

                Action<bool> disposeCallback = ret =>
                {
                    jobReturn = ret;
                    Utils.Log("Test: disposing_async_pool_can_call_dispose_callback");
                };

                GenericThreadPool.Init<GtpAsync>(1, 3, 100, disposeCallback, new object[] {true});

                GenericThreadPool.End();

                Assert.IsTrue(jobReturn);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposing_sync_pool_can_call_dispose_callback()
            {
                var jobReturn = false;

                Action<bool> disposeCallback = ret =>
                {
                    jobReturn = ret;
                    Utils.Log("Test: disposing_sync_pool_can_call_dispose_callback");
                };

                var gtp = GenericThreadPool.Init<GtpSync>(2, 100, disposeCallback, new object[] {true});

                gtp.Dispose();

                Assert.IsTrue(jobReturn);
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
                    GenericThreadPool.Init<GtpAsync>(1, 3, 100);

                    Action<bool> job = ret =>
                    {
                        jobReturn = ret;
                        Utils.Log("Test: can_add_job_as_closures_to_async_pool");
                    };

                    GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] {true}));

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
                    using (var gtp = GenericThreadPool.Init<GtpSync>(3, 100))
                    {
                        Action<bool> job = ret =>
                        {
                            jobReturn = ret;
                            Utils.Log("Test: can_add_job_as_closures_to_sync_pool");
                        };

                        gtp.AddJob(new ManagedSyncJob(job, new object[] {true}));
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
                    GenericThreadPool.Init<GtpAsync>(1, 3, 100);

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

                    GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] {true},
                        callback, new object[] {"Test: ASYNC CALBACK"}));

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

                    using (var gtp = GenericThreadPool.Init<GtpSync>(3, 100))
                    {
                        gtp.AddJob(new ManagedSyncJob(job, new object[] {true},
                            callback, new object[] {"Test: SYNC CALBACK"}));

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

                    GenericThreadPool.Init<GtpAsync>(1, 3, 100);
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

                    using (var gtp = GenericThreadPool.Init<GtpSync>(3, 100))
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

                    GenericThreadPool.Init<GtpAsync>(1, 3, 100);
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

                    using (var gtp = GenericThreadPool.Init<GtpSync>(3, 100))
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
            public void added_jobs_with_higher_priority_are_picked_first()
            {
                var isCorrectOrder = true;
                var nextPriority = 1;

                Action job1 = () =>
                {
                    isCorrectOrder = isCorrectOrder && nextPriority == 5;
                    nextPriority++;
                    Utils.Log("Test: jobs_with_higher_priority_are_picked_first - ThreadPriority.Lowest");
                };

                Action job2 = () =>
                {
                    isCorrectOrder = isCorrectOrder && nextPriority == 4;
                    nextPriority++;
                    Utils.Log("Test: jobs_with_higher_priority_are_picked_first - ThreadPriority.BelowNormal");
                };

                Action job3 = () =>
                {
                    isCorrectOrder = isCorrectOrder && nextPriority == 3;
                    nextPriority++;
                    Utils.Log("Test: jobs_with_higher_priority_are_picked_first - ThreadPriority.Normal");
                };

                Action job4 = () =>
                {
                    isCorrectOrder = isCorrectOrder && nextPriority == 2;
                    nextPriority++;
                    Utils.Log("Test: jobs_with_higher_priority_are_picked_first - ThreadPriority.AboveNormal");
                };

                Action job5 = () =>
                {
                    isCorrectOrder = isCorrectOrder && nextPriority == 1;
                    nextPriority++;
                    Utils.Log("Test: jobs_with_higher_priority_are_picked_first - ThreadPriority.Highest");
                };

                using (var gtp = GenericThreadPool.Init<GtpSync>(1, 300))
                {
                    gtp.AddJob(new ManagedSyncJob(job1, null, ThreadPriority.Lowest, true));
                    gtp.AddJob(new ManagedSyncJob(job2, null, ThreadPriority.BelowNormal, true));
                    gtp.AddJob(new ManagedSyncJob(job3, null, ThreadPriority.Normal, true));
                    gtp.AddJob(new ManagedSyncJob(job4, null, ThreadPriority.AboveNormal, true));
                    gtp.AddJob(new ManagedSyncJob(job5, null, ThreadPriority.Highest, true));
                }

                Assert.IsTrue(isCorrectOrder);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void added_jobs_exception_is_handled_by_onerror_callback()
            {
                var onErrorWasExecuted = true;
                const string exceptionMessge = "Exception from can_cancel_all_jobs_that_have_not_been_picked";

                using (var gtp = GenericThreadPool.Init<GtpSync>(4, 350))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        gtp.AddJob(new ManagedSyncJob(
                            (Action<int>) (index =>
                            {
                                Utils.Log(
                                    "Test: can_cancel_all_jobs_that_have_not_been_picked - WORK Method - index = " +
                                    index);
                                if (index == 6)
                                {
                                    Utils.Log(
                                        "Test: can_cancel_all_jobs_that_have_not_been_picked - Throw Exception - index = " +
                                        index);
                                    throw new Exception(exceptionMessge);
                                }
                            }),
                            new object[] {i},
                            ex =>
                            {
                                onErrorWasExecuted = onErrorWasExecuted && exceptionMessge.Equals(ex.Message);
                                Utils.Log("Exception Handler; " + ex.Message);
                            }));
                    }
                }

                Assert.IsTrue(onErrorWasExecuted);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void added_jobs_callbacks_exception_is_handled_by_onerror_calback()
            {
                var onErrorWasExecuted = true;
                const string exceptionMessge = "Exception from jobs_callbacks_exception_is_handled_by_onerror_calback";

                using (var gtp = GenericThreadPool.Init<GtpSync>(3, 150))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        gtp.AddJob(new ManagedSyncJob(
                            (Func<int, int>) (index =>
                            {
                                Utils.Log(
                                    "Test: jobs_callbacks_exception_is_handled_by_onerror_calback - WORK Method - index = " +
                                    index);
                                return index;
                            }),
                            new object[] {i},
                            (Action<int>) (index =>
                            {
                                Utils.Log(
                                    "Test: jobs_callbacks_exception_is_handled_by_onerror_calback - CALLBACK Method - index = " +
                                    index);
                                if (index == 7)
                                {
                                    Utils.Log(
                                        "Test: jobs_callbacks_exception_is_handled_by_onerror_calback - CALLBACK Method - Throw Exception - index = " +
                                        index);
                                    throw new Exception(exceptionMessge);
                                }
                            }),
                            null,
                            ex =>
                            {
                                onErrorWasExecuted = onErrorWasExecuted && exceptionMessge.Equals(ex.Message);
                                Utils.Log("Exception Handler; " + ex.Message);
                            }));
                    }
                }

                Assert.IsTrue(onErrorWasExecuted);
            }
        }

        [TestClass]
        public class CancelJob
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_cancel_jobs_that_have_not_been_picked()
            {
                try
                {
                    var executedJobs = new int[4];
                    var index = -1;
                    var managedJobs = new List<ManagedAsyncJob>();

                    Action<int> job = jobNumber =>
                    {
                        index++;
                        executedJobs[index] = jobNumber;
                        Thread.Sleep(1000);
                        Utils.Log("Test: can_cancel_jobs_that_have_not_been_picked - jobNumber = " + jobNumber);
                    };

                    GenericThreadPool.Init<GtpAsync>(1, 2, 300);

                    for (var i = 0; i <= 5; i++)
                    {
                        var managedJob = new ManagedAsyncJob(job, new object[] {i});
                        managedJobs.Add(managedJob);
                        GenericThreadPool.AddJob(managedJob);
                    }

                    GenericThreadPool.CancelJob(managedJobs[3]);
                    GenericThreadPool.CancelJob(managedJobs[5]);

                    GenericThreadPool.End();

                    Assert.IsTrue(index == 3 &&
                                  executedJobs[0] + executedJobs[1] + executedJobs[2] + executedJobs[3] == 7);
                }
                catch (Exception)
                {
                    Assert.Fail("Executed more jobs than it should.");
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_cancel_all_jobs_that_have_not_been_picked()
            {
                try
                {
                    var executedJobs = new int[6];
                    var index = -1;

                    Action<int> job = jobNumber =>
                    {
                        index++;
                        executedJobs[index] = jobNumber;
                        Thread.Sleep(500);
                        Utils.Log("Test: can_cancel_all_jobs_that_have_not_been_picked - jobNumber = " + jobNumber);
                    };

                    GenericThreadPool.Init<GtpAsync>(1, 2, 300);

                    for (var i = 0; i <= 5; i++)
                    {
                        var managedJob = new ManagedAsyncJob(job, new object[] {i});
                        GenericThreadPool.AddJob(managedJob);
                    }

                    Thread.Sleep(1000);
                    GenericThreadPool.CancellAllJobs();
                    GenericThreadPool.End();

                    Assert.IsTrue(index > 0 && index < 6);
                }
                catch (Exception)
                {
                    Assert.Fail("Executed more jobs than it should.");
                }
            }
        }

        [TestClass]
        public class StressTest
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void stress_test_generic_thread_pool()
            {
                var isJob1True = false;
                var isJob2True = false;

                try
                {

                    //WaitHandle.WaitAll()
                    //ManualResetEvent

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
                        for (var i = 0; i < (int)cars[c][1]; i++)
                        {
                            ret += i * (cars[c][0]).ToString().Length;
                        }
                        Thread.Sleep(100);
                        Utils.Log(string.Format("My car is: {0} with {1} miles run - Thread: {2}", cars[c][0], ret,
                            Thread.CurrentThread.Name));
                    };

                    GenericThreadPool.Init<GtpAsync>(5, 100, 5000);

                    for (var t = 0; t < cars.Count(); t++)
                    {
                        GenericThreadPool.AddJob(new ManagedAsyncJob(job, new object[] { t }));
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
