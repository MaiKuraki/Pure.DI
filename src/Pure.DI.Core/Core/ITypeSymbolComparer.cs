namespace Pure.DI.Core;

interface ITypeSymbolComparer
{
    IEqualityComparer<ITypeSymbol> Runtime { get; }

    IEqualityComparer<ITypeSymbol> Dependency { get; }

    bool RuntimeEquals(ITypeSymbol? type, ITypeSymbol? otherType);

    bool DependencyEquals(ITypeSymbol? type, ITypeSymbol? otherType);

    int GetRuntimeHashCode(ITypeSymbol type);

    int GetDependencyHashCode(ITypeSymbol type);
}
