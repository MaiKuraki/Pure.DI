// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core;

sealed class SetupContextRewriter(SetupContextRewriterContext ctx)
    : CSharpSyntaxRewriter, ISetupContextRewriter
{
    private readonly IdentifierNameSyntax _contextIdentifier = SyntaxFactory.IdentifierName(ctx.ContextArgName);

    public LambdaExpressionSyntax Rewrite(LambdaExpressionSyntax lambda) =>
        (LambdaExpressionSyntax)Visit(lambda);

    public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node) =>
        _contextIdentifier.WithTriviaFrom(node);

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (node.Parent is MemberAccessExpressionSyntax { Name: IdentifierNameSyntax name } && ReferenceEquals(name, node))
        {
            return base.VisitIdentifierName(node);
        }

        var symbol = ctx.SemanticModel.GetSymbolInfo(node).Symbol;
        if (symbol is not null && IsInstanceMember(symbol, ctx.SetupType))
        {
            var memberName = SyntaxFactory.IdentifierName(node.Identifier).WithTriviaFrom(node);
            var member = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                _contextIdentifier,
                memberName.WithoutTrivia());
            return member.WithTriviaFrom(node);
        }

        return base.VisitIdentifierName(node);
    }

    private static bool IsInstanceMember(ISymbol symbol, INamedTypeSymbol type) =>
        !symbol.IsStatic && SymbolEqualityComparer.Default.Equals(symbol.ContainingType, type);
}
