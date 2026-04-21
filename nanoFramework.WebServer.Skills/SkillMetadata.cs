// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Text;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Represents metadata information for a registered skill, compatible with the A2A AgentSkill schema.
    /// Contains the skill's identity, description, tags, examples, input/output modes, and actions.
    /// </summary>
    public class SkillMetadata
    {
        /// <summary>
        /// Gets or sets the unique identifier (A2A: AgentSkill.id).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the human-readable name (A2A: AgentSkill.name).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the detailed description (A2A: AgentSkill.description).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the version of the skill.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the tags for this skill (A2A: AgentSkill.tags).
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Gets or sets the example prompts or scenarios (A2A: AgentSkill.examples).
        /// </summary>
        public string[] Examples { get; set; }

        /// <summary>
        /// Gets or sets the input MIME types (A2A: AgentSkill.input_modes).
        /// </summary>
        public string[] InputModes { get; set; }

        /// <summary>
        /// Gets or sets the output MIME types (A2A: AgentSkill.output_modes).
        /// </summary>
        public string[] OutputModes { get; set; }

        /// <summary>
        /// Gets or sets the list of actions (SkillActionMetadata) for this skill.
        /// </summary>
        public ArrayList Actions { get; set; }

        /// <summary>
        /// Finds an action by name within this skill.
        /// </summary>
        /// <param name="actionName">The name of the action to find.</param>
        /// <returns>The matching SkillActionMetadata, or null if not found.</returns>
        public SkillActionMetadata FindAction(string actionName)
        {
            if (Actions == null)
            {
                return null;
            }

            for (int i = 0; i < Actions.Count; i++)
            {
                SkillActionMetadata action = (SkillActionMetadata)Actions[i];
                if (action.Name == actionName)
                {
                    return action;
                }
            }

            return null;
        }

        /// <summary>
        /// Appends the JSON representation of this skill metadata to the specified StringBuilder.
        /// Output is compatible with the A2A AgentSkill schema.
        /// </summary>
        /// <param name="sb">The StringBuilder to append to.</param>
        public void AppendJson(StringBuilder sb)
        {
            sb.Append("{\"id\":\"");
            sb.Append(SkillJsonHelper.EscapeJsonString(Id));
            sb.Append("\",\"name\":\"");
            sb.Append(SkillJsonHelper.EscapeJsonString(Name));
            sb.Append("\",\"description\":\"");
            sb.Append(SkillJsonHelper.EscapeJsonString(Description));
            sb.Append("\",\"version\":\"");
            sb.Append(SkillJsonHelper.EscapeJsonString(Version));
            sb.Append("\"");

            // Tags (A2A required)
            AppendStringArray(sb, "tags", Tags);

            // Examples (A2A optional)
            if (Examples != null && Examples.Length > 0)
            {
                AppendStringArray(sb, "examples", Examples);
            }

            // Input/Output Modes (A2A)
            if (InputModes != null && InputModes.Length > 0)
            {
                AppendStringArray(sb, "inputModes", InputModes);
            }

            if (OutputModes != null && OutputModes.Length > 0)
            {
                AppendStringArray(sb, "outputModes", OutputModes);
            }

            // Actions (extension beyond A2A)
            sb.Append(",\"actions\":[");
            if (Actions != null)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    ((SkillActionMetadata)Actions[i]).AppendJson(sb);
                }
            }

            sb.Append("]}");
        }

        /// <summary>
        /// Checks whether this skill has any tag matching the specified value.
        /// </summary>
        /// <param name="tag">The tag to search for.</param>
        /// <returns>true if the skill has the specified tag; otherwise, false.</returns>
        public bool HasTag(string tag)
        {
            if (Tags == null)
            {
                return false;
            }

            for (int i = 0; i < Tags.Length; i++)
            {
                if (Tags[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendStringArray(StringBuilder sb, string fieldName, string[] values)
        {
            sb.Append(",\"");
            sb.Append(fieldName);
            sb.Append("\":[");
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append("\"");
                    sb.Append(SkillJsonHelper.EscapeJsonString(values[i]));
                    sb.Append("\"");
                }
            }

            sb.Append("]");
        }
    }
}
