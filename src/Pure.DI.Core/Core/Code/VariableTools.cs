namespace Pure.DI.Core.Code;

using static Lifetime;

sealed class VariableTools : IVariableTools
{
    public Comparison<VarInjection> InjectionComparer => (a, b) => GetInjectionPriority(a).CompareTo(GetInjectionPriority(b));

    private static int GetInjectionPriority(VarInjection varInjection)
    {
        var node = varInjection.Var.AbstractNode;
        if (node.Arg is not null)
        {
            return 1;
        }

        if (varInjection.Var.HasCycle == true)
        {
            return 2;
        }

        if (node.Node.Implementation is not null)
        {
            return node.ActualLifetime switch
            {
                PerBlock => 10,
                Singleton => 11,
                Scoped => 12,
                PerResolve => 13,
                _ => 14
            };
        }

        if (node.Construct is { } construct)
        {
            return construct.Source.Kind switch
            {
                MdConstructKind.Accumulator => 0,
                MdConstructKind.ExplicitDefaultValue => 1,
                MdConstructKind.Composition => 1,
                MdConstructKind.OnCannotResolve => 20,
                MdConstructKind.Enumerable => 22,
                MdConstructKind.Array => 21,
                MdConstructKind.Span => 21,
                MdConstructKind.AsyncEnumerable => 22,
                MdConstructKind.Override => 28,
                _ => 29
            };
        }

        if (node.Node.Factory is {} factory)
        {
            return 30 + factory.Resolvers.Length + factory.Initializers.Length * 100 + (factory.HasOverrides ? 1000 : 0);
        }

        return int.MaxValue;
    }
}
