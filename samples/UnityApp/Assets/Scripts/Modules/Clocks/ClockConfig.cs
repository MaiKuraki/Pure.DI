using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ClockConfig", menuName = "Configs/Clock")]
public class ClockConfig : ScriptableObject, IClockConfig
{
    [SerializeField] bool showDigital;
    [SerializeField] ClockDigital clockDigitalPrefab;

    public bool ShowDigital => showDigital;
    public ClockDigital ClockDigitalPrefab => clockDigitalPrefab;
}