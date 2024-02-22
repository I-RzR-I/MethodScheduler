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
using DomainCommonExtensions.DataTypeExtensions;
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
        ///     Start methods scheduler
        /// </summary>
        /// <param name="scheduleMethod">Schedule method</param>
        /// <remarks></remarks>
        protected void StartScheduler(Action scheduleMethod)
        {
            var error = new StringBuilder();
            var failure = false;

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
                    if (Settings.ThrowException.IsTrue() && failure.IsTrue())
                        throw new Exception($"{error}");

                    if (Settings.DisableOnFailure.IsTrue() && failure.IsTrue())
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    else _timer.Change(_interval.MinutesToMs(), Timeout.Infinite);
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
                    if (Settings.ThrowException.IsTrue() && failure.IsTrue())
                        throw new Exception($"{error}");

                    if (Settings.DisableOnFailure.IsTrue() && failure.IsTrue())
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    else _timer.Change(_interval.MinutesToMs(), Timeout.Infinite);
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
                    if (Settings.ThrowException.IsTrue() && failure.IsTrue())
                        throw new Exception($"{error}");

                    if (Settings.DisableOnFailure.IsTrue() && failure.IsTrue())
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    else _timer.Change(_interval.MinutesToMs(), Timeout.Infinite);
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
                    if (Settings.ThrowException.IsTrue() && failure.IsTrue())
                        throw new Exception($"{error}");

                    if (Settings.DisableOnFailure.IsTrue() && failure.IsTrue())
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    else _timer.Change(_interval.MinutesToMs(), Timeout.Infinite);
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
    }
}