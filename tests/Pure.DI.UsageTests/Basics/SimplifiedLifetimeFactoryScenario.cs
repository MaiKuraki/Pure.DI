/*
$v=true
$p=8
$d=Simplified lifetime-specific factory
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
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedParameter.Global
namespace Pure.DI.UsageTests.Basics.SimplifiedLifetimeFactoryScenario;

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
            .Transient(Guid.NewGuid)
            .Transient(() => DateTime.Today, "today")
            // Injects FileLogger and DateTime instances
            // and performs further initialization logic
            // defined in the lambda function to set up the log file name
            .Singleton<FileLogger, DateTime, IFileLogger>((
                FileLogger logger,
                [Tag("today")] DateTime date) => {
                logger.Init($"app-{date:yyyy-MM-dd}.log");
                return logger;
            })
            .Transient<OrderProcessingService>()

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

class FileLogger(Func<Guid> idFactory) : IFileLogger
{
    public string FileName { get; private set; } = "";

    public void Init(string fileName) => FileName = fileName;

    public void Log(string message)
    {
        var id = idFactory();
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