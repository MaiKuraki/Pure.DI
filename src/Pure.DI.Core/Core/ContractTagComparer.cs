namespace Pure.DI.Core;

sealed class ContractTagComparer : IEqualityComparer<(ITypeSymbol ContractType, object? Tag)>
{
    public bool Equals((ITypeSymbol ContractType, object? Tag) x, (ITypeSymbol ContractType, object? Tag) y) =>
        SymbolEqualityComparer.Default.Equals(x.ContractType, y.ContractType)
        && Equals(x.Tag, y.Tag);

    public int GetHashCode((ITypeSymbol ContractType, object? Tag) obj)
    {
        unchecked
        {
            var hash = SymbolEqualityComparer.Default.GetHashCode(obj.ContractType);
            hash = hash * 397 ^ (obj.Tag?.GetHashCode() ?? 0);
            return hash;
        }
    }
}