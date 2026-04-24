// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using nanoFramework.Json;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Registry for AI agent skills, allowing discovery and invocation of skills defined with the SkillAttribute.
    /// </summary>
    public class SkillRegistry : RegistryBase
    {
        private static readonly Hashtable _skills = new Hashtable();
        private static bool _isInitialized = false;

        /// <summary>
        /// Resets the skill registry, clearing all registered skills.
        /// This is intended for testing scenarios where the registry needs to be re-initialized.
        /// </summary>
        public static void Reset()
        {
            _skills.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// Discovers skills by scanning the provided types for classes decorated with the SkillAttribute.
        /// This method should be called once at startup to populate the skill registry.
        /// </summary>
        /// <param name="skillTypes">An array of types to scan for skills.</param>
        public static void DiscoverSkills(Type[] skillTypes)
        {
            if (_isInitialized)
            {
                return;
            }

            foreach (Type skillType in skillTypes)
            {
                try
                {
                    SkillAttribute skillAttribute = null;
                    ArrayList tags = new ArrayList();
                    ArrayList examples = new ArrayList();

                    // Scan class-level attributes
                    var classAttributes = skillType.GetCustomAttributes(true);
                    foreach (var attr in classAttributes)
                    {
                        if (attr is SkillAttribute sa)
                        {
                            skillAttribute = sa;
                        }
                        else if (attr is SkillTagAttribute tagAttr)
                        {
                            tags.Add(tagAttr.Tag);
                        }
                        else if (attr is SkillExampleAttribute exAttr)
                        {
                            examples.Add(exAttr.Example);
                        }
                    }

                    if (skillAttribute == null)
                    {
                        continue;
                    }

                    // Build tags array
                    string[] tagsArray = new string[tags.Count];
                    for (int i = 0; i < tags.Count; i++)
                    {
                        tagsArray[i] = (string)tags[i];
                    }

                    // Build examples array
                    string[] examplesArray = new string[examples.Count];
                    for (int i = 0; i < examples.Count; i++)
                    {
                        examplesArray[i] = (string)examples[i];
                    }

                    SkillMetadata skillMetadata = new SkillMetadata
                    {
                        Id = skillAttribute.Id,
                        Name = skillAttribute.Name,
                        Description = skillAttribute.Description,
                        Version = skillAttribute.Version,
                        Tags = tagsArray,
                        Examples = examplesArray,
                        Actions = new ArrayList()
                    };

                    // Track unique input/output modes
                    ArrayList inputModes = new ArrayList();
                    ArrayList outputModes = new ArrayList();

                    // Scan methods for actions
                    MethodInfo[] methods = skillType.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        try
                        {
                            var methodAttributes = method.GetCustomAttributes(true);
                            foreach (var attr in methodAttributes)
                            {
                                if (attr.GetType() != typeof(SkillActionAttribute))
                                {
                                    continue;
                                }

                                SkillActionAttribute actionAttr = (SkillActionAttribute)attr;
                                var parameters = method.GetParameters();

                                string inputSchema = string.Empty;
                                // We only support either no parameters or one parameter
                                if (parameters.Length == 1)
                                {
                                    inputSchema = SkillJsonHelper.GenerateInputJson(parameters[0].ParameterType);
                                }
                                else if (parameters.Length > 1)
                                {
                                    continue;
                                }

                                string outputSchema = string.Empty;
                                if (!SkillJsonHelper.IsPrimitiveType(method.ReturnType)
                                    && method.ReturnType != typeof(string)
                                    && method.ReturnType != typeof(void))
                                {
                                    outputSchema = SkillJsonHelper.GenerateOutputJson(method.ReturnType, actionAttr.OutputDescription);
                                }

                                SkillActionMetadata actionMetadata = new SkillActionMetadata
                                {
                                    Name = actionAttr.Name,
                                    Description = actionAttr.Description,
                                    InputSchema = inputSchema,
                                    OutputSchema = outputSchema,
                                    ContentType = actionAttr.ContentType,
                                    Method = method,
                                    ParameterType = parameters.Length > 0 ? parameters[0].ParameterType : null
                                };

                                skillMetadata.Actions.Add(actionMetadata);

                                // Track input modes
                                if (parameters.Length > 0)
                                {
                                    AddIfNotPresent(inputModes, "application/json");
                                }
                                else
                                {
                                    AddIfNotPresent(inputModes, "text/plain");
                                }

                                // Track output modes
                                AddIfNotPresent(outputModes, actionAttr.ContentType);
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    // Build input/output mode arrays
                    skillMetadata.InputModes = ToStringArray(inputModes);
                    skillMetadata.OutputModes = ToStringArray(outputModes);

                    _skills.Add(skillAttribute.Id, skillMetadata);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Gets the JSON array of all registered skills.
        /// </summary>
        /// <returns>A JSON array string containing all skills metadata.</returns>
        public static string GetSkillsArrayJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            bool first = true;
            foreach (SkillMetadata skill in _skills.Values)
            {
                if (!first)
                {
                    sb.Append(",");
                }

                skill.AppendJson(sb);
                first = false;
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the JSON for a single skill by its identifier.
        /// </summary>
        /// <param name="skillId">The skill identifier.</param>
        /// <returns>A JSON string for the skill, or null if not found.</returns>
        public static string GetSkillJson(string skillId)
        {
            if (!_skills.Contains(skillId))
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            ((SkillMetadata)_skills[skillId]).AppendJson(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Gets the JSON array of skills filtered by tag.
        /// </summary>
        /// <param name="tag">The tag to filter by.</param>
        /// <returns>A JSON array string containing matching skills.</returns>
        public static string GetSkillsByTagJson(string tag)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            bool first = true;
            foreach (SkillMetadata skill in _skills.Values)
            {
                if (skill.HasTag(tag))
                {
                    if (!first)
                    {
                        sb.Append(",");
                    }

                    skill.AppendJson(sb);
                    first = false;
                }
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the content type for a specific action within a skill.
        /// </summary>
        /// <param name="skillId">The skill identifier.</param>
        /// <param name="actionName">The action name.</param>
        /// <returns>The MIME content type, or null if the skill or action is not found.</returns>
        public static string GetActionContentType(string skillId, string actionName)
        {
            if (!_skills.Contains(skillId))
            {
                return null;
            }

            SkillMetadata skill = (SkillMetadata)_skills[skillId];
            SkillActionMetadata action = skill.FindAction(actionName);
            return action?.ContentType;
        }

        /// <summary>
        /// Invokes a skill action by skill identifier and action name.
        /// </summary>
        /// <param name="skillId">The skill identifier.</param>
        /// <param name="actionName">The action name.</param>
        /// <param name="parameters">The parameters to pass to the action as a Hashtable.</param>
        /// <returns>The serialized result string. For text/markdown or text/plain actions, the raw string is returned.
        /// For JSON actions, the JSON-serialized result is returned.</returns>
        /// <exception cref="Exception">Thrown when the skill is not found in the registry.</exception>
        /// <exception cref="Exception">Thrown when the action is not found in the skill.</exception>
        /// <exception cref="Exception">Thrown when the action requires parameters but none were provided.</exception>
        public static string InvokeAction(string skillId, string actionName, Hashtable parameters)
        {
            if (!_skills.Contains(skillId))
            {
                throw new Exception();
            }

            SkillMetadata skill = (SkillMetadata)_skills[skillId];
            SkillActionMetadata action = skill.FindAction(actionName);
            if (action == null)
            {
                throw new Exception();
            }

            Debug.WriteLine($"Skill: {skillId}, Action: {actionName}, Method: {action.Method.Name}");

            object[] methodParams = null;
            if (action.ParameterType != null)
            {
                if (parameters == null)
                {
                    throw new Exception();
                }

                methodParams = new object[1];
                Type paramType = action.ParameterType;
                if (SkillJsonHelper.IsPrimitiveType(paramType) || paramType == typeof(string))
                {
                    object value = parameters["value"];
                    if (value != null)
                    {
                        methodParams[0] = ConvertToPrimitiveType(value, paramType);
                    }
                }
                else
                {
                    methodParams[0] = DeserializeFromHashtable(parameters, paramType);
                }
            }

            object result = action.Method.Invoke(null, methodParams);

            // For text-based content types, return raw string
            if (action.ContentType == "text/markdown" || action.ContentType == "text/plain")
            {
                return result?.ToString() ?? string.Empty;
            }

            // For JSON content types, serialize the result
            if (result == null)
            {
                return "null";
            }

            Type resultType = result.GetType();

            if (SkillJsonHelper.IsPrimitiveType(resultType) || resultType == typeof(string))
            {
                var stringResult = result.GetType() == typeof(bool) ? result.ToString().ToLower() : result.ToString();
                return "\"" + stringResult + "\"";
            }
            else
            {
                return JsonConvert.SerializeObject(result);
            }
        }

        private static void AddIfNotPresent(ArrayList list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if ((string)list[i] == value)
                {
                    return;
                }
            }

            list.Add(value);
        }

        private static string[] ToStringArray(ArrayList list)
        {
            string[] result = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = (string)list[i];
            }

            return result;
        }
    }
}
