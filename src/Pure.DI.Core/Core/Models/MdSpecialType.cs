namespace Pure.DI.Core.Models;

readonly record struct MdSpecialType(
    SemanticModel SemanticModel,
    ExpressionSyntax Source,
    INamedTypeSymbol Type);