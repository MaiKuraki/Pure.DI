This Markdown-formatted document contains information about working with Pure.DI

# Usage scenarios.

## Auto-bindings

Auto-bindings are implicit bindings for non-abstract types, so the generator can build an object graph without explicit registrations.
Pros: fast to get started and low ceremony in small demos or spikes.
Cons: weaker adherence to dependency inversion and less explicit lifetime control, which can make larger graphs harder to reason about.
Recommendation: prefer explicit bindings for abstractions in production code and use auto-bindings sparingly for simple leaf types.
In this example, a composition is created with only a root; the `OrderService` and `Database` types are resolved implicitly.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Injections of abstractions

This example shows the recommended approach: depend on abstractions and bind them to implementations.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Composition roots

This example shows several ways to create a composition root.
> [!TIP]
> There is no hard limit on roots, but prefer a small number. Ideally, an application has a single composition root.

In classic DI containers, the composition is resolved dynamically via calls like `T Resolve<T>()` or `object GetService(Type type)`. The root is simply the requested type, and you can have as many as you like. In Pure.DI, each root generates a property or method at compile time, so roots are explicit and defined via `Root(string rootName)`.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Resolve methods

This example shows how to resolve composition roots via `Resolve` methods, using the composition as a _Service Locator_. The `Resolve` methods are generated automatically.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Simplified binding

You can call `Bind()` without type parameters. It binds the implementation type itself, and if it is not abstract, all directly implemented abstract types except special ones.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Factory

This example shows manual creation and initialization. The generator usually infers dependencies from constructors, but sometimes you need custom creation or setup logic.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Simplified factory

This example shows a simplified manual factory. Each lambda parameter represents an injected dependency, and starting with C# 10 you can add `Tag(...)` to specify a tagged dependency.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Injection on demand

This example creates dependencies on demand using a factory delegate. The service (`GameLevel`) needs multiple instances of `IEnemy`, so it receives a `Func<IEnemy>` that can create new instances when needed.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Injections on demand with arguments

This example uses a parameterized factory so dependencies can be created with runtime arguments. The service creates sensors with specific IDs at instantiation time.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Composition arguments

Use composition arguments when you need to pass state into the composition. Define them with `Arg<T>(string argName)` (optionally with tags) and use them like any other dependency. Only arguments that are used in the object graph become constructor parameters.
> [!NOTE]
> Actually, composition arguments work like normal bindings. The difference is that they bind to the values of the arguments. These values will be injected wherever they are required.

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<IBankGateway>().To<BankGateway>()
    .Bind<IPaymentProcessor>().To<PaymentProcessor>()

    // Composition root "PaymentService"
    .Root<IPaymentProcessor>("PaymentService")

    // Composition argument: Connection timeout (e.g., from config)
    .Arg<int>("timeoutSeconds")

    // Composition argument: API Token (using a tag to distinguish from other strings)
    .Arg<string>("authToken", "api token")

    // Composition argument: Bank gateway address
    .Arg<string>("gatewayUrl");

// Create the composition, passing real settings from outside
var composition = new Composition(
    timeoutSeconds: 30,
    authToken: "secret_token_123",
    gatewayUrl: "https://api.bank.com/v1");

var paymentService = composition.PaymentService;

paymentService.Token.ShouldBe("secret_token_123");
paymentService.Gateway.Timeout.ShouldBe(30);
paymentService.Gateway.Url.ShouldBe("https://api.bank.com/v1");

interface IBankGateway
{
    int Timeout { get; }

    string Url { get; }
}

// Simulation of a bank gateway client
class BankGateway(int timeoutSeconds, string gatewayUrl) : IBankGateway
{
    public int Timeout { get; } = timeoutSeconds;

    public string Url { get; } = gatewayUrl;
}

interface IPaymentProcessor
{
    string Token { get; }

    IBankGateway Gateway { get; }
}

