#### Exposed generic roots

Composition roots from other assemblies or projects can be used as a source of bindings. When you add a binding to a composition from another assembly or project, the roots of the composition with the `RootKind.Exposed` type will be used in the bindings automatically. For example, in some assembly a composition is defined as:
```c#
public partial class CompositionInOtherProject
{
    private static void Setup() =>
    DI.Setup()
        .Hint(Hint.Resolve, "Off")
        .Bind().To(() => 99)
        .Bind().As(Lifetime.Singleton).To<MyDependency>()
        .Bind().To<MyGenericService<TT>>()
        .Root<IMyGenericService<TT>>("GetMyService", kind: RootKinds.Exposed);
}
```
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Pure.DI;
using static Pure.DI.Lifetime;
using OtherAssembly;

DI.Setup(nameof(Composition))
    // Binds to exposed composition roots from other project
    .Bind().As(Singleton).To<CompositionWithGenericRootsInOtherProject>()
    .Root<Program>("Program");

var composition = new Composition();
var program = composition.Program;
program.DoSomething(99);

partial class Program(IMyGenericService<int> myService)
{
    public void DoSomething(int value) => myService.DoSomething(value);
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

> [!IMPORTANT]
> At this point, a composition from another assembly or another project can be used for this purpose. Compositions from the current project cannot be used in this way due to limitations of the source code generators.
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
#if NET9_0_OR_GREATER
  private readonly Lock _lock = new Lock();
#else
  private readonly Object _lock = new Object();
#endif

  private OtherAssembly.CompositionWithGenericRootsInOtherProject? _singletonCompositionWithGenericRootsInOtherProject51;

  public Program Program
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      OtherAssembly.IMyGenericService<int> transientIMyGenericService33;
      if (_singletonCompositionWithGenericRootsInOtherProject51 is null)
        lock (_lock)
          if (_singletonCompositionWithGenericRootsInOtherProject51 is null)
          {
            _singletonCompositionWithGenericRootsInOtherProject51 = new OtherAssembly.CompositionWithGenericRootsInOtherProject();
          }

      OtherAssembly.CompositionWithGenericRootsInOtherProject localInstance_1182D127 = _singletonCompositionWithGenericRootsInOtherProject51;
      transientIMyGenericService33 = localInstance_1182D127.GetMyService<int>();
      return new Program(transientIMyGenericService33);
    }
  }
}
```


