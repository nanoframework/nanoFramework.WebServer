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
        /// The network credential user and password.
        /// </summary>
        public NetworkCredential Credentials { get; internal set; } = null;

        /// <summary>
        /// The API Key to use for authentication.
        /// </summary>
        public string ApiKey { get; internal set; } = null;

        /// <summary>
        /// Creates an autentication class from a credential.
        /// </summary>
        /// <param name="credential">The credentials.</param>
        public Authentication(NetworkCredential credential)
        {
            AuthenticationType = AuthenticationType.Basic;
            Credentials = credential;
        }

        /// <summary>
        /// Creates an authentication from a key.
        /// </summary>
        /// <param name="apiKey">The key.</param>
        public Authentication(string apiKey)
        {
            AuthenticationType = AuthenticationType.ApiKey;
            ApiKey = apiKey;
        }

        /// <summary>
        /// Creates an empty authenticate.
        /// </summary>
        public Authentication()
        {
            AuthenticationType = AuthenticationType.None;
        }
    }
}
