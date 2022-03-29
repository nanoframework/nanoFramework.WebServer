//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// The type of authentication to use
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// No authentication is needed
        /// </summary>
        None,

        /// <summary>
        /// Basic authentication with user and password
        /// </summary>
        Basic,

        /// <summary>
        /// Using an ApiKey
        /// </summary>
        ApiKey
    }
}
