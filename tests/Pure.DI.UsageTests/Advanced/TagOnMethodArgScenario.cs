/*
$v=true
$p=6
$d=Tag on a method argument
$h=The wildcards `*` and `?` are supported.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=> [!WARNING]
$f=> Each potentially injectable argument, property, or field contains an additional tag. This tag can be used to specify what can be injected there. This will only work if the binding type and the tag match. So while this approach can be useful for specifying what to enter, it can be more expensive to maintain and less reliable, so it is recommended to use attributes like `[Tag(...)]` instead.
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
// ReSharper disable UnusedType.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedTypeParameter

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.TagOnMethodArgScenario;

using System.Diagnostics.CodeAnalysis;
using Pure.DI;
using UsageTests;
using Xunit;

// {
//# using Pure.DI;
// }

[SuppressMessage("WRN", "DIW003:WRN")]
public class Scenario
{
    [Fact]
    public void Run()
    {
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup(nameof(Composition))
            .Bind().To<TemperatureSensor>()
            // Binds specifically to the argument "sensor" of the "Calibrate" method
            // in the "WeatherStation" class
            .Bind(Tag.OnMethodArg<WeatherStation>(nameof(WeatherStation.Calibrate), "sensor"))
            .To<HumiditySensor>()
            .Bind<IWeatherStation>().To<WeatherStation>()

            // Specifies to create the composition root named "Station"
            .Root<IWeatherStation>("Station");

        var composition = new Composition();
        var station = composition.Station;
        station.Sensor.ShouldBeOfType<HumiditySensor>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface ISensor;

class TemperatureSensor : ISensor;

class HumiditySensor : ISensor;

interface IWeatherStation
{
    ISensor? Sensor { get; }
}

class WeatherStation : IWeatherStation
{
    // The [Dependency] attribute is used to mark the method for injection
    [Dependency]
    public void Calibrate(ISensor sensor) =>
        Sensor = sensor;

    public ISensor? Sensor { get; private set; }
}
// }