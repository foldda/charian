# Freightman

Freightman is a lightweight data-transport system for cross-application data communication.

Thanks to its unconventional data-serialization method, Freightman allows effortless transfer of structured data (e.g. objects of nested classes) cross-language and cross-platform, and is much simpler to implement compared to solutions using other data-serialization methods.

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

Serializing a person object with address nested class


To fulfill 

## Example: serializating a complex data object that has nested classes

Base on RDA, Freightman is able to serialize and transfer arbitoryily complex data, using the following approach- 

## Example: cross-language and cross-platform object-serialization

A python save a person object to a disk

A java program restores a person object from a python disk file


## What is "the catch"?

Freightman is a philosorphy change of cross-application data transfer, and accordingly requires the change of the design of the communicating applications. 

With Freightman, there is no schema to design and to maintain. Your code is the schema and is where you maintain your data structure and handle data validation. This means a responsibility-shift the data transport layer (i.e. the protocol parser and encoder) to the sender and the receiver applications, which are now responsible for data packing and unpacking. This is compared to with the conventional methods, which require setting up pre-agreed schemas or having pre-build proxy objects maintained at the data transport layer, but the applications are less involved in the data communications handling.

It is arguable that, with Freightman it allows the application flexibaly and dynamically handling of any data format changes or errors at the places of data packing and unpacking, whilst with the conventional methods, the applications have to stick with what has be set, and any data format change will more likely break the data communication. 

It's like when you're moving house, do you prefer to pack and unpack the boxes yourself and the moving company is only responsible for the boxes' transportation (like using Freightman), or you can give the moving company a strict list to do the packing/unpacking and the transfer following the list (like in using a XML/Json schema); or let the moving company to automatically "discover" what needs be moved and does it for you (like using some other object-serialization framework)?

## Freightman's vision





### simplicity

### flexibility

## What's the catch (i.e. the 'cons')?




## How to start

## More details

* Project Wiki

* Test cases
