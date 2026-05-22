// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:33
//  ***********************************************************************
//  <copyright file="ScheduledJobOptions.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      mailto: ddpRzR@hotmail.com
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using RzR.Scheduling.RecurringJobs.Enums;

#endregion

namespace RzR.Scheduling.RecurringJobs.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Configuration options for a single scheduled job.
    /// </summary>
    /// =================================================================================================
    public sealed class ScheduledJobOptions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Optional stable identifier for the job. Used with <see cref="Abstractions.IMethodScheduler.TryStopAsync" />
        ///     to stop an individual job by name. A random GUID is assigned automatically when left <see langword="null" />.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        /// =================================================================================================
        public string Id { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Interval between iterations after a <b>successful</b> execution. Default: 1 minute.
        /// </summary>
        /// <value>
        ///     The success interval.
        /// </value>
        /// =================================================================================================
        public TimeSpan SuccessInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Interval before retrying after a <b>failed</b> execution. Default: 30 seconds.
        /// </summary>
        /// <value>
        ///     The fail interval.
        /// </value>
        /// =================================================================================================
        public TimeSpan FailInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Delay before the <b>first</b> iteration fires.
        ///     <see langword="null" /> means the first iteration starts immediately.
        ///     Default: <see langword="null" />.
        /// </summary>
        /// <value>
        ///     The initial delay.
        /// </value>
        /// =================================================================================================
        public TimeSpan? InitialDelay { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Stop the job after this many total iterations (successes + failures combined).
        ///     <see langword="null" /> means run indefinitely.
        ///     Default: <see langword="null" />.
        /// </summary>
        /// <value>
        ///     The maximum iterations.
        /// </value>
        /// =================================================================================================
        public int? MaxIterations { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Stop the job after its first <b>successful</b> iteration. Default: <see langword="false" />
        /// </summary>
        /// <value>
        ///     True if stop on first success, false if not.
        /// </value>
        /// =================================================================================================
        public bool StopOnFirstSuccess { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Stop the job (transitioning to <see cref="JobState.Faulted" />) after any failed
        ///     iteration. Default: <see langword="false" />.
        /// </summary>
        /// <value>
        ///     True if stop on failure, false if not.
        /// </value>
        /// =================================================================================================
        public bool StopOnFailure { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Re-throw the exception from a failed iteration, faulting the background task. The faulted
        ///     task surfaces when <see cref="Abstractions.IScheduledJob.StopAsync" /> is awaited.
        ///     Default: <see langword="false" />.
        /// </summary>
        /// <value>
        ///     True if throw on failure, false if not.
        /// </value>
        /// =================================================================================================
        public bool ThrowOnFailure { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Controls how multiple work items are executed within a single iteration. Applies only
        ///     when more than one work delegate is passed to <see cref="Abstractions.IMethodScheduler" />.
        ///     Default: <see cref="ParallelExecutionMode.WhenAll" />.
        /// </summary>
        /// <value>
        ///     The parallel mode.
        /// </value>
        /// =================================================================================================
        public ParallelExecutionMode ParallelMode { get; set; } = ParallelExecutionMode.WhenAll;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Maximum number of work items that may execute concurrently within a single iteration.
        ///     <para>
        ///         Applies only to <see cref="ParallelExecutionMode.WhenAll" /> and
        ///         <see cref="ParallelExecutionMode.WhenAllIgnoreErrors" />.
        ///         <see cref="ParallelExecutionMode.Sequential" /> is inherently DOP=1 and
        ///         ignores this setting.
        ///     </para>
        ///     <para>
        ///         <see langword="null" /> or a value &gt;= the number of work items means all items
        ///         start simultaneously (the default, unbounded behaviour).
        ///     </para>
        ///     <para>
        ///         Must be &gt;= 1 when set.
        ///     </para>
        ///     Default: <see langword="null" /> (unbounded).
        /// </summary>
        /// <value>
        ///     The maximum degree of parallelism.
        /// </value>
        /// =================================================================================================
        public int? MaxDegreeOfParallelism { get; set; }
    }
}