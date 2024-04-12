<!--- PROJECT SHIELDS --->

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]


<div align="center">
<img src="img/Charian-logo-orange-text.png" width="250" align="center">

**_"For simple and efficient cross-program data exchange."_**
</div>

<!--- TABLE OF CONTENTS --->
# Table of Contents
1. [Introduction](#introduction)
    - [Inside the API](#inside-the-api)
    - [How does it work](#how-does-it-work)
2. [Getting Started](#getting-started)
    - [How-to: Transporting primitive data items in an RDA string](#how-to-transporting-primitive-data-items-in-an-rda-string)
    - [How-to: Serializing a simple composite data object](#how-to-serializing-a-simple-composite-data-object)
    - [How-to: Serializing a complex object with nested classes](#how-to-serializing-a-complex-object-with-nested-classes)
    - [How-to: Exception handling](#how-to-exception-handling)
3. [Other Applicable Uses](#other-applicable-uses)
4. [License, Etc.](#license-etc)
5. [The Big Picture](#the-big-picture)

# Introduction

Charian (pron. /ka-ri-en/) is an API for encoding structured data to a text string - a process known as serialization. Charian serialization can be used for implementing -

- **Persistent data storage** - for storing arbitrarily structured data in files or databases as a string;
- **Distributed computing** - for passing dynamic programming object or data structure in RPC calls as a "string parameter";
- **Data communication** - for sending complex structured data in a serialized form over the network;
- **ETL solutions** - for transferring and transforming data of various data models through simple programming.

Charian serialization uses a new, schemaless format called RDA[^1] in its encoding, meaning the process does not require pre-establishing a data model or schema. Compared to conventional approaches, Charian's one-size-fits-all approach has many advantages such as being -

[^1]: RDA (Recursive Delimited Array) is a delimited text data encoding format that uses multiple delimiters that can be dynamically defined and expanded. An RDA-encoded string provides an encoded storage space accessible as a multidimensional array.

- **Simple and universal**: It is ideal for data exchange between programs with evolving and dynamic data models;
- **Minimalism and lightweight**: The API is implemented with a minimal code base (of ~800 lines), with no 3rd-party dependency;
- **Easy to use**: Charian is "one size fits all" - it has no settings or configuration to set;
- **Language and system independent**: Charian-serialized objects can be exchanged cross-language and cross-platform[^2].

[^2]: Subject to RDA encoder and parser availability for the language and the platform.

In contrast to the traditional approach of building data-model-dependent custom pipelines, Charian allows flexible cross-program data exchange using only generic data exchange methods and protocols and paves the way for [a new data communication ecosystem that simplifies and unifies data communication between collaborative programs](#the-big-picture). 

Charian API is implemented in [C#](src/CSharp), [Python](src/Python), and [Java](src/Java) in this repo. These implementations are clones of each other, meaning they share a near-identical programming design/structure/naming convention. Below we'll use the C# API as an example to explain Charian's concept and usage pattern. 

## Inside the API

The C# API contains only two defined types: **class Rda** and **interface IRda**. 

**Class Rda**

The Rda class is modeled as a "container" object for storing data. It has a multidimensional space where each storage location in the space is uniquely addressed by an integer array index[^3]. A client uses the following Getter/Setter methods for accessing a data item in the space for a given address:

[^3]: The index has a dimension limit of 40 in the current implementation, and the index value for each dimension must be a non-negative integer.
```csharp
public void SetValue(string value, int[] address)     /* save a string value at the addressed location */
public string GetValue(int[] address)        /* retrieve a string value from the addressed location */
public void SetRda(Rda rda, int[] address)      /* save an Rda object at the addressed location */
public Rda GetRda(int[] address)      /* retrieve an Rda object from the addressed location */
```

Note through the API only two "data types" are supported by the Rda container - a data item can be either a string or an Rda (container) object. Charian assumes all primitive data, like an integer or a date, can be converted to a string; and all composite data, like a class or an array, can be stored as an Rda object (by recursively decomposing a complex data into less complex structures or to primitive data items, as illustrated in [this example below](#how-to-serializing-a-complex-object-with-nested-classes)).

An Rda object is also "serializable". The Rda class implements the following methods that allow itself to be converted to and from a text string that is encoded in [the RDA format](#the-invention---rda-encoding):

```csharp
public string ToString()      /* convert this Rda container object to an RDA string */
public static Rda Parse(string rdaEncodedString)   /* decode the RDA string and return an Rda container object  */
```

_**Note:** From the API, class Rda offers additional methods and properties to the above-described (core) methods. Please refer to the class test cases from this repo for usage examples all the implemented features._

**Interface IRda**

The IRda interface contains the definition of two methods:

```csharp
Rda ToRda()   /* returns properties and state of this object in an Rda container object */
IRda FromRda(Rda rda) /* restores properties and state of this object from values in an Rda container */
```

A client object implements the IRda interface to indicate that itself can be converted to and from an Rda object, which in turn can be converted to an RDA-encoded string - in other words, a class implementing the IRda interface to make itself "serializable".

## How does it work

Imagine you're moving house: you would first pack household items into boxes, disassemble them if required, and then transport the boxes using a courier company. Once the boxes are delivered to the new place, you would unpack the boxes, reassemble the items, and place them to their designated places.

Serializing data using Charian is similar to the moving house exercise, except we are packing and moving data rather than household items. In Charian serialization, a data-sending program would -

1) create an Rda object and use it as a container,
2) use the **Setter** methods to “pack” data items that require transfer into the container, and then
3) use the **ToString** method to convert the container to an RDA string.

Then, a data "courier" process takes over transporting the data container in the form of a string. Such a process can be saving the string to a file or a database table, or sending it to a network destination via a network protocol.

In the deserializing process, a data-receiving program, upon having received the RDA string, would -

1) use the **Parse** method to convert the string back to an Rda container, and
2) use the **Getter** methods to "unpack" and consume the data items from the container.

In the above process, the Rda class plays the important role of being a 'container box' for packing and unpacking data items; because it can be turned into a string, it effectively serializes the data it contains.

The IRda interface is a signature indicating an object implements serialization and deserialization in "the Charian way": in the **ToRda** method it would specify the data-packing logic that stores the class' properties and the state at designated places inside an Rda container, and in the **FromRda** method it'd specify the logic of unpacking a received Rda container and restoring the object's properties and state using the received data values.

The "how-to" examples in the next section demonstrate these concepts and operations.

# Getting Started
Using Charian is very simple because it has no third-party dependency so there is nothing to set up. You can simply include the Charian source files in your project and use Charian's class and interface alongside yours[^4]. Source-code level integration can simplify your build process and give transparency during debugging (if required).

[^4]: Tip: you can use the test cases provided in this repo as examples of using Charian.

## How-to: Transporting primitive data items in an RDA string
This example shows grouping a collection of discrete data items and saving them to a file as an RDA-encoded string. The program utilizes the provided "unrestricted" storage to store arbitrarily structured data (in this case, the structure is sequential), without having to pre-define a schema. Note through the API, the underlying RDA-encoding is transparent to the client.

```csharp
    using Charian;

    class RdaDemo1
    {
        public void Main(string[] args)
        {
            //a file is used as the physical media/channel for the data transport
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
            rda1.SetValue(1, 2.5.ToString());  //storing a decimal value
            rda1.SetValue(2, DateTime.Now.ToString());  //storing a date value

            string encodedRdaString = rda1.ToString();     //serialize the data container

            File.WriteAllText(filePath, encodedRdaString);  //output to a physical media
        }

        void ReceiveSomeData(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);  //input from a physical media

            Rda rda1 = Rda.Parse(encodedRdaString);    //restore the container object from the RDA string

            //"unpacking" the data items from the container
            string a = rda1.GetValue(0);  //retrieve the stored value ("A string") from location index = 0
            double b = double.Parse(rda1.GetValue(1));
            DateTime c = DateTime.Parse(rda1.GetValue(2));
        }
    }
```

**Takeaway**: Primitive type data are stored as strings. The sender and the receiver are expected to know where (placement) and what (types) the data items are in a container. Rda container has no schema and does not enforce data validation. The clients are responsible for type conversion and data validation, and [handle exceptions if any unexpected data is encountered](#how-to-exception-handling).

## How-to: Serializing a simple composite data object
This code example illustrates Charian object serialization by implementing the IRda interface. It includes implementing the logic of "packing" properties in the ToRda() method for serialization, and the logic of "unpacking" data in the FromRda() method for de-serialization.
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

        //store the class' properties into an Rda object
        public virtual Rda ToRda()
        {
            var rda = new Rda();  //create an RDA container

            //stores each of the properties value
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
            Person person = new Person();  //an initial "empty" person object
            person.FromRda(rda);  //restores the Person's properties here.
            return person;
        }
    }

```

**Takeaway**: The IRda interface's ToRda() method is the place for a sender packing its "essential" properties and state data during serialization, and the FromRda() method is the place for a receiver unpacking a container and restores the "essential" properties and state data that "deserialize" the object. In between, the container is converted to a string for easy transportation by a 'courier' process. Note conventional serialization systems would typically attempt to decompose and serialize everything of a targeted object, which incurs higher overheads and may not always be necessary.

## How-to: Serializing a complex object with nested classes
Because you can store an Rda object inside another Rda object, it theoretically allows an arbitrarily complex object to be stored inside an Rda container, through recurrsive decomposition. The following example extends from the last example, and shows how a ComplexPerson object with two Address properties (which are also serializable) is packed into an Rda container.
```csharp
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

            //.. here we store a further “postal address” Rda to the person Rda, and so on ...
            personRda[(int)RDA_INDEX.POST_ADDRESS] = this.PostalAddress.ToRda();

            return personRda;
        }

        public override IRda FromRda(Rda rda)
        {
            //restore the base 'Person' object
            base.FromRda(rda);  //restores the FirstName and LastName properties

            //de-serialize and restore the address properties by invoking Address.FromRda()
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

## How-to: Exception handling
The following code expands from the last example and illustrates certain techniques that can be applied during "unpacking" and if the received data is unexpected.
```csharp

    class ComplexPerson : Person
    {
        //.....

        public override IRda FromRda(Rda rda)
        {
            try
            {
                //...
   
                //enforce mandatory residential address
                if(string.IsNullOrEmpty(rda[(int)RDA_INDEX.RES_ADDRESS]))
                {
                    throw new Exception("Missing mandatory residential address.");
                }
                else
                {
                    this.ResidentialAddress.FromRda(rda[(int)RDA_INDEX.RES_ADDRESS]);
                }

                //if the postal address is missing in the container, default to use the residential address
                if(string.IsNullOrEmpty(rda[(int)RDA_INDEX.POST_ADDRESS]))
                {
                    this.ResidentialAddress.FromRda(rda[(int)RDA_INDEX.RES_ADDRESS]);
                }
                else
                {
                    this.PostalAddress.FromRda(rda[(int)RDA_INDEX.POST_ADDRESS]);
                }
   
                //...
            }
            catch
            {
                /*
                    Anything that handles the error situation, eg -
                    1) setting a default value
                    2) escalating the error (i.e. re-throw)
                    3) returning the data back to the sender, and/or requesting re-send
                */
            }
        }
    }

```

**Takeaway**: You can implement flexible and sophisticated error handling when "unpacking" the data container.

# Other Applicable Uses

**Maintain compatibility** As illustrated in the above examples, the ComplexPerson object extends the Person object while remaining backward compatible. This means if you have a connected network where some programs work with the Person object, and some other programs have evolved and become using the ComplexPerson object, these programs will remain compatible in communicating with each other in the network.

**Cross-language data exchange** Because the schemaless RDA string is language and system-neutral, it can be used as a data container for flexibly transferring data cross-language and cross-platform. The connected programs can flexibly deposit and consume data items stored in an RDA container without being constrained by a fixed data model, and be able to flexibly handle the data conversions and any associated exceptions, in the designated data-packing and unpacking operations.

For example, an RDA container packed by a Java program contains the properties of a Java 'Person', and these properties can be unpacked in a Python program and be used for constructing say a Python 'User' object, which may or may not have exactly the same properties as the Java Person object. If anything unexpected happens, such as an item is missing, or a data conversion has failed, the Python program can put exception handling in its 'unpacking' process e.g. sending out an alert or substituting the missing item with a default value.

**Maintaining rich and diverse data sets in parallel** Take advantage of RDA's unrestricted and recursive feature. Each Rda data item stored in a Rda container is itself an isolated container. So multiple datasets or different versions of the same dataset can be stored or sent in one container "side-by-side", and a receiver can intelligently test and pick the correct version to use.

# License, Etc.

* Charian is licensed under GPL -v3

* You may contact Charian's developer by email - contact@foldda.com

**Links**

* [Project Wiki] (coming soon)

* [FAQ] (coming soon)

# The Big Picture

Why do we need Charian and RDA while there are already many XML/JSON-based data serialization and transport solutions? 

## The problem of schema-based data exchange

Independent programs, such as a browser-hosted app and a Web server, or an IoT device and a control console, often need to communicate with each other to form a collaborative distributed solution. Exchanging data in such cases is normally complicated and requires extra effort because of the implied diversity and uncertainty - the programs can have a different business and data model, be written in different languages, executed in separate computer environments, and can be developed and maintained by different parties. The conventional approach for cross-program data exchange typically involves building a dedicated pipeline connecting the communicating parties and having an 'agreed' format (i.e. a schema) for the data exchange.

<div align='center'>
<img src='img/Pre-Charian-data-transport.png' width='550' align='center'>
</div>

Developing a dedicated connection for every application with a different data model is likely time-consuming and costly. The ongoing cost of managing data exchange over schema-based connections can also be significant because the connected programs become “tightly coupled” by these connections, if one of the programs has evolved and the data model needs to be changed, a developed solution often requires significant modification or using a dedicated middleware system to mediate the data model transformation.

In an analogy, building ad-hoc schema-bound data exchange solutions is like sending parcels to people without using the Post Office, but doing everything yourself - meaning you’ll have to make ad-hoc transport and delivery arrangements on each occasion, limited by the resources you have.

<div align='center'>
<img src='img/Pre-Post-office-system.png' width='470' align='center'>
</div>

## The solution and the challenge - Unified Data Exchange

As we know, using the Post Office is convenient and cost-effective for posting goods of different shapes and sizes, because the standard parcel processing can meet the client's wide range of requirements, and the shared logistics and freight system helps cut down the cost.

<div align='center'>
<img src='img/Post-office-system.png' width='550' align='center'>
</div>

Unified Data Exchange, or UDX, is an envisioned data exchange service that provides the benefits of being convenient and cost-effective using the same “post-office-like” approach - that is, by creating and sharing a common, generic data collection and delivery service to all programs that require exchanging data, rather than building ad-hoc dedicated data-exchange solutions.

<div align='center'>
<img src='img/Charian-data-transport.png' width='550'>
</div>

As mentioned, the Post Office's parcel-processing service must cater to the different parcel-posting requirements of all its clients, and this is achieved by using standardized packaging. Packing loose items in boxes simplifies parcel handling and allows modularized, more effective transportation by general courier companies. Similarly, a key in UDX's design is to use a generic data container for packaging (and regulating) various data items (e.g. properties of a data object), so irregular data can be handled uniformly using general data transport protocols and methods.

Messaging is most suitable for implementing the UDX container. Because string is a supported data type by most computer systems and programming languages, so encoding, decoding, and transporting data in a “string container” can be naturally carried out using generic tools and protocols. In other words, an UDX data container, as a text message, can be saved to a file system or a database, or be transferred via common network protocols, such as HTTP/RPC, TCP/IP, and FTP. 

Thus the challenge to implementing UDX is to have a text encoding format that supports encoding any data into a string. Unfortunately, popular data formats, such as XML, JSON, and CSV, are not suitable for encoding the UDX container. That’s because each data instance in one of these formats assumes a certain data model (by structure and type), meaning a container encoded in these formats won't be the “generic and universal” that we want for accommodating _any data_. So our quest for a suitable encoding has led to the development of RDA - a new schemaless data format.

## The invention - RDA encoding

RDA stands for "Recursive Delimited Array". It is a delimited encoding format similar to CSV where encoded data elements are separated by delimiter chars except, among other things, RDA allows dynamically defining multiple delimiters for encoding more complex, multidimensional data[^4].

[^4]: According to the RDA encoding rule, the header section starts from the first letter of the string and finishes at the first repeat of the string's starting letter which is called the ‘end-of-section’ marker. Any char can be used as a delimiter or the escape char, the only requirement is they must be different to each other in an RDA string header. By placing the encoding chars in the header at the front of an RDA string allows a parser to be automatically configured when it starts parsing the string. 

Here is an example of an RDA format string containing a 2D (3x3) data table, using two delimiter chars for separating the data elements -

```
|,\|A,B,C|a,b,c|1,2,3
```
The beginning of an RDA string is a substring section known as the "header" which contains the definition of the RDA string’s encoding chars including one or many delimiter chars (“delimiters”) and one escape char. In this example, the header is the substring "|,\\|", and the delimiters are the first two chars '|' and ','. The third char ‘\\’ is the ‘escape’ char, and the last char ‘|’ is the ‘end-of-section’ marker which marks the end of the header section. 

Following the header, the remaining RDA string is the 'payload' section that contains the encoded data. The RDA payload section provides a 'virtual' storage space of a multi-dimensional array where stored data elements are delimited using the delimiters defined in the header, and each data element is accessible via an index address comprised of an array of 0-based integers. In the above example, the top dimension of the array is ored delimited by delimiter '|' and the second dimension is delimited by delimiter ',', and the data element stored in this 2D array at the indexed location [0,1] is the string value "B".

**Compared to XML and JSON**

> The space from XML/JSON is like a wallet, where places are specifically defined for holding cards, notes, and coins; the space from RDA is like an enormous shelf, where you can place anything anywhere in the unlimited space provided.

RDA is specifically designed to avoid targeting a certain data model and having to define and maintain a schema. Such design is reflected by the structure of RDA's encoded storage space, the way of addressing a location in the space, and the supported data types[^5].  

[^5]: First, RDA has multi-dimensional array storage space that is dynamically expandable, that is, the size of each dimension and the number of dimensions can be increased or decreased as required, like an elastic bag. This is in contrast to the ‘fixed’ hierarchical space provided by schema-based encodings, like XML or JSON, which is restricted by a predefined data mode, like a rigid, fixed-shaped box. Second, RDA uses integer-based indexes for addressing the storage locations in its multi-dimensional array storage space, which means, and combination of non-negative integers is a valid address referring to a valid storage location in the space. This is in contrast to XML and JSON, the address for accessing a storage location is a ‘path’ that has to be ‘validated’ against a pre-defined schema. Third, RDA assumes all data (of any type) can be 'expressed as a string' and a value stored at a location (referred to as “a data's value expression”) can only be a string; whilst XML and JSON attempt to define and include every possible data types and a data values stored at a location must conform with what has been defined in the schema.

Inheritively from RDA's schemaless design, the encoding is simpler, more space-efficient, and configuration-free compared to XML and JSON. But perhaps the most interesting and unique property of RDA is the **recursiveness** of the storage space: the multi-dimensional array structure is homogenous, and there can be only one 'unified' data type, so a sub-dimension in the space is itself a multi-dimensional space that has the same structure as its containing (parent dimension) space, and can be used in the same way. The recursiveness of the multi-dimensional space allows an arbitrarily complex data structure to be (recursively) decomposed into sub-components and stored in the dimensions and their sub-dimensions from the provided space.

## The product - Charian

The **Rda class** and the **IRda interface** from Charian API are designed to make object serialization practical, simple, and intuitive. For object serialization, a client only needs to do "data packing and unpacking" using a provided generic container object, without having to establish and maintain a rigid data model or schema for each and every special cases.

But indeed, Charian is not just another data encoder or object serializer, but an **enabling technology of a new way of data communication**. By making cross-program data exchange simpler and more flexible, Charian allows for building a "post-office-like" data exchange eco-system through which more programs and devices can connect and work collaboratively like never before. 

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
