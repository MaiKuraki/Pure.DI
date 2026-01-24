namespace Pure.DI.Core.Models;

readonly record struct MdSpecialType(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    SemanticModel SemanticModel,
    ExpressionSyntax Source,
    INamedTypeSymbol Type);