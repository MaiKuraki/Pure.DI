using System;

public class DatetimeService : IDatetimeService, IDisposable
{
    private readonly IDatetimeConfig _config;

    public DatetimeService(IDatetimeConfig config)
    {
        _config = config;
    }

    public DateTime Now => DateTime.UtcNow + _config.Offset;

    public void Dispose()
    {
        // Perform any necessary cleanup here
    }
}