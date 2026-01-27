namespace Pure.DI.Core;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public interface ISetupInvocationMatcher
{
    bool IsSetupInvocation(ExpressionSyntax expression);
}
