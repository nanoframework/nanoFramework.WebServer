// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Marks a class as a discoverable AI agent skill (A2A AgentSkill compatible).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SkillAttribute : Attribute
    {
        /// <summary>
        /// Gets the unique identifier (A2A: AgentSkill.id).
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the human-readable name (A2A: AgentSkill.name).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the detailed description (A2A: AgentSkill.description).
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the agent/skill version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillAttribute"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this skill.</param>
        /// <param name="name">The human-readable name of this skill.</param>
        /// <param name="description">A detailed description of this skill.</param>
        /// <param name="version">The version of this skill. Defaults to "1.0".</param>
        public SkillAttribute(string id, string name, string description, string version = "1.0")
        {
            Id = id;
            Name = name;
            Description = description;
            Version = version;
        }
    }
}
