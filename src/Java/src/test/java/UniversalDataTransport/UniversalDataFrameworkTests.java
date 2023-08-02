package UniversalDataTransport;

import static org.junit.jupiter.api.Assertions.*;

import java.io.*;
import org.junit.jupiter.api.Test;

public class UniversalDataFrameworkTests {

  @Test
  public void ConstructorAndParsingTest() throws Exception {
    String rdaString0 = "";
    Rda rda0 = Rda.Parse(rdaString0);
    assertEquals(rdaString0, rda0.ToString()); //dim=0
    String newRandomValue = "Two";
    //String value = "Two";
    rda0.SetValue(2, newRandomValue);
    assertEquals(newRandomValue, rda0.GetValue(2)); //value at expanded container
    assertEquals(1, rda0.Dimension());  //dim=1

    rdaString0 = "Xyz";
    int[] addr = new int[] { 1, 2, 3 };
    rda0 = Rda.Parse(rdaString0);
    assertEquals(rdaString0, rda0.GetScalarValue()); //test scalar value keeping
    rda0.SetValue(addr, newRandomValue);
    assertEquals(rdaString0, rda0.GetScalarValue()); //test scalar value keeping
    assertEquals(newRandomValue, rda0.GetValue(addr)); //test stored value
    assertEquals(3, rda0.Dimension());  //dim=3


    rdaString0 ="Abc";
    int[] addr2D= new int[] { 0, 2 };
    rda0 = Rda.Parse(rdaString0);
    assertEquals(rdaString0, rda0.GetScalarValue()); //test scalar value keeping
    rda0.SetValue(addr2D, newRandomValue);
    assertEquals(2, rda0.Dimension());  //dim=2
    assertEquals(newRandomValue, rda0.GetValue(addr2D)); //test stored value
    assertEquals(rdaString0, rda0.GetScalarValue()); //test scalar value keeping


    String newScarlar = "scalar value at [0,0,0] replaced";
    rda0.SetValue(new int[] { 0, 0, 0 }, newScarlar);
    assertEquals(3, rda0.Dimension());  //dim=3
    assertEquals(newRandomValue, rda0.GetValue(addr2D)); //test stored value being kept
    assertEquals(newScarlar, rda0.GetScalarValue()); //test scalar value replaced
    //now replacing scalar value at root, expecting everything underneath to be deleted
    newScarlar = "scalar value at [0] replaced";
    rda0.SetValue(0, newScarlar);
    assertEquals(newScarlar, rda0.GetScalarValue()); //test scalar value replaced
    assertEquals(1, rda0.Dimension());  //dim becomes 1
    assertEquals("", rda0.GetValue(addr2D)); //test previously stored value being deleted
    assertTrue(rda0.GetRda(addr2D).IsDummy()); //test previously stored value being deleted

    rda0.SetScalarValue("D-0 Scalar");
    assertEquals(rda0.ToString(), rda0.GetScalarValue());
    assertEquals(0, rda0.Dimension());  //dim is 0
    //test scalar value being "push donw" to higher dimension
    assertEquals(rda0.GetScalarValue(), rda0.GetValue(0)); //test previously stored value being deleted
    assertFalse(rda0.GetRda(0).IsDummy()); //left-most node always "exists"
    assertEquals(1, rda0.Dimension());  //dim is expanded and becomes 1
    assertTrue(rda0.GetRda(new int[] { 0, 0, 1 }).IsDummy()); //test previously stored value being deleted
    assertEquals(3, rda0.Dimension());  //dim gets expanded
    System.out.printf("expanded rda with sigle scalar value:\n %s\n", rda0.ToString());
    rda0.CompressDimension();
    System.out.printf("compressed rda with sigle scalar value:\n %s\n", rda0.ToString());



















    //delimiters are not mendatory for a RDA container, delimiters can be added (or be removed) as required
    String rdaString1 = "|\\|";
    Rda rda1 = Rda.Parse(rdaString1);
    assertEquals(rdaString1, rda1.ToString()); //dim=1
    assertEquals("", rda1.GetRda(0).ToString()); //dim=1
    assertEquals("", rda1.ToStringMinimal()); //dim=0 after trimming
    rdaString1 = "|;\\|A"; //for the container to function, the redundantly defined delimiters are not necessary
    rda1 = Rda.Parse(rdaString1);
    //minimal dimension required for encoding that data
    assertEquals(2, rda1.Dimension()); //dim=0
    assertEquals(rdaString1, rda1.ToString()); //dim=0 the "minimal 'encoded' rda"
    //First-value's "dimension equality"
    assertEquals(";\\;A", rda1.GetRda(0).ToString()); //as dim=1
    assertEquals("A", rda1.GetRda(new int[] { 0, 0 }).ToString()); //as dim=2
    assertEquals("A", rda1.GetRda(new int[] { 0, 0, 0 }).ToString()); //as dim=2
    assertEquals("A", rda1.ToStringMinimal()); //as dim=0
    rdaString1 = "|\\|A| ";
    rda1 = Rda.Parse(rdaString1);
    //minimal dimension required for encoding that data
    assertEquals(1, rda1.Dimension()); //dim=1
    assertEquals(rdaString1, rda1.ToString()); //dim=1
    rdaString1 = "|\\|A|B";
    rda1 = Rda.Parse(rdaString1);
    assertEquals(1, rda1.Dimension()); //dim=1
    assertEquals(rdaString1, rda1.ToString()); //dim=1
    String rdaString2 = "|;,\\|";
    Rda rda2 = Rda.Parse(rdaString2);
    assertEquals(rdaString2, rda2.ToString()); //dim=0
    rdaString2 = "|;,\\|A;a,1;b|B";
    rda2 = Rda.Parse(rdaString2);
    assertEquals(rdaString2, rda2.ToString()); //dim=2
    Rda rda3 = new Rda();
    rda3.SetScalarValue("Michael");
    //   rda3.ToString().Print("Dimension-0 Rda");
    System.out.printf("Dimension-0 Rda %s\n", rda3.ToString());
    assertEquals("Michael", rda3.ToString());
    rda3.GetRda(2).SetScalarValue("Chen");
    //   rda3.ToString().Print("Dimension-1 Rda");
    System.out.printf("Dimension-1 Rda %s\n", rda3.ToString());
    assertEquals("Michael", rda3.GetRda(0).ToString());
    assertEquals("Chen", rda3.GetRda(2).ToString());
  }

