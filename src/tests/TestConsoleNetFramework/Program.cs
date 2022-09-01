// ***********************************************************************
//  Assembly         : RzR.Services.TestConsoleNetFramework
//  Author           : RzR
//  Created On       : 2022-08-24 02:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="Program.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MethodScheduler.Helpers;
using MethodScheduler.Models;

#endregion

namespace TestConsoleNetFramework
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Any())
            {
                if (args[0] == "1") Run();
                if (args[0] == "2") RunMultiple();
                if (args[0] == "3") RunMultipleTask();
                if (args[0] == "4") RunMultipleInstance();
                if (args[0] == "5") RunMultipleInstanceStop();
                if (args[0] == "6") RunMultipleResult();
            }
            else
            {
                RunProcess();
            }

            Console.ReadKey();
        }

        private static void Run()
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            var obj1 = new WriteTestLog();

            MultipleScheduler.Instance.Start(obj1.InitAsync, settings);
            MultipleScheduler.Instance.Start(obj1.Init2Async, new SchedulerSettings
            {
                DisableOnFailure = false,
                SuccessInterval = 0.5,
                FailInterval = 0.3
            });
        }

        private static void RunMultiple()
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            var obj1 = new WriteTestLog();

            MultipleScheduler.Instance.Start(new List<Func<Task<bool>>>
            {
                obj1.InitAsync,
                obj1.Init2Async
            }, settings);
        }

        private static void RunMultipleResult()
        {
            MultipleScheduler.Instance.Start(new List<Func<Task>>
            {
                new WriteTestLog().InitTResultAsync,
                new WriteTestLog().InitTResult2Async
            }, new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            });
        }

        private static void RunMultipleTask()
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            var obj1 = new WriteTestLog();
            MultipleScheduler.Instance.Start(new List<Func<Task>>
            {
                obj1.InitTask1Async,
                obj1.InitTask2Async,
                obj1.InitTask3Async
            }, settings);
        }

        private static void RunMultipleInstance()
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            MultipleScheduler.Instance.Start(new List<Func<Task>>
            {
                WriteTestLogInstance.Instance.InitAsync
            }, settings);
        }

        private static void RunMultipleInstanceStop()
        {
            var mInst = MultipleScheduler.Instance;
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 0.5,
                FailInterval = 0.5
            };

            Console.WriteLine("Start");
            mInst.Start(new List<Func<Task>>
            {
                WriteTestLogInstance.Instance.InitAsync
            }, settings);

            Console.WriteLine("Stop");
            mInst.Stop();
        }

        private static void RunProcess()
        {
            var initPath = AppContext.BaseDirectory;
            using (var process1 = new Process())
            {
                process1.StartInfo.FileName = $@"{initPath}TestConsoleNetFramework.exe";
                process1.StartInfo.Arguments = "1";
                process1.Start();
            }

            using (var process2 = new Process())
            {
                process2.StartInfo.FileName = $@"{initPath}TestConsoleNetFramework.exe";
                process2.StartInfo.Arguments = "2";
                process2.Start();
            }

            using (var process3 = new Process())
            {
                process3.StartInfo.FileName = $@"{initPath}TestConsoleNetFramework.exe";
                process3.StartInfo.Arguments = "3";
                process3.Start();
            }

            using (var process4 = new Process())
            {
                process4.StartInfo.FileName = $@"{initPath}TestConsoleNetFramework.exe";
                process4.StartInfo.Arguments = "4";
                process4.Start();
            }

            using (var process5 = new Process())
            {
                process5.StartInfo.FileName = $@"{initPath}TestConsoleNetFramework.exe";
                process5.StartInfo.Arguments = "5";
                process5.Start();
            }

            using (var process6 = new Process())
            {
                process6.StartInfo.FileName = $@"{initPath}TestConsoleNetFramework.exe";
                process6.StartInfo.Arguments = "6";
                process6.Start();
            }
        }
    }
}