/*
$v=true
$p=0
$d=Auto-bindings
$h=Auto-bindings are implicit bindings for non-abstract types, so the generator can build an object graph without explicit registrations.
$h=Pros: fast to get started and low ceremony in small demos or spikes.
$h=Cons: weaker adherence to dependency inversion and less explicit lifetime control, which can make larger graphs harder to reason about.
$h=Recommendation: prefer explicit bindings for abstractions in production code and use auto-bindings sparingly for simple leaf types.
$h=In this example, a composition is created with only a root; the `OrderService` and `Database` types are resolved implicitly.
$f=
$f=> [!WARNING]
$f=> This approach is not recommended if you follow the dependency inversion principle or need precise lifetime control.
$f=
$f=Prefer injecting abstractions (for example, interfaces) and map them to implementations as in most [other examples](injections-of-abstractions.md).
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

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Basics.AutoBindingsScenario;

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
        // Specifies to create a partial class named "Composition"
        DI.Setup("Composition")
            // with the root "Orders"
            .Root<OrderService>("Orders");

        var composition = new Composition();

        // service = new OrderService(new Database())
        var orders = composition.Orders;
// }
        composition.SaveClassDiagram();
    }
}

// {
class Database;

class OrderService(Database database);
// }
