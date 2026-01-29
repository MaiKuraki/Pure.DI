/*
$v=true
$p=4
$d=Scope
$h=The _Scoped_ lifetime ensures that there will be a single instance of the dependency for each scope.
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
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Lifetimes.ScopeScenario;

using Xunit;
using static Lifetime;

// {
//# using Pure.DI;
//# using static Pure.DI.Lifetime;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
// {
        var composition = new Composition();
        var app = composition.AppRoot;

        // Real-world analogy:
        // each HTTP request (or message consumer handling) creates its own scope.
        // Scoped services live exactly as long as the request is being processed.

        // Request #1
        var request1 = app.CreateRequestScope();
        var checkout1 = request1.RequestRoot;

        var ctx11 = checkout1.Context;
        var ctx12 = checkout1.Context;

        // Same request => same scoped instance
        ctx11.ShouldBe(ctx12);

        // Request #2
        var request2 = app.CreateRequestScope();
        var checkout2 = request2.RequestRoot;

        var ctx2 = checkout2.Context;

        // Different request => different scoped instance
        ctx11.ShouldNotBe(ctx2);

        // End of Request #1 => scoped instance is disposed
        request1.Dispose();
        ctx11.IsDisposed.ShouldBeTrue();

        // End of Request #2 => scoped instance is disposed
        request2.Dispose();
        ctx2.IsDisposed.ShouldBeTrue();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IRequestContext
{
    Guid CorrelationId { get; }

    bool IsDisposed { get; }
}

// Typically: DbContext / UnitOfWork / RequestTelemetry / Activity, etc.
sealed class RequestContext : IRequestContext, IDisposable
{
    public Guid CorrelationId { get; } = Guid.NewGuid();

    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

interface ICheckoutService
{
    IRequestContext Context { get; }
}

// "Controller/service" that participates in request processing.
// It depends on a scoped context (per-request resource).
sealed class CheckoutService(IRequestContext context) : ICheckoutService
{
    public IRequestContext Context => context;
}

// Implements a request scope (per-request container)
sealed class RequestScope(Composition parent) : Composition(parent);

partial class App(Func<RequestScope> requestScopeFactory)
{
    // In a web app this would roughly map to: "create scope for request"
    public RequestScope CreateRequestScope() => requestScopeFactory();
}

partial class Composition
{
    static void Setup() =>
// }
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup()
            // Per-request lifetime
            .Bind().As(Scoped).To<RequestContext>()

            // Regular service that consumes scoped context
            .Bind().To<CheckoutService>()

            // "Request root" (what your controller/handler resolves)
            .Root<ICheckoutService>("RequestRoot")

            // "Application root" (what creates request scopes)
            .Root<App>("AppRoot");
}
// }