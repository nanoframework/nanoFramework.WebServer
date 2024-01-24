//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// The HTTP Method.
    /// </summary>
    /// <remarks>
    /// No validation is performed if the HTTP method is a valid one.
    /// For details on how to use, see: https://github.com/nanoframework/nanoFramework.WebServer#usage
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : Attribute
    {
        /// <summary>
        /// Gets the method.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Creates a method attribute.
        /// </summary>
        /// <param name="method">The method.</param>
        public MethodAttribute(string method)
        {
            Method = method;
        }
    }
}
