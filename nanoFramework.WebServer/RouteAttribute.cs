//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// This attribute defines which HTTP route a method will handle.
    /// </summary>
    /// <remarks>
    /// For example: test/any.
    /// For details on how to use, see: https://github.com/nanoframework/nanoFramework.WebServer#usage
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// The route that the method will respond to.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute"/> class.
        /// </summary>
        /// <param name="route">The route the method will handle.</param>
        /// <remarks>
        /// For example: test/any.
        /// For details on how to use, see: https://github.com/nanoframework/nanoFramework.WebServer#usage</remarks>
        public RouteAttribute(string route)
        {
            Route = route;
        }
    }
}
