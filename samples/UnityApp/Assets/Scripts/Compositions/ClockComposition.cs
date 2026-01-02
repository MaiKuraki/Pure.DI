using Pure.DI;
using UnityEngine;
using static Pure.DI.Lifetime;

public partial class ClockComposition
{
    [SerializeField] public ClockConfig clockConfig;

    void Setup() => DI.Setup(kind: CompositionKind.Global)
                        .DefaultLifetime(Singleton)
                        .Bind().To(_ => clockConfig)
                        .Bind().To<ClockService>();
}