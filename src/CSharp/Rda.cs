// Copyright (c) 2022 Foldda Pty Ltd
// Licensed under the GPL License -
// https://github.com/foldda/charian/blob/main/LICENSE

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
        //"scalar" content is used when RDA's Dimension = 0, it's the unescaped 'original' dateTimeValue that is independent to the encoding chars.
        private string _scalarValue = null;
        //Children are for storing the "composite" content, when RDA's Dimension > 0, where each child is a sub-RDA
        public List<Rda> Elements { get; } = new List<Rda>();
        public Rda Parent { get; private set; } // the upper-level RDA of which this RDA is a child

        //for the whole RDA structure, encoding is fixed and shared amongs parent and children
        private RdaEncoding _encoding = null;
        public RdaEncoding GlobalEncoding => Parent == null ? _encoding : Parent.GlobalEncoding;

        public int OriginalDelimitersLength => GlobalEncoding.CustomParsingDelimiters.Length;

        internal char[] AvaliableEncodingDelimiters { get; private set; } = RdaEncoding.DEFAULT_DELIMITER_CANDIDATES;


        /**
         * Root-level Rda constructor
         */
        public Rda(RdaEncoding encoding)
        {
            _encoding = encoding;
            AvaliableEncodingDelimiters 
                = CombineDelimiters(encoding.CustomParsingDelimiters, RdaEncoding.DEFAULT_DELIMITER_CANDIDATES, encoding.EscapeChar);
        }

        char[] CombineDelimiters(char[] delimitersSet1, char[] delimitersSet2, char escapeChar)
        {
            HashSet<char> uniqueChars = new HashSet<char>();

            foreach (char c in delimitersSet1)
            {
                if (c != escapeChar) { uniqueChars.Add(c); }
            }

            foreach (char c in delimitersSet2)
            {
                if (c != escapeChar) { uniqueChars.Add(c); }
            }

            char[] result = new char[uniqueChars.Count];
            uniqueChars.CopyTo(result);
            return result;
        }


        //use default encoding
        public Rda() : this(new RdaEncoding()) { }

        public Rda MakeChild(Rda parent) 
        {
            return new Rda()
            {
                Parent = parent,    //inherites parent's encoding
                AvaliableEncodingDelimiters = parent.AvaliableEncodingDelimiters
            };
        }

        public static Rda Parse(string rdaString)
        {
            RdaEncoding encoding = GetHeaderSectionEncoder(rdaString);
            Rda rda = new Rda(encoding);
            if(encoding.CustomParsingDelimiters.Length == 0)
            {   //dimension = 0
                rda.ScalarValue = rdaString;
            }
            else
            {
                string payload = rdaString.Substring(encoding.CustomParsingDelimiters.Length + 2);
                rda.ParsePayload(payload, DetermineParsingFormatVersion(payload) == FORMATTING_VERSION.LOCAL);
            }

            return rda;
        }

        public IRda FromString(string s)
        {
            return Parse(s);
        }

        /**
         * Derived properties from the "storage fields" and the encoding field
         */

        public string PayLoad => GetPayload(EncodingDelimiters, FORMATTING_VERSION.STANDARD);
        public string LocalPayLoad => GetPayload(EncodingDelimiters, FORMATTING_VERSION.LOCAL);

        //determines the number of delimiters required for encoding this RDA, 
        private int MinDelimiterDimension
        {
            get
            {
                int maxChildDimemsion = -1;
                if(Elements.Count == 1)
                {
                    return Elements[0].MinDelimiterDimension;
                }
                else
                {
                    foreach (var e in Elements)
                    {
                        maxChildDimemsion = Math.Max(maxChildDimemsion, e.MinDelimiterDimension);
                    }
                }

                return maxChildDimemsion + 1;   //dimension will be 0 when there is no children. 
            }
        }

        public char[] MinimumEncodingDelimiters
        {
            get
            {
                int delimitersLength = Dimension;   // Math.Max(OriginalDelimitersLength, MinDelimiterDimension);
                char[] result = new char[delimitersLength];
                Array.Copy(AvaliableEncodingDelimiters, Level, result, 0, delimitersLength);
                return result;
            }
        }

        //it's the max-depth towards the bottom
        public int Dimension
        {
            get
            {
                int maxChildDimemsion = -1;

                foreach (var e in Elements)
                {
                    maxChildDimemsion = Math.Max(maxChildDimemsion, e.Dimension);
                }

                return maxChildDimemsion + 1;   //dimension will be 0 when there is no children. 
            }
        }


        //the number of steps from the root Parent RDA
        //it's used as the index to Delimiters array for determing the next-level delimiter
        private int Level => Parent == null ? 0 : Parent.Level + 1;

        /**
         * API Properties and Methods - when using RDA as a storage container
         */

        //the client's 'string dateTimeValue' stored in this RDA. 
        public string ScalarValue 
        { 
            //For Dimension-0 (leaf) RDA, it's the stored scalar-dateTimeValue, for composite RDA (dimension > 0), it's the left-most child's scalar-dateTimeValue
            get => Elements.Count > 0 ? Elements[0].ScalarValue : _scalarValue ?? string.Empty;

            //sets the scalar-dateTimeValue, and clears children (making this RDA as Dimension-0)
            set
            {
                Elements.Clear();
                _scalarValue = value;
            }
        }

        //this rda's "string expression", i.e. a properly encoded RDA string with the header and the payload sections
        //NB, for Dimension-0 RDA, it outputs the stored scalar dateTimeValue (i.e. the header-section is an empty string for Dimension-0 RDA)
        public override string ToString()
        {
            return MinimumEncodingDelimiters.Length == 0 ? ScalarValue : $"{new string(MinimumEncodingDelimiters)}{EscapeChar}{MinimumEncodingDelimiters[0]}{PayLoad}";
        }

        //prints this rda's 'string expression', with version-2 formatting applied.
        //version-2 formatting uses reserved formatting chars such as white-space, line-breaks, and double-quotes in the payload's encoding
        public string Print()
        {
            return Dimension == 0 ? ScalarValue : $"{new string(MinimumEncodingDelimiters)}{EscapeChar}{MinimumEncodingDelimiters[0]}{LINE_BREAK} {LocalPayLoad}";
        }


        //set a child RDA at the index'd location, extend the max index if required 
        public void SetRda(int index, Rda childRda)
        {
            EnsureArrayLength(index);   //make sure the addressed position exists, create dummies if required

            if (childRda != null)
            {
                //make the child-Rda encoding the same as this Rda
                childRda.Parent = this;

                Elements[index] = childRda; //set or replace the child at the addressed position
            }
            else
            {
                //GlobalEncoding.TryExtendRequiredDelimiters(Level + 1);  //throws Exception if limit is reached
                Elements[index] = MakeChild(this);    //make a dummy
            }
        }

        //get a child RDA at the index'd location, return null if RDA is not allocated 
        //NB: by indexing over an RDA having no children (i.e. Dim-0) would automatically increases its dimension
        public Rda GetRda(int index)
        {
            //GlobalEncoding.TryExtendRequiredDelimiters(Level + 1);  //throws Exception if limit is reached
            
            if(Dimension == 0)
            {
                //push this RDA's scalar-dateTimeValue to become the left-most child's dateTimeValue
                var child = MakeChild(this);
                child.ScalarValue = _scalarValue;
                Elements.Add(child); ;
            }

            EnsureArrayLength(index);   //creates dummies if required

            //the indexed child can be safely retrived
            return Elements[index];     //NB, if this element is-dummy, the supposed dateTimeValue would be NULL if IRda.FromRda() is called.
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
        /// Sets the scalar dateTimeValue to the child rda addressed by the multi-dimension index array
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
                    if (!string.IsNullOrEmpty(_scalarValue))
                    {
                        result.Add(_scalarValue);   //single child, not dummy node
                    }
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
                else if(value.Length == 1)
                {
                    _scalarValue = value[0];
                }
                else
                {
                    _scalarValue = null;
                    foreach (var s in value)
                    {
                        var child = MakeChild(this);
                        child.ScalarValue = s;
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

        //the encoding delimiters (used by ToString()) for the given Rda
        public char[] EncodingDelimiters
        {
            get
            {
                char[] result = new char[Dimension];
                Array.Copy(AvaliableEncodingDelimiters, Level, result, 0, result.Length);
                return result;
            }
        }

        public char ChildDelimiter => AvaliableEncodingDelimiters[Level];  //the char that separates the immediate children elements
        public char EscapeChar => GlobalEncoding.EscapeChar;

        public int Length => Elements.Count;

        //decode the delimited values in payload and apply unescaping to restore the 'original' dateTimeValue
        //and store these unescaped values to the scalar_value variable of an rda
        private void ParsePayload(string payloadString, bool v2Formatted)
        {
            Elements.Clear();

            //make sure the parsing doesn't go beyond the RDA-string "levels" limit (set by the encoding header section)
            if (GlobalEncoding.CustomParsingDelimiters.Length > Level)
            {
                //... then continue to (recurrsively) parse the rda-encoded payload string ..
                var sections = ParseChildrenContentSections(payloadString);

                foreach (string childPayLoad in sections)
                {
                    var child = MakeChild(this);
                    child.ParsePayload(childPayLoad, v2Formatted);  //recursion

                    Elements.Add(child);
                }
            }
            else
            {
                //apply maximun unescape to "string-dateTimeValue" before it's stored
                //this will be reversed (escaped) when the dateTimeValue is used for assembling a payload section.
                _scalarValue = UnEscape(payloadString, GlobalEncoding.CustomParsingDelimiters, EscapeChar, v2Formatted);
            }
        }

        //dimensions can be expended automatically when accessing dummy elements (ie. it dynamically creates elements/levels)
        //this method reverse the effect (of expending) and shorten each branch to its minimun required dimension
        public void CompressDimension()
        {
            _encoding = new RdaEncoding();
            if (Dimension > 0)  /* Elements.Count > 0 */
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
                    else
                    {
                        Elements.RemoveAt(i);
                    }
                }

                //reduce this Rda's dimension if there is only one child, and its dimension is 0,
                //... by bringing the child's scalar dateTimeValue up, and deletes all children
                //note if Elements[0] is dummy, this will also become a dummy
                if (Elements[0].Dimension == 0)
                {
                    this.ScalarValue = Elements[0].ScalarValue;  
                    Elements.Clear();
                }
            }
        }

        public static Rda NULL = new Rda();

        //test if an Rda object contains no data.
        public static bool IsNullOrEmpty(Rda rda)
        {
            if(rda == null)
            {
                return true;
            }
            else if (rda.Dimension > 0)
            {
                foreach (var element in rda.Elements)
                {
                    if (IsNullOrEmpty(element) == false)
                    {
                        return false; /* no compression - if non-null child found before index 0 */
                    }
                }

                return true;
            }
            else
            {
                return string.IsNullOrEmpty(rda.PayLoad.Trim());
            }
        }

        //since in an encoded RDA string an empty slot is representing logical NULL (scalar or composite) payload, there is a limitation that an empty (zero-length)
        //string cannot be encoded as a scalar dateTimeValue, as a work-around convention, a one-char (non-printable) string is used to "represent" an empty-string dateTimeValue.
        //this convention is not inforced, as the communicating parties can choose their own convention for work-arounds that allows passing empty-string dateTimeValue
        //(eg using quoted strings "")

        public readonly static string EMPTY_SCALAR_VALUE = new string(new char[] { (char)0 });
        protected static string AssignScalarStringValue(string sInput)
        {
            if (sInput == null) { return string.Empty; /* logical NULL */}
            else if (sInput.Length == 0) { return EMPTY_SCALAR_VALUE;  /* logical EMPTY */}
            else { return sInput; }
        }

        protected static string RetrieveScalarString(string sOutput)
        {
            if (sOutput == null) { throw new Exception("Invalid property scalar dateTimeValue (stored as NULL), NULL dateTimeValue should be stored as string.Empty."); }
            else if (EMPTY_SCALAR_VALUE.Equals(sOutput)) { return string.Empty; /* translate logical-empty to physical-empty */}
            else if (sOutput.Length == 0) { return null; /* translate logical-null to physical-null */}
            else { return sOutput; }
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

        public enum FORMATTING_VERSION : int { STANDARD = 1, LOCAL = 2};     /* V0 = 0, not defined */

        /* below are helper properties and methods */

        /// <summary>
        /// RDA is v2-formatted if the first line only contains the header section and trailing white-spaces.
        /// In v2-formatted RDA, leading/trailing spaces and line-breakes are for formatting and are not considered as part of the element's string dateTimeValue.
        /// </summary>
        private static FORMATTING_VERSION DetermineParsingFormatVersion(string payloadString)
        {
            char[] valueCharArray = payloadString.ToCharArray();

            for (int i= 0; i < valueCharArray.Length; i++)
            {
                char currChar = valueCharArray[i];
                if (!char.IsWhiteSpace(currChar)) /* any non-white-spcae before EOL indicating it is not v2 formatted */
                {
                    return FORMATTING_VERSION.STANDARD;
                }
                else if (currChar == '\n') /* this EOL indicating previous chars are all white-spaces, so it is v2 formatted */
                {
                    return FORMATTING_VERSION.LOCAL;
                }            
            }

            return FORMATTING_VERSION.STANDARD;   //valueCharArray are all white-space chars
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
            //double-quote-char is used (in v2-formatted RDA) for enclosing leading and trailing spaces in string dateTimeValue 
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

            return unescaped.ToString();   //un-escaped section dateTimeValue
        }

        const string LINE_BREAK = "\r\n";
        static readonly string INDENT = new string(new char[] { ' ', ' ' });

        string Indent
        {
            get
            {
                if (Parent == null || Parent.Elements.Count == 1) { return string.Empty; }
                else { return Parent.Indent + INDENT; }
            }
        }

        //payload = <delimitor at this level> + concatenated children payloads (recurrsion)
        //NB, payload string contains escaping to the stored scalar values
        private string GetPayload(char[] delimiterChars, FORMATTING_VERSION formattingVersion )
        {
            bool applyFormatting = (formattingVersion == FORMATTING_VERSION.LOCAL);
            StringBuilder result = new StringBuilder();

            if (LastNonDummyIndex < 0)
            {
                //apply escaping to the unescaped dateTimeValue (the stored "real/original" dateTimeValue) when it becomes part of a payload
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

                    //append a leading delimiter - except for the first element 
                    if (i != 0) { result.Append(ChildDelimiter); } 
                                      
                    result.Append(child.GetPayload(delimiterChars, formattingVersion)); //recurrsion ...
                }
            }

            return result.ToString();
        }

        private string GetFormattingPrefix(int index)
        {
            //if this is the first child ...
            if(index == 0)
            {
                return Elements.Count > 1 && Parent != null ? INDENT : string.Empty;
            }
            else
            {
                return LINE_BREAK + Indent; 
            }
        }

        //dummy child (holds a NULL Rda dateTimeValue here?) - is 'place-holder' that is created when accessor 'over-indexed' the RDA existing values
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

        //public char[] DelimitersInUse => Delimiters;

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
                var dummy = MakeChild(this);/*dummy*/
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

        //helper: used for parsing a section-dateTimeValue, that may conatins delimiters chars and/or escape char, from an encoded RDA string
        //returns the actual dateTimeValue that needs to be stored 
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

        /// <summary>
        /// Make a new Rda object that has copied data from another Rda
        /// </summary>
        /// <param name="originalRda">The source Rda</param>
        public Rda(Rda originalRda)
        {
            FromRda(originalRda);
        }

        //This only clones a "top-level" Rda as it does not have the
        //reference to Parent
        public IRda FromRda(Rda sourceRda)
        {
            if(sourceRda == null) { return null; }

            _encoding = sourceRda.GlobalEncoding;   //this is a top-level Rda

            Elements.Clear(); 
            if (sourceRda.Elements == null || sourceRda.Elements.Count == 0)
            {
                _scalarValue = sourceRda.ScalarValue;
            }
            else
            {
                foreach(var element in sourceRda.Elements)
                {
                    Elements.Add(new Rda(element) { Parent = this }); 
                }
            }

            return this;
        }

        enum TIME_TOKEN : int { YEAR = 0, MONTH = 1, DAY = 2, HOUR = 3, MINUTE = 4, SECOND = 5 }
        public static DateTime MakeDateTime(string[] dateTimeTokens)
        {
            if (dateTimeTokens.Length < 3)
            {
                throw new Exception($"Date-time string '{dateTimeTokens}' expecting minumum 3 (year, month, and day) tokens, plus optionally the hour, minute, and second tokens.");
            }

            int[] dateTimeTokenIntegers = new int[Enum.GetValues(typeof(TIME_TOKEN)).Length];
            for (int i = 0; i < dateTimeTokenIntegers.Length; i++)
            {
                if (i < dateTimeTokens.Length)
                {
                    if (int.TryParse(dateTimeTokens[i], out int result))
                    {
                        dateTimeTokenIntegers[i] = result;
                    }
                    else
                    {
                        throw new Exception($"Date-time token {i} '{dateTimeTokens[i]}' is not an integer.");
                    }
                }
                else
                {
                    break;
                }
            }

            return new DateTime(dateTimeTokenIntegers[(int)TIME_TOKEN.YEAR], dateTimeTokenIntegers[(int)TIME_TOKEN.MONTH], dateTimeTokenIntegers[(int)TIME_TOKEN.DAY],
                                dateTimeTokenIntegers[(int)TIME_TOKEN.HOUR], dateTimeTokenIntegers[(int)TIME_TOKEN.MINUTE], dateTimeTokenIntegers[(int)TIME_TOKEN.SECOND]);
        }

        public static string[] MakeDateTimeTokens(DateTime dateTimeValue)
        {
            string[] tokens = new string[Enum.GetValues(typeof(TIME_TOKEN)).Length];
            //stores the integer values (of datetime tokens) as strings
            tokens[(int)TIME_TOKEN.YEAR] = dateTimeValue.Year.ToString();
            tokens[(int)TIME_TOKEN.MONTH] = dateTimeValue.Month.ToString();
            tokens[(int)TIME_TOKEN.DAY] = dateTimeValue.Day.ToString();
            tokens[(int)TIME_TOKEN.HOUR] = dateTimeValue.Hour.ToString();
            tokens[(int)TIME_TOKEN.MINUTE] = dateTimeValue.Minute.ToString();
            tokens[(int)TIME_TOKEN.SECOND] = dateTimeValue.Second.ToString();
            return tokens;
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



            internal char[] CustomParsingDelimiters { get; } = new char[0];

            internal char EscapeChar { get; private set; } = DEFAULT_ESCAPE_CHAR;

            internal RdaEncoding(char[] customDelimiters, char escapeChar)
            {
                if(Array.IndexOf(customDelimiters, escapeChar) >= 0 || ContainsDuplicate(customDelimiters))
                {
                    throw new Exception("Duplicate found among the supplied delimiters and escape char.");
                }

                CustomParsingDelimiters = customDelimiters;
                EscapeChar = escapeChar;
            }


            bool ContainsDuplicate(char[] chars)
            {
                HashSet<char> uniqueChars = new HashSet<char>();

                foreach (char c in chars)
                {
                    if (!uniqueChars.Add(c))
                    {
                        return true;
                    }
                }
                return false;
            }

            internal RdaEncoding() { }
        }

    }
}

