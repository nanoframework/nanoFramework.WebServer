// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Provides dynamically registered MCP resources to the MCP server controller.
    /// </summary>
    public interface IMcpResourceProvider
    {
        /// <summary>
        /// Gets MCP resource-list metadata.
        /// </summary>
        /// <returns>The JSON member containing the resource list.</returns>
        string GetResourceMetadataJson();

        /// <summary>
        /// Reads an MCP resource.
        /// </summary>
        /// <param name="uri">The resource URI.</param>
        /// <returns>The MCP resource-read result.</returns>
        string ReadResource(string uri);
    }
}