  @Test
  public void ElementAddressingTest() throws Exception {
    String s1 = "s1", s2 = "s2", s3 = "s3", s4 = "s4";
    String original = String.format("|;,\\|%s|%s|%s\\%s", s1, s2, s3, s4);
    Rda rda = Rda.Parse(original);

    //get
    assertEquals(";,\\;s1", rda.GetRda(0).ToString()); //rda[0] is 2-dimensional
    assertEquals(s1, rda.GetRda(0).GetScalarValue()); //dim=0
    assertEquals("s3\\s4", rda.GetRda(2).GetScalarValue()); //dim=0

    int[] addr1 = new int[] { 1 };
    assertEquals(";,\\;s2", rda.GetRda(addr1).ToString()); //rda[1] is 2-dimensional

    //set
    rda.GetRda(5).SetScalarValue("555");
    assertEquals("555", rda.GetRda(5).ToString());
    rda.GetRda(new int[] { 5, 1 }).SetScalarValue("555b");
    assertEquals("555b", rda.GetRda(new int[] { 5, 1 }).ToString()); //default-child
    assertEquals("555b", rda.GetRda(new int[] { 5, 1, 0, 0, 0, 0 }).ToString()); //default-child
    //original [5] value is pushed down, but is the "defualt" value (aka FirstValue) for [5]
    assertEquals("555", rda.GetRda(new int[] { 5, 0 }).ToString());
    assertEquals("555", rda.GetRda(5).GetScalarValue());

    //add
    rda.AddValue("6666"); //will be added to position [6], as pos [5] was the last element
    assertEquals("6666", rda.GetRda(6).ToString());
  }

