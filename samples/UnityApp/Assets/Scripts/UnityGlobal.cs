using Pure.DI;
using UnityEngine;

class UnityGlobal
{
    void Setup() => DI.Setup(kind: CompositionKind.Global)
        .SpecialType<ScriptableObject>()
        .SpecialType<MonoBehaviour>()
        .Builders<MonoBehaviour>();
}