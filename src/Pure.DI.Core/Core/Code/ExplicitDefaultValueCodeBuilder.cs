namespace Pure.DI.Core.Code;

using System.Collections;

sealed class ExplicitDefaultValueCodeBuilder(ITypeResolver typeResolver)
    : IBuilder<CodeBuilderContext, IEnumerator>
{
    public IEnumerator Build(CodeBuilderContext data)
    {
        var var = data.Context.VarInjection.Var;
        var construct = var.AbstractNode.Construct!;
        if (!construct.Source.HasExplicitDefaultValue)
        {
            yield break;
        }

        var explicitDefaultValue = construct.Source.ExplicitDefaultValue;
        if (explicitDefaultValue is null
            && var.InstanceType.IsValueType
            && !IsNullableValueType(var.InstanceType))
        {
            var setup = data.Context.RootContext.Graph.Source;
            var.CodeExpression = $"default({typeResolver.Resolve(setup, var.InstanceType)})";
            yield break;
        }

        var.CodeExpression = explicitDefaultValue.ValueToString();
    }

    private static bool IsNullableValueType(ITypeSymbol type) =>
        type is INamedTypeSymbol
        {
            IsGenericType: true,
            Name: "Nullable",
            ContainingNamespace.Name: "System"
        };
}
