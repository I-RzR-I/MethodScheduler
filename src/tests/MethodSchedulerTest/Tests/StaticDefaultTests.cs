// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 20:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="StaticDefaultTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class StaticDefaultTests
    {
        [TestMethod]
        public void Default_ReturnsSameInstance_Test()
        {
            var a = MethodSchedulerService.Default;
            var b = MethodSchedulerService.Default;

            Assert.AreSame(a, b);
        }

        [TestMethod]
        public void Default_IsAssignableToIMethodScheduler_Test()
        {
            Assert.IsInstanceOfType(MethodSchedulerService.Default, typeof(IMethodScheduler));
        }

        [TestMethod]
        public async Task Default_CanScheduleAndRunWork_Test()
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            IScheduledJob? job = null;

            try
            {
                job = MethodSchedulerService.Default.Schedule(
                    new ScheduledJobOptions
                    {
                        SuccessInterval = TimeSpan.FromMilliseconds(50),
                        StopOnFirstSuccess = true
                    },
                    _ =>
                    {
                        tcs.TrySetResult(true);

                        return Task.CompletedTask;
                    });

                var fired = await Task.WhenAny(tcs.Task, Task.Delay(3000)) == tcs.Task;
                Assert.IsTrue(fired, "Work was not invoked on Default instance.");
            }
            finally
            {
                if (job != null)
                {
                    await job.StopAsync();
                }
            }
        }

        [TestMethod]
        public async Task Default_TryStopAsync_StopsJobById_Test()
        {
            const string id = "static-default-stop-test";
            IScheduledJob? job = null;

            try
            {
                job = MethodSchedulerService.Default.Schedule(
                    new ScheduledJobOptions
                    {
                        Id = id,
                        SuccessInterval = TimeSpan.FromMilliseconds(50)
                    },
                    _ => Task.CompletedTask);

                var stopped = await MethodSchedulerService.Default.TryStopAsync(id);
                Assert.IsTrue(stopped);
            }
            finally
            {
                if (job != null && job.State == JobState.Running)
                {
                    await job.StopAsync();
                }
            }
        }

        [TestMethod]
        public async Task Default_Schedule_AssignsUniqueIdsForAnonymousJobs_Test()
        {
            IScheduledJob? job1 = null;
            IScheduledJob? job2 = null;

            try
            {
                job1 = MethodSchedulerService.Default.Schedule(
                    new ScheduledJobOptions { SuccessInterval = TimeSpan.FromMilliseconds(50) },
                    _ => Task.CompletedTask);

                job2 = MethodSchedulerService.Default.Schedule(
                    new ScheduledJobOptions { SuccessInterval = TimeSpan.FromMilliseconds(50) },
                    _ => Task.CompletedTask);

                Assert.AreNotEqual(job1.Id, job2.Id);
            }
            finally
            {
                if (job1 != null)
                {
                    await job1.StopAsync();
                }

                if (job2 != null)
                {
                    await job2.StopAsync();
                }
            }
        }
    }
}