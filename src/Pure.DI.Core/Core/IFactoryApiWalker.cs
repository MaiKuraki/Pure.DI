namespace Pure.DI.Core;

interface IFactoryApiWalker
{
    IReadOnlyCollection<FactoryMeta> Meta{ get; }

    void Initialize(SemanticModel semanticModel, ParameterSyntax contextParameter, LambdaExpressionSyntax rootLambda);

    void Visit(SyntaxNode? node);
}
