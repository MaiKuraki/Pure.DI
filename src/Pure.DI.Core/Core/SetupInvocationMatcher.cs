namespace Pure.DI.Core;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SetupInvocationMatcher : ISetupInvocationMatcher
{
    public bool IsSetupInvocation(ExpressionSyntax expression) =>
        expression switch
        {
            IdentifierNameSyntax { Identifier.Text: "Setup" } => true,
            MemberAccessExpressionSyntax memberAccess
                when memberAccess.Kind() == SyntaxKind.SimpleMemberAccessExpression
                     && memberAccess.Name.Identifier.Text == "Setup"
                     && (memberAccess.Expression is IdentifierNameSyntax { Identifier.Text: "DI" }
                         || memberAccess.Expression is MemberAccessExpressionSyntax firstMemberAccess
                         && firstMemberAccess.Kind() == SyntaxKind.SimpleMemberAccessExpression
                         && firstMemberAccess.Name.Identifier.Text == "DI") => true,
            _ => false
        };
}
