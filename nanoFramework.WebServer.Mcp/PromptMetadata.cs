// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Represents metadata information for a prompt, including its name, description and associated method.
    /// </summary>
    public class PromptMetadata
    {
        /// <summary>
        /// Gets or sets the unique name of the prompt.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the prompt.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets JSON schema respresentniog the arguments for the prompt.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MethodInfo"/> representing the method associated with the prompt.
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// Returns a JSON string representation of the prompt metadata.
        /// </summary>
        /// <returns>A JSON string containing the prompt's name, description schema.</returns>
        public override string ToString()
        {
            string output = $"{{\"name\":\"{Name}\",\"description\":\"{Description}\"";
            output += string.IsNullOrEmpty(Arguments) ? string.Empty : $",\"arguments\":[{Arguments}]";
            output += $"}}";

            return output;
        }
    }
}
