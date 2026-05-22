# Migration Guide: v2.x to 3.x

This guide is for projects that already use the v2 scheduler API and want to move to 3.x without guessing what changed.

The short version is this: v3 shifts the public API from a global start/stop model to a job-based model. You still schedule recurring work, but now the scheduler returns a job handle, options are more explicit, and the API fits better with dependency injection and hosted applications.

The good news is that the old types still exist as obsolete compatibility shims. That means you can migrate in steps instead of rewriting everything in one pass.

## What Changed at a Glance

These are the changes most projects will feel immediately:

- The runtime namespaces moved to `RzR.Scheduling.RecurringJobs.*`.
- `RegisterMultipleScheduler()` became `AddMethodScheduler()`.
- `IMultipleScheduler` became `IMethodScheduler`.
- `MultipleScheduler` was replaced by `MethodSchedulerService`.
- `Start(...)` became `Schedule(...)`.
- `Stop()` became `StopAsync(...)`, `TryStopAsync(...)`, or `StopAllAsync(...)`.
- `SchedulerSettings` became `ScheduledJobOptions`.
- Time-based settings moved from numeric minute values to `TimeSpan` values.
- Job lifetime controls moved from method parameters into the options object.
- The canonical work delegate is now `Func<CancellationToken, Task>`.

## What Did Not Get Harder

Even though the API shape changed, the core usage is still familiar:

- You can still register the scheduler in DI.
- You can still schedule one delegate or several delegates.
- You can still run the scheduler without DI.
- You can still keep old code compiling for a while because the v2 types are marked obsolete, not removed.

## Old to New Mapping

Use this table as the quickest reference during the upgrade.

| v2.* | 3.* | Notes |
| --- | --- | --- |
| `MethodScheduler.*` | `RzR.Scheduling.RecurringJobs.*` | Update your `using` statements. |
| `RegisterMultipleScheduler()` | `AddMethodScheduler()` | New DI registration entry point. |
| `IMultipleScheduler` | `IMethodScheduler` | Main scheduler abstraction. |
| `MultipleScheduler` | `MethodSchedulerService` | Use `MethodSchedulerService.Default` outside DI. |
| `Start(...)` | `Schedule(...)` | Scheduling now returns an `IScheduledJob`. |
| `Stop()` | `await job.StopAsync()` | Best when you already have the returned job handle. |
| `Stop()` | `await scheduler.TryStopAsync(id)` | Best when you stop by job id. |
| `Stop()` | `await scheduler.StopAllAsync()` | Best when you want to stop everything. |
| `SchedulerSettings` | `ScheduledJobOptions` | The new options type is richer and more explicit. |
| `DisableOnFailure` | `StopOnFailure` | Same intention, clearer name. |
| `ThrowException` | `ThrowOnFailure` | Same intention, clearer name. |
| `stopAfterXIteration` | `MaxIterations` | Moved into the options object. |
| `forceStopAfterFirstSuccessExecution` | `StopOnFirstSuccess` | Moved into the options object. |

## The Biggest Behavioral Differences

### 1. Scheduling now returns a job handle

In v2, you mainly thought in terms of starting or stopping a scheduler.

In v3, each call to `Schedule(...)` gives you an `IScheduledJob`. That handle lets you inspect state, track errors, and stop that specific job later.

This is the biggest mental shift in the upgrade.

### 2. Intervals are now `TimeSpan`

In v2, `SchedulerSettings.SuccessInterval` and `SchedulerSettings.FailInterval` were numeric minute values.

In v3, `ScheduledJobOptions.SuccessInterval` and `ScheduledJobOptions.FailInterval` are `TimeSpan` values.

That means this old code:

```csharp
new SchedulerSettings
{
    SuccessInterval = 1,
    FailInterval = 0.5
}
```

becomes this in 3.*:

```csharp
new ScheduledJobOptions
{
    SuccessInterval = TimeSpan.FromMinutes(1),
    FailInterval = TimeSpan.FromSeconds(30)
}
```

### 3. The preferred work delegate now accepts a `CancellationToken`

The canonical v3 delegate shape is:

```csharp
Func<CancellationToken, Task>
```

This is better for real applications because jobs can react cleanly when the host shuts down or when a specific job is stopped.

If you want the fastest migration path, you can still keep `Action` and `Func<Task>` delegates by importing the extension namespace:

```csharp
using RzR.Scheduling.RecurringJobs.Extensions;
```

That lets you move first and clean up delegate signatures later.

### 4. Stopping is now explicit and async

The v2 API exposed a simple `Stop()` call.

In v3, stopping is more precise:

- `await job.StopAsync()` stops one known job.
- `await scheduler.TryStopAsync("job-id")` stops one job by id.
- `await scheduler.StopAllAsync()` stops everything tracked by that scheduler instance.

This means your shutdown logic usually needs `await`, which is the right trade-off because the scheduler now waits for the current iteration to finish instead of cutting work off blindly.

## Before and After: DI-Based Migration

This is the most common upgrade path.

### DI v2.x example

```csharp
using MethodScheduler.Abstractions;
using MethodScheduler.Models;

public void ConfigureServices(IServiceCollection services)
{
    services.RegisterMultipleScheduler();
}

public sealed class Foo
{
    private readonly IMultipleScheduler _scheduler;

    public Foo(IMultipleScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public void Run()
    {
        _scheduler.Start(
            WriteTestLog.InitAsync,
            new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5,
                ThrowException = false
            },
            stopAfterXIteration: 5,
            forceStopAfterFirstSuccessExecution: false);
    }
}
```

### DI 3.x example

