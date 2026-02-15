/*
$v=true
$p=18
$d=Async Root
$h=Demonstrates how to define asynchronous composition roots that return Task or Task<T>, enabling async operations during composition.
$f=> [!NOTE]
$f=> Async roots are useful when you need to perform asynchronous initialization or when your services require async creation.
$r=Shouldly
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