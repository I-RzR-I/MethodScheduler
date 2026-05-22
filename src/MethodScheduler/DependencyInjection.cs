// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-22 23:13
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#if NETSTANDARD2_0_OR_GREATER || NET

#region U S A G E S

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs
{
    /// <summary>
    ///     Dependency injection registration helpers for MethodScheduler.
    /// </summary>
    /// <remarks></remarks>
    public static class DependencyInjection
    {
        /// <summary>
        ///     Registers <see cref="IMethodScheduler"/> as a singleton using
        ///     <see cref="MethodSchedulerService"/>.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddMethodScheduler(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            // Idempotent: defensive double-registration by libraries is safe and yields a
            // single shared scheduler instance.
            services.TryAddSingleton<IMethodScheduler, MethodSchedulerService>();

            return services;
        }

        /// <summary>
        ///     Registers <see cref="IMultipleScheduler"/> as a singleton using the legacy
        ///     <see cref="MultipleScheduler"/> implementation.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        [Obsolete("RegisterMultipleScheduler is obsolete. Use AddMethodScheduler() instead.", error: false)]
        public static IServiceCollection RegisterMultipleScheduler(this IServiceCollection services)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddSingleton<IMultipleScheduler, MultipleScheduler>();
#pragma warning restore CS0618

            return services;
        }

        /// <summary>
        ///     Registers <typeparamref name="TTask"/> as a scoped <see cref="IScheduledTask"/>
        ///     and starts it as an <see cref="IHostedService"/> that runs on the schedule
        ///     defined by <see cref="IScheduledTask.Options"/>.
        /// </summary>
        /// <typeparam name="TTask">
        ///     The task type to register. Must implement <see cref="IScheduledTask"/>.
        /// </typeparam>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        /// <remarks>
        ///     A new DI scope is created for every iteration, so constructor-injected
        ///     scoped services (e.g. <c>DbContext</c>) are resolved and disposed cleanly.
        /// </remarks>
        public static IServiceCollection AddScheduledTask<TTask>(this IServiceCollection services)
            where TTask : class, IScheduledTask
            => services.AddScheduledTask<TTask>(null);

        /// <summary>
        ///     Registers <typeparamref name="TTask"/> as a scoped <see cref="IScheduledTask"/>
        ///     and starts it as an <see cref="IHostedService"/>, applying <paramref name="configure"/>
        ///     on top of the options returned by <see cref="IScheduledTask.Options"/>.
        /// </summary>
        /// <typeparam name="TTask">
        ///     The task type to register. Must implement <see cref="IScheduledTask"/>.
        /// </typeparam>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="configure">
        ///     Optional delegate to override individual <see cref="ScheduledJobOptions"/> properties.
        ///     Applied after reading <see cref="IScheduledTask.Options"/> from the task instance.
        ///     Pass <see langword="null"/> to use the task's own options unchanged.
        /// </param>
        /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
        public static IServiceCollection AddScheduledTask<TTask>(
            this IServiceCollection services,
            Action<ScheduledJobOptions> configure)
            where TTask : class, IScheduledTask
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            // Register the task itself as scoped so it can receive scoped dependencies.
            services.AddScoped<TTask>();

            // Register the hosted service via a factory so we can pass configure without
            // polluting the service container with a ScheduledJobOptions instance.
            services.AddSingleton<IHostedService>(provider =>
                new ScheduledTaskHostedService<TTask>(
                    provider.GetRequiredService<IServiceScopeFactory>(),
                    provider.GetService<ILogger<ScheduledTaskHostedService<TTask>>>(),
                    configure));

            return services;
        }
    }
}
#endif