  @Test
  public void InputParsingOutputFormattingTest() throws Exception {
    Rda rda = new Rda();
    //setup
    String[] values = { "SEC0", "SEC1", null, "SEC3" };
    for (var itemValue : values) {
      rda.AddValue(itemValue);
    }

    //assert
    Rda r0 = rda.GetRda(0);
    assertEquals("SEC0", r0.ToString());
    System.out.printf("rda %s\n", rda.ToStringFormatted());
    System.out.printf("rda[0] %s\n", r0.ToStringFormatted());
    System.out.printf("rda[0,0] %s\n", r0.GetRda(0).ToStringFormatted());
    Rda r00 = r0.GetRda(0);
    String s = r00.ToString();
    assertEquals("SEC0", s);
    assertEquals("SEC0", rda.GetRda(new int[] { 0, 0 }).ToString());
    assertEquals("SEC1", rda.GetRda(1).ToString());
    assertEquals("", rda.GetRda(new int[] { 2 }).ToString());
    assertEquals("SEC3", rda.GetRda(new int[] { 3 }).ToString());
    assertEquals("", rda.GetRda(new int[] { 4 }).ToString());
    String v2FormattedString =
      "|;\\|\r\n".concat(" \"SEC0\"\r\n")
        .concat("|\"SEC1\"\r\n")
        .concat("|\r\n")
        .concat("|\"SEC3\"");
    //assertEquals(v2FormattedString, rdaString.ToString2());
    System.out.printf("input %s\n", v2FormattedString);
    System.out.printf("output %s\n", rda.ToStringFormatted());
    var s1 = rda.ToStringFormatted();
    assertEquals(v2FormattedString, s1);
    //input is a casually formatted v2 rda string:
    //1) arbitory leading and trailing whitespaces, 2) S1c contains in element spaces 3) S1d is fully quoted 4) S1e is half-quoted
    String v2FormattedIN =
      "|;\\|  \r\n\r\n SEC0| \t \r    \r\n\t S1a\r\n\t;\r  S1b\r\r\n\t;\r\n\n  S1c.1 \tS1c.2   \r\n\t;\"\t S1d \" \n\r\n; \" S1e|\r\n|SEC3  ";
    System.out.printf("v2RDA-input %s\n", v2FormattedIN);

    //Expects leading/trailing to be trimmed, quoted spaces are reserved, delimiter-based parsing as usual
    String v1Equivalent = "|;\\|SEC0|S1a;S1b;S1c.1 \tS1c.2;\t S1d ; S1e||SEC3";
    Rda rda3 = Rda.Parse(v2FormattedIN);
    assertEquals(v1Equivalent, rda3.ToString());

    //test v2-rda formatted output, which has the "standard" indentation and quotes
    String v2FormattedOUT = rda3.ToStringFormatted();
    System.out.printf("v2-formatted-outpout %s\n", v2FormattedOUT);

    //construct a RDA based on the standard v2-output, then compare the RDA content with the original
    Rda rda3FromV2out = Rda.Parse(
      v2FormattedOUT
    );
    assertEquals(rda3FromV2out.ToString(), rda3.ToString());
  }

