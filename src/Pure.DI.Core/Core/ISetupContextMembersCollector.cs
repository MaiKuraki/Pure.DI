namespace Pure.DI.Core;

using Microsoft.CodeAnalysis.CSharp.Syntax;

interface ISetupContextMembersCollector
{
    ImmutableArray<MemberDeclarationSyntax> Collect();
}
