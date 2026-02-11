namespace Pure.DI.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SetupInvocationMatcher : ISetupInvocationMatcher
{
    public bool IsSetupInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol
            ?? semanticModel.GetSymbolInfo(invocation).CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (symbol is not null)
        {
            return IsSetupMethod(symbol);
        }

        // ReSharper disable once InvertIf
        if (invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: nameof(DI.Setup), Expression: var receiver })
        {
            var receiverSymbol = semanticModel.GetSymbolInfo(receiver).Symbol;
            if (receiverSymbol is IAliasSymbol { Target: INamedTypeSymbol targetType })
            {
                return IsSetupType(targetType);
            }

            if (receiver is IdentifierNameSyntax identifierName)
            {
                var aliasSymbol = semanticModel.GetAliasInfo(identifierName);
                if (aliasSymbol?.Target is INamedTypeSymbol aliasType)
                {
                    return IsSetupType(aliasType);
                }

                if (IsAliasToSetupType(identifierName))
                {
                    return true;
                }
            }

            if (semanticModel.GetTypeInfo(receiver).Type is INamedTypeSymbol receiverTypeInfo)
            {
                return IsSetupType(receiverTypeInfo);
            }

            if (receiverSymbol is INamedTypeSymbol receiverType)
            {
                return IsSetupType(receiverType);
            }
        }

        return false;
    }

    private static bool IsSetupMethod(IMethodSymbol symbol) =>
        symbol.Name == nameof(DI.Setup) && IsSetupType(symbol.ContainingType);

    private static bool IsSetupType(INamedTypeSymbol? type) =>
        type is { Name: nameof(DI) } && type.ContainingNamespace.ToDisplayString() == Names.GeneratorName;

    private static bool IsAliasToSetupType(IdentifierNameSyntax identifierName)
    {
        var aliasName = identifierName.Identifier.Text;
        var root = identifierName.SyntaxTree.GetRoot();
        foreach (var usingDirective in root.DescendantNodes().OfType<UsingDirectiveSyntax>())
        {
            if (usingDirective.Alias?.Name.Identifier.Text != aliasName)
            {
                continue;
            }

            var targetName = usingDirective.Name?.ToString();
            switch (targetName)
            {
                case null:
                    continue;

                case $"{Names.GeneratorName}.{nameof(DI)}" or $"{Names.GlobalNamespacePrefix}{Names.GeneratorName}.{nameof(DI)}":
                    return true;
            }
        }

        return false;
    }
}
