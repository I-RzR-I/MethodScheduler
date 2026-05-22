// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:33
//  ***********************************************************************
//  <copyright file="TestOptionsFactory.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Scheduling.RecurringJobs.Models;
using SchedulerParallelMode = RzR.Scheduling.RecurringJobs.Enums.ParallelExecutionMode;

#endregion

namespace MethodSchedulerTest.Helpers
{
    internal static class TestOptionsFactory
    {
        internal static ScheduledJobOptions Quick(string? id = null)
        {
            return new ScheduledJobOptions
            {
                Id = id,
                SuccessInterval = TimeSpan.FromMilliseconds(50),
                FailInterval = TimeSpan.FromMilliseconds(50)
            };
        }

        internal static ScheduledJobOptions QuickOptions(string? id = null)
        {
            return Quick(id);
        }

        internal static ScheduledJobOptions OneShot(
            SchedulerParallelMode mode,
            int? maxDop = null)
        {
            return new ScheduledJobOptions
            {
                SuccessInterval = TimeSpan.FromMilliseconds(50),
                StopOnFirstSuccess = true,
                ParallelMode = mode,
                MaxDegreeOfParallelism = maxDop
            };
        }
    }
}