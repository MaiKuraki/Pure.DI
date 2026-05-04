// ReSharper disable once CheckNamespace
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
namespace Pure.DI.UsageTests.IntegrationTests.MicrosoftDependencyInjectionTests;

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Pure.DI;
using MS;

public class MicrosoftDependencyInjectionTests
{
    [Fact]
    public void ShouldNotFallbackToUntaggedServiceForTaggedExternalDependency()
    {
        var composition = new TaggedExternalComposition
        {
            ServiceProvider = new ServiceCollection().BuildServiceProvider()
        };

        Should.Throw<CannotResolveException>(() => _ = composition.ExternalConsumer);
    }

    [Fact]
    public void ShouldRegisterValueTypeRootInServiceCollection()
    {
        var composition = new ValueTypeRootComposition();
        var serviceProvider = composition.ServiceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<int>().ShouldBe(42);
    }

    [Fact]
    public void ShouldExportPureDiRootLifetimesAsMicrosoftDescriptors()
    {
        var serviceCollection = new LifetimeComposition().ServiceCollection;

        serviceCollection.Single(descriptor => descriptor.ServiceType == typeof(ISingletonService)).Lifetime.ShouldBe(ServiceLifetime.Singleton);
        serviceCollection.Single(descriptor => descriptor.ServiceType == typeof(IScopedService)).Lifetime.ShouldBe(ServiceLifetime.Scoped);
        serviceCollection.Single(descriptor => descriptor.ServiceType == typeof(ITransientService)).Lifetime.ShouldBe(ServiceLifetime.Transient);
    }

    [Fact]
    public void ShouldResolveServicesAccordingToMicrosoftLifetimes()
    {
        using var serviceProvider = new LifetimeComposition().ServiceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<ISingletonService>().ShouldBe(serviceProvider.GetRequiredService<ISingletonService>());
        serviceProvider.GetRequiredService<IScopedService>().ShouldBe(serviceProvider.GetRequiredService<IScopedService>());
        serviceProvider.GetRequiredService<ITransientService>().ShouldNotBe(serviceProvider.GetRequiredService<ITransientService>());
    }

    [Fact]
    public void ShouldDisposePureDiSingletonOnceWhenResolvedMultipleTimesFromMicrosoftProvider()
    {
        DisposableSingletonService.DisposeCount = 0;
        var serviceProvider = new LifetimeComposition().ServiceCollection.BuildServiceProvider();

        serviceProvider.GetRequiredService<ISingletonService>().ShouldBe(serviceProvider.GetRequiredService<ISingletonService>());

        serviceProvider.Dispose();

        DisposableSingletonService.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void ShouldCreatePureDiScopedRootPerMicrosoftScope()
    {
        using var serviceProvider = new LifetimeComposition().ServiceCollection.BuildServiceProvider();
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        scope1.ServiceProvider.GetRequiredService<IScopedService>()
            .ShouldNotBe(scope2.ServiceProvider.GetRequiredService<IScopedService>());
    }

    [Fact]
    public void ShouldReusePureDiScopedRootWithinMicrosoftScope()
    {
        using var serviceProvider = new LifetimeComposition().ServiceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IScopedService>()
            .ShouldBe(scope.ServiceProvider.GetRequiredService<IScopedService>());
    }

    [Fact]
    public void ShouldDisposePureDiScopedRootOnceWhenMicrosoftScopeIsDisposed()
    {
        DisposableScopedService.DisposeCount = 0;
        var serviceProvider = new LifetimeComposition().ServiceCollection.BuildServiceProvider();
        var scope = serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IScopedService>()
            .ShouldBe(scope.ServiceProvider.GetRequiredService<IScopedService>());

        scope.Dispose();
        serviceProvider.Dispose();

        DisposableScopedService.DisposeCount.ShouldBe(1);
    }
}

interface IExternalConsumer
{
    IServiceScopeFactory ScopeFactory { get; }
}

class ExternalConsumer(
    [Tag("External")] IServiceScopeFactory scopeFactory)
    : IExternalConsumer
{
    public IServiceScopeFactory ScopeFactory { get; } = scopeFactory;
}

partial class TaggedExternalComposition : ServiceProviderFactory<TaggedExternalComposition>
{
    private static void Setup() =>
        DI.Setup()
            .Bind<IExternalConsumer>().To<ExternalConsumer>()
            .Root<IExternalConsumer>("ExternalConsumer");
}

partial class ValueTypeRootComposition : ServiceProviderFactory<ValueTypeRootComposition>
{
    public IServiceCollection ServiceCollection =>
        CreateServiceCollection(this);

    private static void Setup() =>
        DI.Setup()
            .Bind<int>().To(_ => 42)
            .Root<int>("Number");
}

interface ISingletonService;

interface IScopedService;

interface ITransientService;

class DisposableSingletonService : ISingletonService, IDisposable
{
    public static int DisposeCount;

    public void Dispose() => DisposeCount++;
}

class DisposableScopedService : IScopedService, IDisposable
{
    public static int DisposeCount;

    public void Dispose() => DisposeCount++;
}

class TransientService : ITransientService;

partial class LifetimeComposition : ServiceProviderFactory<LifetimeComposition>
{
    public IServiceCollection ServiceCollection =>
        CreateServiceCollection(this);

    private static void Setup() =>
        DI.Setup()
            .Bind<ISingletonService>().As(Lifetime.Singleton).To<DisposableSingletonService>()
            .Bind<IScopedService>().As(Lifetime.Scoped).To<DisposableScopedService>()
            .Bind<ITransientService>().As(Lifetime.Transient).To<TransientService>()
            .Root<ISingletonService>("Singleton")
            .Root<IScopedService>("Scoped")
            .Root<ITransientService>("Transient");
}
