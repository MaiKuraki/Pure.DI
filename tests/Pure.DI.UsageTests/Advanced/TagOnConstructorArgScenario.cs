/*
$v=true
$p=6
$d=Tag on a constructor argument
$h=The wildcards `*` and `?` are supported.
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=> [!WARNING]
$f=> Each potentially injectable argument, property, or field contains an additional tag. This tag can be used to specify what can be injected there. This will only work if the binding type and the tag match. So while this approach can be useful for specifying what to enter, it can be more expensive to maintain and less reliable, so it is recommended to use attributes like `[Tag(...)]` instead.
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
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedTypeParameter

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Advanced.TagOnConstructorArgScenario;

using Pure.DI;
using UsageTests;
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
            .Bind(Tag.OnConstructorArg<DataReplicator>("sourceStream"))
                .To<FileStream>()
            .Bind(Tag.OnConstructorArg<StreamProcessor<TT>>("stream"))
                .To<NetworkStream>()
            .Bind<IDataReplicator>().To<DataReplicator>()

            // Specifies to create the composition root named "Root"
            .Root<IDataReplicator>("Replicator");

        var composition = new Composition();
        var replicator = composition.Replicator;
        replicator.SourceStream.ShouldBeOfType<FileStream>();
        replicator.TargetStream.ShouldBeOfType<NetworkStream>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IStream;

class FileStream : IStream;

class NetworkStream : IStream;

class StreamProcessor<T>(IStream stream)
{
    public IStream Stream { get; } = stream;
}

interface IDataReplicator
{
    IStream SourceStream { get; }

    IStream TargetStream { get; }
}

class DataReplicator(
    IStream sourceStream,
    StreamProcessor<string> processor)
    : IDataReplicator
{
    public IStream SourceStream { get; } = sourceStream;

    public IStream TargetStream => processor.Stream;
}
// }