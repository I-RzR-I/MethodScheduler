// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 22:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="DefectFixesTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using MethodSchedulerTest.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Scheduling.RecurringJobs;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class DefectFixesTests
    {
        private static ScheduledJobOptions Quick(string? id = null)
        {
            return TestOptionsFactory.Quick(id);
        }

        [TestMethod]
        public async Task StopAllAsync_AllowsScheduleAfterwards_Test()
        {
            using var scheduler = new MethodSchedulerService();

            var fired1 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            scheduler.Schedule(Quick("first"), _ =>
            {
                fired1.TrySetResult(true);

                return Task.CompletedTask;
            });
            await fired1.Task.WaitAsync(TimeSpan.FromSeconds(3));
            await scheduler.StopAllAsync();

            var fired2 = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            scheduler.Schedule(Quick("second"), _ =>
            {
                fired2.TrySetResult(true);

                return Task.CompletedTask;
            });

            var ran = await Task.WhenAny(fired2.Task, Task.Delay(3000)) == fired2.Task;
            Assert.IsTrue(ran, "Job scheduled after StopAllAsync did not run — scheduler was zombified.");
        }

        [TestMethod]
        public async Task StopAllAsync_DoesNotPropagatePreviousCancellation_Test()
        {
            using var scheduler = new MethodSchedulerService();
            scheduler.Schedule(Quick("a"), _ => Task.CompletedTask);
            await scheduler.StopAllAsync();

            CancellationToken cancellationToken = default;
            var captured = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            scheduler.Schedule(Quick("b"), ct =>
            {
                cancellationToken = ct;
                captured.TrySetResult(true);

                return Task.CompletedTask;
            });

            await captured.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.IsFalse(cancellationToken.IsCancellationRequested,
                "Job after StopAllAsync received a pre-cancelled token.");
        }

        [TestMethod]
        public void DuplicateId_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();
            scheduler.Schedule(Quick("dup"), ct => Task.Delay(500, ct));

            Assert.ThrowsException<InvalidOperationException>(() =>
                scheduler.Schedule(Quick("dup"), _ => Task.CompletedTask));
        }

        [TestMethod]
        public async Task DuplicateId_AfterStop_IsAllowed_Test()
        {
            using var scheduler = new MethodSchedulerService();
            scheduler.Schedule(Quick("reuse"), _ => Task.CompletedTask);
            var stopped = await scheduler.TryStopAsync("reuse");
            Assert.IsTrue(stopped);

            // Re-scheduling the same id after explicit stop must succeed.
            var job = scheduler.Schedule(Quick("reuse"), _ => Task.CompletedTask);
            Assert.AreEqual("reuse", job.Id);
        }

        [TestMethod]
        public async Task ThrowOnFailure_SurfacesViaCompletion_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = Quick();
            opts.ThrowOnFailure = true;

            var job = scheduler.Schedule(opts, _ => throw new InvalidOperationException("boom"));

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                job.Completion.WaitAsync(TimeSpan.FromSeconds(3)));
            Assert.AreEqual("boom", ex.Message);
            Assert.AreEqual(JobState.Faulted, job.State);
        }

        [TestMethod]
        public async Task StopAsync_DoesNotThrowEvenWithThrowOnFailure_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = Quick();
            opts.ThrowOnFailure = true;

            var threw = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var job = scheduler.Schedule(opts, _ =>
            {
                threw.TrySetResult(true);
                throw new InvalidOperationException("boom");
            });


            await threw.Task.WaitAsync(TimeSpan.FromSeconds(3));

            var deadline = DateTime.UtcNow.AddSeconds(2);
            while (DateTime.UtcNow < deadline && job.State != JobState.Faulted)
            {
                await Task.Delay(10);
            }

            await job.StopAsync();
            Assert.AreEqual(JobState.Faulted, job.State);
        }

        [TestMethod]
        public async Task LastError_ClearedOnRecovery_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var calls = 0;

            var job = scheduler.Schedule(Quick(), _ =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new InvalidOperationException("transient");
                }

                return Task.CompletedTask;
            });

            var deadline = DateTime.UtcNow.AddSeconds(3);
            while (DateTime.UtcNow < deadline && (calls < 2 || job.LastError != null))
            {
                await Task.Delay(20);
            }

            Assert.IsTrue(calls >= 2, "Job did not retry after failure.");
            Assert.IsNull(job.LastError, "LastError should be cleared after a successful iteration.");
            Assert.IsNotNull(job.LastSuccessAt);
        }

        [TestMethod]
        public async Task StopOnFirstSuccess_RemovesJobFromRegistry_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = Quick("auto-remove");
            opts.StopOnFirstSuccess = true;

            var job = scheduler.Schedule(opts, _ => Task.CompletedTask);

            await job.Completion.WaitAsync(TimeSpan.FromSeconds(3));

            var deadline = DateTime.UtcNow.AddSeconds(2);
            var removed = false;
            while (DateTime.UtcNow < deadline)
            {
                if (!await scheduler.TryStopAsync("auto-remove"))
                {
                    removed = true;
                    break;
                }

                await Task.Delay(20);
            }

            Assert.IsTrue(removed, "Naturally-completed job leaked in the registry.");
        }

        [TestMethod]
        public async Task MaxIterations_RemovesJobFromRegistry_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = Quick("max-iter");
            opts.MaxIterations = 2;

            var job = scheduler.Schedule(opts, _ => Task.CompletedTask);
            await job.Completion.WaitAsync(TimeSpan.FromSeconds(3));

            Assert.IsFalse(await scheduler.TryStopAsync("max-iter"));
        }

        [TestMethod]
        public void Validation_NonPositiveSuccessInterval_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = new ScheduledJobOptions { SuccessInterval = TimeSpan.Zero };

            Assert.ThrowsException<ArgumentException>(() => scheduler.Schedule(opts, _ => Task.CompletedTask));
        }

        [TestMethod]
        public void Validation_NonPositiveFailInterval_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = new ScheduledJobOptions
            {
                SuccessInterval = TimeSpan.FromMilliseconds(50),
                FailInterval = TimeSpan.Zero
            };

            Assert.ThrowsException<ArgumentException>(() => scheduler.Schedule(opts, _ => Task.CompletedTask));
        }

        [TestMethod]
        public void Validation_NegativeInitialDelay_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = Quick();
            opts.InitialDelay = TimeSpan.FromMilliseconds(-1);

            Assert.ThrowsException<ArgumentException>(() => scheduler.Schedule(opts, _ => Task.CompletedTask));
        }

        [TestMethod]
        public void Validation_ZeroMaxIterations_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();
            var opts = Quick();
            opts.MaxIterations = 0;

            Assert.ThrowsException<ArgumentException>(() => scheduler.Schedule(opts, _ => Task.CompletedTask));
        }

        [TestMethod]
        public void Validation_NullWorkItemInArray_Throws_Test()
        {
            using var scheduler = new MethodSchedulerService();
            Func<CancellationToken, Task> good = _ => Task.CompletedTask;
            Func<CancellationToken, Task>? bad = null;

            Assert.ThrowsException<ArgumentException>(() => scheduler.Schedule(Quick(), good, bad!));
        }

        [TestMethod]
        public void DI_AddMethodScheduler_IsIdempotent_Test()
        {
            var services = new ServiceCollection();
            services.AddMethodScheduler();
            services.AddMethodScheduler();

            using var provider = services.BuildServiceProvider();
            var s1 = provider.GetRequiredService<IMethodScheduler>();
            var s2 = provider.GetRequiredService<IMethodScheduler>();
            Assert.AreSame(s1, s2);

            var count = 0;
            foreach (var d in services)
            {
                if (d.ServiceType == typeof(IMethodScheduler))
                {
                    count++;
                }
            }

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void DI_AddMethodScheduler_NullServices_Throws_Test()
        {
            Assert.ThrowsException<ArgumentNullException>(() => DependencyInjection.AddMethodScheduler(null!));
        }

        [TestMethod]
        public async Task DI_AddScheduledTask_RunsInHost_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestTaskCounter>();
            services.AddScheduledTask<TestScheduledTask>();
            using var provider = services.BuildServiceProvider();

            var counter = provider.GetRequiredService<TestTaskCounter>();
            var hosted = provider.GetRequiredService<IHostedService>();

            using var cts = new CancellationTokenSource();
            await hosted.StartAsync(cts.Token);

            var deadline = DateTime.UtcNow.AddSeconds(3);
            while (DateTime.UtcNow < deadline && counter.Value < 2)
            {
                await Task.Delay(20);
            }

            await hosted.StopAsync(CancellationToken.None);
            Assert.IsTrue(counter.Value >= 2, $"Hosted task ran {counter.Value} time(s).");
        }

        [TestMethod]
        public async Task DI_AddScheduledTask_ConfigureOverridesOptions_Test()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestTaskCounter>();
            services.AddScheduledTask<TestScheduledTask>(o =>
            {
                o.MaxIterations = 1;
                o.SuccessInterval = TimeSpan.FromMilliseconds(20);
            });
            using var provider = services.BuildServiceProvider();

            var counter = provider.GetRequiredService<TestTaskCounter>();
            var hosted = provider.GetRequiredService<IHostedService>();

            await hosted.StartAsync(CancellationToken.None);

            var deadline = DateTime.UtcNow.AddSeconds(2);
            while (DateTime.UtcNow < deadline && counter.Value < 1)
            {
                await Task.Delay(10);
            }

            await hosted.StopAsync(CancellationToken.None);
            Assert.AreEqual(1, counter.Value);
        }
    }
}