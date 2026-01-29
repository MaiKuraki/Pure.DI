/*
$v=true
$p=9
$d=Builder
$h=Sometimes you need to build up an existing composition root and inject all of its dependencies, in which case the `Builder` method will be useful, as in the example below:
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=Important Notes:
$f=- The default builder method name is `BuildUp`
$f=- The first argument to the builder method is always the instance to be built
$f=
$f=Advantages:
$f=- Allows working with pre-existing objects
$f=- Provides flexibility in dependency injection
$f=- Supports partial injection of dependencies
$f=- Can be used with legacy code
$f=
$f=Use Cases:
$f=- When objects are created outside the DI container
$f=- For working with third-party libraries
$f=- When migrating existing code to DI
$f=- For complex object graphs where full construction is not feasible
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
namespace Pure.DI.UsageTests.Basics.BuilderScenario;

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
            .Bind().To<PhotonBlaster>()
            .Builder<Player>("Equip");

        var composition = new Composition();

        // The Game Engine instantiates the Player entity,
        // so we need to inject dependencies into the existing instance.
        var player = composition.Equip(new Player());

        player.Id.ShouldNotBe(Guid.Empty);
        player.Weapon.ShouldBeOfType<PhotonBlaster>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IWeapon;

class PhotonBlaster : IWeapon;

interface IGameEntity
{
    Guid Id { get; }

    IWeapon? Weapon { get; }
}

record Player : IGameEntity
{
    public Guid Id { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public IWeapon? Weapon { get; set; }

    [Dependency]
    public void SetId(Guid id) => Id = id;
}
// }