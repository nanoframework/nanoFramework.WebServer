// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using nanoFramework.Json;
using System.Threading;

namespace nanoFramework.WebServer.Mcp
{
    public static class McpToolRegistry
    {
        private static readonly Hashtable tools = new Hashtable();

        public static void DiscoverTools(Type[] mcpTools)
        {
            foreach (Type mcpTool in mcpTools)
            {
                MethodInfo[] methods = mcpTool.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    var allAttribute = method.GetCustomAttributes(false);
                    foreach (var attrib in allAttribute)
                    {
                        if (attrib.GetType() == typeof(McpToolAttribute))
                        {
                            McpToolAttribute attribute = (McpToolAttribute)attrib;
                            if (attribute != null)
                            {
                                tools[attribute.Name] = new ToolMetadata
                                {
                                    Name = attribute.Name,
                                    Description = attribute.Description,
                                    InputType = attribute.InputType,
                                    OutputType = attribute.OutputType,
                                    Method = method
                                };
                            }
                        }
                    }
                }
            }
        }        
    }
}
