/*
$v=true
$p=5
$d=Composition arguments
$h=Use composition arguments when you need to pass state into the composition. Define them with `Arg<T>(string argName)` (optionally with tags) and use them like any other dependency. Only arguments that are used in the object graph become constructor parameters.
$h=> [!NOTE]
$h=> Actually, composition arguments work like normal bindings. The difference is that they bind to the values of the arguments. These values will be injected wherever they are required.
$h=
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
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers

namespace Pure.DI.UsageTests.Basics.CompositionArgumentsScenario;

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
// }
        composition.SaveClassDiagram();
    }
}

// {
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
// }
