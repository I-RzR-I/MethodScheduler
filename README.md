> **Note** This repository is developed using .netstandard2.0

# RzR.Scheduling.RecurringJobs

[![NuGet Version](https://img.shields.io/nuget/v/RzR.Scheduling.RecurringJobs.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Scheduling.RecurringJobs/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/RzR.Scheduling.RecurringJobs.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/RzR.Scheduling.RecurringJobs)

<details>

  <summary>Old version</summary>
  
[![NuGet Version](https://img.shields.io/nuget/v/MethodScheduler.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/MethodScheduler/)
[![Nuget Downloads](https://img.shields.io/nuget/dt/MethodScheduler.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/MethodScheduler)

</details>

<br />

`RzR.Scheduling.RecurringJobs` is a lightweight recurring job scheduler for .NET.

It is designed for cases where you want to run one or more pieces of work on a repeating schedule without pulling in a heavier scheduling framework. The library is built around `System.Threading.Timer`, works well with dependency injection, and now exposes a clearer job-based API for starting, observing, and stopping recurring work.

## What It Gives You

- Schedule a single recurring job or several work items in one job.
- Run through DI with `IMethodScheduler` or without DI through `MethodSchedulerService.Default`.
- Stop a job by handle, by id, or stop everything managed by the scheduler.
- Configure retries, first-run delay, max iterations, stop-on-failure, and stop-on-first-success behavior.
- Control parallel execution when one job contains multiple work items.
- Register class-based scheduled tasks through `AddScheduledTask<TTask>()`.

## Package and Namespace Notes

The current package and runtime namespace are:

```csharp
using RzR.Scheduling.RecurringJobs;
```

Older versions of this library were published and documented under `MethodScheduler`.

If you are upgrading from v2.x or earlier:

- the old package name was `MethodScheduler`,
- older examples may still show `MethodScheduler.*` namespaces,
- the current migration path is documented in [docs/migration.md](docs/migration.md).

## Install

Install the current package from NuGet:

```powershell
Install-Package RzR.Scheduling.RecurringJobs
```

If you are maintaining an older codebase still pinned to the legacy package, see [docs/migration.md](docs/migration.md) before upgrading.

## Quick Start

For most applications, the simplest path is:

1. Register the scheduler in DI.
2. Inject `IMethodScheduler`.
3. Call `Schedule(...)` with `ScheduledJobOptions`.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RzR.Scheduling.RecurringJobs;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMethodScheduler();

public sealed class ReportSyncService
{
  private readonly IMethodScheduler _scheduler;

  public ReportSyncService(IMethodScheduler scheduler)
  {
    _scheduler = scheduler;
  }

  public IScheduledJob Start()
  {
    return _scheduler.Schedule(
      new ScheduledJobOptions
      {
        Id = "reports.sync",
        SuccessInterval = TimeSpan.FromMinutes(5),
        FailInterval = TimeSpan.FromSeconds(30),
        InitialDelay = TimeSpan.FromSeconds(10)
      },
      SyncReportsAsync);
  }

  private static Task SyncReportsAsync(CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }
}
```

Once a job is scheduled, you can:

- keep the returned `IScheduledJob` and call `await job.StopAsync()`,
- stop a job later with `await scheduler.TryStopAsync("reports.sync")`,
- or stop everything with `await scheduler.StopAllAsync()`.

## Choose the Right Entry Point

Use `IMethodScheduler` when your application decides at runtime what jobs to create.

Use `IScheduledTask` with `AddScheduledTask<TTask>()` when a recurring task should start automatically with the host and run inside a fresh DI scope on each iteration.

Use `MethodSchedulerService.Default` only when DI is not available, such as a small console tool or an older framework-style integration.

## Documentation

- [docs/usage.md](docs/usage.md) for the full API guide and examples.
- [docs/migration.md](docs/migration.md) for upgrading from v2.x to 3.x.
- [docs/CHANGELOG.md](docs/CHANGELOG.md) for release history.
- [docs/branch-guide.md](docs/branch-guide.md) for branch workflow details.

If you want a small recurring job scheduler for .NET that supports both modern DI-based apps and simpler no-DI scenarios, this library is the current, streamlined version of `MethodScheduler`.
