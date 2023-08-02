// Copyright (c) 2020 Michael Chen
// Licensed under the MIT License -
// https://github.com/foldda/rda/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Text;

/*
 * Charian name-space is for unified data storage and transportation using the RDA data storage structure. 
 */

namespace Charian
{
    /*
     * Recursive Delimited Array (RDA) is a simplified and flexible text-encoding format for encoding and storing data in a text string. 
     * 
     * An RDA-encoded string comprises two substring sections: a "header" section which contains the RDA-encoding characters and 
     * a "payload" section which contains the encoded data elements. The RDA-encoding characters include a number of delimiter characters and 
     * an escape character, 
     * 
     * The Rda class is a RDA parser/encoder reference implementation. An Rda object can be used as a 
     * generic data container using the provided API to store generic data elements. The container and its stored data can be seriallized into 
     * an encoded string, which can then be written to a file, be sent through a computer network, or be used for exchanging data between 
     * integrated computer applications.
     * 
     * "Recursive Delimited Array" and "RDA" are trademarks of Foldda Pty Ltd (an Australian company)
     * 
     */

    public class Rda : IRda
    {
        /**
         * Base properties
         */

        //ScalarValue and Children are the base of an RDA's internal data structure.
        //"scalar" content is used when RDA's Dimension = 0, it's the unescaped 'original' value that is independent to the encoding chars.
        private string _scalarValue = null;
        //Children are for storing the "composite" content, when RDA's Dimension > 0, where each child is a sub-RDA
        public List<Rda> Elements { get; } = new List<Rda>();
        public Rda Home { get; private set; } // the upper-level RDA of which this RDA is a child

        //for the whole RDA structure, encoding is fixed and shared amongs parent and children
        private RdaEncoding _encoding;
        public RdaEncoding GlobalEncoding => Home == null ? _encoding : Home.GlobalEncoding;   

        /**
         * Constructors
         */
        public Rda(RdaEncoding encoding)
        {
            _encoding = encoding;
        }

        //use default encoding
        public Rda() : this(new RdaEncoding()) { }

        private Rda(Rda parent) 
        {
            Home = parent;    //inherites parent's encoding
        }

        public static Rda Parse(string rdaString)
        {
            RdaEncoding encoding = GetHeaderSectionEncoder(rdaString);
            Rda rda = new Rda(encoding);
            if(encoding.Delimiters.Length == 0)
            {
                rda.ScalarValue = rdaString;
            }
            else
            {
                string payload = rdaString.Substring(encoding.Delimiters.Length + 2);
                rda.ParsePayload(payload, DetermineParsingFormatVersion(payload) == FORMATTING_VERSION.V2);
            }

            return rda;
        }

        /**
         * Derived properties from the "storage fields" and the encoding field
         */

        public string PayLoad => GetPayload(DelimitersInUse, FORMATTING_VERSION.V1);
        public string PayLoadV2 => GetPayload(DelimitersInUse, FORMATTING_VERSION.V2);

        //it's the max-depth towards the bottom, it determines the number of delimiters required for encoding this RDA, 
        public int Dimension
        {
            get
            {
                //if (Children.Count == 1) { return Children[0].Dimension; }
                int maxChildDimemsion = -1;
                foreach (var c in Elements)
                {
                    maxChildDimemsion = Math.Max(maxChildDimemsion, c.Dimension);
                }

                return maxChildDimemsion + 1;
            }
        }


        //the number of steps from the root Parent RDA
        //it's used as the index to Delimiters array for determing the next-level delimiter
        private int Level => Home == null ? 0 : Home.Level + 1;

        /**
         * API Properties and Methods - when using RDA as a storage container
         */

        //the client's 'string value' stored in this RDA. 
        public string ScalarValue 
        { 
            //For Dimension-0 (leaf) RDA, it's the stored scalar-value, for composite RDA (dimension > 0), it's the left-most child's scalar-value
            get => Elements.Count > 0 ? Elements[0].ScalarValue : _scalarValue ?? string.Empty;

            //sets the scalar-value, and clears children (making this RDA as Dimension-0)
            set
            {
                Elements.Clear();
                _scalarValue = value;
            }
        }

        //this rda's "string expression", i.e. a properly encoded RDA string with the header and the payload sections
        //NB, for Dimension-0 RDA, it outputs the stored scalar value (i.e. the header-section is an empty string for Dimension-0 RDA)
        public override string ToString()
        {
            return Dimension == 0 ? ScalarValue : $"{new string(DelimitersInUse)}{EscapeChar}{DelimitersInUse[0]}{PayLoad}";
        }

