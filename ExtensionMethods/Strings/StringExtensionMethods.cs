﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using TidyNet;

namespace Umbraco.Community.ExtensionMethods.Strings
{
    public static class StringExtensionMethods
    {
        static readonly string _stripHTMLRegex = "<.+?>";
        static readonly string _stripHTMLRegexConditionalFormat = "<(?!({0})\\b)[^>]*>";

        /// <summary>
        /// Counts number of words in a string
        /// </summary>
        /// <param name="str">The string to parse</param>
        /// <returns>An integer of the number of words found</returns>
        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        /// <summary>
        /// Uppercases the first character of a string
        /// </summary>
        /// <param name="input">The string which first character should be uppercased</param>
        /// <returns>The input string with it'input first character uppercased</returns>
        public static string FirstCharToUpper(this string input) {
            return String.IsNullOrEmpty(input) ? "" : String.Concat(input.Substring(0, 1).ToUpper(), input.Substring(1));
        }

        /// <summary>
        /// Highlights specified keywords in the input string with the specified class name by using a &lt;span /&gt;
        /// </summary>
        /// <param name="input">The input string</param>
        /// <param name="keywords">The keywords to highlight</param>
        /// <param name="className">The class name</param>
        /// <returns>The input string with highlighted keywords</returns>
        public static string HighlightKeywords(this string input, IEnumerable<string> keywords, string className)
        {
            if (string.IsNullOrEmpty(input) || keywords == null || !keywords.Any())
                return input;

            foreach (string keyword in keywords)
            {
                input = Regex.Replace(input, keyword, string.Format("<span class=\"{1}\">{0}</span>", "$0", className), RegexOptions.IgnoreCase);
            }
            return input;
        }

        /// <summary>
        /// Strips HTML from a string with the specified options
        /// </summary>
        /// <param name="input">The HTML formatted input string</param>
        /// <param name="ignoreParagraphs">Indicates if paragraphs should be remained</param>
        /// <param name="ignoreItalic">Indicates if italic tags should be remained</param>
        /// <param name="ignoreUnderline">Indicates if underline tags should be remained</param>
        /// <param name="ignoreBold">Indicates if bold tags should be remained</param>
        /// <param name="ignoreLinebreak">Indicates if linebreaks should be remained</param>
        /// <param name="otherTagsToIgnore">A list of other tag names (without the brackets, like 'div') to ignore</param>
        /// <returns>The HTML stripped result</returns>
        public static string StripHtml(this string input, bool ignoreParagraphs = true, bool ignoreItalic = true, bool ignoreUnderline = true, bool ignoreBold = true, bool ignoreLinebreak = true, List<string> otherTagsToIgnore = null)
        {
            if (ignoreParagraphs || ignoreItalic || ignoreUnderline || ignoreBold || ignoreLinebreak || (otherTagsToIgnore != null && otherTagsToIgnore.Any()))
            {
                string conditions = string.Empty;

                if (ignoreParagraphs)
                    conditions += "/?p|";
                if (ignoreItalic)
                    conditions += "/?i|/?em|";
                if (ignoreUnderline)
                    conditions += "/?u|";
                if (ignoreBold)
                    conditions += "/?b|/?strong|";
                if (ignoreLinebreak)
                    conditions += "br|";
                if (otherTagsToIgnore != null && otherTagsToIgnore.Any())
                {
                    otherTagsToIgnore.ForEach((x) =>
                    {
                        conditions += string.Concat("/?", x, "|");
                    });
                }

                conditions = conditions.Substring(0, conditions.Length - 1); // Remove last '|'

                string regex = string.Format(_stripHTMLRegexConditionalFormat, conditions);
                Regex rgx = new Regex(regex, RegexOptions.Singleline);

                return rgx.Replace(input, string.Empty);
            }
            else
                if (input != null){
                return new Regex(_stripHTMLRegex, RegexOptions.Singleline).Replace(input, string.Empty);
                }
                else
                {
                    return String.Empty;
                }
        }

        /// <summary>
        /// Shortens a HTML formatted string, while keeping HTML formatting and complete words (also removes line-breakes at the end of the shortened string)
        /// </summary>
        /// <param name="input">The HTML formatted string</param>
        /// <param name="inputIsShortened">Output boolean telling if the input string has been shortened</param>
        /// <param name="length">The approximate length of the output string (default: 300)</param>
        /// <param name="elipsis">Elipsis text to append to the output string (use string.Empty when elipsis should not be added, default: ...)</param>
        /// <returns>The shortened input string with HTML formatting</returns>
        public static string ShortenHtml(this string input, out bool inputIsShortened, int length = 300, string elipsis = "...")
        {
            inputIsShortened = false;

            if (input.Length <= length)
                return input;

            input = input.Replace("<br />", "<br/>");

            string substring = input.Substring(0, length);
            string leftover = input.Substring(length);
            while (!leftover.StartsWith(" ") && leftover != string.Empty)
            {
                substring += leftover.Substring(0, 1);
                leftover = leftover.Substring(1);
            }
            substring = substring.Trim();
            while (substring.EndsWith("<br/>"))
            {
                substring = substring.Substring(0, substring.Length - 5);
                substring = substring.Trim();
            }

            if (input.Length > substring.Length)
                inputIsShortened = true;

            substring = substring.Replace("<br/>", "<br />");

            Tidy tidy = new Tidy();
            tidy.Options.DocType = DocType.Omit;
            tidy.Options.CharEncoding = CharEncoding.UTF8;
            tidy.Options.Xhtml = true;
            tidy.Options.NumEntities = true;

            TidyMessageCollection tmc = new TidyMessageCollection();
            MemoryStream inputStream = new MemoryStream();
            MemoryStream outputStream = new MemoryStream();

            byte[] bytes = Encoding.UTF8.GetBytes(substring);
            inputStream.Write(bytes, 0, bytes.Length);
            inputStream.Position = 0;
            tidy.Parse(inputStream, outputStream, tmc);

            string tidyResult = Encoding.UTF8.GetString(outputStream.ToArray());
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(tidyResult);
            tidyResult = xmlDoc.SelectSingleNode("//body").InnerXml;

            if (!string.IsNullOrEmpty(elipsis))
            {
                if (tidyResult.EndsWith("</p>"))
                    return string.Concat(tidyResult.Substring(0, tidyResult.Length - 4), elipsis, "</p>");
                return string.Concat(tidyResult, elipsis);
            }
            return tidyResult;
        }

