namespace Pure.DI.Core.Models;

record SetupContextRewriterContext(
    SemanticModel SemanticModel,
    INamedTypeSymbol SetupType,
    string ContextArgName);
