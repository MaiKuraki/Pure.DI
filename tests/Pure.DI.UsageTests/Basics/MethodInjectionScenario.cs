/*
$v=true
$p=9
$d=Method injection
$h=To use dependency implementation for a method, simply add the _Ordinal_ attribute to that method, specifying the sequence number that will be used to define the call to that method:
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The key points are:
$f=- The method must be available to be called from a composition class
$f=- The `Dependency` (or `Ordinal`) attribute is used to mark the method for injection
$f=- The container automatically calls the method to inject dependencies
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

namespace Pure.DI.UsageTests.Basics.MethodInjectionScenario;

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
            .Bind<IMap>().To<Map>()
            .Bind<INavigator>().To<Navigator>()

            // Composition root
            .Root<INavigator>("Navigator");

        var composition = new Composition();
        var navigator = composition.Navigator;
        navigator.CurrentMap.ShouldBeOfType<Map>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IMap;

class Map : IMap;

interface INavigator
{
    IMap? CurrentMap { get; }
}

class Navigator : INavigator
{
    // The Dependency attribute specifies that the container should call this method
    // to inject the dependency.
    [Dependency]
    public void LoadMap(IMap map) =>
        CurrentMap = map;

    public IMap? CurrentMap { get; private set; }
}
// }