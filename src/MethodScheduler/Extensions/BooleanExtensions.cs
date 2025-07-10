// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2025-07-10 15:47
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-07-10 19:37
// ***********************************************************************
//  <copyright file="BooleanExtensions.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Linq;
using DomainCommonExtensions.DataTypeExtensions;

#endregion

namespace MethodScheduler.Extensions
{
    /// <summary>
    ///     A boolean extensions.
    /// </summary>
    internal static class BooleanExtensions
    {
        /// <summary>
        ///     All true.
        /// </summary>
        /// <param name="sourceBooleans">
        ///     A variable-length parameters list containing source booleans.
        /// </param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        internal static bool AllTrue(params bool[] sourceBooleans)
        {
            return sourceBooleans.All(x => x.IsTrue());
        }
    }
}