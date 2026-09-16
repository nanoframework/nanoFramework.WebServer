// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Attribute to mark a method as an MCP server resource and provide metadata for discovery and documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class McpServerResourceAttribute : Attribute
    {
        /// <summary>
        /// Gets the unique absolute URI of the resource.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the resource.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the MIME type of the resource.
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="McpServerResourceAttribute"/> class with the specified URI, name, description, and MIME type.
        /// </summary>
        /// <param name="uri">The unique absolute URI of the resource.</param>
        /// <param name="name">The name of the resource.</param>
        /// <param name="description">The description of the resource.</param>
        /// <param name="mimeType">The MIME type of the resource.</param>
        public McpServerResourceAttribute(string uri, string name, string description = "", string mimeType = "text/plain")
        {
            Uri = new Uri(uri, UriKind.Absolute);
            Name = name;
            Description = description;
            MimeType = mimeType;
        }
    }
}
