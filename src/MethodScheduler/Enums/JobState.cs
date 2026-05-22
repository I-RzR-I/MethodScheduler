// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 12-05-2026 19:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:35
//  ***********************************************************************
//  <copyright file="JobState.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      mailto: ddpRzR@hotmail.com
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Represents the lifecycle state of a scheduled job.
    /// </summary>
    /// =================================================================================================
    public enum JobState
    {
        /// <summary>
        ///     Job has been created and is waiting for its initial delay before the first iteration.
        /// </summary>
        Pending,

        /// <summary>
        ///     Job is actively executing its schedule loop.
        /// </summary>
        Running,

        /// <summary>
        ///     Job has completed normally (cancelled, max-iterations reached, or stopped explicitly).
        /// </summary>
        Stopped,

        /// <summary>
        ///     Job stopped due to an unhandled exception when
        ///     <see cref="ScheduledJobOptions.StopOnFailure" /> or
        ///     <see cref="ScheduledJobOptions.ThrowOnFailure" /> is set.
        /// </summary>
        Faulted
    }
}