/*
$v=true
$p=8
$d=Generic root arguments
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

// ReSharper disable UnusedVariable
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable RedundantTypeArgumentsOfMethod
// ReSharper disable UnusedMemberInSuper.Global
namespace Pure.DI.UsageTests.Generics.GenericRootArgScenario;

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
            .RootArg<TT>("model")
            .Bind<IPresenter<TT>>().To<Presenter<TT>>()

            // Composition root
            .Root<IPresenter<TT>>("GetPresenter");

        var composition = new Composition();

        // The "model" argument is passed to the composition root
        // and then injected into the "Presenter" class
        var presenter = composition.GetPresenter<string>(model: "Hello World");

        presenter.Model.ShouldBe("Hello World");
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IPresenter<out T>
{
    T? Model { get; }
}

class Presenter<T> : IPresenter<T>
{
    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public void Present(T model) =>
        Model = model;

    public T? Model { get; private set; }
}
// }