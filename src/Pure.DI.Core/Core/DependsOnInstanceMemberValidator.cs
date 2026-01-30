// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core;

sealed class DependsOnInstanceMemberValidator(
    ILogger logger,
    ILocationProvider locationProvider)
    : IValidator<MdSetup>
{
    public bool Validate(MdSetup setup)
    {
        foreach (var binding in setup.Bindings)
        {
            if (binding.SourceSetup.Name.Equals(setup.Name))
            {
                continue;
            }

            if (binding.Factory is not { } factory)
            {
                continue;
            }

            var setupType = GetContainingType(binding.SourceSetup);
            if (setupType is null)
            {
                continue;
            }

            var walker = new InstanceMemberAccessWalker(factory.SemanticModel, setupType);
            walker.Visit(factory.Factory);

            foreach (var access in walker.Accesses)
            {
                logger.CompileWarning(
                    LogMessage.Format(
                        nameof(Strings.Warning_Template_InstanceMemberInDependsOnSetup),
                        Strings.Warning_Template_InstanceMemberInDependsOnSetup,
                        access.MemberName,
                        binding.SourceSetup.Name),
                    ImmutableArray.Create(locationProvider.GetLocation(access.Node)),
                    LogId.WarningInstanceMemberInDependsOnSetup);
            }
        }

        return true;
    }

    private static INamedTypeSymbol? GetContainingType(MdSetup setup) =>
        setup.Source.Ancestors()
            .OfType<BaseTypeDeclarationSyntax>()
            .FirstOrDefault() is { } typeDeclaration
            ? setup.SemanticModel.GetDeclaredSymbol(typeDeclaration) as INamedTypeSymbol
            : null;

    private sealed class InstanceMemberAccessWalker(
        SemanticModel semanticModel,
        INamedTypeSymbol setupType)
        : CSharpSyntaxWalker
    {
        private readonly HashSet<TextSpan> _spans = [];

        public List<MemberAccess> Accesses { get; } = [];

        public override void VisitThisExpression(ThisExpressionSyntax node)
        {
            if (node.Parent is MemberAccessExpressionSyntax)
            {
                return;
            }

            Add(node, "this");
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.SyntaxTree != semanticModel.SyntaxTree)
            {
                base.VisitIdentifierName(node);
                return;
            }

            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            switch (symbol)
            {
                case IFieldSymbol field when IsInstanceMember(field, setupType):
                    Add(node, field.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                    break;

                case IPropertySymbol property when IsInstanceMember(property, setupType):
                    Add(node, property.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                    break;

                case IEventSymbol @event when IsInstanceMember(@event, setupType):
                    Add(node, @event.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                    break;

                case IMethodSymbol method
                    when method.MethodKind == MethodKind.Ordinary
                         && IsInstanceMember(method, setupType):
                    Add(node, method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                    break;
            }

            base.VisitIdentifierName(node);
        }

        private void Add(SyntaxNode node, string memberName)
        {
            if (!_spans.Add(node.Span))
            {
                return;
            }

            Accesses.Add(new MemberAccess(node, memberName));
        }

        private static bool IsInstanceMember(ISymbol symbol, INamedTypeSymbol setupType) =>
            !symbol.IsStatic
            && SymbolEqualityComparer.Default.Equals(symbol.ContainingType, setupType);
    }

    private sealed record MemberAccess(SyntaxNode Node, string MemberName);
}
