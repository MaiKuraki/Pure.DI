// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

sealed class SetupContextMembersCollector(SetupContextMembersCollectorContext ctx)
    : ISetupContextMembersCollector
{
    public ImmutableArray<MemberDeclarationSyntax> Collect()
    {
        var compilation = ctx.Setup.SemanticModel.Compilation;
        var languageVersion = GetLanguageVersion(compilation, ctx.Setup.SemanticModel);
        var queue = new Queue<ISymbol>();
        var processed = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        var memberKeys = new HashSet<(SyntaxTree Tree, TextSpan Span)>();
        var members = new List<MemberDeclarationSyntax>();

        foreach (var binding in ctx.Setup.Bindings)
        {
            if (binding.Factory is not { } factory)
            {
                continue;
            }

            var walker = new InstanceMemberAccessWalker(factory.SemanticModel, ctx.SetupType);
            walker.Visit(factory.Factory);
            foreach (var symbol in walker.Members)
            {
                queue.Enqueue(symbol);
            }
        }

        while (queue.Count > 0)
        {
            var symbol = queue.Dequeue();
            if (!processed.Add(symbol))
            {
                continue;
            }

            foreach (var member in GetMemberDeclarations(symbol))
            {
                var key = (member.SyntaxTree, member.Span);
                if (!memberKeys.Add(key))
                {
                    continue;
                }

                members.Add(member);

                var memberSemanticModel = compilation.GetSemanticModel(member.SyntaxTree);
                var walker = new InstanceMemberAccessWalker(memberSemanticModel, ctx.SetupType);
                walker.Visit(member);
                foreach (var nested in walker.Members)
                {
                    if (!processed.Contains(nested))
                    {
                        queue.Enqueue(nested);
                    }
                }
            }
        }

        if (members.Count == 0)
        {
            return ImmutableArray<MemberDeclarationSyntax>.Empty;
        }

        var sortedMembers = members
            .OrderBy(member => member.SyntaxTree.FilePath, StringComparer.Ordinal)
            .ThenBy(member => member.SpanStart)
            .ToList();

        var rewrittenMembers = sortedMembers
            .SelectMany(member => RewriteMember(compilation, member, languageVersion))
            .ToImmutableArray();

        return rewrittenMembers;
    }

    private static IEnumerable<MemberDeclarationSyntax> RewriteMember(Compilation compilation, MemberDeclarationSyntax member, LanguageVersion languageVersion)
    {
        var semanticModel = compilation.GetSemanticModel(member.SyntaxTree);
        var rewriter = new SetupContextMemberRewriter(semanticModel, languageVersion);
        var rewritten = (MemberDeclarationSyntax)rewriter.Visit(member)!;
        if (languageVersion >= LanguageVersion.CSharp9
            && rewritten is MethodDeclarationSyntax method
            && (method.Body is not null || method.ExpressionBody is not null)
            && method.Modifiers.Any(i => i.IsKind(SyntaxKind.PartialKeyword)))
        {
            yield return CreatePartialDeclaration(method);
        }

        yield return rewritten;
    }

    private static IEnumerable<MemberDeclarationSyntax> GetMemberDeclarations(ISymbol symbol)
    {
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();
            switch (syntax)
            {
                case MemberDeclarationSyntax member:
                    yield return member;
                    break;

                case VariableDeclaratorSyntax variable
                    when variable.Parent?.Parent is MemberDeclarationSyntax member:
                    yield return member;
                    break;
            }
        }
    }

    private static LanguageVersion GetLanguageVersion(Compilation compilation, SemanticModel semanticModel) =>
        compilation is CSharpCompilation { LanguageVersion: var version }
            ? version
            : semanticModel.SyntaxTree.Options is CSharpParseOptions { LanguageVersion: var fallback }
                ? fallback
                : LanguageVersion.CSharp8;

    private sealed class InstanceMemberAccessWalker(
        SemanticModel semanticModel,
        INamedTypeSymbol setupType)
        : CSharpSyntaxWalker
    {
        private readonly HashSet<TextSpan> _spans = [];

        public List<ISymbol> Members { get; } = [];

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
                    Add(node, field);
                    break;

                case IPropertySymbol property when IsInstanceMember(property, setupType):
                    Add(node, property);
                    break;

                case IEventSymbol @event when IsInstanceMember(@event, setupType):
                    Add(node, @event);
                    break;

                case IMethodSymbol method
                    when method.MethodKind == MethodKind.Ordinary
                         && IsInstanceMember(method, setupType):
                    Add(node, method);
                    break;
            }

            base.VisitIdentifierName(node);
        }

        private void Add(SyntaxNode node, ISymbol symbol)
        {
            if (!_spans.Add(node.Span))
            {
                return;
            }

            Members.Add(symbol);
        }

        private static bool IsInstanceMember(ISymbol symbol, INamedTypeSymbol setupType) =>
            !symbol.IsStatic
            && SymbolEqualityComparer.Default.Equals(symbol.ContainingType, setupType);
    }

    private sealed class SetupContextMemberRewriter(SemanticModel semanticModel, LanguageVersion languageVersion)
        : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            var type = semanticModel.GetTypeInfo(node).Type;
            if (type is not null)
            {
                var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var name = SyntaxFactory.ParseName(typeName).WithTriviaFrom(node.Name);
                node = node.WithName(name);
            }

            return base.VisitAttribute(node);
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node) =>
            TryCreateTypeSyntax(node) ?? base.VisitIdentifierName(node);

        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node) =>
            TryCreateTypeSyntax(node) ?? base.VisitQualifiedName(node);

        public override SyntaxNode? VisitAliasQualifiedName(AliasQualifiedNameSyntax node) =>
            TryCreateTypeSyntax(node) ?? base.VisitAliasQualifiedName(node);

        public override SyntaxNode? VisitGenericName(GenericNameSyntax node) =>
            TryCreateTypeSyntax(node) ?? base.VisitGenericName(node);

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) =>
            TryCreateTypeSyntax(node) ?? base.VisitMemberAccessExpression(node);

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var updated = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
            if (languageVersion >= LanguageVersion.CSharp9
                && (updated.Body is not null || updated.ExpressionBody is not null)
                && !updated.Modifiers.Any(i => i.IsKind(SyntaxKind.PartialKeyword)))
            {
                updated = updated.WithModifiers(AddPartialModifier(updated.Modifiers));
            }

            return updated;
        }

        private SyntaxNode? TryCreateTypeSyntax(SyntaxNode node)
        {
            if (node.SyntaxTree != semanticModel.SyntaxTree)
            {
                return null;
            }

            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol is not ITypeSymbol typeSymbol)
            {
                return null;
            }

            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return SyntaxFactory.ParseTypeName(typeName).WithTriviaFrom(node);
        }

        private static SyntaxTokenList AddPartialModifier(SyntaxTokenList modifiers)
        {
            var insertIndex = 0;
            for (var i = 0; i < modifiers.Count; i++)
            {
                var modifier = modifiers[i];
                if (modifier.IsKind(SyntaxKind.PublicKeyword)
                    || modifier.IsKind(SyntaxKind.InternalKeyword)
                    || modifier.IsKind(SyntaxKind.PrivateKeyword)
                    || modifier.IsKind(SyntaxKind.ProtectedKeyword))
                {
                    insertIndex = i + 1;
                }
            }

            return modifiers.Insert(insertIndex, SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        }
    }

    private static MethodDeclarationSyntax CreatePartialDeclaration(MethodDeclarationSyntax method)
    {
        var emptyTrivia = SyntaxFactory.TriviaList();
        var declaration = method
            .WithAttributeLists(default)
            .WithLeadingTrivia(emptyTrivia)
            .WithTrailingTrivia(emptyTrivia)
            .WithBody(null)
            .WithExpressionBody(null)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        return declaration;
    }
}
