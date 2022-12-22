//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// This attribute defines which HTTP Method the class method will handle.
    /// </summary>
    /// <remarks>
    /// No validation is performed if the HTTP method is a valid one.
    /// For details on how to use, see: https://github.com/nanoframework/nanoFramework.WebServer#usage
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : Attribute
    {
        /// <summary>
        /// The HTTP Method to use.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAttribute"/> class.
        /// </summary>
        /// <param name="method">The HTTP Method to handle./// </param>
        public MethodAttribute(string method)
        {
            Method = method;
        }
    }
}
