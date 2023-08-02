// Copyright (c) 2020 Michael Chen
// Licensed under the MIT License -
// https://github.com/foldda/rda/blob/main/LICENSE

package UniversalDataTransport;

import java.util.ArrayList;
import java.util.List;

/*
 * UniversalDataTransport name-space is for unified data storage and transportation using the RDA data storage structure.
 */

/*
 * Recursive Delimited-Array (RDA) - a text-encoding format for composing in a single-String  "container" for storing multi-dimentional array values.
 * Part of the encoded String  (the "header section") contains the charactors ("delimiters" and escape-char) used for endoding and decoding, thus a receiving program can decode the
 * string and retrive the enclosed values on-the-fly, since the decoding envolves not 'sender-specific' configuration. This is in contrast to transporting data using XML, where
 * a schema must be estabilished and shared, and in comparison to using CSV, where the delimitor and qualifier chars must be pre-agreed and configured at both the sender and the receiver.
 *
 * Much like an "universal" plug or pipe used in plumbing that allows any appliances be connected together. RDA allows appliactions freely exchange data between each other without having to
 * configure the data-exchange each and every time.
 *
 * A generic universal parser (such as being implemented in this class) can be placed at both sending and receiving applictaions, and allow the appications to exhange data with
 * posibily complex structure without pre-established undertanding of the data that are being exchanged. As such, it could reduce or eliminate the need of using a middleware that is
 * typically required in Systems Integration, because of the use of an application-neutural data container for the data exchange.
 *
 */

public class Rda implements IRda {

  public static <T> T getValueOrDefault(T value, T defaultValue) {
    return value == null ? defaultValue : value;
  }

  public Rda ToRda() {
    return this;
  }

  public IRda FromRda(Rda rda) {
    if (rda.Dimension() == 0) {
      SetScalarValue(rda.GetScalarValue());
    } else {
      Elements.clear();
      Elements.addAll(rda.Elements);
    }

    return this;
  }

  /**
   * Base properties
   */

  //These are the RDA's data storage. Elements are for storing the "composite" content, when RDA's Dimension > 0
  public List<Rda> Elements = new ArrayList<Rda>();
  //"scalar" content is used when RDA's Dimension = 0
  private String _scalarValue = null;

  //for the whole RDA structure, encoding is fixed and shared amongs parent and Elements
  public Rda Parent; // the upper-level RDA of which this RDA is a child
  private RdaEncoding _encoding;

  // public RdaEncoding GlobalEncoding = Parent == null ? _encoding : Parent.GlobalEncoding;
  public RdaEncoding GlobalEncoding() {
    return Parent == null ? _encoding : Parent.GlobalEncoding();
  }

  /**
   * Constructors
   */
  public Rda(RdaEncoding encoding) {
    _encoding = encoding;
  }

  //use default encoding
  // public Rda() : this(new RdaEncoding()) { }
  public Rda() {
    _encoding = new RdaEncoding();
  }

  private Rda(Rda parent) {
    Parent = parent; //inherites parent's encoding
  }

  public static Rda Parse(String rdaString) {
    RdaEncoding encoding = GetHeaderSectionEncoder(rdaString);
    Rda rda = new Rda(encoding);
    if (encoding.Delimiters.length == 0) {
      rda.SetScalarValue(rdaString);
    } else {
      String payload = rdaString.substring(encoding.Delimiters.length + 2);
      rda.ParsePayload(
        payload,
        DetermineParsingFormatVersion(payload) == FORMATTING_VERSION.V2
      );
    }

    return rda;
  }

  /**
   * Derived properties from the "storage fields" and the encoding field
   */

  public String PayLoad() {
    return GetPayload(DelimitersInUse(), FORMATTING_VERSION.V1);
  }

  public String PayLoadV2() {
    return GetPayload(DelimitersInUse(), FORMATTING_VERSION.V2);
  }

  //it's the max-depth towards the bottom, it determines the number of delimiters required for encoding this RDA,
  public int Dimension() {
    //if (Elements.Count == 1) { return Elements[0].Dimension; }
    int maxChildDimemsion = -1;
    for (Rda c : Elements) {
      maxChildDimemsion = Math.max(maxChildDimemsion, c.Dimension());
    }

    return maxChildDimemsion + 1;
  }

  //the number of steps from the root Parent RDA
  //it's used as the index to Delimiters array for determing the next-level delimiter
  private int Level() {
    return Parent == null ? 0 : Parent.Level() + 1;
  }