  @Test
  public void ValueSetterGetterEscapingTest() throws Exception {
    // test ContainerHeader getters
    Rda rda = Rda.Parse(
      "|;,\\|sec1|sec2|,a;b,c"
    );
    //normal
    assertEquals(";,\\;sec1", rda.GetRda(new int[] { 0 }).ToString());
    assertEquals(";,\\;sec2", rda.GetRda(new int[] { 1 }).ToString());
    //.. first child-index, if there is no delimiter in the value
    //imaging there were delimiters at the end of the value, but they were trimmed when being encoded
    //eg "sec1,;" as "sec1"
    assertEquals(",\\,sec1", rda.GetRda(new int[] { 0, 0 }).ToString());
    assertEquals("sec1", rda.GetRda(new int[] { 0, 0, 0 }).ToString());
    assertEquals("sec1", rda.GetValue(0));
    assertEquals(",\\,sec2", rda.GetRda(new int[] { 1, 0 }).ToString());
    assertEquals("sec2", rda.GetRda(1).GetScalarValue());
    assertEquals("a", rda.GetRda(new int[] { 2, 0, 1 }).ToString());
    //'empty' if no value (probably meaning NULL in the application) is stored at the indexed location
    assertEquals("", rda.GetRda(new int[] { 0, 0, 1 }).ToString());
    assertEquals("", rda.GetRda(new int[] { 5, 0 }).ToString());
    assertEquals("", rda.GetRda(new int[] { 0, 0, 0, 6 }).ToString()); //rda indexes always valid, but retrived item may contains no-value
    //overwrite existing values ..
    rda.SetValue(new int[] { 0 }, "SEC1");
    rda.SetValue(new int[] { 1 }, "SEC2");
    rda.SetValue(new int[] { 2, 0, 1 }, "a");
    rda.SetValue(new int[] { 2, 1 }, "b");
    rda.SetValue(new int[] { 2, 1, 1 }, "c"); //this pushes 'b' to pos [2,1,0]
    assertEquals("|;,\\|SEC1|SEC2|,a;b,c", rda.ToString());
    assertEquals("SEC2", rda.GetValue(1));
    assertEquals("SEC1", rda.GetValue(0));

    System.out.printf("rda %s\n", rda.ToStringFormatted());
    //this replace the whole value at rda[2]
    rda.GetRda(2).SetScalarValue("SEC3");
    assertEquals("|;\\|SEC1|SEC2|SEC3", rda.ToString());

    rda.SetValue(new int[] { 0, 1 }, "SEC1b");
    assertEquals("|;\\|SEC1;SEC1b|SEC2|SEC3", rda.ToString());

    rda.SetValue(new int[] { 0, 0, 1 }, "SEC1c");
    assertEquals("|;,\\|SEC1,SEC1c;SEC1b|SEC2|SEC3", rda.ToString());

    rda.SetValue(new int[] { 0, 1, 1 }, "SEC1d");
    assertEquals("|;,\\|SEC1,SEC1c;SEC1b,SEC1d|SEC2|SEC3", rda.ToString());
    assertEquals(
      ";,\\;SEC1,SEC1c;SEC1b,SEC1d",
      rda.GetRda(new int[] { 0 }).ToString()
    );
    assertEquals(",\\,SEC1,SEC1c", rda.GetRda(new int[] { 0, 0 }).ToString());
    assertEquals("SEC1d", rda.GetRda(new int[] { 0, 1, 1 }).ToString()); //no encoding section when rda is dim-0
    rda.SetValue(new int[] { 0, 1, 4 }, "SE;|C1d4"); // ";|" before 'C1d4' are expected to be escaped..
    assertEquals("SE;|C1d4", rda.GetRda(new int[] { 0, 1, 4 }).ToString());
    //assertEquals("SE;|C1d4", rda.GetSection(new int[] { 0, 1, 4 }).Payload);
    assertEquals(
      "|;,\\|SEC1,SEC1c;SEC1b,SEC1d,,,SE\\;\\|C1d4|SEC2|SEC3",
      rda.ToString()
    ); //Raw encoded header string, - NB escaped ";|" before 'C1d4'
    //test turple encoder chars
    var section = rda.GetRda(new int[] { 0 });
    assertEquals(
      ";,\\;SEC1,SEC1c;SEC1b,SEC1d,,,SE\\;|C1d4",
      section.ToString()
    );
    assertEquals(';', section.DelimitersInUse()[0]);
    assertEquals(',', section.DelimitersInUse()[1]);
    assertEquals('\\', section.EscapeChar());
    //test default value
    assertEquals("", rda.GetRda(new int[] { 0, 1, 3 }).ToString());
    assertEquals("", rda.GetRda(new int[] { 0, 1, 8 }).ToString()); //non-exist
    assertNotEquals(null, rda.GetRda(new int[] { 0, 1, 0, 0 }).ToString()); //over (dimension) index
    assertEquals("SEC1d", rda.GetRda(new int[] { 0, 1, 1 }).ToString());

    rda.SetValue(new int[] { 0, 1, 4, 2, 1 }, "Test Over-Index set");
    assertEquals(
      "|;,^:\\|SEC1,SEC1c;SEC1b,SEC1d,,,SE\\;\\|C1d4^^:Test Over-Index set|SEC2|SEC3",
      rda.ToString()
    ); //Raw encoded payload string
  }

