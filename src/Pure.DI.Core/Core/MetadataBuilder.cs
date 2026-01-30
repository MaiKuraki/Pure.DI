// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ObjectAllocation.Possible
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable LoopCanBeConvertedToQuery

namespace Pure.DI.Core;

sealed class MetadataBuilder(
    Func<IBuilder<SyntaxUpdate, IEnumerable<MdSetup>>> setupsBuilderFactory,
    Func<ISetupFinalizer> setupFinalizerFactory,
    Func<SetupContextRewriterContext, ISetupContextRewriter> setupContextRewriterFactory,
    ICompilations compilations,
    IRegistryManager<int> bindingsRegistryManager,
    ILocationProvider locationProvider,
    CancellationToken cancellationToken)
    : IBuilder<IEnumerable<SyntaxUpdate>, IEnumerable<MdSetup>>
{
    private readonly record struct SetupDependency(
        MdSetup Setup,
        string? ContextArgName,
        ExpressionSyntax? ContextArgSource,
        MdDependsOn? DependsOn);

    public IEnumerable<MdSetup> Build(IEnumerable<SyntaxUpdate> updates)
    {
        var actualUpdates = updates
            .GroupBy(i => i.Node.SyntaxTree.GetRoot())
            .Select(i => new SyntaxUpdate(i.Key, i.First().SemanticModel));

        var setups = new List<MdSetup>();
        foreach (var update in actualUpdates)
        {
            var languageVersion = compilations.GetLanguageVersion(update.SemanticModel.Compilation);
            if (languageVersion < LanguageVersion.CSharp8)
            {
                throw new CompileErrorException(
                    string.Format(Strings.Error_Template_UnsupportLanguage, Names.GeneratorName, languageVersion.ToDisplayString(), LanguageVersion.CSharp8.ToDisplayString()),
                    ImmutableArray.Create(locationProvider.GetLocation(update.Node)),
                    LogId.ErrorNotSupportedLanguageVersion,
                    nameof(Strings.Error_Template_UnsupportLanguage));
            }

            var setupsBuilder = setupsBuilderFactory();
            foreach (var newSetup in setupsBuilder.Build(update))
            {
                setups.Add(newSetup);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        if (setups.Count == 0)
        {
            yield break;
        }

        var setupMap = setups
            .Where(i => i.Kind != CompositionKind.Global)
            .GroupBy(i => i.Name)
            .Select(setupGroup => {
                MergeSetups(setupGroup.Select(i => new SetupDependency(i, null, null, null)), out var mergedSetup, false);
                return mergedSetup;
            })
            .ToDictionary(i => i.Name, i => i);

        var globalSetups = setups.Where(i => i.Kind == CompositionKind.Global).OrderBy(i => i.Name.ClassName).ToList();
        foreach (var setup in setupMap.Values.Where(i => i.Kind == CompositionKind.Public).OrderBy(i => i.Name))
        {
            var setupsChain = globalSetups
                .Select(i => new SetupDependency(i, null, null, null))
                .Concat(ResolveDependencies(setup, setupMap, new HashSet<CompositionName>()))
                .Concat(Enumerable.Repeat(new SetupDependency(setup, null, null, null), 1));

            MergeSetups(setupsChain, out var mergedSetup, true);
            var setupFinalizer = setupFinalizerFactory();
            yield return setupFinalizer.Finalize(mergedSetup, setupMap);
        }
    }

    private IEnumerable<SetupDependency> ResolveDependencies(
        MdSetup setup,
        IReadOnlyDictionary<CompositionName, MdSetup> map,
        ISet<CompositionName> processed)
    {
        foreach (var dependsOn in setup.DependsOn)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var item in dependsOn.Items)
            {
                if (!processed.Add(item.CompositionTypeName))
                {
                    continue;
                }

                if (!map.TryGetValue(item.CompositionTypeName, out var dependsOnSetup))
                {
                    if (!dependsOn.Explicit)
                    {
                        continue;
                    }

                    throw new CompileErrorException(
                        string.Format(Strings.Error_Template_CannotFindSetup, item.CompositionTypeName),
                        ImmutableArray.Create(locationProvider.GetLocation(dependsOn.Source)),
                        LogId.ErrorCannotFindSetup,
                        nameof(Strings.Error_Template_CannotFindSetup));
                }

                if (!dependsOn.Explicit && dependsOnSetup.Kind != CompositionKind.Internal)
                {
                    continue;
                }

                yield return new SetupDependency(dependsOnSetup, item.ContextArgName, item.ContextArgSource, dependsOn);
                foreach (var result in ResolveDependencies(dependsOnSetup, map, processed))
                {
                    yield return result;
                }
            }
        }
    }

    private void MergeSetups(IEnumerable<SetupDependency> setups, out MdSetup mergedSetup, bool resolveDependsOn)
    {
        MdSetup? lastSetup = null;
        var contextArgs = new List<(string ArgName, ExpressionSyntax? ArgSource, ITypeSymbol ArgType, MdDependsOn DependsOn, SemanticModel SemanticModel)>();
        var name = new CompositionName("Composition", "", null);
        var kind = CompositionKind.Global;
        var settings = new Hints();
        var bindingsBuilder = ImmutableArray.CreateBuilder<MdBinding>(64);
        var rootsBuilder = ImmutableArray.CreateBuilder<MdRoot>(64);
        var dependsOnBuilder = ImmutableArray.CreateBuilder<MdDependsOn>(2);
        var genericTypeArgumentBuilder = ImmutableArray.CreateBuilder<MdGenericTypeArgument>(0);
        var genericTypeArgumentAttributesBuilder = ImmutableArray.CreateBuilder<MdGenericTypeArgumentAttribute>(1);
        var typeAttributesBuilder = ImmutableArray.CreateBuilder<MdTypeAttribute>(2);
        var tagAttributesBuilder = ImmutableArray.CreateBuilder<MdTagAttribute>(2);
        var specialTypeBuilder = ImmutableArray.CreateBuilder<MdSpecialType>(0);
        var ordinalAttributesBuilder = ImmutableArray.CreateBuilder<MdOrdinalAttribute>(2);
        var usingDirectives = ImmutableArray.CreateBuilder<MdUsingDirectives>(2);
        var accumulators = ImmutableArray.CreateBuilder<MdAccumulator>(1);
        var bindingId = 0;
        var comments = new List<string>();
        foreach (var item in setups)
        {
            var setup = item.Setup;
            lastSetup = setup;
            name = setup.Name;
            kind = setup.Kind;
            foreach (var setting in setup.Hints)
            {
                var items = settings.GetOrAdd(setting.Key, _ => new LinkedList<string>());
                foreach (var newValue in setting.Value)
                {
                    items.AddLast(newValue);
                }
            }

            if (resolveDependsOn)
            {
                bindingsBuilder.AddRange(setup.Bindings.Select(i =>
                {
                    var updated = i;
                    if (item.ContextArgName is { Length: > 0 }
                        && i.Factory is { } factory
                        && GetContainingType(setup) is { } setupType)
                    {
                        var rewriterContext = new SetupContextRewriterContext(factory.SemanticModel, setupType, item.ContextArgName);
                        var rewritten = setupContextRewriterFactory(rewriterContext).Rewrite(factory.Factory);
                        updated = updated with { Factory = factory with { Factory = rewritten } };
                    }

                    return updated with { Id = bindingId++ };
                }));
            }
            else
            {
                bindingsBuilder.AddRange(setup.Bindings);
            }

            rootsBuilder.AddRange(setup.Roots);
            dependsOnBuilder.AddRange(setup.DependsOn);
            genericTypeArgumentBuilder.AddRange(setup.GenericTypeArguments);
            genericTypeArgumentAttributesBuilder.AddRange(setup.GenericTypeArgumentAttributes);
            typeAttributesBuilder.AddRange(setup.TypeAttributes);
            tagAttributesBuilder.AddRange(setup.TagAttributes);
            ordinalAttributesBuilder.AddRange(setup.OrdinalAttributes);
            specialTypeBuilder.AddRange(setup.SpecialTypes);
            accumulators.AddRange(setup.Accumulators);
            foreach (var usingDirective in setup.UsingDirectives)
            {
                usingDirectives.Add(usingDirective);
            }

            if (setup.Kind == CompositionKind.Public)
            {
                comments.AddRange(setup.Comments);
            }

            if (resolveDependsOn
                && item.ContextArgName is { Length: > 0 }
                && item.DependsOn is { } dependsOn
                && GetContainingType(setup) is { } setupType)
            {
                contextArgs.Add((item.ContextArgName, item.ContextArgSource, setupType, dependsOn, dependsOn.SemanticModel));
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        if (resolveDependsOn && lastSetup is not null && contextArgs.Count > 0)
        {
            foreach (var (argName, argSource, argType, dependsOn, semanticModel) in contextArgs)
            {
                var argLocation = argSource ?? dependsOn.Source;
                var arg = new MdArg(semanticModel, argLocation, argType, argName, ArgKind.Composition, false, [], true);
                var binding = new MdBinding(
                    bindingId++,
                    argLocation,
                    lastSetup,
                    semanticModel,
                    ImmutableArray.Create(new MdContract(semanticModel, argLocation, argType, ContractKind.Explicit, ImmutableArray<MdTag>.Empty)),
                    ImmutableArray<MdTag>.Empty,
                    null,
                    null,
                    null,
                    arg);
                bindingsBuilder.Add(binding);
                bindingsRegistryManager.Register(lastSetup, binding.Id);
            }
        }

        var bindings = bindingsBuilder.ToImmutable();

        var tagOn = bindings
            .OrderBy(i => i.Id)
            .SelectMany(i => i.Contracts)
            .SelectMany(binding => binding.Tags.Select(i => i.Value).OfType<MdTagOnSites>())
            .Where(i => i.InjectionSites.Length > 0)
            .Distinct()
            .Reverse()
            .ToList();

        mergedSetup = new MdSetup(
            lastSetup?.SemanticModel!,
            lastSetup?.Source!,
            name,
            usingDirectives.ToImmutableArray(),
            kind,
            settings,
            bindings,
            rootsBuilder.ToImmutable(),
            resolveDependsOn ? ImmutableArray<MdDependsOn>.Empty : dependsOnBuilder.ToImmutable(),
            genericTypeArgumentBuilder.ToImmutableArray(),
            genericTypeArgumentAttributesBuilder.ToImmutableArray(),
            typeAttributesBuilder.ToImmutable(),
            tagAttributesBuilder.ToImmutable(),
            ordinalAttributesBuilder.ToImmutable(),
            specialTypeBuilder.ToImmutable(),
            accumulators.ToImmutable(),
            tagOn,
            comments);
    }

    private static INamedTypeSymbol? GetContainingType(MdSetup setup) =>
        setup.Source.Ancestors()
            .OfType<BaseTypeDeclarationSyntax>()
            .FirstOrDefault() is { } typeDeclaration
            ? setup.SemanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol
            : null;
}
