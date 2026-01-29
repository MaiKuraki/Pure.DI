/*
$v=true
$p=4
$d=OnNewInstance wildcard hint
$h=Hints are used to fine-tune code generation. The _OnNewInstance_ hint determines whether to generate partial _OnNewInstance_ method.
$h=In addition, setup hints can be comments before the _Setup_ method in the form ```hint = value```, for example: `// OnNewInstance = On`.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The `OnNewInstanceImplementationTypeNameWildcard` hint helps you define a set of implementation types that require instance creation control. You can use it to specify a wildcard to filter bindings by implementation name.
$f=For more hints, see [this](README.md#setup-hints) page.
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
// ReSharper disable UnusedParameterInPartialMethod
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Global

namespace Pure.DI.UsageTests.Hints.OnNewInstanceWildcardHintScenario;

using Shouldly;
using Xunit;
using static Hint;

// {
//# using Pure.DI;
//# using static Pure.DI.Hint;
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
            .Hint(OnNewInstance, "On")
            // Hints restrict the generation of the partial OnNewInstance method
            // to only those types whose names match the specified wildcards.
            // In this case, we want to track the creation of repositories and services.
            .Hint(OnNewInstanceImplementationTypeNameWildcard, "*Repository")
            .Hint(OnNewInstanceImplementationTypeNameWildcard, "*Service")
            .Bind().As(Lifetime.Singleton).To<UserRepository>()
            .Bind().To<OrderService>()
            // This type will not be tracked because its name
            // does not match the wildcards
            .Bind().To<ConsoleLogger>()
            .Root<IOrderService>("Root");

        var log = new List<string>();
        var composition = new Composition(log);

        var service1 = composition.Root;
        var service2 = composition.Root;

        log.ShouldBe([
            "UserRepository created",
            "OrderService created",
            "OrderService created"
        ]);
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IRepository;

class UserRepository : IRepository
{
    public override string ToString() => nameof(UserRepository);
}

interface ILogger;

class ConsoleLogger : ILogger
{
    public override string ToString() => nameof(ConsoleLogger);
}

interface IOrderService
{
    IRepository Repository { get; }
}

class OrderService(IRepository repository, ILogger logger) : IOrderService
{
    public IRepository Repository { get; } = repository;

    public ILogger Logger { get; } = logger;

    public override string ToString() => nameof(OrderService);
}

internal partial class Composition(List<string> log)
{
    partial void OnNewInstance<T>(
        ref T value,
        object? tag,
        Lifetime lifetime) =>
        log.Add($"{typeof(T).Name} created");
}
// }