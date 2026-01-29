/*
$v=true
$p=3
$d=OnCannotResolve wildcard hint
$h=Hints are used to fine-tune code generation. The _OnCannotResolve_ hint determines whether to generate a partial `OnCannotResolve<T>(...)` method to handle a scenario where an instance which cannot be resolved.
$h=In addition, setup hints can be comments before the _Setup_ method in the form ```hint = value```, for example: `// OnCannotResolveContractTypeNameWildcard = string`.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The `OnCannotResolveContractTypeNameWildcard` hint helps define the set of types that require manual dependency resolution. You can use it to specify a wildcard to filter the full type name.
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

namespace Pure.DI.UsageTests.Hints.OnCannotResolveWildcardHintScenario;
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
        // OnCannotResolveContractTypeNameWildcard = string
        DI.Setup(nameof(Composition))
            .Hint(OnCannotResolve, "On")
            .Bind().To<DatabaseSettings>()
            .Bind().To<DataService>()
            .Root<IDataService>("DataService");

        var composition = new Composition();
        var dataService = composition.DataService;
        dataService.Settings.ConnectionString.ShouldBe("Server=localhost;");

// }
        composition.SaveClassDiagram();
    }
}

// {
interface IDatabaseSettings
{
    string ConnectionString { get; }
}

class DatabaseSettings(string connectionString) : IDatabaseSettings
{
    public string ConnectionString { get; } = connectionString;
}

interface IDataService
{
    IDatabaseSettings Settings { get; }
}

class DataService(IDatabaseSettings settings) : IDataService
{
    public IDatabaseSettings Settings { get; } = settings;
}

partial class Composition
{
    private partial T OnCannotResolve<T>(
        object? tag,
        Lifetime lifetime)
    {
        // Emulates obtaining a configuration value
        if (typeof(T) == typeof(string))
        {
            return (T)(object)"Server=localhost;";
        }

        throw new InvalidOperationException("Cannot resolve.");
    }
}
// }