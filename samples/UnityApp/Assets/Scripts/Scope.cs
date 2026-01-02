using Pure.DI;
using UnityEngine;
using static Pure.DI.Lifetime;

public partial class Scope : MonoBehaviour
{
    void Setup() => DI.Setup().Builders<MonoBehaviour>();

    void Start()
    {
        ClockManager.Initialize();
    }

    void OnDestroy()
    {
        Dispose();
    }
}
