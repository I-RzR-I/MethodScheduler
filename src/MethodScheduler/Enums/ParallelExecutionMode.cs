// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 12-05-2026 19:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:35
//  ***********************************************************************
//  <copyright file="ParallelExecutionMode.cs" company="RzR SOFT & TECH">
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
    ///     Controls how multiple work items within a single job iteration are executed.
    /// </summary>
    /// <remarks>
    ///     For <see cref="WhenAll" /> and <see cref="WhenAllIgnoreErrors" />, the degree of
    ///     concurrency can be further limited by setting
    ///     <see cref="ScheduledJobOptions.MaxDegreeOfParallelism" />.
    ///     Leaving it <see langword="null" /> (the default) means all items start simultaneously.
    /// 
    /// </remarks>
    /// =================================================================================================
    public enum ParallelExecutionMode
    {
        /// <summary>
        ///     Run each work item one after another in the order supplied.
        ///     A failure in one item prevents subsequent items from running.
        ///     <see cref="ScheduledJobOptions.MaxDegreeOfParallelism" /> is ignored for this mode.
        /// </summary>
        Sequential,

        /// <summary>
        ///     Run all work items concurrently.
        ///     The iteration fails if any work item throws.
        ///     Concurrency is bounded by <see cref="ScheduledJobOptions.MaxDegreeOfParallelism" />
        ///     when set.
        /// </summary>
        WhenAll,

        /// <summary>
        ///     Run all work items concurrently.
        ///     Individual errors are silently swallowed; the iteration is always considered successful.
        ///     <see cref="System.OperationCanceledException" /> is still propagated.
        ///     Concurrency is bounded by <see cref="ScheduledJobOptions.MaxDegreeOfParallelism" />
        ///     when set.
        /// </summary>
        WhenAllIgnoreErrors
    }
}