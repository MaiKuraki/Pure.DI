/*
$v=true
$p=2
$d=Factory
$h=This example shows manual creation and initialization. The generator usually infers dependencies from constructors, but sometimes you need custom creation or setup logic.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=There are scenarios where manual control over the creation process is required, such as
$f=- When additional initialization logic is needed
$f=- When complex construction steps are required
$f=- When specific object states need to be set during creation
$f=
$f=> [!IMPORTANT]
$f=> The method `Inject()` cannot be used outside of the binding setup.
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
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Global

namespace Pure.DI.UsageTests.Basics.FactoryScenario;

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
            .Bind<IDatabaseService>().To<DatabaseService>(ctx => {
                // Some logic for creating an instance.
                // For example, we need to manually initialize the connection.
                ctx.Inject(out DatabaseService service);
                service.Connect();
                return service;
            })
            .Bind<IUserRegistry>().To<UserRegistry>()

            // Composition root
            .Root<IUserRegistry>("Registry");

        var composition = new Composition();
        var registry = composition.Registry;
        registry.Database.IsConnected.ShouldBeTrue();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IDatabaseService
{
    bool IsConnected { get; }
}

class DatabaseService : IDatabaseService
{
    public bool IsConnected { get; private set; }

    // Simulates a connection establishment that must be called explicitly
    public void Connect() => IsConnected = true;
}

interface IUserRegistry
{
    IDatabaseService Database { get; }
}

class UserRegistry(IDatabaseService database) : IUserRegistry
{
    public IDatabaseService Database { get; } = database;
}
// }
