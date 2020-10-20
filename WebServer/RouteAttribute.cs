using System;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Route custom attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute: Attribute
    {
        public string Route { get; set; }

        public RouteAttribute(string route)
        {
            Route = route;
        }
    }
}
