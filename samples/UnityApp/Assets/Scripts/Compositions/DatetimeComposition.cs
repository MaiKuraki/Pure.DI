using Pure.DI;
using UnityEngine;
using static Pure.DI.Lifetime;

public partial class BaseComposition
{
    [SerializeField] public DatetimeConfig datetimeConfig;

    void SetupDatetime() => DI.Setup(kind: CompositionKind.Internal)
                        .DefaultLifetime(Singleton)
                        .Bind().To(_ => datetimeConfig)
                        .Bind().To<DatetimeService>();
}