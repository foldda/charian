# Freightman

Freightman is an object-serialization framework that converts an object of a program to a string, so that the object can be saved to a file, be stored in a database, or be transferred to another program over the network. 

By using to a new, schemaless encoding called RDA, Freightman's unconventional serialization method allows effortless transfer of structured data (such as an object with nested classes) cross-language and cross-platform, and is much simpler to implement compared to the other object-serialization methods.

## The Freightman concept: a "Post Office" style data-exchange

When sending a parcel via Post-office, there are three parties involved in the process - 
* A sender:
* The Post office:
* A receiver:

Freightman's data-exchange process closely assembles the Post Office scenario -

* A sending application:
* Freightman:
* A receiving application:

Just like the Post office offers general delievery for anyone's posting needs in our normal life, the intended benefit of using Freightman is to have a standard, low-cost transport for any applications to connect and to exchange data. This is compared to the conventional methods of cross-application data exchange, which requires building custom dedicated data transport for the specific data requirements of the communicating applications.

## A serializable "universal" data container 




To fulfill 

## Example: serializating a complex data object that has nested classes

Serializing a person object with address nested class

This example shows, with Freightman, you can easily store a complex data object to a file (or into database), and retsore it back.

## Example: cross-language and cross-platform object-serialization

A python save a person object to a disk

A java program restores a person object from a python disk file


## What is "the catch"?

With Freightman, there is no schema to design and to maintain. Your code is the schema and is where you maintain the data structure and handle data validations. From software architecture design perspective, this means a responsibility-shift from the data transport layer to the sending and the receiving applications. The applications are now responsible for data packing and unpacking while the data transport layer is responsible for data delivery. This is compared to with the conventional methods, with which the data transport layer requires setting up pre-agreed schemas or managing pre-build proxy objects, while the applications are less involved in the data communications handling.

It is arguable that, with Freightman it allows the application to flexibaly and dynamically handle any data format changes or errors, at the places of data packing and unpacking, whilst with the conventional methods, the applications are stuck with what has been set, and any data format change will more likely break the data communication. 

For example, with Freightman, a receiving application can have several attempts when parsing an incoming data value, like this -

```

```

It's like when you're moving house, do you prefer to pack and unpack the boxes yourself and the moving company is only responsible for transporting the boxes (like using Freightman), or you can give the moving company a strict list of items for packing, transport and unpacking (like serialization using a fixed XML/Json schema); or let the moving company to automatically "discover" what needs be moved and does it for you (like using some other object-serialization framework)?

## Why use Freightman?

You may consider using Freightman for its simplicity. 

Freightman also makes your code more clear and easier to understand, thanks to the "post office" metaphor .

Effortless handling storing and transporting arbitorily complex data cross-platform and cross-language.


## How to start

## More details

* Project Wiki

* Test cases
