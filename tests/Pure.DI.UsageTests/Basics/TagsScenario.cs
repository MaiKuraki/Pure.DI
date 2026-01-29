/*
$v=true
$p=6
$d=Tags
$h=Tags let you control dependency selection when multiple implementations exist:
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The example shows how to:
$f=- Define multiple bindings for the same interface
$f=- Use tags to differentiate between implementations
$f=- Control lifetime management
$f=- Inject tagged dependencies into constructors
$f=
$f=The tag can be a constant, a type, a [smart tag](smart-tags.md), or a value of an `Enum` type. The _default_ and _null_ tags are also supported.
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

// ReSharper disable PreferConcreteValueOverDefault
namespace Pure.DI.UsageTests.Basics.TagsScenario;

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
            // The `default` tag is used when the consumer does not specify a tag
            .Bind<IApiClient>("Public", default).To<RestApiClient>()
            .Bind<IApiClient>("Internal").As(Lifetime.Singleton).To<InternalApiClient>()
            .Bind<IApiFacade>().To<ApiFacade>()

            // "InternalRoot" is a root name, "Internal" is a tag
            .Root<IApiClient>("InternalRoot", "Internal")

            // Specifies to create the composition root named "Root"
            .Root<IApiFacade>("Api");

        var composition = new Composition();
        var api = composition.Api;
        api.PublicClient.ShouldBeOfType<RestApiClient>();
        api.InternalClient.ShouldBeOfType<InternalApiClient>();
        api.InternalClient.ShouldBe(composition.InternalRoot);
        api.DefaultClient.ShouldBeOfType<RestApiClient>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IApiClient;

class RestApiClient : IApiClient;

class InternalApiClient : IApiClient;

interface IApiFacade
{
    IApiClient PublicClient { get; }

    IApiClient InternalClient { get; }

    IApiClient DefaultClient { get; }
}

class ApiFacade(
    [Tag("Public")] IApiClient publicClient,
    [Tag("Internal")] IApiClient internalClient,
    IApiClient defaultClient)
    : IApiFacade
{
    public IApiClient PublicClient { get; } = publicClient;

    public IApiClient InternalClient { get; } = internalClient;

    public IApiClient DefaultClient { get; } = defaultClient;
}
// }
