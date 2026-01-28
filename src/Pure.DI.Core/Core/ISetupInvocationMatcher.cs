namespace Pure.DI.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public interface ISetupInvocationMatcher
{
    bool IsSetupInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel);
}