// Payment processing service
class PaymentProcessor(
    // The tag allows specifying exactly which string to inject here
    [Tag("api token")] string token,
    IBankGateway gateway) : IPaymentProcessor
{
    public string Token { get; } = token;

    public IBankGateway Gateway { get; } = gateway;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Root arguments

Use root arguments when you need to pass state into a specific root. Define them with `RootArg<T>(string argName)` (optionally with tags) and use them like any other dependency. A root that uses at least one root argument becomes a method, and only arguments used in that root's object graph appear in the method signature. Use unique argument names to avoid collisions.
> [!NOTE]
> Actually, root arguments work like normal bindings. The difference is that they bind to the values of the arguments. These values will be injected wherever they are required.

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using static Pure.DI.Tag;

DI.Setup(nameof(Composition))
    // Disable Resolve methods because root arguments are not compatible
    .Hint(Hint.Resolve, "Off")
    .Bind<IDatabaseService>().To<DatabaseService>()
    .Bind<IApplication>().To<Application>()

    // Root arguments serve as values passed
    // to the composition root method
    .RootArg<int>("port")
    .RootArg<string>("connectionString")

    // An argument can be tagged
    // to be injectable by type and this tag
    .RootArg<string>("appName", AppDetail)

    // Composition root
    .Root<IApplication>("CreateApplication");

var composition = new Composition();

// Creates an application with specific arguments
var app = composition.CreateApplication(
    appName: "MySuperApp",
    port: 8080,
    connectionString: "Server=.;Database=MyDb;");

app.Name.ShouldBe("MySuperApp");
app.Database.Port.ShouldBe(8080);
app.Database.ConnectionString.ShouldBe("Server=.;Database=MyDb;");

interface IDatabaseService
{
    int Port { get; }

    string ConnectionString { get; }
}

class DatabaseService(int port, string connectionString) : IDatabaseService
{
    public int Port { get; } = port;

    public string ConnectionString { get; } = connectionString;
}

interface IApplication
{
    string Name { get; }

    IDatabaseService Database { get; }
}

class Application(
    [Tag(AppDetail)] string name,
    IDatabaseService database)
    : IApplication
{
    public string Name { get; } = name;

    public IDatabaseService Database { get; } = database;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

When using root arguments, compilation warnings are emitted if `Resolve` methods are generated because these methods cannot create such roots. Disable `Resolve` via `Hint(Hint.Resolve, "Off")`, or ignore the warnings and accept the risks.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Tags

Tags let you control dependency selection when multiple implementations exist:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

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
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The example shows how to:
- Define multiple bindings for the same interface
- Use tags to differentiate between implementations
- Control lifetime management
- Inject tagged dependencies into constructors

The tag can be a constant, a type, a [smart tag](smart-tags.md), or a value of an `Enum` type. The _default_ and _null_ tags are also supported.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Smart tags

Large object graphs often need many tags. String tags are error-prone and easy to mistype. Prefer `Enum` values as tags, and Pure.DI helps make this safe.

When the compiler cannot determine a tag value, Pure.DI generates a constant inside `Pure.DI.Tag`. For the example below, the generated constants would look like this:

```c#
namespace Pure.DI
{
  internal partial class Tag
  {
    public const string Abc = "Abc";
    public const string Xyz = "Xyz";
  }
}
```
This enables safe refactoring and compiler-checked tag usage, reducing errors.

![](smart_tags.gif)

The example below also uses the `using static Pure.DI.Tag;` directive to access tags in `Pure.DI.Tag` without specifying a type name:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

using static Pure.DI.Tag;
using static Pure.DI.Lifetime;

DI.Setup(nameof(Composition))
    // The `default` tag is used to resolve dependencies
    // when the tag was not specified by the consumer
    .Bind<IMessageSender>(Email, default).To<EmailSender>()
    .Bind<IMessageSender>(Sms).As(Singleton).To<SmsSender>()
    .Bind<IMessagingService>().To<MessagingService>()

    // "SmsSenderRoot" is root name, Sms is tag
    .Root<IMessageSender>("SmsSenderRoot", Sms)

    // Specifies to create the composition root named "Root"
    .Root<IMessagingService>("MessagingService");

var composition = new Composition();
var messagingService = composition.MessagingService;
messagingService.EmailSender.ShouldBeOfType<EmailSender>();
messagingService.SmsSender.ShouldBeOfType<SmsSender>();
messagingService.SmsSender.ShouldBe(composition.SmsSenderRoot);
messagingService.DefaultSender.ShouldBeOfType<EmailSender>();

interface IMessageSender;

class EmailSender : IMessageSender;

class SmsSender : IMessageSender;

interface IMessagingService
{
    IMessageSender EmailSender { get; }

    IMessageSender SmsSender { get; }

    IMessageSender DefaultSender { get; }
}

class MessagingService(
    [Tag(Email)] IMessageSender emailSender,
    [Tag(Sms)] IMessageSender smsSender,
    IMessageSender defaultSender)
    : IMessagingService
{
    public IMessageSender EmailSender { get; } = emailSender;

    public IMessageSender SmsSender { get; } = smsSender;

    public IMessageSender DefaultSender { get; } = defaultSender;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Simplified lifetime-specific bindings

You can use the `Transient<>()`, `Singleton<>()`, `PerResolve<>()`, etc. methods. In this case binding will be performed for the implementation type itself, and if the implementation is not an abstract type or structure, for all abstract but NOT special types that are directly implemented.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using System.Collections;
using Pure.DI;

// Specifies to create a partial class "Composition"
DI.Setup(nameof(Composition))
    // The equivalent of the following:
    // .Bind<IOrderRepository, IOrderNotification, OrderManager>()
    //   .As(Lifetime.PerBlock)
    //   .To<OrderManager>()
    .PerBlock<OrderManager>()
    // The equivalent of the following:
    // .Bind<IShop, Shop>()
    //   .As(Lifetime.Transient)
    //   .To<Shop>()
    // .Bind<IOrderNameFormatter, OrderNameFormatter>()
    //   .As(Lifetime.Transient)
    //   .To<OrderNameFormatter>()
    .Transient<Shop, OrderNameFormatter>()

    // Specifies to create a property "MyShop"
    .Root<IShop>("MyShop");

var composition = new Composition();
var shop = composition.MyShop;

interface IManager;

class ManagerBase : IManager;

interface IOrderRepository;

interface IOrderNotification;

class OrderManager(IOrderNameFormatter orderNameFormatter) :
    ManagerBase,
    IOrderRepository,
    IOrderNotification,
    IDisposable,
    IEnumerable<string>
{
    public void Dispose() {}

    public IEnumerator<string> GetEnumerator() =>
        new List<string>
        {
            orderNameFormatter.Format(1),
            orderNameFormatter.Format(2)
        }.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

interface IOrderNameFormatter
{
    string Format(int orderId);
}

class OrderNameFormatter : IOrderNameFormatter
{
    public string Format(int orderId) => $"Order #{orderId}";
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

These methods perform the binding with appropriate lifetime:

- with the implementation type itself
- and if it is NOT an abstract type or structure
  - with all abstract types that it directly implements
  - exceptions are special types

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

For class `OrderManager`, the `PerBlock<OrderManager>()` binding will be equivalent to the `Bind<IOrderRepository, IOrderNotification, OrderManager>().As(Lifetime.PerBlock).To<OrderManager>()` binding. The types `IDisposable`, `IEnumerable<string>` did not get into the binding because they are special from the list above. `ManagerBase` did not get into the binding because it is not abstract. `IManager` is not included because it is not implemented directly by class `OrderManager`.

|    |                       |                                                   |
|----|-----------------------|---------------------------------------------------|
| ✅ | `OrderManager`        | implementation type itself                        |
| ✅ | `IOrderRepository`    | directly implements                               |
| ✅ | `IOrderNotification`  | directly implements                               |
| ❌ | `IDisposable`         | special type                                      |
| ❌ | `IEnumerable<string>` | special type                                      |
| ❌ | `ManagerBase`         | non-abstract                                      |
| ❌ | `IManager`            | is not directly implemented by class OrderManager |
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Simplified lifetime-specific factory

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

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
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Build up of an existing object

This example shows the Build-Up pattern in dependency injection, where an existing object is injected with necessary dependencies through its properties, methods, or fields.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .RootArg<string>("name")
    .Bind().To(Guid.NewGuid)
    .Bind().To(ctx => {
        var person = new Person();
        // Injects dependencies into an existing object
        ctx.BuildUp(person);
        return person;
    })
    .Bind().To<Greeter>()

    // Composition root
    .Root<IGreeter>("GetGreeter");

var composition = new Composition();
var greeter = composition.GetGreeter("Nik");

greeter.Person.Name.ShouldBe("Nik");
greeter.Person.Id.ShouldNotBe(Guid.Empty);

interface IPerson
{
    string Name { get; }

    Guid Id { get; }
}

class Person : IPerson
{
    // The Dependency attribute specifies to perform an injection and its order
    [Dependency] public string Name { get; set; } = "";

    public Guid Id { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection and its order
    [Dependency] public void SetId(Guid id) => Id = id;
}

interface IGreeter
{
    IPerson Person { get; }
}

record Greeter(IPerson Person) : IGreeter;
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Key Concepts:
**Build-Up** - injecting dependencies into an already created object
**Dependency Attribute** - marker for identifying injectable members
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Builder

Sometimes you need to build up an existing composition root and inject all of its dependencies, in which case the `Builder` method will be useful, as in the example below:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().To(Guid.NewGuid)
    .Bind().To<PhotonBlaster>()
    .Builder<Player>("Equip");

var composition = new Composition();

// The Game Engine instantiates the Player entity,
// so we need to inject dependencies into the existing instance.
var player = composition.Equip(new Player());

player.Id.ShouldNotBe(Guid.Empty);
player.Weapon.ShouldBeOfType<PhotonBlaster>();

interface IWeapon;

class PhotonBlaster : IWeapon;

interface IGameEntity
{
    Guid Id { get; }

    IWeapon? Weapon { get; }
}

record Player : IGameEntity
{
    public Guid Id { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public IWeapon? Weapon { get; set; }

    [Dependency]
    public void SetId(Guid id) => Id = id;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Important Notes:
- The default builder method name is `BuildUp`
- The first argument to the builder method is always the instance to be built

Advantages:
- Allows working with pre-existing objects
- Provides flexibility in dependency injection
- Supports partial injection of dependencies
- Can be used with legacy code

Use Cases:
- When objects are created outside the DI container
- For working with third-party libraries
- When migrating existing code to DI
- For complex object graphs where full construction is not feasible
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Builder with arguments

This example shows how to use builders with custom arguments in dependency injection. It shows how to pass additional parameters during the build-up process.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .RootArg<Guid>("id")
    .Bind().To<TelemetrySystem>()
    .Builder<Satellite>("Initialize");

var composition = new Composition();

var id = Guid.NewGuid();
var satellite = composition.Initialize(new Satellite(), id);
satellite.Id.ShouldBe(id);
satellite.Telemetry.ShouldBeOfType<TelemetrySystem>();

interface ITelemetrySystem;

class TelemetrySystem : ITelemetrySystem;

interface ISatellite
{
    Guid Id { get; }

    ITelemetrySystem? Telemetry { get; }
}

record Satellite : ISatellite
{
    public Guid Id { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public ITelemetrySystem? Telemetry { get; set; }

    [Dependency]
    public void SetId(Guid id) => Id = id;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Important Notes:
- The default builder method name is `BuildUp`
- The first argument to the builder method is always the instance to be built
- Additional arguments are passed in the order they are defined in the setup
- Root arguments can be used to provide custom values during build-up

Use Cases:
- When additional parameters are required during object construction
- For scenarios where dependencies depend on runtime values
- When specific initialization data is needed
- For conditional injection based on provided arguments

Best Practices
- Keep the number of builder arguments minimal
- Use meaningful names for root arguments
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Builders

Sometimes you need builders for all types derived from `T` that are known at compile time.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().To(Guid.NewGuid)
    .Bind().To<PlutoniumBattery>()
    // Creates a builder for each type inherited from IRobot.
    // These types must be available at this point in the code.
    .Builders<IRobot>("BuildUp");

var composition = new Composition();

var cleaner = composition.BuildUp(new CleanerBot());
cleaner.Token.ShouldNotBe(Guid.Empty);
cleaner.Battery.ShouldBeOfType<PlutoniumBattery>();

var guard = composition.BuildUp(new GuardBot());
guard.Token.ShouldBe(Guid.Empty);
guard.Battery.ShouldBeOfType<PlutoniumBattery>();

// Uses a common method to build an instance
IRobot robot = new CleanerBot();
robot = composition.BuildUp(robot);
robot.ShouldBeOfType<CleanerBot>();
robot.Token.ShouldNotBe(Guid.Empty);
robot.Battery.ShouldBeOfType<PlutoniumBattery>();

interface IBattery;

class PlutoniumBattery : IBattery;

interface IRobot
{
    Guid Token { get; }

    IBattery? Battery { get; }
}

record CleanerBot : IRobot
{
    public Guid Token { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public IBattery? Battery { get; set; }

    [Dependency]
    public void SetToken(Guid token) => Token = token;
}

record GuardBot : IRobot
{
    public Guid Token => Guid.Empty;

    [Dependency]
    public IBattery? Battery { get; set; }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Important Notes:
- The default builder method name is `BuildUp`
- The first argument to the builder method is always the instance to be built
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Builders with a name template

Sometimes you need to build up an existing composition root and inject all of its dependencies, in which case the `Builder` method will be useful, as in the example below:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().To(Guid.NewGuid)
    .Bind().To<WiFi>()
    // Creates a builder based on the name template
    // for each type inherited from IDevice.
    // These types must be available at this point in the code.
    .Builders<IDevice>("Install{type}");

var composition = new Composition();

var webcam = composition.InstallWebcam(new Webcam());
webcam.Id.ShouldNotBe(Guid.Empty);
webcam.Network.ShouldBeOfType<WiFi>();

var thermostat = composition.InstallThermostat(new Thermostat());
thermostat.Id.ShouldBe(Guid.Empty);
thermostat.Network.ShouldBeOfType<WiFi>();

// Uses a common method to build an instance
IDevice device = new Webcam();
device = composition.InstallIDevice(device);
device.ShouldBeOfType<Webcam>();
device.Id.ShouldNotBe(Guid.Empty);
device.Network.ShouldBeOfType<WiFi>();

interface INetwork;

class WiFi : INetwork;

interface IDevice
{
    Guid Id { get; }

    INetwork? Network { get; }
}

record Webcam : IDevice
{
    public Guid Id { get; private set; } = Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public INetwork? Network { get; set; }

    [Dependency]
    public void SetId(Guid id) => Id = id;
}

record Thermostat : IDevice
{
    public Guid Id => Guid.Empty;

    // The Dependency attribute specifies to perform an injection
    [Dependency]
    public INetwork? Network { get; set; }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The default builder method name is `BuildUp`. The first argument to this method will always be the instance to be built.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Field injection

To use dependency injection for a field, make sure the field is writable and simply add the _Ordinal_ attribute to that field, specifying an ordinal that will be used to determine the injection order:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<ICoffeeMachine>().To<CoffeeMachine>()
    .Bind<ISmartKitchen>().To<SmartKitchen>()

    // Composition root
    .Root<ISmartKitchen>("Kitchen");

var composition = new Composition();
var kitchen = composition.Kitchen;
kitchen.CoffeeMachine.ShouldBeOfType<CoffeeMachine>();

interface ICoffeeMachine;

class CoffeeMachine : ICoffeeMachine;

interface ISmartKitchen
{
    ICoffeeMachine? CoffeeMachine { get; }
}

class SmartKitchen : ISmartKitchen
{
    // The Dependency attribute specifies to perform an injection.
    // The container will automatically assign a value to this field
    // when creating the SmartKitchen instance.
    [Dependency]
    public ICoffeeMachine? CoffeeMachineImpl;

    // Expose the injected dependency through a public property
    public ICoffeeMachine? CoffeeMachine => CoffeeMachineImpl;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The key points are:
- The field must be writable
- The `Dependency` (or `Ordinal`) attribute is used to mark the field for injection
- The container automatically injects the dependency when resolving the object graph
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Method injection

To use dependency implementation for a method, simply add the _Ordinal_ attribute to that method, specifying the sequence number that will be used to define the call to that method:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<IMap>().To<Map>()
    .Bind<INavigator>().To<Navigator>()

    // Composition root
    .Root<INavigator>("Navigator");

var composition = new Composition();
var navigator = composition.Navigator;
navigator.CurrentMap.ShouldBeOfType<Map>();

interface IMap;

class Map : IMap;

interface INavigator
{
    IMap? CurrentMap { get; }
}

class Navigator : INavigator
{
    // The Dependency attribute specifies that the container should call this method
    // to inject the dependency.
    [Dependency]
    public void LoadMap(IMap map) =>
        CurrentMap = map;

    public IMap? CurrentMap { get; private set; }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The key points are:
- The method must be available to be called from a composition class
- The `Dependency` (or `Ordinal`) attribute is used to mark the method for injection
- The container automatically calls the method to inject dependencies
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Property injection

To use dependency injection on a property, make sure the property is writable and simply add the _Ordinal_ attribute to that property, specifying the ordinal that will be used to determine the injection order:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<ILogger>().To<ConsoleLogger>()
    .Bind<IService>().To<Service>()

    // Composition root
    .Root<IService>("MyService");

var composition = new Composition();
var service = composition.MyService;
service.Logger.ShouldBeOfType<ConsoleLogger>();

interface ILogger;

class ConsoleLogger : ILogger;

interface IService
{
    ILogger? Logger { get; }
}

class Service : IService
{
    // The Dependency attribute specifies to perform an injection,
    // the integer value in the argument specifies
    // the ordinal of injection.
    // Usually, property injection is used for optional dependencies.
    [Dependency] public ILogger? Logger { get; set; }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The key points are:
- The property must be writable
- The `Dependency` (or `Ordinal`) attribute is used to mark the property for injection
- The container automatically injects the dependency when resolving the object graph
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Default values

This example shows how to use default values in dependency injection when explicit injection is not possible.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<ISensor>().To<MotionSensor>()
    .Bind<ISecuritySystem>().To<SecuritySystem>()

    // Composition root
    .Root<ISecuritySystem>("SecuritySystem");

var composition = new Composition();
var securitySystem = composition.SecuritySystem;
securitySystem.Sensor.ShouldBeOfType<MotionSensor>();
securitySystem.SystemName.ShouldBe("Home Guard");

interface ISensor;

class MotionSensor : ISensor;

interface ISecuritySystem
{
    string SystemName { get; }

    ISensor Sensor { get; }
}

// If injection cannot be performed explicitly,
// the default value will be used
class SecuritySystem(string systemName = "Home Guard") : ISecuritySystem
{
    public string SystemName { get; } = systemName;

    // The 'required' modifier ensures that the property is set during initialization.
    // The default value 'new MotionSensor()' provides a fallback
    // if no dependency is injected.
    public required ISensor Sensor { get; init; } = new MotionSensor();
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The key points are:
- Default constructor arguments can be used for simple values
- The DI container will use these defaults if no explicit bindings are provided

This example shows how to handle default values in a dependency injection scenario:
- **Constructor Default Argument**: The `SecuritySystem` class has a constructor with a default value for the name parameter. If no value is provided, "Home Guard" will be used.
- **Required Property with Default**: The `Sensor` property is marked as required but has a default instantiation. This ensures that:
  - The property must be set
  - If no explicit injection occurs, a default value will be used
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Required properties or fields

This example shows how the `required` modifier can be used to automatically inject dependencies into properties and fields. When a property or field is marked with `required`, the DI will automatically inject the dependency without additional effort.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Arg<string>("connectionString")
    .Bind<IDatabase>().To<SqlDatabase>()
    .Bind<IUserRepository>().To<UserRepository>()

    // Composition root
    .Root<IUserRepository>("Repository");

var composition = new Composition(connectionString: "Server=.;Database=MyDb;");
var repository = composition.Repository;

repository.Database.ShouldBeOfType<SqlDatabase>();
repository.ConnectionString.ShouldBe("Server=.;Database=MyDb;");

interface IDatabase;

class SqlDatabase : IDatabase;

interface IUserRepository
{
    string ConnectionString { get; }

    IDatabase Database { get; }
}

class UserRepository : IUserRepository
{
    // The required field will be injected automatically.
    // In this case, it gets the value from the composition argument
    // of type 'string'.
    public required string ConnectionStringField;

    public string ConnectionString => ConnectionStringField;

    // The required property will be injected automatically
    // without additional effort.
    public required IDatabase Database { get; init; }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

This approach simplifies dependency injection by eliminating the need to manually configure bindings for required dependencies, making the code more concise and easier to maintain.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Overrides

This example shows advanced dependency injection techniques using Pure.DI's override mechanism to customize dependency instantiation with runtime arguments and tagged parameters. The implementation creates multiple `IDependency` instances with values manipulated through explicit overrides.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using System.Collections.Immutable;
using System.Drawing;

DI.Setup(nameof(Composition))
    .Bind(Tag.Red).To(() => Color.Red)
    .Bind().As(Lifetime.Singleton).To<Clock>()
    // The factory accepts the widget ID and the layer index
    .Bind().To<Func<int, int, IWidget>>(ctx =>
        (widgetId, layerIndex) => {
            // Overrides the 'id' argument of the constructor with the first lambda argument
            ctx.Override(widgetId);

            // Overrides the 'layer' tagged argument of the constructor with the second lambda argument
            ctx.Override(layerIndex, "layer");

            // Overrides the 'name' argument with a formatted string
            ctx.Override($"Widget {widgetId} on layer {layerIndex}");

            // Resolves the 'Color' dependency tagged with 'Red'
            ctx.Inject(Tag.Red, out Color color);
            // Overrides the 'color' argument with the resolved value
            ctx.Override(color);

            // Creates the instance using the overridden values
            ctx.Inject<Widget>(out var widget);
            return widget;
        })
    .Bind().To<Dashboard>()

    // Composition root
    .Root<IDashboard>("Dashboard");

var composition = new Composition();
var dashboard = composition.Dashboard;
dashboard.Widgets.Length.ShouldBe(3);

dashboard.Widgets[0].Id.ShouldBe(0);
dashboard.Widgets[0].Layer.ShouldBe(99);
dashboard.Widgets[0].Name.ShouldBe("Widget 0 on layer 99");

dashboard.Widgets[1].Id.ShouldBe(1);
dashboard.Widgets[1].Name.ShouldBe("Widget 1 on layer 99");

dashboard.Widgets[2].Id.ShouldBe(2);
dashboard.Widgets[2].Name.ShouldBe("Widget 2 on layer 99");

interface IClock
{
    DateTimeOffset Now { get; }
}

class Clock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}

interface IWidget
{
    string Name { get; }

    int Id { get; }

    int Layer { get; }
}

class Widget(
    string name,
    IClock clock,
    int id,
    [Tag("layer")] int layer,
    Color color)
    : IWidget
{
    public string Name => name;

    public int Id => id;

    public int Layer => layer;
}

interface IDashboard
{
    ImmutableArray<IWidget> Widgets { get; }
}

class Dashboard(Func<int, int, IWidget> widgetFactory) : IDashboard
{
    public ImmutableArray<IWidget> Widgets { get; } =
    [
        widgetFactory(0, 99),
        widgetFactory(1, 99),
        widgetFactory(2, 99)
    ];
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Root binding

In general, it is recommended to define one composition root for the entire application. But Sometimes you need to have multiple roots. To simplify the definition of composition roots, a "hybrid" API method `RootBind<T>(string rootName)` was added. It lets you define a binding and at the same time the root of the composition. You can it in order to reduce repetitions. The registration `composition.RootBind<IDependency>().To<Dependency>()` is an equivalent to `composition.Bind<IDependency>().To<Dependency>().Root<IDependency>()`.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().As(Lifetime.Singleton).To<DbConnection>()
    // RootBind lets you define a binding and a composition root
    // simultaneously. This is useful for creating public entry points
    // for your application components while keeping the configuration concise.
    .RootBind<IOrderService>("OrderService").To<OrderService>();

// The line above is functionally equivalent to:
//  .Bind<IOrderService>().To<OrderService>()
//  .Root<IOrderService>("OrderService")

var composition = new Composition();
var orderService = composition.OrderService;
orderService.ShouldBeOfType<OrderService>();

interface IDbConnection;

class DbConnection : IDbConnection;

interface IOrderService;

class OrderService(IDbConnection connection) : IOrderService;
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Static root

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().As(Lifetime.PerResolve).To<FileSystem>()
    .Bind().To<Configuration>()
    .Root<IConfiguration>("GlobalConfiguration", kind: RootKinds.Static);

var configuration = Composition.GlobalConfiguration;
configuration.ShouldBeOfType<Configuration>();

interface IFileSystem;

class FileSystem : IFileSystem;

interface IConfiguration;

class Configuration(IFileSystem fileSystem) : IConfiguration;
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Async Root

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<IFileStore>().To<FileStore>()
    .Bind<IBackupService>().To<BackupService>()

    // Specifies to use CancellationToken from the argument
    // when resolving a composition root
    .RootArg<CancellationToken>("cancellationToken")

    // Composition root
    .Root<Task<IBackupService>>("GetBackupServiceAsync");

var composition = new Composition();

// Resolves composition roots asynchronously
var service = await composition.GetBackupServiceAsync(CancellationToken.None);

interface IFileStore;

class FileStore : IFileStore;

interface IBackupService;

class BackupService(IFileStore fileStore) : IBackupService;
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Consumer type

`ConsumerType` is used to get the consumer type of the given dependency. The use of `ConsumerType` is demonstrated on the example of [Serilog library](https://serilog.net/):
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Serilog.Core;
using Serilog.Events;
using Pure.DI;
using Serilog.Core;

Serilog.ILogger serilogLogger = new Serilog.LoggerConfiguration().CreateLogger();
var composition = new Composition(logger: serilogLogger);
var service = composition.Root;

interface IDependency;

class Dependency : IDependency
{
    public Dependency(Serilog.ILogger log)
    {
        log.Information("Dependency created");
    }
}

interface IService
{
    IDependency Dependency { get; }
}

class Service : IService
{
    public Service(
        Serilog.ILogger log,
        IDependency dependency)
    {
        Dependency = dependency;
        log.Information("Service initialized");
    }

    public IDependency Dependency { get; }
}

partial class Composition
{
    private void Setup() =>

        DI.Setup(nameof(Composition))
            .Arg<Serilog.ILogger>("logger", "from arg")
            .Bind().To(ctx => {
                ctx.Inject<Serilog.ILogger>("from arg", out var logger);

                // Enriches the logger with the specific context of the consumer type.
                // ctx.ConsumerType represents the type into which the dependency is being injected.
                // This allows logs to be tagged with the name of the class that created them.
                return logger.ForContext(ctx.ConsumerType);
            })
            .Bind().To<Dependency>()
            .Bind().To<Service>()
            .Root<IService>(nameof(Root));
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)
 - [Serilog.Core](https://www.nuget.org/packages/Serilog.Core)
 - [Serilog.Events](https://www.nuget.org/packages/Serilog.Events)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Ref dependencies

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup("Composition")
    // Represents a large data set or buffer
    .Bind().To<int[]>(() => [10, 20, 30])
    .Root<Service>("MyService");

var composition = new Composition();
var service = composition.MyService;
service.Sum.ShouldBe(60);

class Service
{
    public int Sum { get; private set; }

    // Ref structs cannot be fields, so they are injected via a method
    // with the [Ordinal] attribute. This allows working with
    // high-performance types like Span<T> or other ref structs.
    [Ordinal]
    public void Initialize(ref Data data) =>
        Sum = data.Sum();
}

// A ref struct that holds a reference to the data
// to process it without additional memory allocations
readonly ref struct Data(ref int[] data)
{
    private readonly ref int[] _dep = ref data;

    public int Sum() => _dep.Sum();
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Roots

Sometimes you need roots for all types inherited from <see cref="T"/> available at compile time at the point where the method is called.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().As(Lifetime.Singleton).To<Preferences>()
    // Roots can be used to register all descendants of a type as roots.
    .Roots<IWindow>("{type}");

var composition = new Composition();
composition.MainWindow.ShouldBeOfType<MainWindow>();
composition.SettingsWindow.ShouldBeOfType<SettingsWindow>();

interface IPreferences;

class Preferences : IPreferences;

interface IWindow;

class MainWindow(IPreferences preferences) : IWindow;

class SettingsWindow(IPreferences preferences) : IWindow;
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Roots with filter

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind().As(Lifetime.Singleton).To<Configuration>()
    .Roots<INotificationService>("My{type}", filter: "*Email*");

var composition = new Composition();
composition.MyEmailService.ShouldBeOfType<EmailService>();

interface IConfiguration;

class Configuration : IConfiguration;

interface INotificationService;

// This service requires an API key which is not bound,
// so it cannot be resolved and should be filtered out.
class SmsService(string apiKey) : INotificationService;

class EmailService(IConfiguration config) : INotificationService;
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Transient

The _Transient_ lifetime specifies to create a new dependency instance each time. It is the default lifetime and can be omitted.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Singleton

The _Singleton_ lifetime ensures that there will be a single instance of the dependency for each composition.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## PerResolve

The _PerResolve_ lifetime ensures that there will be one instance of the dependency for each composition root instance.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

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

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Func

_Func<T>_ helps when the logic must enter instances of some type on demand or more than once. This is a very handy mechanism for instance replication. For example it is used when implementing the `Lazy<T>` injection.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using System.Collections.Immutable;

DI.Setup(nameof(Composition))
    .Bind().As(Lifetime.Singleton).To<TicketIdGenerator>()
    .Bind().To<Ticket>()
    .Bind().To<QueueTerminal>()

    // Composition root
    .Root<IQueueTerminal>("Terminal");

var composition = new Composition();
var terminal = composition.Terminal;

terminal.Tickets.Length.ShouldBe(3);

terminal.Tickets[0].Id.ShouldBe(1);
terminal.Tickets[1].Id.ShouldBe(2);
terminal.Tickets[2].Id.ShouldBe(3);

interface ITicketIdGenerator
{
    int NextId { get; }
}

class TicketIdGenerator : ITicketIdGenerator
{
    public int NextId => ++field;
}

interface ITicket
{
    int Id { get; }
}

class Ticket(ITicketIdGenerator idGenerator) : ITicket
{
    public int Id { get; } = idGenerator.NextId;
}

interface IQueueTerminal
{
    ImmutableArray<ITicket> Tickets { get; }
}

class QueueTerminal(Func<ITicket> ticketFactory) : IQueueTerminal
{
    public ImmutableArray<ITicket> Tickets { get; } =
    [
        // The factory creates a new instance of the ticket each time it is called
        ticketFactory(),
        ticketFactory(),
        ticketFactory()
    ];
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Be careful, replication takes into account the lifetime of the object.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Enumerable

Specifying `IEnumerable<T>` as the injection type lets you inject instances of all bindings that implement type `T` in a lazy fashion - the instances will be provided one by one, in order corresponding to the sequence of bindings.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using System.Collections.Immutable;

DI.Setup(nameof(Composition))
    .Bind<IMessageSender>().To<EmailSender>()
    .Bind<IMessageSender>("sms").To<SmsSender>()
    .Bind<INotificationService>().To<NotificationService>()

    // Composition root
    .Root<INotificationService>("NotificationService");

var composition = new Composition();
var notificationService = composition.NotificationService;
notificationService.Senders.Length.ShouldBe(2);
notificationService.Senders[0].ShouldBeOfType<EmailSender>();
notificationService.Senders[1].ShouldBeOfType<SmsSender>();

notificationService.Notify("Hello World");

interface IMessageSender
{
    void Send(string message);
}

class EmailSender : IMessageSender
{
    public void Send(string message)
    {
        // Sending email...
    }
}

class SmsSender : IMessageSender
{
    public void Send(string message)
    {
        // Sending SMS...
    }
}

interface INotificationService
{
    ImmutableArray<IMessageSender> Senders { get; }

    void Notify(string message);
}

class NotificationService(IEnumerable<IMessageSender> senders) : INotificationService
{
    public ImmutableArray<IMessageSender> Senders { get; }
        = [..senders];

    public void Notify(string message)
    {
        foreach (var sender in Senders)
        {
            sender.Send(message);
        }
    }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Enumerable generics

Shows how generic middleware pipelines collect all matching implementations.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using System.Collections.Immutable;

DI.Setup(nameof(Composition))
    // Register generic middleware components.
    // LoggingMiddleware<T> is registered as the default implementation.
    .Bind<IMiddleware<TT>>().To<LoggingMiddleware<TT>>()
    // MetricsMiddleware<T> is registered with the "Metrics" tag.
    .Bind<IMiddleware<TT>>("Metrics").To<MetricsMiddleware<TT>>()

    // Register the pipeline that takes the collection of all middleware.
    .Bind<IPipeline<TT>>().To<Pipeline<TT>>()

    // Composition roots for different data types (int and string)
    .Root<IPipeline<int>>("IntPipeline")
    .Root<IPipeline<string>>("StringPipeline");

var composition = new Composition();

// Validate the pipeline for int
var intPipeline = composition.IntPipeline;
intPipeline.Middlewares.Length.ShouldBe(2);
intPipeline.Middlewares[0].ShouldBeOfType<LoggingMiddleware<int>>();
intPipeline.Middlewares[1].ShouldBeOfType<MetricsMiddleware<int>>();

// Validate the pipeline for string
var stringPipeline = composition.StringPipeline;
stringPipeline.Middlewares.Length.ShouldBe(2);
stringPipeline.Middlewares[0].ShouldBeOfType<LoggingMiddleware<string>>();
stringPipeline.Middlewares[1].ShouldBeOfType<MetricsMiddleware<string>>();

// Middleware interface
interface IMiddleware<T>;

// Logging implementation
class LoggingMiddleware<T> : IMiddleware<T>;

// Metrics implementation
class MetricsMiddleware<T> : IMiddleware<T>;

// Pipeline interface
interface IPipeline<T>
{
    ImmutableArray<IMiddleware<T>> Middlewares { get; }
}

// Pipeline implementation that aggregates all available middleware
class Pipeline<T>(IEnumerable<IMiddleware<T>> middlewares) : IPipeline<T>
{
    public ImmutableArray<IMiddleware<T>> Middlewares { get; }
        = [..middlewares];
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Array

Specifying `T[]` as the injection type allows instances from all bindings that implement the `T` type to be injected.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<ISensor>().To<TemperatureSensor>()
    .Bind<ISensor>("External").To<WindSensor>()
    .Bind<ISensorService>().To<SensorService>()

    // Composition root
    .Root<ISensorService>("Sensor");

var composition = new Composition();
var sensor = composition.Sensor;

// Checks that all bindings for the ISensor interface are injected,
// regardless of whether they are tagged or not.
sensor.Sensors.Length.ShouldBe(2);
sensor.Sensors[0].ShouldBeOfType<TemperatureSensor>();
sensor.Sensors[1].ShouldBeOfType<WindSensor>();

interface ISensor;

class TemperatureSensor : ISensor;

class WindSensor : ISensor;

interface ISensorService
{
    ISensor[] Sensors { get; }
}

class SensorService(ISensor[] sensors) : ISensorService
{
    public ISensor[] Sensors { get; } = sensors;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

In addition to arrays, other collection types are also supported, such as:
- System.Memory<T>
- System.ReadOnlyMemory<T>
- System.Span<T>
- System.ReadOnlySpan<T>
- System.Collections.Generic.ICollection<T>
- System.Collections.Generic.IList<T>
- System.Collections.Generic.List<T>
- System.Collections.Generic.IReadOnlyCollection<T>
- System.Collections.Generic.IReadOnlyList<T>
- System.Collections.Generic.ISet<T>
- System.Collections.Generic.HashSet<T>
- System.Collections.Generic.SortedSet<T>
- System.Collections.Generic.Queue<T>
- System.Collections.Generic.Stack<T>
- System.Collections.Immutable.ImmutableArray<T>
- System.Collections.Immutable.IImmutableList<T>
- System.Collections.Immutable.ImmutableList<T>
- System.Collections.Immutable.IImmutableSet<T>
- System.Collections.Immutable.ImmutableHashSet<T>
- System.Collections.Immutable.ImmutableSortedSet<T>
- System.Collections.Immutable.IImmutableQueue<T>
- System.Collections.Immutable.ImmutableQueue<T>
- System.Collections.Immutable.IImmutableStack<T>
And of course this list can easily be supplemented on its own.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Generics

Generic types are also supported.
> [!IMPORTANT]
> Instead of open generic types, as in classical DI container libraries, regular generic types with _marker_ types as type parameters are used here. Such "marker" types allow to define dependency graph more precisely.

For the case of `IDependency<TT>`, `TT` is a _marker_ type, which allows the usual `IDependency<TT>` to be used instead of an open generic type like `IDependency<>`. This makes it easy to bind generic types by specifying _marker_ types such as `TT`, `TT1`, etc. as parameters of generic types:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    // Disable Resolve methods to keep the public API minimal
    .Hint(Hint.Resolve, "Off")
    // Binding a generic interface to a generic implementation
    // using the marker type TT. This allows Pure.DI to match
    // IRepository<User> to Repository<User>, IRepository<Order> to Repository<Order>, etc.
    .Bind<IRepository<TT>>().To<Repository<TT>>()
    .Bind<IDataService>().To<DataService>()

    // Composition root
    .Root<IDataService>("DataService");

var composition = new Composition();
var dataService = composition.DataService;

// Verifying that the correct generic types were injected
dataService.Users.ShouldBeOfType<Repository<User>>();
dataService.Orders.ShouldBeOfType<Repository<Order>>();

interface IRepository<T>;

class Repository<T> : IRepository<T>;

// Domain entities
record User;

record Order;

interface IDataService
{
    IRepository<User> Users { get; }

    IRepository<Order> Orders { get; }
}

class DataService(
    IRepository<User> users,
    IRepository<Order> orders)
    : IDataService
{
    public IRepository<User> Users { get; } = users;

    public IRepository<Order> Orders { get; } = orders;
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Actually, the property _Root_ looks like:
```c#
public IService Root
{
  get
  {
    return new Service(new Dependency<int>(), new Dependency<string>());
  }
}
```
Even in this simple scenario, it is not possible to precisely define the binding of an abstraction to its implementation using open generic types:
```c#
.Bind(typeof(IMap<,>)).To(typeof(Map<,>))
```
You can try to match them by order or by name derived from the .NET type reflection. But this is not reliable, since order and name matching is not guaranteed. For example, there is some interface with two arguments of type _key and _value_. But in its implementation the sequence of type arguments is mixed up: first comes the _value_ and then the _key_ and the names do not match:
```c#
class Map<TV, TK>: IMap<TKey, TValue> { }
```
At the same time, the marker types `TT1` and `TT2` handle this easily. They determine the exact correspondence between the type arguments in the interface and its implementation:
```c#
.Bind<IMap<TT1, TT2>>().To<Map<TT2, TT1>>()
```
The first argument of the type in the interface, corresponds to the second argument of the type in the implementation and is a _key_. The second argument of the type in the interface, corresponds to the first argument of the type in the implementation and is a _value_. This is a simple example. Obviously, there are plenty of more complex scenarios where tokenized types will be useful.
Marker types are regular .NET types marked with a special attribute, such as:
```c#
[GenericTypeArgument]
internal abstract class TT1 { }

[GenericTypeArgument]
internal abstract class TT2 { }
```
This way you can easily create your own, including making them fit the constraints on the type parameter, for example:
```c#
[GenericTypeArgument]
internal struct TTS { }

[GenericTypeArgument]
internal interface TTDisposable: IDisposable { }

[GenericTypeArgument]
internal interface TTEnumerator<out T>: IEnumerator<T> { }
```
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Generic composition roots

Sometimes you want to be able to create composition roots with type parameters. In this case, the composition root can only be represented by a method.
> [!IMPORTANT]
> ``Resolve()` methods cannot be used to resolve generic composition roots.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Pure.DI;

DI.Setup(nameof(Composition))
    // Disable Resolve methods to keep the public API minimal
    .Hint(Hint.Resolve, "Off")
    .Bind().To<Repository<TT>>()
    .Bind().To<CreateCommandHandler<TT>>()
    // Creates UpdateCommandHandler manually,
    // just for the sake of example
    .Bind("Update").To(ctx => {
        ctx.Inject(out IRepository<TT> repository);
        return new UpdateCommandHandler<TT>(repository);
    })

    // Specifies to create a regular public method
    // to get a composition root of type ICommandHandler<T>
    // with the name "GetCreateCommandHandler"
    .Root<ICommandHandler<TT>>("GetCreateCommandHandler")

    // Specifies to create a regular public method
    // to get a composition root of type ICommandHandler<T>
    // with the name "GetUpdateCommandHandler"
    // using the "Update" tag
    .Root<ICommandHandler<TT>>("GetUpdateCommandHandler", "Update");

var composition = new Composition();

// createHandler = new CreateCommandHandler<int>(new Repository<int>());
var createHandler = composition.GetCreateCommandHandler<int>();

// updateHandler = new UpdateCommandHandler<string>(new Repository<string>());
var updateHandler = composition.GetUpdateCommandHandler<string>();

interface IRepository<T>;

class Repository<T> : IRepository<T>;

interface ICommandHandler<T>;

class CreateCommandHandler<T>(IRepository<T> repository) : ICommandHandler<T>;

class UpdateCommandHandler<T>(IRepository<T> repository) : ICommandHandler<T>;
```

To run the above code, the following NuGet package must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)

> [!IMPORTANT]
> The method `Inject()`cannot be used outside of the binding setup.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Constructor ordinal attribute

Applying this attribute disables automatic constructor selection. Only constructors marked with this attribute are considered, ordered by ordinal (ascending).
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Arg<string>("connectionString")
    .Bind().To<Configuration>()
    .Bind().To<SqlDatabaseClient>()

    // Composition root
    .Root<IDatabaseClient>("Client");

var composition = new Composition(connectionString: "Server=.;Database=MyDb;");
var client = composition.Client;

// The client was created using the connection string constructor (Ordinal 0)
// even though the configuration constructor (Ordinal 1) was also possible.
client.ConnectionString.ShouldBe("Server=.;Database=MyDb;");

interface IConfiguration;

class Configuration : IConfiguration;

interface IDatabaseClient
{
    string ConnectionString { get; }
}

class SqlDatabaseClient : IDatabaseClient
{
    // The integer value in the argument specifies
    // the ordinal of injection.
    // The DI container will try to use this constructor first (Ordinal 0).
    [Ordinal(0)]
    internal SqlDatabaseClient(string connectionString) =>
        ConnectionString = connectionString;

    // If the first constructor cannot be used (e.g. connectionString is missing),
    // the DI container will try to use this one (Ordinal 1).
    [Ordinal(1)]
    public SqlDatabaseClient(IConfiguration configuration) =>
        ConnectionString = "Server=.;Database=DefaultDb;";

    public SqlDatabaseClient() =>
        ConnectionString = "InMemory";

    public string ConnectionString { get; }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The `Ordinal` attribute is part of the API, but you can define your own in any assembly or namespace.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Dependency attribute

When applied to a property or field, the member participates in DI, ordered by ordinal (ascending).
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using System.Text;

DI.Setup(nameof(PersonComposition))
    .Arg<int>("personId")
    .Arg<string>("personName")
    .Arg<DateTime>("personBirthday")
    .Bind().To<Person>()

    // Composition root
    .Root<IPerson>("Person");

var composition = new PersonComposition(
    personId: 123,
    personName: "Nik",
    personBirthday: new DateTime(1977, 11, 16));

var person = composition.Person;
person.Name.ShouldBe("123 Nik 1977-11-16");

interface IPerson
{
    string Name { get; }
}

class Person : IPerson
{
    private readonly StringBuilder _name = new();

    public string Name => _name.ToString();

    [Dependency] public int Id;

    // The Ordinal attribute specifies to perform an injection,
    // the integer value in the argument specifies
    // the ordinal of injection
    [Dependency(ordinal: 1)]
    public string FirstName
    {
        set
        {
            _name.Append(Id);
            _name.Append(' ');
            _name.Append(value);
        }
    }

    [Dependency(ordinal: 2)]
    public DateTime Birthday
    {
        set
        {
            _name.Append(' ');
            _name.Append($"{value:yyyy-MM-dd}");
        }
    }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The `Dependency` attribute is part of the API, but you can define your own in any assembly or namespace.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Member ordinal attribute

When applied to a property or field, the member participates in DI, ordered by ordinal (ascending).
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;
using System.Text;

DI.Setup(nameof(PersonComposition))
    .Arg<int>("personId")
    .Arg<string>("personName")
    .Arg<DateTime>("personBirthday")
    .Bind().To<Person>()

    // Composition root
    .Root<IPerson>("Person");

var composition = new PersonComposition(
    personId: 123,
    personName: "Nik",
    personBirthday: new DateTime(1977, 11, 16));

var person = composition.Person;
person.Name.ShouldBe("123 Nik 1977-11-16");

interface IPerson
{
    string Name { get; }
}

class Person : IPerson
{
    private readonly StringBuilder _name = new();

    public string Name => _name.ToString();

    // The Ordinal attribute specifies to perform an injection,
    // the integer value in the argument specifies
    // the ordinal of injection
    [Ordinal(0)] public int Id;

    [Ordinal(1)]
    public string FirstName
    {
        set
        {
            _name.Append(Id);
            _name.Append(' ');
            _name.Append(value);
        }
    }

    [Ordinal(2)]
    public DateTime Birthday
    {
        set
        {
            _name.Append(' ');
            _name.Append($"{value:yyyy-MM-dd}");
        }
    }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

The `Ordinal` attribute is part of the API, but you can define your own in any assembly or namespace.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Decorator

Interception is the ability to intercept calls between objects in order to enrich or change their behavior, but without having to change their code. A prerequisite for interception is weak binding. That is, if programming is abstraction-based, the underlying implementation can be transformed or improved by "packaging" it into other implementations of the same abstraction. At its core, intercept is an application of the Decorator design pattern. This pattern provides a flexible alternative to inheritance by dynamically "attaching" additional responsibility to an object. Decorator "packs" one implementation of an abstraction into another implementation of the same abstraction like a "matryoshka doll".
_Decorator_ is a well-known and useful design pattern. It is convenient to use tagged dependencies to build a chain of nested decorators, as in the example below:
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind("base").To<TextWidget>()
    .Bind().To<BoxWidget>()
    .Root<IWidget>("Widget");

var composition = new Composition();
var widget = composition.Widget;
widget.Render().ShouldBe("[ Hello World ]");

interface IWidget
{
    string Render();
}

class TextWidget : IWidget
{
    public string Render() => "Hello World";
}

class BoxWidget([Tag("base")] IWidget baseWidget) : IWidget
{
    public string Render() => $"[ {baseWidget.Render()} ]";
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)

Here an instance of the _TextWidget_ type, labeled _"base"_, is injected in the decorator _BoxWidget_. You can use any tag that semantically reflects the feature of the abstraction being embedded. The tag can be a constant, a type, or a value of an enumerated type.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Interception

Interception lets you enrich or change the behavior of a certain set of objects from the object graph being created without changing the code of the corresponding types.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Castle.DynamicProxy;
using System.Runtime.CompilerServices;
using Pure.DI;

// OnDependencyInjection = On
// OnDependencyInjectionContractTypeNameWildcard = *IGreeter
DI.Setup(nameof(Composition))
    .Bind().To<Greeter>()
    .Root<IGreeter>("Greeter");

var composition = new Composition();
var greeter = composition.Greeter;

// The greeting is modified by the interceptor
greeter.Greet("World").ShouldBe("Hello World !!!");

public interface IGreeter
{
    string Greet(string name);
}

class Greeter : IGreeter
{
    public string Greet(string name) => $"Hello {name}";
}

partial class Composition : IInterceptor
{
    private static readonly ProxyGenerator ProxyGenerator = new();

    // Intercepts the instantiation of services to wrap them in a proxy
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial T OnDependencyInjection<T>(
        in T value,
        object? tag,
        Lifetime lifetime)
    {
        // Proxying is only possible for reference types (interfaces, classes)
        if (typeof(T).IsValueType)
        {
            return value;
        }

        // Creates a proxy that delegates calls to the 'value' object
        // and passes them through the 'this' interceptor
        return (T)ProxyGenerator.CreateInterfaceProxyWithTargetInterface(
            typeof(T),
            value,
            this);
    }

    // Logic performed when a method on the proxy is called
    public void Intercept(IInvocation invocation)
    {
        // Executes the original method
        invocation.Proceed();

        // Enhances the result of the Greet method
        if (invocation.Method.Name == nameof(IGreeter.Greet)
            && invocation.ReturnValue is string message)
        {
            invocation.ReturnValue = $"{message} !!!";
        }
    }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)
 - [Castle.DynamicProxy](https://www.nuget.org/packages/Castle.DynamicProxy)

Using an intercept gives you the ability to add end-to-end functionality such as:

- Logging

- Action logging

- Performance monitoring

- Security

- Caching

- Error handling

- Providing resistance to failures, etc.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


## Advanced interception

This approach of interception maximizes performance by precompiling the proxy object factory.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.

```c#
using Shouldly;
using Castle.DynamicProxy;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Pure.DI;

// OnDependencyInjection = On
DI.Setup(nameof(Composition))
    .Bind().To<DataService>()
    .Bind().To<BusinessService>()
    .Root<IBusinessService>("BusinessService");

var log = new List<string>();
var composition = new Composition(log);
var businessService = composition.BusinessService;

// Use the services to verify interception.
businessService.Process();
businessService.DataService.Count();

log.ShouldBe(
    ImmutableArray.Create(
        "Process returns Processed",
        "get_DataService returns Castle.Proxies.IDataServiceProxy",
        "Count returns 55"));

public interface IDataService
{
    int Count();
}

class DataService : IDataService
{
    public int Count() => 55;
}

public interface IBusinessService
{
    IDataService DataService { get; }

    string Process();
}

class BusinessService(IDataService dataService) : IBusinessService
{
    public IDataService DataService { get; } = dataService;

    public string Process() => "Processed";
}

internal partial class Composition : IInterceptor
{
    private readonly List<string> _log = [];
    private static readonly IProxyBuilder ProxyBuilder = new DefaultProxyBuilder();
    private readonly IInterceptor[] _interceptors = [];

    public Composition(List<string> log)
    {
        _log = log;
        _interceptors = [this];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial T OnDependencyInjection<T>(
        in T value,
        object? tag,
        Lifetime lifetime)
    {
        if (typeof(T).IsValueType)
        {
            return value;
        }

        return ProxyFactory<T>.GetFactory(ProxyBuilder)(
            value,
            _interceptors);
    }

    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
        _log.Add($"{invocation.Method.Name} returns {invocation.ReturnValue}");
    }

    private static class ProxyFactory<T>
    {
        private static Func<T, IInterceptor[], T>? _factory;

        public static Func<T, IInterceptor[], T> GetFactory(IProxyBuilder proxyBuilder) =>
            _factory ?? CreateFactory(proxyBuilder);

        private static Func<T, IInterceptor[], T> CreateFactory(IProxyBuilder proxyBuilder)
        {
            // Compiles a delegate to create a proxy for the performance boost
            var proxyType = proxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(
                typeof(T),
                Type.EmptyTypes,
                ProxyGenerationOptions.Default);
            var ctor = proxyType.GetConstructors()
                .Single(i => i.GetParameters().Length == 2);
            var instance = Expression.Parameter(typeof(T));
            var interceptors = Expression.Parameter(typeof(IInterceptor[]));
            var newProxyExpression = Expression.New(ctor, interceptors, instance);
            return _factory = Expression.Lambda<Func<T, IInterceptor[], T>>(
                    newProxyExpression,
                    instance,
                    interceptors)
                .Compile();
        }
    }
}
```

To run the above code, the following NuGet packages must be added:
 - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
 - [Shouldly](https://www.nuget.org/packages/Shouldly)
 - [Castle.DynamicProxy](https://www.nuget.org/packages/Castle.DynamicProxy)

What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.

