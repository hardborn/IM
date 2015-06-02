using Nova.NovaWeb.McGo.BLL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Nova.NovaWeb.McGo.Utilities
{

        public static class CodeTimer
        {

            private static ILog _loger;
            public static void Initialize()
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                _loger = AppEnvionment.Default.Get<ILog>();
            }

            /// <summary>
            /// Time方法接受三个参数，名称，循环次数以及需要执行的方法体。
            /// 打印出花费时间，消耗的CPU时钟周期，以及各代垃圾收集的回收次数。
            /// </summary>
            /// <param name="name"></param>
            /// <param name="iteration"></param>
            /// <param name="action"></param>
            public static void Time(string name, int iteration, Action action)
            {
                if (string.IsNullOrEmpty(name))
                    return;

                ConsoleColor currentForeColor = Console.ForegroundColor;
               // Console.ForegroundColor = ConsoleColor.Yellow;
                //Console.WriteLine(name);

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                int[] gcCounts = new int[GC.MaxGeneration + 1];
                for (int i = 0; i < GC.MaxGeneration; i++)
                {
                    gcCounts[i] = GC.CollectionCount(i);
                }

                Stopwatch watch = new Stopwatch();
                watch.Start();
                ulong cycleCount = GetCycleCount();
                for (int i = 0; i < iteration; i++)
                {
                    action();
                }
                ulong cpuCycles = GetCycleCount() - cycleCount;
                watch.Stop();

                //Console.ForegroundColor = currentForeColor;
                Debug.WriteLine("*-----------------------------------*");
                Debug.WriteLine(name);
                Debug.WriteLine("\tTime Elapsed:\t" + watch.ElapsedMilliseconds.ToString("N0") + "ms");
                Debug.WriteLine("\tCPU Cycles:\t" + cpuCycles.ToString("N0"));

                for (int i = 0; i < GC.MaxGeneration; i++)
                {
                    int count = GC.CollectionCount(i) - gcCounts[i];
                    Debug.WriteLine("\tGen " + i + ": \t\t" + count);
                }
                Debug.WriteLine("*-----------------------------------*");
            }

            private static ulong GetCycleCount()
            {
                ulong cycleCount = 0;
                QueryThreadCycleTime(GetCurrentThread(), ref cycleCount);
                return cycleCount;
            }

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool QueryThreadCycleTime(IntPtr threadHandle, ref ulong cycleTime);

            [DllImport("kernel32.dll")]
            static extern IntPtr GetCurrentThread();
        }

}
