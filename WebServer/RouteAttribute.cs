//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Route custom attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        public string Route { get; set; }

        /// <summary>
        /// A route attribute
        /// </summary>
        /// <param name="route">The route like route/second/third</param>
        public RouteAttribute(string route)
        {
            Route = route;
        }

    }
}
