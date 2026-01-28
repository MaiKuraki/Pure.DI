namespace Pure.DI.IntegrationTests;

/// <summary>
/// Tests for variable map scoping behavior across nested code generation scopes.
/// </summary>
public class VarsMapTests
{
    [Fact]
    public async Task ShouldNotReusePerBlockBetweenBlocks()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IPerBlock {}

                               class PerBlockDep : IPerBlock {}

                               class ServiceA
                               {
                                   public ServiceA(IPerBlock dep) => Dep = dep;

                                   public IPerBlock Dep { get; }
                               }

                               class ServiceB
                               {
                                   public ServiceB(IPerBlock dep) => Dep = dep;

                                   public IPerBlock Dep { get; }
                               }

                               class Root
                               {
                                   public Root(ServiceA a, ServiceB b)
                                   {
                                       A = a;
                                       B = b;
                                   }

                                   public ServiceA A { get; }

                                   public ServiceB B { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<IPerBlock>().As(Lifetime.PerBlock).To<PerBlockDep>()
                                           .Bind().As(Lifetime.Singleton).To<ServiceA>()
                                           .Bind().As(Lifetime.Singleton).To<ServiceB>()
                                           .Bind().To<Root>()
                                           .Root<Root>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var root = composition.Root;
                                       Console.WriteLine(ReferenceEquals(root.A.Dep, root.B.Dep));
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["False"], result);
    }

    [Fact]
    public async Task ShouldNotReusePerBlockBetweenLocalFunctionScopes()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IPerBlock {}

                               class PerBlockDep : IPerBlock {}

                               interface IOther {}

                               class Other : IOther {}

                               class ServiceA
                               {
                                   public ServiceA(IPerBlock dep, IOther other) => Dep = dep;

                                   public IPerBlock Dep { get; }
                               }

                               class ServiceB
                               {
                                   public ServiceB(IPerBlock dep) => Dep = dep;

                                   public IPerBlock Dep { get; }
                               }

                               class Root
                               {
                                   public Root(ServiceA a, ServiceB b)
                                   {
                                       A = a;
                                       B = b;
                                   }

                                   public ServiceA A { get; }

                                   public ServiceB B { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<IPerBlock>().As(Lifetime.PerBlock).To<PerBlockDep>()
                                           .Bind<IOther>().To<Other>()
                                           .Bind().As(Lifetime.Singleton).To<ServiceA>()
                                           .Bind().As(Lifetime.Singleton).To<ServiceB>()
                                           .Bind().To<Root>()
                                           .Root<Root>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var root = composition.Root;
                                       Console.WriteLine(ReferenceEquals(root.A.Dep, root.B.Dep));
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["False"], result);
    }

    [Fact]
    public async Task ShouldNotReusePerBlockFromLazyScope()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IPerBlock {}

                               class PerBlockDep : IPerBlock {}

                               class ServiceA
                               {
                                   public ServiceA(Lazy<IPerBlock> dep) => Dep = dep.Value;

                                   public IPerBlock Dep { get; }
                               }

                               class ServiceB
                               {
                                   public ServiceB(IPerBlock dep) => Dep = dep;

                                   public IPerBlock Dep { get; }
                               }

                               class Root
                               {
                                   public Root(ServiceA a, ServiceB b)
                                   {
                                       A = a;
                                       B = b;
                                   }

                                   public ServiceA A { get; }

                                   public ServiceB B { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<IPerBlock>().As(Lifetime.PerBlock).To<PerBlockDep>()
                                           .Bind().As(Lifetime.Singleton).To<ServiceA>()
                                           .Bind().As(Lifetime.Singleton).To<ServiceB>()
                                           .Bind().To<Root>()
                                           .Root<Root>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var root = composition.Root;
                                       Console.WriteLine(ReferenceEquals(root.A.Dep, root.B.Dep));
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["False"], result);
    }
}
