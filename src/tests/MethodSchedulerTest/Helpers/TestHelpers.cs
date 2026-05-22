// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 23:05
// 
//  Last Modified By : RzR
//  Last Modified On : 21-05-2026 22:34
//  ***********************************************************************
//  <copyright file="TestHelpers.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;

#endregion

namespace MethodSchedulerTest.Helpers
{
    internal sealed class TestTaskCounter
    {
        private int _value;

        public int Value => Volatile.Read(ref _value);

        public void Increment()
        {
            Interlocked.Increment(ref _value);
        }
    }

    internal sealed class TestScheduledTask : IScheduledTask
    {
        private readonly TestTaskCounter _counter;

        public TestScheduledTask(TestTaskCounter counter)
        {
            _counter = counter;
        }

        /// <inheritdoc />
        public ScheduledJobOptions Options { get; } = new()
        {
            SuccessInterval = TimeSpan.FromMilliseconds(50),
            FailInterval = TimeSpan.FromMilliseconds(50)
        };

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _counter.Increment();

            return Task.CompletedTask;
        }
    }

    internal static class InterlockedHelper
    {
        internal static void Max(ref int location, int value)
        {
            int current;
            do
            {
                current = location;
                if (current >= value)
                {
                    return;
                }
            } while (Interlocked.CompareExchange(ref location, value, current) != current);
        }
    }
}