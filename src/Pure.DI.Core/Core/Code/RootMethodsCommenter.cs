namespace Pure.DI.Core.Code;

sealed class RootMethodsCommenter(
    IFormatter formatter,
    IComments comments)
    : ICommenter<Root>
{
    public void AddComments(CompositionCode composition, Root root)
    {
        var hints = composition.Hints;
        if (!hints.IsCommentsEnabled)
        {
            return;
        }

        var rootComments = root.Source.Comments;
        var code = composition.Code;
        if (rootComments.Count > 0 && comments.IsXml(rootComments))
        {
            foreach (var comment in comments.FormatXml(rootComments))
            {
                code.AppendLine(comment);
            }

            return;
        }

        code.AppendLine("/// <summary>");
        try
        {
            code.AppendLine("/// <para>");
            try
            {
                if (rootComments.Count > 0)
                {
                    foreach (var comment in comments.Format(rootComments, true))
                    {
                        code.AppendLine(comment);
                    }
                }
                else
                {
                    code.AppendLine($"/// Provides a composition root of type {formatter.FormatRef(root.Node.Type)}.");
                }
            }
            finally
            {
                code.AppendLine("/// </para>");
            }

            if (root.RootArgs.Length > 0)
            {
                code.AppendLine("/// <para>");
                code.AppendLine($"/// This root requires root arguments and is generated as a method. It cannot be resolved by generated {hints.ResolveMethodName}/{hints.ResolveByTagMethodName} methods.");
                code.AppendLine("/// </para>");
            }

            if (!root.IsPublic)
            {
                return;
            }

            code.AppendLine("/// <example>");
            code.AppendLine($"/// This example shows how to get an instance of type {formatter.FormatRef(root.Node.Type)}:");
            code.AppendLine("/// <code>");
            var args = composition.ClassArgs
                .Where(i => i.Node.Arg?.Source.Kind == ArgKind.Composition)
                .Select(arg => arg.Name)
                .Concat(composition.SetupContextArgs.Where(arg => arg.Kind == SetupContextKind.Argument).Select(arg => arg.Name));
            code.AppendLine($"/// {(composition.TotalDisposablesCount == 0 ? "" : "using ")}var composition = new {composition.Name.ClassName}({string.Join(", ", args)});");
            code.AppendLine($"/// var instance = composition.{formatter.Format(root)};");
            code.AppendLine("/// </code>");
            code.AppendLine("/// </example>");
        }
        finally
        {
            code.AppendLine("/// </summary>");
        }
    }
}
