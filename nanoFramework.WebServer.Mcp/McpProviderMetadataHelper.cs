// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Mcp
{
    internal static class McpProviderMetadataHelper
    {
        public static string Merge(string memberName, string registryMetadata, string providerMetadata)
        {
            string registryItems = GetItems(registryMetadata);
            string providerItems = GetItems(providerMetadata);

            if (string.IsNullOrEmpty(registryItems))
            {
                return "\"" + memberName + "\":[" + providerItems + "]";
            }

            if (string.IsNullOrEmpty(providerItems))
            {
                return "\"" + memberName + "\":[" + registryItems + "]";
            }

            return "\"" + memberName + "\":[" + registryItems + "," + providerItems + "]";
        }

        private static string GetItems(string metadata)
        {
            if (string.IsNullOrEmpty(metadata))
            {
                return string.Empty;
            }

            int start = metadata.IndexOf('[');
            int end = metadata.LastIndexOf(']');
            if (start < 0 || end < start)
            {
                throw new ArgumentException("Invalid MCP metadata JSON");
            }

            return metadata.Substring(start + 1, end - start - 1).Trim();
        }
    }
}