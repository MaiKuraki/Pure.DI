using Pure.DI;
using UnityEngine;
using static Pure.DI.Lifetime;

public partial class BaseComposition: MonoBehaviour
{
    [SerializeField] public ClockConfig clockConfig;

    void SetupClock() => DI.Setup(kind: CompositionKind.Internal)
                        .DefaultLifetime(Singleton)
                        .Bind().To(_ => clockConfig)
                        .Bind().To<ClockService>();
}