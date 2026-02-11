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
        var memberKeys = new HashSet<(SyntaxTree Tree, TextSpan Span)>();
        var members = new List<MemberDeclarationSyntax>();

        // Collect only instance members referenced directly by bindings
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var member in GetRootMembers())
        {
            if (!IsInstanceMemberForCollect(member, ctx.SetupType, ctx.TargetType))
            {
                continue;
            }

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var declaration in GetMemberDeclarations(member))
            {
                var key = (declaration.SyntaxTree, declaration.Span);
                if (!memberKeys.Add(key))
                {
                    continue;
                }

                members.Add(declaration);
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

    private IEnumerable<ISymbol> GetRootMembers()
    {
        var root = (SyntaxNode?)ctx.Setup.Source
            .AncestorsAndSelf()
            .OfType<BaseMethodDeclarationSyntax>()
            .FirstOrDefault() ?? ctx.Setup.Source;

        var walker = new InstanceMemberAccessWalker(ctx.Setup.SemanticModel, ctx.SetupType, ctx.TargetType);
        walker.Visit(root);
        foreach (var member in walker.Members)
        {
            yield return member;
        }
    }

    private static IEnumerable<MemberDeclarationSyntax> RewriteMember(Compilation compilation, MemberDeclarationSyntax member, LanguageVersion languageVersion)
    {
        var semanticModel = compilation.GetSemanticModel(member.SyntaxTree);
        var rewriter = new SetupContextMemberRewriter(semanticModel, languageVersion);
        var rewritten = (MemberDeclarationSyntax)rewriter.Visit(member);
        switch (rewritten)
        {
            case MethodDeclarationSyntax method:
                yield return CreatePartialMethodDeclaration(method);
                yield break;

            case PropertyDeclarationSyntax property:
                foreach (var rewrittenMember in RewriteProperty(property))
                {
                    yield return rewrittenMember;
                }
                yield break;

            default:
                yield return rewritten;
                yield break;
        }
    }

    private static IEnumerable<MemberDeclarationSyntax> GetMemberDeclarations(ISymbol symbol)
    {
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var syntax = reference.GetSyntax();
            switch (syntax)
            {
                case MemberDeclarationSyntax member:
                    yield return member;
                    break;

                case VariableDeclaratorSyntax { Parent.Parent: MemberDeclarationSyntax member }:
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

    private static bool IsInstanceMemberForCollect(ISymbol symbol, INamedTypeSymbol setupType, INamedTypeSymbol targetType)
    {
        if (symbol.IsStatic)
        {
            return false;
        }

        // Check if symbol belongs to the setup type itself
        if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType, setupType))
        {
            return false;
        }

        // Check if targetType inherits from setupType
        var inheritsFromSetup = InheritsFrom(targetType, setupType);
        
        if (inheritsFromSetup)
        {
            // When the target composition inherits from the base composition,
            // only collect private members (public/internal are already visible via inheritance)
            return symbol.DeclaredAccessibility == Accessibility.Private;
        }

        // When there's no inheritance relationship, collect all instance members,
        // because none of them are available via inheritance.
        return true;
    }
    
    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol potentialBase)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, potentialBase))
            {
                return true;
            }
            current = current.BaseType;
        }
        
        return false;
    }
    
    private sealed class InstanceMemberAccessWalker(
        SemanticModel semanticModel,
        INamedTypeSymbol setupType,
        INamedTypeSymbol targetType)
        : CSharpSyntaxWalker
    {
        private readonly Compilation _compilation = semanticModel.Compilation;
        private readonly HashSet<TextSpan> _spans = [];

        public List<ISymbol> Members { get; } = [];

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            var model = node.SyntaxTree == semanticModel.SyntaxTree
                ? semanticModel
                : _compilation.GetSemanticModel(node.SyntaxTree);
            var symbol = model.GetSymbolInfo(node).Symbol;
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (symbol is null)
            {
                symbol = model
                    .LookupSymbols(node.SpanStart, name: node.Identifier.ValueText)
                    .FirstOrDefault(s => IsInstanceMember(s, setupType, targetType));
            }

            switch (symbol)
            {
                case IFieldSymbol field when IsInstanceMember(field, setupType, targetType):
                    Add(node, field);
                    break;

                case IPropertySymbol property when IsInstanceMember(property, setupType, targetType):
                    Add(node, property);
                    break;

                case IEventSymbol @event when IsInstanceMember(@event, setupType, targetType):
                    Add(node, @event);
                    break;

                case IMethodSymbol { MethodKind: MethodKind.Ordinary } method
                    when IsInstanceMember(method, setupType, targetType):
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

        private static bool IsInstanceMember(ISymbol symbol, INamedTypeSymbol setupType, INamedTypeSymbol targetType)
        {
            if (symbol.IsStatic)
            {
                return false;
            }

            // Check if symbol belongs to the setup type itself
            if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingType, setupType))
            {
                return false;
            }

            // Check if targetType inherits from setupType
            var inheritsFromSetup = InheritsFrom(targetType, setupType);

            if (inheritsFromSetup)
            {
                // When the target composition inherits from the base composition,
                // only collect private members (public/internal are already visible via inheritance)
                return symbol.DeclaredAccessibility == Accessibility.Private;
            }

            // When there's no inheritance relationship, collect all instance members,
            // because none of them are available via inheritance.
            return true;
        }
    }

    private sealed class SetupContextMemberRewriter(SemanticModel semanticModel, LanguageVersion languageVersion)
        : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            var type = semanticModel.GetTypeInfo(node).Type;
            if (type is null)
            {
                return base.VisitAttribute(node);
            }

            var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var name = SyntaxFactory.ParseName(typeName).WithTriviaFrom(node.Name);
            node = node.WithName(name);
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

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var updated = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
            if (languageVersion >= LanguageVersion.CSharp9
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

    }

    private static MemberDeclarationSyntax CreatePartialMethodDeclaration(MethodDeclarationSyntax method)
    {
        var updated = method;
        
        // Always create a partial method declaration without body (defining declaration)
        // The implementing declaration with body should already exist in the source code
        updated = updated
            .WithBody(null)
            .WithExpressionBody(null)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        updated = updated.WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

        if (!updated.Modifiers.Any(i => i.IsKind(SyntaxKind.PartialKeyword)))
        {
            updated = updated.WithModifiers(AddPartialModifier(updated.Modifiers));
        }

        return updated;
    }

    private static IEnumerable<MemberDeclarationSyntax> RewriteProperty(PropertyDeclarationSyntax property)
    {
        if (!HasGetterLogic(property))
        {
            yield return property;
            yield break;
        }

        var accessors = property.AccessorList?.Accessors
            .Select(accessor => RewriteAccessor(property, accessor))
            .Where(accessor => accessor is not null)
            .Select(accessor => accessor!)
            .ToList();

        if (property.ExpressionBody is not null)
        {
            accessors ??= [];
            accessors.Add(CreateAccessorFromExpression(property, SyntaxKind.GetAccessorDeclaration));
        }

        if ((accessors is null || accessors.Count == 0) && property.ExpressionBody is null)
        {
            if (property.AccessorList is null)
            {
                yield return property;
            }

            yield break;
        }

        accessors ??= [];
        var hasNonAutoAccessors = accessors.Any(a => a.Body is not null || a.ExpressionBody is not null);
        var initializer = hasNonAutoAccessors ? null : property.Initializer;

        var updatedProperty = property
            .WithExpressionBody(null)
            .WithInitializer(initializer)
            .WithSemicolonToken(initializer is not null ? property.SemicolonToken : default)
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));

        yield return updatedProperty;

        foreach (var accessor in accessors.Where(i => i.Body is null && i.ExpressionBody is not null && i.Kind() == SyntaxKind.GetAccessorDeclaration))
        {
            yield return CreateAccessorPartialMethod(property, accessor);
        }
    }

    private static bool HasGetterLogic(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody is not null)
        {
            return true;
        }

        var getter = property.AccessorList?.Accessors
            .FirstOrDefault(accessor => accessor.Kind() == SyntaxKind.GetAccessorDeclaration);

        if (getter is null)
        {
            return false;
        }

        return getter.Body is not null || getter.ExpressionBody is not null;
    }

    private static AccessorDeclarationSyntax? RewriteAccessor(PropertyDeclarationSyntax property, AccessorDeclarationSyntax accessor)
    {
        if (accessor.Body is null && accessor.ExpressionBody is null)
        {
            return accessor;
        }

        var accessorKind = accessor.Kind();
        if (accessorKind != SyntaxKind.GetAccessorDeclaration
            && accessorKind != SyntaxKind.SetAccessorDeclaration
            && accessorKind != SyntaxKind.InitAccessorDeclaration)
        {
            return accessor;
        }

        if (accessorKind is SyntaxKind.SetAccessorDeclaration or SyntaxKind.InitAccessorDeclaration)
        {
            return null;
        }

        var methodName = GetAccessorMethodName(property.Identifier.Text, accessorKind);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        var invocation = accessorKind == SyntaxKind.GetAccessorDeclaration
            ? SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(methodName))
            : SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(methodName),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")))));

        return accessor
            .WithBody(null)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(invocation))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private static AccessorDeclarationSyntax CreateAccessorFromExpression(PropertyDeclarationSyntax property, SyntaxKind accessorKind)
    {
        var methodName = GetAccessorMethodName(property.Identifier.Text, accessorKind);
        var invocation = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(methodName));

        return SyntaxFactory.AccessorDeclaration(accessorKind)
            .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(invocation))
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private static MemberDeclarationSyntax CreateAccessorPartialMethod(
        PropertyDeclarationSyntax property,
        AccessorDeclarationSyntax accessor)
    {
        var accessorKind = accessor.Kind();
        var methodName = GetAccessorMethodName(property.Identifier.Text, accessorKind);
        var returnType = accessorKind == SyntaxKind.GetAccessorDeclaration
            ? property.Type
            : SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

        var parameterList = accessorKind == SyntaxKind.GetAccessorDeclaration
            ? SyntaxFactory.ParameterList()
            : SyntaxFactory.ParameterList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Parameter(SyntaxFactory.Identifier("value")).WithType(property.Type)));

        var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        if (!modifiers.Any(i => i.IsKind(SyntaxKind.PartialKeyword)))
        {
            modifiers = AddPartialModifier(modifiers);
        }

        return SyntaxFactory.MethodDeclaration(returnType, methodName)
            .WithModifiers(modifiers)
            .WithParameterList(parameterList)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    private static string GetAccessorMethodName(string propertyName, SyntaxKind accessorKind) =>
        accessorKind == SyntaxKind.GetAccessorDeclaration
            ? $"get__{propertyName}"
            : $"set__{propertyName}";

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
