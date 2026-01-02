using System;

public class ProjectManager : IDisposable
{
    private readonly IDatetimeService _datetimeService;
    private readonly IClockService _clockService;

    public ProjectManager(IDatetimeService datetimeService, IClockService clockService)
    {
        _datetimeService = datetimeService;
        _clockService = clockService;
    }

    public void Initialize()
    {
        _datetimeService.Initialize();
        _clockService.Initialize();
    }

    public void Dispose()
    {
        // Perform any necessary cleanup here
    }
}