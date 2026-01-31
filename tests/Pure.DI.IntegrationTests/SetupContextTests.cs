namespace Pure.DI.IntegrationTests;

using Core;

public class SetupContextTests
{
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

                                   internal partial int GetValue() => Value;
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
    public async Task ShouldSupportMembersWhenCompositionInheritsBaseComposition()
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
                                   // Public members
                                   public int PublicField = 1;
                                   public int PublicProperty { get; set; } = 2;
                                   public int PublicMethod() => 3;

                                   // Internal members
                                   internal int InternalField = 4;
                                   internal int InternalProperty { get; set; } = 5;
                                   internal int InternalMethod() => 6;

                                   // Private members
                                   private int PrivateField = 7;
                                   private int PrivateProperty { get; set; } = 8;
                                   private int PrivateMethod() => 9;
                                   
                                   // These fields are out of composition roots in Composition, so generator should skip them
                                   #pragma warning disable
                                   private int PrivateFieldOutOfBinding = 33;
                                   private int PrivatePropertyOutOfBinding { get; set; } = 34;
                                   private int PrivateMethodOutOfBinding() => 45;
                                   #pragma warning restore

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>("publicField").To(_ => PublicField)
                                           .Bind<int>("publicProperty").To(_ => PublicProperty)
                                           .Bind<int>("publicMethod").To(_ => PublicMethod())
                                           .Bind<int>("internalField").To(_ => InternalField)
                                           .Bind<int>("internalProperty").To(_ => InternalProperty)
                                           .Bind<int>("internalMethod").To(_ => InternalMethod())
                                           .Bind<int>("privateField").To(_ => PrivateField)
                                           .Bind<int>("privateProperty").To(_ => PrivateProperty)
                                           .Bind<int>("privateMethod").To(_ => PrivateMethod());
                                   }
                               }

                               internal partial class Composition : BaseComposition
                               {
                                   private void Setup()
                                   {
                                       DI.Setup(nameof(Composition))
                                           .DependsOn(nameof(BaseComposition), SetupContextKind.Members)
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                                   
                                   private partial int PrivateMethod() => 9;
                               }

                               interface IService
                               {
                                   int Total { get; }
                               }

                               class Service : IService
                               {
                                   private readonly int _publicField;
                                   private readonly int _publicProperty;
                                   private readonly int _publicMethod;
                                   private readonly int _internalField;
                                   private readonly int _internalProperty;
                                   private readonly int _internalMethod;
                                   private readonly int _privateField;
                                   private readonly int _privateProperty;
                                   private readonly int _privateMethod;

                                   public Service(
                                       [Tag("publicField")] int publicField,
                                       [Tag("publicProperty")] int publicProperty,
                                       [Tag("publicMethod")] int publicMethod,
                                       [Tag("internalField")] int internalField,
                                       [Tag("internalProperty")] int internalProperty,
                                       [Tag("internalMethod")] int internalMethod,
                                       [Tag("privateField")] int privateField,
                                       [Tag("privateProperty")] int privateProperty,
                                       [Tag("privateMethod")] int privateMethod)
                                   {
                                       _publicField = publicField;
                                       _publicProperty = publicProperty;
                                       _publicMethod = publicMethod;
                                       _internalField = internalField;
                                       _internalProperty = internalProperty;
                                       _internalMethod = internalMethod;
                                       _privateField = privateField;
                                       _privateProperty = privateProperty;
                                       _privateMethod = privateMethod;
                                   }

                                   public int Total => _publicField + _publicProperty + _publicMethod
                                       + _internalField + _internalProperty + _internalMethod
                                       + _privateField + _privateProperty + _privateMethod;
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       Console.WriteLine(composition.Service.Total);
                                   }
                               }
                           }
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["45"], result);
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

                                   internal partial int GetValue() => Settings.Value;
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
        result.GeneratedCode.Contains("return Settings.Value").ShouldBeFalse(result);
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

                                   internal partial int GetValue() => SettingsField.Value + SettingsProperty.Value;
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
        result.GeneratedCode.Contains("return SettingsField.Value").ShouldBeFalse(result);
    }

    [Fact]
    public async Task ShouldCreatePartialAccessorMethodsForPropertyLogic()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace External
                           {
                               internal sealed class Counter
                               {
                                   public int Value { get; set; }
                               }

                               [AttributeUsage(AttributeTargets.Property)]
                               internal sealed class ExternalMarkerAttribute : Attribute {}
                           }

                           namespace Sample
                           {
                               using External;

                               internal partial class BaseComposition
                               {
                                   private readonly Counter _counter = new Counter();

                                   // external counter property
                                   [ExternalMarker]
                                   internal int Count
                                   {
                                       get => _counter.Value;
                                       set => _counter.Value = value;
                                   }

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => Count);
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

                                   internal partial int get_CountCore() => _counter.Value;

                                   internal partial void set_CountCore(int value) => _counter.Value = value;
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
                                       composition.Count = 9;
                                       Console.WriteLine(composition.Service.Value);
                                   }
                               }
                           }
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["9"], result);
        result.GeneratedCode.Contains("global::External.ExternalMarkerAttribute").ShouldBeTrue(result);
        result.GeneratedCode.Contains("global::External.Counter").ShouldBeTrue(result);
        result.GeneratedCode.Contains("// external counter property").ShouldBeTrue(result);
        result.GeneratedCode.Contains("get => get_CountCore()").ShouldBeTrue(result);
        result.GeneratedCode.Contains("set => set_CountCore(value)").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial int get_CountCore();").ShouldBeTrue(result);
        result.GeneratedCode.Contains("partial void set_CountCore").ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportOverridesForMembers()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               internal abstract class BaseSettings
                               {
                                   protected int ValueField;

                                   public virtual int Value
                                   {
                                       get => ValueField;
                                       set => ValueField = value;
                                   }

                                   public virtual int GetValue() => Value;
                               }

                               internal partial class BaseComposition : BaseSettings
                               {
                                   public override int Value
                                   {
                                       get => base.Value;
                                       set => base.Value = value + 1;
                                   }

                                   public override int GetValue() => Value * 2;

                                   private void Setup()
                                   {
                                       DI.Setup(nameof(BaseComposition), CompositionKind.Internal)
                                           .Bind<int>().To(_ => GetValue());
                                   }
                               }

                               internal partial class Composition : BaseComposition
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
                                       var composition = new Composition();
                                       composition.Value = 3;
                                       Console.WriteLine(composition.Service.Value);
                                   }
                               }
                           }
                           """.RunAsync(new Options(LanguageVersion: LanguageVersion.CSharp9));

        // Then
        result.Success.ShouldBeTrue(result.Errors.FirstOrDefault().Exception?.ToString() ?? result.ToString());
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count.ShouldBe(0, result);
        result.StdOut.ShouldBe(["8"], result);
    }
}