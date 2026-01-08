using Pure.DI;
using UnityEngine;

public partial class Scope : MonoBehaviour
{
    [SerializeField] public ClockConfig clockConfig;

    void Setup() => DI.Setup()
        .SpecialType<MonoBehaviour>()
        .Transient(() => clockConfig)
        .Singleton<ClockService>()
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
