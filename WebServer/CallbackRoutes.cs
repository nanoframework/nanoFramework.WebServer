using System;
using System.Reflection;
using System.Text;

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
        /// The http method ex GET or POST, leave string.Empty for any
        /// </summary>
        public string Method { get; set; }
    }
}
