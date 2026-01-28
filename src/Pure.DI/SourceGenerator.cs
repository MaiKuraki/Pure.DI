// ReSharper disable InvocationIsSkipped

namespace Pure.DI;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pure.DI.Core;

[Generator(LanguageNames.CSharp)]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1036:Specify analyzer banned API enforcement setting")]
public class SourceGenerator : IIncrementalGenerator
{
    private static readonly Generator Generator = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // ReSharper disable once InvocationIsSkipped
        // Run Rider as administrator
        DebugHelper.Debug();

        context.RegisterPostInitializationOutput(initializationContext => {
            foreach (var apiSource in Generator.Api)
            {
                initializationContext.AddSource(apiSource.HintName, apiSource.SourceText);
            }
        });

        var setupContexts = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is InvocationExpressionSyntax invocation && IsPotentialSetup(invocation),
                static (syntaxContext, _) => syntaxContext)
            .Collect();

        var optionsProvider = context.AnalyzerConfigOptionsProvider
            .Combine(context.ParseOptionsProvider);

        var valuesProvider = optionsProvider
            .Combine(context.CompilationProvider)
            .Combine(setupContexts);

        context.RegisterSourceOutput(valuesProvider, (sourceProductionContext, options) =>
        {
            var ((configAndParse, _), updates) = options;
            var (config, parseOptions) = configAndParse;
            Generator.Generate(
                parseOptions,
                config,
                sourceProductionContext,
                updates,
                sourceProductionContext.CancellationToken);
        });
    }

    private static bool IsPotentialSetup(InvocationExpressionSyntax invocation) =>
        invocation.Expression switch
        {
            IdentifierNameSyntax { Identifier.Text: "Setup" } => true,
            MemberAccessExpressionSyntax { Name.Identifier.Text: "Setup" } => true,
            _ => false
        };
}
