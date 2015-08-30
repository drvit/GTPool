using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTPool.Tests
{
    [TestClass]
    public class GenericThreadPoolTests
    {
        protected static void LogTestRunning([CallerMemberName]string memberName = "")
        {
            Utils.Log("______________________________________________________________");
            Utils.Log(string.Format("Test: {0}", memberName));
        }

        [ClassInitialize]
        public void Initialize()
        {
        }

        [ClassCleanup]
        public void CleanUp()
        {
            Utils.WaitLoggingToFinish();
        }

        [TestClass]
        public class StaticInstance
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void static_instance_exists()
            {
                LogTestRunning();

                Assert.IsNotNull(GenericThreadPool.Current);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_has_correct_settings()
            {
                LogTestRunning();

                //TestHelper.ExecuteTestInMta(() =>
                //{
                    const int minThreads = 3;
                    const int maxThreads = 5;
                    const int idleTime = 565;

                    try
                    {
                        GenericThreadPool.Init(minThreads, maxThreads, idleTime);
                        Assert.AreEqual(GenericThreadPool.Settings.MinThreads, minThreads);
                        Assert.AreEqual(GenericThreadPool.Settings.MaxThreads, maxThreads);
                        Assert.AreEqual(GenericThreadPool.Settings.IdleTime, idleTime);
                        GenericThreadPool.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail("Error: " + ex.Message);
                    }

                //});
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void initialized_instance_cant_be_initialized_again()
            {
                LogTestRunning();
                
                const int minThreads = 1;
                const int maxThreads = 7;
                const int idleTime = 350;

                const int newMinThreads = 5;
                const int newMaxThreads = 15;
                const int newIdleTime = 560;

                GenericThreadPool.Init(minThreads, maxThreads, idleTime);
                var gtpSettings = GenericThreadPool.Settings;

                GenericThreadPool.Init(newMinThreads, newMaxThreads, newIdleTime);
                var newGtpSettings = GenericThreadPool.Settings;

                Assert.IsTrue(
                    gtpSettings.MinThreads == newGtpSettings.MinThreads && newGtpSettings.MinThreads != newMinThreads &&
                    gtpSettings.MaxThreads == newGtpSettings.MaxThreads && newGtpSettings.MaxThreads != newMaxThreads &&
                    gtpSettings.IdleTime == newGtpSettings.IdleTime && newGtpSettings.IdleTime != newIdleTime);

                GenericThreadPool.Shutdown();
            }
            
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposed_instance_can_be_initialized()
            {
                LogTestRunning();

                const int minThreads = 1;
                const int maxThreads = 7;
                const int idleTime = 350;

                GenericThreadPool.Init(minThreads, maxThreads, idleTime);
                GenericThreadPool.Shutdown();

                const int newMinThreads = 5;
                const int newMaxThreads = 15;
                const int newIdleTime = 560;

                try
                {
                    GenericThreadPool.Init(newMinThreads, newMaxThreads, newIdleTime);
                    Assert.IsNotNull(GenericThreadPool.Settings);
                    Assert.AreEqual(GenericThreadPool.Settings.MinThreads, newMinThreads);
                    Assert.AreEqual(GenericThreadPool.Settings.MaxThreads, newMaxThreads);
                    Assert.AreEqual(GenericThreadPool.Settings.IdleTime, newIdleTime);
                    GenericThreadPool.Shutdown();
                }
                catch (Exception)
                {
                    if (GenericThreadPool.IsInitialized)
                        GenericThreadPool.Shutdown();

                    Assert.Inconclusive("Failed to assert.");
                }
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposed_instance_is_really_disposed()
            {
                LogTestRunning();

                GenericThreadPool.Init(1, 5, 100);
                GenericThreadPool.Shutdown();

                Assert.IsNull(GenericThreadPool.Settings);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void disposing_thread_pool_must_call_dispose_callback_method()
            {
                LogTestRunning();

                var jobReturn = false;

                Action<bool> disposeCallback = ret =>
                {
                    jobReturn = ret;
                    Utils.Log("Test: disposing_async_pool_can_call_dispose_callback");
                };

                GenericThreadPool.Init(1, 3, 100, disposeCallback, new object[] {true});

                GenericThreadPool.Shutdown();

                Assert.IsTrue(jobReturn);
            }
        }

        [TestClass]
        public class AddJob
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_add_job_as_closures_to_thread_pool()
            {
                LogTestRunning();

                var jobReturn = false;

                try
                {
                    GenericThreadPool.Init(minThreads: 3, maxThreads: 20, idleTime: 1000);

                    Action<bool> job = ret =>
                    {
                        jobReturn = ret;
                        Utils.Log("Test: can_add_job_as_closures_to_async_pool");
                    };

                    GenericThreadPool.AddJob(new ManagedJob(job, new object[] {true}));

                    GenericThreadPool.Shutdown();
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
            public void can_add_job_with_callback_as_closures_to_thread_pool()
            {
                LogTestRunning();

                var isTrue = false;

                try
                {
                    GenericThreadPool.Init(1, 3, 100);

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

                    GenericThreadPool.AddJob(new ManagedJob(job, new object[] {true},
                        callback, new object[] {"Test: ASYNC CALBACK"}));

                    GenericThreadPool.Shutdown();
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
            public void add_jobs_using_the_5_different_thread_priorities_to_thread_pool()
            {
                LogTestRunning();

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

                    GenericThreadPool.Init(1, 3, 100);
                    GenericThreadPool.AddJob(new ManagedJob(job1, null, ThreadPriority.Lowest, true));
                    GenericThreadPool.AddJob(new ManagedJob(job2, null, ThreadPriority.BelowNormal, true));
                    GenericThreadPool.AddJob(new ManagedJob(job3, null, ThreadPriority.Normal, true));
                    GenericThreadPool.AddJob(new ManagedJob(job4, null, ThreadPriority.AboveNormal, true));
                    GenericThreadPool.AddJob(new ManagedJob(job5, null, ThreadPriority.Highest, true));
                    GenericThreadPool.Shutdown();
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
                LogTestRunning();

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

                    GenericThreadPool.Init(1, 3, 100);
                    GenericThreadPool.AddJob(new ManagedJob(job1, null, ThreadPriority.BelowNormal, true));
                    GenericThreadPool.AddJob(new ManagedJob(job2, null, ThreadPriority.AboveNormal, false));
                    GenericThreadPool.Shutdown();
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
            public void added_jobs_with_higher_priority_are_picked_up_first()
            {
                LogTestRunning();

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

                GenericThreadPool.Init(1, 1, 50);
                GenericThreadPool.AddJob(new ManagedJob(job1, null, ThreadPriority.Lowest, true));
                GenericThreadPool.AddJob(new ManagedJob(job2, null, ThreadPriority.BelowNormal, true));
                GenericThreadPool.AddJob(new ManagedJob(job3, null, ThreadPriority.Normal, true));
                GenericThreadPool.AddJob(new ManagedJob(job4, null, ThreadPriority.AboveNormal, true));
                GenericThreadPool.AddJob(new ManagedJob(job5, null, ThreadPriority.Highest, true));
                GenericThreadPool.Shutdown();

                Assert.IsTrue(isCorrectOrder);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void added_job_with_exception_handling_method_is_handled_by_onerror_callback()
            {
                LogTestRunning();

                var onErrorWasExecuted = true;
                const string exceptionMessge = "Exception from added_job_with_exception_handling_method_is_handled_by_onerror_callback";

                GenericThreadPool.Init(1, 4, 500);
                for (var i = 0; i < 10; i++)
                {
                    GenericThreadPool.AddJob(new ManagedJob(
                        (Action<int>) (index =>
                        {
                            Utils.Log(
                                "Test: added_job_with_exception_handling_method_is_handled_by_onerror_callback - WORK Method - index = " +
                                index);
                            if (index == 6)
                            {
                                Utils.Log(
                                    "Test: added_job_with_exception_handling_method_is_handled_by_onerror_callback - Throw Exception - index = " +
                                    index);
                                throw new Exception(exceptionMessge);
                            }
                        }),
                        new object[] {i},
                        ex =>
                        {
                            onErrorWasExecuted = onErrorWasExecuted && exceptionMessge.Equals(ex.InnerException.Message);
                            Utils.Log("Exception Handler; " + ex.Message);
                        }));
                }
                GenericThreadPool.Shutdown();

                Assert.IsTrue(onErrorWasExecuted);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void added_jobs_callbacks_exception_is_handled_by_onerror_callback()
            {
                LogTestRunning();

                var onErrorWasExecuted = true;
                const string exceptionMessge = "Exception from added_jobs_callbacks_exception_is_handled_by_onerror_callback";

                GenericThreadPool.Init(1, 3, 500);
                for (var i = 0; i < 10; i++)
                {
                    GenericThreadPool.AddJob(new ManagedJob(
                        (Func<int, int>) (index =>
                        {
                            Utils.Log(
                                "Test: added_jobs_callbacks_exception_is_handled_by_onerror_callback - WORK Method - index = " +
                                index);
                            return index;
                        }),
                        new object[] {i},
                        (Action<int>) (index =>
                        {
                            Utils.Log(
                                "Test: added_jobs_callbacks_exception_is_handled_by_onerror_callback - CALLBACK Method - index = " +
                                index);
                            if (index == 7)
                            {
                                Utils.Log(
                                    "Test: added_jobs_callbacks_exception_is_handled_by_onerror_callback - CALLBACK Method - Throw Exception - index = " +
                                    index);
                                throw new Exception(exceptionMessge);
                            }
                        }),
                        null,
                        ex =>
                        {
                            onErrorWasExecuted = onErrorWasExecuted && exceptionMessge.Equals(ex.InnerException.Message);
                            Utils.Log("Exception Handler; " + ex.Message);
                        }));
                }
                GenericThreadPool.Shutdown();

                Assert.IsTrue(onErrorWasExecuted);
            }

            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void added_jobs_with_group_by_id_wait_all_to_finish()
            {
                LogTestRunning();

                TestHelper.ExecuteTestInMta(() =>
                {
                    var job1Finished = false;
                    var job2Finished = false;
                    var job4Finished = false;

                    Action job1 = () =>
                    {
                        Thread.Sleep(2000);
                        job1Finished = true;
                    };

                    Action job2 = () =>
                    {
                        Thread.Sleep(1000);
                        job2Finished = true;
                    };

                    Action job3 = () =>
                    {
                        Thread.Sleep(1);
                    };

                    Action job4 = () =>
                    {
                        Thread.Sleep(100);
                        job4Finished = true;
                    };

                    Action job5 = () =>
                    {
                        Thread.Sleep(5000);
                    };
                    try
                    {
                        GenericThreadPool.Init(1, 5, 300);
                        GenericThreadPool.AddJob(new ManagedJob(job1), 50);
                        GenericThreadPool.AddJob(new ManagedJob(job2), 50);
                        GenericThreadPool.AddJob(new ManagedJob(job3));
                        GenericThreadPool.AddJob(new ManagedJob(job4), 50);
                        GenericThreadPool.AddJob(new ManagedJob(job5));

                        GenericThreadPool.WaitAllJobs(50);
                        Assert.IsTrue(job1Finished && job2Finished && job4Finished);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail(ex.Message);
                    }
                    finally
                    {
                        GenericThreadPool.Shutdown();
                    }
                });
            }
        }

        [TestClass]
        public class CancelJob
        {
            [TestMethod]
            [TestCategory("GenericThreadPool")]
            public void can_cancel_jobs_that_have_not_been_picked()
            {
                LogTestRunning();

                try
                {
                    var executedJobs = new int[4];
                    var index = -1;
                    var managedJobs = new List<ManagedJob>();

                    Action<int> job = jobNumber =>
                    {
                        index++;
                        executedJobs[index] = jobNumber;
                        Thread.Sleep(1000);
                        Utils.Log("Test: can_cancel_jobs_that_have_not_been_picked - jobNumber = " + jobNumber);
                    };

                    GenericThreadPool.Init(1, 2, 300);

                    for (var i = 0; i <= 5; i++)
                    {
                        var managedJob = new ManagedJob(job, new object[] {i});
                        managedJobs.Add(managedJob);
                        GenericThreadPool.AddJob(managedJob);
                    }

                    GenericThreadPool.CancelJob(managedJobs[3]);
                    GenericThreadPool.CancelJob(managedJobs[5]);

                    GenericThreadPool.Shutdown();

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
                LogTestRunning();

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

                    GenericThreadPool.Init(1, 2, 300);

                    for (var i = 0; i <= 5; i++)
                    {
                        var managedJob = new ManagedJob(job, new object[] {i});
                        GenericThreadPool.AddJob(managedJob);
                    }

                    Thread.Sleep(1000);
                    GenericThreadPool.CancellAllJobs();
                    GenericThreadPool.Shutdown();

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
                LogTestRunning();

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

                    GenericThreadPool.Init(5, 100, 5000);

                    for (var t = 0; t < cars.Length; t++)
                    {
                        GenericThreadPool.AddJob(new ManagedJob(job, new object[] { t }));
                    }

                    Thread.Sleep(20000);

                    for (var t = 0; t < cars.Length; t++)
                    {
                        Thread.Sleep(10);
                        GenericThreadPool.AddJob(new ManagedJob(job, new object[] { t }));
                    }

                    GenericThreadPool.Shutdown();
                }
                catch (Exception ex)
                {
                    Assert.Inconclusive("Failed to assert. Error: " + ex.Message);
                }
                finally
                {
                    Assert.IsTrue(!isJob1True && !isJob2True);
                }
            }
        }
    }
}
