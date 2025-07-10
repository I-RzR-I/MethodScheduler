// ***********************************************************************
//  Assembly         : RzR.Services.MethodSchedulerTest
//  Author           : RzR
//  Created On       : 2025-07-10 15:54
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-07-10 19:47
// ***********************************************************************
//  <copyright file="BooleanTests.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using MethodScheduler.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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