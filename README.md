<!--- PROJECT SHIELDS --->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![GPL v3 License][license-shield]][license-url]


<div align="center">
<img src="img/Charian-logo-orange-text.png" width="250" align="center">

**_"Schema-independent integration through application-controlled late binding."_**
</div>

<!--- TABLE OF CONTENTS --->
# Table of Contents
1. [What Is Charian](#what-is-charian)
   - [A simple example](#a-simple-example)
   - [Why Charian](#why-charian)
   - [When to use Charian](#when-to-use-charian)
2. [Getting Started](#getting-started)
   - [Installation](#installation)
   - [Example: serializing a simple data object](#example-serializing-a-simple-data-object)
3. [Core Concepts](#core-concepts)
   - [The late-binding analogy](#the-late-binding-analogy)
   - [Class Rda - an RDA encoder/parser](#class-rda---an-rda-encoderparser)
   - [Interface IRda - app-layer schema resolution](#interface-irda---app-layer-schema-resolution)
4. [Use Cases](#use-cases)
5. [License & Commercial Use](#license--commercial-use)
   - [Commercial licensing](#commercial-licensing)
6. [Get Involved](#get-involved)
   - [Write parser/encoder in more languages](#write-parserencoder-in-more-languages)
   - [Write test cases](#write-test-cases)
   - [Write documentation](#write-documentation)


# What Is Charian

**Charian** (pron. /ka-ri-en/) is a lightweight, dependency-free serialization library for applications whose data models evolve independently.

Unlike Protocol Buffers, Avro, or JSON Schema, Charian requires **no schema definitions**, **no code generation**, and **no schema version management**. Instead, applications explicitly control how they pack and unpack data, making long-lived integrations more resilient to change.

Built on the [**Recursive Delimited Array (RDA)**](https://github.com/foldda/rda) format, Charian serializes arbitrary object structures into a portable, language-independent text representation while remaining small enough to understand completely—its core implementation is approximately 800 lines of code with zero third-party dependencies.

Therefore Charian provides:

* **No schema, ever** — no `.proto`/`.avsc` files, no code generation, no schema registry to keep in sync
* **Late-binding data resolution** — each side interprets the data at read-time, so sender and receiver don't need to agree on a model in advance
* **Structural independence** — a field added, removed, or reordered on one side doesn't break the other
* **Recursive, self-similar containers** — an Rda can hold another Rda at any depth, so arbitrarily complex objects decompose the same simple way
* **Multiple schema versions side-by-side** — pack several versions of the same dataset in one payload and let the receiver pick the right one
* **Tiny footprint** — ~800 lines of code, zero third-party dependencies

### A simple example

```csharp
Person person = new Person("John", "Smith");

// [sender] serialize ...
string text = person.ToRda().ToString();

// [receiver] deserialize ...
Person restored = new Person();
restored.FromRda(Rda.Parse(text));
```

ToRda() - serialized, FromRda() - deserialized, that's all it takes.

No schema files.

No code generation.

No serialization attributes.

Just explicit, application-controlled serialization that remains robust as your software evolves.

### Why Charian

Protobuf, Avro, and JSON Schema all solve data exchange by fixing a schema up front and generating code from it. That works well when both sides of a connection are owned by the same team and evolve together. Charian takes a different approach: it skips the schema entirely, so there's nothing to keep in sync in the first place. Instead, the connecting systems are responsible for managing the established, or evolved, data models — an approach known as [data exchange late-binding](#the-late-binding-analogy).

The core difference is **tight coupling vs. loose coupling**. Protobuf and Avro require both sides of a connection to share a schema — that shared contract is what enables their compactness and compile-time validation, but it also means both sides must stay synchronized as the data model changes. Charian removes the shared schema entirely, trading those benefits for structural independence between systems that don't evolve together.

| | Protobuf / Avro / JSON Schema | Charian |
|---|---|---|
| **Contract** | Schema defined up front; both sides must agree on it | No schema; each side reads/writes by position, independently |
| **Schema drift** | A field added, removed, or retyped on one side can break the other unless versioning rules are followed carefully | Structure changes on one side don't break the other — the receiver only reads what it expects and handles the rest itself |
| **Setup** | Requires a schema file (`.proto`, `.avsc`, etc.) and a code-generation step | No schema file, no codegen — just two source files added to your project |
| **Tooling footprint** | Compiler/codegen toolchain, schema registry (for Avro), versioning discipline | None — ~800 lines, no 3rd-party dependency |
| **Validation** | Strong compile-time type safety and schema enforcement, made possible by the shared contract | None — validation and error handling are the client's responsibility |
| **Payload size** | Compact binary for Protobuf/Avro; the shared schema lets field names and type tags be stripped from the wire | Delimited text string, larger than Protobuf/Avro's binary but more compact than JSON |
| **Best fit** | Stable, high-throughput systems within a single team's control (internal microservices, high-volume event streams) | Systems integration across teams, vendors, or legacy platforms where data models are inconsistent, evolving, or outside your control |

Charian is aimed at the specific pain point of **integration between systems you don't fully control** — connecting a legacy system to a modern one, exchanging data with a third-party vendor, or maintaining a pipeline where the data model on either end changes independently and without warning. In these situations, a shared schema becomes a liability: every change on one side risks a synchronized (and often coordinated, multi-team) update on the other, or the pipeline breaks.

Because of late-binding, Charian allows flexible handling of data's schema changes, even processing data of multiple schema versions concurrently and dynamically. This trades the schema's built-in validation for structural independence — a fair trade when the alternative is fragile, tightly-coupled pipelines between systems that were never designed to evolve together.

### When to use Charian

In summary, choose **Charian** when:

* Your applications evolve independently.
* You integrate with third-party or legacy systems.
* Maintaining shared, or multiple versions of, schemas has become difficult.
* Cross-language compatibility matters.
* You prefer explicit serialization logic over generated code.

Choose **Protocol Buffers**, **Avro**, or similar technologies when:

* You own both ends of the communication.
* Maximum throughput is more important than flexibility.
* Compile-time schema validation is desirable.

# Getting Started

You can start using Charian in your project in one of two ways: **install it as a package** (recommended for most users), or **include the source files directly** (useful if you want source-level transparency, need to target a framework not covered by the package, or prefer to avoid an external dependency).

## Installation

### Option 1: Install via package manager

**C# (NuGet)**

```
dotnet add package Foldda.Charian
```

Or via the Package Manager Console:

```
Install-Package Foldda.Charian
```

Or add it directly to your `.csproj`:

```xml
<PackageReference Include="Foldda.Charian" Version="1.0.1" />
```

The package targets .NET 5.0, .NET Core 2.0, .NET Standard 2.0, and .NET Framework 4.6.1, so it should be compatible with most existing C# projects. View the package on [NuGet.org](https://www.nuget.org/packages/Foldda.Charian).

**Python / Java**

Python and Java packages are not yet published to PyPI or Maven Central. For now, use the source-inclusion method below for these languages.

### Option 2: Include the source files directly

Charian has no third-party dependencies, so integrating it is as simple as downloading two source files from this repo and adding them to your project. This approach simplifies your build process and gives you full transparency during debugging, when needed.

1. Download the source files for your language from this repo:
   - [C#](https://github.com/foldda/charian/blob/main/src/CSharp)
   - [Java](https://github.com/foldda/charian/blob/main/src/Java)
   - [Python](https://github.com/foldda/charian/blob/main/src/Python)
2. Add the files to your project.
3. Reference them as you would any other local source file — no additional configuration is required.

> **Tip:** You can use the test cases in this repo as examples of how to use Charian.

## Example: serializing a simple data object

This example shows how to serialize a `Person` class by implementing the `IRda` interface: the (data model) "packing" logic is `ToRda()` and the "unpacking" logic is in `FromRda()`. These methods hide the class's internal data model, letting a client serialize and deserialize with simple calls. The optional `SaveToFile()` and `ReadFromFile()` methods show how the serialized data can be exchanged.

```csharp
public class Person : IRda
{
    public string FirstName = "John";
    public string LastName = "Smith";

    //specify an allocated position in the RDA for storing each of the object's properties
    public enum RDA_INDEX : int
    {
        FIRST_NAME = 0,
        LAST_NAME = 1
    }

    //store the class's properties into an Rda object
    public virtual Rda ToRda()
    {
        var rda = new Rda();  //create an RDA container

        //stores each of the properties' value
        rda[(int)RDA_INDEX.FIRST_NAME].ScalarValue = this.FirstName;
        rda[(int)RDA_INDEX.LAST_NAME].ScalarValue = this.LastName;
        return rda;
    }

    //restore the class's properties from an RDA
    public virtual IRda FromRda(Rda rda)
    {
        this.FirstName = rda[(int)RDA_INDEX.FIRST_NAME].ScalarValue;
        this.LastName = rda[(int)RDA_INDEX.LAST_NAME].ScalarValue;
        return this;
    }

    //client calls this method to serialize and save this Person object to a file
    public void SaveToFile(string filePath)
    {
        string encodedRdaString = this.ToRda().ToString(); //serialize
        File.WriteAllText(filePath, encodedRdaString);
    }

    //client calls this method to restore a Person object from an RDA string read from a file
    public static Person ReadFromFile(string filePath)
    {
        string encodedRdaString = File.ReadAllText(filePath);
        Rda rda = Rda.Parse(encodedRdaString);
        Person person = new Person();  //an initial "empty" person object
        person.FromRda(rda);  //restores the Person's properties here.
        return person;
    }
}
```

**Takeaway**: The Person class (at application layer) implements two methods: the `ToRda()` method is where the object's essential properties and state are stored into an Rda container object, which represents a RDA string at the back; the `FromRda()` restores that essential state back to a Person object during deserialization. In between, the container is converted to a string for easy transport by a simple, conventional "courier" process, e.g. a file transfer. Other serialization systems typically decompose and serialize an object in its entirety, which adds overhead that isn't always necessary.

# Core Concepts

Unlike schema-based formats, in Charian the transport layer does not impose restrictions on the structure of the data being carried, allowing loosely-coupled integration by moving data validation to the application layer.

## The late-binding analogy

Imagine moving house.

Furniture is disassembled, packed into boxes, transported, and then reassembled at the destination. The freight company never needs to know what is inside each box.

Data exchange can work the same way.

Applications disassemble complex objects into generic RDA containers for transport and the receiving application reconstructs the objects after delivery.

The Charian API's Rda class and its IRda interface are specifically designed to allow these operations.

## Class Rda - an RDA encoder/parser

The Rda class is modeled as a "container" object for storing data. It has a multidimensional space where each storage location in the space is uniquely addressed by an integer array index, and a client uses Getter/Setter methods to access a data item at a given address. 

an Rda container supports storing only two "data types" — a data item can be either a string or another Rda (container) object. Charian assumes all primitive data, like an integer or a date, can be converted to a string, and all composite data, like a class or an array, can be stored as an Rda object by recursively decomposing it into less complex structures or primitive data items.

The Rda class also implements methods that convert itself to and from an [RDA string](https://github.com/foldda/rda), so it can be used as a generic RDA parser/encoder.

For the full method signatures and a worked example of encoding and decoding an RDA string, see **[API.md](API.md#class-rda---an-rda-encoderparser)**.

## Interface IRda - app-layer schema resolution

The IRda interface defines two methods: `ToRda()`, where a data object "packs" its properties and state into an Rda container, and `FromRda(Rda rda)`, where it "unpacks and restores" its properties and state from values stored in an Rda container. We've already seen the `Person` class implement these two methods above, in [Example: serializing a simple data object](#example-serializing-a-simple-data-object).

For an extended example showing how a more complex object with nested classes is packed and unpacked, and how to handle unexpected or evolving data during unpacking, see **[API.md](API.md#interface-irda---app-layer-schema-resolution)**.

# Use Cases

**Maintain compatibility.** As illustrated in the examples in [API.md](API.md), the `ComplexPerson` object extends the `Person` object while remaining backward compatible. This means if you have a connected network where some programs work with the `Person` object, and other programs have evolved to use the `ComplexPerson` object, these programs will remain compatible when communicating with each other on the network.

**Cross-language data exchange.** Because the schemaless RDA string is language- and system-neutral, it can be used as a data container for flexibly transferring data cross-language and cross-platform. Connected programs can flexibly deposit and consume data items stored in an RDA container without being constrained by a fixed data model, and can flexibly handle data conversions and any associated exceptions in their designated packing and unpacking operations.

For example, an RDA container packed by a Java program contains the properties of a Java `Person`, and these properties can be unpacked in a Python program and used to construct, say, a Python `User` object, which may or may not have exactly the same properties as the Java `Person` object. If anything unexpected happens, such as an item being missing or a data conversion failing, the Python program can add exception handling to its unpacking process — for example, sending out an alert or substituting the missing item with a default value.

**Maintaining rich and diverse data sets in parallel.** Take advantage of RDA's unrestricted and recursive feature. Each Rda data item stored in an Rda container is itself an isolated container, so multiple datasets, or different versions of the same dataset, can be stored or sent in one container "side-by-side," with the receiver intelligently testing and picking the correct version to use.

# License & Commercial Use

Charian is open‑source software released under the
**GNU General Public License v3.0 (GPL‑3.0)**.

This means you are free to use, modify, and redistribute Charian
under the terms of the GPL.

## Commercial licensing

If you want to use Charian in a **proprietary or closed‑source product**,
or distribute it without the requirements of the GPL, a **commercial license** is available.

Commercial licensing offers:
- Permission for closed‑source usage
- Legal clarity for enterprise environments
- Optional support and long‑term maintenance agreements

For commercial licensing inquiries, please contact: **contact@foldda.com**

Open‑source users are welcome and encouraged to use Charian under GPL‑3.0.

# Get Involved

This project needs contributions in the following areas:

## Write parser/encoder in more languages

RDA has a very simple encoding rule for programmers fluent in a given language. Once a new language is supported, it would enable exponential growth in the number of applications, cross-language and cross-platform, that can interact with each other and exchange data.

For example, someone writing a C library that enables IoT devices to consume RDA data. An RDA codec would have a very small footprint, suitable for being embeded into IoT devices.

It would also be impactful if someone wrote a parser library for TypeScript that would enable rendering RDA-encoded data in TS web controls.

## Write test cases

Charian's API is relatively small, but more test cases would make it more rock-solid and would benefit anyone using the free library.

## Write documentation

Richer and better documentation would help the Charian project convey its novel concepts, such as late-binding, into good understanding and practical use cases for programmers and systems developers.



<!--- MARKDOWN LINKS & IMAGES
[# Template from](https://github.com/othneildrew/Best-README-Template/blob/master/README.md)
--->
<!--- https://www.markdownguide.org/basic-syntax/#reference-style-links --->
[contributors-shield]: https://img.shields.io/github/contributors/foldda/charian.svg?style=for-the-badge
[contributors-url]: https://github.com/foldda/charian/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/foldda/charian.svg?style=for-the-badge
[forks-url]: https://github.com/foldda/charian/network/members
[stars-shield]: https://img.shields.io/github/stars/foldda/charian.svg?style=for-the-badge
[stars-url]: https://github.com/foldda/charian/stargazers
[issues-shield]: https://img.shields.io/github/issues/foldda/charian.svg?style=for-the-badge
[issues-url]: https://github.com/foldda/charian/issues
[license-shield]: https://img.shields.io/github/license/foldda/charian.svg?style=for-the-badge
[license-url]: https://github.com/foldda/charian/blob/master/LICENSE.txt
[product-screenshot]: images/screenshot.png
