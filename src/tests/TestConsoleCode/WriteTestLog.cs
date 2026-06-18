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
using System.Threading.Tasks;
using RzR.Extensions.Domain.Primitives;

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
            await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);

            // throw new Exception("10-11-12");
            return true;
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
            await Task.Delay(TimeSpan.FromMinutes(0.5)).ConfigureAwait(false);
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
            await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
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
            await Task.Delay(TimeSpan.FromMinutes(2)).ConfigureAwait(false);
        }
    }
}