// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Specifies a description for a class member, such as a property or method, for use in documentation or metadata.
    /// </summary>
    public class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the description text associated with the member.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DescriptionAttribute"/> class with the specified description.
        /// </summary>
        /// <param name="description">The description text to associate with the member.</param>
        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
