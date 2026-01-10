using Pure.DI;

public interface IDatetimeComposition
{
    DatetimeConfig DatetimeConfig { get; }

    void Setup() => DI.Setup(kind: CompositionKind.Internal)
        .Transient(_ => DatetimeConfig)
        .Singleton<DatetimeService>();
}