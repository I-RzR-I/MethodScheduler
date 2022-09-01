> **Note** This repository is developed in .netstandard2.0

The method cron-based Scheduler for .NET Framework or Core. This is a simple and lightweight version of more complex available schedulers.

In this library is available to execute multiple tasks and simple methods with a few settings, like success interval, failure execute interval, and the possibility to disable on failure.

There are used `System.Threading.Timer` timer, and the possibility of invoking cron are called static class instance or through DI(where is possible).
For using a scheduler are available 2 methods: `Start` and `Stop`.

**In case you wish to use it in your project, u can install the package from <a href="https://www.nuget.org/packages/MethodScheduler" target="_blank">nuget.org</a>** or specify what version you want:

> `Install-Package MethodScheduler -Version x.x.x.x`

[![NuGet Version](https://img.shields.io/nuget/v/MethodScheduler.svg?style=flat)](https://www.nuget.org/packages/MethodScheduler/)

## Content
1. [USING](docs/usage.md)
1. [CHANGELOG](docs/CHANGELOG.md)
1. [BRANCH-GUIDE](docs/branch-guide.md)