// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace nanoFramework.WebServer.HttpMultipartParser
{
    /// <summary>
    /// Provides parsing headers from a Http Multipart Form
    /// </summary>
    public static class HeaderUtility
    {
        /// <summary>
        /// Reads headers from a line of text.
        /// Headers are delimited by a semi-colon ';'
        /// Key-value pairs are separated by colon ':' or equals '='
        /// Values can be delimited by quotes '"' or not
        /// </summary>
        /// <param name="text">The line of text containing one or more headers</param>
        /// <param name="headers">
        /// The hashtable that will receive the key values.
        /// Passed in since a Multipart Part can contain multiple lines of headers
        /// </param>
        public static void ParseHeaders(string text, Hashtable headers)
        {
            bool inQuotes = false;
            bool inKey = true;
            string key = string.Empty;
            string value = string.Empty;

            foreach (char c in text)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (inQuotes)
                {
                    value += c;
                }
                else if (c == ';')
                {
                    headers[key.ToLower()] = value;
                    key = string.Empty;
                    inKey = true;
                }
                else if (c == '=' || c == ':')
                {
                    value = string.Empty;
                    inKey = false;
                }
                else if (c == ' ')
                {
                    continue;
                }
                else if (inKey)
                {
                    key += c;
                }
                else
                {
                    value += c;
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                headers.Add(key.ToLower(), value);
            }
        }
    }
}
