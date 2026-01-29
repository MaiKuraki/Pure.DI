/*
$v=true
$p=5
$d=Auto scoped
$h=You can use the following example to automatically create a session when creating instances of a particular type:
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=> [!IMPORTANT]
$f=> The method `Inject()`cannot be used outside of the binding setup.
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
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeMemberModifiers
#pragma warning disable CS9113 // Parameter is unread.

namespace Pure.DI.UsageTests.Lifetimes.AutoScopedScenario;

using Shouldly;
using Xunit;
using static Lifetime;

// {
//# using Pure.DI;
//# using static Pure.DI.Lifetime;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
// {
        var composition = new Composition();
        var musicApp = composition.MusicAppRoot;

        // Session #1: user starts listening on "Living Room Speaker"
        var session1 = musicApp.StartListeningSession();
        session1.Enqueue("Daft Punk - One More Time");
        session1.Enqueue("Massive Attack - Teardrop");

        // Session #2: user starts listening on "Headphones"
        var session2 = musicApp.StartListeningSession();
        session2.Enqueue("Radiohead - Weird Fishes/Arpeggi");

        // Different sessions -> different scoped queue instances
        session1.Queue.ShouldNotBe(session2.Queue);

        // But inside one session, the same queue is used everywhere within that scope
        session1.Queue.Items.Count.ShouldBe(2);
        session2.Queue.Items.Count.ShouldBe(1);
// }
        composition.SaveClassDiagram();
    }
}
// {
// Domain abstractions

interface IPlaybackQueue
{
    IReadOnlyList<string> Items { get; }
    void Add(string trackTitle);
}

sealed class PlaybackQueue : IPlaybackQueue
{
    private readonly List<string> _items = [];

    public IReadOnlyList<string> Items => _items;

    public void Add(string trackTitle) => _items.Add(trackTitle);
}

interface IListeningSession
{
    IPlaybackQueue Queue { get; }

    void Enqueue(string trackTitle);
}

sealed class ListeningSession(IPlaybackQueue queue) : IListeningSession
{
    public IPlaybackQueue Queue => queue;

    public void Enqueue(string trackTitle) => queue.Add(trackTitle);
}

// Implements a "session boundary" for listening
class MusicApp(Func<IListeningSession> sessionFactory)
{
    // Each call creates a new DI scope under the hood (new "listening session").
    public IListeningSession StartListeningSession() => sessionFactory();
}

partial class Composition
{
    static void Setup() =>
// }
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup()
            // Scoped: one queue per listening session
            .Bind().As(Scoped).To<PlaybackQueue>()

            // Session composition root (private root used only to build sessions)
            .Root<ListeningSession>("Session", kind: RootKinds.Private)

            // Auto scoped factory: creates a new scope for each listening session
            .Bind().To(IListeningSession (Composition parentScope) => {
                // Create a child scope so scoped services (PlaybackQueue) are unique per session.
                var scope = new Composition(parentScope);
                return scope.Session;
            })

            // App-level root
            .Root<MusicApp>("MusicAppRoot");
}
// }