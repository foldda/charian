<!--- PROJECT SHIELDS --->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]


<div align="center">
<img src="img/Charian-logo-orange-text.png" width="250" align="center">
</div>

> *Charian is a general-purpose data encoding API that uses the new, schemaless RDA format[^1] in its encoding. Structured data encoded in an RDA string can be transported using generic, non-proprietary protocols and methods, and be exchanged freely between independent programs without using middleware.*

[^1]: RDA (Recursive Delimited Array) is a delimited text data encoding format. The encoding uses multiple delimiters, which can be dynamically defined and expanded, and provides an encoded storage space that is accessible as a multidimensional array.

<!--- TABLE OF CONTENTS --->
# Table of Contents
1. [About Charian](#about-charian)
    - [Inside the API](#inside-the-api)
    - [How does it work](#how-does-it-work)
2. [Getting Started](#getting-started)
3. [Examples](#examples)
    - [Transporting primitive data items in an RDA string](#transporting-primitive-data-items-in-an-rda-string)
    - [Serializing a simple composite data object](#serializing-a-simple-composite-data-object)
    - [Serializing a complex object with nested classes](#serializing-a-complex-object-with-nested-classes)
    - [Other uses](#other-uses)
4. [License, Etc.](#license-etc)
5. [Afterword: The Big Picture](#afterword-the-big-picture)

# About Charian
Charian (pron. /ka-ri-en/) is a data encoding API for "storing" any complex structured data in a text string. Encoding with Charian does not require pre-establishing a fixed data model (aka. a schema), which makes it simple and easy to use. 

Charian can be used for implementing -

- **Persistent data storage** - for conveniently storing structured data (as strings) in files or databases;
- **Distributed computing** - for, as a hack, passing any complex data structure as a "string parameter" in a remote call;
- **Systems integration** - for exchanging data between independent programs without using a dedicated middleware;
- **ETL solutions** - for transforming and transferring data of various data models through simple programming.

Charian can also be used for implementing object serialization in distributed object-oriented programs. Compared to the other object serialization systems and methods, Charien's schema-less, one-size-fits-all approach has many benefits including being -

- **Flexible**: it is designed to be generic and fit any data model, ideal for evolving and dynamic programs;
- **Minimal and compact**: the API is written with minimal code (around 800 lines of code), with no 3rd-party dependency;
- **Easy to use**: Charian is "one size fits all" - there is no settings or configurations;
- **Language and framework independent**: Charian-serialized objects can be exchanged cross-language and cross-platform[^2].

[^2]: Subject to RDA encoder and parser availability for the language and the platform.

## Inside the API

Charian is available in [C#](src/CSharp), [Python](src/Python), and [Java](src/Java) in this repo. C# syntax is used below to illustrate the content and the example usage of the API, which is also applicable to the other languages as they follow the same design and have near-identical structure and naming convention. 

**Class Rda**

The Rda class is modeled as a "container" object. Internally it provides a multidimensional storage space where a storage location in the space is addressed by an integer-array index. A client can use the following Getter/Setter methods for accessing a data item stored at a given address[^3]:

[^3]: The integer array address must contain only non-negative index values, and has a length limit of 40.
```
public void SetValue(string value, int[] address)
public string GetValue(int[] address)
public void SetRda(Rda rda, int[] address)
public Rda GetRda(int[] address)
```

Note only two "data types" are supported by an Rda container - a data item can only be either a string value or an Rda (container) object. Charian assumes all primitive data, like an integer or a date, can be converted to a string; and all composite data, like a class or an array, can be converted to an Rda object (through recursion as illustrated in Example #3 below).

An Rda container object is also "serializable". The Rda class implements the following methods that allows itself to be coverted to and from an encoded text string:

```
public string ToString()
public static Rda Parse(string rdaEncodedString)
```

The produced string is encoded in a special text format called RDA, which is a delimited data format similar to the more popular CSV format, but is enhanced to support encoding multidimensional data.

**Interface IRda**

The IRda interface has defined two methods:
```
Rda ToRda()
IRda FromRda(Rda rda)
```
A client object implements this interface to indicate that itself can be converted to and from an Rda object, which can be in turn converted to and from an RDA-encoded string.

## How does it work

Imagine you're moving house and have lots of household items to be moved to the new place, you would first package these items into boxes before the courier company come and pick them up, and once the boxes are delivered to the new place, you would unpack the boxes and retrieve the items and place them to their designated places. 

Exchanging data using Charian is easy to understand if we compare it to moving house, except in this case we are packing and moving data using an Rda container (in place of a 'box'). In such a process: a data-sending client would -
1) construct an Rda container object, 
2) use the Setter methods to “pack” its data items that require transfer into the container, and then 
3) use the ToString method to convert the container to an RDA string. 

Afterward, a "courier process" takes over transporting the string "data container", which can be saving the string to a file or to a database table, or sending the string to a network destination via a suitable network protocol. 

A data-receiving client, upon having received the RDA string, would -
4) use the Parse method to convert the string back to an Rda container, then 
5) use the Getter methods to "unpack" and consume the data items stored in the container. 

Storing and transporting arbitrary multiple data items using the Rda class is demonstrated in Example #1 below.

For object serialization, a class would implement the IRda interface, where in the ToRda method it would specify the data-packing logic that "packs the essential properties of the class it self and places inside an Rda container"; and in the FromRda method it'd specify the logic of unpacking a received Rda container, and using the received values to restore the class properties to make the object to the intended state. 

Implementation of object serialization through using the IRda interface is demonstrated in Example #2 and Example #3 below. 

# Getting Started
It is recommended to use Charian by including the provided source code in your project - that is, use the Rda class and the IRda interface as a native part of your project. The benefit is less "package dependency" meaning simpler build process and product distribution.

# Examples
Here are some practical examples illustrating the uses of the API.

## Transporting primitive data items in an RDA string
```c#
    using Charian;

    class RdaDemo1
    {
        public void Main(string[] args)
        {
            string PATH = "C:\\Temp\\file1.txt";

             //as sender ...
            SendSomeData(PATH);

            //as receiver ...
            ReceiveSomeData(PATH);
        }

        void SendSomeData(string filePath)
        {

            Rda rda1 = new Rda();    //create a new Rda container object

            //data-packing involves item placement and type-conversion
            rda1.SetValue(0, "A string");  //storing a string value at index = 0
            rda1.SetValue(1, 2.5.ToString());
            rda1.SetValue(2, DateTime.Now.ToString());

            string encodedRdaString = rda1.ToString();     //serialize

            File.WriteAllText(filePath, encodedRdaString);
        }

        void ReceiveSomeData(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);

            Rda rda1 = Rda.Parse(encodedRdaString);    //restore the container object from the RDA string

            //"unpacking" the data items from the container
            string a = rda1.GetValue(0);  //storing value "One" at index = 0
            double b = double.Parse(rda1.GetValue(1));
            DateTime c = DateTime.Parse(rda1.GetValue(2));
        }
    }
```

**Takeaway**: The sender and the receiver are expected to know where (placement) and what (types) the data items are, and handle exception if the expectation is not met.

**Takeaway**: Rda container has no schema and does not enforce data validation. Primitive type data are stored as strings, the client is responsible for type conversion and the associated error handling.

## Serializing a simple composite data object

```C#
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

        //store the class' properties into an Rda object
        public virtual Rda ToRda()
        {
            var rda = new Rda();  //create an RDA container

            //stores each of the properties' value
            rda[(int)RDA_INDEX.FIRST_NAME].ScalarValue = this.FirstName;
            rda[(int)RDA_INDEX.LAST_NAME].ScalarValue = this.LastName;
            return rda;
        }

        //restore the class' properties from an RDA
        public virtual IRda FromRda(Rda rda)
        {
            this.FirstName = rda[(int)RDA_INDEX.FIRST_NAME].ScalarValue;
            this.LastName = rda[(int)RDA_INDEX.LAST_NAME].ScalarValue;
            return this;
        }

        //serialize and save this Person object to a file
        public void SaveToFile(string filePath)
        {
            string encodedRdaString = this.ToRda().ToString(); //serialize
            File.WriteAllText(filePath, encodedRdaString);
        }

        //restoring a Person object from an RDA string that is stored in a file
        public static Person ReadFromFile(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);
            Rda rda = Rda.Parse(encodedRdaString);
            Person person = new Person();
            person.FromRda(rda);
            return person;
        }
    }

```

**Takeaway**: The IRda interface's ToRda() and FromRda() methods are the “standard” places of a class for packing and unpacking data.

## Serializing a complex object with nested classes

```C#
    class Address : IRda
    {
        public enum RDA_INDEX : int { LINES = 0, ZIP = 1 }

        public string AddressLines = "Line 1\nLine 2\nLine 3";
        public string ZIP = "NY 21540";

        //"packing" properties into an Rda container
        public Rda ToRda()
        {
            var rda = new Rda();  //create an RDA container
            // properties
            rda[(int)RDA_INDEX.LINES].ScalarValue = this.AddressLines;
            rda[(int)RDA_INDEX.ZIP].ScalarValue = this.ZIP;
            return rda;
        }

        //"unpacking" and restoring properties from an Rda container
         public IRda FromRda(Rda rda)
        {
            this.AddressLines = rda[(int)RDA_INDEX.LINES].ScalarValue;
            this.ZIP = rda[(int)RDA_INDEX.ZIP].ScalarValue;
            return this;
        }
    }

    class ComplexPerson : Person
    {
        public new enum RDA_INDEX : int
        {
            FIRST_NAME = 0,
            LAST_NAME = 1,
            RES_ADDRESS = 2,   //location of the "residential address" stored in the container
            POST_ADDRESS = 3
        }

        //extended properties of ComplexPerson
        public Address ResidentialAddress = new Address() { AddressLines = "1, 2, 3", ZIP = "12345" };
        public Address PostalAddress = new Address() { AddressLines = "a, b, c", ZIP = "23456" };

        public override Rda ToRda()
        {
            Rda personRda = base.ToRda();

            //storing an extra "address" property, as a child-Rda, inside the person's Rda container
            personRda[(int)RDA_INDEX.RES_ADDRESS] = this.ResidentialAddress.ToRda();

            //now person Rda is 2-dimensional
            //Console.Println(personRda[2][1].ScalarValue);   //prints ResidentialAddress.ZIP

            //
            //eg here we store a further “postal address” Rda to the person Rda, and so on ...
            personRda[(int)RDA_INDEX.POST_ADDRESS] = this.PostalAddress.ToRda();

            return personRda;
        }

        public override IRda FromRda(Rda rda)
        {
            //restore the base Person properties
            base.FromRda(rda);  //restores FirstName and LastName

            //de-serializing composite properties (embedded objects) recursively
            this.ResidentialAddress.FromRda(rda[(int)RDA_INDEX.RES_ADDRESS]);
            this.PostalAddress.FromRda(rda[(int)RDA_INDEX.POST_ADDRESS]);
            return this;
        }

        //retrieve a stored ComplexPerson object from a file
        public new static ComplexPerson ReadFromFile(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);
            Rda rda = Rda.Parse(encodedRdaString);
            ComplexPerson person = new ComplexPerson();
            person.FromRda(rda);
            return person;
        }
    }

```

**Takeaway**: Rda container has ‘unlimited space’ and supports recursion, which can accommodate the complexity of an evolving object to be extended indefinitely.

## Other uses

**Maintain compatibility** As illustrated in the above examples, the ComplexPerson object extends the Person object while remaining backward compatible. This means you can have a connected network where some programs work with the Person object, and some other programs have evolved and use the ComplexPerson object, and these programs will remain compatible in communicating with each other in the network.

**Cross-language data exchange** Because the schemaless RDA string is language and system neutral, it can be used as a data container for flexibly transferring data cross-language and cross-platform. Without having a fixed data model, the connected programs can flexibly place and use data items stored in an RDA container during the data packing and unpacking, and to be able to flexibly handle the data conversions and any associated exceptions, in these processes.

For example, an RDA container packed by a Java program contains the properties of a Java 'Person', and these properties can be unpacked in a Python program and be used for constructing say a Python 'User' object, which may or may not have exactly the same properties as the Java Person object. If anything unexpected happens, such as an item is missing, or a data conversion has failed, the Python program can put exception handling in its 'unpacking' process e.g. sending out an alert or substituting the missing item with a default value.

**Maintaining rich and diverse data sets in parallel** Take advantage of RDA's unrestricted and recursive feature. Each Rda data item stored in a Rda container is itself an isolated container. So multiple datasets or different versions of the same dataset can be stored or sent in one container "side-by-side", and a receiver can intelligently test and pick the correct version to use.

# License, Etc.

* Charian is licensed under GPL -v3

* You may contact Charian's developer by email - contact@foldda.com

**Links**

* [Project Wiki] (coming soon)

* [FAQ] (coming soon)

# Afterword: The Big Picture

Independent programs, such as a browser-hosted app and a Web server, or an IoT device and a control console, often need to communicate with each other in a collaborative distributed solution. Because these programs are often developed by different parties and executed on separate computer environments, exchanging data between them is normally complicated and requires extra effort. 

## The problem - schema-bound data exchange

 The conventional approach to cross-program data exchange typically involves establishing an ad hoc, dedicated connection between the communicating parties, and use an ‘agreed’ data model (i.e. bound by a schema) for the data communication.

<div align='center'>
<img src='img/Pre-Charian-data-transport.png' width='550' align='center'>
</div>

Developing a dedicated connection for every application that has a different data model is not cost-effective because of the duplicated effort and the high costs. The cost of managing data exchange over schema-based connections can also be significant because the connected programs become “tightly coupled” by these connections. If one of the programs has evolved and the data model needs to be changed, a developed solution often requires significant modification or using a dedicated middleware system to mediate the data model transformation.

In an analogy, building ad-hoc schema-bound data exchange solutions is like sending parcels to people without using the Post Office, but doing everything yourself - meaning you’ll have to make ad-hoc transport and delivery arrangements on each occasion, limited by the resources you have.

<div align='center'>
<img src='img/Pre-Post-office-system.png' width='470' align='center'>
</div>

## The solution and the challenge - universal data transport

As with the Post Office, the reason it’s convenient and can save money is that it provides a standard parcel processing service that covers the different requirements of all of its clients and it uses a shared common logistics and freight system helps cut down the cost.

<div align='center'>
<img src='img/Post-office-system.png' width='550' align='center'>
</div>

Universal Data Transport, or UDT, is a proposed “post-office-like” data transport service aiming for the benefit from the same approach - that is, by creating and sharing a common, generic data transport service to be shared by all programs that require exchanging data, we can avoid building ad-hoc dedicated data-exchange connections to make the operation simpler and also save money.

<div align='center'>
<img src='img/Charian-data-transport.png' width='550'>
</div>

One “difficulty” the Post Office had was to provide a single consistent service that can cater for the different requirements from all it clients, and the answer is the use of standardized packaging. Packaging loose items in a box also allow modularized, more effective transportation that can be carried out by a general courier company. Thus a key design to UDT’s design is to have a data container for packaging (and regulating) various data items (e.g. properties of a data object), so irregular data can be handled uniformly using general data transport protocols and methods.

Also, UDT is most suitable to be implemented as a messaging technology, where the data container is an encoded text message. That’s because through data encoding, a text message can be used as a container to store data, and text (i.e. ‘string’) is one of the most supported data types by major computer systems and programming languages. Data stored in a “string container” can be readily processed using generic tools and protocols, without the need for any custom proprietary treatment. For example, it can be saved to a file system or a database, or be transferred via common network protocols, such as HTTP/RPC, TCP/IP, and FTP. Thus the challenge to implementing UDT is to have a text encoding format that supports encoding any data into a string.

Unfortunately, popular data formats, such as XML and JSON and CSV, are not suitable for encoding the UDT container. That’s because each data instance in one of these formats assumes a certain data model (by structure and type), meaning a container encoded in these formats won't be “generic and universal” that we want for accomandating _any data_. So our quest for a suitable encoding has led to the development of RDA - a new schemaless data format.

## The invention - RDA encoding

RDA stands for "Recursive Delimited Array". It is a delimited encoding format similar to CSV where encoded data elements are separated by delimiter chars. Below is an example of a RDA format string that contains data elements of a 2D (3x3) table, using two delimiter chars for separating the data elements.

```
|,\|A,B,C|a,b,c|1,2,3
```
The beginning of an RDA string is a substring section known as the "header" which contains the definition of the RDA string’s encoding chars including one or many delimiter chars (“delimiters”) and one escape char. In the above example, the header is the substring "|,\\|", and the delimiters are the first two chars '|' and ','. The third char ‘\\’ is the ‘escape’ char, and the last char ‘|’ is the ‘end-of-section’ marker which marks the end of the header section. The header structure allows dynamically defining multiple delimiters for encoding more complex, multidimensional data, which also means every RDA string can use a different set of chars for its encoding[^4].

[^4]: According to the RDA encoding rule, the header section starts from the first letter of the string and finishes at the first repeat of the string's starting letter which is called the ‘end-of-section’ marker. Any char can be used as a delimiter or the escape char, the only requirement is they must be different to each other in an RDA string header. By placing the encoding chars in the header at the front of an RDA string allows a parser to be automatically configured when it starts parsing the string.

Following the header, the remaining RDA string is the 'payload' section that contains the encoded data. The RDA payload section provides a 'virtual' storage space of a multi-dimensional array where stored data elements are delimited using the delimiters defined in the header, and each data element is accessible via an index address comprised of an array of 0-based integers. In the above example, the top dimension of the array is delimited by delimiter '|' and the second dimension is delimited by delimiter ',', and in this 2D array the data element at the location [0,1] has a string value "B".

**Compared to XML and JSON**

> The space from XML/JSON is like a wallet, where it has places specifically defined for holding cards, notes, and coins; whilst the space from RDA is like an enormous shelf, where you can place anything at anywhere in the unlimited space that is provided.

RDA is specifically designed to avoid targetting a certain data model and having to define and maintain a schema. Such design is reflected by the structure of RDA's encoded storage space, the way of addressing a location in the space, and the supported data types[^5].  

[^5]: First, RDA has multi-dimensional array storage space that is dynamically expandable, that is, the size of each dimension and the number of dimensions can be increased or decreased as required, like an elastic bag. This is in contrast to the ‘fixed’ hierarchical space provided by schema-based encodings, like XML or JSON, which is restricted by a predefined data mode, like a rigid, fix-shaped box. Second, RDA uses integer-based indexes for addressing the storage locations in its multi-dimensional array storage space, which means, and combination of non-negative integers is a valid address referring to a valid storage location in the space. This is in contrast to XML and JSON, the address for accessing a storage location is a ‘path’ that has to be ‘validated’ against a pre-defined schema. Third, RDA assumes all data (of any type) can be 'expressed as a string' and a value stored at a location (referred as “a data's value expression”) can only be a string; whilst XML and JSON attempt to define and include every possible data types and a data values stored at a location must conform with what has been defined in the schema.

Inheritively from RDA's schemaless design, the encoding is simpler, more space-efficient, and configuration-free compared to XML and JSON. But perhaps the most interesting and unique property of RDA is the **recursiveness** of the storage space: the multi-dimensional array structure is homogenous, and there can be only one 'unified' data type, so a sub-dimension in the space is itself a multi-dimensional space that has the same structure as its containing (parent dimension) space, and can be used in the same way. The recursiveness of the multi-dimensional space allows an arbitrarily complex data structure to be (recursively) decomposed into sub-components and be stored int the dimensions and their sub-dimensions from the provided space.

## The product - Charian

The **Rda class** and the **IRda interface** from the API are implemented with methods and properties for UDT-oriented operations[^6], and from a client's perspective, it only needs to deal with a simple, easy-to-access storage space, which is also serializable, through the API. A Charian client does not have to know or to deal with the RDA encoding at all.

[^6]: The description and examples are given in C# syntax, but the illustrated methods can be easily translated to the Python and Java implementations which are also provided in this repo, and have identical functions.

Thus, Charian is not just another data encoder or object serializer. By making cross-program data exchange much simpler and more flexible, programs and devices can connect to each other and work collaboratively, and play-part in an eco-system of distributed solutions that leverage the technology.

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

<END>

