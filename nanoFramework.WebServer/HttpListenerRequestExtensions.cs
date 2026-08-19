// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net;
using nanoFramework.WebServer.HttpMultipartParser;

namespace nanoFramework.WebServer
{
    /// <summary>Contains extension methods for HttpListenerRequest</summary>
    public static class HttpListenerRequestExtensions
    {
        /// <summary>
        /// Reads a Multipart form from the request
        /// </summary>
        /// <param name="httpListenerRequest">The request to read the form from</param>
        /// <returns>A <see cref="MultipartFormDataParser">MultipartFormDataParser</see> containing a collection of the parameters and files in the form.</returns>
        public static MultipartFormDataParser ReadForm(this HttpListenerRequest httpListenerRequest) =>
            MultipartFormDataParser.Parse(httpListenerRequest.InputStream);

        /// <summary>
        /// Reads a body from the HttpListenerRequest inputstream.
        /// </summary>
        /// <param name="httpListenerRequest">The request to read the body from</param>
        /// <param name="maximumBodySize">The maximum body size in bytes. Any negative value disables the check.</param>
        /// <returns>
        /// A byte[] containing the body of the request, or <see langword="null"/> if the body could not be read.
        /// </returns>
        public static byte[] ReadBody(this HttpListenerRequest httpListenerRequest, long maximumBodySize = -1)
        {
            long contentLength = httpListenerRequest.ContentLength64;

            if (contentLength == 0)
            {
                return new byte[0];
            }

            if (contentLength < 0)
            {
                return null;
            }

            if (maximumBodySize >= 0 && contentLength > maximumBodySize)
            {
                return null;
            }

            // Sanity check for huge content-length
            // A managed array cannot exceed int.MaxValue elements
            // Treat an oversized Content-Length the same as an allocation failure.
            if (contentLength > int.MaxValue)
            {
                return null;
            }

            try
            {
                int bodySize = (int)contentLength;
                byte[] body = new byte[bodySize];
                byte[] buffer = new byte[4096];
                Stream stream = httpListenerRequest.InputStream;

                int position = 0;

                while (position < bodySize)
                {
                    int remaining = bodySize - position;
                    int bytesRead = stream.Read(buffer, 0, remaining > buffer.Length ? buffer.Length : remaining);

                    if (bytesRead == 0)
                    {
                        return null;
                    }

                    Array.Copy(buffer, 0, body, position, bytesRead);

                    position += bytesRead;
                }

                return body;
            }
            catch
            {
                return null;
            }
        }
    }
}
