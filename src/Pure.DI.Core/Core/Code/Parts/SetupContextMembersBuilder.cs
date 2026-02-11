// ReSharper disable ClassNeverInstantiated.Global

namespace Pure.DI.Core.Code.Parts;

sealed class SetupContextMembersBuilder
    : IClassPartBuilder
{
    public ClassPart Part => ClassPart.SetupContextMembers;

    public CompositionCode Build(CompositionCode composition)
    {
        if (composition.SetupContextMembers.IsDefaultOrEmpty)
        {
            return composition;
        }

        var membersCounter = composition.MembersCount;
        var code = composition.Code;
        var hasMembers = false;
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var setupContext in composition.SetupContextMembers)
        {
            foreach (var member in setupContext.Members)
            {
                if (hasMembers)
                {
                    code.AppendLine();
                }

                hasMembers = true;
                var normalized = member.NormalizeWhitespace("\t", Environment.NewLine).GetText().Lines;
                foreach (var line in normalized)
                {
                    code.AppendLine(line.ToString());
                }

                membersCounter++;
            }
        }

        return composition with { MembersCount = membersCounter };
    }
}
