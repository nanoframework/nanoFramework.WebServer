// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Net;
using System.Threading;
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
        /// <returns>
        /// A byte[] containing the body of the request, or <see langword="null"/> if the body could not be read.
        /// </returns>
        public static byte[] ReadBody(this HttpListenerRequest httpListenerRequest)
        {
            long contentLength = httpListenerRequest.ContentLength64;

            // check missing or invalid content-length
            if (contentLength <= 0)
            {
                return new byte[0];
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
                    // The stream is (should be) a NetworkStream which might still be receiving data while
                    // we're already processing. Give the stream a chance to receive more data or we might
                    // end up with "zero bytes read" too soon...
                    Thread.Sleep(1);

                    long length = stream.Length;

                    if (length <= 0)
                    {
                        break;
                    }

                    if (length > buffer.Length)
                    {
                        length = buffer.Length;
                    }

                    long remaining = bodySize - position;
                    if (length > remaining)
                    {
                        length = remaining;
                    }

                    int bytesRead = stream.Read(buffer, 0, (int)length);

                    if (bytesRead == 0)
                    {
                        break;
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
