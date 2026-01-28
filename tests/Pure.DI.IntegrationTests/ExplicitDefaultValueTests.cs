namespace Pure.DI.IntegrationTests;

using Core;

/// <summary>
/// Tests related to explicit default value handling.
/// </summary>
public class ExplicitDefaultValueTests
{
    [Fact]
    public async Task ShouldReportMissingDependencyWhenOtherInitializerUsesExplicitDefaultValue()
    {
        // Given

        // When
        var result = await """
                           using System;
                           using Pure.DI;

                           namespace Sample
                           {
                               class Service
                               {
                                   public string First { get; private set; } = "";
                                   public string Second { get; private set; } = "";

                                   [Dependency]
                                   public void InitFirst(string value = "Default")
                                   {
                                       First = value;
                                   }

                                   [Dependency]
                                   public void InitSecond(string value)
                                   {
                                       Second = value;
                                   }
                               }

                               static class Setup
                               {
                                   private static void SetupComposition()
                                   {
                                       DI.Setup("Composition")
                                           .Bind<Service>().To<Service>()
                                           .Root<Service>("Root");
                                   }
                               }

                               public class Program
                               {
                                   public static void Main()
                                   {
                                       var composition = new Composition();
                                       var service = composition.Root;
                                       Console.WriteLine(service.First);
                                       Console.WriteLine(service.Second);
                                   }
                               }
                           }
                           """.RunAsync(new Options(LanguageVersion.CSharp9, CheckCompilationErrors: false));

        // Then
        result.Success.ShouldBeFalse(result);
        result.StdOut.Any(line => line.Contains("InitSecond", StringComparison.Ordinal)).ShouldBeTrue(result);
    }
}
