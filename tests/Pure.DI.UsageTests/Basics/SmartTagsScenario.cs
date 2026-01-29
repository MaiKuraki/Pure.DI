/*
$v=true
$p=7
$d=Smart tags
$h=Large object graphs often need many tags. String tags are error-prone and easy to mistype. Prefer `Enum` values as tags, and Pure.DI helps make this safe.
$h=
$h=When the compiler cannot determine a tag value, Pure.DI generates a constant inside `Pure.DI.Tag`. For the example below, the generated constants would look like this:
$h=
$h=```c#
$h=namespace Pure.DI
$h={
$h=  internal partial class Tag
$h=  {
$h=    public const string Abc = "Abc";
$h=    public const string Xyz = "Xyz";
$h=  }
$h=}
$h=```
$h=This enables safe refactoring and compiler-checked tag usage, reducing errors.
$h=
$h=![](smart_tags.gif)
$h=
$h=The example below also uses the `using static Pure.DI.Tag;` directive to access tags in `Pure.DI.Tag` without specifying a type name:
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
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable PreferConcreteValueOverDefault
// ReSharper disable RedundantNameQualifier
namespace Pure.DI.UsageTests.Basics.SmartTagsScenario;

using Shouldly;
using static Pure.DI.Tag;
using static Pure.DI.Lifetime;
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
        //# using static Pure.DI.Tag;
        //# using static Pure.DI.Lifetime;

        DI.Setup(nameof(Composition))
            // The `default` tag is used to resolve dependencies
            // when the tag was not specified by the consumer
            .Bind<IMessageSender>(Email, default).To<EmailSender>()
            .Bind<IMessageSender>(Sms).As(Singleton).To<SmsSender>()
            .Bind<IMessagingService>().To<MessagingService>()

            // "SmsSenderRoot" is root name, Sms is tag
            .Root<IMessageSender>("SmsSenderRoot", Sms)

            // Specifies to create the composition root named "Root"
            .Root<IMessagingService>("MessagingService");

        var composition = new Composition();
        var messagingService = composition.MessagingService;
        messagingService.EmailSender.ShouldBeOfType<EmailSender>();
        messagingService.SmsSender.ShouldBeOfType<SmsSender>();
        messagingService.SmsSender.ShouldBe(composition.SmsSenderRoot);
        messagingService.DefaultSender.ShouldBeOfType<EmailSender>();
// }
        composition.SaveClassDiagram();
    }
}

// {
interface IMessageSender;

class EmailSender : IMessageSender;

class SmsSender : IMessageSender;

interface IMessagingService
{
    IMessageSender EmailSender { get; }

    IMessageSender SmsSender { get; }

    IMessageSender DefaultSender { get; }
}

class MessagingService(
    [Tag(Email)] IMessageSender emailSender,
    [Tag(Sms)] IMessageSender smsSender,
    IMessageSender defaultSender)
    : IMessagingService
{
    public IMessageSender EmailSender { get; } = emailSender;

    public IMessageSender SmsSender { get; } = smsSender;

    public IMessageSender DefaultSender { get; } = defaultSender;
}
// }
