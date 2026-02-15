namespace Pure.DI.IntegrationTests;

using Core;

/// <summary>
/// Tests related to explicit default value handling.
/// </summary>
public class ExplicitDefaultValueTests
{
    [Fact]
    public async Task ShouldUseCtorWhenItHasDefaultValue()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IBox<out T> { T Content { get; } }
                           
                               interface ICat { }
                           
                               class CardboardBox<T> : IBox<T>
                               {
                                   public CardboardBox(T content) => Content = content;
                           
                                   public T Content { get; }
                           
                                   public override string ToString() => $"[{Content}]";
                               }
                           
                               class ShroedingersCat : ICat
                               {
                                   public ShroedingersCat(int id = 99)
                                   {
                                       Console.WriteLine(id);
                                   }
                               }
                           
                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<ICat>().To<ShroedingersCat>()
                                           .Bind<IBox<TT>>().To<CardboardBox<TT>>() 
                                           .Root<Program>("Root");
                                   }
                               }
                           
                               public class Program
                               {
                                   IBox<ICat> _box;
                           
                                   internal Program(IBox<ICat> box) => _box = box;
                           
                                   private void Run() { }
                           
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var program = composition.Root;
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["99"], result);
    }

    [Fact]
    public async Task ShouldUseCtorWhenItHasDefaultValueFromOtherAssembly()
    {
        // Given

        // When
        var result = await """
                               using System;
                               using Pure.DI;

                               namespace Sample
                               {
                                   interface IDependency
                                   {
                                   }
                                   
                                   sealed class Dependency : IDependency
                                   {
                                   }
                                   
                                   interface IService
                                   {
                                       ConsoleColor Color { get; }
                                   }
                                   
                                   class Service : IService
                                   {
                                       public ConsoleColor Color { get; }
                                   
                                       public Service(IDependency dependency, ConsoleColor color = ConsoleColor.DarkBlue)
                                       {
                                           Color = color;
                                       }
                                   }
                                   
                                   static class Setup
                                   {
                                       private static void SetupComposition()
                                       {
                                           DI.Setup(nameof(Composition))
                                               .Bind().To<Dependency>()
                                               .Bind().To<Service>()
                                               .Root<IService>("Root");
                                       }
                                   }
                                       
                                   public class Program
                                   {
                                       public static void Main()
                                       {
                                           var composition = new Composition();
                                           var root = composition.Root;
                                           Console.WriteLine(root.Color);
                                       }
                                   }
                               }
                               """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["DarkBlue"], result);
    }

    [Fact]
    public async Task ShouldUseCtorWhenItHasSeveralDefaultValuesFromOtherAssembly()
    {
        // Given

        // When
        var result = await """
                               using System;
                               using Pure.DI;

                               namespace Sample
                               {
                                   interface IDependency
                                   {
                                   }
                                   
                                   sealed class Dependency : IDependency
                                   {
                                   }
                                   
                                   interface IService
                                   {
                                       ConsoleColor Color1 { get; }
                                       ConsoleColor Color2 { get; }
                                   }
                                   
                                   class Service : IService
                                   {
                                       public ConsoleColor Color1 { get; }
                                       public ConsoleColor Color2 { get; }
                                   
                                       public Service(IDependency dependency, ConsoleColor color1 = ConsoleColor.DarkBlue, ConsoleColor color2 = ConsoleColor.Red)
                                       {
                                           Color1 = color1;
                                           Color2 = color2;
                                       }
                                   }
                                   
                                   static class Setup
                                   {
                                       private static void SetupComposition()
                                       {
                                           DI.Setup(nameof(Composition))
                                               .Bind().To<Dependency>()
                                               .Bind().To<Service>()
                                               .Root<IService>("Root");
                                       }
                                   }
                                       
                                   public class Program
                                   {
                                       public static void Main()
                                       {
                                           var composition = new Composition();
                                           var root = composition.Root;
                                           Console.WriteLine(root.Color1);
                                           Console.WriteLine(root.Color2);
                                       }
                                   }
                               }
                               """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["DarkBlue", "Red"], result);
    }

    [Fact]
    public async Task ShouldUseDefaultForStructWhenExplicitDefaultValueIsNull()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               struct Counter
                               {
                                   public int Value { get; }
                                   
                                   public Counter(int value) => Value = value;
                               }

                               class Service
                               {
                                   public Service(Counter counter = default)
                                   {
                                       Counter = counter;
                                   }

                                   public Counter Counter { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<Service>().To<Service>()
                                           .Root<Service>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var service = composition.Root;
                                       Console.WriteLine(service.Counter.Value);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["0"], result);
    }

    [Fact]
    public async Task ShouldReportMissingDependencyWhenOtherInitializerUsesExplicitDefaultValue()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               class Service
                               {
                                   public string First { get; private set; } = "";
                                   public string Second { get; private set; } = "";

                                   [Dependency]
                                   public void InitFirst(string value = "Default")
                                   {
                                       First = value;
                                   }

                                   [Dependency]
                                   public void InitSecond(string value)
                                   {
                                       Second = value;
                                   }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<Service>().To<Service>()
                                           .Root<Service>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var service = composition.Root;
                                       Console.WriteLine(service.First);
                                       Console.WriteLine(service.Second);
                                   }
                               }
                           }
                           """.RunAsync(new Options(LanguageVersion.CSharp9, CheckCompilationErrors: false));

        // Then
        result.Success.ShouldBeFalse(result);
        result.StdOut.Any(line => line.Contains("InitSecond", StringComparison.Ordinal)).ShouldBeTrue(result);
    }
}
