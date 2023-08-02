// Copyright (c) 2020 Michael Chen
// Licensed under the MIT License -
// https://github.com/foldda/rda/blob/main/LICENSE

using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{

    public static class StringExtension
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static void Print(this string str, string title)
        {
            if (str != null) { Console.WriteLine($"\r\n** {title} ** \r\n{str}"); }
        }

        public static string ToPrintable(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                sb.Append(char.IsControl(c) || char.IsWhiteSpace(c) ? c.ToPrintable() : c.ToString());
            }

            return sb.ToString();
        }

        public static string ToPrintable(this char chr)
        {
            switch (chr)
            {   //C# specific shortcut chars.
                case '\b':
                    return @"\b";
                case '\f':
                    return @"\f";
                case '\n':
                    return @"\n";
                case '\'':
                    return @"\'";
                case '"':
                    return "\\\"";
                case '\\':
                    return @"\\";
                case '\0':
                    return @"\0";
                case '\a':
                    return @"\a";
                case '\r':
                    return @"\r";
                case '\t':
                    return @"\t";
                case '\v':
                    return @"\v";
                default:
                    return @"\u" + ((int)chr).ToString("X4");
            }
        }

    }

}
