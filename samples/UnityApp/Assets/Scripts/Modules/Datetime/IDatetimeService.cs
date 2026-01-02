using System;

public interface IDatetimeService
{
    DateTime Now { get; }

    void Initialize();
}