        //this rda's 'string expression', with version-2 formatting applied.
        //version-2 formatting uses redundant formatting chars such as white-space, line-breaks, and double-quotes in the payload's encoding
        public string ToStringFormatted()
        {
            return Dimension == 0 ? ScalarValue : $"{new string(DelimitersInUse)}{EscapeChar}{DelimitersInUse[0]}{LINE_BREAK} {PayLoadV2}";
        }


        //set a child RDA at the index'd location, extend the max index if required 
        public void SetRda(int index, Rda childRda)
        {
            EnsureArrayLength(index);   //make sure the addressed position exists, create dummies if required

            if (childRda != null)
            {
                GlobalEncoding.TryExtendRequiredDelimiters(Level + childRda.Dimension + 1); //throws Exception if limit is reached
                childRda.Home = this;

                Elements[index] = childRda; //set or replace the child at the addressed position
            }
            else
            {
                GlobalEncoding.TryExtendRequiredDelimiters(Level + 1);  //throws Exception if limit is reached
                Elements[index] = new Rda(this);    //make a dummy
            }
        }

        //get a child RDA at the index'd location, return null if RDA is not allocated 
        //NB: by indexing over an RDA having no children (i.e. Dim-0) would automatically increases its dimension
        public Rda GetRda(int index)
        {
            GlobalEncoding.TryExtendRequiredDelimiters(Level + 1);  //throws Exception if limit is reached
            
            if(Dimension == 0)
            {
                //push this RDA's scalar-value to become the left-most child's value
                Elements.Add(new Rda(this) { ScalarValue = _scalarValue }); ;
            }

            EnsureArrayLength(index);   //creates dummies if required

            //the indexed child can be safely retrived
            return Elements[index];
        }

        //set a child RDA at the index'd location, extend the max index if required 
        public void SetValue(int index, string value)
        {
            Rda rda = new Rda() { ScalarValue = value };
            SetRda(index, rda);
        }

        //get a child RDA at the index'd location, return null if RDA is not allocated 
        public string GetValue(int index)
        {
            return GetRda(index)?.ScalarValue ?? String.Empty;
        }

        public Rda GetRda(int[] sectionIndexAddress)
        {
            if (sectionIndexAddress == null || sectionIndexAddress.Length == 0)
            {
                return this;
            }
            else
            {
                var child = GetRda(sectionIndexAddress[0]);   /* this auto extends the # of dummy children at this level, if it's over indexed (unless it exceeds dimension limit */

                if (sectionIndexAddress.Length == 1 || child == null)
                {
                    return child;
                }
                else
                {
                    int[] nextLevelSectionIndexAddress = new int[sectionIndexAddress.Length - 1];
                    Array.Copy(sectionIndexAddress, 1, nextLevelSectionIndexAddress, 0, nextLevelSectionIndexAddress.Length);
                    return child.GetRda(nextLevelSectionIndexAddress);  //recurrsion, get next-level child with the remaining index-addresses
                }
            }
        }

        /// <summary>
        /// Sets the scalar value to the child rda addressed by the multi-dimension index array
        /// </summary>
        /// <param name="addressIndexArray"></param>
        /// <param name="newScalarValue"></param>
        public void SetValue(int[] addressIndexArray, string newScalarValue)
        {
            var childRda = GetRda(addressIndexArray);
            if (childRda != null) { childRda.ScalarValue = newScalarValue; }
        }

        public string GetValue(int[] addressIndexArray)
        {
            var childRda = GetRda(addressIndexArray);
            return childRda?.ScalarValue ?? string.Empty;
        }


        public void AddValue(string valueString)
        {
            SetValue(Elements.Count, valueString);
        }

        public void AddRda(Rda rda)
        {
            SetRda(Elements.Count, rda);
        }

        public string[] ChildrenValueArray
        {
            get
            {
                List<string> result = new List<string>();
                if (Elements.Count == 0)
                {
                    result.Add(_scalarValue);
                }
                else
                {
                    foreach (var child in Elements)
                    {
                        result.Add(child.ScalarValue);
                    }
                }
                return result.ToArray();
            }

            set
            {
                Elements.Clear();
                if (value == null || value.Length == 0)
                {
                    _scalarValue = null;
                }
                else
                {
                    foreach (var s in value)
                    {
                        var child = new Rda(this) { ScalarValue = s };
                        Elements.Add(child);
                    }
                }
            }
        }

