/*
$v=true
$p=99
$d=Func with arguments
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
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable VariableHidesOuterVariable

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.BCL.FuncWithArgumentsScenario;

using System.Collections.Immutable;
using Shouldly;
using Xunit;

// {
//# using Pure.DI;
//# using System.Collections.Immutable;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
        // FormatCode = On
// {    
        DI.Setup(nameof(Composition))
            .Bind().As(Lifetime.Singleton).To<Clock>()
            .Bind().To<Person>()
            .Bind().To<Team>()

            // Composition root
            .Root<ITeam>("Team");

        var composition = new Composition();
        var team = composition.Team;

        team.Members.Length.ShouldBe(3);

        team.Members[0].Id.ShouldBe(10);
        team.Members[0].Name.ShouldBe("Nik");

        team.Members[1].Id.ShouldBe(20);
        team.Members[1].Name.ShouldBe("Mike");

        team.Members[2].Id.ShouldBe(30);
        team.Members[2].Name.ShouldBe("Jake");
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IClock
{
    DateTimeOffset Now { get; }
}

class Clock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}

interface IPerson
{
    int Id { get; }

    string Name { get; }
}

class Person(string name, IClock clock, int id)
    : IPerson
{
    public int Id => id;

    public string Name => name;
}

interface ITeam
{
    ImmutableArray<IPerson> Members { get; }
}

class Team(Func<int, string, IPerson> personFactory) : ITeam
{
    public ImmutableArray<IPerson> Members { get; } =
    [
        personFactory(10, "Nik"),
        personFactory(20, "Mike"),
        personFactory(30, "Jake")
    ];
}
// }