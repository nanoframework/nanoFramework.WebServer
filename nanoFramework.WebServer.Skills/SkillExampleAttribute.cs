// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Provides an example prompt or scenario for a skill (A2A: AgentSkill.examples).
    /// Apply multiple times to add multiple examples.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SkillExampleAttribute : Attribute
    {
        /// <summary>
        /// Gets the example text.
        /// </summary>
        public string Example { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkillExampleAttribute"/> class.
        /// </summary>
        /// <param name="example">An example prompt or scenario this skill handles.</param>
        public SkillExampleAttribute(string example)
        {
            Example = example;
        }
    }
}
