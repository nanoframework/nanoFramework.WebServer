//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Authentication attribute for classes and method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthenticationAttribute : Attribute
    {
        /// <summary>
        /// The authentication method, examples:
        /// - Basic:user password
        /// - Basic
        /// - ApiKey:OneApiKey
        /// - ApiKey
        /// - None
        /// In case of Basic and ApiKey alone, the default one passed at server properties ones will be used
        /// The Basic authentication is a classical http basic authentication and the couple user password have to be separated with a space, the password can contain spaces but not the user name. Basic and the user name has to be separated with a :
        /// ApiKey and the current apikey has to be separated with :
        /// The current ApiKey can contain only characters that are allow in http headers
        /// </summary>
        public string AuthenticationMethod { get; set; }

        /// <summary>
        /// The constructor for the Authentication attribute
        /// </summary>
        /// <param name="auth"></param>
        public AuthenticationAttribute(string auth)
        {
            AuthenticationMethod = auth;
        }
    }
}
