//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Route custom attribute.
    /// </summary>
    /// <remarks>
    /// For example: test/any.
    /// For details on how to use, see: https://github.com/nanoframework/nanoFramework.WebServer#usage
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the route.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// A route attribute.
        /// </summary>
        /// <param name="route">The complete route like 'route/second/third'.</param>
        public RouteAttribute(string route)
        {
            Route = route;
        }
    }
}
