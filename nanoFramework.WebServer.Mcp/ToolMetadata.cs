// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;

namespace nanoFramework.WebServer.Mcp
{
    public class ToolMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string InputType { get; set; }
        public string OutputType { get; set; }
        public MethodInfo Method { get; set; }
    }
}
