// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Provides dynamically registered MCP tools to the MCP server controller.
    /// </summary>
    public interface IMcpToolProvider
    {
        /// <summary>
        /// Gets MCP tool-list metadata.
        /// </summary>
        /// <returns>The JSON member containing the tool list.</returns>
        string GetToolMetadataJson();

        /// <summary>
        /// Invokes an MCP tool.
        /// </summary>
        /// <param name="toolName">The tool name.</param>
        /// <param name="arguments">The tool arguments.</param>
        /// <returns>The JSON value returned by the tool.</returns>
        string InvokeTool(string toolName, Hashtable arguments);
    }
}