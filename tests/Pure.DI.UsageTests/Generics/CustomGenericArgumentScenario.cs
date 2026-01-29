/*
$v=true
$p=6
$d=Custom generic argument
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
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
// ReSharper disable UnusedTypeParameter
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable InconsistentNaming

namespace Pure.DI.UsageTests.Generics.CustomGenericArgumentScenario;

using Shouldly;
using Xunit;

// {
//# using Pure.DI;
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
            // Registers the "MyTT" interface as a custom generic type argument
            // to be used as a marker for generic bindings
            .GenericTypeArgument<MyTT>()
            .Bind<ISequence<MyTT>>().To<Sequence<MyTT>>()
            .Bind<IProgram>().To<MyApp>()

            // Composition root
            .Root<IProgram>("Root");

        var composition = new Composition();
        var program = composition.Root;
        program.IntSequence.ShouldBeOfType<Sequence<int>>();
        program.StringSequence.ShouldBeOfType<Sequence<string>>();
// }
        composition.SaveClassDiagram();
    }
}

// {
// Defines a custom generic type argument marker
interface MyTT;

interface ISequence<T>;

class Sequence<T> : ISequence<T>;

interface IProgram
{
    ISequence<int> IntSequence { get; }

    ISequence<string> StringSequence { get; }
}

class MyApp(
    ISequence<int> intSequence,
    ISequence<string> stringSequence)
    : IProgram
{
    public ISequence<int> IntSequence { get; } = intSequence;

    public ISequence<string> StringSequence { get; } = stringSequence;
}
// }