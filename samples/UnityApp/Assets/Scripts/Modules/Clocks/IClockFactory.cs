using UnityEngine;

public interface IClockFactory
{
    ClockDigital Instantiate(ClockDigital prefab);
}