/*
$v=true
$p=8
$d=Generic root arguments
$h=Demonstrates how to pass type arguments as parameters to generic composition roots.
$f=> [!NOTE]
$f=> Generic root arguments enable flexible type parameterization while maintaining compile-time type safety.
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