namespace Pure.DI.Core;

readonly record struct LogMessage(string? Key, string Text)
{
    public static LogMessage From(string key, string text) =>
        new(key, text);

    public static LogMessage Format(string key, string format, params object[] args) =>
        new(key, string.Format(format, args));
}
