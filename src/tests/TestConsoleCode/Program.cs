// ***********************************************************************
//  Assembly         : RzR.Services.TestConsoleCode
//  Author           : RzR
//  Created On       : 2022-08-26 18:57
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
using MethodScheduler;
using MethodScheduler.Abstractions;
using MethodScheduler.Models;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace TestConsoleCode
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                //.AddSingleton<IMultipleScheduler, MultipleScheduler>()
                .RegisterMultipleScheduler()
                .BuildServiceProvider();

            var service = serviceProvider.GetService<IMultipleScheduler>();

            if (args.Any())
            {
                if (args[0] == "1") Run(service);
                if (args[0] == "1.1") Run2(service);
                if (args[0] == "1.2") Run3(service);
                if (args[0] == "2") RunMultiple(service);
                if (args[0] == "3") RunMultipleTask(service);
                if (args[0] == "4") RunMultipleInstance(service);
            }
            else
            {
                RunProcess();
            }


            Console.ReadKey();
        }

        private static void Run(IMultipleScheduler service)
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            var obj1 = new WriteTestLog();

            service.Start(() => { _ = WriteTestLog.InitAsync("run infinite"); }, settings);
            service.Start(() => { _ = WriteTestLog.Init2Async("run infinite"); }, new SchedulerSettings
            {
                DisableOnFailure = false,
                SuccessInterval = 0.5,
                FailInterval = 0.3
            });
        }

        private static void Run2(IMultipleScheduler service)
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 0.3,
                FailInterval = 0.3
            };


            service.Start(() => { _ = WriteTestLog.Init2Async("run only x5"); }, settings, 5);
        }
        private static void Run3(IMultipleScheduler service)
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 0.3,
                FailInterval = 0.3
            };
            
            service.Start(() => { _ = WriteTestLog.Init2Async("run infinite"); }, settings);
        }

        private static void RunMultiple(IMultipleScheduler service)
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            var obj1 = new WriteTestLog();
            service.Start(new List<Func<Task<bool>>>
            {
                () => WriteTestLog.InitAsync("run infinite"),
                () => WriteTestLog.Init2Async("run infinite")
            }, settings);
        }

        private static void RunMultipleTask(IMultipleScheduler service)
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            var obj1 = new WriteTestLog();
            service.Start(new List<Func<Task>>
            {
                () => WriteTestLog.InitTask1Async("run infinite"),
                () => WriteTestLog.InitTask2Async("run infinite"),
                () => WriteTestLog.InitTask3Async("run infinite"),
            }, settings);
        }

        private static void RunMultipleInstance(IMultipleScheduler service)
        {
            var settings = new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            };

            service.Start(new List<Func<Task>>
            {
                WriteTestLogInstance.InitAsync
            }, settings);
        }

        private static void RunProcess()
        {
            var initPath = AppContext.BaseDirectory;
            using (var process1 = new Process())
            {
                process1.StartInfo.FileName = $@"{initPath}TestConsoleCode.exe";
                process1.StartInfo.Arguments = "1";
                process1.Start();
            }

            using (var process11 = new Process())
            {
                process11.StartInfo.FileName = $@"{initPath}TestConsoleCode.exe";
                process11.StartInfo.Arguments = "1.1";
                process11.Start();
            }

            using (var process12 = new Process())
            {
                process12.StartInfo.FileName = $@"{initPath}TestConsoleCode.exe";
                process12.StartInfo.Arguments = "1.2";
                process12.Start();
            }

            using (var process2 = new Process())
            {
                process2.StartInfo.FileName = $@"{initPath}TestConsoleCode.exe";
                process2.StartInfo.Arguments = "2";
                process2.Start();
            }

            using (var process3 = new Process())
            {
                process3.StartInfo.FileName = $@"{initPath}TestConsoleCode.exe";
                process3.StartInfo.Arguments = "3";
                process3.Start();
            }

            using var process4 = new Process()
            {
                StartInfo = { FileName = $@"{initPath}TestConsoleCode.exe", Arguments = "4" }
            };
            process4.Start();
        }
    }
}