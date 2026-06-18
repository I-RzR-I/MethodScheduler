### **v3.1.0.8026** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 18-06-2026
* [7058597] (RzR) -> Auto commit uncommited files
* [4cf4203] (RzR) -> Upgrade reference package version and fix legacy scheduler bugs

### **v3.0.0.7953** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 22-05-2026
* [DEV] - (RzR) -> Move runtime namespaces and package identity to RzR.Scheduling.RecurringJobs.
* [DEV] - (RzR) -> Replace the `IMultipleScheduler` Start/Stop model with `IMethodScheduler`, `Schedule`, and async stop operations.
* [DEV] - (RzR) -> Introduce `ScheduledJobOptions`, `IScheduledJob`, hosted scheduled task registration, and improved default scheduler behavior.
* [DEV] - (RzR) -> Refresh the main project documentation to reflect the current RzR.Scheduling.RecurringJobs package and namespace.
* [DEV] - (RzR) -> Rewrite the usage guide in a clearer, more humanized style around the current job-based API, including DI registration, scheduling, stopping jobs, hosted tasks, and non-DI usage.
* [DEV] - (RzR) -> Add a dedicated migration guide for moving from v2.x to 3.x, covering renamed namespaces, updated types and methods, TimeSpan-based options, and the new stop model.
* [DEV] - (RzR) -> Clarify the relationship between the legacy MethodScheduler package naming and the current runtime API so upgrades are easier to follow.
* [DEV] - (RzR) -> Keep legacy MultipleScheduler and related v2 APIs as obsolete compatibility shims.

### **v2.1.0.8378** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 15-07-2025
* [a0aefb6] (RzR) -> Auto commit uncommited files
* [c4a5f96] (RzR) -> Add the `forceStopAfterFirstSuccessExecution` param to `MultipleScheduler` implementation.
* [be9c062] (RzR) -> Add `ForceStopAfterFirstSuccessExecution` parameter and integrate the timer stop when execution was successful.

### v**2.0.0.7854** [[RzR](mailto:108324929+I-RzR-I@users.noreply.github.com)] 10-07-2025
* [5431bf0] (RzR) -> Auto commit uncommited files
* [0f62164] (RzR) -> Add new version genetate scripts.
* [15105b3] (RzR) -> Adjust using docs.
* [2936df2] (RzR) -> Add new methods for executing code changes.
* [40dc74f] (RzR) -> Add the `stopAfterXIteration` param to `MultipleScheduler` implementation.
* [19857f3] (RzR) -> Add `StopAfterXIteration` param, integrate timer stop.
* [3fe69da] (RzR) -> Add bool extension method `AllTrue` and tests.
* [a7fbef0] (RzR) -> Upgrade the `DomainCommonExtensions` package version.

### **v1.1.0.0** 
-> Upgrade reference package version. <br/>
-> Remove vulnerable package 'System.Linq.Dynamic.Core' and upgrade version in test project. Fix CVE-2024-51417. <br/>

### **v1.0.6.7128** 
-> Update lib extension version. <br/>

### **v1.0.5.8002** 
-> Add `Start` method with `Action`.

### **v1.0.4.8097** 
-> Update lib extension version. <br/>
-> Fix warning package. <br/>

### **v1.0.3.2317** 
-> Update lib extension version.<br />

### **v1.0.2.057** 
-> Update lib extension version. <br/>

### **v1.0.1.2212** 
-> Update lib extension version. <br/>
-> Cleanup code and review code. <br/>
