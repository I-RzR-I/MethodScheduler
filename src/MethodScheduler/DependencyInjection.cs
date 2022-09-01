// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-22 23:13
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="DependencyInjection.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using MethodScheduler.Abstractions;
using MethodScheduler.Helpers;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace MethodScheduler
{
    /// <summary>
    ///     Local DI
    /// </summary>
    /// <remarks></remarks>
    public static class DependencyInjection
    {
        /// <summary>
        ///     Register method scheduler
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static IServiceCollection RegisterMultipleScheduler(this IServiceCollection services)
        {
            services.AddSingleton<IMultipleScheduler, MultipleScheduler>();

            return services;
        }
    }
}