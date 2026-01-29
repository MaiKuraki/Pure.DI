/*
$v=true
$p=0
$d=Constructor ordinal attribute
$h=Applying this attribute disables automatic constructor selection. Only constructors marked with this attribute are considered, ordered by ordinal (ascending).
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=The `Ordinal` attribute is part of the API, but you can define your own in any assembly or namespace.
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
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable RedundantArgumentDefaultValue

namespace Pure.DI.UsageTests.Attributes.ConstructorOrdinalAttributeScenario;

using System.Diagnostics.CodeAnalysis;
using Shouldly;
using Xunit;

// {
//# using Pure.DI;
// }

[SuppressMessage("WRN", "DIW001:WRN")]
public class Scenario
{
    [Fact]
    public void Run()
    {
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
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
// }
        composition.SaveClassDiagram();
    }
}

// {
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
// }
