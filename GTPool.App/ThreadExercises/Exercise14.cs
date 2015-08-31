using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTP = GTPool.GenericThreadPool;

namespace GTPool.App.ThreadExercises
{
    public class Exercise14
    {
public static void Run()
{
    const int groupById = 33;
    const string task1 = "Task 01";
    const string task2 = "Task 02";

    GTP.AddJob(new ManagedJob(
        work: (Func<string, int, int, int>) ((taskName, x, y) =>
        {
            for (var i = 0; i < y; i++)
            {
                x += y + i;
                Log(string.Format("{0}, calculating: x[{2}] = {1} ", 
                    taskName, x, i));

                Thread.Sleep(80);
            }
            return x;
        }),
        parameters: new object[] { task1, 1000, 10 },
        callback: (Action<string, int>) DisplayResult,
        callbackParameters: new object[] { task1 }), groupById);

    GTP.AddJob(new ManagedJob(
        work: (Func<string, int, int, int>)((taskName, x, y) =>
        {
            for (var i = 0; i < y; i++)
            {
                x = x * y / (i + 1);
                Log(string.Format("{0}, calculating: x[{2}] = {1} ",
                    taskName, x, i));

                Thread.Sleep(50);
            }
            return x;
        }),
        parameters: new object[] { task2, 1000, 10 },
        callback: (Action<string, int>) DisplayResult,
        callbackParameters: new object[] { task2 }), groupById);

    Console.WriteLine("---- Waiting tasks to finish ----");
    GTP.WaitAllJobs(groupById);
    Console.WriteLine("---- Tasks finished ----");

    Thread.Sleep(3000);
}

static void DisplayResult(string taskName, int result)
{
    Console.WriteLine("{0} | {1} | {2} - DISPLAY | Final result: {3}",
        Utils.HiResDateTime.UtcNow, Thread.CurrentThread.Name, taskName, result);
}

static void Log(string message)
{
    GTP.AddJob(new ManagedJob(
        work: (Action)(() =>
        {
            Thread.Sleep(200);
            Console.WriteLine("{0} | {1} | {2}", 
                Utils.HiResDateTime.UtcNow, Thread.CurrentThread.Name, message);
        }), 
        parameters: null, 
        onError: null, 
        threadPriority: ThreadPriority.Lowest, 
        isBackground: true));
}
    }
}
