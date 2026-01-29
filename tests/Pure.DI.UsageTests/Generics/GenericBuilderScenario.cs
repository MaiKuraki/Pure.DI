/*
$v=true
$p=9
$d=Generic builder
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
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable RedundantArgumentDefaultValue
// ReSharper disable ClassNeverInstantiated.Global
namespace Pure.DI.UsageTests.Generics.GenericBuilderScenario;

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
            .Bind(Tag.Id).To(() => (TT)(object)Guid.NewGuid())
            .Bind().To<Repository<TT>>()
            // Generic service builder
            // Defines a generic builder "BuildUp".
            // This is useful when instances are created by an external framework
            // (like a UI library or serialization) but require dependencies to be injected.
            .Builder<ViewModel<TTS, TT2>>("BuildUp");

        var composition = new Composition();

        // A view model instance created manually (or by a UI framework)
        var viewModel = new ViewModel<Guid, Customer>();

        // Inject dependencies (Id and Repository) into the existing instance
        var builtViewModel = composition.BuildUp(viewModel);

        builtViewModel.Id.ShouldNotBe(Guid.Empty);
        builtViewModel.Repository.ShouldBeOfType<Repository<Customer>>();
// }
        composition.SaveClassDiagram();
    }
}

// {
// Domain model
record Customer;

interface IRepository<T>;

class Repository<T> : IRepository<T>;

interface IViewModel<out TId, TModel>
{
    TId Id { get; }

    IRepository<TModel>? Repository { get; }
}

// The view model is generic, allowing it to be used for various entities
record ViewModel<TId, TModel> : IViewModel<TId, TModel>
    where TId : struct
{
    public TId Id { get; private set; }

    // The dependency to be injected
    [Dependency]
    public IRepository<TModel>? Repository { get; set; }

    // Method injection for the ID
    [Dependency]
    public void SetId([Tag(Tag.Id)] TId id) => Id = id;
}
// }