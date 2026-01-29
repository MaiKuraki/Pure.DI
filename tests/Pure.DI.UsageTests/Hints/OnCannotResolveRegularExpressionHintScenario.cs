/*
$v=true
$p=3
$d=OnCannotResolve regular expression hint
$h=Hints are used to fine-tune code generation. The _OnCannotResolve_ hint determines whether to generate a partial `OnCannotResolve<T>(...)` method to handle a scenario where an instance which cannot be resolved.
$h=In addition, setup hints can be comments before the _Setup_ method in the form ```hint = value```, for example: `// OnCannotResolveContractTypeNameRegularExpression = string`.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The `OnCannotResolveContractTypeNameRegularExpression` hint helps define the set of types that require manual dependency resolution. You can use it to specify a regular expression to filter the full type name.
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
$r=Shouldly
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameterInPartialMethod
// ReSharper disable UnusedVariable
// ReSharper disable ArrangeTypeModifiers

namespace Pure.DI.UsageTests.Hints.OnCannotResolveRegularExpressionHintScenario;
#pragma warning disable CA1822
using Shouldly;
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
        // The "OnCannotResolveContractTypeNameRegularExpression" hint defines a regular expression
        // to filter the full type name of unresolved dependencies.
        // In this case, we want to manually handle only "string" types.
        // OnCannotResolveContractTypeNameRegularExpression = string
        DI.Setup(nameof(Composition))
            .Hint(OnCannotResolve, "On")
            .Bind().To<DatabaseAccess>()
            .Bind().To<BusinessService>()
            .Root<IBusinessService>("BusinessService");

        var composition = new Composition();
        var businessService = composition.BusinessService;

        // Check that the connection string was successfully injected via OnCannotResolve
        businessService.DatabaseAccess.ConnectionString.ShouldBe("Server=localhost;Database=MyDb;");

// }
        composition.SaveClassDiagram();
    }
}

// {
interface IDatabaseAccess
{
    string ConnectionString { get; }
}

// A service requiring a connection string.
// The connection string is a primitive type 'string' that is not bound in the DI setup.
// It will be resolved via the 'OnCannotResolve' fallback method.
class DatabaseAccess(string connectionString) : IDatabaseAccess
{
    public string ConnectionString { get; } = connectionString;
}

interface IBusinessService
{
    IDatabaseAccess DatabaseAccess { get; }
}

class BusinessService(IDatabaseAccess databaseAccess) : IBusinessService
{
    public IDatabaseAccess DatabaseAccess { get; } = databaseAccess;
}

partial class Composition
{
    // This method is called when a dependency cannot be resolved by the standard DI container.
    // It serves as a fallback mechanism.
    private partial T OnCannotResolve<T>(
        object? tag,
        Lifetime lifetime)
    {
        // Check if the requested type is a string (according to the hint filter)
        if (typeof(T) == typeof(string))
        {
            // Provide the configuration value (e.g., loaded from a file)
            return (T)(object)"Server=localhost;Database=MyDb;";
        }

        throw new InvalidOperationException("Cannot resolve " + typeof(T));
    }
}
// }