// Copyright (c) 2020 Michael Chen
// Licensed under the MIT License -
// https://github.com/foldda/rda/blob/main/LICENSE

using Charian;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTests
{
    public class Person : IRda
    {
        public string FirstName = "John";
        public string LastName = "Smith";

        //specify an allocated position in the RDA for storing each of the object's proterties
        public enum RDA_INDEX : int
        {
            FIRST_NAME = 0,
            LAST_NAME = 1
        }

        //store the properties of this object into an RDA
        public virtual Rda ToRda()
        {
            var rda = new Rda();  //create an RDA container

            //stores each of the properties’ value
            rda[(int)RDA_INDEX.FIRST_NAME].ScalarValue = this.FirstName;
            rda[(int)RDA_INDEX.LAST_NAME].ScalarValue = this.LastName;
            return rda;
        }

        //restore the object’s properties from an RDA
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

        //restore a Person object from an RDA string that is stored in a file
        public static Person ReadFromFile(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);
            Rda rda = Rda.Parse(encodedRdaString);
            Person person = new Person();
            person.FromRda(rda);
            return person;
        }
    }

    class Address : IRda
    {
        public enum RDA_INDEX : int { LINES = 0, ZIP = 1 }

        public string AddressLines = "Line 1\nLine 2\nLine 3";
        public string ZIP = "NY 21540";

        //store the properties into an RDA
        public Rda ToRda()
        {
            var rda = new Rda();  //create an RDA container
                                  //serialize the properties
            rda[(int)RDA_INDEX.LINES].ScalarValue = this.AddressLines;
            rda[(int)RDA_INDEX.ZIP].ScalarValue = this.ZIP;
            return rda;
        }

        //restore the properties from an RDA
        public IRda FromRda(Rda rda)
        {
            this.AddressLines = rda[(int)RDA_INDEX.LINES].ScalarValue;
            this.ZIP = rda[(int)RDA_INDEX.ZIP].ScalarValue;
            return this;
        }
    }

    class ComplexPerson : Person
    {
        public Address ResidentialAddress = new Address() { AddressLines = "1, 2, 3", ZIP = "12345" };
        public Address PostalAddress = new Address() { AddressLines = "a, b, c", ZIP = "23456" };

        public new enum RDA_INDEX : int
        {
            FIRST_NAME = 0,
            LAST_NAME = 1,
            RES_ADDRESS = 2,
            POST_ADDRESS = 3
        }

        public override Rda ToRda()
        {
            Rda personRda = base.ToRda();

            //assign the “residential address” RDA to a location in the “person” RDA
            personRda[(int)RDA_INDEX.RES_ADDRESS] = this.ResidentialAddress.ToRda();

            //now personRda is 2-dimensional
            //Console.Println(personRda[2][1].ScalarValue);   //prints ResidentialAddress.ZIP

            //You can continue to grow the complexity of the Person object.
            //eg storing a further “postal address” RDA to the person RDA, and so on ..
            personRda[(int)RDA_INDEX.POST_ADDRESS] = this.PostalAddress.ToRda();

            return personRda;
        }

        public override IRda FromRda(Rda rda)
        {
            //restore the base Person properties
            base.FromRda(rda);  //restores FirstName and LastName

            //sub-RDA structure is passed on to recursively de-serialize sub objects
            this.ResidentialAddress.FromRda(rda[(int)RDA_INDEX.RES_ADDRESS]);
            this.PostalAddress.FromRda(rda[(int)RDA_INDEX.POST_ADDRESS]);
            return this;
        }

        //deserialize a ComplexPerson object 
        public new static ComplexPerson ReadFromFile(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);
            Rda rda = Rda.Parse(encodedRdaString);
            ComplexPerson person = new ComplexPerson();
            person.FromRda(rda);
            return person;
        }

    }


    [TestClass]
    public class IRdaTests
    {

        [TestMethod]
        public void ObjectSerializationTest()
        {
            string filePath1 = "C:\\Temp\\IRda_Person_Test_binaryfile111.bin";
            var person = new Person();
            person.SaveToFile(filePath1);

            var person2 = Person.ReadFromFile(filePath1);
            Assert.AreEqual(person.FirstName, person2.FirstName);
            Assert.AreEqual(person.LastName, person2.LastName);

            File.Delete(filePath1);  //tear down

            /* test recurrsively embeded serializable objects */

            string filePath2 = "C:\\Temp\\IRda_Person_C_Test_binaryfile222.bin";
            var complexPerson = new ComplexPerson();
            complexPerson.SaveToFile(filePath2);

            var complexPerson2 = ComplexPerson.ReadFromFile(filePath2);
            Assert.AreEqual(complexPerson.FirstName, complexPerson2.FirstName);
            Assert.AreEqual(complexPerson.LastName, complexPerson2.LastName);
            Assert.AreEqual(complexPerson.ResidentialAddress.AddressLines, complexPerson2.ResidentialAddress.AddressLines);
            Assert.AreEqual(complexPerson.ResidentialAddress.ZIP, complexPerson2.ResidentialAddress.ZIP);

            complexPerson2.ToRda().ToString().Print("Complex Person RDA");

            File.Delete(filePath2);  //tear down
        }
    }





    class RdaDemo1
    {
        string PATH = "C:\\Temp\\IRda_Person_Test_binaryfile111.bin";
        public void SendSomeData(string filePath)
        {

            Rda rda1 = new Rda();    //create a new RDA container object

            //store some values at some randomly-chosen locations in the container
            rda1.SetValue(0, "A string");  //storing value "One" at index = 0
            rda1.SetValue(1, 2.5.ToString());
            rda1.SetValue(2, DateTime.Now.ToString());

            string encodedRdaString = rda1.ToString(); //serialize
            File.WriteAllText(filePath, encodedRdaString);
        }

        public void ReceiveSomeData(string filePath)
        {
            string encodedRdaString = File.ReadAllText(filePath);

            Rda rda1 = Rda.Parse(encodedRdaString);    //create a new RDA container object from the RDA string

            //retrive  some values at some designated locations in the container
            string a = rda1.GetValue(0);  //storing value "One" at index = 0
            double b = double.Parse(rda1.GetValue(1));
            DateTime c = DateTime.Parse(rda1.GetValue(2));
        }

        public void Main(string[] args)
        {
            SendSomeData(PATH);
            ReceiveSomeData(PATH);
        }
    }
}
