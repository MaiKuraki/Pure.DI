namespace Pure.DI.IntegrationTests;

using Core;

/// <summary>
/// Tests related to nullable reference type support in dependency contracts and generated code.
/// </summary>
public class NullableReferenceTypesTests
{
    [Theory]
    [InlineData(NullableContextOptions.Disable)]
    [InlineData(NullableContextOptions.Enable)]
    [InlineData(NullableContextOptions.Annotations)]
    [InlineData(NullableContextOptions.Warnings)]
    public async Task ShouldSupportNullableRootAndCompositionArguments(NullableContextOptions nullableContextOptions)
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;
                           using static Pure.DI.Tag;

                           namespace Sample
                           {
                               interface IService
                               {
                                   string? ConnectionString { get; }

                                   string? AppName { get; }
                               }

                               class Service: IService
                               {
                                   public Service([Tag("connection")] string? connectionString, [Tag("app")] string? appName)
                                   {
                                       ConnectionString = connectionString;
                                       AppName = appName;
                                   }

                                   public string? ConnectionString { get; }

                                   public string? AppName { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Hint(Hint.Resolve, "Off")
                                           .Bind<IService>().To<Service>()
                                           .Arg<string?>("appName", "app")
                                           .RootArg<string?>("connectionString", "connection")
                                           .Root<IService>("CreateService");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition(appName: null);
                                       var service = composition.CreateService(connectionString: null);
                                       Console.WriteLine(service.ConnectionString is null);
                                       Console.WriteLine(service.AppName is null);
                                   }
                               }
                           }
                           """.RunAsync(new Options { NullableContextOptions = nullableContextOptions });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True", "True"], result);
        result.GeneratedCode.Contains("public Composition(string? appName)", StringComparison.Ordinal).ShouldBeTrue(result);
        result.GeneratedCode.Contains("CreateService(string? connectionString)", StringComparison.Ordinal).ShouldBeTrue(result);
        result.GeneratedCode.Contains("if (connectionString == null)", StringComparison.Ordinal).ShouldBeFalse(result);
        result.GeneratedCode.Contains("throw new global::System.ArgumentNullException(nameof(appName))", StringComparison.Ordinal).ShouldBeFalse(result);
    }

    [Fact]
    public async Task ShouldSupportNullableConstructorDependenciesAndNullableGenerics()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using System.Collections.Generic;
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IDependency
                               {
                               }

                               class Dependency: IDependency
                               {
                               }

                               interface IService
                               {
                                   IDependency? OptionalDependency { get; }

                                   IReadOnlyList<IDependency?> Dependencies { get; }
                               }

                               class Service: IService
                               {
                                   public Service(IDependency? optionalDependency, Func<IDependency?> dependencyFactory, IEnumerable<IDependency?> dependencies)
                                   {
                                       OptionalDependency = optionalDependency;
                                       var result = new List<IDependency?> { dependencyFactory() };
                                       result.AddRange(dependencies);
                                       Dependencies = result;
                                   }

                                   public IDependency? OptionalDependency { get; }

                                   public IReadOnlyList<IDependency?> Dependencies { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<IDependency>().To<Dependency>()
                                           .Bind<IService>().To<Service>()
                                           .Root<IService>("Service");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var service = new Composition().Service;
                                       Console.WriteLine(service.OptionalDependency is not null);
                                       Console.WriteLine(service.Dependencies.Count > 0);
                                   }
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True", "True"], result);
        result.GeneratedCode.Contains("System.Func<global::Sample.IDependency?>", StringComparison.Ordinal).ShouldBeTrue(result);
        result.GeneratedCode.Contains("System.Collections.Generic.IEnumerable<global::Sample.IDependency?>", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldShowNullableTypesInMermaidDiagram()
    {
        // Given

        // When
        var result = await """
                           using Pure.DI;

                           namespace Sample
                           {
                               interface IDependency
                               {
                               }

                               class Dependency: IDependency
                               {
                               }

                               class Service
                               {
                                   public Service(IDependency? dependency)
                                   {
                                       Dependency = dependency;
                                   }

