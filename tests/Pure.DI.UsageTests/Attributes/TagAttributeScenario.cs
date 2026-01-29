/*
$v=true
$p=3
$d=Tag attribute
$h=Tags let you choose among multiple implementations of the same contract.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=A tag can be a constant, a type, a [smart tag](smart-tags.md), or an enum value. The `Tag` attribute is part of the API, but you can define your own in any assembly or namespace.
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
// ReSharper disable UnusedType.Global
// ReSharper disable ArrangeTypeModifiers

namespace Pure.DI.UsageTests.Basics.TagAttributeScenario;

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
            .Bind("Fast").To<FastRenderer>()
            .Bind("Quality").To<QualityRenderer>()
            .Bind().To<PageRenderer>()

            // Composition root
            .Root<IPageRenderer>("Renderer");

        var composition = new Composition();
        var pageRenderer = composition.Renderer;
        pageRenderer.FastRenderer.ShouldBeOfType<FastRenderer>();
        pageRenderer.QualityRenderer.ShouldBeOfType<QualityRenderer>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IRenderer;

class FastRenderer : IRenderer;

class QualityRenderer : IRenderer;

interface IPageRenderer
{
    IRenderer FastRenderer { get; }

    IRenderer QualityRenderer { get; }
}

class PageRenderer(
    [Tag("Fast")] IRenderer fastRenderer,
    [Tag("Quality")] IRenderer qualityRenderer)
    : IPageRenderer
{
    public IRenderer FastRenderer { get; } = fastRenderer;

    public IRenderer QualityRenderer { get; } = qualityRenderer;
}
// }
