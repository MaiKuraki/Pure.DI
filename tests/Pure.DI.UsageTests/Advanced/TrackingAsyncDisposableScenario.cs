/*
$v=true
$p=102
$d=Tracking async disposable instances per a composition root
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

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameterInPartialMethod
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ArrangeTypeMemberModifiers

namespace Pure.DI.UsageTests.Advanced.TrackingAsyncDisposableScenario;

using Xunit;

// {
//# using Pure.DI;
// }

public class Scenario
{
    [Fact]
    public async Task Run()
    {
// {
        var composition = new Composition();
        // Creates two independent roots (queries), each with its own dependency graph
        var query1 = composition.Query;
        var query2 = composition.Query;

        // Disposes of the second query
        await query2.DisposeAsync();

        // Checks that the connection associated with the second query has been closed
        query2.Value.Connection.IsDisposed.ShouldBeTrue();

        // At the same time, the connection of the first query remains active
        query1.Value.Connection.IsDisposed.ShouldBeFalse();

        // Disposes of the first query
        await query1.DisposeAsync();

        // Now the first connection is also closed
        query1.Value.Connection.IsDisposed.ShouldBeTrue();
// }
        new Composition().SaveClassDiagram();
    }
}

// {
// Interface for a resource requiring asynchronous disposal (e.g., DB)
interface IDbConnection
{
    bool IsDisposed { get; }
}

class DbConnection : IDbConnection, IAsyncDisposable
{
    public bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }
}

interface IQuery
{
    public IDbConnection Connection { get; }
}

class Query(IDbConnection connection) : IQuery
{
    public IDbConnection Connection { get; } = connection;
}

partial class Composition
{
    static void Setup() =>
// }
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
        // SystemThreadingLock = Off
// {
        DI.Setup()
            .Bind().To<DbConnection>()
            .Bind().To<Query>()

            // A special composition root 'Owned' that allows
            // managing the lifetime of IQuery and its dependencies
            .Root<Owned<IQuery>>("Query");
}
// }