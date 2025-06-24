// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Attribute to mark a method as an MCP server tool and provide metadata for discovery and documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class McpServerToolAttribute : Attribute
    {
        /// <summary>
        /// Gets the unique name of the tool.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the tool.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the description of the tool's output.
        /// </summary>
        public string OutputDescription { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="McpServerToolAttribute"/> class with the specified name, description, and output description.
        /// </summary>
        /// <param name="name">The unique name of the tool.</param>
        /// <param name="description">The description of the tool.</param>
        /// <param name="outputDescription">The description of the tool's output.</param>
        public McpServerToolAttribute(string name, string description = "", string outputDescription = "")
        {
            Name = name;
            Description = description;
            OutputDescription = outputDescription;
        }
    }
}
