// ***********************************************************************
//  Assembly         : RzR.Services.MethodScheduler
//  Author           : RzR
//  Created On       : 2022-08-22 23:12
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-09-01 17:55
// ***********************************************************************
//  <copyright file="GeneralAssemblyInfo.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Reflection;
using System.Resources;

#endregion

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("RzR ®")]
[assembly: AssemblyProduct("Method scheduler")]
[assembly: AssemblyCopyright("Copyright © 2022-2025 RzR All rights reserved.")]
[assembly: AssemblyTrademark("® RzR™")]
[assembly:
    AssemblyDescription(
        "The primary purpose of this repository is to provide a simple and effective scheduler for one or multiple methods(functions, business logic blocks, etc) using `System.Threading.Timer` with some timer settings.")]

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET
[assembly: AssemblyMetadata("TermsOfService", "")]

[assembly: AssemblyMetadata("ContactUrl", "")]
[assembly: AssemblyMetadata("ContactName", "RzR")]
[assembly: AssemblyMetadata("ContactEmail", "ddpRzR@hotmail.com")]
#endif

#if NETSTANDARD2_0_OR_GREATER || NET
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]
#endif

[assembly: AssemblyVersion("1.0.6.7128")]
[assembly: AssemblyFileVersion("1.0.6.7128")]
[assembly: AssemblyInformationalVersion("1.0.6.7128")]
