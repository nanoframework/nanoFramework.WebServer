using System;
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
        public WebServerEventArgs(Socket mresponse, string mrawURL, string method, Header[] headers, byte[] content)
        {
            Response = mresponse;
            RawURL = mrawURL;
            Method = method;
            Headers = headers;
            Content = content;
        }

        /// <summary>
        /// The response class
        /// </summary>
        public Socket Response { get; protected set; }

        /// <summary>
        /// The raw URL elements
        /// </summary>
        public string RawURL { get; protected set; }

        /// <summary>
        /// The method used, GET/PUT/POST/DELETE/etc
        /// </summary>
        public string Method { get; protected set; }

        /// <summary>
        /// Http request headers
        /// </summary>
        public Header[] Headers { get; internal set; }

        /// <summary>
        /// Content in the request
        /// </summary>
        public byte[] Content { get; internal set; }
    }
}
