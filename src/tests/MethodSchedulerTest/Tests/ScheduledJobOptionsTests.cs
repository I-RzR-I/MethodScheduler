// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 11-05-2026 20:05
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="ScheduledJobOptionsTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Scheduling.RecurringJobs.Models;
using SchedulerParallelMode = RzR.Scheduling.RecurringJobs.Enums.ParallelExecutionMode;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class ScheduledJobOptionsTests
    {
        [TestMethod]
        public void Defaults_AreCorrect_Test()
        {
            var opts = new ScheduledJobOptions();

            Assert.IsNull(opts.Id);
            Assert.AreEqual(TimeSpan.FromMinutes(1), opts.SuccessInterval);
            Assert.AreEqual(TimeSpan.FromSeconds(30), opts.FailInterval);
            Assert.IsNull(opts.InitialDelay);
            Assert.IsNull(opts.MaxIterations);
            Assert.IsFalse(opts.StopOnFirstSuccess);
            Assert.IsFalse(opts.StopOnFailure);
            Assert.IsFalse(opts.ThrowOnFailure);
            Assert.AreEqual(SchedulerParallelMode.WhenAll, opts.ParallelMode);
            Assert.IsNull(opts.MaxDegreeOfParallelism);
        }

        [TestMethod]
        public void AllProperties_CanBeSet_Test()
        {
            var opts = new ScheduledJobOptions
            {
                Id = "my-job",
                SuccessInterval = TimeSpan.FromMinutes(5),
                FailInterval = TimeSpan.FromSeconds(10),
                InitialDelay = TimeSpan.FromSeconds(3),
                MaxIterations = 10,
                StopOnFirstSuccess = true,
                StopOnFailure = true,
                ThrowOnFailure = true,
                ParallelMode = SchedulerParallelMode.Sequential,
                MaxDegreeOfParallelism = 4
            };

            Assert.AreEqual("my-job", opts.Id);
            Assert.AreEqual(TimeSpan.FromMinutes(5), opts.SuccessInterval);
            Assert.AreEqual(TimeSpan.FromSeconds(10), opts.FailInterval);
            Assert.AreEqual(TimeSpan.FromSeconds(3), opts.InitialDelay);
            Assert.AreEqual(10, opts.MaxIterations);
            Assert.IsTrue(opts.StopOnFirstSuccess);
            Assert.IsTrue(opts.StopOnFailure);
            Assert.IsTrue(opts.ThrowOnFailure);
            Assert.AreEqual(SchedulerParallelMode.Sequential, opts.ParallelMode);
            Assert.AreEqual(4, opts.MaxDegreeOfParallelism);
        }
    }
}