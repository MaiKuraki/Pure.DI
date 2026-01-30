#### Generic roots

When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Pure.DI;

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

interface IFormatter<T>;

class JsonFormatter<T> : IFormatter<T>;

interface IExporter<T>;

class FileExporter<T>(IFormatter<T> formatter) : IExporter<T>;

class NetworkExporter<T>(IFormatter<T> formatter) : IExporter<T>;
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
- Add a reference to the NuGet package
  - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
```bash
dotnet add package Pure.DI
```
- Copy the example code into the _Program.cs_ file

You are ready to run the example 🚀
```bash
dotnet run
```

</details>

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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public NetworkExporter<T2> GetMyNetworkExporter_T<T2>()
  {
    NetworkExporter<T2> transientNetworkExporter461;
    IFormatter<T2> localFormatter = new JsonFormatter<T2>();
    transientNetworkExporter461 = new NetworkExporter<T2>(localFormatter);
    return transientNetworkExporter461;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public FileExporter<T2> GetMyFileExporter_T<T2>()
  {
    return new FileExporter<T2>(new JsonFormatter<T2>());
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
	FileExporterᐸT2ᐳ --|> IExporterᐸT2ᐳ
	JsonFormatterᐸT2ᐳ --|> IFormatterᐸT2ᐳ
	Composition ..> NetworkExporterᐸT2ᐳ : NetworkExporterᐸT2ᐳ GetMyNetworkExporter_TᐸT2ᐳ()
	Composition ..> FileExporterᐸT2ᐳ : FileExporterᐸT2ᐳ GetMyFileExporter_TᐸT2ᐳ()
	NetworkExporterᐸT2ᐳ *--  JsonFormatterᐸT2ᐳ : IFormatterᐸT2ᐳ
	FileExporterᐸT2ᐳ *--  JsonFormatterᐸT2ᐳ : IFormatterᐸT2ᐳ
	namespace Pure.DI.UsageTests.Generics.GenericsRootsScenario {
		class Composition {
		<<partial>>
		+FileExporterᐸT2ᐳ GetMyFileExporter_TᐸT2ᐳ()
		+NetworkExporterᐸT2ᐳ GetMyNetworkExporter_TᐸT2ᐳ()
		}
		class FileExporterᐸT2ᐳ {
				<<class>>
			+FileExporter(IFormatterᐸT2ᐳ formatter)
		}
		class IExporterᐸT2ᐳ {
			<<interface>>
		}
		class IFormatterᐸT2ᐳ {
			<<interface>>
		}
		class JsonFormatterᐸT2ᐳ {
				<<class>>
			+JsonFormatter()
		}
		class NetworkExporterᐸT2ᐳ {
				<<class>>
		}
	}
```

