// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Provides data for the WebServerStatus event.
    /// </summary>
    public class WebServerStatusEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the WebServerStatusEventArgs class with the specified status.
        /// </summary>
        /// <param name="status">The status of the web server.</param>
        public WebServerStatusEventArgs(WebServerStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Gets the status of the web server.
        /// </summary>
        public WebServerStatus Status { get; protected set; }
    }
}
