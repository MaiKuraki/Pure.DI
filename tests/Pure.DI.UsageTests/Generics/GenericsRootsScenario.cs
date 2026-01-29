/*
$v=true
$p=10
$d=Generic roots
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

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedVariable
// ReSharper disable UnusedTypeParameter

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Generics.GenericsRootsScenario;

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
            // Disable Resolve methods to keep the public API minimal
            .Hint(Hint.Resolve, "Off")
            .Bind().To<JsonFormatter<TT>>()
            .Bind().To<FileExporter<TT>>()
            // Creates NetworkExporter manually,
            // just for the sake of example
            .Bind<NetworkExporter<TT>>().To(ctx => {
                ctx.Inject(out IFormatter<TT> formatter);
                return new NetworkExporter<TT>(formatter);
            })

            // Specifies to define composition roots for all types inherited from IExporter<TT>
            // available at compile time at the point where the method is called
            .Roots<IExporter<TT>>("GetMy{type}");

        var composition = new Composition();

        // fileExporter = new FileExporter<int>(new JsonFormatter<int>());
        var fileExporter = composition.GetMyFileExporter_T<int>();

        // networkExporter = new NetworkExporter<string>(new JsonFormatter<string>());
        var networkExporter = composition.GetMyNetworkExporter_T<string>();
        // }
        fileExporter.ShouldBeOfType<FileExporter<int>>();
        networkExporter.ShouldBeOfType<NetworkExporter<string>>();
        composition.SaveClassDiagram();
    }
}

// {
interface IFormatter<T>;

class JsonFormatter<T> : IFormatter<T>;

interface IExporter<T>;

class FileExporter<T>(IFormatter<T> formatter) : IExporter<T>;

class NetworkExporter<T>(IFormatter<T> formatter) : IExporter<T>;
// }