  /**
   * API Properties and Methods - when using RDA as a storage container
   */

  //the client's 'string value' stored in this RDA.
  //For Dimension-0 (leaf) RDA, it's the stored scalar-value, for composite RDA (dimension > 0), it's the left-most child's scalar-value
  public String GetScalarValue() {
    return Elements.size() > 0
      ? Elements.get(0).GetScalarValue()
      : ((_scalarValue == null) ? "" : _scalarValue);
  }

  //sets the scalar-value, and clears Elements (making this a Dimension-0 rda)
  public void SetScalarValue(String value) {
    Elements.clear();
    _scalarValue = value;
  }

  //this rda's "string expression", i.e. a properly encoded RDA string with the header and the payload sections
  //NB, for Dimension-0 RDA, it outputs the stored scalar value (i.e. the header-section is an empty string in this case)
  public String ToString() {
    return Dimension() == 0
      ? GetScalarValue()
      : String.format(
        "%s%c%c%s",
        new String(DelimitersInUse()),
        EscapeChar(),
        DelimitersInUse()[0],
        PayLoad()
      );
  }

  public String toString() {
    return ToString();
  }

  //this rda's 'string expression, with version-2 formatting applied.
  //version-2 formatting uses redundant formatting chars such as white-space, line-breaks, and double-quotes in the payload's encoding
  public String ToStringFormatted() {
    return Dimension() == 0
      ? GetScalarValue()
      : String.format(
        "%s%c%c%s %s",
        new String(DelimitersInUse()),
        EscapeChar(),
        DelimitersInUse()[0],
        LINE_BREAK,
        PayLoadV2()
      );
  }

  //set a child RDA at the index'd location, extend the max index if required
  public void SetRda(int index, Rda childRda)
    throws Exception {
    EnsureArrayLength(index); //creates dummies if required

    if (childRda != null) {
      GlobalEncoding().ExtendDelimiters(Level() + childRda.Dimension() + 1); //throws Exception if limit is reached
      childRda.Parent = this;

      Elements.set(index, childRda);
    } else {
      GlobalEncoding().ExtendDelimiters(Level() + 1); //throws Exception if limit is reached
      Elements.set(index, new Rda(this)); //make a dummy
    }
  }

  //get a child RDA at the index'd location, return null if RDA is not allocated
  public Rda GetRda(int index) throws Exception {
    GlobalEncoding().ExtendDelimiters(Level() + 1); //throws Exception if limit is reached
    if (Dimension() == 0) {
      //push existing scalar value to become the left-most child's value
      Rda rda = new Rda(this);
      rda.SetScalarValue(_scalarValue);
      Elements.add(rda);
    }

    EnsureArrayLength(index); //creates dummies if required

    //the indexed child can be safely retrived
    return Elements.get(index);
  }

  //set a child RDA at the index'd location, extend the max index if required
  public void SetValue(int index, String value) throws Exception {
    Rda rda = new Rda();
    rda.SetScalarValue(value);
    SetRda(index, rda);
  }

  //get a child RDA at the index'd location, return null if RDA is not allocated
  public String GetValue(int index) throws Exception {
    return getValueOrDefault(GetRda(index).GetScalarValue(), "");
  }

  public Rda GetRda(int[] sectionIndexAddress)
    throws Exception {
    if (sectionIndexAddress == null || sectionIndexAddress.length == 0) {
      return this;
    } else {
      var child = GetRda(
        sectionIndexAddress[0]
      );/* this auto extends the # of dummy Elements at this level, if it's over indexed (unless it exceeds dimension limit */

      if (sectionIndexAddress.length == 1 || child == null) {
        return child;
      } else {
        int[] nextLevelSectionIndexAddress = new int[sectionIndexAddress.length -
        1];
        System.arraycopy(
          sectionIndexAddress,
          1,
          nextLevelSectionIndexAddress,
          0,
          nextLevelSectionIndexAddress.length
        );
        return child.GetRda(nextLevelSectionIndexAddress); //recursion, get next-level child with the remaining index-addresses
      }
    }
  }

  /// <summary>
  /// Sets the scalar value to the child rda addressed by the multi-dimension index array
  /// </summary>
  /// <param name="addressIndexArray"></param>
  /// <param name="newScalarValue"></param>
  public void SetValue(int[] addressIndexArray, String newScalarValue)
    throws Exception {
    var childRda = GetRda(addressIndexArray);
    if (childRda != null) {
      childRda.SetScalarValue(newScalarValue);
    }
  }

