using Pure.DI;

public interface IClockComposition
{
    ClockConfig ClockConfig { get; }

    void Setup() => DI.Setup(kind: CompositionKind.Internal)
        .Transient(_ => ClockConfig)
        .Singleton<ClockService>();
}