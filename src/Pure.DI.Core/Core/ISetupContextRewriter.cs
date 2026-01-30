namespace Pure.DI.Core;

interface ISetupContextRewriter
{
    LambdaExpressionSyntax Rewrite(LambdaExpressionSyntax lambda);
}
