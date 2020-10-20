using System;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// The HTTP Method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : Attribute
    {
        public string Method { get; set; }

        public MethodAttribute(string method)
        {
            Method = method;
        }
    }
}
