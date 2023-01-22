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
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        public string Method { get; set; }

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
