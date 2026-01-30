#### Span and ReadOnlySpan

Specifying `Span<T>` and `ReadOnlySpan<T>` work the same as with the array `T[]`.
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Shouldly;
using Pure.DI;

DI.Setup(nameof(Composition))
    .Bind<Point>('a').To(() => new Point(1, 1))
    .Bind<Point>('b').To(() => new Point(2, 2))
    .Bind<Point>('c').To(() => new Point(3, 3))
    .Bind<IPath>().To<Path>()

    // Composition root
    .Root<IPath>("Path");

var composition = new Composition();
var path = composition.Path;
path.PointCount.ShouldBe(3);

readonly struct Point(int x, int y)
{
    public int X { get; } = x;

    public int Y { get; } = y;
}

interface IPath
{
    int PointCount { get; }
}

class Path(ReadOnlySpan<Point> points) : IPath
{
    // The 'points' span is allocated on the stack, so it's very efficient.
    // However, we cannot store it in a field because it's a ref struct.
    // We can process it here in the constructor.
    public int PointCount { get; } = points.Length;
}
```

<details>
<summary>Running this code sample locally</summary>

- Make sure you have the [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or later installed
```bash
dotnet --list-sdk
```
- Create a net10.0 (or later) console application
```bash
dotnet new console -n Sample
```
- Add references to the NuGet packages
  - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
  - [Shouldly](https://www.nuget.org/packages/Shouldly)
```bash
dotnet add package Pure.DI
dotnet add package Shouldly
```
- Copy the example code into the _Program.cs_ file

You are ready to run the example 🚀
```bash
dotnet run
```

</details>

This scenario is even more efficient in the case of `Span<T>` or `ReadOnlySpan<T>` when `T` is a value type. In this case, there is no heap allocation, and the composition root `IPath` looks like this:
```c#
public IPath Path
{
  get
  {
    ReadOnlySpan<Point> points = stackalloc Point[3] { new Point(1, 1), new Point(2, 2), new Point(3, 3) };
    return new Path(points);
  }
}
```
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


The following partial class will be generated:

```c#
partial class Composition
{
  public IPath Path
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      Point transientPoint377 = new Point(1, 1);
      Point transientPoint378 = new Point(2, 2);
      Point transientPoint379 = new Point(3, 3);
      return new Path(stackalloc Point[3] { transientPoint377, transientPoint378, transientPoint379 });
    }
  }
}
```

Class diagram:

```mermaid
---
 config:
  maxTextSize: 2147483647
  maxEdges: 2147483647
  class:
   hideEmptyMembersBox: true
---
classDiagram
	Path --|> IPath
	Composition ..> Path : IPath Path
	Path *--  ReadOnlySpanᐸPointᐳ : ReadOnlySpanᐸPointᐳ
	ReadOnlySpanᐸPointᐳ *--  Point : 'a'  Point
	ReadOnlySpanᐸPointᐳ *--  Point : 'b'  Point
	ReadOnlySpanᐸPointᐳ *--  Point : 'c'  Point
	namespace Pure.DI.UsageTests.BCL.SpanScenario {
		class Composition {
		<<partial>>
		+IPath Path
		}
		class IPath {
			<<interface>>
		}
		class Path {
				<<class>>
			+Path(ReadOnlySpanᐸPointᐳ points)
		}
		class Point {
				<<struct>>
		}
	}
	namespace System {
		class ReadOnlySpanᐸPointᐳ {
				<<struct>>
		}
	}
```

