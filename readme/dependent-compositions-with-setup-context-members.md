#### Dependent compositions with setup context members

This scenario shows how to copy referenced members from a base setup into the dependent composition.
When this occurs: you want to reuse base setup state without passing a separate context instance.
What it solves: lets dependent compositions access base setup members directly (Unity-friendly, no constructor args).
How it is solved in the example: uses DependsOn(..., SetupContextKind.Members) and sets members on the composition instance.


```c#
var composition = new Composition
{
    Settings = new AppSettings("prod", 3),
    Retries = 4
};
var service = composition.Service;

interface IService
{
    string Report { get; }
}

class Service(IAppSettings settings, [Tag("retries")] int retries) : IService
{
    public string Report { get; } = $"env={settings.Environment}, retries={retries}";
}

internal partial class BaseComposition
{
    internal AppSettings Settings { get; set; } = new("", 0);

    internal int Retries { get; set; }

    internal int GetRetries() => Retries;

    private void Setup()
    {
        DI.Setup(nameof(BaseComposition), Internal)
            .Bind<IAppSettings>().To(_ => Settings)
            .Bind<int>("retries").To(_ => GetRetries());
    }
}

internal partial class Composition
{
    private void Setup()
    {
        DI.Setup(nameof(Composition))
            .DependsOn(nameof(BaseComposition), SetupContextKind.Members)
            .Bind<IService>().To<Service>()
            .Root<IService>("Service");
    }
}

record AppSettings(string Environment, int RetryCount) : IAppSettings;

interface IAppSettings
{
    string Environment { get; }

    int RetryCount { get; }
}
```

<details>
<summary>Running this code sample locally</summary>

- Make sure you have the [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or later installed
```bash
dotnet --list-sdk
```
- Create a net10.0 (or later) console application
```bash
dotnet new console -n Sample
```
- Add a reference to the NuGet package
  - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
```bash
dotnet add package Pure.DI
```
- Copy the example code into the _Program.cs_ file

You are ready to run the example.
```bash
dotnet run
```

</details>

What it shows:
- Setup context members copied into the dependent composition.

Important points:
- The composition remains parameterless and can be configured via its own members.

Useful when:
- Base setup has instance members initialized by the host or framework.
