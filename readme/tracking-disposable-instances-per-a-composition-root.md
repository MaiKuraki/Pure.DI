#### Tracking disposable instances per a composition root

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Shouldly;
using Pure.DI;

var composition = new Composition();
var orderProcessingService1 = composition.OrderProcessingService;
var orderProcessingService2 = composition.OrderProcessingService;

orderProcessingService2.Dispose();

// Checks that the disposable instances
// associated with orderProcessingService2 have been disposed of
orderProcessingService2.Value.DbConnection.IsDisposed.ShouldBeTrue();

// Checks that the disposable instances
// associated with orderProcessingService1 have not been disposed of
orderProcessingService1.Value.DbConnection.IsDisposed.ShouldBeFalse();

orderProcessingService1.Dispose();

// Checks that the disposable instances
// associated with orderProcessingService1 have been disposed of
orderProcessingService1.Value.DbConnection.IsDisposed.ShouldBeTrue();

interface IDbConnection
{
    bool IsDisposed { get; }
}

class DbConnection : IDbConnection, IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

interface IOrderProcessingService
{
    public IDbConnection DbConnection { get; }
}

class OrderProcessingService(IDbConnection dbConnection) : IOrderProcessingService
{
    public IDbConnection DbConnection { get; } = dbConnection;
}

partial class Composition
{
    static void Setup() =>

        DI.Setup()
            .Bind().To<DbConnection>()
            .Bind().To<OrderProcessingService>()

            // A special composition root
            // that allows to manage disposable dependencies
            .Root<Owned<IOrderProcessingService>>("OrderProcessingService");
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
#if NET9_0_OR_GREATER
  private readonly Lock _lock = new Lock();
#else
  private readonly Object _lock = new Object();
#endif

  public Owned<IOrderProcessingService> OrderProcessingService
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      var perBlockOwned138 = new Owned();
      Owned<IOrderProcessingService> transientOwned137;
      // Creates the owner of an instance
      Owned transientOwned139;
      Owned localOwned7 = perBlockOwned138;
      transientOwned139 = localOwned7;
      lock (_lock)
      {
        perBlockOwned138.Add(transientOwned139);
      }

      IOwned localOwned6 = transientOwned139;
      var transientDbConnection141 = new DbConnection();
      lock (_lock)
      {
        perBlockOwned138.Add(transientDbConnection141);
      }

      IOrderProcessingService localValue6 = new OrderProcessingService(transientDbConnection141);
      transientOwned137 = new Owned<IOrderProcessingService>(localValue6, localOwned6);
      lock (_lock)
      {
        perBlockOwned138.Add(transientOwned137);
      }

      return transientOwned137;
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
	Owned --|> IOwned
	DbConnection --|> IDbConnection
	OrderProcessingService --|> IOrderProcessingService
	Composition ..> OwnedᐸIOrderProcessingServiceᐳ : OwnedᐸIOrderProcessingServiceᐳ OrderProcessingService
	OrderProcessingService *--  DbConnection : IDbConnection
	OwnedᐸIOrderProcessingServiceᐳ *--  Owned : IOwned
	OwnedᐸIOrderProcessingServiceᐳ *--  OrderProcessingService : IOrderProcessingService
	namespace Pure.DI {
		class IOwned {
			<<interface>>
		}
		class Owned {
				<<class>>
		}
		class OwnedᐸIOrderProcessingServiceᐳ {
				<<struct>>
		}
	}
	namespace Pure.DI.UsageTests.Advanced.TrackingDisposableScenario {
		class Composition {
		<<partial>>
		+OwnedᐸIOrderProcessingServiceᐳ OrderProcessingService
		}
		class DbConnection {
				<<class>>
			+DbConnection()
		}
		class IDbConnection {
			<<interface>>
		}
		class IOrderProcessingService {
			<<interface>>
		}
		class OrderProcessingService {
				<<class>>
			+OrderProcessingService(IDbConnection dbConnection)
		}
	}
```

