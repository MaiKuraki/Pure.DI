namespace Pure.DI.IntegrationTests;

using Core;

public class SetupContextTests
{
    [Fact]
    public async Task ShouldSupportMultipleSetupContextsWithMixedKinds()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal partial class BaseCompositionA
                               {
                                   internal int Value { get; set; }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseCompositionA), CompositionKind.Internal)
                                           .Bind<int>().To(_ => Value);
                                   }
                               }

                               internal partial class BaseCompositionB
                               {
                                   internal string Name { get; set; } = "";

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseCompositionB), CompositionKind.Internal)
                                           .Bind<string>().To(_ => Name);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseCompositionA), "baseA", SetupContextKind.Field)
                                           .DependsOn(nameof(BaseCompositionB), "baseB", SetupContextKind.Property)
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                               }

                               interface IService
                               {
                                   string Report { get; }
                               }

                               class Service : IService
                               {
                                   public Service(int value, string name)
                                   {
                                       Report = $"{name}:{value}";
                                   }

                                   public string Report { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var baseA = new BaseCompositionA { Value = 9 };
                                       var baseB = new BaseCompositionB { Name = "prod" };
                                       var composition = new Composition
                                       {
                                           baseA = baseA,
                                           baseB = baseB
                                       };
                                       Console.WriteLine(composition.Service.Report);
                                   }
                               }
                           }
                           """.RunAsync(new Options(CheckCompilationErrors: false));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["prod:9"], result);
    }

    [Fact]
    public async Task ShouldFailWhenContextArgumentIsMissingForArgumentKind()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal partial class BaseComposition
                               {
                                   internal int Value { get; set; }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => Value);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), "baseContext")
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                               }

                               interface IService
                               {
                                   int Value { get; }
                               }

                               class Service : IService
                               {
                                   public Service(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       Console.WriteLine(composition.Service.Value);
                                   }
                               }
                           }
                           """.RunAsync(new Options(CheckCompilationErrors: false));

        // Then
        result.Success.ShouldBeFalse(result);
    }

    [Fact]
    public async Task ShouldFailCompilationWhenInstanceMemberUsedWithoutSetupContext()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal partial class BaseComposition
                               {
                                   private int Value { get; } = 7;

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => Value);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition))
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                               }

                               interface IService
                               {
                                   int Value { get; }
                               }

                               class Service : IService
                               {
                                   public Service(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       Console.WriteLine(composition.Service.Value);
                                   }
                               }
                           }
                           """.RunAsync(new Options(CheckCompilationErrors: false));

        // Then
        result.Success.ShouldBeFalse(result);
        result.Warnings.Count(i => i.Id == LogId.WarningInstanceMemberInDependsOnSetup).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldSupportRootArgumentWithSimpleFactory()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal partial class BaseComposition
                               {
                                   internal int Value { get; set; }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(() => Value);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       // Resolve = Off
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), "baseContext", SetupContextKind.RootArgument)
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                               }

                               interface IService
                               {
                                   int Value { get; }
                               }

                               class Service : IService
                               {
                                   public Service(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var baseContext = new BaseComposition { Value = 41 };
                                       var composition = new Composition();
                                       Console.WriteLine(composition.Service(baseContext: baseContext).Value);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["41"], result);
    }

    [Fact]
    public async Task ShouldSupportMethodRootWithSetupContextField()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal partial class BaseComposition
                               {
                                   internal int Value { get; set; }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => Value);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), "baseContext", SetupContextKind.Field)
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service", kind: RootKinds.Method);
                                   }
                               }

                               interface IService
                               {
                                   int Value { get; }
                               }

                               class Service : IService
                               {
                                   public Service(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var baseContext = new BaseComposition { Value = 12 };
                                       var composition = new Composition { baseContext = baseContext };
                                       Console.WriteLine(composition.Service().Value);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["12"], result);
    }

    [Fact]
    public async Task ShouldSupportMethodRootWithSetupContextProperty()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal partial class BaseComposition
                               {
                                   internal int Value { get; set; }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => Value);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), "baseContext", SetupContextKind.Property)
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service", kind: RootKinds.Method);
                                   }
                               }

                               interface IService
                               {
                                   int Value { get; }
                               }

                               class Service : IService
                               {
                                   public Service(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var baseContext = new BaseComposition { Value = 33 };
                                       var composition = new Composition { baseContext = baseContext };
                                       Console.WriteLine(composition.Service().Value);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["33"], result);
    }
}
