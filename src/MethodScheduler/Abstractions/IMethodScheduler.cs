// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:28
//  ***********************************************************************
//  <copyright file="IMethodScheduler.cs" company="RzR SOFT & TECH">
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
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Schedules and manages recurring method execution.
    /// </summary>
    /// <remarks>
    ///     Register via <c>services.AddMethodScheduler()</c>. The scheduler is registered as a
    ///     singleton and is safe to inject into any service lifetime. It implements <see cref="IAsyncDisposable" />
    ///     ; the DI container disposes it on shutdown, which stops all running jobs cleanly.
    /// </remarks>
    /// =================================================================================================
    public interface IMethodScheduler : IAsyncDisposable
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Schedules a single work item to run repeatedly according to <paramref name="options" />.
        /// </summary>
        /// <param name="options">Required. Job configuration.</param>
        /// <param name="work">
        ///     Required. The delegate to invoke on each iteration. The <see cref="CancellationToken" />
        ///     is cancelled when the job is stopped;
        ///     pass it into any async operations inside the delegate for cooperative cancellation.
        /// </param>
        /// <returns>
        ///     A handle to the running job.
        /// </returns>
        /// =================================================================================================
        IScheduledJob Schedule(ScheduledJobOptions options, Func<CancellationToken, Task> work);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Schedules multiple work items to run together in each iteration. Execution order and
        ///     parallelism are controlled by
        ///     <see cref="ScheduledJobOptions.ParallelMode" />.
        /// </summary>
        /// <param name="options">Required. Job configuration.</param>
        /// <param name="work">Required. One or more delegates to invoke each iteration.</param>
        /// <returns>
        ///     A handle to the running job.
        /// </returns>
        /// =================================================================================================
        IScheduledJob Schedule(ScheduledJobOptions options, params Func<CancellationToken, Task>[] work);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Attempts to stop the job identified by <paramref name="jobId" /> and removes it from the
        ///     scheduler's tracking list.
        /// </summary>
        /// <param name="jobId">The <see cref="ScheduledJobOptions.Id" /> of the job to stop.</param>
        /// <param name="cancellationToken">
        ///     (Optional) Token to cancel waiting for the current iteration to finish.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the job was found and stopped;
        ///     <see langword="false" /> if no job with that id is registered.
        /// </returns>
        /// =================================================================================================
        Task<bool> TryStopAsync(string jobId, CancellationToken cancellationToken = default);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Stops all running jobs and waits for every current iteration to complete. The scheduler
        ///     remains usable after this call returns - subsequent
        ///     <see cref="Schedule(ScheduledJobOptions, Func{CancellationToken, Task})" />
        ///     calls will start fresh jobs against a new internal cancellation source. To shut the
        ///     scheduler down permanently, dispose it instead.
        /// </summary>
        /// <param name="cancellationToken">
        ///     (Optional) Token to cancel waiting for iterations to complete.
        /// </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        Task StopAllAsync(CancellationToken cancellationToken = default);
    }
}