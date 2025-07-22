// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Used to indicate that a method should be considered an <see cref="McpServerPromptAttribute"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is applied to methods that should be exposed as prompts in the Model Context Protocol. When a class 
    /// containing methods marked with this attribute is registered with McpServerBuilderExtensions,
    /// these methods become available as prompts that can be called by MCP clients.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class McpServerPromptAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the prompt.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the tool.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="McpServerPromptAttribute"/> class with the specified name and description.
        /// </summary>
        /// <param name="name">The unique name of the prompt.</param>
        /// <param name="description">The description of the prompt.</param>
        public McpServerPromptAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
