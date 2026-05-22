// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:28
//  ***********************************************************************
//  <copyright file="MethodSchedulerExtensions.cs" company="RzR SOFT & TECH">
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
using RzR.Extensions.Domain.Primitives;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Extensions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Convenience extension methods for <see cref="IMethodScheduler" /> that wrap synchronous <see cref="Action" />
    ///     delegates and <see cref="Func{Task}" />
    ///     delegates (without <see cref="CancellationToken" />) into the canonical
    ///     <c>Func&lt;CancellationToken, Task&gt;</c> signature.
    /// </summary>
    /// =================================================================================================
    public static class MethodSchedulerExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Schedules a synchronous <see cref="Action" /> to run repeatedly.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="scheduler">The scheduler to act on.</param>
        /// <param name="options">Options for controlling the operation.</param>
        /// <param name="work">A variable-length parameters list containing work.</param>
        /// <returns>
        ///     An IScheduledJob.
        /// </returns>
        /// =================================================================================================
        public static IScheduledJob Schedule(this IMethodScheduler scheduler,
            ScheduledJobOptions options, Action work)
        {
            if (scheduler.IsNull())
                throw new ArgumentNullException(nameof(scheduler));

            if (work.IsNull())
                throw new ArgumentNullException(nameof(work));

            return scheduler.Schedule(options, _ =>
            {
                work();

                return Task.CompletedTask;
            });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Schedules a <see cref="Func{Task}" /> (without <see cref="CancellationToken" />) to run
        ///     repeatedly.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="scheduler">The scheduler to act on.</param>
        /// <param name="options">Options for controlling the operation.</param>
        /// <param name="work">A variable-length parameters list containing work.</param>
        /// <returns>
        ///     An IScheduledJob.
        /// </returns>
        /// =================================================================================================
        public static IScheduledJob Schedule(this IMethodScheduler scheduler,
            ScheduledJobOptions options, Func<Task> work)
        {
            if (scheduler.IsNull())
                throw new ArgumentNullException(nameof(scheduler));

            if (work.IsNull())
                throw new ArgumentNullException(nameof(work));

            return scheduler.Schedule(options, _ => work());
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Schedules multiple synchronous <see cref="Action" /> delegates to run together each
        ///     iteration. Parallelism is controlled by <see cref="ScheduledJobOptions.ParallelMode" />. 
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="scheduler">The scheduler to act on.</param>
        /// <param name="options">Options for controlling the operation.</param>
        /// <param name="work">A variable-length parameters list containing work.</param>
        /// <returns>
        ///     An IScheduledJob.
        /// </returns>
        /// =================================================================================================
        public static IScheduledJob Schedule(this IMethodScheduler scheduler,
            ScheduledJobOptions options, params Action[] work)
        {
            if (scheduler.IsNull())
                throw new ArgumentNullException(nameof(scheduler));

            if (work.IsNull() || work.Length == 0)
                throw new ArgumentException("At least one work item is required.", nameof(work));

            var wrapped = Array.ConvertAll(work, item =>
                (Func<CancellationToken, Task>)(_ =>
                {
                    item();

                    return Task.CompletedTask;
                }));

            return scheduler.Schedule(options, wrapped);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Schedules multiple <see cref="Func{Task}" /> delegates (without
        ///     <see cref="CancellationToken" />) to run together each iteration.
        ///     Parallelism is controlled by <see cref="ScheduledJobOptions.ParallelMode" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="scheduler">The scheduler to act on.</param>
        /// <param name="options">Options for controlling the operation.</param>
        /// <param name="work">A variable-length parameters list containing work.</param>
        /// <returns>
        ///     An IScheduledJob.
        /// </returns>
        /// =================================================================================================
        public static IScheduledJob Schedule(this IMethodScheduler scheduler,
            ScheduledJobOptions options, params Func<Task>[] work)
        {
            if (scheduler.IsNull())
                throw new ArgumentNullException(nameof(scheduler));

            if (work.IsNull() || work.Length == 0)
                throw new ArgumentException("At least one work item is required.", nameof(work));

            var wrapped = Array.ConvertAll(work, item =>
                (Func<CancellationToken, Task>)(_ => item()));

            return scheduler.Schedule(options, wrapped);
        }
    }
}