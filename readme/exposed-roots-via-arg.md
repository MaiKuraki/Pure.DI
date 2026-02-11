#### Exposed roots via arg

Composition roots from other assemblies or projects can be used as a source of bindings passed through composition arguments. When you add a binding to a composition from another assembly or project, the roots of the composition with the `RootKind.Exposed` type will be used in the bindings automatically. For example, in some assembly a composition is defined as:
```c#
public partial class CompositionInOtherProject
{
    private static void Setup() =>
        DI.Setup()
            .Bind().As(Lifetime.Singleton).To<MyDependency>()
            .Bind().To<MyService>()
            .Root<IMyService>("MyService", kind: RootKinds.Exposed);
}
```
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Pure.DI;
using OtherAssembly;

DI.Setup(nameof(Composition))
    // Binds to exposed composition roots from other project
    .Arg<CompositionInOtherProject>("baseComposition")
    .Root<Program>("Program");

var baseComposition = new CompositionInOtherProject();
var composition = new Composition(baseComposition);
var program = composition.Program;
program.DoSomething();

partial class Program(IMyService myService)
{
    public void DoSomething() => myService.DoSomething();
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
  private readonly Lock _lock;
#else
  private readonly Object _lock;
#endif

  private readonly OtherAssembly.CompositionInOtherProject _argBaseComposition;

  [OrdinalAttribute(128)]
  public Composition(OtherAssembly.CompositionInOtherProject baseComposition)
  {
    _argBaseComposition = baseComposition ?? throw new ArgumentNullException(nameof(baseComposition));
#if NET9_0_OR_GREATER
    _lock = new Lock();
#else
    _lock = new Object();
#endif
  }

  public Program Program
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      OtherAssembly.IMyService transientIMyService47;
      OtherAssembly.CompositionInOtherProject localInstance_1182D1273 = _argBaseComposition;
      transientIMyService47 = localInstance_1182D1273.MyService;
      return new Program(transientIMyService47);
    }
  }
}
```


