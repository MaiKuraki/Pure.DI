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
                                           .DependsOn(nameof(BaseCompositionA), SetupContextKind.Field, "baseA")
                                           .DependsOn(nameof(BaseCompositionB), SetupContextKind.Property, "baseB")
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
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Argument, "baseContext")
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
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.RootArgument, "baseContext")
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
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["41"], result);
    }

    [Fact]
    public async Task ShouldShowErrorWhenSetupContextNameIsMissing()
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
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal);
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Field)
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                               }

                               interface IService {}
                               class Service : IService {}

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       Console.WriteLine(composition.Service);
                                   }
                               }
                           }
                           """.RunAsync(new Options(CheckCompilationErrors: false));

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorSetupContextNameIsRequired).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldAllowMissingNameForMembersContext()
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
                                   // base value
                                   internal int Value { get; set; } = 2;

                                   internal int GetValue() => Value;

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => GetValue());
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Members)
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
                                       var composition = new Composition { Value = 5 };
                                       Console.WriteLine(composition.Service.Value);
                                   }
                               }
                           }
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["5"], result);
        result.GeneratedCode.Contains("// base value").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial int GetValue();").ShouldBeTrue(result);
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
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Field, "baseContext")
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
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

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
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Property, "baseContext")
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
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["33"], result);
    }

    [Fact]
    public async Task ShouldSupportSetupContextMembers()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal sealed class Settings
                               {
                                   public Settings(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               [AttributeUsage(AttributeTargets.Field)]
                               internal sealed class MarkerAttribute : Attribute {}

                               internal partial class BaseComposition
                               {
                                   // Settings holder
                                   [Marker]
                                   internal Settings Settings = new Settings(3);

                                   internal int GetValue()
                                   {
                                       return Settings.Value;
                                   }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => GetValue());
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Members, "baseContext")
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
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["3"], result);
        result.GeneratedCode.Contains("global::Sample.MarkerAttribute").ShouldBeTrue(result);
        result.GeneratedCode.Contains("global::Sample.Settings").ShouldBeTrue(result);
        result.GeneratedCode.Contains("// Settings holder").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial int GetValue();").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial int GetValue()").ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldCopyMembersWithCommentsAttributesAndExternalTypes()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace External
                           {
                               internal sealed class ExternalSettings
                               {
                                   public ExternalSettings(int value)
                                   {
                                       Value = value;
                                   }

                                   public int Value { get; }
                               }

                               [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
                               internal sealed class ExternalMarkerAttribute : Attribute {}
                           }

                           namespace Sample
                           {
                               using External;

                               internal partial class BaseComposition
                               {
                                   // external settings field
                                   [ExternalMarker]
                                   internal ExternalSettings SettingsField = new ExternalSettings(5);

                                   // external settings property
                                   [ExternalMarker]
                                   internal ExternalSettings SettingsProperty { get; } = new ExternalSettings(7);

                                   // external settings method
                                   [ExternalMarker]
                                   internal int GetValue()
                                   {
                                       return SettingsField.Value + SettingsProperty.Value;
                                   }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => GetValue());
                                   }
                               }

                               internal partial class Composition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Members, "baseContext")
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
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["12"], result);
        result.GeneratedCode.Contains("global::External.ExternalMarkerAttribute").ShouldBeTrue(result);
        result.GeneratedCode.Contains("global::External.ExternalSettings").ShouldBeTrue(result);
        result.GeneratedCode.Contains("// external settings field").ShouldBeTrue(result);
        result.GeneratedCode.Contains("// external settings property").ShouldBeTrue(result);
        result.GeneratedCode.Contains("// external settings method").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial int GetValue();").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial int GetValue()").ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportMixedSetupContextsWithMembers()
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
                                   internal int Factor { get; set; }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseCompositionA), CompositionKind.Internal)
                                           .Bind<int>("factor").To(_ => Factor);
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
                                           .DependsOn(nameof(BaseCompositionA), SetupContextKind.Members, "baseA")
                                           .DependsOn(nameof(BaseCompositionB), SetupContextKind.Field, "baseB")
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
                                   public Service([Tag("factor")] int factor, string name)
                                   {
                                       Report = $"{name}:{factor}";
                                   }

                                   public string Report { get; }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var baseB = new BaseCompositionB { Name = "stage" };
                                       var composition = new Composition
                                       {
                                           baseB = baseB,
                                           Factor = 4
                                       };
                                       Console.WriteLine(composition.Service.Report);
                                   }
                               }
                           }
                           """.RunAsync();

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["stage:4"], result);
    }
}

