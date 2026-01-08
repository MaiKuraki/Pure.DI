namespace Pure.DI.Core.Code;

using System.Collections;

sealed class OnCannotResolveCodeBuilder : IBuilder<CodeBuilderContext, IEnumerator>
{
    public IEnumerator Build(CodeBuilderContext data)
    {
        var ctx = data.Context;
        var varInjection = ctx.VarInjection;
        var var = varInjection.Var;
        var.CodeExpression = $"{Names.OnCannotResolve}<{varInjection.ContractType}>({varInjection.Injection.Tag.ValueToString()}, {var.AbstractNode.Lifetime.ValueToString()})";
        yield break;
    }
}
