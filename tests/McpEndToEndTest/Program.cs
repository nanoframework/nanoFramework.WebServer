// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
//using nanoFramework.WebServer.Mcp;

namespace McpEndToEndTest
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from MCP Server!");

            //McpToolRegistry.DiscoverTools(new Type[] { typeof(McpServerTests.McpTools) });

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
