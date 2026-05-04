// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core;

sealed class Marker(
    IGenericTypeArguments genericTypeArguments,
    ICache<Marker.BasedMarkerKey, bool> markerBasedCache,
    ICache<Marker.MarkerKey, bool> markerCache,
    ITypeSymbolComparer typeSymbolComparer): IMarker
{
    public bool IsMarkerBased(MdSetup setup, ITypeSymbol type) =>
        markerBasedCache.Get(new BasedMarkerKey(type, typeSymbolComparer), _ => IsMarkerBasedInternal(setup, type));

    public bool IsMarker(MdSetup setup, ITypeSymbol type) =>
        markerCache.Get(new MarkerKey(type, typeSymbolComparer), _ => IsMarkerInternal(setup, type));

    private bool IsMarkerBasedInternal(MdSetup setup, ITypeSymbol type) =>
        IsMarker(setup, type) || type switch
        {
            INamedTypeSymbol { IsGenericType: false } => false,
            INamedTypeSymbol namedTypeSymbol => namedTypeSymbol.TypeArguments.Any(i => IsMarkerBased(setup, i)),
            IArrayTypeSymbol arrayTypeSymbol => IsMarkerBased(setup, arrayTypeSymbol.ElementType),
            _ => false
        };

    private bool IsMarkerInternal(MdSetup setup, ITypeSymbol type) =>
        genericTypeArguments.IsGenericTypeArgument(setup, type)
        || type.GetAttributes()
            .Where(i => i.AttributeClass is not null)
            .Any(i => genericTypeArguments.IsGenericTypeArgumentAttribute(setup, i.AttributeClass!));

    internal class MarkerKeyBase(ITypeSymbol typeSymbol, ITypeSymbolComparer typeSymbolComparer)
    {
        private readonly ITypeSymbol _typeSymbol = typeSymbol;

        public override bool Equals(object? obj) =>
            obj is MarkerKeyBase other && typeSymbolComparer.RuntimeEquals(_typeSymbol, other._typeSymbol);

        public override int GetHashCode() => typeSymbolComparer.GetRuntimeHashCode(_typeSymbol);
    }

    internal class BasedMarkerKey(ITypeSymbol typeSymbol, ITypeSymbolComparer typeSymbolComparer) : MarkerKeyBase(typeSymbol, typeSymbolComparer);

    internal class MarkerKey(ITypeSymbol typeSymbol, ITypeSymbolComparer typeSymbolComparer) : MarkerKeyBase(typeSymbol, typeSymbolComparer);
}
