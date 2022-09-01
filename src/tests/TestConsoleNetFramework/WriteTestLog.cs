// ***********************************************************************
//  Assembly         : RzR.Services.TestConsoleNetFramework
//  Author           : RzR
//  Created On       : 2022-08-24 02:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="WriteTestLog.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Threading;
using System.Threading.Tasks;
using DomainCommonExtensions.DataTypeExtensions;

#endregion

namespace TestConsoleNetFramework
{
    public class WriteTestLog
    {
        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task<bool> InitAsync()
        {
            Console.WriteLine($"async WriteTestLog.InitAsync-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");
            Thread.Sleep(TimeSpan.FromMinutes(2));

            // throw new Exception("10-11-12");
            return await Task.FromResult(true);
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task<bool> Init2Async()
        {
            Console.WriteLine($"async WriteTestLog.Init2Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");

            return await Task.FromResult(true);
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task<int> InitTResultAsync()
        {
            Console.WriteLine($"async WriteTestLog.InitTResultAsync-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");
            
            return await Task.FromResult(1);
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task<string> InitTResult2Async()
        {
            Console.WriteLine($"async WriteTestLog.InitTResult2Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");
            Thread.Sleep(TimeSpan.FromMinutes(0.5));

            return await Task.FromResult("1");
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task InitTask1Async()
        {
            Console.WriteLine(
                $"async WriteTestLog.InitTask1Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");
            Thread.Sleep(TimeSpan.FromMinutes(0.5));
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task InitTask2Async()
        {
            Console.WriteLine(
                $"async WriteTestLog.InitTask2Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");
            Thread.Sleep(TimeSpan.FromMinutes(1));
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public async Task InitTask3Async()
        {
            Console.WriteLine(
                $"async WriteTestLog.InitTask3Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");
            Thread.Sleep(TimeSpan.FromMinutes(2));
            await Task.CompletedTask;
        }
    }
}