// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 20:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="SchedulerLifecycleTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using MethodSchedulerTest.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class SchedulerLifecycleTests
    {
        private static ScheduledJobOptions QuickOptions(string? id = null)
        {
            return TestOptionsFactory.QuickOptions(id);
        }

        [TestMethod]
        public void Schedule_AssignsAutoId_WhenOptionsIdIsNull_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var job = scheduler.Schedule(QuickOptions(), _ => Task.CompletedTask);

            Assert.IsFalse(string.IsNullOrEmpty(job.Id));
        }

        [TestMethod]
        public void Schedule_UsesProvidedId_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var job = scheduler.Schedule(QuickOptions("custom-id"), _ => Task.CompletedTask);

            Assert.AreEqual("custom-id", job.Id);
        }

        [TestMethod]
        public void Schedule_NullOptions_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();

            Assert.ThrowsException<ArgumentNullException>(() => scheduler.Schedule(null!, _ => Task.CompletedTask));
        }

        [TestMethod]
        public void Schedule_NullWork_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();

            Assert.ThrowsException<ArgumentNullException>(() =>
                scheduler.Schedule(QuickOptions(), (Func<CancellationToken, Task>)null!));
        }

        [TestMethod]
        public async Task Schedule_WorkIsInvoked_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.Schedule(QuickOptions(), _ =>
            {
                tcs.TrySetResult(true);

                return Task.CompletedTask;
            });

            var fired = await Task.WhenAny(tcs.Task, Task.Delay(3000)) == tcs.Task;
            Assert.IsTrue(fired, "Work was not invoked within the timeout.");
        }

        [TestMethod]
        public async Task Schedule_JobTransitionsToRunning_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var started = new SemaphoreSlim(0, 1);

            var job = scheduler.Schedule(QuickOptions(), async ct =>
            {
                started.Release();
                try
                {
                    await Task.Delay(500, ct);
                }
                catch (OperationCanceledException)
                {
                }
            });

            await started.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.AreEqual(JobState.Running, job.State);
        }

        [TestMethod]
        public async Task Schedule_LastRunAt_IsPopulated_AfterFirstIteration_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var done = new SemaphoreSlim(0, 1);

            var job = scheduler.Schedule(QuickOptions(), _ =>
            {
                done.Release();

                return Task.CompletedTask;
            });

            await done.WaitAsync(TimeSpan.FromSeconds(3));
            await Task.Delay(20);
            Assert.IsNotNull(job.LastRunAt);
        }

        [TestMethod]
        public async Task Schedule_WithInitialDelay_DoesNotFireImmediately_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var fired = false;

            scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(50),
                    InitialDelay = TimeSpan.FromSeconds(60) // very long
                },
                _ =>
                {
                    fired = true;

                    return Task.CompletedTask;
                });

            await Task.Delay(200);
            Assert.IsFalse(fired, "Work should not have fired during the initial delay.");
        }

        [TestMethod]
        public async Task Schedule_WithShortInitialDelay_FiresAfterDelay_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(50),
                    InitialDelay = TimeSpan.FromMilliseconds(100)
                },
                _ =>
                {
                    tcs.TrySetResult(true);

                    return Task.CompletedTask;
                });

            var fired = await Task.WhenAny(tcs.Task, Task.Delay(500)) == tcs.Task;
            Assert.IsTrue(fired);
        }

        [TestMethod]
        public async Task StopAsync_TransitionsJobToStopped_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var started = new SemaphoreSlim(0, 1);

            var job = scheduler.Schedule(QuickOptions(), _ =>
            {
                started.Release();

                return Task.CompletedTask;
            });

            await started.WaitAsync(TimeSpan.FromSeconds(3));
            await job.StopAsync();

            Assert.AreEqual(JobState.Stopped, job.State);
        }

        [TestMethod]
        public async Task TryStopAsync_ReturnsTrueForExistingJob_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var job = scheduler.Schedule(QuickOptions("job-a"), _ => Task.CompletedTask);

            var result = await scheduler.TryStopAsync("job-a");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TryStopAsync_ReturnsFalseForUnknownId_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var result = await scheduler.TryStopAsync("does-not-exist");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TryStopAsync_NullId_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => scheduler.TryStopAsync(null!));
        }

        [TestMethod]
        public async Task StopAllAsync_StopsAllRunningJobs_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var job1 = scheduler.Schedule(QuickOptions(), _ => Task.CompletedTask);
            var job2 = scheduler.Schedule(QuickOptions(), _ => Task.CompletedTask);

            await scheduler.StopAllAsync();

            Assert.AreEqual(JobState.Stopped, job1.State);
            Assert.AreEqual(JobState.Stopped, job2.State);
        }

        [TestMethod]
        public async Task DisposeAsync_StopsAllRunningJobs_Test()
        {
            var scheduler = new MethodSchedulerService();

            var job = scheduler.Schedule(QuickOptions(), _ => Task.CompletedTask);
            await scheduler.DisposeAsync();

            Assert.AreEqual(JobState.Stopped, job.State);
        }

        [TestMethod]
        public async Task DisposeAsync_IsIdempotent_Test()
        {
            var scheduler = new MethodSchedulerService();
            await scheduler.DisposeAsync();
            await scheduler.DisposeAsync();
        }

        [TestMethod]
        public void Schedule_AfterDispose_Throws_Test()
        {
            var scheduler = new MethodSchedulerService();
            scheduler.DisposeAsync().AsTask().Wait();

            Assert.ThrowsException<ObjectDisposedException>(() =>
                scheduler.Schedule(QuickOptions(), _ => Task.CompletedTask));
        }
    }
}