// Copyright (c) 2020 Michael Chen
// Licensed under the MIT License -
// https://github.com/foldda/rda/blob/main/LICENSE

using Charian;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class RdaTests
    {
        [TestMethod]
        public void ConstructorAndParsingTest()
        {
            var newRandomValue = "Two";
            string rdaString0 = @"";
            Rda rda0 = Rda.Parse(rdaString0);
            Assert.AreEqual(string.Empty, rda0.ToString()); //dim=0
            rda0.SetValue(2, newRandomValue);
            Assert.AreEqual(newRandomValue, rda0.GetValue(2)); //value at expanded container
            Assert.AreEqual(1, rda0.Dimension);  //dim=1

            rdaString0 = @"Xyz";
            int[] addr = new int[] { 1, 2, 3 };
            rda0 = Rda.Parse(rdaString0);
            Assert.AreEqual(rdaString0, rda0.ScalarValue); //test scalar value keeping
            rda0.SetValue(addr, newRandomValue);
            Assert.AreEqual(rdaString0, rda0.ScalarValue); //test scalar value keeping
            Assert.AreEqual(newRandomValue, rda0.GetValue(addr)); //test stored value
            Assert.AreEqual(3, rda0.Dimension);  //dim=3

            rdaString0 = @"Abc";
            int[] addr2D= new int[] { 0, 2 };
            rda0 = Rda.Parse(rdaString0);
            Assert.AreEqual(rdaString0, rda0.ScalarValue); //test scalar value keeping
            rda0.SetValue(addr2D, newRandomValue);
            Assert.AreEqual(2, rda0.Dimension);  //dim=2
            Assert.AreEqual(newRandomValue, rda0.GetValue(addr2D)); //test stored value
            Assert.AreEqual(rdaString0, rda0.ScalarValue); //test scalar value keeping

            var newScarlar = "scalar value at [0,0,0] replaced";
            rda0.SetValue(new int[] { 0, 0, 0 }, newScarlar);
            Assert.AreEqual(3, rda0.Dimension);  //dim=3
            Assert.AreEqual(newRandomValue, rda0.GetValue(addr2D)); //test stored value being kept
            Assert.AreEqual(newScarlar, rda0.ScalarValue); //test scalar value replaced
            //now replacing scalar value at root, expecting everything underneath to be deleted
            newScarlar = "scalar value at [0] replaced";
            rda0.SetValue(0, newScarlar);
            Assert.AreEqual(newScarlar, rda0.ScalarValue); //test scalar value replaced
            Assert.AreEqual(1, rda0.Dimension);  //dim becomes 1
            Assert.AreEqual(string.Empty, rda0.GetValue(addr2D)); //test previously stored value being deleted
            Assert.IsTrue(rda0.GetRda(addr2D).IsDummy); //test previously stored value being deleted

            rda0.ScalarValue = "D-0 Scalar";
            Assert.AreEqual(rda0.ToString(), rda0.ScalarValue);
            Assert.AreEqual(0, rda0.Dimension);  //dim is 0
            //test scalar value being "push donw" to higher dimension
            Assert.AreEqual(rda0.ScalarValue, rda0.GetValue(0)); //test previously stored value being deleted
            Assert.IsFalse(rda0.GetRda(0).IsDummy); //left-most node always "exists"
            Assert.AreEqual(1, rda0.Dimension);  //dim is expanded and becomes 1
            Assert.IsTrue(rda0.GetRda(new int[] { 0, 0, 1 }).IsDummy); //test previously stored value being deleted
            Assert.AreEqual(3, rda0.Dimension);  //dim gets expanded
            rda0.ToString().Print("expanded rda with sigle scalar value");
            rda0.CompressDimension();
            rda0.ToString().Print("compressed rda with sigle scalar value");

            //delimiters are not mendatory for a RDA container, delimiters can be added (or be removed) as required
            string rdaString1 = @"|\|";
            Rda rda1 = Rda.Parse(rdaString1);
            Assert.AreEqual(rdaString1, rda1.ToString()); //dim=1
            Assert.AreEqual(string.Empty, rda1.GetRda(0).ToString()); //dim=1

            rda1.CompressDimension();
            Assert.AreEqual(string.Empty, rda1.ToString()); //dim=0 after trimming

            rdaString1 = @"|;\|A";  //for the container to function, the redundantly defined delimiters are not necessary
            rda1 = Rda.Parse(rdaString1);   
            //.. but is reserved as the result of of parsing
            Assert.AreEqual(2, rda1.Dimension); //dim=2 
            Assert.AreEqual(rdaString1, rda1.ToString()); //dim=2 
            Assert.AreEqual(@";\;A", rda1[0].ToString()); //as dim=1

            //we can enforce only minimal dimension required for encoding that data
            rda1.CompressDimension();
            Assert.AreEqual("A", rda1.ToString()); //the "minimal 'encoded' rda" can be as little as dim=0
            Assert.AreEqual("A", rda1[0].ToString()); //as dim=0 after compression

            //testing indexing at position-0 (addressing arbitrarily with "non-existing" dimensions) 
            //it demonstrates an element's scalar-value at this position has "dimension equality" 
            Assert.AreEqual("A", rda1[0].ScalarValue); //as dim=0
            Assert.AreEqual("A", rda1[0][0].ScalarValue); //as dim=1
            Assert.AreEqual("A", rda1[0][0][0].ScalarValue); //as dim=2

            rdaString1 = @"|\|A| ";
            rda1 = Rda.Parse(rdaString1);
            //minimal dimension required for encoding that data
            Assert.AreEqual(1, rda1.Dimension); //dim=1
            Assert.AreEqual(rdaString1, rda1.ToString()); //dim=1

            rdaString1 = @"|\|A|B";
            rda1 = Rda.Parse(rdaString1);
            Assert.AreEqual(1, rda1.Dimension); //dim=1
            Assert.AreEqual(rdaString1, rda1.ToString()); //dim=1

            string rdaString2 = @"|;,\|";   //a empty 2-d container
            Rda rda2 = Rda.Parse(rdaString2);
            Assert.AreEqual(rdaString2, rda2.ToString()); //dim=2
            rdaString2 = @"|;,\|A;a,1;b|B"; //a 3-d container with data
            rda2 = Rda.Parse(rdaString2);
            Assert.AreEqual(rdaString2, rda2.ToString()); //dim=3

            Rda rda3 = new Rda();
            rda3.ScalarValue = "Michael";
            rda3.ToString().Print("Dimension-0 Rda");
            Assert.AreEqual("Michael", rda3.ToString());
            //assigning a value at a higher dimension make the RDA becoming the required (higher) dimension
            rda3[2].ScalarValue = "Chen";   
            //the original scalar value is preserved as [0]
            Assert.AreEqual("Michael", rda3[0].ToString());
            Assert.AreEqual("Chen", rda3[2].ToString());
            rda3.ToString().Print("Dimension-1 Rda");
        }

        [TestMethod]
        public void ElementAddressingTest()
        {
            string s1 = "s1", s2 = "s2", s3 = "s3", s4 = "s4";
            string original = @$"|;,\|{s1}|{s2}|{s3}\{s4}";
            Rda rda = Rda.Parse(original);

            //get
            Assert.AreEqual(@";,\;s1", rda[0].ToString()); //rda[0] is 2-dimensional
            Assert.AreEqual(s1, rda[0].ScalarValue); //dim=0
            Assert.AreEqual("s3\\s4", rda[2].ScalarValue); //dim=0

            int[] addr1 = new int[] { 1 };
            Assert.AreEqual(@";,\;s2", rda.GetRda(addr1).ToString()); //rda[1] is 2-dimensional

            //set
            rda[5].ScalarValue = "555";
            Assert.AreEqual("555", rda[5].ToString());
            rda[5, 1].ScalarValue = "555b";
            Assert.AreEqual("555b", rda.GetRda(new int[] { 5, 1 }).ToString());  //default-child
            Assert.AreEqual("555b", rda.GetRda(new int[] { 5, 1, 0, 0, 0, 0 }).ToString());  //default-child
            //original [5] value is pushed down, but is the "defualt" value (aka FirstValue) for [5] 
            Assert.AreEqual("555", rda[5, 0].ToString());
            Assert.AreEqual("555", rda[5].ScalarValue);

            //add
            rda.AddValue("6666");    //will be added to position [6], as pos [5] was the last element
            Assert.AreEqual("6666", rda[6].ToString());

        }

        [TestMethod]
        public void InputParsingOutputFormattingTest()
        {
            Rda rda = new Rda();
            //setup
            foreach (var itemValue in new string[] { "SEC0", "SEC1", null, "SEC3" }) 
            { 
                rda.AddValue(itemValue); 
            }

            //assert
            var r0 = rda[0];
            Assert.AreEqual("SEC0", r0.ToString());
            rda.ToStringFormatted().Print("rda"); ;
            r0.ToStringFormatted().Print("rda[0]"); ;
            r0[0].ToStringFormatted().Print("rda[0,0]"); ;
            var r00 = r0[0];
            string s = r00.ToString();
            Assert.AreEqual("SEC0", s);
            Assert.AreEqual("SEC0", rda[0, 0].ToString());
            Assert.AreEqual("SEC1", rda[1].ToString());
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 2 }).ToString());
            Assert.AreEqual("SEC3", rda.GetRda(new int[] { 3 }).ToString());
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 4 }).ToString());

            string v2FormattedString = @"|;\|
 ""SEC0""
