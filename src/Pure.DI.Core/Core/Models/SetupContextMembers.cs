namespace Pure.DI.Core.Models;

using Microsoft.CodeAnalysis.CSharp.Syntax;

readonly record struct SetupContextMembers(
    CompositionName SetupName,
    string ContextName,
    ImmutableArray<MemberDeclarationSyntax> Members);
