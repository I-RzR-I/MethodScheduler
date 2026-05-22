# Usage

This library is now centered around recurring jobs.

If you are coming from older versions, the mental model is simpler than before:

- Register `AddMethodScheduler()` when you want to schedule jobs through DI.
- Call `Schedule(...)` to start a recurring job.
- Keep the returned `IScheduledJob` when you want to inspect or stop that specific job.
- Use `TryStopAsync(...)` or `StopAllAsync(...)` when you want to stop jobs later.

## Namespaces

The package new name is `RzR.Scheduling.RecurringJobs`, and the current runtime namespaces are:

```csharp
using RzR.Scheduling.RecurringJobs;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;
```

## Choose the Right Entry Point

Use `IMethodScheduler` when your application decides at runtime what to schedule.

Use `IScheduledTask` with `AddScheduledTask<TTask>()` when a job should start automatically with the host and run inside a fresh DI scope on every iteration.

Use `MethodSchedulerService.Default` only when DI is not available, for example in a small console app or older .NET Framework integration.

## Register with Dependency Injection

In ASP.NET Core, Worker Services, or any host that uses `IServiceCollection`, register the scheduler once:

```csharp
using RzR.Scheduling.RecurringJobs;

public void ConfigureServices(IServiceCollection services)
{
    services.AddMethodScheduler();
}
```

For minimal hosting:

```csharp
using RzR.Scheduling.RecurringJobs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMethodScheduler();
```

## Schedule a Single Recurring Job

This is the most direct way to run a recurring piece of work.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;

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
                InitialDelay = TimeSpan.FromSeconds(10),
                StopOnFailure = false,
                ThrowOnFailure = false
            },
            SyncReportsAsync);
    }

    private static Task SyncReportsAsync(CancellationToken cancellationToken)
    {
        // Your recurring work goes here.
        return Task.CompletedTask;
    }
}
```

If you do not set `Id`, the scheduler generates one automatically.

## Schedule Multiple Work Items in One Job

You can run several work items on the same schedule. The `ParallelMode` option controls whether they run sequentially or in parallel.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Enums;
using RzR.Scheduling.RecurringJobs.Models;

public sealed class MaintenanceService
{
    private readonly IMethodScheduler _scheduler;

    public MaintenanceService(IMethodScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public IScheduledJob Start()
    {
        return _scheduler.Schedule(
            new ScheduledJobOptions
            {
                Id = "maintenance.batch",
                SuccessInterval = TimeSpan.FromMinutes(10),
                FailInterval = TimeSpan.FromMinutes(1),
                ParallelMode = ParallelExecutionMode.WhenAll,
                MaxDegreeOfParallelism = 2
            },
            CleanupTempFilesAsync,
            PublishMetricsAsync,
            WarmCacheAsync);
    }

    private static Task CleanupTempFilesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Task PublishMetricsAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Task WarmCacheAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

Available parallel modes:

- `Sequential` runs items one by one in the order you pass them.
- `WhenAll` runs items in parallel and fails the iteration if any item fails.
- `WhenAllIgnoreErrors` runs items in parallel and ignores individual work item errors.

## Stop a Job

You have three common ways to stop work:

```csharp
// Stop a specific job by id.
await scheduler.TryStopAsync("reports.sync");

// Stop the job through the handle returned by Schedule(...).
await job.StopAsync();

// Stop everything tracked by this scheduler instance.
await scheduler.StopAllAsync();
```

Use `TryStopAsync(...)` when you know the job id.

Use `job.StopAsync()` when you already have the returned `IScheduledJob`.

Use `StopAllAsync()` during application shutdown or when you want to clear all active jobs at once.

## Inspect Job State

The returned `IScheduledJob` gives you runtime information about the job:

- `Id` is the stable job identifier.
- `State` shows whether the job is `Pending`, `Running`, `Stopped`, or `Faulted`.
- `IterationCount` tells you how many iterations have finished.
- `LastRunAt`, `NextRunAt`, and `LastSuccessAt` help with monitoring.
- `LastError` keeps the last failure.
- `Completion` can be awaited if you need to observe the final outcome.

## Use a Class-Based Scheduled Task

If you prefer a task class instead of calling `Schedule(...)` manually, implement `IScheduledTask`.

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Models;

public sealed class CleanupTask : IScheduledTask
{
    public ScheduledJobOptions Options { get; } = new()
    {
        Id = "cleanup.task",
        SuccessInterval = TimeSpan.FromMinutes(30),
        FailInterval = TimeSpan.FromMinutes(5),
        StopOnFailure = false
    };

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

Register it like this:

```csharp
using System;
using RzR.Scheduling.RecurringJobs;

builder.Services.AddScheduledTask<CleanupTask>();

// Or override only the options you want to change.
builder.Services.AddScheduledTask<CleanupTask>(options =>
{
    options.SuccessInterval = TimeSpan.FromMinutes(10);
    options.StopOnFirstSuccess = false;
});
```

Each iteration runs inside a fresh DI scope, which makes this option a good fit for tasks that depend on scoped services such as `DbContext`.

## Use Without Dependency Injection

If DI is not available, you can still use the built-in default scheduler instance:

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

var scheduler = MethodSchedulerService.Default;

var job = scheduler.Schedule(
    new ScheduledJobOptions
    {
        Id = "console.job",
        SuccessInterval = TimeSpan.FromSeconds(20)
    },
    cancellationToken => Task.CompletedTask);

Console.ReadLine();

await job.StopAsync();
await scheduler.DisposeAsync();
```

When you use `MethodSchedulerService.Default`, shutdown is your responsibility. Stop or dispose it explicitly before the process exits.

## ScheduledJobOptions in Plain Language

- `Id`: Optional job name. Helpful when you want to stop a job later by id.
- `SuccessInterval`: Delay before the next successful run. Default is 1 minute.
- `FailInterval`: Delay before retrying after a failure. Default is 30 seconds.
- `InitialDelay`: Optional delay before the first run.
- `MaxIterations`: Stops after the specified total number of iterations.
- `StopOnFirstSuccess`: Stops after the first successful iteration.
- `StopOnFailure`: Stops the job after a failed iteration.
- `ThrowOnFailure`: Re-throws failures so the job ends in a faulted state.
- `ParallelMode`: Controls how multiple work items are executed.
- `MaxDegreeOfParallelism`: Limits concurrency when parallel execution is enabled.

## Migration from the Legacy API

Older examples in this repository used `IMultipleScheduler`, `MultipleScheduler`, `RegisterMultipleScheduler()`, and `SchedulerSettings`.

Those APIs are now obsolete. Use this mapping when updating existing code:

| Legacy API | Current API |
| --- | --- |
| `RegisterMultipleScheduler()` | `AddMethodScheduler()` |
| `IMultipleScheduler` | `IMethodScheduler` |
| `MultipleScheduler` | `MethodSchedulerService` |
| `Start(...)` | `Schedule(...)` |
| `Stop()` | `StopAsync(...)`, `TryStopAsync(...)`, or `StopAllAsync(...)` |
| `SchedulerSettings` | `ScheduledJobOptions` |
| `DisableOnFailure` | `StopOnFailure` |
| `ThrowException` | `ThrowOnFailure` |
| `StopAfterXIteration` | `MaxIterations` |
| `ForceStopAfterFirstSuccessExecution` | `StopOnFirstSuccess` |

If you are starting fresh, prefer the modern API everywhere and treat the legacy types as compatibility shims only.
