// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:37
//  ***********************************************************************
//  <copyright file="ScheduledTaskHostedService.cs" company="RzR SOFT & TECH">
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     An <see cref="IHostedService" /> that runs a class-based <see cref="IScheduledTask" />
    ///     on a repeating schedule.
    /// </summary>
    /// <remarks>
    ///     Registered automatically by
    ///     <c>services.AddScheduledTask&lt;TTask&gt;()</c> — do not register directly.
    /// </remarks>
    /// <typeparam name="TTask">
    ///     The concrete task type. Resolved from a new DI scope on every iteration so that scoped
    ///     services (e.g. <c>DbContext</c>) are safely recreated each time.
    /// </typeparam>
    /// <seealso cref="T:Microsoft.Extensions.Hosting.BackgroundService"/>
    /// =================================================================================================
    internal sealed class ScheduledTaskHostedService<TTask> : BackgroundService
        where TTask : class, IScheduledTask
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the configure option.
        /// </summary>
        /// =================================================================================================
        private readonly Action<ScheduledJobOptions> _configure;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the logger.
        /// </summary>
        /// =================================================================================================
        private readonly ILogger<ScheduledTaskHostedService<TTask>> _logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the scope factory.
        /// </summary>
        /// =================================================================================================
        private readonly IServiceScopeFactory _scopeFactory;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScheduledTaskHostedService{TTask}"/>
        ///     class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <param name="scopeFactory">The scope factory.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configure">The configure option.</param>
        /// =================================================================================================
        internal ScheduledTaskHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<ScheduledTaskHostedService<TTask>> logger,
            Action<ScheduledJobOptions> configure)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger;
            _configure = configure;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Resolve the task once to read its default options.
            ScheduledJobOptions options;
            using (var initScope = _scopeFactory.CreateScope())
            {
                var initTask = initScope.ServiceProvider.GetRequiredService<TTask>();
                options = initTask.Options ?? new ScheduledJobOptions();
            }

            // Apply any registration-time overrides on top of the task's own defaults.
            _configure?.Invoke(options);

            var taskName = typeof(TTask).Name;

            _logger?.LogInformation(
                "Scheduled task {TaskName} starting. SuccessInterval={SuccessInterval}, FailInterval={FailInterval}.",
                taskName, options.SuccessInterval, options.FailInterval);

            // Initial delay before the first iteration.
            if (options.InitialDelay.HasValue)
            {
                _logger?.LogDebug("Scheduled task {TaskName} waiting for initial delay of {Delay}.", taskName,
                    options.InitialDelay.Value);
                try
                {
                    await Task.Delay(options.InitialDelay.Value, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            var iterationCount = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                var failed = false;

                // Create a fresh DI scope per iteration — lets DbContext and other scoped
                // services be resolved, used, and disposed cleanly each time.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var task = scope.ServiceProvider.GetRequiredService<TTask>();

                    _logger?.LogDebug(
                        "Scheduled task {TaskName} starting iteration {Iteration}.",
                        taskName, iterationCount + 1);

                    try
                    {
                        await task.ExecuteAsync(stoppingToken).ConfigureAwait(false);

                        _logger?.LogDebug(
                            "Scheduled task {TaskName} completed iteration {Iteration}.",
                            taskName, iterationCount + 1);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        // Host is shutting down — exit cleanly without logging as an error.
                        return;
                    }
                    catch (Exception ex)
                    {
                        failed = true;

                        _logger?.LogError(
                            ex,
                            "Scheduled task {TaskName} failed on iteration {Iteration}.",
                            taskName, iterationCount + 1);

                        if (options.ThrowOnFailure)
                        {
                            throw;
                        }

                        if (options.StopOnFailure)
                        {
                            _logger?.LogWarning(
                                "Scheduled task {TaskName} stopped after failure (StopOnFailure=true).",
                                taskName);
                            return;
                        }
                    }
                }

                iterationCount++;

                if (!failed && options.StopOnFirstSuccess)
                {
                    _logger?.LogInformation(
                        "Scheduled task {TaskName} stopping after first successful iteration (StopOnFirstSuccess=true).",
                        taskName);
                    return;
                }

                if (options.MaxIterations.HasValue && iterationCount >= options.MaxIterations.Value)
                {
                    _logger?.LogInformation(
                        "Scheduled task {TaskName} stopping after reaching MaxIterations={MaxIterations}.",
                        taskName, options.MaxIterations.Value);
                    return;
                }

                var delay = failed ? options.FailInterval : options.SuccessInterval;

                _logger?.LogDebug(
                    "Scheduled task {TaskName} waiting {Delay} before next iteration.",
                    taskName, delay);

                try
                {
                    await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}