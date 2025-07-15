// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-23 03:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 19:12
// ***********************************************************************
//  <copyright file="MultipleScheduler.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainCommonExtensions.CommonExtensions;
using MethodScheduler.Abstractions;
using MethodScheduler.Models;

#endregion

namespace MethodScheduler.Helpers
{
    /// <inheritdoc cref="IMultipleScheduler" />
    public class MultipleScheduler : InternalSchedulerBase, IMultipleScheduler
    {
        /// <summary>
        ///     Class instance
        /// </summary>
        public static MultipleScheduler Instance { get; } = new MultipleScheduler();

        /// <summary>
        ///     Gets or sets execution queue method.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        private static IList<MultipleScheduler> Queue { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleScheduler" /> class.
        /// </summary>
        /// <remarks></remarks>
        public MultipleScheduler()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleScheduler" /> class.
        /// </summary>
        /// <param name="scheduleMethod">Required. Method to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     Stop the schedule execution when the specified number is reached.
        ///     Default value is 'null' (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     Force stop scheduler after the first successful execution.
        ///     Default value is 'false'.
        /// </param>
        /// <remarks></remarks>
        private MultipleScheduler(
            Action scheduleMethod, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            base.Settings = settings;
            base.StopAfterXIteration = stopAfterXIteration;
            base.ForceStopAfterFirstSuccessExecution = forceStopAfterFirstSuccessExecution;

            base.StartScheduler(scheduleMethod);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleScheduler" /> class.
        /// </summary>
        /// <param name="scheduleMethods">Required. Methods to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     Stop the schedule execution when the specified number is reached.
        ///     Default value is 'null' (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     Force stop scheduler after the first successful execution.
        ///     Default value is 'false'.
        /// </param>
        /// <remarks></remarks>
        private MultipleScheduler(
            IEnumerable<Action> scheduleMethods, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            base.Settings = settings;
            base.StopAfterXIteration = stopAfterXIteration;
            base.ForceStopAfterFirstSuccessExecution = forceStopAfterFirstSuccessExecution;

            base.StartScheduler(scheduleMethods);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleScheduler" /> class.
        /// </summary>
        /// <param name="scheduleMethod">Required. Method to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     Stop the schedule execution when the specified number is reached.
        ///     Default value is 'null' (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     Force stop scheduler after the first successful execution.
        ///     Default value is 'false'.
        /// </param>
        /// <remarks></remarks>
        private MultipleScheduler(
            Func<Task> scheduleMethod, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            base.Settings = settings;
            base.StopAfterXIteration = stopAfterXIteration;
            base.ForceStopAfterFirstSuccessExecution = forceStopAfterFirstSuccessExecution;

            base.StartScheduler(scheduleMethod);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultipleScheduler" /> class.
        /// </summary>
        /// <param name="scheduleMethods">Required. Methods to schedule.</param>
        /// <param name="settings">Required. Scheduler settings.</param>
        /// <param name="stopAfterXIteration">
        ///     Stop the schedule execution when the specified number is reached.
        ///     Default value is 'null' (scheduler will not stop).
        /// </param>
        /// <param name="forceStopAfterFirstSuccessExecution">
        ///     Force stop scheduler after the first successful execution.
        ///     Default value is 'false'.
        /// </param>
        /// <remarks></remarks>
        private MultipleScheduler(
            IEnumerable<Func<Task>> scheduleMethods, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            base.Settings = settings;
            base.StopAfterXIteration = stopAfterXIteration;
            base.ForceStopAfterFirstSuccessExecution = forceStopAfterFirstSuccessExecution;

            base.StartScheduler(scheduleMethods);
        }

        /// <inheritdoc />
        public void Start(
            Action scheduleMethod, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            if (Queue.IsNull()) Queue = new List<MultipleScheduler>();
            Queue.Add(new MultipleScheduler(scheduleMethod, settings, stopAfterXIteration, forceStopAfterFirstSuccessExecution));
        }

        /// <inheritdoc />
        public void Start(
            IEnumerable<Action> scheduleMethods, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            if (Queue.IsNull()) Queue = new List<MultipleScheduler>();
            Queue.Add(new MultipleScheduler(scheduleMethods, settings, stopAfterXIteration, forceStopAfterFirstSuccessExecution));
        }

        /// <inheritdoc />
        public void Start(
            Func<Task> scheduleMethod, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            if (Queue.IsNull()) Queue = new List<MultipleScheduler>();
            Queue.Add(new MultipleScheduler(scheduleMethod, settings, stopAfterXIteration, forceStopAfterFirstSuccessExecution));
        }

        /// <inheritdoc />
        public void Start(
            IEnumerable<Func<Task>> scheduleMethods, 
            SchedulerSettings settings, 
            int? stopAfterXIteration = null,
            bool forceStopAfterFirstSuccessExecution = false)
        {
            if (Queue.IsNull()) Queue = new List<MultipleScheduler>();
            Queue.Add(new MultipleScheduler(scheduleMethods, settings, stopAfterXIteration, forceStopAfterFirstSuccessExecution));
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (Queue.IsNotNull() && Queue.Any())
                Queue.ToList().ForEach(n =>
                {
                    n.StopScheduler();
                    //Queue.Remove(n);
                });

            base.StopScheduler();
        }
    }
}