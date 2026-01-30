// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core.Code.Parts;

sealed class ArgFieldsBuilder(ITypeResolver typeResolver)
    : IClassPartBuilder
{
    public ClassPart Part => ClassPart.ArgFields;

    public CompositionCode Build(CompositionCode composition)
    {
        var classArgs = composition.ClassArgs.GetArgsOfKind(ArgKind.Composition).ToList();
        if (classArgs.Count == 0 && composition.SetupContextArgs.Length == 0)
        {
            return composition;
        }

        var code = composition.Code;
        var membersCounter = composition.MembersCount;
        foreach (var arg in classArgs)
        {
            code.AppendLine($"[{Names.NonSerializedAttributeTypeName}] private readonly {typeResolver.Resolve(composition.Source.Source, arg.InstanceType)} {arg.Name};");
            membersCounter++;
        }

        foreach (var arg in composition.SetupContextArgs)
        {
            if (classArgs.Any(existing => existing.Name == arg.Name))
            {
                continue;
            }

            code.AppendLine($"[{Names.NonSerializedAttributeTypeName}] private readonly {typeResolver.Resolve(composition.Source.Source, arg.Type)} {arg.Name};");
            membersCounter++;
        }

        return composition with { MembersCount = membersCounter };
    }
}
