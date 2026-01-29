/*
$v=true
$p=1
$d=ThreadSafe hint
$h=Hints are used to fine-tune code generation. The _ThreadSafe_ hint determines whether object composition will be created in a thread-safe manner. This hint is _On_ by default. It is good practice not to use threads when creating an object graph, in which case this hint can be turned off, which will lead to a slight increase in performance.
$h=In addition, setup hints can be comments before the _Setup_ method in the form ```hint = value```, for example: `// ThreadSafe = Off`.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=For more hints, see [this](README.md#setup-hints) page.
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
// ReSharper disable UnusedParameterInPartialMethod
// ReSharper disable UnusedVariable
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Hints.ThreadSafeHintScenario;

using Xunit;
using static Hint;

// {
//# using Pure.DI;
//# using static Pure.DI.Hint;
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
            // Disabling thread-safety can improve performance.
            // This is safe when the object graph is resolved on a single thread,
            // for example at application startup.
            .Hint(ThreadSafe, "Off")
            .Bind().To<SqlDatabaseConnection>()
            .Bind().As(Lifetime.Singleton).To<ReportGenerator>()
            .Root<IReportGenerator>("Generator");

        var composition = new Composition();
        var reportGenerator = composition.Generator;
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IDatabaseConnection;

class SqlDatabaseConnection : IDatabaseConnection;

interface IReportGenerator;

class ReportGenerator(Func<IDatabaseConnection> connectionFactory) : IReportGenerator;
// }