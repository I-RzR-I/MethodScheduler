// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-23 09:27
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="IMultipleScheduler.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MethodScheduler.Models;

#endregion

namespace MethodScheduler.Abstractions
{
    /// <summary>
    ///     Multiple method scheduler
    /// </summary>
    public interface IMultipleScheduler
    {
        /// <summary>
        ///     Start new schedule
        /// </summary>
        /// <param name="scheduleMethod">Required. Method to schedule</param>
        /// <param name="settings">Required. Scheduler settings</param>
        /// <remarks></remarks>
        void Start(Action scheduleMethod, SchedulerSettings settings);

        /// <summary>
        ///     Start new schedule
        /// </summary>
        /// <param name="scheduleMethods">Required. Methods to schedule</param>
        /// <param name="settings">Required. Scheduler settings</param>
        /// <remarks></remarks>
        void Start(IEnumerable<Action> scheduleMethods, SchedulerSettings settings);

        /// <summary>
        ///     Start new schedule
        /// </summary>
        /// <param name="scheduleMethod">Required. Method to schedule</param>
        /// <param name="settings">Required. Scheduler settings</param>
        /// <remarks></remarks>
        void Start(Func<Task> scheduleMethod, SchedulerSettings settings);

        /// <summary>
        ///     Start new schedule
        /// </summary>
        /// <param name="scheduleMethods">Required. Methods to schedule</param>
        /// <param name="settings">Required. Scheduler settings</param>
        /// <remarks></remarks>
        void Start(IEnumerable<Func<Task>> scheduleMethods, SchedulerSettings settings);

        /// <summary>
        ///     Stop scheduler
        /// </summary>
        /// <remarks></remarks>
        void Stop();
    }
}