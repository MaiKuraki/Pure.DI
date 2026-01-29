namespace Pure.DI.IntegrationTests;

/// <summary>
/// Integration tests for factory rewriting behaviors.
/// </summary>
public class FactoryRewriterIntegrationTests
{
    [Fact]
    public async Task ShouldIgnoreShadowedContextInNestedLambda()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               class FakeContext
                               {
                                   public void Inject(out int value) => value = 13;
                               }

                               class Service
                               {
                                   public Service(int value) => Value = value;

                                   public int Value { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind().To<Service>(ctx =>
                                           {
                                               Func<FakeContext, int> createValue = ctx =>
                                               {
                                                   ctx.Inject(out var value);
                                                   return value;
                                               };

                                               var fake = new FakeContext();
                                               return new Service(createValue(fake));
                                           })
                                           .Root<Service>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var service = composition.Root;
                                       Console.WriteLine(service.Value);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["13"], result);
    }

    [Fact]
    public async Task ShouldSupportDiscardInjection()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IDependency { }

                               class Dependency : IDependency
                               {
                                   public static int Count;

                                   public Dependency() => Count++;
                               }

                               class Service { }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<IDependency>().To<Dependency>()
                                           .Bind().To<Service>(ctx =>
                                           {
                                               ctx.Inject(out IDependency _);
                                               return new Service();
                                           })
                                           .Root<Service>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var service = composition.Root;
                                       Console.WriteLine(Dependency.Count);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["1"], result);
    }
}
