// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;

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
            StringBuilder key = new();
            StringBuilder value = new();

            foreach (char c in text)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (inQuotes)
                {
                    value.Append(c);
                }
                else if (c == ';')
                {
                    headers[key.ToString().ToLower()] = value.ToString();
                    key.Clear();
                    inKey = true;
                }
                else if (c == '=' || c == ':')
                {
                    value = value.Clear();
                    inKey = false;
                }
                else if (c != ' ')
                {
                    if (inKey)
                    {
                        key.Append(c);
                    }
                    else
                    {
                        value.Append(c);
                    }
                }
            }

            if (key.Length > 0)
            {
                headers.Add(key.ToString().ToLower(), value.ToString());
            }
        }
    }
}
