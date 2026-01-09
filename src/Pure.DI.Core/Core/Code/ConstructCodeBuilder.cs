namespace Pure.DI.Core.Code;

using System.Collections;
using static MdConstructKind;

sealed class ConstructCodeBuilder(
    [Tag(CodeBuilderKind.Enumerable)] IBuilder<CodeBuilderContext, IEnumerator> enumerableBuilder,
    [Tag(CodeBuilderKind.AsyncEnumerable)] IBuilder<CodeBuilderContext, IEnumerator> asyncEnumerableBuilder,
    [Tag(CodeBuilderKind.Array)] IBuilder<CodeBuilderContext, IEnumerator> arrayBuilder,
    [Tag(CodeBuilderKind.Span)] IBuilder<CodeBuilderContext, IEnumerator> spanBuilder,
    [Tag(CodeBuilderKind.Composition)] IBuilder<CodeBuilderContext, IEnumerator> compositionBuilder,
    [Tag(CodeBuilderKind.CannotResolve)] IBuilder<CodeBuilderContext, IEnumerator> onCannotResolveBuilder,
    [Tag(CodeBuilderKind.ExplicitDefaultValue)] IBuilder<CodeBuilderContext, IEnumerator> explicitDefaultValueBuilder)
    : IBuilder<CodeBuilderContext, IEnumerator>
{
    public IEnumerator Build(CodeBuilderContext data) =>
        data.Context.VarInjection.Var.AbstractNode.Construct?.Source.Kind switch
        {
            Enumerable => enumerableBuilder.Build(data),
            AsyncEnumerable => asyncEnumerableBuilder.Build(data),
            Array => arrayBuilder.Build(data),
            Span => spanBuilder.Build(data),
            Composition => compositionBuilder.Build(data),
            OnCannotResolve => onCannotResolveBuilder.Build(data),
            ExplicitDefaultValue => explicitDefaultValueBuilder.Build(data),
            _ => EmptyEnumerator()
        };

    private static IEnumerator EmptyEnumerator()
    {
        yield break;
    }
}