```csharp
using System;
using Microsoft.Extensions.DependencyInjection;
using RzR.Scheduling.RecurringJobs;
using RzR.Scheduling.RecurringJobs.Abstractions;
using RzR.Scheduling.RecurringJobs.Extensions;
using RzR.Scheduling.RecurringJobs.Models;

public void ConfigureServices(IServiceCollection services)
{
    services.AddMethodScheduler();
}

public sealed class Foo
{
    private readonly IMethodScheduler _scheduler;

    public Foo(IMethodScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public IScheduledJob Run()
    {
        return _scheduler.Schedule(
            new ScheduledJobOptions
            {
                Id = "write-test-log",
                SuccessInterval = TimeSpan.FromMinutes(1),
                FailInterval = TimeSpan.FromSeconds(30),
                StopOnFailure = true,
                ThrowOnFailure = false,
                MaxIterations = 5,
                StopOnFirstSuccess = false
            },
            WriteTestLog.InitAsync);
    }
}
```

The migration is mostly mechanical:

- change the registration call,
- change the injected abstraction,
- replace `SchedulerSettings` with `ScheduledJobOptions`,
- convert numeric intervals to `TimeSpan`,
- move extra stop flags into the options object,
- and keep the returned job if you need to stop or inspect it later.

## Before and After: No-DI Migration

If your v2 code used the static singleton-style entry point, the equivalent in 3.x is `MethodSchedulerService.Default`.

### No-DI v2.x example

```csharp
MultipleScheduler.Instance.Start(
    WriteTestLog.InitAsync,
    new SchedulerSettings
    {
        DisableOnFailure = false,
        SuccessInterval = 1,
        FailInterval = 0.5
    });

MultipleScheduler.Instance.Stop();
```

### No-DI 3.x example

```csharp
using System;
using System.Threading.Tasks;
using RzR.Scheduling.RecurringJobs.Extensions;
using RzR.Scheduling.RecurringJobs.Helpers;
using RzR.Scheduling.RecurringJobs.Models;

var scheduler = MethodSchedulerService.Default;

var job = scheduler.Schedule(
    new ScheduledJobOptions
    {
        Id = "console-job",
        SuccessInterval = TimeSpan.FromMinutes(1),
        FailInterval = TimeSpan.FromSeconds(30)
    },
    WriteTestLog.InitAsync);

await job.StopAsync();
await scheduler.DisposeAsync();
```

The important difference here is ownership. When you use `MethodSchedulerService.Default`, shutdown is your responsibility. In hosted apps, DI handles that for you. In console or framework-style apps, you should stop or dispose the default instance explicitly.

## Migrating Collections of Delegates

In v2, it was common to pass `IEnumerable<Action>` or `IEnumerable<Func<Task>>` into `Start(...)`.

In v3, the main scheduling methods use `params` arrays. In practice that means one of these patterns:

```csharp
scheduler.Schedule(options, Job1Async, Job2Async, Job3Async);
```

or, if you already have a collection:

```csharp
var tasks = new[] { Job1Async, Job2Async, Job3Async };

scheduler.Schedule(options, tasks);
```

If your old code builds a `List<Func<Task>>`, the migration is usually just a `.ToArray()` away.

## New Options You Can Adopt After the Move

You do not need to use all of these on day one, but they are worth knowing about once you are on 3.*:

- `Id` gives a job a stable name so you can stop it later by id.
- `InitialDelay` delays the first execution.
- `ParallelMode` controls how multiple work items execute in one iteration.
- `MaxDegreeOfParallelism` lets you throttle concurrent work.
- `IScheduledJob` exposes state, iteration count, timestamps, and the last error.
- `IScheduledTask` plus `AddScheduledTask<TTask>()` lets you run class-based recurring tasks through the host.

## Practical Migration Checklist

If you want a low-risk migration, use this order:

1. Update `using` statements to `RzR.Scheduling.RecurringJobs.*`.
2. Replace `RegisterMultipleScheduler()` with `AddMethodScheduler()`.
3. Replace `IMultipleScheduler` with `IMethodScheduler`.
4. Replace `SchedulerSettings` with `ScheduledJobOptions`.
5. Convert interval numbers to `TimeSpan` values.
6. Move `stopAfterXIteration` and `forceStopAfterFirstSuccessExecution` into the options object.
7. Replace `Start(...)` with `Schedule(...)`.
8. Store the returned `IScheduledJob` where you need per-job stop or status control.
9. Replace `Stop()` with `StopAsync(...)`, `TryStopAsync(...)`, or `StopAllAsync()`.
10. Decide whether to keep `Action` and `Func<Task>` temporarily through the extension overloads or move directly to `Func<CancellationToken, Task>`.

## Common Gotchas

- The NuGet package can still be the same library, but your code namespaces need to change.
- `SuccessInterval = 1` no longer means anything in 3.*. It must become `TimeSpan.FromMinutes(1)`.
- If you want to keep scheduling `Action` or `Func<Task>` directly, make sure `RzR.Scheduling.RecurringJobs.Extensions` is imported.
- If you assign a custom `Id`, it must be unique inside that scheduler instance.
- `StopAllAsync()` stops all tracked jobs, but the scheduler remains usable afterward.
- If you use `MethodSchedulerService.Default`, remember to shut it down yourself.

## A Reasonable Migration Strategy

If your codebase is large, do not try to make the migration “perfect” in the first pass.

The usual safe path is:

1. move to the new namespaces and types,
2. keep old delegate shapes temporarily through the extension overloads,
3. convert settings and stop behavior,
4. then later adopt cancellation-token-aware delegates and newer v3 features where they add value.

That gets you onto 3.x quickly without turning the upgrade into a full rewrite.