|""SEC1""
|
|""SEC3""";
            //Assert.AreEqual(v2FormattedString, rdaString.ToString2());
            v2FormattedString.Print("input");
            rda.ToStringFormatted().Print("output");
            var s1 = rda.ToStringFormatted();
            Assert.AreEqual(v2FormattedString.ToPrintable(), s1.ToPrintable());



            //input is a casually formatted v2 rda string:
            //1) arbitory leading and trailing whitespaces, 2) S1c contains in element spaces 3) S1d is fully quoted 4) S1e is half-quoted
            string v2FormattedIN = "|;\\|  \r\n\r\n SEC0|  \t \r        \r\n\t S1a\r\n\t;\r  S1b\r\r\n\t;\r\n\n  S1c.1 \tS1c.2   \r\n\t;\"\t S1d \" \n\r\n; \" S1e|\r\n|SEC3  ";
            v2FormattedIN.Print("v2RDA-input");

            //Expects leading/trailing to be trimmed, quoted spaces are reserved, delimiter-based parsing as usual
            string v1Equivalent = "|;\\|SEC0|S1a;S1b;S1c.1 \tS1c.2;\t S1d ; S1e||SEC3";
            Rda rda3 = Rda.Parse(v2FormattedIN);
            Assert.AreEqual(v1Equivalent, rda3.ToString());

            //test v2-rda formatted output, which has the "standard" indentation and quotes
            string v2FormattedOUT = rda3.ToStringFormatted(); 
            v2FormattedOUT.Print("v2-formatted-outpout");

            //construct a RDA based on the standard v2-output, then compare the RDA content with the original
            Rda rda3FromV2out = Rda.Parse(v2FormattedOUT);
            Assert.AreEqual(rda3FromV2out.ToString(), rda3.ToString());
        }

        [TestMethod]
        public void ValueSetterGetterEscapingTest()
        {
            // test ContainerHeader getters
            Rda rda = Rda.Parse(@"|;,\|sec1|sec2|,a;b,c");
            //normal
            Assert.AreEqual(@";,\;sec1", rda.GetRda(new int[] { 0 }).ToString());
            Assert.AreEqual(@";,\;sec2", rda.GetRda(new int[] { 1 }).ToString());

            //.. first child-index, if there is no delimiter in the value
            //imaging there were delimiters at the end of the value, but they were trimmed when being encoded
            //eg "sec1,;" as "sec1"
            Assert.AreEqual(@",\,sec1", rda.GetRda(new int[] { 0, 0 }).ToString());
            Assert.AreEqual(@"sec1", rda.GetRda(new int[] { 0, 0, 0 }).ToString());
            Assert.AreEqual(@"sec1", rda.GetValue(0));
            Assert.AreEqual(@",\,sec2", rda.GetRda(new int[] { 1, 0 }).ToString());
            Assert.AreEqual(@"sec2", rda[1].ScalarValue);
            Assert.AreEqual(@"a", rda.GetRda(new int[] { 2, 0, 1 }).ToString());

            //'empty' if no value (probably meaning NULL in the application) is stored at the indexed location 
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 0, 0, 1 }).ToString());
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 5, 0 }).ToString());
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 0, 0, 0, 6 }).ToString());  //rda indexes always valid, but retrived item may contains no-value

            //overwrite existing values ..
            rda.SetValue(new int[] { 0 }, "SEC1");
            rda.SetValue(new int[] { 1 }, "SEC2");
            rda.SetValue(new int[] { 2, 0, 1 }, "a");
            rda.SetValue(new int[] { 2, 1 }, "b");
            rda.SetValue(new int[] { 2, 1, 1 }, "c");   //this pushes 'b' to pos [2,1,0]
            Assert.AreEqual(@"|;,\|SEC1|SEC2|,a;b,c", rda.ToString());
            Assert.AreEqual(@"SEC2", rda.GetValue(1));
            Assert.AreEqual(@"SEC1", rda.GetValue(0));
            rda.ToStringFormatted().Print("rda");

            //this replace the whole value at rda[2]
            rda[2].ScalarValue="SEC3";
            Assert.AreEqual("|;\\|SEC1|SEC2|SEC3", rda.ToString());

            rda.SetValue(new int[] { 0, 1 }, "SEC1b");
            Assert.AreEqual(@"|;\|SEC1;SEC1b|SEC2|SEC3", rda.ToString());

            rda.SetValue(new int[] { 0, 0, 1 }, "SEC1c");
            Assert.AreEqual(@"|;,\|SEC1,SEC1c;SEC1b|SEC2|SEC3", rda.ToString());

            rda.SetValue(new int[] { 0, 1, 1 }, "SEC1d");
            Assert.AreEqual(@"|;,\|SEC1,SEC1c;SEC1b,SEC1d|SEC2|SEC3", rda.ToString());
            Assert.AreEqual(@";,\;SEC1,SEC1c;SEC1b,SEC1d", rda.GetRda(new int[] { 0 }).ToString());
            Assert.AreEqual(@",\,SEC1,SEC1c", rda.GetRda(new int[] { 0, 0 }).ToString());
            Assert.AreEqual("SEC1d", rda.GetRda(new int[] { 0, 1, 1 }).ToString());    //no encoding section when rda is dim-0

            rda.SetValue(new int[] { 0, 1, 4 }, "SE;|C1d4"); // ";|" before 'C1d4' are expected to be escaped..
            Assert.AreEqual("SE;|C1d4", rda.GetRda(new int[] { 0, 1, 4 }).ToString());
            //Assert.AreEqual("SE;|C1d4", rda.GetSection(new int[] { 0, 1, 4 }).Payload);
            Assert.AreEqual(@"|;,\|SEC1,SEC1c;SEC1b,SEC1d,,,SE\;\|C1d4|SEC2|SEC3", rda.ToString());    //Raw encoded header string, - NB escaped ";|" before 'C1d4'
            //test turple encoder chars
            var section = rda.GetRda(new int[] { 0 });
            Assert.AreEqual(@";,\;SEC1,SEC1c;SEC1b,SEC1d,,,SE\;|C1d4", section.ToString()); // NB '|' before 'C1d4' no longer require escaping
            Assert.AreEqual(';', section.DelimitersInUse[0]);
            Assert.AreEqual(',', section.DelimitersInUse[1]);
            Assert.AreEqual('\\', section.EscapeChar);
            //test default value
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 0, 1, 3 }).ToString());
            Assert.AreEqual(string.Empty, rda.GetRda(new int[] { 0, 1, 8 }).ToString());  //non-exist
            Assert.IsNotNull(rda.GetRda(new int[] { 0, 1, 0, 0 })?.ToString());   //over (dimension) index
            Assert.AreEqual("SEC1d", rda.GetRda(new int[] { 0, 1, 1 }).ToString());

            rda.SetValue(new int[] { 0, 1, 4, 2, 1 }, "Test Over-Index set");
            Assert.AreEqual(@"|;,^:\|SEC1,SEC1c;SEC1b,SEC1d,,,SE\;\|C1d4^^:Test Over-Index set|SEC2|SEC3", rda.ToString());   //Raw encoded payload string 

        }

        [TestMethod]
        public void RdaSetterGetterEscapingTest()
        {
            // test ContainerHeader getters
            Rda rdaBase = Rda.Parse(@"|;,\|sec0|sec1|sec2");
            rdaBase.ToStringFormatted().Print("rda-before-assigned-new-child-rda");
            Rda rdaSec1new = Rda.Parse(@"&_;/&sec1-n0&sec1-n1 /||| ; X_ x&sec1-n2_A/;B");
            rdaSec1new.ToStringFormatted().Print("inserted rda[1]");
            rdaBase[1] = rdaSec1new;
            rdaBase.ToStringFormatted().Print("rda-after-assigned-new-child-rda");
            Assert.AreEqual("sec1-n0", rdaBase[1,0].ScalarValue);   //test the wrapper
            Assert.AreEqual("sec1-n0", rdaBase[1].ScalarValue);   //test the wrapper
            Assert.IsTrue(rdaSec1new.ContentEqual(rdaBase[1]));
            Assert.AreEqual("sec1-n1 /||| ", rdaBase[1,1,0,0].ToString());   //
            Assert.AreEqual(" X", rdaBase[1,1,0,1].ToString());   //

            Rda rdaSec3new = Rda.Parse(@">;/>sec3>sec3-n1>a;b;c");
            rdaSec3new.ToStringFormatted().Print("inserted rda[3]");
            rdaBase.AddRda(rdaSec3new);
            rdaBase.ToStringFormatted().Print("rda-after-adding-new-child-rda");
            Assert.IsTrue(rdaSec3new.ContentEqual(rdaBase[3]));
        }



        [TestMethod]
        public void EscapedValueFormattingTest()
        {
            //double-quote as escape is supposed to be replaced internally, as double-quote is reserved for v2-formatting
            //string s1 = @"|;,""| AAA; xx""|"";"",x | B,b; C ";
            string quoteReplaced = "|;,^| AAA; xx^|^;^,x | B,b; C ";
            Rda rda1 = Rda.Parse(quoteReplaced);
            Assert.AreEqual(quoteReplaced, rda1.ToString());
            string v2FormattedRDA = rda1.ToStringFormatted();
            v2FormattedRDA.Print("v2-formatted");
            Rda rda2 = Rda.Parse(v2FormattedRDA);
            Assert.IsTrue(rda1.ContentEqual(rda2));
            Assert.AreEqual(quoteReplaced.ToPrintable(), rda2.ToString().ToPrintable());


            string s2 = @"|;,^| AAA; xx^|^;^,x | B,b; C ";
            Rda rda3 = Rda.Parse(s2);
            Assert.AreEqual(s2, rda3.ToString());
            string s2f = rda3.ToStringFormatted();
            s2f.Print("v2-formatted");
            Rda rda4 = Rda.Parse(s2f);
            Assert.AreEqual(s2.ToPrintable(), rda4.ToString().ToPrintable());
        }
    }
}
