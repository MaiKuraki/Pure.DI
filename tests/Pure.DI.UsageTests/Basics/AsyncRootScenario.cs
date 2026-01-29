/*
$v=true
$p=18
$d=Async Root
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

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable CheckNamespace

#pragma warning disable CS9113 // Parameter is unread.
namespace Pure.DI.UsageTests.Basics.AsyncRootScenario;

using Xunit;

// {
//# using Pure.DI;
// }

public class Scenario
{
    [Fact]
    public async Task Run()
    {
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup(nameof(Composition))
            .Bind<IFileStore>().To<FileStore>()
            .Bind<IBackupService>().To<BackupService>()

            // Specifies to use CancellationToken from the argument
            // when resolving a composition root
            .RootArg<CancellationToken>("cancellationToken")

            // Composition root
            .Root<Task<IBackupService>>("GetBackupServiceAsync");

        var composition = new Composition();

        // Resolves composition roots asynchronously
        var service = await composition.GetBackupServiceAsync(CancellationToken.None);
// }
        service.ShouldBeOfType<BackupService>();
        composition.SaveClassDiagram();
    }
}

// {
interface IFileStore;

class FileStore : IFileStore;

interface IBackupService;

class BackupService(IFileStore fileStore) : IBackupService;
// }