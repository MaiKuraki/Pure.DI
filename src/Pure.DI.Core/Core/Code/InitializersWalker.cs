using System.Collections;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
namespace Pure.DI.Core.Code;

sealed class InitializersWalker(
    IInjections injections,
    InitializersWalkerContext ctx)
    : IInitializersWalker
{
    private readonly List<(Action Run, int? Ordinal)> _actions = [];
    private readonly List<VarInjection> _varInjections = [];

    public IEnumerator VisitInitializer(CodeContext codeCtx, DpInitializer initializer)
    {
        _actions.Clear();
        _varInjections.Clear();

        foreach (var field in initializer.Fields)
        {
            if (ctx.VarInjections.MoveNext())
            {
                yield return ctx.BuildVarInjection(ctx.VarInjections.Current);
                var curVariable = ctx.VarInjections.Current;
                var curField = field;
                var curCtx = codeCtx;
                _actions.Add(new(() => injections.FieldInjection(ctx.VariableName, curCtx, curField, curVariable), curField.Ordinal));
            }
        }

        foreach (var property in initializer.Properties)
        {
            if (ctx.VarInjections.MoveNext())
            {
                yield return ctx.BuildVarInjection(ctx.VarInjections.Current);
                var curVariable = ctx.VarInjections.Current;
                var curProperty = property;
                var curCtx = codeCtx;
                _actions.Add(new(() => injections.PropertyInjection(ctx.VariableName, curCtx, curProperty, curVariable), curProperty.Ordinal));
            }
        }

        foreach (var method in initializer.Methods)
        {
            var curVariables = new List<VarInjection>();
            foreach (var parameter in method.Parameters)
            {
                if (ctx.VarInjections.MoveNext())
                {
                    yield return ctx.BuildVarInjection(ctx.VarInjections.Current);
                    curVariables.Add(ctx.VarInjections.Current);
                }
            }

            var curMethod = method;
            var curCtx = codeCtx;
            _actions.Add(new(() => injections.MethodInjection(ctx.VariableName, curCtx, curMethod, curVariables), curMethod.Ordinal));
        }

        foreach (var action in _actions.OrderBy(i => i.Ordinal ?? int.MaxValue).Select(i => i.Run))
        {
            action();
        }
    }
}