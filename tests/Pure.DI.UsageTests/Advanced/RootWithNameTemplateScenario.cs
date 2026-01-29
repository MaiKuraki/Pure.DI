/*
$v=true
$p=2
$d=Root with name template
$h=When this occurs: you need this feature while building the composition and calling roots.
$h=What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
$h=How it is solved in the example: shows the minimal DI configuration and how the result is used in code.
$f=
$f=What it shows:
$f=- Demonstrates the scenario setup and resulting object graph in Pure.DI.
$f=
$f=Important points:
$f=- Highlights the key configuration choices and their effect on resolution.
$f=
$f=Useful when:
$f=- You want a concrete template for applying this feature in a composition.
$f=
$r=Shouldly
*/

// ReSharper disable CheckNamespace
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedVariable
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeMemberModifiers
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBeMadeStatic.Global
#pragma warning disable CS9113 // Parameter is unread.
#pragma warning disable CA1822

namespace Pure.DI.UsageTests.Advanced.RootWithNameTemplateScenario;

using Shouldly;
using Xunit;

// {
//# using Pure.DI;
// }

public class Scenario
{
    [Fact]
    public void Run()
    {
        // Disable Resolve methods to keep the public API minimal
        // Resolve = Off
// {
        DI.Setup("Composition")
            // The name template "My{type}" specifies that the root property name
            // will be formed by adding the prefix "My" to the type name "ApiClient".
            .Root<ApiClient>("My{type}");

        var composition = new Composition();

        // The property name is "MyApiClient" instead of "ApiClient"
        // thanks to the name template "My{type}"
        var apiClient = composition.MyApiClient;

        apiClient.GetProfile().ShouldBe("Content from https://example.com/profile");
        // }
        composition.SaveClassDiagram();
    }
}

// {
class NetworkClient
{
    public string Get(string uri) => $"Content from {uri}";
}

class ApiClient(NetworkClient client)
{
    public string GetProfile() => client.Get("https://example.com/profile");
}
// }