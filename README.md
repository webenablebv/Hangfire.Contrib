# Hangfire.Contrib
Useful and opinionated set of ASP.NET Core integration extensions for Hangfire.

| Windows | Linux |
| --- | --- |
| [![Windows Build status](https://ci.appveyor.com/api/projects/status/49lhxdj0nuu5x9fe?svg=true)](https://ci.appveyor.com/project/henkmollema/hangfire-contrib) | [![Linux Build Status](https://travis-ci.org/webenablebv/Hangfire.Contrib.svg?branch=master)](https://travis-ci.org/webenablebv/Hangfire.Contrib) |

## Introduction
Hangfire.Contrib provides a set of useful extensions to provide a nice Hangfire integration experience in ASP.NET Core 2.1, 2.2 and 3.0.

### Key features
- Automatic scheduling of jobs
- [ASP.NET Core Logging](https://github.com/aspnet/Logging) integration (`ILogger<T>`) with [Hangfire.Console](https://github.com/pieceofsummer/Hangfire.Console)
- Easy IP-based dashboard authorization filter or custom authorization callback

## Installation
Hangfire.Contrib is [available on NuGet](https://www.nuget.org/packages/Webenable.Hangfire.Contrib):

```
Webenable.Hangfire.Contrib
```

## Usage

### Configuration

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHangfireContrib(c => 
        {
            // Configure storage and any other Hangfire settings
            c.UseStorage(...);
        });

        services.PostConfigure<HangfireContribOptions>(o =>
        {
            // Override default Hangfire.Contrib options here
        });
    }
}
```

By default the Hangfire server and dashboard are enabled. The dashboard is configured without a custom dashboard authorization filter, which means only local requests have access.

### Creating jobs

```cs
using Hangfire;
using Webenable.Hangfire.Contrib;

public class MyJob : HangfireJob
{
    public MyJob(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    // Automatically schedule the job at application startup
    public override string Schedule => Cron.Daily();

    protected override async Task ExecuteCoreAsync(CancellationToken cancellationToken)
    {
        // Execute your job here
    }
}
```

## Features
Hangfire.Contrib provides several features which are described below.

### Automatic job scheduling

Override the `Schedule` property of `HangfireJob` to specify the automatic schedule. Leave `null` or don't override if you don't want to schedule it automatically. An instance of the job is created when determining to automatically schedule the job, so you can use services from dependency injection to determine the job schedule. For example:

```cs
public class MyJob : HangfireJob
{
    private readonly IFeatureManager _featureManager;

    public MyJob(IFeatureManager featureManager, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _featureManager = featureManager;
    }

    public override string Schedule => _featureManager.IsEnabled("MyJob") ? Cron.Daily() : null;

    // ...
}
```

By default jobs are discovered in the entry assembly of the application. If your jobs are specified in another assembly you can configure them using `HangfireContribOptions`:

```cs
services.PostConfigure<HangfireContribOptions>(o =>
{
    o.ScanningAssemblies = new[] { Assembly.GetEntryAssembly(), typeof(Foo).Assembly,, typeof(Bar).Assembly };
});
```

#### Attribute based schedule
You can also use the `[AutoSchedule]` attribute to enabled automatic scheduling without using the `HangfireJob` base class:

```cs
[AutoSchedule(nameof(Run), Crons.Hourly)]
public class AnotherJob
{
    public async Task Run()
    {
        // ...
    }
}
```

> Note that this uses a small utility class `Crons` to provide constant [CRON-expressions](https://en.wikipedia.org/wiki/Cron) which can be used in attributes.

### Logging integration
Hangfire.Contrib integrates the excellent [Hangfire.Console](https://github.com/pieceofsummer/Hangfire.Console) library with the default `ILogger` abstraction of ASP.NET Core. You can use the `Logger` property on `HangfireJob` to log your stuff like you always do and it will appear in the console:

```cs
protected override async Task ExecuteCoreAsync(CancellationToken cancellationToken)
{
    Logger.LogInformation("The job is being executed");

    // ...
}
```

When passing an exception to the logger the stack trace will be printed in the console, after being 'demystified' by [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier).

### Dashboard authorization
By default the dashboard is only accessible from local requests. Hangfire.Contrib offers an IP-based dashboard authorization filter out of the box. 
You can also provide your own authorization logic.  Use the `HangfireContribOptions` class to configure the authorization policy.

#### Configuring IP addresses
```cs
services.PostConfigure<HangfireContribOptions>(o =>
{
    o.Dasbhoard.EnableAuthorization = true;
    o.Dasbhoard.AllowedIps = new[]
    {
        "1.1.1.1",
        "2.2.2.2",
        "3.3.3.3"
    };
});
```

> Please note that IP-based authorization is not fully secure as an IP-address could be spoofed. If you need more security, implement a custom callback.

#### Custom authorization callback

```cs
services.PostConfigure<HangfireContribOptions>(o =>
{
    o.Dasbhoard.EnableAuthorization = true;
    o.Dasbhoard.AuthorizationCallback = httpContext =>
    {
        // Implement your logic
        return true;
    };
});
```

The custom callback will take precedence over the IP-based authorization.
