namespace Pure.DI.Core.Models;

using System.Runtime.CompilerServices;

readonly record struct Injection(
    InjectionKind Kind,
    RefKind RefKind,
    ITypeSymbol Type,
    object? Tag,
    ImmutableArray<Location> Locations)
{
    private static readonly SymbolDisplayFormat DependencyFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public override string ToString() => $"{Type}{(Tag != null && Tag is not MdTagOnSites ? $"({Tag.ValueToString()})" : "")}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Injection other)
    {
        var type = Type;
        var otherType = other.Type;
        return (ReferenceEquals(type, otherType) || GetDependencyKey(type) == GetDependencyKey(otherType))
               && EqualTags(Tag, other.Tag);
    }

    public override int GetHashCode() =>
        GetDependencyKey(Type).GetHashCode();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualTags(object? tag, object? otherTag) =>
        ReferenceEquals(tag, otherTag)
        || SpecialEqualTags(tag, otherTag)
        || SpecialEqualTags(otherTag, tag)
        || Equals(tag, otherTag);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SpecialEqualTags(object? tag, object? otherTag) =>
        ReferenceEquals(tag, MdTag.ContextTag)
        || ReferenceEquals(tag, MdTag.AnyTag)
        || tag is MdTagOnSites tagOn && tagOn.Equals(otherTag);

    private static string GetDependencyKey(ITypeSymbol type) =>
        $"{type.ToDisplayString(NullableFlowState.None, DependencyFormat)}{GetTopLevelNullableMarker(type)}";

    private static string GetTopLevelNullableMarker(ITypeSymbol type) =>
        type is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated } ? "?" : "";
}
