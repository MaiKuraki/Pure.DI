namespace Pure.DI.Core.Code;

interface IInitializersWalker
{
    IEnumerator VisitInitializer(CodeContext ctx, DpInitializer initializer);
}