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
        /// <param name="scheduleMethod"></param>
        /// <remarks></remarks>
        protected void StartScheduler(Func<Task> scheduleMethod)
        {
            var error = new StringBuilder();
            var failure = false;

            _timer = new Timer(async state =>
            {
                try
                {
                    await scheduleMethod();
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
                    if (Settings.ThrowException.Equals(true))
                        throw new Exception($"{error}");

                    if (Settings.DisableOnFailure.Equals(true) && failure.Equals(true))
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    else _timer.Change(_interval.MinutesToMs(), Timeout.Infinite);
                }
            }, null, _interval.MinutesToMs(), Timeout.Infinite);
        }

        /// <summary>
        ///     Start methods scheduler
        /// </summary>
        /// <param name="scheduleMethods"></param>
        /// <remarks></remarks>
        protected void StartScheduler(IEnumerable<Func<Task>> scheduleMethods)
        {
            var error = new StringBuilder();
            var failure = false;

            _timer = new Timer(async state =>
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
                    if (Settings.ThrowException.Equals(true))
                        throw new Exception($"{error}");

                    if (Settings.DisableOnFailure.Equals(true) && failure.Equals(true))
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