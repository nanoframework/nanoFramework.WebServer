// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.HttpMultipartParser
{
    /// <summary>
    /// Specific exception while parsing a multipart form
    /// </summary>
    public class MultipartFormDataParserException : Exception
    {
        /// <summary>
        /// Initializes a MultipartFormDataParserException
        /// </summary>
        /// <param name="message"></param>
        public MultipartFormDataParserException(string message) : base(message)
        {
        }
    }
}
