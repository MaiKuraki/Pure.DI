namespace Pure.DI.Core.Code;

sealed class CodeNameProvider : ICodeNameProvider
{
    private const string BaseName = "T";

    public string GetConstructorName(string className)
    {
        var nameSyntax = SyntaxFactory.ParseName(className);
        var simpleName = GetRightmostSimpleName(nameSyntax);
        return simpleName is null ? className : simpleName.Identifier.Text;
    }

    public string GetUniqueTypeParameterName(string className)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        var nameSyntax = SyntaxFactory.ParseName(className);
        foreach (var genericName in nameSyntax.DescendantNodesAndSelf().OfType<GenericNameSyntax>())
        {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var typeArg in genericName.TypeArgumentList.Arguments)
            {
                foreach (var identifier in typeArg.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
                {
                    var name = identifier.Identifier.Text;
                    if (IsSimpleTypeParameterName(name))
                    {
                        used.Add(name);
                    }
                }
            }
        }

        if (!used.Contains(BaseName))
        {
            return BaseName;
        }

        var index = 1;
        while (used.Contains($"{BaseName}{index}"))
        {
            index++;
        }

        return $"{BaseName}{index}";
    }

    private static bool IsSimpleTypeParameterName(string name)
    {
        if (name == BaseName)
        {
            return true;
        }

        if (name.Length < 2 || !name.StartsWith(BaseName))
        {
            return false;
        }

        for (var i = 1; i < name.Length; i++)
        {
            if (!char.IsDigit(name[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static SimpleNameSyntax? GetRightmostSimpleName(NameSyntax nameSyntax) =>
        nameSyntax switch
        {
            QualifiedNameSyntax qualified => GetRightmostSimpleName(qualified.Right),
            AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name,
            SimpleNameSyntax simple => simple,
            _ => null
        };
}
