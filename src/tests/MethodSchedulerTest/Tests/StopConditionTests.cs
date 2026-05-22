// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 20:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="StopConditionTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class StopConditionTests
    {
        [TestMethod]
        public async Task MaxIterations_StopsJobAfterNIterations_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var count = 0;
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    MaxIterations = 3
                },
                _ =>
                {
                    if (Interlocked.Increment(ref count) >= 3)
                    {
                        done.TrySetResult(true);
                    }

                    return Task.CompletedTask;
                });

            await Task.WhenAny(done.Task, Task.Delay(5000));
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(3, job.IterationCount);
            Assert.AreEqual(JobState.Stopped, job.State);
        }

        [TestMethod]
        public async Task MaxIterations_DoesNotStopEarlier_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var thirdFired = new SemaphoreSlim(0, 1);
            var count = 0;

            scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    MaxIterations = 3
                },
                _ =>
                {
                    if (Interlocked.Increment(ref count) == 3)
                    {
                        thirdFired.Release();
                    }

                    return Task.CompletedTask;
                });

            var reached = await thirdFired.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsTrue(reached, "Job stopped before reaching MaxIterations.");
        }

        [TestMethod]
        public async Task StopOnFirstSuccess_StopsAfterFirstSuccessfulIteration_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var count = 0;
            var firstDone = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    StopOnFirstSuccess = true
                },
                _ =>
                {
                    Interlocked.Increment(ref count);
                    firstDone.TrySetResult(true);
                    return Task.CompletedTask;
                });

            await firstDone.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(1, count, "Job should have run exactly once.");
            Assert.AreEqual(JobState.Stopped, job.State);
        }

        [TestMethod]
        public async Task StopOnFirstSuccess_DoesNotStopAfterFailure_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var successFired = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callCount = 0;

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    FailInterval = TimeSpan.FromMilliseconds(30),
                    StopOnFirstSuccess = true
                },
                _ =>
                {
                    var n = Interlocked.Increment(ref callCount);
                    if (n == 1)
                    {
                        throw new InvalidOperationException("first call fails");
                    }

                    successFired.TrySetResult(true);

                    return Task.CompletedTask;
                });

            await successFired.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(JobState.Stopped, job.State);
            Assert.IsTrue(callCount >= 2, "Job should have executed at least twice.");
        }

        [TestMethod]
        public async Task StopOnFailure_StopsJobAndSetsStateFaulted_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var threw = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    StopOnFailure = true
                },
                _ =>
                {
                    threw.TrySetResult(true);
                    throw new InvalidOperationException("deliberate failure");
                });

            await threw.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(JobState.Faulted, job.State);
            Assert.IsNotNull(job.LastError);
            Assert.IsInstanceOfType(job.LastError, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task StopOnFailure_DoesNotStopOnSuccess_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var thirdDone = new SemaphoreSlim(0, 1);
            var count = 0;

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    StopOnFailure = true
                },
                _ =>
                {
                    if (Interlocked.Increment(ref count) == 3)
                    {
                        thirdDone.Release();
                    }

                    return Task.CompletedTask;
                });

            var reached = await thirdDone.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsTrue(reached, "Job should keep running on success with StopOnFailure.");
            Assert.AreEqual(JobState.Running, job.State);
        }

        [TestMethod]
        public async Task ThrowOnFailure_SetsFaultedState_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var threw = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    ThrowOnFailure = true
                },
                _ =>
                {
                    threw.TrySetResult(true);
                    throw new InvalidOperationException("throw on failure");
                });

            await threw.Task.WaitAsync(TimeSpan.FromSeconds(5));
            try
            {
                await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch
            {
            }

            Assert.AreEqual(JobState.Faulted, job.State);
            Assert.IsNotNull(job.LastError);
        }

        [TestMethod]
        public async Task FailureFlag_ResetsPerIteration_SuccessIntervalUsedAfterRecovery_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var callCount = 0;
            var successRecorded =
                new TaskCompletionSource<DateTimeOffset>(TaskCreationOptions.RunContinuationsAsynchronously);
            DateTimeOffset? afterFailStart = null;

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(40),
                    FailInterval = TimeSpan.FromMilliseconds(200),
                    MaxIterations = 3
                },
                ct =>
                {
                    var n = Interlocked.Increment(ref callCount);
                    if (n == 1)
                    {
                        throw new Exception("iteration 1 fails");
                    }

                    if (n == 2)
                    {
                        afterFailStart = DateTimeOffset.UtcNow;
                    }

                    if (n == 3)
                    {
                        successRecorded.TrySetResult(DateTimeOffset.UtcNow);
                    }

                    return Task.CompletedTask;
                });

            await successRecorded.Task.WaitAsync(TimeSpan.FromSeconds(10));

            var gap = successRecorded.Task.Result - afterFailStart!.Value;
            Assert.IsTrue(gap < TimeSpan.FromMilliseconds(150),
                $"Gap was {gap.TotalMilliseconds:F0}ms — failure interval was incorrectly applied after recovery.");
        }

        [TestMethod]
        public async Task IterationCount_IncreasesAfterEachIteration_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var twoIterations = new SemaphoreSlim(0);
            var count = 0;

            var job = scheduler.Schedule(
                new ScheduledJobOptions { SuccessInterval = TimeSpan.FromMilliseconds(30) },
                _ =>
                {
                    if (Interlocked.Increment(ref count) == 2)
                    {
                        twoIterations.Release();
                    }

                    return Task.CompletedTask;
                });

            await twoIterations.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsTrue(job.IterationCount >= 2);
        }

        [TestMethod]
        public async Task LastError_IsNull_WhenNoFailureHasOccurred_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var done = new SemaphoreSlim(0, 1);

            var job = scheduler.Schedule(
                new ScheduledJobOptions { SuccessInterval = TimeSpan.FromMilliseconds(30) },
                _ =>
                {
                    done.Release();

                    return Task.CompletedTask;
                });

            await done.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.IsNull(job.LastError);
        }

        [TestMethod]
        public async Task LastError_IsSet_AfterFailedIteration_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var failed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(30),
                    FailInterval = TimeSpan.FromMilliseconds(30)
                },
                _ =>
                {
                    failed.TrySetResult(true);
                    throw new InvalidOperationException("test error");
                });

            await failed.Task.WaitAsync(TimeSpan.FromSeconds(3));
            await Task.Delay(50);

            Assert.IsNotNull(job.LastError);
            Assert.AreEqual("test error", job.LastError.Message);
        }
    }
}