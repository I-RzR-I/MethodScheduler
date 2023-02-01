// ***********************************************************************
//  Assembly         : RzR.Services.TestConsoleCode
//  Author           : RzR
//  Created On       : 2022-08-26 19:01
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="WriteTestLogInstance.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Threading.Tasks;
using DomainCommonExtensions.DataTypeExtensions;
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace TestConsoleCode
{
    public class WriteTestLogInstance
    {
#pragma warning disable IDE0090 // Use 'new(...)'
        public static readonly WriteTestLogInstance Instance = new WriteTestLogInstance();
#pragma warning restore IDE0090 // Use 'new(...)'

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public static async Task<bool> InitAsync()
        {
            Console.WriteLine(
                $"async WriteTestLogInstance.InitAsync-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");

            return await Task.FromResult(true);
        }

        /// <summary>
        ///     Init
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        // ReSharper disable once UnusedMember.Global
        public static bool Init()
        {
            Console.WriteLine($"WriteTestLogInstance.Init-{DateTime.Now.FormatToString("yyyy-MM-dd-HH-mm-ss")}");

            return true;
        }
    }
}