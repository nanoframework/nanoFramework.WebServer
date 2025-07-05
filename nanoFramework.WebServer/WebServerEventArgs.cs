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
        public WebServerEventArgs(HttpListenerContext context)
        {
            Context = context;
            RouteParameters = null;
        }

        /// <summary>
        /// Constructor for the event arguments with route parameters
        /// </summary>
        public WebServerEventArgs(HttpListenerContext context, UrlParameter[] routeParameters)
        {
            Context = context;
            RouteParameters = routeParameters;
        }

        /// <summary>
        /// The response class
        /// </summary>
        public HttpListenerContext Context { get; protected set; }

        /// <summary>
        /// Route parameters extracted from the URL (if any)
        /// </summary>
        public UrlParameter[] RouteParameters { get; protected set; }

        /// <summary>
        /// Gets the value of a route parameter by name
        /// </summary>
        /// <param name="parameterName">The name of the parameter to retrieve</param>
        /// <returns>The parameter value if found, otherwise null</returns>
        public string GetRouteParameter(string parameterName)
        {
            if (RouteParameters == null || string.IsNullOrEmpty(parameterName))
            {
                return null;
            }

            foreach (UrlParameter param in RouteParameters)
            {
                if (param.Name.ToLower() == parameterName.ToLower())
                {
                    return param.Value;
                }
            }

            return null;
        }
    }
}
