//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// If the route is case sensitive or not
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CaseSensitiveAttribute : Attribute
    { }
}
