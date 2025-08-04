// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Represents a parameter for a Model Context Protocol (MCP) prompt, providing metadata about the parameter such as its name and description.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These have to be added to the methods implementing the MCP prompts in the same order as the parameters.
    /// </para>
    /// <para>
    /// By design, if a prompt parameter is specified, it is considered required. .NET nanoFramework does not support optional parameters in MCP prompts.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class McpPromptParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the prompt parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the prompt parameter.
        /// </summary>
        /// <remarks>
        /// The description is optional.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="McpPromptParameterAttribute"/> class with the specified name and description.
        /// </summary>
        /// <param name="name">The name of the prompt parameter.</param>
        /// <param name="description">The description of the prompt parameter.</param>
        public McpPromptParameterAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{\"name\":\"{Name}\",\"description\":\"{Description}\",\"required\":true}}";
        }
    }
}
