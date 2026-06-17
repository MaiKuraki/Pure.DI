using Pure.DI;
using static Pure.DI.CompositionKind;
using static Pure.DI.Lifetime;

namespace _PureDIProjectName_;

internal class $(CompositionName)
{
    private void Setup() => DI.Setup(kind: Global)
        .Bind().As(Singleton).To<ConsoleAdapter>();
}
