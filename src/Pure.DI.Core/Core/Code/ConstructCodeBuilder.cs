namespace Pure.DI.Core.Code;

using System.Collections;

sealed class ConstructCodeBuilder(
    [Tag(CodeBuilderKind.Enumerable)] IBuilder<CodeBuilderContext, IEnumerator> enumerableBuilder,
    [Tag(CodeBuilderKind.AsyncEnumerable)] IBuilder<CodeBuilderContext, IEnumerator> asyncEnumerableBuilder,
    [Tag(CodeBuilderKind.Array)] IBuilder<CodeBuilderContext, IEnumerator> arrayBuilder,
    [Tag(CodeBuilderKind.Span)] IBuilder<CodeBuilderContext, IEnumerator> spanBuilder,
    [Tag(CodeBuilderKind.Composition)] IBuilder<CodeBuilderContext, IEnumerator> compositionBuilder,
    [Tag(CodeBuilderKind.OnCannotResolve)] IBuilder<CodeBuilderContext, IEnumerator> onCannotResolveBuilder,
    [Tag(CodeBuilderKind.ExplicitDefaultValue)] IBuilder<CodeBuilderContext, IEnumerator> explicitDefaultValueBuilder)
    : IBuilder<CodeBuilderContext, IEnumerator>
{
    public IEnumerator Build(CodeBuilderContext data) =>
        data.Context.VarInjection.Var.AbstractNode.Construct?.Source.Kind switch
        {
            MdConstructKind.Enumerable => enumerableBuilder.Build(data),
            MdConstructKind.AsyncEnumerable => asyncEnumerableBuilder.Build(data),
            MdConstructKind.Array => arrayBuilder.Build(data),
            MdConstructKind.Span => spanBuilder.Build(data),
            MdConstructKind.Composition => compositionBuilder.Build(data),
            MdConstructKind.OnCannotResolve => onCannotResolveBuilder.Build(data),
            MdConstructKind.ExplicitDefaultValue => explicitDefaultValueBuilder.Build(data),
            _ => EmptyEnumerator()
        };

    private static IEnumerator EmptyEnumerator()
    {
        yield break;
    }
}
