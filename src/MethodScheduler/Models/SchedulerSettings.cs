// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-25 01:34
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-02-01 23:51
// ***********************************************************************
//  <copyright file="SchedulerSettings.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

// ReSharper disable ClassNeverInstantiated.Global

namespace MethodScheduler.Models
{
    /// <summary>
    ///     Scheduler settings
    /// </summary>
    public class SchedulerSettings
    {
        /// <summary>
        ///     Success execute interval in minutes
        /// </summary>
        public double SuccessInterval { get; set; } = 1;

        /// <summary>
        ///     Fail/Exception execute interval, retry in minutes
        /// </summary>
        public double FailInterval { get; set; } = 0.5;

        /// <summary>
        ///     Disable on failure
        /// </summary>
        public bool DisableOnFailure { get; set; } = false;

        /// <summary>
        ///     Gets or sets a value indicating whether throw exception in case when an error occured.
        /// </summary>
        /// <value>
        ///     <see langword="true" /> if the error will be thrown; otherwise, <see langword="false" />.
        /// </value>
        /// <remarks></remarks>
        public bool ThrowException { get; set; } = false;
    }
}