  public String GetValue(int[] addressIndexArray) throws Exception {
    var childRda = GetRda(addressIndexArray);

    return getValueOrDefault(childRda.GetScalarValue(), "");
  }

  public void AddValue(String valueString) throws Exception {
    SetValue(Elements.size(), valueString);
  }

  public void AddRda(Rda rda) throws Exception {
    SetRda(Elements.size(), rda);
  }

  public String[] GetElementsValueArray() {
    List<String> result = new ArrayList();
    if (Elements.size() == 0) {
      result.add(_scalarValue);
    } else {
      for (var child : Elements) {
        result.add(child.GetScalarValue());
      }
    }
    return (String[]) result.toArray();
  }

  public void SetElementsValueArray(String[] value) {
    Elements.clear();
    if (value == null || value.length == 0) {
      _scalarValue = null;
    } else {
      for (var s : value) {
        var child = new Rda(this);
        child.SetScalarValue(s);
        Elements.add(child);
      }
    }
  }

  public boolean ContentEqual(Rda other) {
    if (
      this.Dimension() != other.Dimension() || this.Length() != other.Length()
    ) {
      return false;
    } else if (this.Dimension() == 0) {
      return this.GetScalarValue().equals(other.GetScalarValue());
    } else {
      for (int i = 0; i < Length(); i++) {
        if (Elements.get(i).ContentEqual(other.Elements.get(i)) == false) {
          return false;
        }
      }
      return true;
    }
  }

  //remove unused delimiters in the header
  public String ToStringMinimal() {
    CompressDimension(); //remove unnecessary levels if a branch only has one leaf-node
    return ToString();
  }

  /* this is the end of the main API, below are helper methods */

  public char ChildDelimiter() {
    return GlobalEncoding().Delimiters[Level()];
  } //the char that separates the immediate Elements elements

  public char EscapeChar() {
    return GlobalEncoding().EscapeChar;
  }

  public int Length() {
    return Elements.size();
  }

  public char[] DelimitersInUse() {
    char[] subArray = new char[Dimension()];
    System.arraycopy(
      GlobalEncoding().Delimiters,
      Level(),
      subArray,
      0,
      Dimension()
    );
    return subArray;
  }

  private void ParsePayload(String payloadString, boolean v2Formatted) {
    Elements.clear();

    //apply maximun unescape to "string-value" before it's stored
    //this will be reversed (escaped) when the value is used for assembling a payload section.
    _scalarValue =
      UnEscape(
        payloadString,
        GlobalEncoding().Delimiters,
        EscapeChar(),
        v2Formatted
      );

    //... then continue to (recurrsively) parse the rda-encoded payload string ..

    //make sure the parsing doesn't go beyond the RDA-string "levels" limit (set by the encoding header section)
    if (Level() < GlobalEncoding().Delimiters.length) {
      List<String> sections = ParseElementsContentSections(payloadString);
      //if (sections.Count > 1)
      //{
      for (String childPayLoad : sections) {
        var child = new Rda(this);
        child.ParsePayload(childPayLoad, v2Formatted);

        Elements.add(child);
      }
    }
  }

  public void CompressDimension()
  {
      if (Dimension() > 0)
      {
          //compress all children (recursion)
          for (int i = 0; i < Elements.size(); i++) { 		      
            Elements.get(i).CompressDimension(); 		
          }   		

          //check... skips all the dummies from the end
          for(int i = Elements.size() - 1; i > 0; i--)
          {
              if (Elements.get(i).IsDummy() == false) 
              { 
                  return; /* no compression - if non-dummy child found before index 0 */
              }
          }

          //reduce the dimension if these is only one non-dummy child, and its dimension is 0,
          //... by bringing the child's scalar value up, which also deletes all children
          if (Elements.get(0).Dimension() == 0)
          {
              this.SetScalarValue(Elements.get(0).GetScalarValue());  
          }
      }
  }

  public enum FORMATTING_VERSION {
    V1(1),
    V2(2);

    private int numVal;

    FORMATTING_VERSION(int numVal) {
      this.numVal = numVal;
    }

    public int getNumVal() {
      return numVal;
    }
  }

  /* below are helper properties and methods */

