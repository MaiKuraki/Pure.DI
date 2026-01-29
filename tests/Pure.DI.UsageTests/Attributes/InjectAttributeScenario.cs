/*
$v=true
$p=5
$d=Inject attribute
$h=If you want attributes without defining your own, add this package:
$h=
$h=[![NuGet](https://img.shields.io/nuget/v/Pure.DI.Abstractions)](https://www.nuget.org/packages/Pure.DI.Abstractions)
$h=
$h=It provides `Inject` and `Inject<T>` for constructors, methods, properties, and fields, letting you configure injection metadata.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=This package should also be included in a project:
$f=
$f=[![NuGet](https://img.shields.io/nuget/v/Pure.DI)](https://www.nuget.org/packages/Pure.DI)
$f=What it shows:
$f=- Demonstrates the scenario setup and resulting object graph in Pure.DI.
$f=
$f=Important points:
$f=- Highlights the key configuration choices and their effect on resolution.
$f=
$f=Useful when:
$f=- You want a concrete template for applying this feature in a composition.
$f=
$r=Shouldly;Pure.DI.Abstractions
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedTypeParameter
// ReSharper disable UnusedMember.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable RedundantNameQualifier

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Attributes.InjectAttributeScenario;

using Xunit;
using Pure.DI.Abstractions;

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
        DI.Setup(nameof(PersonComposition))
            .Arg<int>("personId")
            .Bind<Uri>("Person Uri").To(() => new Uri("https://github.com/DevTeam/Pure.DI"))
            .Bind("NikName").To(() => "Nik")
            .Bind().To<Person>()

            // Composition root
            .Root<IPerson>("Person");

        var composition = new PersonComposition(personId: 123);
        var person = composition.Person;
        person.ToString().ShouldBe("123 Nik https://github.com/DevTeam/Pure.DI");
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IPerson;

class Person([Inject("NikName")] string name) : IPerson
{
    private object? _state;

    [Inject<int>] internal object Id = "";

    public void Initialize([Inject<Uri>("Person Uri", 1)] object state) =>
        _state = state;

    public override string ToString() => $"{Id} {name} {_state}";
}
// }
