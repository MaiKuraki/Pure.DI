/*
$v=true
$p=3
$d=Injections on demand with arguments
$h=This example uses a parameterized factory so dependencies can be created with runtime arguments. The service creates sensors with specific IDs at instantiation time.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=Delayed dependency instantiation:
$f=- Injection of dependencies requiring runtime parameters
$f=- Creation of distinct instances with different configurations
$f=- Type-safe resolution of dependencies with constructor arguments
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
// ReSharper disable ArrangeTypeModifiers

namespace Pure.DI.UsageTests.Basics.InjectionOnDemandWithArgumentsScenario;

using Shouldly;
using Xunit;

// {
//# using Pure.DI;
//# using System.Collections.Generic;
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
            .Bind().To<Sensor>()
            .Bind().To<SmartHome>()

            // Composition root
            .Root<ISmartHome>("SmartHome");

        var composition = new Composition();
        var smartHome = composition.SmartHome;
        var sensors = smartHome.Sensors;

        sensors.Count.ShouldBe(2);
        sensors[0].Id.ShouldBe(101);
        sensors[1].Id.ShouldBe(102);
// }
        composition.SaveClassDiagram();
    }
}

// {
interface ISensor
{
    int Id { get; }
}

class Sensor(int id) : ISensor
{
    public int Id { get; } = id;
}

interface ISmartHome
{
    IReadOnlyList<ISensor> Sensors { get; }
}

class SmartHome(Func<int, ISensor> sensorFactory) : ISmartHome
{
    public IReadOnlyList<ISensor> Sensors { get; } =
    [
        // Use the injected factory to create a sensor with ID 101
        sensorFactory(101),

        // Create another sensor with ID 102
        sensorFactory(102)
    ];
}
// }
