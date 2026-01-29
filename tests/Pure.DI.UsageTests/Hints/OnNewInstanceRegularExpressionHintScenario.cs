/*
$v=true
$p=4
$d=OnNewInstance regular expression hint
$h=Hints are used to fine-tune code generation. The _OnNewInstance_ hint determines whether to generate partial _OnNewInstance_ method.
$h=In addition, setup hints can be comments before the _Setup_ method in the form ```hint = value```, for example: `// OnNewInstance = On`.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The `OnNewInstanceLifetimeRegularExpression` hint helps you define a set of lifetimes that require instance creation control. You can use it to specify a regular expression to filter bindings by lifetime name.
$f=For more hints, see [this](README.md#setup-hints) page.
$f=What it shows:
$f=- Demonstrates the scenario setup and resulting object graph in Pure.DI.
$f=
$f=Important points:
$f=- Highlights the key configuration choices and their effect on resolution.
$f=
$f=Useful when:
$f=- You want a concrete template for applying this feature in a composition.
$f=
$r=Shouldly
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameterInPartialMethod
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Global

namespace Pure.DI.UsageTests.Hints.OnNewInstanceRegularExpressionHintScenario;

using Shouldly;
using Xunit;
using static Hint;

// {
//# using Pure.DI;
//# using static Pure.DI.Hint;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup(nameof(Composition))
            .Hint(OnNewInstance, "On")
            .Hint(OnNewInstanceLifetimeRegularExpression, "(Singleton|PerBlock)")
            .Bind().As(Lifetime.Singleton).To<GlobalCache>()
            .Bind().As(Lifetime.PerBlock).To<OrderProcessor>()
            .Root<IOrderProcessor>("OrderProcessor");

        var log = new List<string>();
        var composition = new Composition(log);

        // Create the OrderProcessor twice
        var processor1 = composition.OrderProcessor;
        var processor2 = composition.OrderProcessor;

        log.ShouldBe([
            "GlobalCache created",
            "OrderProcessor created",
            "OrderProcessor created"
        ]);
        // }
        composition.SaveClassDiagram();
    }
}

// {
interface IGlobalCache;

class GlobalCache : IGlobalCache;

interface IOrderProcessor
{
    IGlobalCache Cache { get; }
}

class OrderProcessor(IGlobalCache cache) : IOrderProcessor
{
    public IGlobalCache Cache { get; } = cache;
}

internal partial class Composition(List<string> log)
{
    partial void OnNewInstance<T>(
        ref T value,
        object? tag,
        Lifetime lifetime) =>
        log.Add($"{typeof(T).Name} created");
}
// }