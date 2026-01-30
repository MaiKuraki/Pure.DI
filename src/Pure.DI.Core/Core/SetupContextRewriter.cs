// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core;

sealed class SetupContextRewriter(SetupContextRewriterContext ctx)
    : CSharpSyntaxRewriter, ISetupContextRewriter
{
    private readonly IdentifierNameSyntax _contextIdentifier = SyntaxFactory.IdentifierName(ctx.ContextArgName);
    private readonly bool _useRootArgument = ctx.ContextArgKind == SetupContextKind.RootArgument;

    public LambdaExpressionSyntax Rewrite(LambdaExpressionSyntax lambda)
    {
        var rewritten = (LambdaExpressionSyntax)Visit(lambda);
        if (!_useRootArgument)
        {
            return rewritten;
        }

        return ctx.IsSimpleFactory
            ? AddContextParameter(rewritten)
            : AddContextInjection(rewritten);
    }

    public override SyntaxNode? VisitThisExpression(ThisExpressionSyntax node) =>
        _contextIdentifier.WithTriviaFrom(node);

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (node.Parent is MemberAccessExpressionSyntax { Name: IdentifierNameSyntax name } && ReferenceEquals(name, node))
        {
            return base.VisitIdentifierName(node);
        }

        if (node.SyntaxTree != ctx.SemanticModel.SyntaxTree)
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

    private ParameterSyntax CreateContextParameter()
    {
        var typeName = SyntaxFactory.ParseTypeName(
            ctx.SetupType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        return SyntaxFactory.Parameter(SyntaxFactory.Identifier(ctx.ContextArgName)).WithType(typeName);
    }

    private LambdaExpressionSyntax AddContextInjection(LambdaExpressionSyntax lambda)
    {
        var contextName = ctx.ContextParameter.Identifier.Text;
        if (string.IsNullOrWhiteSpace(contextName))
        {
            return lambda;
        }

        var declaration = SyntaxFactory.DeclarationExpression(
            SyntaxFactory.ParseTypeName(ctx.SetupType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
            SyntaxFactory.SingleVariableDesignation(SyntaxFactory.Identifier(ctx.ContextArgName)));

        var injectCall = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(contextName),
                    SyntaxFactory.IdentifierName(nameof(IContext.Inject))))
            .AddArgumentListArguments(
                SyntaxFactory.Argument(declaration)
                    .WithRefOrOutKeyword(SyntaxFactory.Token(SyntaxKind.OutKeyword)));

        var injectStatement = SyntaxFactory.ExpressionStatement(injectCall);

        if (lambda.Block is { } block)
        {
            var updatedBlock = block.WithStatements(block.Statements.Insert(0, injectStatement));
            return lambda.WithBlock(updatedBlock);
        }

        if (lambda.ExpressionBody is { } expressionBody)
        {
            var blockWithReturn = SyntaxFactory.Block(
                injectStatement,
                SyntaxFactory.ReturnStatement(expressionBody));
            return lambda.WithExpressionBody(null).WithBlock(blockWithReturn);
        }

        return lambda;
    }

    private LambdaExpressionSyntax AddContextParameter(LambdaExpressionSyntax lambda) =>
        lambda switch
        {
            SimpleLambdaExpressionSyntax simple when simple.Parameter.Identifier.Text == ctx.ContextArgName => simple,
            SimpleLambdaExpressionSyntax simple => SyntaxFactory.ParenthesizedLambdaExpression(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList(new[]
                        {
                            simple.Parameter,
                            CreateContextParameter()
                        })),
                    simple.Body)
                .WithTriviaFrom(simple),
            ParenthesizedLambdaExpressionSyntax parenthesized
                when parenthesized.ParameterList.Parameters.Any(p => p.Identifier.Text == ctx.ContextArgName) => parenthesized,
            ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.WithParameterList(
                parenthesized.ParameterList.WithParameters(parenthesized.ParameterList.Parameters.Add(CreateContextParameter()))),
            _ => lambda
        };
}
