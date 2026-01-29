/*
$v=true
$p=20
$d=Ref dependencies
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

// ReSharper disable once CheckNamespace
// ReSharper disable ClassNeverInstantiated.Global
namespace Pure.DI.UsageTests.Basics.RefDependenciesScenario;

using Xunit;

// {
//# using Pure.DI;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
// {        
        DI.Setup("Composition")
            // Represents a large data set or buffer
            .Bind().To<int[]>(() => [10, 20, 30])
            .Root<Service>("MyService");

        var composition = new Composition();
        var service = composition.MyService;
        service.Sum.ShouldBe(60);
// }
        composition.SaveClassDiagram();
    }
}

// {
class Service
{
    public int Sum { get; private set; }

    // Ref structs cannot be fields, so they are injected via a method
    // with the [Ordinal] attribute. This allows working with
    // high-performance types like Span<T> or other ref structs.
    [Ordinal]
    public void Initialize(ref Data data) =>
        Sum = data.Sum();
}

// A ref struct that holds a reference to the data
// to process it without additional memory allocations
readonly ref struct Data(ref int[] data)
{
    private readonly ref int[] _dep = ref data;

    public int Sum() => _dep.Sum();
}
// }