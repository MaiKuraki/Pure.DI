/*
$v=true
$p=15
$d=Bind attribute for a generic type
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

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable UnusedTypeParameter
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CA1822
namespace Pure.DI.UsageTests.Basics.BindAttributeForGenericTypeScenario;

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
        DI.Setup(nameof(Composition))
            .Bind().As(Lifetime.Singleton).To<CommentsFactory>()
            .Bind().To<ArticleService>()

            // Composition root
            .Root<IArticleService>("ArticleService");

        var composition = new Composition();
        var articleService = composition.ArticleService;
        articleService.DisplayComments();
        // }
        composition.SaveClassDiagram();
    }
}

// {
interface IComments<T>
{
    void Load();
}

class Comments<T> : IComments<T>
{
    public void Load()
    {
    }
}

class CommentsFactory
{
    // The 'TT' type marker in the attribute indicates that this method
    // can produce 'IComments<T>' for any generic type 'T'.
    // This allows the factory to handle all requests for IComments<T>.
    [Bind(typeof(IComments<TT>))]
    public IComments<T> Create<T>() => new Comments<T>();
}

interface IArticleService
{
    void DisplayComments();
}

class ArticleService(IComments<Article> comments) : IArticleService
{
    public void DisplayComments() => comments.Load();
}

class Article;
// }

#pragma warning restore CA1822