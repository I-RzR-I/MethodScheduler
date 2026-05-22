// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:29
//  ***********************************************************************
//  <copyright file="IScheduledJob.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      mailto: ddpRzR@hotmail.com
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Threading;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A handle to a running scheduled job returned by
    ///     <see cref="IMethodScheduler.Schedule(RzR.Scheduling.RecurringJobs.Models.ScheduledJobOptions,System.Func{System.Threading.CancellationToken,System.Threading.Tasks.Task})"/>. 
    /// </summary>
    /// =================================================================================================
    public interface IScheduledJob
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Unique identifier of the job. Either the value from
        ///     <see cref="ScheduledJobOptions.Id" /> or an auto-generated GUID string.
        /// </summary>
        /// <value>
        ///     The identifier.
        /// </value>
        /// =================================================================================================
        string Id { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Current lifecycle state of the job.
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        /// =================================================================================================
        JobState State { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Total number of completed iterations (successes and failures combined).
        /// </summary>
        /// <value>
        ///     The number of iterations.
        /// </value>
        /// =================================================================================================
        int IterationCount { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     UTC timestamp when the most recent iteration started, or
        ///     <see langword="null" /> if the job has not yet started its first iteration.
        /// </summary>
        /// <value>
        ///     The last run at.
        /// </value>
        /// =================================================================================================
        DateTimeOffset? LastRunAt { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     UTC timestamp when the next iteration is expected to start, or
        ///     <see langword="null" /> while an iteration is in progress or the job is stopped.
        /// </summary>
        /// <value>
        ///     The next run at.
        /// </value>
        /// =================================================================================================
        DateTimeOffset? NextRunAt { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     UTC timestamp when the most recent <b>successful</b> iteration completed, or
        ///     <see langword="null" /> if no iteration has succeeded yet.
        /// </summary>
        /// <value>
        ///     The last success at.
        /// </value>
        /// =================================================================================================
        DateTimeOffset? LastSuccessAt { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The exception thrown during the last failed iteration, or
        ///     <see langword="null" /> if the last iteration succeeded (the value is cleared on
        ///     every successful iteration).
        /// </summary>
        /// <value>
        ///     The last error.
        /// </value>
        /// =================================================================================================
        Exception LastError { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     A task that completes when the job's run loop exits (naturally, by cancellation, or by
        ///     fault). Awaiting this task observes any exception thrown when
        ///     <see cref="ScheduledJobOptions.ThrowOnFailure" /> is set.
        /// </summary>
        /// <value>
        ///     A Task.
        /// </value>
        /// =================================================================================================
        Task Completion { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Signals this job to stop and waits for the current iteration to finish.
        /// </summary>
        /// <param name="cancellationToken">(Optional) Token to cancel waiting for the iteration to complete.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}