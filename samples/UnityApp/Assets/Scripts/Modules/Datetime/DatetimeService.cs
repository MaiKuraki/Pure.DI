using System;

public class DatetimeService : IDatetimeService, IDisposable
{
    private readonly IDatetimeConfig _config;

    public DateTime Now => DateTime.UtcNow + _config.Offset;


    public DatetimeService(IDatetimeConfig config)
    {
        _config = config;
    }

    public void Initialize()
    {
        // Initialization logic if needed
    }

    public void Dispose()
    {
        // Perform any necessary cleanup here
    }
}