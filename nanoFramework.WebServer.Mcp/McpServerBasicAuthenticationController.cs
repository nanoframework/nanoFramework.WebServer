// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// McpServerController class provides endpoints for handling requests related to MCP (Model Context Protocol) tools.
    /// This controller is specifically designed for basic (user, password) authentication.
    /// </summary>
    [Authentication("Basic")]
    public class McpServerBasicAuthenticationController : McpServerController
    {
    }
}
