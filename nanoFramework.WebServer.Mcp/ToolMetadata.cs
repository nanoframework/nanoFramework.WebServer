// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Represents metadata information for a registered tool, including its name, description, input/output types, and associated method.
    /// </summary>
    public class ToolMetadata
    {
        /// <summary>
        /// Gets or sets the unique name of the tool.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the tool.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the description of the tool's output.
        /// </summary>
        public string OutputDescription { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema string describing the input parameters for the tool.
        /// </summary>
        public string InputType { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema string describing the output type for the tool.
        /// </summary>
        public string OutputType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MethodInfo"/> representing the method associated with the tool.
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// Gets or sets the type of the method associated with the tool.
        /// </summary>
        public Type MethodType { get; set; }

        /// <summary>
        /// Returns a JSON string representation of the tool metadata.
        /// </summary>
        /// <returns>A JSON string containing the tool's name, description, input, and output schema.</returns>
        public override string ToString()
        {
            string output = $"{{\"name\":\"{Name}\",\"description\":\"{Description}\"";
            output += string.IsNullOrEmpty(InputType) ? string.Empty : $",\"inputSchema\":{InputType}";
            output += string.IsNullOrEmpty(OutputType) ? string.Empty : $",\"outputSchema\":{OutputType}";
            output += "}";
            return output;
        }
    }
}
