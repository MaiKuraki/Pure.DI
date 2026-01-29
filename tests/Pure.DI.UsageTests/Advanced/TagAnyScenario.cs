/*
$v=true
$p=3
$d=Tag Any
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
// ReSharper disable UnusedType.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable PreferConcreteValueOverDefault
namespace Pure.DI.UsageTests.Advanced.TagAnyScenario;

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
            // Binds IQueue to the Queue implementation.
            // Tag.Any creates a binding that matches any tag (including null),
            // allowing the specific tag value to be used within the factory (ctx.Tag).
            .Bind<IQueue>(Tag.Any).To(ctx => new Queue(ctx.Tag))
            .Bind<IQueueService>().To<QueueService>()

            // Composition root
            .Root<IQueueService>("QueueService")

            // Root by Tag.Any: Resolves IQueue with the tag "Audit"
            .Root<IQueue>("AuditQueue", "Audit");

        var composition = new Composition();
        var queueService = composition.QueueService;

        queueService.WorkItemsQueue.Id.ShouldBe("WorkItems");
        queueService.PartitionQueue.Id.ShouldBe(42);
        queueService.DefaultQueue.Id.ShouldBeNull();
        composition.AuditQueue.Id.ShouldBe("Audit");
        // }
        composition.SaveClassDiagram();
    }
}

// {
interface IQueue
{
    object? Id { get; }
}

record Queue(object? Id) : IQueue;

interface IQueueService
{
    IQueue WorkItemsQueue { get; }

    IQueue PartitionQueue { get; }

    IQueue DefaultQueue { get; }
}

class QueueService(
    // Injects IQueue tagged with "WorkItems"
    [Tag("WorkItems")] IQueue workItemsQueue,
    // Injects IQueue tagged with integer 42
    [Tag(42)] Func<IQueue> partitionQueueFactory,
    // Injects IQueue with the default (null) tag
    IQueue defaultQueue)
    : IQueueService
{
    public IQueue WorkItemsQueue { get; } = workItemsQueue;

    public IQueue PartitionQueue { get; } = partitionQueueFactory();

    public IQueue DefaultQueue { get; } = defaultQueue;
}
// }