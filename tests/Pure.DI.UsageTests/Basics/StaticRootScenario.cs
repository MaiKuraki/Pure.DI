/*
$v=true
$p=17
$d=Static root
$h=Demonstrates how to create static composition roots that don't require instantiation of the composition class.
$f=>[!NOTE]
$f=>Static roots are useful when you want to access services without creating a composition instance.
$r=Shouldly
*/

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable ArrangeTypeModifiers

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Basics.StaticRootScenario;

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
            .Bind().As(Lifetime.PerResolve).To<FileSystem>()
            .Bind().To<Configuration>()
            .Root<IConfiguration>("GlobalConfiguration", kind: RootKinds.Static);

        var configuration = Composition.GlobalConfiguration;
        configuration.ShouldBeOfType<Configuration>();
// }
        new Composition().SaveClassDiagram();
    }
}

// {
interface IFileSystem;

class FileSystem : IFileSystem;

interface IConfiguration;

class Configuration(IFileSystem fileSystem) : IConfiguration;
// }