//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System.Net;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Web server event argument class
    /// </summary>
    public class WebServerEventArgs
    {
        /// <summary>
        /// Constructor for the event arguments
        /// </summary>
        /// <param name="mresponse"></param>
        /// <param name="mrawURL"></param>
        public WebServerEventArgs(HttpListenerContext context)
        {
            Context = context;
        }

        /// <summary>
        /// The response class
        /// </summary>
        public HttpListenerContext Context { get; protected set; }
    }
}
