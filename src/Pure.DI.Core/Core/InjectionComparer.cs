namespace Pure.DI.Core;

sealed class InjectionComparer(ITypeSymbolComparer typeSymbolComparer) : IInjectionComparer
{
    public bool Equals(Injection x, Injection y) =>
        typeSymbolComparer.DependencyEquals(x.Type, y.Type)
        && Injection.EqualTags(x.Tag, y.Tag);

    public int GetHashCode(Injection obj) =>
        typeSymbolComparer.GetDependencyHashCode(obj.Type);
}
