/*
$v=true
$p=9
$d=Global compositions
$h=When the `Setup(name, kind)` method is called, the second optional parameter specifies the composition kind. If you set it as `CompositionKind.Global`, no composition class will be created, but this setup will be the base setup for all others in the current project, and `DependsOn(...)` is not required. The setups will be applied in the sort order of their names.
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

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ArrangeTypeMemberModifiers

namespace Pure.DI.UsageTests.Advanced.GlobalCompositionsScenario;

using static CompositionKind;

// {
//# using Pure.DI;
//# using static Pure.DI.CompositionKind;

//# return;
// }

// {
class MyGlobalComposition
{
    static void Setup() =>
        DI.Setup(kind: Global)
            .Hint(Hint.ToString, "Off")
            .Hint(Hint.FormatCode, "On");
}

class MyGlobalComposition2
{
    static void Setup() =>
        DI.Setup("Some name", kind: Global)
            .Hint(Hint.ToString, "On");
}
// }