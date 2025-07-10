// ***********************************************************************
//  Assembly         : RzR.Services.TestConsoleCode
//  Author           : RzR
//  Created On       : 2022-08-26 19:01
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

namespace TestConsoleCode
{
    public class WriteTestLog
    {
        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static async Task<bool> InitAsync(string message = null)
        {
            Console.WriteLine($"async WriteTestLog.InitAsync-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}\t{message}");
            Thread.Sleep(TimeSpan.FromMinutes(2));

            // throw new Exception("10-11-12");
            return await Task.FromResult(true);
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static async Task<bool> Init2Async(string message = null)
        {
            Console.WriteLine($"async WriteTestLog.Init2Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}\t{message}");

            return await Task.FromResult(true);
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static async Task InitTask1Async(string message = null)
        {
            Console.WriteLine(
                $"async WriteTestLog.InitTask1Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}\t{message}");
            Thread.Sleep(TimeSpan.FromMinutes(0.5));
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static async Task InitTask2Async(string message = null)
        {
            Console.WriteLine(
                $"async WriteTestLog.InitTask2Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}\t{message}");
            Thread.Sleep(TimeSpan.FromMinutes(1));
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static async Task InitTask3Async(string message = null)
        {
            Console.WriteLine(
                $"async WriteTestLog.InitTask3Async-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}\t{message}");
            Thread.Sleep(TimeSpan.FromMinutes(2));
            await Task.CompletedTask;
        }
    }
}