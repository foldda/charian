# Charian API Documentation

This document contains the detailed API reference and worked examples for Charian. For a high-level introduction to the project, see the main [README](README.md).

## Table of Contents
1. [Class Rda - an RDA encoder/parser](#class-rda---an-rda-encoderparser)
   - [How-to: encoding and decoding an RDA string](#how-to-encoding-and-decoding-an-rda-string)
2. [Interface IRda - app-layer schema resolution](#interface-irda---app-layer-schema-resolution)
   - [How-to: Serializing a complex object with nested classes](#how-to-serializing-a-complex-object-with-nested-classes)
3. [Handling data model changes](#handling-data-model-changes)
   - [How-to: Exception handling](#how-to-exception-handling)

## Class Rda - an RDA encoder/parser

The Rda class is modeled as a "container" object for storing data. It has a multidimensional space where each storage location in the space is uniquely addressed by an integer array index[^3]. A client uses the following Getter/Setter methods for accessing a data item in the space for a given address:

[^3]: The index has a dimension limit of 40 in the current implementation, and the index value for each dimension must be a non-negative integer.
```csharp
public void SetValue(string value, int[] address)     /* save a string value at the addressed location */
public string GetValue(int[] address)        /* retrieve a string value from the addressed location */
public void SetRda(Rda rda, int[] address)      /* save an Rda object at the addressed location */
public Rda GetRda(int[] address)      /* retrieve an Rda object from the addressed location */
```

An Rda container supports storing only two "data types" - a data item can be either a string or an Rda (container) object. Charian assumes all primitive data, like an integer or a date, can be converted to a string and all composite data, like a class or an array, can be stored as an Rda object (by recursively decomposing the data object to less complex structures or primitive data items, as in [the example below](#how-to-serializing-a-complex-object-with-nested-classes)).

In addition, the Rda class implements the following methods that convert itself to and from an [RDA string](https://github.com/foldda/rda), so it can be used as a generic RDA parser/encoder:

```csharp
public string ToString()      /* convert this Rda container object to an RDA string */
public static Rda Parse(string rdaEncodedString)   /* decode the RDA string and return an Rda container object  */
```

_**Note:** From the API, class Rda offers additional methods and properties beyond the (core) methods described above. Please refer to the class test cases in this repo for usage examples of all the implemented features._

### How-to: encoding and decoding an RDA string

This example groups a collection of discrete data items and saves them to a file as an RDA-encoded string. The program uses the provided "unrestricted" storage to hold arbitrarily structured data (here, the structure is sequential) without needing to predefine a schema. Note that the underlying RDA encoding is transparent to the client throughout.

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

**Takeaway**: Primitive data is stored as strings. The sender and receiver are each expected to know where (placement) and what (type) the data items are within a container. The Rda container has no schema and performs no data validation — clients are responsible for type conversion and validation, and for [handling exceptions if unexpected data is encountered](#how-to-exception-handling).

## Interface IRda - app-layer schema resolution

The IRda interface defines two methods:

```csharp
Rda ToRda()   /* here, data object "packs" its properties and states into an Rda container */
IRda FromRda(Rda rda) /* here, data object "unpacks and restores" its properties and states from values stored in an Rda container */
```

The [README](README.md#example-serializing-a-simple-data-object) already showed the `Person` class implementing these two methods to make itself serializable during a data exchange. Below is an extended example of serializing a complex Person class having nested classes.

### How-to: Serializing a complex object with nested classes

Because an Rda object can store another Rda object, any arbitrarily complex object can, in principle, be stored inside a single Rda container through recursive decomposition. The following example builds on the last one, showing how a `ComplexPerson` object with two `Address` properties (themselves serializable) is packed into an Rda container.

_NB: Rda class supports indexer accessors (e.g. rda[i].ScalarValue / rda[i][j]) as syntactic sugar over GetValue/SetValue._

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

## Handling data model changes

As in the previous example, a sender application can dynamically enhance its data model while maintaining compatibility — such as through using the C# language's polymorphism feature.

Similarly, a receiver application can dynamically resolve a data model by testing the received data content against expected data and data models. One way of doing this is through the program's "exception handling."

### How-to: Exception handling

The following example builds on the last one and shows techniques you can apply during "unpacking" when the received data is unexpected.

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
                2) escalating the error (i.e., re-throw)
                3) returning the data back to the sender, and/or requesting re-send
            */
        }
    }
}
```

**Takeaway**: You can implement flexible, sophisticated error handling when unpacking a data container.
