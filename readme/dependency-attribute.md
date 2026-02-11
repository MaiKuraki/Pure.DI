#### Dependency attribute

When applied to a property or field, the member participates in DI, ordered by ordinal (ascending).
When this occurs: you need this feature while building the composition and calling roots.
What it solves: provides a clear setup pattern and expected behavior without extra boilerplate or manual wiring.
How it is solved in the example: shows the minimal DI configuration and how the result is used in code.


```c#
using Shouldly;
using Pure.DI;
using System.Text;

DI.Setup(nameof(PersonComposition))
    .Arg<int>("personId")
    .Arg<string>("personName")
    .Arg<DateTime>("personBirthday")
    .Bind().To<Person>()

    // Composition root
    .Root<IPerson>("Person");

var composition = new PersonComposition(
    personId: 123,
    personName: "Nik",
    personBirthday: new DateTime(1977, 11, 16));

var person = composition.Person;
person.Name.ShouldBe("123 Nik 1977-11-16");

interface IPerson
{
    string Name { get; }
}

class Person : IPerson
{
    private readonly StringBuilder _name = new();

    public string Name => _name.ToString();

    [Dependency] public int Id;

    // The Ordinal attribute specifies to perform an injection,
    // the integer value in the argument specifies
    // the ordinal of injection
    [Dependency(ordinal: 1)]
    public string FirstName
    {
        set
        {
            _name.Append(Id);
            _name.Append(' ');
            _name.Append(value);
        }
    }

    [Dependency(ordinal: 2)]
    public DateTime Birthday
    {
        set
        {
            _name.Append(' ');
            _name.Append($"{value:yyyy-MM-dd}");
        }
    }
}
```

<details>
<summary>Running this code sample locally</summary>

- Make sure you have the [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) or later installed
```bash
dotnet --list-sdk
```
- Create a net10.0 (or later) console application
```bash
dotnet new console -n Sample
```
- Add references to the NuGet packages
  - [Pure.DI](https://www.nuget.org/packages/Pure.DI)
  - [Shouldly](https://www.nuget.org/packages/Shouldly)
```bash
dotnet add package Pure.DI
dotnet add package Shouldly
```
- Copy the example code into the _Program.cs_ file

You are ready to run the example 🚀
```bash
dotnet run
```

</details>

The `Dependency` attribute is part of the API, but you can define your own in any assembly or namespace.
What it shows:
- Demonstrates the scenario setup and resulting object graph in Pure.DI.

Important points:
- Highlights the key configuration choices and their effect on resolution.

Useful when:
- You want a concrete template for applying this feature in a composition.


The following partial class will be generated:

```c#
partial class PersonComposition
{
#if NET9_0_OR_GREATER
  private readonly Lock _lock;
#else
  private readonly Object _lock;
#endif

  private readonly int _argPersonId;
  private readonly string _argPersonName;
  private readonly DateTime _argPersonBirthday;

  [OrdinalAttribute(128)]
  public PersonComposition(int personId, string personName, DateTime personBirthday)
  {
    _argPersonId = personId;
    _argPersonName = personName ?? throw new ArgumentNullException(nameof(personName));
    _argPersonBirthday = personBirthday;
#if NET9_0_OR_GREATER
    _lock = new Lock();
#else
    _lock = new Object();
#endif
  }

  public IPerson Person
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get
    {
      var transientPerson179 = new Person();
      transientPerson179.Id = _argPersonId;
      transientPerson179.FirstName = _argPersonName;
      transientPerson179.Birthday = _argPersonBirthday;
      return transientPerson179;
    }
  }
}
```

Class diagram:

```mermaid
---
 config:
  maxTextSize: 2147483647
  maxEdges: 2147483647
  class:
   hideEmptyMembersBox: true
---
classDiagram
	Person --|> IPerson
	PersonComposition ..> Person : IPerson Person
	Person o-- Int32 : Argument "personId"
	Person o-- String : Argument "personName"
	Person o-- DateTime : Argument "personBirthday"
	namespace Pure.DI.UsageTests.Attributes.DependencyAttributeScenario {
		class IPerson {
			<<interface>>
		}
		class Person {
				<<class>>
			+Person()
			+Int32 Id
			+String FirstName
			+DateTime Birthday
		}
		class PersonComposition {
		<<partial>>
		+IPerson Person
		}
	}
	namespace System {
		class DateTime {
				<<struct>>
		}
		class Int32 {
				<<struct>>
		}
		class String {
				<<class>>
		}
	}
```

