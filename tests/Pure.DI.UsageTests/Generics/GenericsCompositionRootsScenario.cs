/*
$v=true
$p=2
$d=Generic composition roots
$h=Sometimes you want to be able to create composition roots with type parameters. In this case, the composition root can only be represented by a method.
$h=> [!IMPORTANT]
$h=> ``Resolve()` methods cannot be used to resolve generic composition roots.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=> [!IMPORTANT]
$f=> The method `Inject()`cannot be used outside of the binding setup.
$f=What it shows:
$f=- Demonstrates the scenario setup and resulting object graph in Pure.DI.
$f=
$f=Important points:
$f=- Highlights the key configuration choices and their effect on resolution.
$f=
$f=Useful when:
$f=- You want a concrete template for applying this feature in a composition.
$f=
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable
// ReSharper disable UnusedTypeParameter

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Generics.GenericsCompositionRootsScenario;

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
            // Disable Resolve methods to keep the public API minimal
            .Hint(Hint.Resolve, "Off")
            .Bind().To<Repository<TT>>()
            .Bind().To<CreateCommandHandler<TT>>()
            // Creates UpdateCommandHandler manually,
            // just for the sake of example
            .Bind("Update").To(ctx => {
                ctx.Inject(out IRepository<TT> repository);
                return new UpdateCommandHandler<TT>(repository);
            })

            // Specifies to create a regular public method
            // to get a composition root of type ICommandHandler<T>
            // with the name "GetCreateCommandHandler"
            .Root<ICommandHandler<TT>>("GetCreateCommandHandler")

            // Specifies to create a regular public method
            // to get a composition root of type ICommandHandler<T>
            // with the name "GetUpdateCommandHandler"
            // using the "Update" tag
            .Root<ICommandHandler<TT>>("GetUpdateCommandHandler", "Update");

        var composition = new Composition();

        // createHandler = new CreateCommandHandler<int>(new Repository<int>());
        var createHandler = composition.GetCreateCommandHandler<int>();

        // updateHandler = new UpdateCommandHandler<string>(new Repository<string>());
        var updateHandler = composition.GetUpdateCommandHandler<string>();
// }
        createHandler.ShouldBeOfType<CreateCommandHandler<int>>();
        updateHandler.ShouldBeOfType<UpdateCommandHandler<string>>();
        composition.SaveClassDiagram();
    }
}

// {
interface IRepository<T>;

class Repository<T> : IRepository<T>;

interface ICommandHandler<T>;

class CreateCommandHandler<T>(IRepository<T> repository) : ICommandHandler<T>;

class UpdateCommandHandler<T>(IRepository<T> repository) : ICommandHandler<T>;
// }