                                   public IDependency? Dependency { get; }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       // ToString = On
                                       DI.Setup("Composition")
                                           .Bind<IDependency>().To<Dependency>()
                                           .Root<Service>("Service");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var diagram = new Composition().ToString();
                                       System.Console.WriteLine(diagram.Contains("IDependencyɁ"));
                                       System.Console.WriteLine(diagram.Contains("IDependency?"));
                                   }
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True", "False"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableFieldInjection()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               [Dependency] public IDependency? Dependency;
                               public bool IsReady => Dependency is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullablePropertyInjection()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               [Dependency] public IDependency? Dependency { get; set; }
                               public bool IsReady => Dependency is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableMethodInjection()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               private IDependency? _dependency;
                               [Dependency] public void Init(IDependency? dependency) => _dependency = dependency;
                               public bool IsReady => _dependency is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableRequiredPropertyInjection()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               [Dependency]
                               public IDependency? Dependency { get; init; }
                               public bool IsReady => Dependency is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableFuncResult()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               private readonly Func<IDependency?> _factory;

                               public Service(Func<IDependency?> factory) => _factory = factory;

                               public bool IsReady => _factory() is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("System.Func<global::Sample.IDependency?>", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportNullableFuncArgumentAndResult()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency { string? Name { get; } }
                           class Dependency: IDependency
                           {
                               public Dependency(string? name) => Name = name;

                               public string? Name { get; }
                           }
                           class Service
                           {
                               private readonly Func<string?, IDependency?> _factory;

                               public Service(Func<string?, IDependency?> factory) => _factory = factory;

                               public bool IsReady => _factory(null)?.Name is null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("System.Func<string?, global::Sample.IDependency?>", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportNullableLazy()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               private readonly Lazy<IDependency?> _dependency;

                               public Service(Lazy<IDependency?> dependency) => _dependency = dependency;

                               public bool IsReady => _dependency.Value is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableEnumerable()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Collections.Generic;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               private readonly IEnumerable<IDependency?> _dependencies;

                               public Service(IEnumerable<IDependency?> dependencies) => _dependencies = dependencies;

                               public bool IsReady => _dependencies.Any(i => i is not null);
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("System.Collections.Generic.IEnumerable<global::Sample.IDependency?>", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportNullableReadOnlyList()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Collections.Generic;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               private readonly IReadOnlyList<IDependency?> _dependencies;

                               public Service(IReadOnlyList<IDependency?> dependencies) => _dependencies = dependencies;

                               public bool IsReady => _dependencies.Any(i => i is not null);
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("System.Collections.ObjectModel.ReadOnlyCollection<global::Sample.IDependency?>", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportNullableArray()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               private readonly IDependency?[] _dependencies;

                               public Service(IDependency?[] dependencies) => _dependencies = dependencies;

                               public bool IsReady => _dependencies.Any(i => i is not null);
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseNonNullableBindingForNullableRootContract()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IService { bool IsReady { get; } }
                           class Service: IService { public bool IsReady => true; }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService>().To<Service>()
                                       .Root<IService?>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("#nullable enable annotations", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportNullableRootArg()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? name) => IsReady = name is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Hint(Hint.Resolve, "Off")
                                       .RootArg<string?>("name")
                                       .Root<Service>("Create");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Create(null).IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("Create(string? name)", StringComparison.Ordinal).ShouldBeTrue(result);
        result.GeneratedCode.Contains("throw new global::System.ArgumentNullException(nameof(name))", StringComparison.Ordinal).ShouldBeFalse(result);
    }

    [Fact]
    public async Task ShouldSupportNullableCompositionArg()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? name) => IsReady = name is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Arg<string?>("name")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition(null).Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("public Composition(string? name)", StringComparison.Ordinal).ShouldBeTrue(result);
        result.GeneratedCode.Contains("throw new global::System.ArgumentNullException(nameof(name))", StringComparison.Ordinal).ShouldBeFalse(result);
    }

    [Fact]
    public async Task ShouldSupportNullableTaggedPrimitiveArg()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service([Tag("name")] string? name) => IsReady = name is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Arg<string?>("name", "name")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition(null).Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableAndNonNullableTaggedPrimitiveArgs()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service([Tag("optional")] string? optional, [Tag("required")] string required) =>
                                   IsReady = optional is null && required == "abc";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Arg<string?>("optional", "optional")
                                       .Arg<string>("required", "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition(null, "abc").Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldDistinguishNullableAndNonNullableReferenceTypeBindings()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value, string? optionalValue) =>
                                   IsReady = value == "required" && optionalValue is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldWarnWhenNullableReferenceTypeBindingIsNotUsed()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value) => IsReady = value == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count(i => i.Id == LogId.WarningBindingNotUsed && i.Locations.FirstOrDefault().GetSource() == "To(_ => (string?)null)").ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldWarnWhenNonNullableReferenceTypeBindingIsNotUsed()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count.ShouldBe(0, result);
        result.Warnings.Count(i => i.Id == LogId.WarningBindingNotUsed && i.Locations.FirstOrDefault().GetSource() == "To(_ => \"required\")").ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldNotUseNullableReferenceTypeBindingForNonNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => "optional")
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseNonNullableReferenceTypeBindingForNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseExactFactoryBindingForNullableAndNonNullableReferenceTypes()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value, string? optionalValue) =>
                                   IsReady = value == "required" && optionalValue == "optional";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(ctx => "required")
                                       .Bind<string?>().To(ctx => "optional")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseExactContextInjectTypeForNullableAndNonNullableReferenceTypes()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value, string? optionalValue) =>
                                   IsReady = value == "required" && optionalValue is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject<string>(out var value);
                                           ctx.Inject<string?>(out var optionalValue);
                                           return new Service(value, optionalValue);
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableReferenceTypeBindingForNonNullableContextInject()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => "optional")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject<string>(out var value);
                                           return new Service(value);
                                       })
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseNullableGenericBindingForNullableGenericDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IBox<T> { T Value { get; } }
                           class Box<T>: IBox<T>
                           {
                               public Box(T value) => Value = value;

                               public T Value { get; }
                           }
                           class Service
                           {
                               public Service(IBox<string?> box) => IsReady = box.Value is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Bind<IBox<TT>>().To<Box<TT>>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseNonNullableGenericBindingForNullableGenericDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IBox<T> { T Value { get; } }
                           class Box<T>: IBox<T>
                           {
                               public Box(T value) => Value = value;

                               public T Value { get; }
                           }
                           class Service
                           {
                               public Service(IBox<string?> box) => IsReady = box.Value == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Bind<IBox<TT>>().To<Box<TT>>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseNullableAndNonNullableReferenceTypeBindingsInBuilder()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               [Dependency] public string Value { get; set; } = "";
                               [Dependency] public string? OptionalValue { get; set; }
                               public bool IsReady => Value == "required" && OptionalValue is null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Builder<Service>("BuildUpService");
                               }
                           }

                           public class Program
                           {
                               public static void Main()
                               {
                                   var service = new Service();
                                   new Composition().BuildUpService(service);
                                   Console.WriteLine(service.IsReady);
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseNonNullableOverrideForNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value == "override";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Override<string>("override");
                                           ctx.Inject(out Service service);
                                           return service;
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableOverrideForNonNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Override<string?>((string?)"override");
                                           ctx.Inject(out Service service);
                                           return service;
                                       })
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseNonNullableLetForNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value == "let";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Let("let");
                                           ctx.Inject(out Service service);
                                           return service;
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableLetForNonNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Let((string?)"let");
                                           ctx.Inject(out Service service);
                                           return service;
                                       })
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseNonNullableCompositionArgForNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value == "arg";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Arg<string>("value")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition("arg").Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableCompositionArgForNonNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Arg<string?>("value")
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseExactNullableAndNonNullableCompositionArgs()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value, string? optionalValue) =>
                                   IsReady = value == "required" && optionalValue is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Arg<string>("value")
                                       .Arg<string?>("optionalValue")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition("required", null).Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseNonNullableRootArgForNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value == "root";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Hint(Hint.Resolve, "Off")
                                       .RootArg<string>("value")
                                       .Root<Service>("CreateService");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().CreateService("root").IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableRootArgForNonNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Hint(Hint.Resolve, "Off")
                                       .RootArg<string?>("value")
                                       .Root<Service>("CreateService");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseTaggedNonNullableBindingForTaggedNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service([Tag("name")] string? value) => IsReady = value == "tagged";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>("name").To(_ => "tagged")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseTaggedNullableBindingForTaggedNonNullableDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service([Tag("name")] string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>("name").To(_ => "tagged")
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseExactTaggedNullableAndNonNullableBindings()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service([Tag("name")] string value, [Tag("name")] string? optionalValue) =>
                                   IsReady = value == "required" && optionalValue is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>("name").To(_ => "required")
                                       .Bind<string?>("name").To(_ => (string?)null)
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseTaggedNonNullableBindingForTaggedNullableContextInject()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value == "tagged";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>("name").To(_ => "tagged")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject<string?>("name", out var value);
                                           return new Service(value);
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseTaggedNullableBindingForTaggedNonNullableContextInject()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>("name").To(_ => "tagged")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject<string>("name", out var value);
                                           return new Service(value);
                                       })
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldSupportNullableRootContract()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IService
                           {
                           }
                           class Service: IService
                           {
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService>().To<Service>()
                                       .Root<IService?>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service is not null);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("IService?", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldNotUseNullableBindingForNonNullableRootContract()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           interface IService
                           {
                           }
                           class Service: IService
                           {
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService?>().To<Service>()
                                       .Root<IService>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldResolveNullableReferenceTypeRoot()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IService
                           {
                           }
                           class Service: IService
                           {
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService>().To<Service>()
                                       .Root<IService?>();
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Resolve<IService?>() is not null);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableReferenceTypeRootWithResolveByRuntimeType()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IService
                           {
                           }
                           class Service: IService
                           {
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService>().To<Service>()
                                       .Root<IService?>();
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Resolve(typeof(IService)) is not null);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldInjectNullableEnumerableFromNonNullableBinding()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Collections.Generic;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(IEnumerable<string?> values) => IsReady = values.Single() == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotInjectNonNullableEnumerableFromNullableBinding()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;
                           using System.Collections.Generic;

                           namespace Sample;

                           class Service
                           {
                               public Service(IEnumerable<string> values)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => "optional")
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldInjectNullableArrayFromNonNullableBinding()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string?[] values) => IsReady = values.Single() == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldInjectNullableReadOnlyListFromNonNullableBinding()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Collections.Generic;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(IReadOnlyList<string?> values) => IsReady = values.Single() == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseNonNullableBindingForNullableFunc()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(Func<string?> factory) => IsReady = factory() == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableBindingForNonNullableFunc()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(Func<string> factory)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => "optional")
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseNonNullableBindingForNullableLazy()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(Lazy<string?> lazy) => IsReady = lazy.Value == "required";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string>().To(_ => "required")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableBindingForNonNullableLazy()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(Lazy<string> lazy)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => "optional")
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldSupportNullableContextInjectWithOutVariableType()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value) => IsReady = value is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => (string?)null)
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject(out string? value);
                                           return new Service(value);
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldNotUseNullableBindingForNonNullableContextInjectWithOutVariableType()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value)
                               {
                               }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>().To(_ => "optional")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject(out string value);
                                           return new Service(value);
                                       })
                                       .Root<Service>("Service");
                               }
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeFalse(result);
        result.Errors.Count(i => i.Id == LogId.ErrorUnableToResolve).ShouldBe(1, result);
    }

    [Fact]
    public async Task ShouldUseAutoBindingForNullableConcreteDependency()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Dependency
                           {
                           }
                           class Service
                           {
                               public Service(Dependency? dependency) => IsReady = dependency is not null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseExplicitDefaultForNullableReferenceTypeWhenBindingIsMissing()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? value = null) => IsReady = value is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldUseExplicitDefaultForNonNullableReferenceTypeWhenBindingIsMissing()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string value = "default") => IsReady = value == "default";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableExplicitDefaultValue()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Service
                           {
                               public Service(IDependency? dependency = null) => IsReady = dependency is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableContextInject()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               public Service(IDependency? dependency) => IsReady = dependency is not null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject<IDependency?>(out var dependency);
                                           return new Service(dependency);
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableTaggedContextInject()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? name) => IsReady = name == "abc";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<string?>("name").To(_ => "abc")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Inject<string?>("name", out var name);
                                           return new Service(name);
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableBuildUpPropertyInFactory()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               [Dependency] public IDependency? Dependency { get; set; }
                               public bool IsReady => Dependency is not null;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Bind<Service>().To(ctx =>
                                       {
                                           var service = new Service();
                                           ctx.BuildUp(service);
                                           return service;
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableGenericContract()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           interface IBox<T> { T Value { get; } }
                           class Box<T>: IBox<T>
                           {
                               public Box(T value) => Value = value;

                               public T Value { get; }
                           }
                           class Service
                           {
                               public Service(IBox<IDependency?> box) => IsReady = box.Value is not null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Bind<IBox<TT>>().To<Box<TT>>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableNestedGenericContract()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using System.Collections.Generic;
                           using System.Linq;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           interface IBox<T> { T Value { get; } }
                           class Box<T>: IBox<T>
                           {
                               public Box(T value) => Value = value;

                               public T Value { get; }
                           }
                           class Service
                           {
                               public Service(IBox<IEnumerable<IDependency?>> box) =>
                                   IsReady = box.Value.Any(i => i is not null);

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Bind<IBox<TT>>().To<Box<TT>>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableTupleElement()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               public Service((IDependency? Dependency, string? Name) value) =>
                                   IsReady = value.Dependency is not null && value.Name == "abc";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Bind<string>().To(_ => "abc")
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableAutoBindingRoot()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public bool IsReady => true;
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Root<Service?>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service?.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableResolveMethod()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IService { bool IsReady { get; } }
                           class Service: IService { public bool IsReady => true; }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService>().To<Service>()
                                       .Root<IService?>();
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Resolve<IService?>()!.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableFactoryReturn()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IService { bool IsReady { get; } }
                           class Service: IService { public bool IsReady => true; }
                           class Consumer
                           {
                               public Consumer(IService? service) => IsReady = service is not null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IService?>().To(_ => new Service())
                                       .Root<Consumer>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullablePerBlockLifetime()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               public Service(IDependency? first, IDependency? second) =>
                                   IsReady = first is not null && ReferenceEquals(first, second);

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().As(Lifetime.PerBlock).To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableSingletonLifetime()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               public Service(IDependency? first, IDependency? second) =>
                                   IsReady = first is not null && ReferenceEquals(first, second);

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().As(Lifetime.Singleton).To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableScopedLifetime()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               public Service(IDependency? first, IDependency? second) =>
                                   IsReady = first is not null && ReferenceEquals(first, second);

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().As(Lifetime.Scoped).To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldSupportNullableOverride()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           class Service
                           {
                               public Service(string? name) => IsReady = name == "abc";

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<Service>().To(ctx =>
                                       {
                                           ctx.Override<string>("abc");
                                           ctx.Inject(out Service service);
                                           return service;
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

    [Fact]
    public async Task ShouldShowNullableConstructorDependencyInGeneratedComments()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency {}
                           class Dependency: IDependency {}
                           class Service
                           {
                               public Service(IDependency? dependency) => IsReady = dependency is not null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Bind<IDependency>().To<Dependency>()
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service.IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
        result.GeneratedCode.Contains("IDependency?", StringComparison.Ordinal).ShouldBeTrue(result);
    }

    [Fact]
    public async Task ShouldSupportNullableGenericFactoryWithExplicitInject()
    {
        // Given

        // When
        var result = await """
                           #nullable enable annotations
                           using System;
                           using Pure.DI;

                           namespace Sample;

                           interface IDependency<T> { T Value { get; } }
                           class Dependency<T>: IDependency<T>
                           {
                               public Dependency(T value) => Value = value;

                               public T Value { get; }
                           }
                           class Service
                           {
                               public Service(IDependency<string?> dependency) => IsReady = dependency.Value is null;

                               public bool IsReady { get; }
                           }

                           static class Setup
                           {
                               private static void SetupComposition()
                               {
                                   DI.Setup("Composition")
                                       .Hint(Hint.Resolve, "Off")
                                       .RootArg<string?>("value")
                                       .Bind<IDependency<TT>>().To(ctx =>
                                       {
                                           ctx.Inject<TT>(out var value);
                                           return new Dependency<TT>(value);
                                       })
                                       .Root<Service>("Service");
                               }
                           }

                           public class Program
                           {
                               public static void Main() => Console.WriteLine(new Composition().Service(null).IsReady);
                           }
                           """.RunAsync(new Options { LanguageVersion = LanguageVersion.CSharp10 });

        // Then
        result.Success.ShouldBeTrue(result);
        result.StdOut.ShouldBe(["True"], result);
    }

}
