/*
$v=true
$p=10
$d=Partial class
$h=A partial class can contain setup code.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The partial class is also useful for specifying access modifiers to the generated class.
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

// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ArrangeTypeMemberModifiers

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.PartialClassScenario;

using System.Diagnostics;
using Shouldly;
using Xunit;
using static RootKinds;

// {
//# using Pure.DI;
//# using static Pure.DI.RootKinds;
//# using System.Diagnostics;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
// {
        var composition = new Composition("NorthStore");
        var orderService = composition.OrderService;

        // Checks that the dependencies were created correctly
        orderService.Order1.Id.ShouldBe(1);
        orderService.Order2.Id.ShouldBe(2);
        // Checks that the injected string contains the store name and the generated ID
        orderService.OrderDetails.ShouldBe("NorthStore_3");
// }
        orderService.ShouldBeOfType<OrderService>();
        composition.SaveClassDiagram();
    }
}

// {
interface IOrder
{
    long Id { get; }
}

class Order(long id) : IOrder
{
    public long Id { get; } = id;
}

class OrderService(
    [Tag("Order details")] string details,
    IOrder order1,
    IOrder order2)
{
    public string OrderDetails { get; } = details;

    public IOrder Order1 { get; } = order1;

    public IOrder Order2 { get; } = order2;
}

// The partial class is also useful for specifying access modifiers to the generated class
public partial class Composition(string storeName)
{
    private long _id;

    private long GenerateId() => Interlocked.Increment(ref _id);

    // In fact, this method will not be called at runtime
    [Conditional("DI")]
    void Setup() =>
// }
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup()
            .Bind<IOrder>().To<Order>()
            .Bind<long>().To(GenerateId)
            // Binds the string with the tag "Order details"
            .Bind<string>("Order details").To(() => $"{storeName}_{GenerateId()}")
            .Root<OrderService>("OrderService", kind: Internal);
}
// }