using Build.Core;
using Build.Core.Targets;

DI.Setup(nameof(Composition))
    .Hint(Hint.ThreadSafe, "Off")
    .Hint(Hint.Resolve, "Off")

    .Root<RootTarget>(nameof(Composition.Root))

    .PerResolve<RootCommand, Settings>()
    .Bind<ITeamCityArtifactsWriter>().To(_ => GetService<ITeamCityWriter>())
    .Transient(_ => GetService<INuGet>())

    // Targets
    .Singleton<
        GeneratorTarget,
        LibrariesTarget,
        CompatibilityCheckTarget,
        PackTarget,
        CreateExamplesTarget,
        ReadmeTarget,
        TestExamplesTarget,
        BenchmarksTarget,
        DeployTarget,
        TemplateTarget,
        InstallTemplateTarget,
        UpdateTarget,
        PublishBlazorTarget,
        PerformanceTestsTarget,
        AIContextTarget>(Tag.Type);

return await new Composition().Root.RunAsync(CancellationToken.None);