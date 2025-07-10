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
using DomainCommonExtensions.CommonExtensions.TypeParam;
using DomainCommonExtensions.DataTypeExtensions;
using MethodScheduler.Extensions;
using MethodScheduler.Models;

// ReSharper disable AsyncVoidLambda

#endregion

namespace MethodScheduler.Helpers
{
    /// <summary>
    ///     Scheduler base
    /// </summary>
    /// <remarks></remarks>
    public class InternalSchedulerBase
    {
        /// <summary>
        ///     Execute timer
        /// </summary>
        /// <remarks></remarks>
        private Timer _timer;

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
        ///     <remarks>
        ///         If value is NULL, the execution will not stop
        ///         until <seealso cref="InternalSchedulerBase.StopScheduler"/> or
        ///         <seealso cref="MultipleScheduler.Stop"/> will be invoked.
        ///     </remarks>
        /// </summary>
        protected int? StopAfterXIteration;

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
                    Task.WaitAll(scheduleMethods.Select(m => Task.Factory.StartNew(m.Invoke)).ToArray());

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
                    await scheduleMethod.Invoke();
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
                    Task.WaitAll(scheduleMethods.Select(m => Task.Factory.StartNew(async () => await m.Invoke()))
                        .Cast<Task>().ToArray());

                    await Task.CompletedTask;
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
            }, null, _interval.MinutesToMs(), Timeout.Infinite);
        }

        /// <summary>
        ///     Stop scheduler/timer
        /// </summary>
        /// <remarks></remarks>
        protected void StopScheduler()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        ///     Finally throw or change timer value.
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
            else _timer.Change(_interval.MinutesToMs(), Timeout.Infinite);

            if (StopAfterXIteration.IsNullOrZero().IsFalse() && executionIteration >= StopAfterXIteration.IfIsNull(0))
                StopScheduler();
        }
    }
}