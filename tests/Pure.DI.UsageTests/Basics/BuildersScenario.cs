/*
$v=true
$p=9
$d=Builders
$h=Sometimes you need builders for all types derived from `T` that are known at compile time.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=Important Notes:
$f=- The default builder method name is `BuildUp`
$f=- The first argument to the builder method is always the instance to be built
$f=What it shows:
$f=- Demonstrates the scenario setup and resulting object graph in Pure.DI.
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
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
namespace Pure.DI.UsageTests.Basics.BuildersScenario;

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
            .Bind().To(Guid.NewGuid)
            .Bind().To<PlutoniumBattery>()
            // Creates a builder for each type inherited from IRobot.
            // These types must be available at this point in the code.
            .Builders<IRobot>("BuildUp");

        var composition = new Composition();

        var cleaner = composition.BuildUp(new CleanerBot());
        cleaner.Token.ShouldNotBe(Guid.Empty);
        cleaner.Battery.ShouldBeOfType<PlutoniumBattery>();

        var guard = composition.BuildUp(new GuardBot());
        guard.Token.ShouldBe(Guid.Empty);
        guard.Battery.ShouldBeOfType<PlutoniumBattery>();

        // Uses a common method to build an instance
        IRobot robot = new CleanerBot();
        robot = composition.BuildUp(robot);
        robot.ShouldBeOfType<CleanerBot>();
        robot.Token.ShouldNotBe(Guid.Empty);
        robot.Battery.ShouldBeOfType<PlutoniumBattery>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IBattery;

class PlutoniumBattery : IBattery;

interface IRobot
{
    Guid Token { get; }

    IBattery? Battery { get; }
}

record CleanerBot : IRobot
{
    public Guid Token { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public IBattery? Battery { get; set; }

    [Dependency]
    public void SetToken(Guid token) => Token = token;
}

record GuardBot : IRobot
{
    public Guid Token => Guid.Empty;

    [Dependency]
    public IBattery? Battery { get; set; }
}
// }