  /// <summary>
  /// RDA is v2-formatted if the first line only contains the header section and trailing white-spaces.
  /// In v2-formatted RDA, leading/trailing spaces and line-breakes are for formatting and are not considered as part of the element's string value.
  /// </summary>
  private static FORMATTING_VERSION DetermineParsingFormatVersion(
    String payloadString
  ) {
    char[] valueCharArray = payloadString.toCharArray();

    for (int i = 0; i < valueCharArray.length; i++) {
      char currChar = valueCharArray[i];
      if (
        !Character.isWhitespace(currChar)
      ) /* any non-white-spcae before EOL indicating it is not v2 formatted */{
        return FORMATTING_VERSION.V1;
      } else if (
        currChar == '\n'
      ) /* this EOL indicating previous chars are all white-spaces, so it is v2 formatted */{
        return FORMATTING_VERSION.V2;
      }
    }

    return FORMATTING_VERSION.V1; //valueCharArray are all white-space chars
  }

  //In RDA encoded char array (string) the first char is the 1st-level-array-delimiter, and the first repeat of the 1st-level-array-delimiter in
  //the following chars marks the end of the encoder section, which is then follwed by the "payload" section that incldues the remainder of the RDA char array.
  //the mandatory escape-char is the second-last char of the encoder section (before the first repeat of the 1st-level-array-delimiter).
  //Thus a minimal RDA encoder section must have at least 3-chars long. In addtion, encoder chars (delimiters and escape-char) in the encoder section
  //must be not-white-space, printable (not control-chars), and non-alphanumeric, plus the double-quote char is reserved (for enclosing leading/trailing spaces in v2-formatted rda strings)
  private static RdaEncoding GetHeaderSectionEncoder(String rdaString) {
    //   if(String.isnull(rdaString) == false)
    if (!(rdaString == null || rdaString.equals(""))) {
      char[] valueCharArray = rdaString.toCharArray();
      for (int i = 0; i < valueCharArray.length; i++) {
        char currChar = valueCharArray[i];

        //NB, this check is not part of the RDA Specification, it adds "editor friendlyness" but introduces restriction meaning less available delimiters options.
        //this is required for v2-formatting where leading/trailing whites-space/control-char/double-quote are ignored in parsing
        //also preferrable disallow char.IsLetterOrDigit(currChar)
        if (
          Character.isWhitespace(currChar) ||
          Character.isISOControl(currChar) ||
          /* non-printable */RdaEncoding.DOUBLE_QUOTE == currChar
        ) {
          break; //invalid delimiter char
        }

        //else
        if (RangeContains(valueCharArray, 0, i, currChar)) {
          if (currChar == valueCharArray[0] && i > 1) {
            // repeat of the primary delimiter found, construct the encoder from the header
            int headerSectionEndIndex = i;
            var delimiters = new char[headerSectionEndIndex - 1];
            System.arraycopy(
              valueCharArray,
              0,
              delimiters,
              0,
              headerSectionEndIndex - 1
            );
            return new RdaEncoding(
              delimiters,
              valueCharArray[headerSectionEndIndex - 1]
            );
          } else {
            break; //invalid repeat in header
          }
        }
      }
    }
    return new RdaEncoding();
  }

  //helper -tests if the source array contains a targeted char in the given range
  static boolean RangeContains(
    char[] sourceCharArray,
    int rangeStartIndex,
    int rangeEndIndex,
    char targetChar
  ) {
    for (int range = rangeStartIndex; range < rangeEndIndex; range++) {
      if (sourceCharArray[range] == targetChar) return true;
    }
    return false;
  }

  static String UnEscape(
    String payloadString,
    char[] delimiters,
    char escapeChar,
    boolean v2Formatted
  ) {
    //no escaping is required if string is too short
    if (payloadString == null || payloadString.equals("")) {
      return payloadString;
    } else if (payloadString.trim().length() < 2) {
      return !v2Formatted ? payloadString : payloadString.trim();
    }
    //for v2-formatted RDA, remove the starting/ending double-quote-char (maximun one only) if it presents
    //double-quote-char is used (in v2-formatted RDA) for enclosing leading and trailing spaces in string value
    char[] valueChars;
    if (v2Formatted) {
      payloadString = payloadString.trim();
      valueChars = payloadString.toCharArray();
      int firstCharIndex = 0, lastCharIndex = valueChars.length - 1;
      if (valueChars[firstCharIndex] == RdaEncoding.DOUBLE_QUOTE) {
        firstCharIndex++;
      }
      if (valueChars[lastCharIndex] == RdaEncoding.DOUBLE_QUOTE) {
        lastCharIndex--;
      }

      char[] trimmedChars = new char[lastCharIndex - firstCharIndex + 1];
      if (trimmedChars.length == 0) {
        return "";
      }
      System.arraycopy(
        valueChars,
        firstCharIndex,
        trimmedChars,
        0,
        trimmedChars.length
      );
      valueChars = trimmedChars;
    } else {
      valueChars = payloadString.toCharArray();
    }

    //now do the un-escaping
    StringBuilder unescaped = new StringBuilder();
    boolean escaping = false;
    for (int i = 0; i < valueChars.length - 1; i++) {
      char currentChar = valueChars[i];
      if (currentChar == escapeChar) {
        escaping = !escaping;
      } else {
        escaping = false;
      }

      char nextChar = valueChars[i + 1];
      if (
        escaping &&
        (
          nextChar == escapeChar ||
          RangeContains(delimiters, 0, delimiters.length, nextChar)
        )
      ) {
        continue; //skip current char
      }
      unescaped.append(currentChar);
    }
    unescaped.append(valueChars[valueChars.length - 1]);

    return unescaped.toString(); //un-escaped section value
  }

