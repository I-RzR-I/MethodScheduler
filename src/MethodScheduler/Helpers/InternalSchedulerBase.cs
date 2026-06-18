// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-22 23:29
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="InternalSchedulerBase.cs" company="">
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RzR.Extensions.Domain.Primitives;
using RzR.Extensions.Domain.Reflection.TypeParam;
using RzR.Scheduling.RecurringJobs.Extensions;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace RzR.Scheduling.RecurringJobs.Helpers
{
    /// <summary>
    ///     Scheduler base.
    /// </summary>
    /// <remarks>
    ///     This class is obsolete. Use <see cref="MethodSchedulerService"/> instead.
    /// </remarks>
    [Obsolete("InternalSchedulerBase is obsolete. Use MethodSchedulerService and ScheduledJobOptions instead.", error: false)]
    public class InternalSchedulerBase : IDisposable
    {
        /// <summary>
        ///     Execute timer
        /// </summary>
        /// <remarks></remarks>
        private Timer _timer;

        private bool _disposed;

        /// <summary>
        ///     Current execute interval
        /// </summary>
        /// <remarks></remarks>
        private double _interval;

        /// <summary>
        ///     Scheduler settings
        /// </summary>
        protected SchedulerSettings Settings;

        /// <summary>
        ///     The stop execution after x iteration.
        /// </summary>
        /// <remarks>
        ///     If value is NULL, the execution will not stop
        ///     until <seealso cref="InternalSchedulerBase.StopScheduler"/> or
        ///     <seealso cref="MultipleScheduler.Stop"/> will be invoked.
        /// </remarks>
        protected int? StopAfterXIteration;

        /// <summary>
        ///     True to force stop after the first successful execution.
        /// </summary>
        /// <remarks>
        ///     If there is no error at the current execution
        ///     and this field is set to 'true',
        ///     the scheduler will stop the infinite execution.
        /// </remarks>
        protected bool ForceStopAfterFirstSuccessExecution;

        /// <summary>
        ///     Start methods scheduler
        /// </summary>
        /// <param name="scheduleMethod">Schedule method</param>
        /// <remarks></remarks>
        protected void StartScheduler(Action scheduleMethod)
        {
            var error = new StringBuilder();
            var failure = false;
            var executionIteration = 0;

            _timer = new Timer(_ =>
            {
                try
                {
                    failure = false;
                    error.Clear();

                    try
                    {
                        scheduleMethod.Invoke();
                        _interval = Settings.SuccessInterval;
                    }
                    catch (Exception e)
                    {
                        failure = true;
                        error.AppendLine(e.ToString());
                        _interval = Settings.FailInterval;
                    }
                    finally
                    {
                        if (StopAfterXIteration.IsNullOrZero().IsFalse())
                            executionIteration++;
                        FinallyThrowOrChangeTimer(failure, error, executionIteration);
                    }
                }
                catch
                {
                    // Swallow — must not escape a TimerCallback and crash the process.
                    // Use MethodSchedulerService for observable failure handling.
                }
            }, null, _interval.MinutesToMs(), Timeout.Infinite);
        }

        /// <summary>
        ///     Start methods scheduler
        /// </summary>
        /// <param name="scheduleMethods">Schedule methods</param>
        /// <remarks></remarks>
        protected void StartScheduler(IEnumerable<Action> scheduleMethods)
        {
            var error = new StringBuilder();
            var failure = false;
            var executionIteration = 0;

            _timer = new Timer(_ =>
            {
                try
                {
                    failure = false;
                    error.Clear();

                    try
                    {
                        Task.WaitAll(scheduleMethods.Select(m => Task.Run(m.Invoke)).ToArray());
                        _interval = Settings.SuccessInterval;
                    }
                    catch (Exception e)
                    {
                        failure = true;
                        error.AppendLine(e.ToString());
                        _interval = Settings.FailInterval;
                    }
                    finally
                    {
                        if (StopAfterXIteration.IsNullOrZero().IsFalse())
                            executionIteration++;
                        FinallyThrowOrChangeTimer(failure, error, executionIteration);
                    }
                }
                catch
                {
                    // Swallow — must not escape a TimerCallback and crash the process.
                }
            }, null, _interval.MinutesToMs(), Timeout.Infinite);
        }

        /// <summary>
        ///     Start methods scheduler
        /// </summary>
        /// <param name="scheduleMethod">Schedule method</param>
        /// <remarks></remarks>
        protected void StartScheduler(Func<Task> scheduleMethod)
        {
            var error = new StringBuilder();
            var failure = false;
            var executionIteration = 0;

            _timer = new Timer(async _ =>
            {
                try
                {
                    failure = false;
                    error.Clear();

                    try
                    {
                        await scheduleMethod.Invoke().ConfigureAwait(false);
                        _interval = Settings.SuccessInterval;
                    }
                    catch (Exception e)
                    {
                        failure = true;
                        error.AppendLine(e.ToString());
                        _interval = Settings.FailInterval;
                    }
                    finally
                    {
                        if (StopAfterXIteration.IsNullOrZero().IsFalse())
                            executionIteration++;
                        FinallyThrowOrChangeTimer(failure, error, executionIteration);
                    }
                }
                catch
                {
                    // Swallow — async void must not let exceptions escape to the thread pool.
                    // Use MethodSchedulerService for observable failure handling.
                }
            }, null, _interval.MinutesToMs(), Timeout.Infinite);
        }

        /// <summary>
        ///     Start methods scheduler
        /// </summary>
        /// <param name="scheduleMethods">Schedule methods</param>
        /// <remarks></remarks>
        protected void StartScheduler(IEnumerable<Func<Task>> scheduleMethods)
        {
            var error = new StringBuilder();
            var failure = false;
            var executionIteration = 0;

            _timer = new Timer(async _ =>
            {
                try
                {
                    failure = false;
                    error.Clear();

                    try
                    {
                        // Task.Run unwraps Task<Task> correctly; WhenAll awaits the inner async work.
                        await Task.WhenAll(scheduleMethods.Select(m => Task.Run(m.Invoke))).ConfigureAwait(false);
                        _interval = Settings.SuccessInterval;
                    }
                    catch (Exception e)
                    {
                        failure = true;
                        error.AppendLine(e.ToString());
                        _interval = Settings.FailInterval;
                    }
                    finally
                    {
                        if (StopAfterXIteration.IsNullOrZero().IsFalse())
                            executionIteration++;
                        FinallyThrowOrChangeTimer(failure, error, executionIteration);
                    }
                }
                catch
                {
                    // Swallow — async void must not let exceptions escape to the thread pool.
                    // Use MethodSchedulerService for observable failure handling.
                }
            }, null, _interval.MinutesToMs(), Timeout.Infinite);
        }

        /// <summary>
        ///     Stop scheduler/timer
        /// </summary>
        /// <remarks></remarks>
        protected void StopScheduler()
        {
            try { _timer?.Change(Timeout.Infinite, Timeout.Infinite); } catch (ObjectDisposedException) { }
            _timer?.Dispose();
            _timer = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopScheduler();
        }

        /// <summary>
        ///     Finally, throw exception or change timer value.
        /// </summary>
        /// <exception cref="Exception">Thrown when an exception error condition occurs.</exception>
        /// <param name="isFailure">True if is failure, false if not.</param>
        /// <param name="error">The error.</param>
        /// <param name="executionIteration">The execution iteration.</param>
        private void FinallyThrowOrChangeTimer(bool isFailure, StringBuilder error, int executionIteration)
        {
            if (BooleanExtensions.AllTrue(Settings.ThrowException, isFailure).IsTrue())
                throw new Exception($"{error}");

            if (BooleanExtensions.AllTrue(Settings.DisableOnFailure, isFailure).IsTrue())
                StopScheduler();
            else { try { _timer?.Change(_interval.MinutesToMs(), Timeout.Infinite); } catch (ObjectDisposedException) { } }

            if (StopAfterXIteration.IsNullOrZero().IsFalse() && executionIteration >= StopAfterXIteration.IfIsNull(0))
                StopScheduler();

            if (isFailure.IsFalse() && ForceStopAfterFirstSuccessExecution.IsTrue())
                StopScheduler();
        }
    }
}