using Pure.DI;
using UnityEngine;
using static Pure.DI.Lifetime;

public partial class DatetimeComposition
{
    [SerializeField] public DatetimeConfig datetimeConfig;

    void Setup() => DI.Setup(kind: CompositionKind.Global)
                        .DefaultLifetime(Singleton)
                        .Bind().To(_ => datetimeConfig)
                        .Bind().To<DatetimeService>();
}