        /// <summary>
        /// Shortens a HTML formatted string, while keeping HTML formatting and complete words (also removes line-breakes at the end of the shortened string)
        /// </summary>
        /// <param name="input">The HTML formatted string</param>
        /// <param name="length">The approximate length of the output string (default: 300)</param>
        /// <param name="elipsis">Elipsis text to append to the output string (use string.Empty when elipsis should not be added, default: ...)</param>
        /// <returns>The shortened input string with HTML formatting</returns>
        public static string ShortenHtml(string input, int length = 300, string elipsis = "...")
        {
            bool dummy;
            return ShortenHtml(input, out dummy, length, elipsis);
        }

        /// <summary>
        /// Removes diacritics from a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(string input)
        {
            // Indicates that a Unicode string is normalized using full canonical decomposition.
            string inputInFormD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int idx = 0; idx < inputInFormD.Length; idx++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(inputInFormD[idx]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(inputInFormD[idx]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// Returns a particular sentence by splitting by fullstop, question mark and exclamation mark
        /// </summary>
        /// <param name="textToSplit">The string</param>
        /// <param name="sentenceIndex">The index of the sentence to return</param>
        /// <returns>The single sentence requested</returns>
        public static string GetSentence(this string textToSplit, int sentenceIndex)
        {
            string[] delim = { ".", "!", "?" };
            string[] splitText = textToSplit.StripHtml(false,false,false,false,false).Split(delim, StringSplitOptions.None);
            var sentanceCount = splitText.Length;
            if (sentanceCount >= sentenceIndex)
            {
                return splitText[sentenceIndex] + ".";
            }
            return String.Empty;
        }

        /// <summary>
        /// Returns a particular paragraph by splitting by p tag
        /// </summary>
        /// <param name="textToSplit">The HTML formatted string</param>
        /// <param name="paragraphIndex">The index of the paragraph to return</param>
        /// <returns>The single paragraph requested</returns>
        public static string GetParagraph(this string textToSplit, int paragraphIndex)
        {
            string[] delim = { "<p>" };
            string[] splitText = textToSplit.RemoveHtmlComments().StripHtml(true,true,true,true,true,new List<string>{"a"}).Split(delim, StringSplitOptions.RemoveEmptyEntries);
            int paragraphCount = splitText.Length;

            if (paragraphCount-1 >= paragraphIndex)
            {
                var endPos = splitText[paragraphIndex].IndexOf("</p>");
                return "<p>" + splitText[paragraphIndex].Substring(0, endPos + 4);
        }
            return "<p></p>";
        }

        /// <summary>
        /// Strips out html comment tags
        /// </summary>
        /// <param name="input">The HTML formatted string</param>
        /// <returns>The html without any comments tags</returns>
        private static string RemoveHtmlComments(this string input)
        {
            if (input != null)
            {
                string output = string.Empty;
                string[] temp = Regex.Split(input, "<!--");
                foreach (string s in temp)
                {
                    var str = !s.Contains("-->") ? s : s.Substring(s.IndexOf("-->") + 3);
                    if (str.Trim() != string.Empty)
                    {
                        output = output + str.Trim();
                    }
                }
                return output;
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Safely truncate any string to a predetermined length and preserve whole words when truncating
        /// </summary>
        /// <param name="textToTruncate">Text to truncate</param>
        /// <param name="length">Length</param>
        /// <returns></returns>
        public static string TruncateAtWord(this string textToTruncate, int length)
        {
            if (textToTruncate == null || textToTruncate.Length < length)
                return textToTruncate;
            int iNextSpace = textToTruncate.LastIndexOf(" ", length);
            return string.Format("{0}...", textToTruncate.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        /// <summary>
        /// Inverts the case of each character in the given string and returns the new string
        /// </summary>
        /// <param name="input">The given string</param>
        /// <returns>The converted string</returns>
        public static string InvertCase(this string input)
        {
            return new string(
            input.Select(c => char.IsLetter(c) ? (char.IsUpper(c) ?
                      char.ToLower(c) : char.ToUpper(c)) : c).ToArray());
        }

        /// <summary>
        /// Returns the substring of string1 before the occurrence of string2.
        /// </summary>
        /// <param name="string1">
        /// The string 1.
        /// </param>
        /// <param name="string2">
        /// The string 2.
        /// </param>
        /// <returns>
        /// The substring
        /// </returns>
        public static string SubstringBefore(this string string1, string string2)
        {
            var posA = string1.IndexOf(string2);
            if (posA != -1)
            {
                return string1.Substring(0, posA);
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the substring of string1 after the occurrence of string2.
        /// </summary>
        /// <param name="string1">
        /// The string 1.
        /// </param>
        /// <param name="string2">
        /// The string 2.
        /// </param>
        /// <returns>
        /// The substring
        /// </returns>
        public static string SubstringAfter(this string string1, string string2)
        {
            var posA = string1.IndexOf(string2);
            if (posA != -1)
            {
                return string1.Substring(posA + 1);
            }
            return string.Empty;
        }


    }
}
