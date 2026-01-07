// ReSharper disable InvertIf
// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core;

using static Tag;

sealed class BindingBuilder(
    [Tag(UniqueTag)] IIdGenerator idGenerator,
    [Tag(SpecialBinding)] IIdGenerator specialBindingIdGenerator,
    IBaseSymbolsProvider baseSymbolsProvider,
    ITypes types,
    ILocationProvider locationProvider)
    : IBindingBuilder
{
    private readonly List<MdContract> _contracts = [];
    private readonly LinkedList<MdDefaultLifetime> _defaultLifetimes = [];
    private readonly List<MdTag> _tags = [];
    private MdArg? _arg;
    private MdFactory? _factory;
    private MdImplementation? _implementation;
    private MdLifetime? _lifetime;
    private SemanticModel? _semanticModel;
    private ExpressionSyntax? _source;

    public void AddDefaultLifetime(MdDefaultLifetime defaultLifetime) =>
        _defaultLifetimes.AddFirst(defaultLifetime);

    public MdLifetime Lifetime
    {
        set => _lifetime = value;
    }

    public MdImplementation Implementation
    {
        set
        {
            _source = value.Source;
            _semanticModel = value.SemanticModel;
            _implementation = value;
        }
    }

    public MdFactory Factory
    {
        set
        {
            _source = value.Source;
            _semanticModel = value.SemanticModel;
            _factory = value;
        }
    }

    public MdArg Arg
    {
        set
        {
            _source = value.Source;
            _semanticModel = value.SemanticModel;
            _arg = value;
        }
    }

    public void AddContract(in MdContract contract) =>
        _contracts.Add(contract);

    public void AddTag(in MdTag tag) =>
        _tags.Add(tag);

    public MdBinding Build(MdSetup setup)
    {
        if (_semanticModel is not {} semanticModel || _source is not {} source)
        {
            throw new CompileErrorException(
                Strings.Error_InvalidBinding,
                ImmutableArray.Create(locationProvider.GetLocation(setup.Source)),
                LogId.ErrorInvalidMetadata);
        }

        var implementationType = _implementation?.Type ?? _factory?.Type ?? _arg?.Type;
        var contractsSource = _implementation?.Source ?? _factory?.Source;
        var explicitContracts = _contracts.Where(i => i.ContractType is not null).ToList();

        // Process implementation contracts if they exist
        if (implementationType is not null && contractsSource is not null)
        {
            explicitContracts.AddRange(CreateExplicitContractsFromImplementation(setup, semanticModel, implementationType, contractsSource));
        }

        var id = new Lazy<int>(idGenerator.Generate);
        var implementationTags = _tags.Select(tag => BuildTag(tag, implementationType, id)).ToImmutableArray();

        // Map tags for all contracts
        var contractsWithTags = explicitContracts
            .Select(c => c with { Tags = c.Tags.Select(tag => BuildTag(tag, implementationType, id)).ToImmutableArray() })
            .ToImmutableArray();

        return new MdBinding(
            int.MaxValue - specialBindingIdGenerator.Generate(),
            source,
            setup,
            semanticModel,
            contractsWithTags,
            implementationTags,
            GetLifetime(implementationType, implementationTags),
            _implementation,
            _factory,
            _arg);
    }

    private IEnumerable<MdContract> CreateExplicitContractsFromImplementation(
        MdSetup setup,
        SemanticModel semanticModel,
        ITypeSymbol implementationType,
        ExpressionSyntax contractsSource)
    {
        var implementationContracts = _contracts.Where(i => i.ContractType is null).ToList();
        if (implementationContracts.Count == 0)
        {
            yield break;
        }

        var baseSymbols = Enumerable.Empty<ITypeSymbol>();

        // Only search for base symbols if the implementation is a concrete class or struct
        if (implementationType is { SpecialType: Microsoft.CodeAnalysis.SpecialType.None, TypeKind: TypeKind.Class or TypeKind.Struct, IsAbstract: false })
        {
            var specialTypes = setup.SpecialTypes.ToImmutableHashSet(SymbolEqualityComparer.Default);
            baseSymbols = baseSymbolsProvider
                .GetBaseSymbols(implementationType, (type, deepness) => deepness switch
                {
                    0 => true,
                    1 => IsSuitableForBinding(specialTypes, type),
                    _ => false
                }, 1)
                .Select(i => i.Type);
        }

        var contracts = new HashSet<ITypeSymbol>(baseSymbols, SymbolEqualityComparer.Default) { implementationType };
        var tags = implementationContracts
            .SelectMany(i => i.Tags)
            .GroupBy(i => i.Value)
            .Select(i => i.First())
            .ToImmutableArray();

        foreach (var contract in contracts)
        {
            yield return new MdContract(semanticModel, contractsSource, contract, ContractKind.Explicit, tags);
        }
    }

    private static bool IsSuitableForBinding(ImmutableHashSet<ISymbol?> specialTypes, ITypeSymbol type)
    {
        // Checks if the type is an interface or an abstract class, which are typical candidates for DI contracts.
        var isAbstractOrInterface = type.TypeKind == TypeKind.Interface || type.IsAbstract;

        // Ensures the type is not a predefined system type like 'object', 'string', or 'int' (SpecialType.None).
        var isNotSpecialType = type.SpecialType == Microsoft.CodeAnalysis.SpecialType.None;

        // Verifies that the type is not explicitly excluded via the 'SpecialTypes' setup configuration.
        var isNotMarkedAsSpecial = !specialTypes.Contains(type);

        return isAbstractOrInterface && isNotSpecialType && isNotMarkedAsSpecial;
    }

    private static MdTag BuildTag(MdTag tag, ITypeSymbol? type, Lazy<int> id)
    {
        if (type is null || tag.Value is not Tag tagVal)
        {
            return tag;
        }

        if (tagVal == Type)
        {
            return MdTag.CreateTypeTag(tag, type);
        }

        if (tagVal == Unique)
        {
            return MdTag.CreateUniqueTag(tag, id.Value);
        }

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (tagVal == Any)
        {
            return MdTag.CreateAnyTag(tag);
        }

        return tag;
    }

    private MdLifetime? GetLifetime(ITypeSymbol? implementationType, ImmutableArray<MdTag> implementationTags)
    {
        if (_lifetime.HasValue)
        {
            return _lifetime.Value;
        }

        if (implementationType is null)
        {
            return _defaultLifetimes.FirstOrDefault(i => i.Type is null).Lifetime;
        }

        foreach (var defaultLifetime in _defaultLifetimes.Where(i => i.Type is not null))
        {
            var baseSymbols = baseSymbolsProvider.GetBaseSymbols(implementationType, (type, _) =>
                IsMatchingDefaultLifetime(defaultLifetime, type, implementationTags));

            if (baseSymbols.Any())
            {
                return defaultLifetime.Lifetime;
            }
        }

        return _defaultLifetimes.FirstOrDefault(i => i.Type is null).Lifetime;
    }

    private bool IsMatchingDefaultLifetime(MdDefaultLifetime defaultLifetime, ITypeSymbol type, ImmutableArray<MdTag> implementationTags)
    {
        if (!types.TypeEquals(defaultLifetime.Type, type))
        {
            return false;
        }

        if (defaultLifetime.Tags.IsDefaultOrEmpty)
        {
            return true;
        }

        // Combine implementation tags and contract tags for intersection check
        var contractTags = _contracts.FirstOrDefault(j => types.TypeEquals(j.ContractType, type)).Tags;
        var combinedTags = implementationTags.ToImmutableHashSet();
        if (!contractTags.IsDefaultOrEmpty)
        {
            combinedTags = combinedTags.Union(contractTags);
        }

        return !combinedTags.Intersect(defaultLifetime.Tags).IsEmpty;
    }
}