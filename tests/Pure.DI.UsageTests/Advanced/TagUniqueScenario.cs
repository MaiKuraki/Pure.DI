/*
$v=true
$p=4
$d=Tag Unique
$h=`Tag.Unique` is useful to register a binding with a unique tag. It will not be available through the composition root or `Resolve` methods directly, but can be injected in compositions as some kind of enumeration.
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
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedTypeParameter

namespace Pure.DI.UsageTests.Advanced.TagUniqueScenario;

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
// {
        DI.Setup(nameof(Composition))
            .Bind<INotificationChannel<TT>>(Tag.Unique).To<EmailChannel<TT>>()
            .Bind<INotificationChannel<TT>>(Tag.Unique).To<SmsChannel<TT>>()
            .Bind<INotificationService<TT>>().To<NotificationService<TT>>()

            // Composition root
            .Root<INotificationService<string>>("NotificationService");

        var composition = new Composition();
        var notificationService = composition.NotificationService;
        notificationService.Channels.Length.ShouldBe(2);
// }
        composition.SaveClassDiagram();
    }
}

// {
interface INotificationChannel<T>;

class EmailChannel<T> : INotificationChannel<T>;

class SmsChannel<T> : INotificationChannel<T>;

interface INotificationService<T>
{
    ImmutableArray<INotificationChannel<T>> Channels { get; }
}

class NotificationService<T>(IEnumerable<INotificationChannel<T>> channels)
    : INotificationService<T>
{
    public ImmutableArray<INotificationChannel<T>> Channels { get; }
        = [..channels];
}
// }