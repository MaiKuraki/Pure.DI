/*
$v=true
$p=2
$d=Simplified factory
$h=This example shows a simplified manual factory. Each lambda parameter represents an injected dependency, and starting with C# 10 you can add `Tag(...)` to specify a tagged dependency.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The example creates a service that depends on a logger initialized with a date-based file name. The `Tag` attribute enables named dependencies for more complex setups.
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
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedParameter.Global
namespace Pure.DI.UsageTests.Basics.SimplifiedFactoryScenario;

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
            .Bind("today").To(() => DateTime.Today)
            // Injects FileLogger and DateTime
            // and applies additional initialization logic
            .Bind<IFileLogger>().To((
                FileLogger logger,
                [Tag("today")] DateTime date) => {
                logger.Init($"app-{date:yyyy-MM-dd}.log");
                return logger;
            })
            .Bind().To<OrderProcessingService>()

            // Composition root
            .Root<IOrderProcessingService>("OrderService");

        var composition = new Composition();
        var service = composition.OrderService;

        service.Logger.FileName.ShouldBe($"app-{DateTime.Today:yyyy-MM-dd}.log");
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IFileLogger
{
    string FileName { get; }

    void Log(string message);
}

class FileLogger : IFileLogger
{
    public string FileName { get; private set; } = "";

    public void Init(string fileName) => FileName = fileName;

    public void Log(string message)
    {
        // Write to file
    }
}

interface IOrderProcessingService
{
    IFileLogger Logger { get; }
}

class OrderProcessingService(IFileLogger logger) : IOrderProcessingService
{
    public IFileLogger Logger { get; } = logger;
}
// }
