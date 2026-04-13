// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Adds a discoverable tag to a skill class (A2A: AgentSkill.tags).
    /// Apply multiple times to add multiple tags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SkillTagAttribute : Attribute
    {
        /// <summary>
        /// Gets the tag value.
        /// </summary>
        public string Tag { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillTagAttribute"/> class.
        /// </summary>
        /// <param name="tag">The tag keyword for this skill.</param>
        public SkillTagAttribute(string tag)
        {
            Tag = tag;
        }
    }
}
