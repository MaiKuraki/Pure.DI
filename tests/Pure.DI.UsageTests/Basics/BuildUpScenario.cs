/*
$v=true
$p=9
$d=Build up of an existing object
$h=This example shows the Build-Up pattern in dependency injection, where an existing object is injected with necessary dependencies through its properties, methods, or fields.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=Key Concepts:
$f=**Build-Up** - injecting dependencies into an already created object
$f=**Dependency Attribute** - marker for identifying injectable members
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

namespace Pure.DI.UsageTests.Basics.BuildUpScenario;

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
            .RootArg<string>("name")
            .Bind().To(Guid.NewGuid)
            .Bind().To(ctx => {
                var person = new Person();
                // Injects dependencies into an existing object
                ctx.BuildUp(person);
                return person;
            })
            .Bind().To<Greeter>()

            // Composition root
            .Root<IGreeter>("GetGreeter");

        var composition = new Composition();
        var greeter = composition.GetGreeter("Nik");

        greeter.Person.Name.ShouldBe("Nik");
        greeter.Person.Id.ShouldNotBe(Guid.Empty);
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IPerson
{
    string Name { get; }

    Guid Id { get; }
}

class Person : IPerson
{
    // The Dependency attribute specifies to perform an injection and its order
    [Dependency] public string Name { get; set; } = "";

    public Guid Id { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection and its order
    [Dependency] public void SetId(Guid id) => Id = id;
}

interface IGreeter
{
    IPerson Person { get; }
}

record Greeter(IPerson Person) : IGreeter;
// }