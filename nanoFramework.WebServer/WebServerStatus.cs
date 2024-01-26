// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Represents the status of the server.
    /// </summary>
    public enum WebServerStatus
    {
        /// <summary>
        /// The server is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The server is running.
        /// </summary>
        Running,
    }
}