  static String LINE_BREAK = "\r\n";
  static String INDENT = new String(new char[] { ' ', ' ' });

  String Indent() {
    if (Parent == null || Parent.Elements.size() == 1) {
      return "";
    } else {
      return Parent.Indent() + INDENT;
    }
  }

  //payload = <delimitor at this level> + concatenated Elements payloads (recurrsion)
  private String GetPayload(
    char[] delimiterChars,
    FORMATTING_VERSION formattingVersion
  ) {
    boolean applyFormatting = (formattingVersion == FORMATTING_VERSION.V2);
    StringBuilder result = new StringBuilder();

    if (LastNonDummyIndex() < 0) {
      //apply escaping to the unescaped value (the stored "real/original" value) when it becomes part of a payload
      var escaped = getValueOrDefault(
        Escape(_scalarValue, delimiterChars, EscapeChar(), applyFormatting),
        ""
      );
      result.append(escaped.toString());
    } else {
      for (int i = 0; i <= LastNonDummyIndex(); i++) {
        var child = Elements.get(i);
        if (applyFormatting) {
          result.append(GetFormattingPrefix(i)); //TODO replace the below.
        }

        //recurrsion ...
        result.append(i == 0 ? "" : String.valueOf(ChildDelimiter()));
        result.append(child.GetPayload(delimiterChars, formattingVersion));
      }
    }

    return result.toString();
  }

  private String GetFormattingPrefix(int index) {
    //if this is the first child ...
    if (index == 0) {
      return Elements.size() > 1 && Parent != null ? INDENT : "";
    } else {
      return LINE_BREAK + Indent();
    }
  }

  //dummy child is 'place-holder' that is created when accessor 'over-indexed' the RDA existing values
  boolean IsDummy() {
    if (Elements.size() == 0) {
      return _scalarValue == null;
    } else {
      //else it's a dummy if all Elements are dummy
      for (var child : Elements) {
        if (child.IsDummy() == false) {
          return false;
        }
      }

      return true;
    }
  }

  int LastNonDummyIndex() {
    int lastNonDummyIndex = Elements.size() - 1;
    while (
      lastNonDummyIndex >= 0 &&
      Elements.get(lastNonDummyIndex).IsDummy() == true
    ) {
      lastNonDummyIndex--;
    }
    return lastNonDummyIndex;
  }

  private void EnsureArrayLength(int index) {
    //1. turns a "leaf" node to a "composite" node - that is, a node that have Elements that can be indexed.
    //if (Elements.Count == 0)
    //{
    //    //push original_value down to become the 1st child
    //    Elements.Add(new Rda(null /* null payload */, this) { Content = _unescapedValueExpression });
    //}

    //2. extend the Elements elements if over-indexing is required
    int diff = index - Elements.size() + 1;

    while (diff > 0) {
      var dummy = new Rda(this);/*dummy*/
      Elements.add(dummy);
      diff--;
    }
  }

