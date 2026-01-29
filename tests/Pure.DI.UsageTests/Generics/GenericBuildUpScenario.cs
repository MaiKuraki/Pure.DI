/*
$v=true
$p=7
$d=Build up of an existing generic object
$h=In other words, injecting the necessary dependencies via methods, properties, or fields into an existing object.
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

namespace Pure.DI.UsageTests.Generics.GenericBuildUpScenario;

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
            .RootArg<string>("userName")
            .Bind().To(Guid.NewGuid)
            .Bind().To(ctx => {
                // The "BuildUp" method injects dependencies into an existing object.
                // This is useful when the object is created externally (e.g., by a UI framework
                // or an ORM) or requires specific initialization before injection.
                var context = new UserContext<TTS>();
                ctx.BuildUp(context);
                return context;
            })
            .Bind().To<Facade<TTS>>()

            // Composition root
            .Root<IFacade<Guid>>("GetFacade");

        var composition = new Composition();
        var facade = composition.GetFacade("Erik");

        facade.Context.UserName.ShouldBe("Erik");
        facade.Context.Id.ShouldNotBe(Guid.Empty);
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IUserContext<out T>
    where T : struct
{
    string UserName { get; }

    T Id { get; }
}

class UserContext<T> : IUserContext<T>
    where T : struct
{
    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public string UserName { get; set; } = "";

    public T Id { get; private set; }

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public void SetId(T id) => Id = id;
}

interface IFacade<out T>
    where T : struct
{
    IUserContext<T> Context { get; }
}

record Facade<T>(IUserContext<T> Context)
    : IFacade<T> where T : struct;
// }