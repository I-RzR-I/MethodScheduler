// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-23 09:27
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="IMultipleScheduler.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Multiple method scheduler.
    /// </summary>
    /// <remarks>
    ///     This interface is obsolete. Use <see cref="IMethodScheduler"/> instead.
    /// </remarks>
    /// =================================================================================================
    [Obsolete("IMultipleScheduler is obsolete. Use IMethodScheduler and ScheduledJobOptions instead.", error: false)]
    public interface IMultipleScheduler
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Start new schedule.
        /// </summary>
        /// <param name="scheduleMethod">Required. Method to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     (Optional)
        ///     Stop the schedule execution when the specified number is reached. Default value is 'null'
        ///     (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     (Optional)
        ///     Force stop scheduler after the first successful execution. Default value is 'false'.
        /// </param>
        /// =================================================================================================
        void Start(
            Action scheduleMethod, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Start new schedule.
        /// </summary>
        /// <param name="scheduleMethods">Required. Methods to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     (Optional)
        ///     Stop the schedule execution when the specified number is reached. Default value is 'null'
        ///     (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     (Optional)
        ///     Force stop scheduler after the first successful execution. Default value is 'false'.
        /// </param>
        /// =================================================================================================
        void Start(
            IEnumerable<Action> scheduleMethods,
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Start new schedule.
        /// </summary>
        /// <param name="scheduleMethod">Required. Method to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     (Optional)
        ///     Stop the schedule execution when the specified number is reached. Default value is 'null'
        ///     (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     (Optional)
        ///     Force stop scheduler after the first successful execution. Default value is 'false'.
        /// </param>
        /// =================================================================================================
        void Start(
            Func<Task> scheduleMethod, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Start new schedule.
        /// </summary>
        /// <param name="scheduleMethods">Required. Methods to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     (Optional)
        ///     Stop the schedule execution when the specified number is reached. Default value is 'null'
        ///     (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     (Optional)
        ///     Force stop scheduler after the first successful execution. Default value is 'false'.
        /// </param>
        /// =================================================================================================
        void Start(
            IEnumerable<Func<Task>> scheduleMethods, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Stop scheduler.
        /// </summary>
        /// =================================================================================================
        void Stop();
    }
}