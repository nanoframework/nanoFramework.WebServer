//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Net;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// The authentication to be used by the server.
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// The type of authentication.
        /// </summary>
        public AuthenticationType AuthenticationType { get; internal set; }

        /// <summary>
        /// Gets or sets the network credentials that are sent to the host and used to authenticate the request.
        /// </summary>
        public NetworkCredential Credentials { get; internal set; } = null;

        /// <summary>
        /// The API Key to use for authentication.
        /// </summary>
        public string ApiKey { get; internal set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Authentication"/> class.
        /// </summary>
        /// <param name="credential">
        /// The network credential user and password.
        /// </param>
        public Authentication(NetworkCredential credential)
        {
            AuthenticationType = AuthenticationType.Basic;
            Credentials = credential;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Authentication"/> class.
        /// </summary>
        /// <param name="apiKey">
        /// The API Key.
        /// </param>
        public Authentication(string apiKey)
        {
            AuthenticationType = AuthenticationType.ApiKey;
            ApiKey = apiKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Authentication"/> class.
        /// </summary>
        public Authentication()
        {
            AuthenticationType = AuthenticationType.None;
        }
    }
}
