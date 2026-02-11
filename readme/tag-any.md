#### Tag Any

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Shouldly;
using Pure.DI;

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
```

<details>
<summary>Running this code sample locally</summary>

- Make sure you have the [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or later installed
```bash
dotnet --list-sdk
```
- Create a net10.0 (or later) console application
```bash
dotnet new console -n Sample
```
- Add references to the NuGet packages
  - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
  - [Shouldly](https://www.nuget.org/packages/Shouldly)
```bash
dotnet add package Pure.DI
dotnet add package Shouldly
```
- Copy the example code into the _Program.cs_ file

You are ready to run the example 🚀
```bash
dotnet run
```

</details>

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


The following partial class will be generated:

```c#
partial class Composition
{
  public IQueue AuditQueue
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      Queue transientQueue81 = new Queue("Audit");
      return transientQueue81;
    }
  }

  public IQueueService QueueService
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      Queue transientQueue83 = new Queue("WorkItems");
      Queue transientQueue85 = new Queue(null);
      Func<IQueue> transientFunc84 = new Func<IQueue>(
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      () =>
      {
        Queue transientQueue86 = new Queue(42);
        IQueue localValue = transientQueue86;
        return localValue;
      });
      return new QueueService(transientQueue83, transientFunc84, transientQueue85);
    }
  }
}
```

Class diagram:

```mermaid
---
 config:
  maxTextSize: 2147483647
  maxEdges: 2147483647
  class:
   hideEmptyMembersBox: true
---
classDiagram
	Queue --|> IQueue
	QueueService --|> IQueueService
	Composition ..> Queue : IQueue AuditQueue
	Composition ..> QueueService : IQueueService QueueService
	QueueService *--  Queue : "WorkItems"  IQueue
	QueueService *--  Queue : IQueue
	QueueService o-- "PerBlock" FuncᐸIQueueᐳ : 42  FuncᐸIQueueᐳ
	FuncᐸIQueueᐳ *--  Queue : 42  IQueue
	namespace Pure.DI.UsageTests.Advanced.TagAnyScenario {
		class Composition {
		<<partial>>
		+IQueue AuditQueue
		+IQueueService QueueService
		}
		class IQueue {
			<<interface>>
		}
		class IQueueService {
			<<interface>>
		}
		class Queue {
				<<record>>
		}
		class QueueService {
				<<class>>
			+QueueService(IQueue workItemsQueue, FuncᐸIQueueᐳ partitionQueueFactory, IQueue defaultQueue)
		}
	}
	namespace System {
		class FuncᐸIQueueᐳ {
				<<delegate>>
		}
	}
```

