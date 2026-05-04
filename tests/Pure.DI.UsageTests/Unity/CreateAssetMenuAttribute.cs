// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Pure.DI.UsageTests.Unity;

[AttributeUsage(AttributeTargets.Class)]
public class CreateAssetMenuAttribute: Attribute
{
    public string? fileName { get; set; }

    public string? menuName{ get; set; }
}