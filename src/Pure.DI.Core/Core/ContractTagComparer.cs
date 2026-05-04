namespace Pure.DI.Core;

sealed class ContractTagComparer(ITypeSymbolComparer typeSymbolComparer) : IEqualityComparer<(ITypeSymbol ContractType, object? Tag)>
{
    public bool Equals((ITypeSymbol ContractType, object? Tag) x, (ITypeSymbol ContractType, object? Tag) y) =>
        typeSymbolComparer.DependencyEquals(x.ContractType, y.ContractType)
        && Equals(x.Tag, y.Tag);

    public int GetHashCode((ITypeSymbol ContractType, object? Tag) obj)
    {
        unchecked
        {
            var hash = typeSymbolComparer.GetDependencyHashCode(obj.ContractType);
            hash = hash * 397 ^ (obj.Tag?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
