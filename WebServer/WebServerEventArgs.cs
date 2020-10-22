using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
