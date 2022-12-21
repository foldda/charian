# Freightman

Freightman a supper lightweight object-serialization[^1] system, which is used for converting an object of a program to a string, so the object can be saved to files or to a database, or be transferred to another program over the network.

With Freightman object-serialization, arbitorily complex data (e.g. objects with nested classes) can be easily transferred from one application to another application, even the applications are programmed in different languages and on different platforms[^2]. Freightman can be used for -

* storing the program's objects in a persistant storage, such as in files or databases. 
* distributed computing - 
* applications integration - 

[^1]: Object-serialization is technology that converts an object of a program to a string,  

[^2]: Subject to the RDA parser's availability for the programming language and the computer platform. 

Freightman uses the schema-less RDA data format for encoding. Compared to other schemas-based object-serialization methods, Freightman is much simpler and easier to use, as it's application-independent and does not require any application-specific setup, rebuild, or configuration. 

This project provides the Freightman implementation as API, available in C#, Java, and Python.

## Universal Data Transport: data-delivery like the Post office

The vision of Freightman data exchange system is to provide generic, standardised data-delivery service to any application, for simple and efficient data exchange. 

When sending a parcel via Post-office, there are three parties involved in the process - 
* A sender:
* The Post office:
* A receiver:

Freightman's role in the data-exchange process closely assembles this scenario -

* A sending application:
* Data transport using Freightman:
* A receiving application:

Just like a post office offering general delievery for anyone's posting needs in our normal life, it eliminates the need of building separate custom link for every data-exchange application.

This is compared to the conventional methods of cross-application data exchange, which requires building custom dedicated data transport for the specific data requirements of the communicating applications.

## An universal serializable data container 




To fulfill 

## Example: serializating a complex data object that has nested classes

Serializing a person object with address nested class

This example shows, with Freightman, you can easily store a complex data object to a file (or into database), and retsore it back.

## Example: cross-language and cross-platform object-serialization

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
