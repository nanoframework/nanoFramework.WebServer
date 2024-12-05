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
        /// Reads a body from the HttpListenerRequest inputstream
        /// </summary>
        /// <param name="httpListenerRequest">The request to read the body from</param>
        /// <returns>A byte[] containing the body of the request</returns>
        public static byte[] ReadBody(this HttpListenerRequest httpListenerRequest)
        {
            byte[] body = new byte[httpListenerRequest.ContentLength64];
            byte[] buffer = new byte[4096];
            Stream stream = httpListenerRequest.InputStream;

            int position = 0;

            while (true)
            {
                //The stream is (should be) a NetworkStream which might still be receiving data while
                //we're already processing. Give the stream a chance to receive more data or we might
                //end up with "zero bytes read" too soon...
                Thread.Sleep(1);

                long length = stream.Length;

                if (length > buffer.Length)
                {
                    length = buffer.Length;
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
    }
}
