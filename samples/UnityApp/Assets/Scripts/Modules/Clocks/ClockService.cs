using Pure.DI;
using UnityEngine;
using Object = UnityEngine.Object;

public class ClockService : IClockService
{
    private readonly IClockConfig _config;
    private readonly IClockFactory _factory;

    public ClockService(IClockConfig config, IClockFactory factory)
    {
        _config = config;
        _factory = factory;
    }

    public void Initialize()
    {
        if (_config.ShowDigital)
        {
            _factory.Instantiate(_config.ClockDigitalPrefab);
        }
    }
}