// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:30
//  ***********************************************************************
//  <copyright file="IScheduledTask.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      mailto: ddpRzR@hotmail.com
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Threading;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Marker interface for a class-based scheduled task registered via
    ///     <c>services.AddScheduledTask&lt;TTask&gt;()</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implement this interface in a class to define a recurring unit of work. The scheduler
    ///         resolves a fresh instance of your class in a new DI scope on every iteration, so
    ///         scoped services such as
    ///         <c>DbContext</c> or <c>IRepository</c> can be safely injected via the
    ///         constructor.
    ///     </para>
    ///     <para>
    ///         Register the task with the DI container using:
    ///         <code>
    ///             services.AddScheduledTask&lt;MyTask&gt;();
    ///         </code>
    ///         or with an option override:
    ///         <code>
    ///             services.AddScheduledTask&lt;MyTask&gt;(opts =&gt;
    ///             {
    ///                 opts.SuccessInterval = TimeSpan.FromMinutes(5);
    ///                 opts.StopOnFailure = true;
    ///             });
    ///         </code>
    ///         Schedule options are read from <see cref="Options" /> on the first iteration. Any <c>
    ///         Action&lt;ScheduledJobOptions&gt;</c> passed at registration time is applied on top
    ///         of those defaults, so you can override individual properties without reimplementing <see cref="Options" />
    ///         .
    ///     </para>
    /// </remarks>
    /// =================================================================================================
    public interface IScheduledTask
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Default scheduling options for this task. These can be overridden at registration time
        ///     via the
        ///     <c>configure</c> parameter of
        ///     <c>AddScheduledTask&lt;TTask&gt;(Action&lt;ScheduledJobOptions&gt;)</c>.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        /// =================================================================================================
        ScheduledJobOptions Options { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes a single iteration of the scheduled work.
        /// </summary>
        /// <param name="cancellationToken">
        ///     Cancelled when the host is shutting down or the task is stopped explicitly. Pass this
        ///     token into every async call inside the method for cooperative cancellation.
        /// </param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}