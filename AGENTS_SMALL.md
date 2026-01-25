This Markdown-formatted document contains information about working with Pure.DI

# Usage scenarios.

## Auto-bindings

Non-abstract types can be injected without any additional bindings.

```c#
using Pure.DI;

// Specifies to create a partial class named "Composition"
DI.Setup("Composition")
    // with the root "Orders"
    .Root<OrderService>("Orders");

var composition = new Composition();

// service = new OrderService(new Database())
var orders = composition.Orders;

class Database;

class OrderService(Database database);
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

> [!WARNING]
> This approach is not recommended if you follow the dependency inversion principle or need precise lifetime control.

Prefer injecting abstractions (for example, interfaces) and map them to implementations as in most [other examples](injections-of-abstractions.md).

## Injections of abstractions

This example shows the recommended approach: depend on abstractions and bind them to implementations.

```c#
using Pure.DI;

DI.Setup(nameof(Composition))
    // Binding abstractions to their implementations:
    // The interface IGpsSensor is bound to the implementation GpsSensor
    .Bind<IGpsSensor>().To<GpsSensor>()
    // The interface INavigationSystem is bound to the implementation NavigationSystem
    .Bind<INavigationSystem>().To<NavigationSystem>()

    // Specifies to create a composition root
    // of type "VehicleComputer" with the name "VehicleComputer"
    .Root<VehicleComputer>("VehicleComputer");

var composition = new Composition();

// Usage:
// var vehicleComputer = new VehicleComputer(new NavigationSystem(new GpsSensor()));
var vehicleComputer = composition.VehicleComputer;

vehicleComputer.StartTrip();

// The sensor abstraction
interface IGpsSensor;

// The sensor implementation
class GpsSensor : IGpsSensor;

// The service abstraction
interface INavigationSystem
{
    void Navigate();
}

// The service implementation
class NavigationSystem(IGpsSensor sensor) : INavigationSystem
{
    public void Navigate()
    {
        // Navigation logic using the sensor...
    }
}

// The consumer of the abstraction
partial class VehicleComputer(INavigationSystem navigationSystem)
{
    public void StartTrip() => navigationSystem.Navigate();
}
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

The binding chain maps abstractions to concrete types so the generator can build a fully concrete object graph. This keeps consumers decoupled and allows swapping implementations. A single implementation can satisfy multiple abstractions.
> [!TIP]
> If a binding is missing, injection still works when the consumer requests a concrete type (not an abstraction).


## Composition roots

This example shows several ways to create a composition root.
> [!TIP]
> There is no hard limit on roots, but prefer a small number. Ideally, an application has a single composition root.

In classic DI containers, the composition is resolved dynamically via calls like `T Resolve<T>()` or `object GetService(Type type)`. The root is simply the requested type, and you can have as many as you like. In Pure.DI, each root generates a property or method at compile time, so roots are explicit and defined via `Root(string rootName)`.

```c#
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<IInvoiceGenerator>().To<PdfInvoiceGenerator>()
    .Bind<IInvoiceGenerator>("Online").To<HtmlInvoiceGenerator>()
    .Bind<ILogger>().To<FileLogger>()

    // Specifies to create a regular composition root
    // of type "IInvoiceGenerator" with the name "InvoiceGenerator".
    // This will be the main entry point for invoice generation.
    .Root<IInvoiceGenerator>("InvoiceGenerator")

    // Specifies to create an anonymous composition root
    // that is only accessible from "Resolve()" methods.
    // This is useful for auxiliary types or testing.
    .Root<ILogger>()

    // Specifies to create a regular composition root
    // of type "IInvoiceGenerator" with the name "OnlineInvoiceGenerator"
    // using the "Online" tag to differentiate implementations.
    .Root<IInvoiceGenerator>("OnlineInvoiceGenerator", "Online");

var composition = new Composition();

// Resolves the default invoice generator (PDF) with all its dependencies
// invoiceGenerator = new PdfInvoiceGenerator(new FileLogger());
var invoiceGenerator = composition.InvoiceGenerator;

// Resolves the online invoice generator (HTML)
// onlineInvoiceGenerator = new HtmlInvoiceGenerator();
var onlineInvoiceGenerator = composition.OnlineInvoiceGenerator;

// All and only the roots of the composition
// can be obtained by Resolve method.
// Here we resolve the private root 'ILogger'.
var logger = composition.Resolve<ILogger>();

// We can also resolve tagged roots dynamically if needed
var tagged = composition.Resolve<IInvoiceGenerator>("Online");

// Common logger interface used across the system
interface ILogger;

// Concrete implementation of a logger that writes to a file
class FileLogger : ILogger;

// Abstract definition of an invoice generator
interface IInvoiceGenerator;

// Implementation for generating PDF invoices, dependent on ILogger
class PdfInvoiceGenerator(ILogger logger) : IInvoiceGenerator;

// Implementation for generating HTML invoices for online viewing
class HtmlInvoiceGenerator : IInvoiceGenerator;
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

The name of the composition root is arbitrarily chosen depending on its purpose but should be restricted by the property naming conventions in C# since it is the same name as a property in the composition class. In reality, the _Root_ property has the form:
```c#
public IService Root
{
  get
  {
    return new Service(new Dependency());
  }
}
```
To avoid generating _Resolve_ methods just add a comment `// Resolve = Off` before a _Setup_ method:
```c#
// Resolve = Off
DI.Setup("Composition")
  .Bind<IDependency>().To<Dependency>()
  ...
```
This can be done if these methods are not needed, in case only certain composition roots are used. It's not significant then, but it will help save resources during compilation.