        public bool ContentEqual(Rda other)
        {
            if (this.Dimension != other.Dimension || this.Length != other.Length)
            {
                return false;
            }
            else if (this.Dimension == 0)
            {
                return this.ScalarValue.Equals(other.ScalarValue);
            }
            else
            {
                for (int i = 0; i < Length; i++)
                {
                    if (Elements[i].ContentEqual(other.Elements[i]) == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /* this is the end of the main API, below are helper methods */

        public char ChildDelimiter => GlobalEncoding.Delimiters[Level];  //the char that separates the immediate children elements
        public char EscapeChar => GlobalEncoding.EscapeChar;

        public int Length => Elements.Count;

        public char[] DelimitersInUse
        {
            get
            {
                char[] subArray = new char[Dimension];
                Array.Copy(GlobalEncoding.Delimiters, Level, subArray, 0, Dimension);
                return subArray;
            }
        }

        //decode the delimited values in payload and apply unescaping to restore the 'original' value
        //and store these unescaped values to the scalar_value variable of an rda
        private void ParsePayload(string payloadString, bool v2Formatted)
        {
            Elements.Clear();

            //apply maximun unescape to "string-value" before it's stored
            //this will be reversed (escaped) when the value is used for assembling a payload section.
            _scalarValue = UnEscape(payloadString, GlobalEncoding.Delimiters, EscapeChar, v2Formatted);

            //... then continue to (recurrsively) parse the rda-encoded payload string ..

            //make sure the parsing doesn't go beyond the RDA-string "levels" limit (set by the encoding header section)
            if (Level < GlobalEncoding.Delimiters.Length)
            {
                var sections = ParseChildrenContentSections(payloadString);
 
                foreach (string childPayLoad in sections)
                {
                    var child = new Rda(this);
                    child.ParsePayload(childPayLoad, v2Formatted); 

                    Elements.Add(child);
                }
            }
        }

        //shrink (shortening) each branch to its minimun required dimension
        public void CompressDimension()
        {
            if (Dimension > 0)
            {
                //compress all children (recursion)
                foreach (var element in Elements)
                {
                    element.CompressDimension();
                }

                //check... skips all the dummies from the end
                for(int i = Elements.Count - 1; i > 0; i--)
                {
                    if (Elements[i].IsDummy == false) 
                    { 
                        return; /* no compression - if non-dummy child found before index 0 */
                    }
                }

                //reduce the dimension if these is only one non-dummy child, and its dimension is 0,
                //... by bringing the child's scalar value up, which also deletes all children
                if (Elements[0].Dimension == 0)
                {
                    this.ScalarValue = Elements[0].ScalarValue;  
                }
            }
        }

        //this indexing syntax is used in C#, it utilizes the GetRda/SetRda API methods above, 
        public Rda this[int index]
        {
            get => GetRda(index);
            set => SetRda(index, value);
        }

        //instead of use multi-dimemsional int[i]..[n] accessor to a child-section, these API shortcuts allow using 1-D int[] for accessing stored elements.
        public Rda this[int i, int j] => this.GetRda(new int[] { i, j });
        public Rda this[int i, int j, int k] => this.GetRda(new int[] { i, j, k });
        public Rda this[int i, int j, int k, int l] => this.GetRda(new int[] { i, j, k, l });
        public Rda this[int i, int j, int k, int l, int m] => this.GetRda(new int[] { i, j, k, l, m });
        public Rda this[int i, int j, int k, int l, int m, int n] => this.GetRda(new int[] { i, j, k, l, m, n });

        public enum FORMATTING_VERSION : int { V1 = 1, V2 = 2};     /* V0 = 0, not defined */

        /* below are helper properties and methods */

        /// <summary>
        /// RDA is v2-formatted if the first line only contains the header section and trailing white-spaces.
        /// In v2-formatted RDA, leading/trailing spaces and line-breakes are for formatting and are not considered as part of the element's string value.
        /// </summary>
        private static FORMATTING_VERSION DetermineParsingFormatVersion(string payloadString)
        {
            char[] valueCharArray = payloadString.ToCharArray();

            for (int i= 0; i < valueCharArray.Length; i++)
            {
                char currChar = valueCharArray[i];
                if (!char.IsWhiteSpace(currChar)) /* any non-white-spcae before EOL indicating it is not v2 formatted */
                {
                    return FORMATTING_VERSION.V1;
                }
                else if (currChar == '\n') /* this EOL indicating previous chars are all white-spaces, so it is v2 formatted */
                {
                    return FORMATTING_VERSION.V2;
                }            
            }

            return FORMATTING_VERSION.V1;   //valueCharArray are all white-space chars
        }

        /// <summary>
        /// Locate the header section in an RDA-encoded string, and retrive the RDA-encoding characters. An RDA's header section is from the start 
        /// of the RDA string to the first repeat of the first letter of the RDA string, inclusive. In the header section, the second-last character is the escape-char,
        /// and all characters before the escape-char are the delimiter chars. Thus a RDA header must be at minimun 3-letters long.
        /// 
        /// In addtion, in a valid RDA header, the delimiter chars and the escape char must be distinct to each other, 
        /// and these characters must be non-white-space (including line-break chars) and must be printable (i.e. no control-chars).
        /// </summary>
        /// <param name="rdaString">An RDA-encoded string</param>
        /// <returns>An RdaEncoding object that contains the delimiters and the escape-char</returns>
        private static RdaEncoding GetHeaderSectionEncoder(string rdaString)
        {
            //check if string is too short
            if (string.IsNullOrEmpty(rdaString) || rdaString.Length < 3) { new RdaEncoding(); }

            char[] valueCharArray = rdaString.ToCharArray();
            for (int i = 0; i < valueCharArray.Length; i++)
            {
                char currChar = valueCharArray[i];

                //check for invalid delimiter char, it is required for v2-formatting where leading/trailing whites-space/control-char/double-quote
                //are ignored in the parsing, also preferrable to dis-allow char.IsLetterOrDigit(currChar)
                //NB, these "restrictions" are to improve formatting "visualy" but make less number of delimiters chars that can be used.
                if (char.IsWhiteSpace(currChar) || char.IsControl(currChar) /* non-printable */ || RdaEncoding.DOUBLE_QUOTE == currChar)
                {
                    break;   //invalid header found, use default
                }

                //else 
                if (RangeContains(valueCharArray, 0, i, currChar))
                {
                    if(currChar== valueCharArray[0] && i > 1)
                    {
                        // repeat of the primary delimiter found, construct the encoder from the header
                        int headerSectionEndIndex = i;   
                        var delimiters = new char[headerSectionEndIndex - 1];
                        Array.Copy(valueCharArray, delimiters, headerSectionEndIndex - 1);
                        return new RdaEncoding(delimiters, valueCharArray[headerSectionEndIndex - 1]);
                    }
                    else
                    {
                        break;  //invalid repeat in header, use default
                    }
                }
            }

            //this RDA string has no valid header section, and is a dimenison-0 RDA
            return new RdaEncoding(); 
        }

        //helper - test if the source array contains a targeted char in the given range
        static bool RangeContains(char[] sourceCharArray, int rangeStartIndex, int rangeEndIndex, char targetChar)
        {
            for(int rangeIndex = rangeStartIndex; rangeIndex < rangeEndIndex; rangeIndex++)
            {
                if (sourceCharArray[rangeIndex] == targetChar) return true;
            }
            return false;
        }

        static string UnEscape(string payloadString, char[] delimiters, char escapeChar, bool v2Formatted)
        {
            //no escaping is required if string is too short
            if(string.IsNullOrEmpty(payloadString))
            {
                return payloadString;
            } 
            else if(payloadString.Trim().Length < 2)
            {
                return !v2Formatted ? payloadString : payloadString.Trim();  
            }
            
            //for v2-formatted RDA, remove the starting/ending double-quote-char (maximun one only) if it presents
            //double-quote-char is used (in v2-formatted RDA) for enclosing leading and trailing spaces in string value 
            char[] valueChars;
            if (v2Formatted)
            {
                payloadString = payloadString.Trim();
                valueChars = payloadString.ToCharArray();
                int firstCharIndex = 0, lastCharIndex = valueChars.Length - 1;
                if (valueChars[firstCharIndex] == RdaEncoding.DOUBLE_QUOTE) { firstCharIndex++; }
                if (valueChars[lastCharIndex] == RdaEncoding.DOUBLE_QUOTE) { lastCharIndex--; }

                char[] trimmedChars = new char[lastCharIndex - firstCharIndex + 1];
                if(trimmedChars.Length == 0) { return string.Empty; }
                Array.Copy(valueChars, firstCharIndex, trimmedChars, 0, trimmedChars.Length);
                valueChars = trimmedChars;
            }
            else
            {
                valueChars = payloadString.ToCharArray();
            }

            //now do the un-escaping
            StringBuilder unescaped = new StringBuilder();
            bool escaping = false;
            for(int i = 0; i < valueChars.Length-1; i++)
            {
                char currentChar = valueChars[i];  
                if(currentChar == escapeChar)
                {
                    escaping = !escaping;
                }
                else
                {
                    escaping = false;
                }

                char nextChar = valueChars[i+1];
                if (escaping && (nextChar == escapeChar || RangeContains(delimiters, 0, delimiters.Length, nextChar)))
                {
                    continue;   //skip current char
                } 
                unescaped.Append(currentChar);
            }
            unescaped.Append(valueChars[valueChars.Length - 1]);

            return unescaped.ToString();   //un-escaped section value
        }

        const string LINE_BREAK = "\r\n";
        static readonly string INDENT = new string(new char[] { ' ', ' ' });

        string Indent
        {
            get
            {
                if (Home == null || Home.Elements.Count == 1) { return string.Empty; }
                else { return Home.Indent + INDENT; }
            }
        }

        //payload = <delimitor at this level> + concatenated children payloads (recurrsion)
        //NB, payload string contains escaping to the stored scalar values
        private string GetPayload(char[] delimiterChars, FORMATTING_VERSION formattingVersion )
        {
            bool applyFormatting = (formattingVersion == FORMATTING_VERSION.V2);
            StringBuilder result = new StringBuilder();

            if (LastNonDummyIndex < 0)
            {
                //apply escaping to the unescaped value (the stored "real/original" value) when it becomes part of a payload
                result.Append(Escape(_scalarValue, delimiterChars, EscapeChar, applyFormatting) ?? string.Empty).ToString();
            }
            else
            {
                for (int i = 0; i <= LastNonDummyIndex; i++)
                {
                    var child = Elements[i];
                    if (applyFormatting)
                    {
                        result.Append(GetFormattingPrefix(i)); //TODO replace the below.
                    }

                    //recurrsion ...
                    result.Append(i == 0? string.Empty : ChildDelimiter.ToString())
                        .Append(child.GetPayload(delimiterChars, formattingVersion));
                }
            }

            return result.ToString();
        }

        private string GetFormattingPrefix(int index)
        {
            //if this is the first child ...
            if(index == 0)
            {
                return Elements.Count > 1 && Home != null ? INDENT : string.Empty;
            }
            else
            {
                return LINE_BREAK + Indent; 
            }
        }

        //dummy child is 'place-holder' that is created when accessor 'over-indexed' the RDA existing values
        public bool IsDummy
        {
            get
            {
                if (Elements.Count == 0) 
                { 
                    return _scalarValue == null; 
                }
                else
                {
                    //else it's a dummy if all children are dummy
                    foreach (var child in Elements)
                    {
                        if (child.IsDummy == false) { return false; }
                    }

                    return true;
                }
            }
        }

        int LastNonDummyIndex
        {
            get
            {
                int lastNonDummyIndex = Elements.Count - 1;
                while (lastNonDummyIndex >= 0 && Elements[lastNonDummyIndex].IsDummy == true)
                {
                    lastNonDummyIndex--;
                }
                return lastNonDummyIndex;
            }
        }

        private void EnsureArrayLength(int index)
        {
            //1. turns a "leaf" node to a "composite" node - that is, a node that have children that can be indexed.
            //if (Children.Count == 0)
            //{
            //    //push original_value down to become the 1st child
            //    Children.Add(new RecursiveDelimitedArray(null /* null payload */, this) { Content = _unescapedValueExpression });
            //}

            //extend the children elements if over-indexing is required
            int diff = index - Elements.Count + 1;

            while (diff > 0)
            {
                var dummy = new Rda(this);/*dummy*/
                Elements.Add(dummy);
                diff--;
            }
        }

        //helper - splits a (parent's) payload string into child-content sections, implements the escaping logic
        private List<string> ParseChildrenContentSections(string parentPayLoad)
        {
            List<string> result = new List<string>();
            if (string.IsNullOrEmpty(parentPayLoad))
            {
                result.Add(parentPayLoad);
                return result;
            }

            bool escaping = false;
            int childSectionStartIndex = 0;
            char[] valueCharArray = parentPayLoad.ToCharArray();
            for (int currCharIndex = childSectionStartIndex; currCharIndex < valueCharArray.Length; currCharIndex++)
            {
                char currChar = valueCharArray[currCharIndex];
                if(currChar == EscapeChar)
                {
                    escaping = !escaping;  //note it flips when escape-char is hit again
                    continue;
                }
                else if(!escaping && currChar == ChildDelimiter)
                {
                    int sectionLength = currCharIndex - childSectionStartIndex;
                    string childPayload = new string(valueCharArray, childSectionStartIndex, sectionLength);
                    result.Add(childPayload);

                    childSectionStartIndex = currCharIndex + 1;    //next section start position
                }
                escaping = false;
            }

            //get the last token, that is, all chars after the last-encountered separator-char
            if (childSectionStartIndex < valueCharArray.Length)
            {
                string lastSectionValue =
                    new string(valueCharArray, childSectionStartIndex, valueCharArray.Length - childSectionStartIndex);
                result.Add(lastSectionValue);
            }

            return result;
        }

        /* "Escaping" Definition: to remove any "special meaning" of the next following char, ie. keeps its original meaning. */

        //helper: used for parsing a section-value, that may conatins delimiters chars and/or escape char, from an encoded RDA string
        //returns the actual value that needs to be stored 
        private static string Escape(string elementValue, char[] delimitersInUse, char escapeChar, bool applyFormatting)
        {
            if (elementValue== null) { return elementValue; }

            StringBuilder escaped = new StringBuilder();
            foreach (char c in elementValue.ToCharArray())
            {
                //insert escape char if required
                if (escapeChar == c)
                {
                    escaped.Append(escapeChar);
                }
                else
                {
                    foreach (char delimiter in delimitersInUse)
                    {
                        if (delimiter == c)
                        {
                            escaped.Append(escapeChar);
                            break;
                        }
                    }
                }

                escaped.Append(c);
            }

            //for v2-formatting, add double-quotes around the content
            if (applyFormatting)
            {
                escaped.Insert(0, RdaEncoding.DOUBLE_QUOTE);
                escaped.Append(RdaEncoding.DOUBLE_QUOTE);
            }

            return escaped.ToString();
        }

        public Rda ToRda()
        {
            return this;
        }

        public IRda FromRda(Rda rda)
        {
            if (rda.Dimension == 0) 
            { 
                ScalarValue = rda.ScalarValue; 
            }
            else
            {
                Elements.Clear();
                Elements.AddRange(rda.Elements);
            }

            return this;
        }

        public class RdaEncoding
        {
            //in RDA spec, delimiters that are restricted to printable, non-white-space (and preferably non-alpha-numeric) chars.
            public static readonly char[] DEFAULT_DELIMITER_CANDIDATES = new char[] {
                '|', ';', ',', '^', ':',
                '~', '$', '&', '#', '=',
                '*', '.', '\'', '@', '_',
                '%', '/', '!', '?', '>',
                '<', '+', '-', '{', '}',
                '[', ']', '(', ')', '`',
                '0', '1', '2', '3', '4',
                '5', '6', '7', '8', '9'};

            public const char DEFAULT_ESCAPE_CHAR = '\\';

            public const char DOUBLE_QUOTE = '"';
            internal char[] Delimiters { get; private set; } = new char[] { }; 
            internal char EscapeChar { get; private set; } = DEFAULT_ESCAPE_CHAR;

            internal RdaEncoding(char[] customDelimiters, char escapeChar)  
            {
                Delimiters = customDelimiters;
                EscapeChar = escapeChar;
            }

            //returns if extension successful
            //Level > Global.RootParentDimensionLimit - 1
            internal void TryExtendRequiredDelimiters(int newLevel)
            {
                if (newLevel <= Delimiters.Length) { return; }
                else if (newLevel < DEFAULT_DELIMITER_CANDIDATES.Length)
                {
                    char[] newLevelDelimiters = new char[newLevel];
                    Array.Copy(Delimiters, 0, newLevelDelimiters, 0, Delimiters.Length);
                    int existingRangeIndex = Delimiters.Length;
                    foreach (char candidateDelimiterChar in DEFAULT_DELIMITER_CANDIDATES)
                    {
                        //make sure the candidate char isn't already in the delimiters range
                        if (!RangeContains(newLevelDelimiters, 0, existingRangeIndex, candidateDelimiterChar))
                        {
                            newLevelDelimiters[existingRangeIndex++] = candidateDelimiterChar;
                            if (existingRangeIndex == newLevelDelimiters.Length)
                            {
                                Delimiters = newLevelDelimiters; //copy the extended dimiters to 
                                return;
                            }
                        }
                    }
                }

                throw new Exception($"Maximun RDA dimension-limit ({newLevel}) reached, no child RDA can be accepted.");
            }

            public RdaEncoding() { }

        }
    }
}

