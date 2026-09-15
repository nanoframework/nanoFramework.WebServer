// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Represents metadata information for a registered resource, including its URI, name, description, MIME type, and associated method.
    /// </summary>
    public class ResourceMetadata
    {
        /// <summary>
        /// Gets or sets the unique URI of the resource.
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the resource.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MethodInfo"/> representing the method associated with the resource.
        /// </summary>
        public MethodInfo Method { get; set; }

        /// <summary>
        /// Gets or sets the owning instance for instance-method resources. Null for static resources.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Returns a JSON string representation of the resource metadata.
        /// </summary>
        /// <returns>A JSON string containing the resource's URI, name, description, and MIME type.</returns>
        public override string ToString()
        {
            string output = $"{{\"uri\":\"{Uri}\",\"name\":\"{Name}\"";
            output += string.IsNullOrEmpty(Description) ? string.Empty : $",\"description\":\"{Description}\"";
            output += string.IsNullOrEmpty(MimeType) ? string.Empty : $",\"mimeType\":\"{MimeType}\"";
            output += "}";
            return output;
        }
    }
}
