# USING
In this library are available 2 methods `Start` and `Stop`. 
* `Start` -> used for init schedule some method;
* `Stop` -> used for stopping execution scheduler.

Scheduler has a few available settings:
* `SuccessInterval` -> Success execute interval in minutes
* `FailInterval` -> Fail execute interval, retry in minutes
* `DisableOnFailure` -> Disable on failure
* `ThrowException` -> Throw exeption 

> In case you using .NET | .NET Core

For using this scheduler you need to add a specific piece of code written below.
Add this piece on your `Startup.cs`.
```csharp
public void ConfigureServices(IServiceCollection services)
        {
            ...
            
            services.RegisterMultipleScheduler();
            
            ...
        }
```
A few examples of using:
```csharp
class Foo
{
    private readonly IMultipleScheduler _scheduler;
    public Foo(IMultipleScheduler scheduler)
    {
        _scheduler = scheduler;
    }
    
    Run()
    {
        _scheduler.Start(WriteTestLog.InitAsync, new SchedulerSettings
       {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
        });
        _scheduler.Start(WriteTestLog.Init2Async, new SchedulerSettings
        {
                DisableOnFailure = false,
                SuccessInterval = 0.5,
                FailInterval = 0.3
        });
    }
}
```


> In case you using .NET Framework

For using the a scheduler in .NET Framework you may call an instance of the base class `MultipleScheduler`.

A few examples of using:
```csharp
void Run()
        {
            MultipleScheduler.Instance.Start(WriteTestLog.InitAsync, new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            });
            MultipleScheduler.Instance.Start(
            new List<Func<Task>>
            {
                WriteTestLog.InitTask1Async,
                WriteTestLog.InitTask2Async,
                WriteTestLog.InitTask3Async
            }, 
            new SchedulerSettings
            {
                DisableOnFailure = false,
                SuccessInterval = 0.5,
                FailInterval = 0.3
            });
        }
```

<hr/>

```csharp
void Run()
        {
            var obj1 = new WriteTestLog();
            MultipleScheduler.Instance.Start(new List<Func<Task>>
            {
                obj1.InitTask1Async,
                obj1.InitTask2Async,
                obj1.InitTask3Async
            }, new SchedulerSettings
            {
                DisableOnFailure = true,
                SuccessInterval = 1,
                FailInterval = 0.5
            });
        }
```

<hr/>

```csharp
void Run()
        {
            MultipleScheduler.Instance.Start(new List<Func<Task>>
            {
                WriteTestLog.InitTask1Async,
                WriteTestLog.InitTask2Async,
                WriteTestLog.InitTask3Async
            }, new SchedulerSettings
            {
                DisableOnFailure = false,
                SuccessInterval = 1,
                FailInterval = 0.5,
                ThrowException = true
            });
        }
```
