// ***********************************************************************
//  Assembly          : RzR.Services.MethodScheduler
//  Author            : RzR
//  Created           : 11-05-2026 21:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 19:38
//  ***********************************************************************
//  <copyright file="ScheduledJob.cs" company="RzR SOFT & TECH">
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
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Internal implementation of <see cref="IScheduledJob" />. Runs the schedule loop as a <see cref="Task" />
    ///     -based async loop so that async work is properly awaited and exceptions do not escape to
    ///     the process-level unhandled-exception handler.
    /// </summary>
    /// <seealso cref="T:MethodScheduler.Abstractions.IScheduledJob"/>
    /// <seealso cref="T:IDisposable"/>
    /// =================================================================================================
    internal sealed class ScheduledJob : IScheduledJob, IDisposable
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the cts.
        /// </summary>
        /// =================================================================================================
        private readonly CancellationTokenSource _cts;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the on terminated.
        /// </summary>
        /// =================================================================================================
        private readonly Action<string> _onTerminated;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) options for controlling the operation.
        /// </summary>
        /// =================================================================================================
        private readonly ScheduledJobOptions _options;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable) the work.
        /// </summary>
        /// =================================================================================================
        private readonly Func<CancellationToken, Task> _work;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The disposed.
        /// </summary>
        /// =================================================================================================
        private int _disposed;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Interlocked counter — safe to read from any thread.
        /// </summary>
        /// =================================================================================================
        private int _iterationCount;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The last error.
        /// </summary>
        /// =================================================================================================
        private Exception _lastError;

        // Observability fields. Reads may be slightly stale; the contract is documented
        // on IScheduledJob. On 32-bit runtimes torn reads of these struct values are
        // possible — callers needing strong consistency should snapshot via Completion.

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     All state writes go through Interlocked.Exchange so reads via Volatile.Read observe a
        ///     consistent value paired with the most recent _lastError write.
        /// </summary>
        /// =================================================================================================
        private int _state;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScheduledJob"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="options">Options for controlling the operation.</param>
        /// <param name="work">The work.</param>
        /// <param name="schedulerToken">A token that allows processing to be cancelled.</param>
        /// <param name="onTerminated">The on terminated.</param>
        /// =================================================================================================
        internal ScheduledJob(
            string id,
            ScheduledJobOptions options,
            Func<CancellationToken, Task> work,
            CancellationToken schedulerToken,
            Action<string> onTerminated)
        {
            Id = id;
            _options = options;
            _work = work;
            _onTerminated = onTerminated;

            // Link to the parent scheduler's token so StopAllAsync cancels everything.
            _cts = CancellationTokenSource.CreateLinkedTokenSource(schedulerToken);
            Volatile.Write(ref _state, (int)JobState.Pending);

            // Task.Run is appropriate for async loops: the thread is released at every
            // await, so LongRunning would only waste a dedicated thread for no benefit.
            Completion = Task.Run(() => RunAsync(_cts.Token));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _cts.Dispose();
        }

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public JobState State => (JobState)Volatile.Read(ref _state);

        /// <inheritdoc/>
        public int IterationCount => Volatile.Read(ref _iterationCount);

        /// <inheritdoc/>
        public DateTimeOffset? LastRunAt { get; private set; }

        /// <inheritdoc/>
        public DateTimeOffset? LastSuccessAt { get; private set; }

        /// <inheritdoc/>
        public DateTimeOffset? NextRunAt { get; private set; }

        /// <inheritdoc/>
        public Exception LastError => Volatile.Read(ref _lastError);

        /// <inheritdoc/>
        public Task Completion { get; }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            // Wait for the run loop to finish. Use Task.WhenAny so the caller's cancellationToken can
            // abort the wait without affecting the actual job cancellation. We do not
            // await _runTask here directly because that would surface ThrowOnFailure
            // exceptions to callers of StopAsync that didn't ask for them; consumers who
            // want exceptions should await IScheduledJob.Completion explicitly.
            await Task.WhenAny(Completion, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Executes the 'asynchronous' operation.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_options.InitialDelay.HasValue)
                {
                    await Task.Delay(_options.InitialDelay.Value, cancellationToken).ConfigureAwait(false);
                }

                Interlocked.Exchange(ref _state, (int)JobState.Running);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var iterationFailed = false;
                    Exception iterationError = null;

                    LastRunAt = DateTimeOffset.UtcNow;
                    NextRunAt = null;

                    try
                    {
                        await _work(cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        // Cooperative cancellation — exit cleanly without treating as failure.
                        break;
                    }
                    catch (Exception ex)
                    {
                        iterationFailed = true;
                        iterationError = ex;
                        Interlocked.Exchange(ref _lastError, ex);
                    }

                    Interlocked.Increment(ref _iterationCount);

                    if (!iterationFailed)
                    {
                        // Clear stale LastError on every successful iteration so observers
                        // (alerts, dashboards) see recovery instead of a permanently stuck error.
                        Interlocked.Exchange(ref _lastError, null);
                        LastSuccessAt = DateTimeOffset.UtcNow;
                    }

                    // ---- Stop-condition evaluation (order matters) ----

                    if (iterationFailed && _options.ThrowOnFailure)
                    {
                        Interlocked.Exchange(ref _state, (int)JobState.Faulted);
                        throw iterationError;
                    }

                    if (iterationFailed && _options.StopOnFailure)
                    {
                        Interlocked.Exchange(ref _state, (int)JobState.Faulted);
                        break;
                    }

                    if (!iterationFailed && _options.StopOnFirstSuccess)
                    {
                        Interlocked.Exchange(ref _state, (int)JobState.Stopped);
                        break;
                    }

                    if (_options.MaxIterations.HasValue && _iterationCount >= _options.MaxIterations.Value)
                    {
                        Interlocked.Exchange(ref _state, (int)JobState.Stopped);
                        break;
                    }

                    // ---- Schedule next iteration ----

                    var delay = iterationFailed ? _options.FailInterval : _options.SuccessInterval;
                    NextRunAt = DateTimeOffset.UtcNow + delay;

                    try
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Initial delay or outer cancellation — exit cleanly.
            }
            finally
            {
                // Transition to Stopped if not already in a terminal state.
                Interlocked.CompareExchange(ref _state, (int)JobState.Stopped, (int)JobState.Running);
                Interlocked.CompareExchange(ref _state, (int)JobState.Stopped, (int)JobState.Pending);
                NextRunAt = null;

                // Notify the scheduler so naturally-completed jobs do not leak in the registry.
                // Idempotent: TryRemove on a non-existent key is a no-op.
                try
                {
                    _onTerminated?.Invoke(Id);
                }
                catch
                {
                    /* never propagate */
                }
            }
        }
    }
}