## Resolve methods

This example shows how to resolve composition roots via `Resolve` methods, using the composition as a _Service Locator_. The `Resolve` methods are generated automatically.

```c#
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<IDevice>().To<Device>()
    .Bind<ISensor>().To<TemperatureSensor>()
    .Bind<ISensor>("Humidity").To<HumiditySensor>()

    // Specifies to create a private root
    // that is only accessible from _Resolve_ methods
    .Root<ISensor>()

    // Specifies to create a public root named _HumiditySensor_
    // using the "Humidity" tag
    .Root<ISensor>("HumiditySensor", "Humidity");

var composition = new Composition();

// The next 3 lines of code do the same thing:
var sensor1 = composition.Resolve<ISensor>();
var sensor2 = composition.Resolve(typeof(ISensor));
var sensor3 = composition.Resolve(typeof(ISensor), null);

// Resolve by "Humidity" tag
// The next 3 lines of code do the same thing too:
var humiditySensor1 = composition.Resolve<ISensor>("Humidity");
var humiditySensor2 = composition.Resolve(typeof(ISensor), "Humidity");
var humiditySensor3 = composition.HumiditySensor; // Resolve via the public root

interface IDevice;

class Device : IDevice;

interface ISensor;

class TemperatureSensor(IDevice device) : ISensor;

class HumiditySensor : ISensor;
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

_Resolve_ methods are similar to calling composition roots, which are properties (or methods). Roots are efficient and do not throw, so they are preferred. In contrast, _Resolve_ methods have drawbacks:
- They provide access to an unlimited set of dependencies (_Service Locator_).
- Their use can potentially lead to runtime exceptions. For example, when the corresponding root has not been defined.
- They are awkward for some UI binding scenarios (e.g., MAUI/WPF/Avalonia).

## Simplified binding

You can call `Bind()` without type parameters. It binds the implementation type itself, and if it is not abstract, all directly implemented abstract types except special ones.

```c#
using System.Collections;
using Pure.DI;

// Specifies to create a partial class "Composition"
DI.Setup(nameof(Composition))
    // Begins the binding definition for the implementation type itself,
    // and if the implementation is not an abstract class or structure,
    // for all abstract but NOT special types that are directly implemented.
    // Equivalent to:
    // .Bind<IOrderRepository, IOrderNotification, OrderManager>()
    //   .As(Lifetime.PerBlock)
    //   .To<OrderManager>()
    .Bind().As(Lifetime.PerBlock).To<OrderManager>()
    .Bind().To<Shop>()

    // Specifies to create a property "MyShop"
    .Root<IShop>("MyShop");

var composition = new Composition();
var shop = composition.MyShop;

interface IManager;

class ManagerBase : IManager;

interface IOrderRepository;

interface IOrderNotification;

