/*
$v=true
$p=7
$d=Dependent compositions with setup context members
$h=This scenario shows how to copy referenced members from a base setup into the dependent composition.
$h=When this occurs: you want to reuse base setup state without passing a separate context instance.
$h=What it solves: lets dependent compositions access base setup members directly (Unity-friendly, no constructor args).
$h=How it is solved in the example: uses DependsOn(..., SetupContextKind.Members) and sets members on the composition instance. The name parameter is optional.
$f=
$f=What it shows:
$f=- Setup context members copied into the dependent composition.
$f=
$f=Important points:
$f=- The composition remains parameterless and can be configured via its own members.
$f=
$f=Useful when:
$f=- Base setup has instance members initialized by the host or framework.
$f=
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantAssignment
// ReSharper disable ArrangeTypeModifiers

// ReSharper disable PartialTypeWithSinglePart
#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.DependentCompositionsWithMembersContextScenario;

using Pure.DI;
using UsageTests;
using Shouldly;
using Xunit;
using static CompositionKind;

public class Scenario
{
    [Fact]
    public void Run()
    {
        // Resolve = Off
        // {
        var composition = new Composition
        {
            Settings = new AppSettings("prod", 3),
            Retries = 4
        };
        var service = composition.Service;
        // }
        service.Report.ShouldBe("env=prod, retries=4");
        composition.SaveClassDiagram();
    }
}

// {
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
// }
