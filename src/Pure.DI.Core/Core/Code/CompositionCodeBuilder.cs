namespace Pure.DI.Core.Code;

using System.Collections;

sealed class CompositionCodeBuilder : IBuilder<CodeBuilderContext, IEnumerator>
{
    public IEnumerator Build(CodeBuilderContext data)
    {
        data.Context.VarInjection.Var.CodeExpression = "this";
        yield break;
    }
}
