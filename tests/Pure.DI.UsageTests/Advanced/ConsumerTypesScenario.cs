/*
$v=true
$p=19
$d=Consumer types
$h=`ConsumerTypes` is used to get the list of consumer types of a given dependency. It contains an array of types and guarantees that it will contain at least one element. The use of `ConsumerTypes` is demonstrated on the example of [Serilog library](https://serilog.net/):
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
$r=Shouldly;Serilog.Core;Serilog.Events
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable

// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMemberInSuper.Global
#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.ConsumerTypesScenario;

#pragma warning disable CA2263
using Serilog.Core;
using Serilog.Events;
using Xunit;

// {
//# using Pure.DI;
//# using Serilog.Core;
// }

class EventSink(ICollection<LogEvent> events)
    : ILogEventSink
{
    public void Emit(LogEvent logEvent) =>
        events.Add(logEvent);
}

public class Scenario
{
    [Fact]
    public void Run()
    {
        var events = new List<LogEvent>();
        var serilogLogger = new Serilog.LoggerConfiguration()
            .WriteTo.Sink(new EventSink(events))
            .CreateLogger();
// {
        //# Serilog.ILogger serilogLogger = new Serilog.LoggerConfiguration().CreateLogger();
        var composition = new Composition(logger: serilogLogger);
        var orderProcessing = composition.OrderProcessing;
// }
        events.Count.ShouldBe(2);
        foreach (var @event in events)
        {
            @event.Properties.ContainsKey("SourceContext").ShouldBeTrue();
        }

        composition.SaveClassDiagram();
    }
}

// {
interface IPaymentGateway;

class PaymentGateway : IPaymentGateway
{
    public PaymentGateway(Serilog.ILogger log)
    {
        log.Information("Payment gateway initialized");
    }
}

interface IOrderProcessing
{
    IPaymentGateway PaymentGateway { get; }
}

class OrderProcessing : IOrderProcessing
{
    public OrderProcessing(
        Serilog.ILogger log,
        IPaymentGateway paymentGateway)
    {
        PaymentGateway = paymentGateway;
        log.Information("Order processing initialized");
    }

    public IPaymentGateway PaymentGateway { get; }
}

partial class Composition
{
    private void Setup() =>
// }
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup(nameof(Composition))
            .Arg<Serilog.ILogger>("logger", "from arg")
            .Bind().To(ctx => {
                ctx.Inject<Serilog.ILogger>("from arg", out var logger);

                // Using ConsumerTypes to get the type of the consumer.
                // This allows us to create a logger with a context specific to the consuming class.
                return logger.ForContext(ctx.ConsumerTypes[0]);
            })
            .Bind().To<PaymentGateway>()
            .Bind().To<OrderProcessing>()
            .Root<IOrderProcessing>(nameof(OrderProcessing));
}
// }