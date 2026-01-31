/*
$v=true
$p=7
$d=Dependent compositions with setup context property
$h=This scenario shows how to pass an explicit setup context via a property.
$h=When this occurs: Unity (or another host) sets fields/properties on the composition instance.
$h=What it solves: avoids constructor arguments while still allowing dependent setups to access base state.
$h=How it is solved in the example: uses DependsOn(..., SetupContextKind.Property, name) and assigns the context property.
$f=
$f=What it shows:
$f=- Setup context as a property on the composition.
$f=
$f=Important points:
$f=- The composition stays parameterless and Unity-friendly.
$f=
$f=Useful when:
$f=- The composition is created by a framework that injects data via properties.
$f=
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantAssignment
// ReSharper disable ArrangeTypeModifiers

// ReSharper disable PartialTypeWithSinglePart
#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.DependentCompositionsWithPropertyContextScenario;

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
        var baseContext = new BaseComposition { Settings = new AppSettings("dev", 1) };
        var composition = new Composition { baseContext = baseContext };
        var service = composition.Service;
        // }
        service.Report.ShouldBe("env=dev, retries=1");
        composition.SaveClassDiagram();
    }
}

// {
interface IService
{
    string Report { get; }
}

class Service(IAppSettings settings) : IService
{
    public string Report { get; } = $"env={settings.Environment}, retries={settings.RetryCount}";
}

internal partial class BaseComposition
{
    internal AppSettings Settings { get; set; } = new("", 0);

    private void Setup()
    {
        DI.Setup(nameof(BaseComposition), Internal)
            .Bind<IAppSettings>().To(_ => Settings);
    }
}

internal partial class Composition
{
    private void Setup()
    {
        DI.Setup(nameof(Composition))
            .DependsOn(nameof(BaseComposition), SetupContextKind.Property, "baseContext")
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
