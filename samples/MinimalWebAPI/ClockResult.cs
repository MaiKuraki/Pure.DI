// ReSharper disable NotAccessedPositionalProperty.Global
using System.Text.Json.Serialization;

namespace MinimalWebAPI;

public record ClockResult(string Title, string Date, string Time);

[JsonSerializable(typeof(ClockResult))]
partial class AppJsonSerializerContext : JsonSerializerContext;
