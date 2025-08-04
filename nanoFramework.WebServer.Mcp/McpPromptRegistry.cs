// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Registry for Model Context Protocol (MCP) prompts, allowing discovery and invocation of prompts defined with the <see cref="McpServerPromptAttribute"/>.
    /// </summary>
    public class McpPromptRegistry : RegistryBase
    {
        private static readonly Hashtable promtps = new Hashtable();
        private static bool isInitialized = false;

        /// <summary>
        /// Discovers MCP prompts by scanning the provided types for methods decorated with the <see cref="McpServerPromptAttribute"/>.
        /// This method should be called once to populate the tool registry.
        /// </summary>
        /// <param name="typesWithPrompts">An array of types to scan for MCP prompts.</param>
        public static void DiscoverPrompts(Type[] typesWithPrompts)
        {
            if (isInitialized)
            {
                // prompts already discovered
                return;
            }

            foreach (Type mcpPrompt in typesWithPrompts)
            {
                MethodInfo[] methods = mcpPrompt.GetMethods();

                foreach (MethodInfo method in methods)
                {
                    try
                    {
                        object[] allAttribute = method.GetCustomAttributes(false);

                        foreach (object attrib in allAttribute)
                        {
                            if (attrib.GetType() != typeof(McpServerPromptAttribute))
                            {
                                continue;
                            }

                            McpServerPromptAttribute attribute = (McpServerPromptAttribute)attrib;
                            if (attribute != null)
                            {
                                // validate if the method returns an array of PromptMessage
                                if (method.ReturnType != typeof(PromptMessage[]) && !method.ReturnType.IsArray)
                                {
                                    throw new Exception($"Method {method.Name} does not return an array of PromptMessage.");
                                }

                                promtps.Add(attribute.Name, new PromptMetadata
                                {
                                    Name = attribute.Name,
                                    Description = attribute.Description,
                                    Arguments = ComposeArgumentsAsJson(allAttribute),
                                    Method = method
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }

            isInitialized = true;
        }

        private static string ComposeArgumentsAsJson(object[] attributes)
        {
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;

            foreach (object attrib in attributes)
            {
                if (attrib is not McpPromptParameterAttribute)
                {
                    continue;
                }

                McpPromptParameterAttribute parameterNameAttribute = (McpPromptParameterAttribute)attrib;
                if (parameterNameAttribute != null)
                {
                    sb.Append(isFirst ? "" : ",");

                    sb.Append($"{parameterNameAttribute}");

                    isFirst = false;
                }
            }

            return sb.Length > 0 ? sb.ToString() : string.Empty;
        }

        /// <summary>
        /// Gets the metadata of all registered MCP prompts in JSON format.
        /// This method should be called after <see cref="DiscoverPrompts"/> to retrieve the prompt metadata.
        /// </summary>
        /// <returns>A JSON string containing the metadata of all registered prompts.</returns>
        /// <exception cref="Exception">Thrown if there is an error building the prompts list.</exception>
        public static string GetPromptMetadataJson()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"prompts\":[");

                foreach (PromptMetadata prompt in promtps.Values)
                {
                    sb.Append(prompt.ToString());
                    sb.Append(",");
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append("],\"nextCursor\":null");
                return sb.ToString();
            }
            catch (Exception)
            {
                throw new Exception("Impossible to build prompts list.");
            }
        }

        /// <summary>
        /// Gets the description of a registered MCP prompt by its name.
        /// </summary>
        /// <param name="promptName">The name of the prompt to invoke.</param>
        /// <returns>A string containing the description of the prompt.</returns>
        /// <exception cref="Exception">Thrown when the specified prompt is not found in the registry.</exception>
        public static string GetPromptDescription(string promptName)
        {
            if (promtps.Contains(promptName))
            {
                PromptMetadata promptMetadata = (PromptMetadata)promtps[promptName];
                return promptMetadata.Description;
            }

            throw new Exception("Prompt not found");
        }

        /// <summary>
        /// Invokes a registered MCP prompt by name and returns the serialized result.
        /// </summary>
        /// <param name="promptName">The name of the prompt to invoke.</param>
        /// <param name="arguments">The arguments to pass to the tool.</param>
        /// <returns>A JSON string containing the serialized result of the prompt invocation.</returns>
        /// <exception cref="Exception">Thrown when the specified prompt is not found in the registry.</exception>
        public static string InvokePrompt(string promptName, Hashtable arguments)
        {
            if (promtps.Contains(promptName))
            {
                PromptMetadata promptMetadata = (PromptMetadata)promtps[promptName];
                MethodInfo method = promptMetadata.Method;
                Debug.WriteLine($"Prompt name: {promptName}, method: {method.Name}");

                object[] methodParams = null;

                if (arguments is not null && arguments.Count > 0)
                {
                    methodParams = new object[arguments.Count];

                    arguments.Values.CopyTo(methodParams, 0);
                }

                PromptMessage[] result = (PromptMessage[])method.Invoke(null, methodParams);

                // serialize the result to JSON using a speedy approach with a StringBuilder
                StringBuilder sb = new StringBuilder();

                // start building the JSON response
                sb.Append($"{{\"description\":\"{GetPromptDescription(promptName)}\",\"messages\":[");

                // iterate through the result array and append each message
                for (int i = 0; i < result.Length; i++)
                {
                    sb.Append(result[i]);
                    if (i < result.Length - 1)
                    {
                        sb.Append(",");
                    }
                }

                // close the messages array and the main object
                sb.Append("]}");

                // done here, return the JSON string
                return sb.ToString();
            }

            throw new Exception("Prompt not found");
        }
    }
}
