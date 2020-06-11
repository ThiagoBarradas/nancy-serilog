[![Build Status](https://barradas.visualstudio.com/Contributions/_apis/build/status/ThiagoBarradas.nancy-serilog?branchName=master)](https://barradas.visualstudio.com/Contributions/_build/latest?definitionId=11&branchName=master)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Nancy.Serilog.Simple.svg)](https://www.nuget.org/packages/Nancy.Serilog.Simple/)
[![NuGet Version](https://img.shields.io/nuget/v/Nancy.Serilog.Simple.svg)](https://www.nuget.org/packages/Nancy.Serilog.Simple/)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=ThiagoBarradas_nancy-serilog&metric=alert_status)](https://sonarcloud.io/dashboard?id=ThiagoBarradas_nancy-serilog)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ThiagoBarradas_nancy-serilog&metric=coverage)](https://sonarcloud.io/dashboard?id=ThiagoBarradas_nancy-serilog)

# Nancy.Serilog.Simple

Serilog logger for Nancy web applications. Handler request, response and exceptions.

# Sample

Configure service in statup
```c#
// Startup.cs

public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
{
    loggerFactory.AddSerilog();
}

```

Resolve dependency and setup pipelines
```c#
// Bootstrapper.cs

protected override void ConfigureApplicationContainer(TinyIoCContainer container)
{
    var jsonSerializerSettings = new JsonSerializerSettings
    {
    	ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    container.Register<JsonSerializerSettings>(jsonSerializerSettings);
    container.Register<ICommunicationLogger, CommunicationLogger>().AsSingleton();
    
    base.ConfigureApplicationContainer(container);
}

protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
{
	// you must setup logger pipeline in application startup
	pipelines.AddLogPipelines(container); 
}

```

Ready! That way all request/response will be sended to serilog.

You can custom information title / error title and Serilog Logger using NancySerilogConfiguration in constructor. By default, global serilog logger will be used.

You can disable logging on success using DisableSerilogExtension in your action:

```c#

public object Home()
{
	this.DisableLogging();
	...
}

```

Additional Property

```
context.Items["NancySerilogAdditionalInfo"] = new AdditionalInfo
{
    Data = new Dictionary<string, object>
    {
        { "SomeProperty", "HERE_SOMEPROPERTY" }
    }
};
```

## Properties 

* `RequestBody`
* `Method`
* `Path`
* `Host`
* `Port`
* `Url`
* `QueryString`
* `Query`
* `RequestHeaders`
* `Ip`
* `IsSuccessful`
* `StatusCode`
* `StatusDescription`
* `StatusCodeFamily`
* `ProtocolVersion`
* `ErrorException`
* `ErrorMessage`
* `ResponseContent`
* `ContentType`
* `ContentLength`
* `ResponseHeaders`
* `ElapsedMilliseconds`
* `RequestKey`

You can use this propeties with serilog log context to build log messages. `HTTP {Method} {Path} {...}`.

## Install via NuGet

```
PM> Install-Package Nancy.Serilog.Simple
```

## How can I contribute?
Please, refer to [CONTRIBUTING](.github/CONTRIBUTING.md)

## Found something strange or need a new feature?
Open a new Issue following our issue template [ISSUE_TEMPLATE](.github/ISSUE_TEMPLATE.md)

## Changelog
See in [nuget version history](https://www.nuget.org/packages/Nancy.Serilog.Simple)

## Did you like it? Please, make a donate :)

if you liked this project, please make a contribution and help to keep this and other initiatives, send me some Satochis.

BTC Wallet: `1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX`

![1G535x1rYdMo9CNdTGK3eG6XJddBHdaqfX](https://i.imgur.com/mN7ueoE.png)
