//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System.Reflection;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Callback function for the various routes
    /// </summary>
    public class CallbackRoutes
    {
        /// <summary>
        /// The method to call for a specific route
        /// </summary>
        public MethodInfo Callback { get; set; }

        /// <summary>
        /// The route ex: api/gpio
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Is the root case sensitive?
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// The http method ex GET or POST, leave string.Empty for any
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// the authentication details
        /// </summary>
        public Authentication Authentication { get; set; }
    }
}
