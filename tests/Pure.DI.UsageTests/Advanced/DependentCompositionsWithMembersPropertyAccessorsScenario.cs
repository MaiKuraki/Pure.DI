/*
$v=true
$p=7
$d=Dependent compositions with setup context members and property accessors
$h=This scenario shows how to copy referenced members and implement custom property accessors via partial methods.
$h=When this occurs: you need base setup properties with logic, but the dependent composition must remain parameterless.
$h=What it solves: keeps Unity-friendly composition while letting the user implement property logic.
$h=How it is solved in the example: uses DependsOn(..., SetupContextKind.Members) and implements partial get_ methods.
$f=
$f=What it shows:
$f=- Custom property logic via partial accessor methods.
$f=
$f=Important points:
$f=- Accessor logic is not copied; the user provides partial implementations.
$f=
$f=Useful when:
$f=- Properties include custom logic and are referenced by bindings in a dependent setup.
$f=
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantAssignment
// ReSharper disable ArrangeTypeModifiers

// ReSharper disable PartialTypeWithSinglePart
#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.DependentCompositionsWithMembersPropertyAccessorsScenario;

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
        var composition = new Composition();
        composition.SetCounter(3);

        var service = composition.Service;
        // }
        service.Value.ShouldBe(4);
        composition.SaveClassDiagram();
    }
}

// {
interface IService
{
    int Value { get; }
}

class Service(int value) : IService
{
    public int Value { get; } = value;
}

internal partial class BaseComposition
{
    private int _counter;

    internal int Counter
    {
        get => _counter;
        set => _counter = value + 1;
    }

    private void Setup()
    {
        DI.Setup(nameof(BaseComposition), Internal)
            .Bind<int>().To(_ => Counter);
    }
}

internal partial class Composition
{
    private int _counter;

    private partial int get__Counter() => ++_counter;

    public void SetCounter(int counter) => _counter = counter;

    private void Setup()
    {
        DI.Setup(nameof(Composition))
            .DependsOn(nameof(BaseComposition), SetupContextKind.Members)
            .Bind<IService>().To<Service>()
            .Root<IService>("Service");
    }
}
// }
