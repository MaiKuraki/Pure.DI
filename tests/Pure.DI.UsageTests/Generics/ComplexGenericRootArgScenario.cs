/*
$v=true
$p=9
$d=Complex generic root arguments
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
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers

// ReSharper disable UnusedVariable
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable NotAccessedPositionalProperty.Global
namespace Pure.DI.UsageTests.Generics.ComplexGenericRootArgScenario;

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
            // Defines a generic root argument 'config' of type SourceConfig<T>.
            // This allows passing specific configuration when resolving ISource<T>.
            .RootArg<SourceConfig<TT>>("config")
            .Bind<ISource<TT2>>().To<Source<TT2>>()

            // Composition root that creates a source for a specific type.
            // The 'GetSource' method will accept 'SourceConfig<T>' as an argument.
            .Root<ISource<TT3>>("GetSource");

        var composition = new Composition();

        // Resolve a source for 'int', passing specific configuration
        var source = composition.GetSource<int>(
            new SourceConfig<int>(33, "IntSource"));

        source.Value.ShouldBe(33);
        source.Name.ShouldBe("IntSource");
        // }
        composition.SaveClassDiagram();
    }
}

// {
// Represents configuration for a data source, including a default value
record SourceConfig<T>(T DefaultValue, string SourceName);

interface ISource<out T>
{
    T? Value { get; }
    string Name { get; }
}

class Source<T> : ISource<T>
{
    // The Dependency attribute specifies to perform an injection.
    // We use method injection to initialize the source with configuration
    // passed from the composition root.
    [Dependency]
    public void Initialize(SourceConfig<T> config)
    {
        Value = config.DefaultValue;
        Name = config.SourceName;
    }

    public T? Value { get; private set; }

    public string Name { get; private set; } = "";
}
// }