// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Marks a static method as an invokable action within a skill.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SkillActionAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the action.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the description of the action's output.
        /// </summary>
        public string OutputDescription { get; }

        /// <summary>
        /// Gets the MIME type of the output. Defaults to "application/json".
        /// Set to "text/markdown" for actions that return markdown documents.
        /// </summary>
        public string ContentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillActionAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="description">The description of the action.</param>
        /// <param name="outputDescription">The description of the action's output.</param>
        /// <param name="contentType">The MIME type of the output. Defaults to "application/json".</param>
        public SkillActionAttribute(string name, string description = "",
            string outputDescription = "", string contentType = "application/json")
        {
            Name = name;
            Description = description;
            OutputDescription = outputDescription;
            ContentType = contentType;
        }
    }
}
