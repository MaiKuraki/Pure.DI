namespace Pure.DI.Core.Code;

interface ITypeResolver
{
    TypeDescription Resolve(MdSetup setup, ITypeSymbol type);

    TypeDescription ResolveRuntime(MdSetup setup, ITypeSymbol type);
}
