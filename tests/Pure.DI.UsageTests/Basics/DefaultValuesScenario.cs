/*
$v=true
$p=15
$d=Default values
$h=This example shows how to use default values in dependency injection when explicit injection is not possible.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The key points are:
$f=- Default constructor arguments can be used for simple values
$f=- The DI container will use these defaults if no explicit bindings are provided
$f=
$f=This example shows how to handle default values in a dependency injection scenario:
$f=- **Constructor Default Argument**: The `SecuritySystem` class has a constructor with a default value for the name parameter. If no value is provided, "Home Guard" will be used.
$f=- **Required Property with Default**: The `Sensor` property is marked as required but has a default instantiation. This ensures that:
$f=  - The property must be set
$f=  - If no explicit injection occurs, a default value will be used
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

namespace Pure.DI.UsageTests.Basics.DefaultValuesScenario;

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
            .Bind<ISensor>().To<MotionSensor>()
            .Bind<ISecuritySystem>().To<SecuritySystem>()

            // Composition root
            .Root<ISecuritySystem>("SecuritySystem");

        var composition = new Composition();
        var securitySystem = composition.SecuritySystem;
        securitySystem.Sensor.ShouldBeOfType<MotionSensor>();
        securitySystem.SystemName.ShouldBe("Home Guard");
// }
        composition.SaveClassDiagram();
    }
}

// {
interface ISensor;

class MotionSensor : ISensor;

interface ISecuritySystem
{
    string SystemName { get; }

    ISensor Sensor { get; }
}

// If injection cannot be performed explicitly,
// the default value will be used
class SecuritySystem(string systemName = "Home Guard") : ISecuritySystem
{
    public string SystemName { get; } = systemName;

    // The 'required' modifier ensures that the property is set during initialization.
    // The default value 'new MotionSensor()' provides a fallback
    // if no dependency is injected.
    public required ISensor Sensor { get; init; } = new MotionSensor();
}
// }