class OrderManager :
    ManagerBase,
    IOrderRepository,
    IOrderNotification,
    IDisposable,
    IEnumerable<string>
{
    public void Dispose() {}

    public IEnumerator<string> GetEnumerator() =>
        new List<string> { "Order #1", "Order #2" }.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

interface IShop;

class Shop(
    OrderManager manager,
    IOrderRepository repository,
    IOrderNotification notification)
    : IShop;
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

In practice, most abstraction types can be inferred. The parameterless `Bind()` binds:

- the implementation type itself
- and, if it is NOT abstract,
  - all abstract types it directly implements
  - except special types

Special types will not be added to bindings:

- `System.Object`
- `System.Enum`
- `System.MulticastDelegate`
- `System.Delegate`
- `System.Collections.IEnumerable`
- `System.Collections.Generic.IEnumerable<T>`
- `System.Collections.Generic.IList<T>`
- `System.Collections.Generic.ICollection<T>`
- `System.Collections.IEnumerator`
- `System.Collections.Generic.IEnumerator<T>`
- `System.Collections.Generic.IReadOnlyList<T>`
- `System.Collections.Generic.IReadOnlyCollection<T>`
- `System.IDisposable`
- `System.IAsyncResult`
- `System.AsyncCallback`

If you want to add your own special type, use the `SpecialType<T>()` call.

For class `OrderManager`, `Bind().To<OrderManager>()` is equivalent to `Bind<IOrderRepository, IOrderNotification, OrderManager>().To<OrderManager>()`. The types `IDisposable` and `IEnumerable<string>` are excluded because they are special. `ManagerBase` is excluded because it is not abstract. `IManager` is excluded because it is not implemented directly by `OrderManager`.

|    |                       |                                                   |
|----|-----------------------|---------------------------------------------------|
| ✅ | `OrderManager`        | implementation type itself                        |
| ✅ | `IOrderRepository`    | directly implements                               |
| ✅ | `IOrderNotification`  | directly implements                               |
| ❌ | `IDisposable`         | special type                                      |
| ❌ | `IEnumerable<string>` | special type                                      |
| ❌ | `ManagerBase`         | non-abstract                                      |
| ❌ | `IManager`            | is not directly implemented by class OrderManager |

## Factory

This example shows manual creation and initialization. The generator usually infers dependencies from constructors, but sometimes you need custom creation or setup logic.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<IDatabaseService>().To<DatabaseService>(ctx => {
        // Some logic for creating an instance.
        // For example, we need to manually initialize the connection.
        ctx.Inject(out DatabaseService service);
        service.Connect();
        return service;
    })
    .Bind<IUserRegistry>().To<UserRegistry>()

    // Composition root
    .Root<IUserRegistry>("Registry");

var composition = new Composition();
var registry = composition.Registry;
registry.Database.IsConnected.ShouldBeTrue();

interface IDatabaseService
{
    bool IsConnected { get; }
}

class DatabaseService : IDatabaseService
{
    public bool IsConnected { get; private set; }

    // Simulates a connection establishment that must be called explicitly
    public void Connect() => IsConnected = true;
}

interface IUserRegistry
{
    IDatabaseService Database { get; }
}

class UserRegistry(IDatabaseService database) : IUserRegistry
{
    public IDatabaseService Database { get; } = database;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

There are scenarios where manual control over the creation process is required, such as
- When additional initialization logic is needed
- When complex construction steps are required
- When specific object states need to be set during creation

> [!IMPORTANT]
> The method `Inject()` cannot be used outside of the binding setup.

## Simplified factory

This example shows a simplified manual factory. Each lambda parameter represents an injected dependency, and starting with C# 10 you can add `Tag(...)` to specify a tagged dependency.

```c#
using Shouldly;
using Pure.DI;

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
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The example creates a service that depends on a logger initialized with a date-based file name. The `Tag` attribute enables named dependencies for more complex setups.

## Injection on demand

This example creates dependencies on demand using a factory delegate. The service (`GameLevel`) needs multiple instances of `IEnemy`, so it receives a `Func<IEnemy>` that can create new instances when needed.

```c#
using Shouldly;
using Pure.DI;
using System.Collections.Generic;

DI.Setup(nameof(Composition))
    .Bind().To<Enemy>()
    .Bind().To<GameLevel>()

    // Composition root
    .Root<IGameLevel>("GameLevel");

var composition = new Composition();
var gameLevel = composition.GameLevel;

// Verifies that two distinct enemies have been spawned
gameLevel.Enemies.Count.ShouldBe(2);

// Represents a game entity that acts as an enemy
interface IEnemy;

class Enemy : IEnemy;

// Represents a game level that manages entities
interface IGameLevel
{
    IReadOnlyList<IEnemy> Enemies { get; }
}

class GameLevel(Func<IEnemy> enemySpawner) : IGameLevel
{
    // The factory spawns a fresh enemy instance on each call.
    public IReadOnlyList<IEnemy> Enemies { get; } =
    [
        enemySpawner(),
        enemySpawner()
    ];
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Key elements:
- `Enemy` is bound to the `IEnemy` interface, and `GameLevel` is bound to `IGameLevel`.
- The `GameLevel` constructor accepts `Func<IEnemy>`, enabling deferred creation of entities.
- The `GameLevel` calls the factory twice, resulting in two distinct `Enemy` instances stored in its `Enemies` collection.

This approach lets factories control lifetime and instantiation timing. Pure.DI resolves a new `IEnemy` each time the factory is invoked.

## Injections on demand with arguments

This example uses a parameterized factory so dependencies can be created with runtime arguments. The service creates sensors with specific IDs at instantiation time.

```c#
using Shouldly;
using Pure.DI;
using System.Collections.Generic;

DI.Setup(nameof(Composition))
    .Bind().To<Sensor>()
    .Bind().To<SmartHome>()

    // Composition root
    .Root<ISmartHome>("SmartHome");

var composition = new Composition();
var smartHome = composition.SmartHome;
var sensors = smartHome.Sensors;

sensors.Count.ShouldBe(2);
sensors[0].Id.ShouldBe(101);
sensors[1].Id.ShouldBe(102);

interface ISensor
{
    int Id { get; }
}

class Sensor(int id) : ISensor
{
    public int Id { get; } = id;
}

interface ISmartHome
{
    IReadOnlyList<ISensor> Sensors { get; }
}

class SmartHome(Func<int, ISensor> sensorFactory) : ISmartHome
{
    public IReadOnlyList<ISensor> Sensors { get; } =
    [
        // Use the injected factory to create a sensor with ID 101
        sensorFactory(101),

        // Create another sensor with ID 102
        sensorFactory(102)
    ];
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Delayed dependency instantiation:
- Injection of dependencies requiring runtime parameters
- Creation of distinct instances with different configurations
- Type-safe resolution of dependencies with constructor arguments

## Transient

The _Transient_ lifetime specifies to create a new dependency instance each time. It is the default lifetime and can be omitted.

```c#
using Shouldly;
using Pure.DI;
using static Pure.DI.Lifetime;

DI.Setup(nameof(Composition))
    .Bind().As(Transient).To<Buffer>()
    .Bind().To<BatchProcessor>()
    .Root<IBatchProcessor>("Processor");

var composition = new Composition();
var processor = composition.Processor;

// Verify that input and output buffers are different instances.
// This is critical for the batch processor to avoid data corruption
// during reading. The Transient lifetime ensures a new instance
// is created for each dependency injection.
processor.Input.ShouldNotBe(processor.Output);

// Represents a memory buffer that should be unique for each operation
interface IBuffer;

class Buffer : IBuffer;

interface IBatchProcessor
{
    public IBuffer Input { get; }

    public IBuffer Output { get; }
}

class BatchProcessor(
    IBuffer input,
    IBuffer output)
    : IBatchProcessor
{
    public IBuffer Input { get; } = input;

    public IBuffer Output { get; } = output;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The _Transient_ lifetime is the safest and is used by default. Yes, its widespread use can cause a lot of memory traffic, but if there are doubts about thread safety, the _Transient_ lifetime is preferable because each consumer has its own instance of the dependency. The following nuances should be considered when choosing the _Transient_ lifetime:

- There will be unnecessary memory overhead that could be avoided.

- Every object created must be disposed of, and this will waste CPU resources, at least when the GC does its memory-clearing job.

- Poorly designed constructors can run slowly, perform functions that are not their own, and greatly hinder the efficient creation of compositions of multiple objects.

> [!IMPORTANT]
> The following very important rule, in my opinion, will help in the last point. Now, when a constructor is used to implement dependencies, it should not be loaded with other tasks. Accordingly, constructors should be free of all logic except for checking arguments and saving them for later use. Following this rule, even the largest compositions of objects will be built quickly.

## Singleton

The _Singleton_ lifetime ensures that there will be a single instance of the dependency for each composition.

```c#
using Shouldly;
using Pure.DI;
using System.Diagnostics.CodeAnalysis;
using static Pure.DI.Lifetime;

DI.Setup(nameof(Composition))
    // Bind the cache as Singleton to share it across all services
    .Bind().As(Singleton).To<Cache>()
    // Bind the order service as Transient (default) for per-request instances
    .Bind().To<OrderService>()
    .Root<IOrderService>("OrderService");

var composition = new Composition();
var orderService1 = composition.OrderService; // First order service instance
var orderService2 = composition.OrderService; // Second order service instance

// Verify that both services share the same cache instance (Singleton behavior)
orderService1.Cache.ShouldBe(orderService2.Cache);
// Simulate real-world usage: add data to cache via one service and check via another
orderService1.AddToCache("Order123", "Processed");
orderService2.GetFromCache("Order123").ShouldBe("Processed");

// Interface for a shared cache (e.g., for storing order statuses)
interface ICache
{
    void Add(string key, string value);

    bool TryGet(string key, [MaybeNullWhen(false)] out string value);
}

// Implementation of a simple in-memory cache (must be thread-safe in real apps)
class Cache : ICache
{
    private readonly Dictionary<string, string> _data = new();

    public void Add(string key, string value) =>
        _data[key] = value;

    public bool TryGet(string key, [MaybeNullWhen(false)] out string value) =>
        _data.TryGetValue(key, out value);
}

// Interface for order processing service
interface IOrderService
{
    ICache Cache { get; }

    void AddToCache(string orderId, string status);

    string GetFromCache(string orderId);
}

// Order service that uses the shared cache
class OrderService(ICache cache) : IOrderService
{
    // The cache is injected and shared (Singleton)
    public ICache Cache { get; } = cache;

    // Real-world method: add order status to cache
    public void AddToCache(string orderId, string status) =>
        Cache.Add(orderId, status);

    // Real-world method: retrieve order status from cache
    public string GetFromCache(string orderId) =>
        Cache.TryGet(orderId, out var status) ? status : "unknown";
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Some articles advise using objects with a _Singleton_ lifetime as often as possible, but the following details must be considered:

- For .NET the default behavior is to create a new instance of the type each time it is needed, other behavior requires, additional logic that is not free and requires additional resources.

- The use of _Singleton_, adds a requirement for thread-safety controls on their use, since singletons are more likely to share their state between different threads without even realizing it.

- The thread-safety control should be automatically extended to all dependencies that _Singleton_ uses, since their state is also now shared.

- Logic for thread-safety control can be resource-costly, error-prone, interlocking, and difficult to test.

- _Singleton_ can retain dependency references longer than their expected lifetime, this is especially significant for objects that hold "non-renewable" resources, such as the operating system Handler.

- Sometimes additional logic is required to dispose of _Singleton_.

## PerResolve

The _PerResolve_ lifetime ensures that there will be one instance of the dependency for each composition root instance.

```c#
using Shouldly;
using Pure.DI;
using static Pure.DI.Lifetime;

DI.Setup(nameof(Composition))
    // PerResolve = one "planning session" per root access.
    // Imagine: each time you ask for a plan, you get a fresh context.
    .Bind().As(PerResolve).To<RoutePlanningSession>()

    // Singleton = created once per Composition instance.
    // Here it intentionally captures session when it's created the first time
    // (this is a realistic pitfall: singleton accidentally holds request-scoped state).
    .Bind().As(Singleton).To<(IRoutePlanningSession s3, IRoutePlanningSession s4)>()

    // Composition root
    .Root<TrainTripPlanner>("Planner");

var composition = new Composition();

// First "user request": plan a trip now
var plan1 = composition.Planner;

// In the same request, PerResolve dependencies are the same instance:
plan1.SessionForOutbound.ShouldBe(plan1.SessionForReturn);

// Tuple is Singleton, so both entries are the same captured instance:
plan1.CapturedSessionA.ShouldBe(plan1.CapturedSessionB);

// Because the singleton tuple was created during the first request,
// it captured THAT request's PerResolve session:
plan1.SessionForOutbound.ShouldBe(plan1.CapturedSessionA);

// Second "user request": plan another trip (new root access)
var plan2 = composition.Planner;

// New request => new PerResolve session:
plan2.SessionForOutbound.ShouldNotBe(plan1.SessionForOutbound);

// But the singleton still holds the old captured session from the first request:
plan2.CapturedSessionA.ShouldBe(plan1.CapturedSessionA);
plan2.SessionForOutbound.ShouldNotBe(plan2.CapturedSessionA);

// A request-scoped context: e.g., contains "now", locale, pricing rules version,
// feature flags, etc. You typically want a new one per route planning request.
interface IRoutePlanningSession;

class RoutePlanningSession : IRoutePlanningSession;

// A service that plans a train trip.
// It asks for two session instances to demonstrate PerResolve:
// both should be the same within a single request.
class TrainTripPlanner(
    IRoutePlanningSession sessionForOutbound,
    IRoutePlanningSession sessionForReturn,
    (IRoutePlanningSession capturedA, IRoutePlanningSession capturedB) capturedSessions)
{
    public IRoutePlanningSession SessionForOutbound { get; } = sessionForOutbound;

    public IRoutePlanningSession SessionForReturn { get; } = sessionForReturn;

    // These come from a singleton tuple - effectively "global cached" instances.
    public IRoutePlanningSession CapturedSessionA { get; } = capturedSessions.capturedA;

    public IRoutePlanningSession CapturedSessionB { get; } = capturedSessions.capturedB;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)


## PerBlock

The _PerBlock_ lifetime does not guarantee that there will be a single dependency instance for each instance of the composition root (as for the _PerResolve_ lifetime), but is useful for reducing the number of instances of a type.

```c#
using Shouldly;
using Pure.DI;
using static Pure.DI.Lifetime;

DI.Setup(nameof(Composition))
    // Bind DatabaseConnection with PerBlock lifetime:
    // Ensures a single connection per composition root (e.g., per user request),
    // but a new one for each new root - useful for batch operations without full singleton overhead.
    .Bind().As(PerBlock).To<DatabaseConnection>()
    // Bind a tuple of two connections as Singleton:
    // This shares the same connection globally, simulating a cached or shared resource.
    .Bind().As(Singleton).To<(IDatabaseConnection conn3, IDatabaseConnection conn4)>()

    // Composition root - represents the main service entry point.
    .Root<OrderRepository>("Repository");

var composition = new Composition();

// Simulate the first user request or batch operation
var repository1 = composition.Repository;
repository1.ProcessOrder("ORD-2025-54546");

// Check that within one repository (one block), connections are shared for consistency
repository1.PrimaryConnection.ShouldBe(repository1.SecondaryConnection);
repository1.OtherConnection.ShouldBe(repository1.FallbackConnection);
repository1.PrimaryConnection.ShouldBe(repository1.OtherConnection);

// Simulate the second user request or batch - should have a new PerBlock connection
var repository2 = composition.Repository;
repository2.PrimaryConnection.ShouldNotBe(repository1.PrimaryConnection);

// Interface for database connection - in a real world, this could handle SQL queries
interface IDatabaseConnection;

// Implementation of database connection - transient-like but controlled by lifetime
class DatabaseConnection : IDatabaseConnection;

// Repository for handling orders, injecting multiple connections for demonstration
// In real-world, this could process orders in a batch, sharing connection within the batch
class OrderRepository(
    IDatabaseConnection primaryConnection,
    IDatabaseConnection secondaryConnection,
    (IDatabaseConnection otherConnection, IDatabaseConnection fallbackConnection) additionalConnections)
{
    // Public properties for connections - in practice, these would be private and used in methods
    public IDatabaseConnection PrimaryConnection { get; } = primaryConnection;

    public IDatabaseConnection SecondaryConnection { get; } = secondaryConnection;

    public IDatabaseConnection OtherConnection { get; } = additionalConnections.otherConnection;

    public IDatabaseConnection FallbackConnection { get; } = additionalConnections.fallbackConnection;

    // Example real-world method: Process an order using the shared connection
    public void ProcessOrder(string orderId)
    {
        // Use PrimaryConnection to query database, e.g.,
        // "SELECT * FROM Orders WHERE Id = @orderId"
    }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