  //helper - splits a (parent's) payload String  into child-content sections, implements the escaping logic
  private List<String> ParseElementsContentSections(String parentPayLoad) {
    List<String> result = new ArrayList();
    if (parentPayLoad == null || parentPayLoad.equals("")) {
      result.add(parentPayLoad);
      return result;
    }

    boolean escaping = false;
    int childSectionStartIndex = 0;
    char[] valueCharArray = parentPayLoad.toCharArray();
    for (
      int currCharIndex = childSectionStartIndex;
      currCharIndex < valueCharArray.length;
      currCharIndex++
    ) {
      char currChar = valueCharArray[currCharIndex];
      if (currChar == EscapeChar()) {
        escaping = !escaping; //note it flips when escape-char is hit again
        continue;
      } else if (!escaping && currChar == ChildDelimiter()) {
        int sectionLength = currCharIndex - childSectionStartIndex;
        String childPayload = new String(
          valueCharArray,
          childSectionStartIndex,
          sectionLength
        );
        result.add(childPayload);

        childSectionStartIndex = currCharIndex + 1; //next section start position
      }
      escaping = false;
    }

    //get the last token, that is, all chars after the last-encountered separator-char
    if (childSectionStartIndex < valueCharArray.length) {
      String lastSectionValue = new String(
        valueCharArray,
        childSectionStartIndex,
        valueCharArray.length - childSectionStartIndex
      );
      result.add(lastSectionValue);
    }

    return result;
  }

  /* "Escaping" Definition: to remove any "special meaning" of the next following char, ie. keeps its original meaning. */

  //helper: used for parsing a section-value, that may conatins delimiters chars and/or escape char, from an encoded RDA String
  //returns the actual value that needs to be stored
  private static String Escape(
    String elementValue,
    char[] delimitersInUse,
    char escapeChar,
    boolean applyFormatting
  ) {
    if (elementValue == null) {
      return elementValue;
    }

    StringBuilder escaped = new StringBuilder();
    for (char c : elementValue.toCharArray()) {
      //insert escape char if required
      if (escapeChar == c) {
        escaped.append(escapeChar);
      } else {
        for (char delimiter : delimitersInUse) {
          if (delimiter == c) {
            escaped.append(escapeChar);
            break;
          }
        }
      }

      escaped.append(c);
    }

    //for v2-formatting, add double-quotes around the content
    if (applyFormatting) {
      escaped.insert(0, RdaEncoding.DOUBLE_QUOTE);
      escaped.append(RdaEncoding.DOUBLE_QUOTE);
    }

    return escaped.toString();
  }

  public static class RdaEncoding {

    //in RDA spec, delimiters that are restricted to printable, non-white-space (and preferably non-alpha-numeric) chars.
    public static final char[] DEFAULT_DELIMITER_CHARS = new char[] {
      '|',
      ';',
      ',',
      '^',
      ':',
      '~',
      '$',
      '&',
      '#',
      '=',
      '*',
      '.',
      '\'',
      '@',
      '_',
      '%',
      '/',
      '!',
      '?',
      '>',
      '<',
      '+',
      '-',
      '{',
      '}',
      '[',
      ']',
      '(',
      ')',
      '`',
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
    };

    public static final char DEFAULT_ESCAPE_CHAR = '\\';

    public static final char DOUBLE_QUOTE = '"';
    public char[] Delimiters = new char[] {}; // (char[])DEFAULT_DELIMITER_CHARS.Clone();
    char EscapeChar = DEFAULT_ESCAPE_CHAR;

    protected RdaEncoding(char[] customDelimiters, char escapeChar) { //, bool v2Formatted)
      Delimiters = customDelimiters;
      EscapeChar = escapeChar;
    }

    //returns if extension successful
    //Level > Global.RootParentDimensionLimit - 1
    void ExtendDelimiters(int newLevel) throws Exception {
      if (newLevel <= Delimiters.length) {
        return;
      } else if (newLevel < DEFAULT_DELIMITER_CHARS.length) {
        char[] newLevelDelimiters = new char[newLevel];
        System.arraycopy(
          Delimiters,
          0,
          newLevelDelimiters,
          0,
          Delimiters.length
        );
        int existingRangeIndex = Delimiters.length;
        for (char candidateDelimiterChar : DEFAULT_DELIMITER_CHARS) {
          //make sure the candidate char isn't already in the delimiters range
          if (
            !RangeContains(
              newLevelDelimiters,
              0,
              existingRangeIndex,
              candidateDelimiterChar
            )
          ) {
            newLevelDelimiters[existingRangeIndex++] = candidateDelimiterChar;
            if (existingRangeIndex == newLevelDelimiters.length) {
              Delimiters = newLevelDelimiters; //copy the extended dimiters to
              return;
            }
          }
        }
      }

      throw new Exception(
        String.format(
          "Maximun RDA dimension-limit (%d) reached, no child RDA can be accepted.",
          newLevel
        )
      );
    }

    public RdaEncoding() {}
  }
}
