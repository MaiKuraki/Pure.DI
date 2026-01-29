/*
$v=true
$p=1
$d=Simplified binding
$h=You can call `Bind()` without type parameters. It binds the implementation type itself, and if it is not abstract, all directly implemented abstract types except special ones.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=In practice, most abstraction types can be inferred. The parameterless `Bind()` binds:
$f=
$f=- the implementation type itself
$f=- and, if it is NOT abstract,
$f=  - all abstract types it directly implements
$f=  - except special types
$f=
$f=Special types will not be added to bindings:
$f=
$f=- `System.Object`
$f=- `System.Enum`
$f=- `System.MulticastDelegate`
$f=- `System.Delegate`
$f=- `System.Collections.IEnumerable`
$f=- `System.Collections.Generic.IEnumerable<T>`
$f=- `System.Collections.Generic.IList<T>`
$f=- `System.Collections.Generic.ICollection<T>`
$f=- `System.Collections.IEnumerator`
$f=- `System.Collections.Generic.IEnumerator<T>`
$f=- `System.Collections.Generic.IReadOnlyList<T>`
$f=- `System.Collections.Generic.IReadOnlyCollection<T>`
$f=- `System.IDisposable`
$f=- `System.IAsyncResult`
$f=- `System.AsyncCallback`
$f=
$f=If you want to add your own special type, use the `SpecialType<T>()` call.
$f=
$f=For class `OrderManager`, `Bind().To<OrderManager>()` is equivalent to `Bind<IOrderRepository, IOrderNotification, OrderManager>().To<OrderManager>()`. The types `IDisposable` and `IEnumerable<string>` are excluded because they are special. `ManagerBase` is excluded because it is not abstract. `IManager` is excluded because it is not implemented directly by `OrderManager`.
$f=
$f=|    |                       |                                                   |
$f=|----|-----------------------|---------------------------------------------------|
$f=| ? | `OrderManager`        | implementation type itself                        |
$f=| ? | `IOrderRepository`    | directly implements                               |
$f=| ? | `IOrderNotification`  | directly implements                               |
$f=| ? | `IDisposable`         | special type                                      |
$f=| ? | `IEnumerable<string>` | special type                                      |
$f=| ? | `ManagerBase`         | non-abstract                                      |
$f=| ? | `IManager`            | is not directly implemented by class OrderManager |
$f=What it shows:
$f=- Demonstrates the scenario setup and resulting object graph in Pure.DI.
$f=
$f=Important points:
$f=- Highlights the key configuration choices and their effect on resolution.
$f=
$f=Useful when:
$f=- You want a concrete template for applying this feature in a composition.
$f=
*/

// ReSharper disable CheckNamespace
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable ClassNeverInstantiated.Global

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Basics.SimplifiedBindingScenario;

using System.Collections;
using Xunit;

// {
//# using System.Collections;
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
// }
        composition.SaveClassDiagram();
    }
}

// {
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
// }
