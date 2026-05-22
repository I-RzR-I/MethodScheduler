// ***********************************************************************
//  Assembly          : RzR.Services.MethodSchedulerTest
//  Author            : RzR
//  Created           : 10-07-2025 21:07
// 
//  Last Modified By : RzR
//  Last Modified On : 12-05-2026 21:32
//  ***********************************************************************
//  <copyright file="BooleanTests.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RzR.Scheduling.RecurringJobs.Extensions;

#endregion

namespace MethodSchedulerTest.Tests
{
    [TestClass]
    public class BooleanTests
    {
        [TestMethod]
        public void AllTrue_One_Param_ShouldBe_True_Test()
        {
            var result = BooleanExtensions.AllTrue(true);

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void AllTrue_One_Param_ShouldBe_False_Test()
        {
            var result = BooleanExtensions.AllTrue(false);

            Assert.IsNotNull(result);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void AllTrue_ShouldBe_True_Test()
        {
            var result = BooleanExtensions.AllTrue(true, true, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void AllTrue_ShouldBe_False_Test()
        {
            var result = BooleanExtensions.AllTrue(true, true, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(false, result);
        }
    }
}