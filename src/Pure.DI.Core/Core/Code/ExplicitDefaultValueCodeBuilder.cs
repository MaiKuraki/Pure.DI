namespace Pure.DI.Core.Code;

using System.Collections;

sealed class ExplicitDefaultValueCodeBuilder : IBuilder<CodeBuilderContext, IEnumerator>
{
    public IEnumerator Build(CodeBuilderContext data)
    {
        var var = data.Context.VarInjection.Var;
        var construct = var.AbstractNode.Construct!;
        var.CodeExpression = construct.Source.ExplicitDefaultValue.ValueToString();
        yield break;
    }
}
