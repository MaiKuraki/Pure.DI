using Pure.DI;
using UnityEngine;
// ReSharper disable InconsistentNaming
// ReSharper disable RequiredBaseTypesIsNotInherited
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable Unity.RedundantSerializeFieldAttribute
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable UnusedMember.Local

internal class ClocksComposition
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
