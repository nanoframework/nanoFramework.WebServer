// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Represents metadata information for a registered skill action, including its name, description,
    /// input/output schemas, content type, and associated method.
    /// </summary>
    public class SkillActionMetadata
    {
        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the action.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema string describing the input parameters for the action.
        /// </summary>
        public string InputSchema { get; set; }

        /// <summary>
        /// Gets or sets the JSON schema string describing the output type for the action.
        /// </summary>
        public string OutputSchema { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the output (e.g. "application/json", "text/markdown").
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MethodInfo"/> representing the method associated with the action.
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter, or null if parameterless.
        /// </summary>
        public Type ParameterType { get; set; }

        /// <summary>
        /// Appends the JSON representation of this action metadata to the specified StringBuilder.
        /// </summary>
        /// <param name="sb">The StringBuilder to append to.</param>
        public void AppendJson(StringBuilder sb)
        {
            sb.Append("{\"name\":\"");
            sb.Append(Name);
            sb.Append("\",\"description\":\"");
            sb.Append(Description);
            sb.Append("\"");

            if (!string.IsNullOrEmpty(ContentType) && ContentType != "application/json")
            {
                sb.Append(",\"contentType\":\"");
                sb.Append(ContentType);
                sb.Append("\"");
            }

            if (!string.IsNullOrEmpty(InputSchema))
            {
                sb.Append(",\"inputSchema\":");
                sb.Append(InputSchema);
            }

            if (!string.IsNullOrEmpty(OutputSchema))
            {
                sb.Append(",\"outputSchema\":");
                sb.Append(OutputSchema);
            }

            sb.Append("}");
        }
    }
}
