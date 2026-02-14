using Pure.DI;
using UnityEngine;

[CreateAssetMenu(menuName = "Clock/Clocks Composition", fileName = "ClocksComposition", order = 0)]
class ClocksComposition : ScriptableObject
{
    [SerializeField] private ClockConfig clockConfig;

    void Setup() => DI.Setup(kind: CompositionKind.Internal)
        .Transient(() => clockConfig)
        .Singleton<ClockService>();
}

public partial class Scope : MonoBehaviour
{
    void Setup() => DI.Setup()
        .DependsOn(nameof(ClocksComposition), SetupContextKind.Members)
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
