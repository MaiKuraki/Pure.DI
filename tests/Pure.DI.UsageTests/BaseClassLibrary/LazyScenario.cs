/*
$v=true
$p=3
$d=Lazy
$h=Demonstrates lazy injection using Lazy<T>, delaying instance creation until the Value property is accessed.
$f=>[!NOTE]
$f=>Lazy<T> is useful for expensive-to-create objects or when the instance may never be needed, improving application startup performance.
$r=Shouldly
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable ArrangeTypeModifiers

namespace Pure.DI.UsageTests.BCL.LazyScenario;

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
            .Bind<IGraphicsEngine>().To<GraphicsEngine>()
            .Bind<IWindow>().To<Window>()

            // Composition root
            .Root<IWindow>("Window");

        var composition = new Composition();
        var window = composition.Window;

        // The graphics engine is created only when it is first accessed
        window.Engine.ShouldBe(window.Engine);
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IGraphicsEngine;

class GraphicsEngine : IGraphicsEngine;

interface IWindow
{
    IGraphicsEngine Engine { get; }
}

class Window(Lazy<IGraphicsEngine> engine) : IWindow
{
    public IGraphicsEngine Engine => engine.Value;
}
// }