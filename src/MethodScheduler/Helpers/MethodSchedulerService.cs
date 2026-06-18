// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 10-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:15
//  ***********************************************************************
//  <copyright file="MethodSchedulerService.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RzR.Extensions.Domain.Primitives;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;
using SchedulerParallelMode = RzR.Scheduling.RecurringJobs.Enums.ParallelExecutionMode;

#endregion

namespace RzR.Scheduling.RecurringJobs.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A service for accessing method schedulers information. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:RzR.Scheduling.RecurringJobs.Abstractions.IMethodScheduler" />
    /// <seealso cref="T:IDisposable" />
    /// ###
    /// <inheritdoc cref="IMethodScheduler" />
    /// =================================================================================================
    public sealed class MethodSchedulerService : IMethodScheduler, IDisposable
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) Lazily initialised process-wide instance for callers that don't use DI. Lazy&lt;T&gt;
        ///     guarantees thread-safe single construction without explicit locking and defers allocation
        ///     until the property is first read.
        /// </summary>
        /// =================================================================================================
        private static readonly Lazy<MethodSchedulerService> DefaultService =
            new(() => new MethodSchedulerService(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) Coordinates swap of _schedulerCts during StopAllAsync so a concurrent
        ///     Schedule call cannot read a half-disposed CTS.
        /// </summary>
        /// =================================================================================================
        private readonly object _ctsLock = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) Thread-safe job registry: jobId → running job.
        /// </summary>
        /// =================================================================================================
        private readonly ConcurrentDictionary<string, ScheduledJob> _jobs = new(StringComparer.Ordinal);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The disposed.
        /// </summary>
        /// =================================================================================================
        private int _disposed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Cancels every job when StopAllAsync runs or when the scheduler is disposed. Replaced
        ///     after StopAllAsync so the scheduler remains usable.
        /// </summary>
        /// =================================================================================================
        private CancellationTokenSource _schedulerCts = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a process-wide default scheduler instance for callers that do not use dependency
        ///     injection (e.g. console apps, .NET Framework code, static helpers).
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Use this only when DI is unavailable. In ASP.NET Core / Worker Service apps prefer
        ///         <c>
        ///             services.AddMethodScheduler()
        ///         </c>
        ///         + constructor injection of
        ///         <see cref="IMethodScheduler" /> so the host can dispose the scheduler on shutdown.
        ///     </para>
        ///     <para>
        ///         The default instance is <b>not</b> automatically disposed. Call
        ///         <see cref="StopAllAsync" /> or <see cref="DisposeAsync" /> explicitly during
        ///         application shutdown to stop running jobs cleanly — for example by hooking
        ///         <see cref="AppDomain.ProcessExit" />.
        ///     </para>
        /// </remarks>
        /// <value>
        ///     The default.
        /// </value>
        /// =================================================================================================
        public static MethodSchedulerService Default => DefaultService.Value;

        /// -------------------------------------------------------------------------------------------------
        /// <remarks>
        ///     Synchronous wrapper over <see cref="DisposeAsync" />. Provided so that
        ///     <c>using var scheduler = new MethodSchedulerService();</c> works in
        ///     synchronous contexts (e.g. unit tests). In async code prefer
        ///     <c>await using var scheduler = new MethodSchedulerService();</c>.
        ///     <para>
        ///         Offloaded to <see cref="Task.Run(Func{Task})" /> so that, if the caller happens to be
        ///         running on the same thread-pool worker as one of the scheduled jobs (e.g. a job whose
        ///         work delegate completed a
        ///         <c>TaskCompletionSource</c> with the default synchronous
        ///         continuation behaviour, then the test method awaited it and reached this Dispose),
        ///         the blocking wait does not deadlock against the very task it is waiting for.
        ///     </para>
        /// </remarks>
        /// <inheritdoc />
        /// =================================================================================================
        public void Dispose()
        {
            Task.Run(() => DisposeAsync().AsTask()).GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public IScheduledJob Schedule(ScheduledJobOptions options, Func<CancellationToken, Task> work)
        {
            ThrowIfDisposed();

            if (options.IsNull())
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (work.IsNull())
            {
                throw new ArgumentNullException(nameof(work));
            }

            ValidateOptions(options);

            var id = string.IsNullOrEmpty(options.Id) ? Guid.NewGuid().ToString("N") : options.Id;

            CancellationToken cancellationToken;
            lock (_ctsLock)
            {
                cancellationToken = _schedulerCts.Token;
            }

            var job = new ScheduledJob(id, options, work, cancellationToken, DeregisterJob);

            // Reject duplicate IDs explicitly instead of orphaning the previous job.
            if (!_jobs.TryAdd(id, job))
            {
                job.Dispose();
                throw new InvalidOperationException(
                    $"A job with id '{id}' is already scheduled. Stop it first via TryStopAsync.");
            }

            return job;
        }

        /// <inheritdoc />
        public IScheduledJob Schedule(ScheduledJobOptions options, params Func<CancellationToken, Task>[] work)
        {
            ThrowIfDisposed();

            if (options.IsNull())
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (work.IsNull() || work.Length == 0)
            {
                throw new ArgumentException("At least one work item is required.", nameof(work));
            }

            // Validate every entry eagerly so the failure surfaces at Schedule() time
            // instead of as a NullReferenceException inside the async loop.
            for (var i = 0; i < work.Length; i++)
            {
                if (work[i].IsNull())
                {
                    throw new ArgumentException($"Work item at index {i} is null.", nameof(work));
                }
            }

            if (options.MaxDegreeOfParallelism.HasValue && options.MaxDegreeOfParallelism.Value < 1)
            {
                throw new ArgumentException(
                    $"{nameof(ScheduledJobOptions.MaxDegreeOfParallelism)} must be >= 1 when set.",
                    nameof(options));
            }

            // Single item — bypass all parallel logic.
            if (work.Length == 1)
            {
                return Schedule(options, work[0]);
            }

            var ignoreErrors = options.ParallelMode == SchedulerParallelMode.WhenAllIgnoreErrors;

            Func<CancellationToken, Task> combined;

            switch (options.ParallelMode)
            {
                case SchedulerParallelMode.Sequential:
                    // DOP = 1 by nature; MaxDegreeOfParallelism is not applicable.
                    combined = async cancellationToken =>
                    {
                        foreach (var item in work)
                        {
                            await item(cancellationToken).ConfigureAwait(false);
                        }
                    };
                    break;

                default: // WhenAll or WhenAllIgnoreErrors
                    var dop = options.MaxDegreeOfParallelism;
                    var isThrottled = dop.HasValue && dop.Value < work.Length;

                    if (isThrottled)
                    {
                        // Bounded concurrency: semaphore-based, works on all target frameworks.
                        var capturedWork = work;
                        var capturedDop = dop.Value;
                        combined = cancellationToken =>
                            ExecuteThrottledAsync(capturedWork, capturedDop, ignoreErrors, cancellationToken);
                    }
                    else if (ignoreErrors)
                    {
                        combined = cancellationToken
                            => Task.WhenAll(Array.ConvertAll(work, item
                                => InvokeIgnoringErrors(item, cancellationToken)));
                    }
                    else
                    {
                        combined = cancellationToken
                            => Task.WhenAll(Array.ConvertAll(work, item => item(cancellationToken)));
                    }

                    break;
            }

            return Schedule(options, combined);
        }

        /// <inheritdoc />
        public async Task<bool> TryStopAsync(string jobId, CancellationToken cancellationToken = default)
        {
            if (jobId.IsNull())
            {
                throw new ArgumentNullException(nameof(jobId));
            }

            if (!_jobs.TryRemove(jobId, out var job))
            {
                return false;
            }

            await job.StopAsync(cancellationToken).ConfigureAwait(false);
            job.Dispose();

            return true;
        }

        /// <inheritdoc />
        public async Task StopAllAsync(CancellationToken cancellationToken = default)
        {
            // Atomically swap the scheduler CTS so any future Schedule() calls bind to a
            // fresh, non-cancelled token. The old CTS still cancels every running job.
            CancellationTokenSource oldCts;
            lock (_ctsLock)
            {
                oldCts = _schedulerCts;
                if (_disposed == 0)
                {
                    _schedulerCts = new CancellationTokenSource();
                }
            }

            try
            {
                oldCts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            var snapshot = _jobs.Values.ToArray();
            _jobs.Clear();

            try
            {
                // ReSharper disable once PossiblyMistakenUseOfCancellationToken
                await Task.WhenAll(Array.ConvertAll(snapshot, j => j.StopAsync(cancellationToken)))
                    .ConfigureAwait(false);
            }
            finally
            {
                foreach (var job in snapshot)
                {
                    job.Dispose();
                }

                oldCts.Dispose();
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            await StopAllAsync().ConfigureAwait(false);

            // After StopAllAsync the lock-protected _schedulerCts is the post-swap instance;
            // dispose it explicitly so we don't leak the replacement created during shutdown.
            CancellationTokenSource finalCts;
            lock (_ctsLock)
            {
                finalCts = _schedulerCts;
            }

            try
            {
                finalCts.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Called by ScheduledJob when its run loop exits naturally so the registry does not retain
        ///     references to terminated jobs.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// =================================================================================================
        private void DeregisterJob(string id)
        {
            if (_jobs.TryRemove(id, out var job))
            {
                job.Dispose();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Validates the options described by options.
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="options">Options for controlling the operation.</param>
        /// =================================================================================================
        private static void ValidateOptions(ScheduledJobOptions options)
        {
            if (options.SuccessInterval <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    $"{nameof(ScheduledJobOptions.SuccessInterval)} must be greater than zero.",
                    nameof(options));
            }

            if (options.FailInterval <= TimeSpan.Zero)
            {
                throw new ArgumentException(
                    $"{nameof(ScheduledJobOptions.FailInterval)} must be greater than zero.",
                    nameof(options));
            }

            if (options.InitialDelay.HasValue && options.InitialDelay.Value < TimeSpan.Zero)
            {
                throw new ArgumentException(
                    $"{nameof(ScheduledJobOptions.InitialDelay)} must be non-negative.",
                    nameof(options));
            }

            if (options.MaxIterations.HasValue && options.MaxIterations.Value < 1)
            {
                throw new ArgumentException(
                    $"{nameof(ScheduledJobOptions.MaxIterations)} must be >= 1 when set.",
                    nameof(options));
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes <paramref name="work" /> items with bounded concurrency controlled by a
        ///     <see cref="SemaphoreSlim" />. At most <paramref name="maxDop" /> items run at the
        ///     same time. Compatible with all target frameworks (netstandard2.0+).
        /// </summary>
        /// <exception cref="OperationCanceledException">
        ///     Thrown when a thread cancels a running operation.
        /// </exception>
        /// <param name="work">The work.</param>
        /// <param name="maxDop">The maximum dop.</param>
        /// <param name="ignoreErrors">True to ignore errors.</param>
        /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        private static async Task ExecuteThrottledAsync(Func<CancellationToken, Task>[] work,
            int maxDop, bool ignoreErrors, CancellationToken cancellationToken)
        {
            using (var semaphore = new SemaphoreSlim(maxDop, maxDop))
            {
                var tasks = new List<Task>(work.Length);

                try
                {
                    foreach (var item in work)
                    {
                        // Block until a concurrency slot is free (or cancellation requested).
                        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                        // Start the work item; release the slot in a finally so it is always returned.
                        tasks.Add(RunWithRelease(item, semaphore, ignoreErrors, cancellationToken));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Cancelled while waiting for a slot.
                    // Wait for already-started tasks so we don't abandon running work.
                    if (tasks.Count > 0)
                    {
                        await Task.WhenAll(tasks).ConfigureAwait(false);
                    }

                    throw;
                }

                // Wait for every started task; propagates any exceptions from non-ignored failures.
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Runs a single work item and guarantees the semaphore slot is released afterwards.
        /// </summary>
        /// <param name="work">The work.</param>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="ignoreErrors">True to ignore errors.</param>
        /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        private static async Task RunWithRelease(Func<CancellationToken, Task> work,
            SemaphoreSlim semaphore, bool ignoreErrors, CancellationToken cancellationToken)
        {
            try
            {
                if (ignoreErrors)
                {
                    await InvokeIgnoringErrors(work, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await work(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes the ignoring errors on a different thread, and waits for the result.
        /// </summary>
        /// <exception cref="OperationCanceledException">
        ///     Thrown when a thread cancels a running operation.
        /// </exception>
        /// <param name="work">The work.</param>
        /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        private static async Task InvokeIgnoringErrors(Func<CancellationToken, Task> work,
            CancellationToken cancellationToken)
        {
            try
            {
                await work(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw; // Always propagate cooperative cancellation.
            }
            catch
            {
                // Intentionally swallowed per WhenAllIgnoreErrors semantics.
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Throw if disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     Thrown when a supplied object has been disposed.
        /// </exception>
        /// =================================================================================================
        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) == 1)
            {
                throw new ObjectDisposedException(nameof(MethodSchedulerService));
            }
        }
    }
}