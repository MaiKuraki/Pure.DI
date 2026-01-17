namespace Pure.DI.Core;

interface ISetupFinalizer
{
    MdSetup Finalize(MdSetup setup, IReadOnlyDictionary<CompositionName, MdSetup> setupMap);
}