  @Test
  public void RdaSetterGetterEscapingTest() throws Exception {
    // test ContainerHeader getters
    Rda rdaBase = Rda.Parse(
      "|;,\\|sec0|sec1|sec2"
    );
    System.out.printf(
      "rda-before-assigned-new-child-rda %s\n",
      rdaBase.ToStringFormatted()
    );
    Rda rdaSec1new = Rda.Parse(
      "&_;/&sec1-n0&sec1-n1 /||| ; X_ x&sec1-n2_A/;B"
    );
    System.out.printf("inserted rda[1] %s\n", rdaSec1new.ToStringFormatted());

    rdaBase.SetRda(1, rdaSec1new);
    System.out.printf(
      "rda-after-assigned-new-child-rda %s\n",
      rdaBase.ToStringFormatted()
    );
    assertEquals(
      "sec1-n0",
      rdaBase.GetRda(new int[] { 1, 0 }).GetScalarValue()
    ); //test the wrapper
    assertEquals("sec1-n0", rdaBase.GetRda(1).GetScalarValue()); //test the wrapper
    assertEquals(true, rdaSec1new.ContentEqual(rdaBase.GetRda(1)));
    assertEquals(
      "sec1-n1 /||| ",
      rdaBase.GetRda(new int[] { 1, 1, 0, 0 }).ToString()
    ); //
    assertEquals(" X", rdaBase.GetRda(new int[] { 1, 1, 0, 1 }).ToString()); //

    Rda rdaSec3new = Rda.Parse(
      ">;/>sec3>sec3-n1>a;b;c"
    );
    System.out.printf("inserted rda[3] %s\n", rdaSec3new.ToStringFormatted());
    rdaBase.AddRda(rdaSec3new);
    System.out.printf(
      "rda-after-adding-new-child-rda %s\n",
      rdaBase.ToStringFormatted()
    );
    assertEquals(true, rdaSec3new.ContentEqual(rdaBase.GetRda(3)));
  }

  @Test
  public void EscapedValueFormattingTest() {
    //double-quote as escape is supposed to be replaced internally, as double-quote is reserved for v2-formatting
    //string s1 = @"|;,""| AAA; xx""|"";"",x | B,b; C ";
    String quoteReplaced = "|;,^| AAA; xx^|^;^,x | B,b; C ";
    Rda rda1 = Rda.Parse(quoteReplaced);
    assertEquals(quoteReplaced, rda1.ToString());
    String v2FormattedRDA = rda1.ToStringFormatted();
    System.out.printf("v2-formatted %s\n", v2FormattedRDA);
    Rda rda2 = Rda.Parse(
      v2FormattedRDA
    );
    assertEquals(true, rda1.ContentEqual(rda2));
    assertEquals(quoteReplaced, rda2.ToString());

    String s2 = "|;,^| AAA; xx^|^;^,x | B,b; C ";
    Rda rda3 = Rda.Parse(s2);
    assertEquals(s2, rda3.ToString());
    String s2f = rda3.ToStringFormatted();
    System.out.printf("v2-formatted %s\n", s2f);
    Rda rda4 = Rda.Parse(s2f);
    assertEquals(s2, rda4.ToString());
  }
}
