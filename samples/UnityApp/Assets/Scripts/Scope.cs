using Pure.DI;
using UnityEngine;

[CreateAssetMenu(menuName = "Clock/Clocks Composition", fileName = "ClocksComposition", order = 0)]
public partial class ClocksComposition : ScriptableObject
{
    [SerializeField] public ClockConfig clockConfig;

    // #1 Variant
    void Setup() => DI.Setup(nameof(ClocksComposition), kind: CompositionKind.Internal)
        .SpecialType<ScriptableObject>()
        .Transient(() => clockConfig)
        .Singleton<ClockService>();

    // #2 Variant
    // void Setup() => DI.Setup()
    //     .SpecialType<ScriptableObject>()
    //     .Transient(() => clockConfig)
    //     .Singleton<ClockService>()
    //     .Root<IClockService>(nameof(IClockService), kind: RootKinds.Exposed);
}

public partial class Scope : MonoBehaviour
{
    [SerializeField] public ClocksComposition clocksComposition;

    void Setup() => DI.Setup()
        .DependsOn(nameof(ClocksComposition)) // #1 Variant
        // .Bind().To(_ => clocksComposition) // #2 Variant
        .SpecialType<MonoBehaviour>()
        .Root<ClockManager>(nameof(ClockManager))
        .Builders<MonoBehaviour>();

    void Start()
    {
        ClockManager.Start();
    }

    void OnDestroy()
    {
        Dispose();
    }
}
