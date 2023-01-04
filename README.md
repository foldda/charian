# Freightman

Freightman an object-serialization[^1] technology for building application-level data transport systems. 

[^1]: object-serialization technology converts an object from a program into a serial of bytes (or chars), so the object can be easily transported to, and be re-constructed in, another program.

Available as a simple API, Freightman can be used for - 

* Persistant storage - storing data objects of a program to a file or a database; 
* Distributed computing - allowing separated parts of a distributed application to communicate with each other;
* Systems integration - allowing applications from different vendors to exchange data;
* Other cross-application data-transfer tasks.

Compared to the other object-serialization technologies and solutions, Freightman offers these distinctive features - 

* It is easy to use: Freightman is schema-less and application-independent, meaning it does not require any application-specific setup, pre-build or rebuild; 
* It is super lightweight: the API has only a handful of non-frill methods and has less than 800 lines of code.
* It is easy to maintain: Freightman has no dependency, and with code-level integration, it can be compiled and built as part of your project with no extra requirement.

Despite its simplicity, Freightman's capability rivals the most sophiscated competing solutions. For example, Freightman is able to serialize data objects with arbitary complexicity (e.g. objects with deeply nested classes), and can be used for transferring data between applications in different programming languages and on different platforms[^2]. 

[^2]: Subject to RDA encoder and parser availablity for the language and on the platform.

This project provides the Freightman API implementation in C#, Java, and Python.

## What problem does Freightman solve, and the others don't?

How Freightman works can be better explained in the context of data transportation, with an analogy of parcel delievery using the Post Office. The diagram below shows how a sender is posting a parcel to a receiver.

Parcel senders <=> Post Office <=> Generic transport options <=> Post Office <=> Parcel receivers

Parcel sender <=> custom, dedicated transport <=> Parcel receiver

By using the Post Office parcel delievery system, any sender and receiver can exchange goods easily with low cost, comparing to custom-made point-to-point parcel transport and delievery.
  
Cross-application data transportation system using Freightman works in the same way as the Post Office parcel delievery system:
  
Data-sending application => Freightman API => generic, unified data-transport => Freightman API => Data receiving application

Data-sending application => custom build, app-specific (i.e. schema-based) data link => Data receiving application

As you may have realized, the key of the Post Office system is to have a generic parcel packaging format (i.e. a box or an envelop) that is **not** specific to a certain content, but is acceptable by all parties for storing any types of contents, and as such, the parcel transport system can be unified and shared. Similarily, Freightman does not serialize any specific data object, instead it provides the applications an universal container that is serializable, and the universal container is capable to store any specific data from an application that requires serialization and transportation. Freightman's approach avoids having to build dedicated data transport links for every application data exchange.

## RDA, an universal serializable data container 

is concepturally different to the other serialization solutions but achieves the same data transportation goal regardlessly, and -enabled data transport system is to provide generic, standardised data-delivery service to any application, for simple and efficient data exchange. 

When sending a parcel via Post-office, there are three parties involved in the process - 
* A sender: 
* The Post office:
* A receiver:

Freightman's role in the data-exchange process closely assembles this scenario -

* A sending application:
* Freightman:
* A receiving application:

Just like a post office offering general delievery for anyone's posting needs in our normal life, it eliminates the need of building separate custom link for every data-exchange application.

This is compared to the conventional methods of cross-application data exchange, which requires building custom dedicated data transport for the specific data requirements of the communicating applications.

## An 




To fulfill 

## Example 1: serializating a complex data object having nested classes

Serializing a person object with address nested class

This example shows, with Freightman, you can easily store a complex data object to a file (or into database), and retsore it back.

## Example 2: cross-language and cross-platform object-serialization

A python save a person object to a disk

A java program restores a person object from a python disk file


## Flexible and precise data-handling

With Freightman, there is no schema to design and to maintain. Your code is the schema and is where you maintain the data structure and handle data validations. From software architecture design perspective, this means a responsibility-shift from the data transport layer to the sending and the receiving applications. The applications are now responsible for data packing and unpacking while the data transport layer is responsible for data delivery. This is compared to with the conventional methods, with which the data transport layer requires setting up pre-agreed schemas or managing pre-build proxy objects, while the applications are less involved in the data communications handling.

Like it would happen in the post-office delievery, if there is a disbute between the sender and the receiver, they will sort out the issue between themselves. For example, if the receiver bought a chair from a shop sender, and if there are a few screws missing in the delivery, then the receiver have a number of options eg use some spare scews or call the sender to re-send part or the whole chair. Just don't blaim or harras the delievery man.

It is very similar to Freightman delivery scenario. Below is an example how a missing/unexpected date is handled -

It is arguable that, with Freightman it allows the application to flexibaly and dynamically handle any data format changes or errors, at the places of data packing and unpacking, whilst with the conventional methods, the applications are stuck with what has been set, and any data format change will more likely break the data communication. For example, with Freightman, a receiving application can have several attempts when parsing an incoming data value, like this -

```

```

Compared to using a schema, and get a "missing date or incorrect format" error, you have more options to handle the situation.

It's like when you're moving house, do you prefer to pack and unpack the boxes yourself and the moving company is only responsible for transporting the boxes (like using Freightman), or you can give the moving company a strict list of items for packing, transport and unpacking (like serialization using a fixed XML/Json schema); or let the moving company to automatically "discover" what needs be moved and does it for you (like using some other object-serialization framework)?

You'd use Freightman for its **ultimate simplicity**. 

You'd use Freightman for its **less dependency** and **better integration**. 

You'd use Freightman for **cleaner** and **easy to manage** code. 

You'd use Freightman for its potential that allows **connecting everything together**. By using a post-office-like generic data-exchange system, you are potentially able to connect all your computing devices together[1], using a common framework and protocol.

[1]: Subject to RDA parser availablity for the application's programming language.

## How to start

## More details

* Project Wiki

* Test cases
