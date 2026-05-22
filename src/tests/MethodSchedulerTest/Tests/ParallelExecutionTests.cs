// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 20:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="ParallelExecutionTests.cs" company="RzR SOFT & TECH">
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
using SchedulerParallelMode = RzR.Scheduling.RecurringJobs.Enums.ParallelExecutionMode;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class ParallelExecutionTests
    {
        private static ScheduledJobOptions OneShot(SchedulerParallelMode mode, int? maxDop = null)
        {
            return TestOptionsFactory.OneShot(mode, maxDop);
        }

        [TestMethod]
        public async Task Sequential_RunsItemsInOrder_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var order = new List<int>();
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.Schedule(
                OneShot(SchedulerParallelMode.Sequential),
                ct =>
                {
                    lock (order)
                    {
                        order.Add(1);
                    }

                    return Task.CompletedTask;
                },
                ct =>
                {
                    lock (order)
                    {
                        order.Add(2);
                    }

                    return Task.CompletedTask;
                },
                ct =>
                {
                    lock (order)
                    {
                        order.Add(3);
                    }

                    done.TrySetResult(true);

                    return Task.CompletedTask;
                });

            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, order);
        }

        [TestMethod]
        public async Task Sequential_StopsAtFirstFailure_SubsequentItemsNotRun_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var executed = new List<int>();
            var done = new SemaphoreSlim(0, 1);

            scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(50),
                    FailInterval = TimeSpan.FromMilliseconds(50),
                    MaxIterations = 1,
                    ParallelMode = SchedulerParallelMode.Sequential
                },
                ct =>
                {
                    lock (executed)
                    {
                        executed.Add(1);
                    }

                    throw new InvalidOperationException("item 1 fails");
                },
                ct =>
                {
                    lock (executed)
                    {
                        executed.Add(2);
                    }

                    done.Release();

                    return Task.CompletedTask;
                });

            var leaked = await done.WaitAsync(TimeSpan.FromMilliseconds(200));
            Assert.IsFalse(leaked, "Item 2 should not run when item 1 throws in Sequential mode.");
            Assert.IsFalse(executed.Contains(2), "Item 2 should not run when item 1 throws in Sequential mode.");
        }

        [TestMethod]
        public async Task WhenAll_AllItemsRun_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var ran = new bool[3];
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.Schedule(
                OneShot(SchedulerParallelMode.WhenAll),
                ct =>
                {
                    ran[0] = true;

                    return Task.CompletedTask;
                },
                ct =>
                {
                    ran[1] = true;

                    return Task.CompletedTask;
                },
                ct =>
                {
                    ran[2] = true;
                    done.TrySetResult(true);

                    return Task.CompletedTask;
                });

            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.IsTrue(ran.All(x => x), "All work items should have run in WhenAll mode.");
        }

        [TestMethod]
        public async Task WhenAll_FailureInOneItem_PropagatesAndCountsAsFailedIteration_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var failSeen = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(50),
                    FailInterval = TimeSpan.FromMilliseconds(50),
                    StopOnFailure = true,
                    ParallelMode = SchedulerParallelMode.WhenAll
                },
                ct => Task.FromException(new InvalidOperationException("item 1 fails")),
                ct =>
                {
                    failSeen.TrySetResult(true);

                    return Task.CompletedTask;
                });

            await failSeen.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(JobState.Faulted, job.State);
            Assert.IsNotNull(job.LastError);
        }

        [TestMethod]
        public async Task WhenAllIgnoreErrors_AllItemsRun_EvenIfOneFails_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var ran = new bool[3];
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.Schedule(
                OneShot(SchedulerParallelMode.WhenAllIgnoreErrors),
                ct => Task.FromException(new InvalidOperationException("ignored")),
                ct =>
                {
                    ran[1] = true;

                    return Task.CompletedTask;
                },
                ct =>
                {
                    ran[2] = true;
                    done.TrySetResult(true);
                    return Task.CompletedTask;
                });

            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.IsTrue(ran[1] && ran[2], "Items 2 and 3 should run despite item 1 failing.");
        }

        [TestMethod]
        public async Task WhenAllIgnoreErrors_IterationCountsAsSuccess_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var job = scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(50),
                    StopOnFirstSuccess = true,
                    ParallelMode = SchedulerParallelMode.WhenAllIgnoreErrors
                },
                ct => Task.FromException(new InvalidOperationException("ignored")),
                ct =>
                {
                    done.TrySetResult(true);

                    return Task.CompletedTask;
                });

            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.AreEqual(JobState.Stopped, job.State);
        }

        [TestMethod]
        public void MaxDegreeOfParallelism_Zero_ThrowsOnSchedule_Test()
        {
            using var scheduler = new MethodSchedulerService();

            Assert.ThrowsException<ArgumentException>(() =>
                scheduler.Schedule(
                    new ScheduledJobOptions { MaxDegreeOfParallelism = 0 },
                    ct => Task.CompletedTask,
                    ct => Task.CompletedTask));
        }

        [TestMethod]
        public void MaxDegreeOfParallelism_Negative_ThrowsOnSchedule_Test()
        {
            using var scheduler = new MethodSchedulerService();

            Assert.ThrowsException<ArgumentException>(() =>
                scheduler.Schedule(
                    new ScheduledJobOptions { MaxDegreeOfParallelism = -1 },
                    ct => Task.CompletedTask,
                    ct => Task.CompletedTask));
        }

        [TestMethod]
        public async Task MaxDegreeOfParallelism_LimitsActualConcurrency_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var concurrentPeak = 0;
            var current = 0;
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var allStarted = new SemaphoreSlim(0);

            scheduler.Schedule(
                new ScheduledJobOptions
                {
                    SuccessInterval = TimeSpan.FromMilliseconds(50),
                    StopOnFirstSuccess = true,
                    ParallelMode = SchedulerParallelMode.WhenAll,
                    MaxDegreeOfParallelism = 2
                },
                ct => SlowItem(0, ct),
                ct => SlowItem(1, ct),
                ct => SlowItem(2, ct),
                ct => SlowItem(3, ct),
                ct => SlowItem(4, ct),
                ct => SlowItem(5, ct));

            await done.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.IsTrue(concurrentPeak <= 2, $"Peak concurrency was {concurrentPeak}, expected <= 2.");

            async Task SlowItem(int idx, CancellationToken ct)
            {
                var c = Interlocked.Increment(ref current);
                InterlockedHelper.Max(ref concurrentPeak, c);
                await Task.Delay(20, ct);
                Interlocked.Decrement(ref current);
                if (idx == 5)
                {
                    done.TrySetResult(true);
                }
            }
        }

        [TestMethod]
        public async Task MaxDegreeOfParallelism_GreaterThanItemCount_RunsUnbounded_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var ran = new bool[3];
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            // DOP = 10 with 3 items → effectively unbounded
            scheduler.Schedule(
                OneShot(SchedulerParallelMode.WhenAll, 10),
                ct =>
                {
                    ran[0] = true;

                    return Task.CompletedTask;
                },
                ct =>
                {
                    ran[1] = true;

                    return Task.CompletedTask;
                },
                ct =>
                {
                    ran[2] = true;
                    done.TrySetResult(true);

                    return Task.CompletedTask;
                });

            await done.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsTrue(ran.All(x => x));
        }

        [TestMethod]
        public async Task SingleItem_ParamsOverload_RunsCorrectly_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            scheduler.Schedule(
                OneShot(SchedulerParallelMode.WhenAll),
                ct =>
                {
                    tcs.TrySetResult(true);
                    return Task.CompletedTask;
                });

            var fired = await Task.WhenAny(tcs.Task, Task.Delay(3000)) == tcs.Task;
            Assert.IsTrue(fired);
        }
    }
}