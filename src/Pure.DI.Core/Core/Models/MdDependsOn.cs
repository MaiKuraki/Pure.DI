// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable NotAccessedPositionalProperty.Global

namespace Pure.DI.Core.Models;

readonly record struct MdDependsOnItem(
    in CompositionName CompositionTypeName,
    string? ContextArgName = null,
    ExpressionSyntax? ContextArgSource = null,
    SetupContextKind ContextArgKind = SetupContextKind.Argument);

readonly record struct MdDependsOn(
    SemanticModel SemanticModel,
    ExpressionSyntax Source,
    in ImmutableArray<MdDependsOnItem> Items,
    bool Explicit)
{
    public override string ToString()
    {
        var items = Items.Select(i => i.ContextArgName is { Length: > 0 }
            ? $"\"{i.CompositionTypeName.FullName}\", \"{i.ContextArgName}\""
            : $"\"{i.CompositionTypeName.FullName}\"");
        return $"DependsOn({string.Join(", ", items)})";
    }
}
