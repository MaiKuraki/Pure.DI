using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DatetimeConfig", menuName = "Configs/Datetime")]
public class DatetimeConfig : ScriptableObject, IDatetimeConfig
{
    [SerializeField] private int _offsetHours;

    public TimeSpan Offset => TimeSpan.FromHours(_offsetHours);
}