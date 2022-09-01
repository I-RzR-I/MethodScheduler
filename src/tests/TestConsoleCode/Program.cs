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

            service.Start(obj1.InitAsync, settings);
            service.Start(obj1.Init2Async, new SchedulerSettings
            {
                DisableOnFailure = false,
                SuccessInterval = 0.5,
                FailInterval = 0.3
            });
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
                obj1.InitAsync,
                obj1.Init2Async
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
                obj1.InitTask1Async,
                obj1.InitTask2Async,
                obj1.InitTask3Async
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
                WriteTestLogInstance.Instance.InitAsync
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

            using (var process4 = new Process())
            {
                process4.StartInfo.FileName = $@"{initPath}TestConsoleCode.exe";
                process4.StartInfo.Arguments = "4";
                process4.Start();
            }
        }
    }
}