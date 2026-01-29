/*
$v=true
$p=1
$d=Composition root kinds
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
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeMemberModifiers

namespace Pure.DI.UsageTests.Advanced.CompositionRootKindsScenario;

using Shouldly;
using Xunit;
using static RootKinds;

// {
//# using Pure.DI;
//# using static Pure.DI.RootKinds;
// }
public class Scenario
{
    [Fact]
    public void Run()
    {
        // {
        var composition = new Composition();
        var paymentService = composition.PaymentService;
        var cashPaymentService = composition.GetCashPaymentService();
        var validator = Composition.Validator;
        // }
        paymentService.ShouldBeOfType<CardPaymentService>();
        cashPaymentService.ShouldBeOfType<CashPaymentService>();
        validator.ShouldBeOfType<LuhnValidator>();
        composition.SaveClassDiagram();
    }
}

// {
interface ICreditCardValidator;

class LuhnValidator : ICreditCardValidator;

interface IPaymentService;

class CardPaymentService : IPaymentService
{
    public CardPaymentService(ICreditCardValidator validator)
    {
    }
}

class CashPaymentService : IPaymentService;

partial class Composition
{
    void Setup() =>
        DI.Setup(nameof(Composition))
            .Bind<IPaymentService>().To<CardPaymentService>()
            .Bind<IPaymentService>("Cash").To<CashPaymentService>()
            .Bind<ICreditCardValidator>().To<LuhnValidator>()

            // Creates a public root method named "GetCashPaymentService"
            .Root<IPaymentService>("GetCashPaymentService", "Cash", Public | Method)

            // Creates a private partial root method named "GetCardPaymentService"
            .Root<IPaymentService>("GetCardPaymentService", kind: Private | Partial | Method)

            // Creates an internal static root named "Validator"
            .Root<ICreditCardValidator>("Validator", kind: Internal | Static);

    private partial IPaymentService GetCardPaymentService();

    public IPaymentService PaymentService => GetCardPaymentService();
}
// }