// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using nanoFramework.Json;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Registry for Model Context Protocol (MCP) tools, allowing discovery and invocation of tools defined with the McpServerToolAttribute.
    /// </summary>
    public class McpToolRegistry : RegistryBase
    {
        private static readonly Hashtable tools = new Hashtable();
        private static bool isInitialized = false;

        /// <summary>
        /// Discovers MCP tools by scanning the provided types for methods decorated with the McpServerToolAttribute.
        /// This method should be called once to populate the tool registry.
        /// </summary>
        /// <param name="mcpTools">An array of types to scan for MCP tools.</param>
        public static void DiscoverTools(Type[] mcpTools)
        {
            if (isInitialized)
            {
                // Tools already discovered
                return;
            }

            foreach (Type mcpTool in mcpTools)
            {
                RegisterTools(mcpTool, null, null);
            }

            isInitialized = true;
        }

        /// <summary>
        /// Discovers MCP tools by scanning the provided object instances for methods decorated with the McpServerToolAttribute.
        /// Instance methods are invoked against the object they were discovered on, allowing live device objects to expose tools.
        /// This overload is additive and can be called in addition to the static <see cref="DiscoverTools(Type[])"/> discovery.
        /// </summary>
        /// <param name="toolInstances">An array of object instances to scan for MCP tools.</param>
        public static void DiscoverTools(object[] toolInstances)
        {
            DiscoverTools(toolInstances, null);
        }

        /// <summary>
        /// Discovers MCP tools on the provided object instances, prepending an optional per-instance name prefix to every
        /// discovered tool name so that several instances of the same type can be registered without name collisions.
        /// This overload is additive and can be called in addition to the static <see cref="DiscoverTools(Type[])"/> discovery.
        /// </summary>
        /// <param name="toolInstances">An array of object instances to scan for MCP tools.</param>
        /// <param name="toolNamePrefixes">An optional parallel array of name prefixes; a null or empty entry means no prefix. May be null to apply no prefixes.</param>
        public static void DiscoverTools(object[] toolInstances, string[] toolNamePrefixes)
        {
            if (toolInstances == null)
            {
                return;
            }

            for (int i = 0; i < toolInstances.Length; i++)
            {
                object instance = toolInstances[i];
                if (instance == null)
                {
                    continue;
                }

                string prefix = (toolNamePrefixes != null && i < toolNamePrefixes.Length) ? toolNamePrefixes[i] : null;
                RegisterTools(instance.GetType(), instance, prefix);
            }
        }

        /// <summary>
        /// Scans a type for methods decorated with the McpServerToolAttribute and registers each as a tool.
        /// </summary>
        /// <param name="mcpTool">The type to scan.</param>
        /// <param name="target">The instance to invoke discovered instance methods against, or null for static-only discovery.</param>
        /// <param name="namePrefix">An optional prefix prepended to every discovered tool name; null or empty means no prefix.</param>
        private static void RegisterTools(Type mcpTool, object target, string namePrefix)
        {
            MethodInfo[] methods = mcpTool.GetMethods();
            foreach (MethodInfo method in methods)
            {
                try
                {
                    var allAttribute = method.GetCustomAttributes(true);
                    foreach (var attrib in allAttribute)
                    {
                        if (attrib.GetType() != typeof(McpServerToolAttribute))
                        {
                            continue;
                        }

                        McpServerToolAttribute attribute = (McpServerToolAttribute)attrib;
                        if (attribute != null)
                        {
                            var parameters = method.GetParameters();
                            string inputType = string.Empty;
                            // We only support either no parameters or one parameter for now
                            if (parameters.Length == 1)
                            {
                                inputType = McpToolJsonHelper.GenerateInputJson(parameters[0].ParameterType);
                            }
                            else if (parameters.Length > 1)
                            {
                                continue;
                            }

                            string toolName = string.IsNullOrEmpty(namePrefix) ? attribute.Name : namePrefix + attribute.Name;

                            tools.Add(toolName, new ToolMetadata
                            {
                                Name = toolName,
                                Description = attribute.Description,
                                InputType = inputType,
                                OutputType = !McpToolJsonHelper.IsPrimitiveType(method.ReturnType) && method.ReturnType != typeof(string)
                                    ? McpToolJsonHelper.GenerateOutputJson(method.ReturnType, attribute.OutputDescription)
                                    : string.Empty,
                                Method = method,
                                MethodType = parameters.Length > 0 ? parameters[0].ParameterType : null,
                                Target = (target != null && !method.IsStatic) ? target : null,
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

        /// <summary>
        /// Gets the metadata of all registered MCP tools in JSON format.
        /// This method should be called after DiscoverTools to retrieve the tool metadata.
        /// </summary>
        /// <returns>A JSON string containing the metadata of all registered tools.</returns>
        /// <exception cref="Exception">Thrown if there is an error building the tools list.</exception>
        public static string GetToolMetadataJson()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"tools\":[");

                foreach (ToolMetadata tool in tools.Values)
                {
                    sb.Append(tool.ToString());
                    sb.Append(",");
                }

                if (tools.Count > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("]");
                return sb.ToString();
            }
            catch (Exception)
            {
                throw new Exception("Impossible to build tools list.");
            }
        }

        /// <summary>
        /// Invokes a registered MCP tool by name with the specified parameters and returns the serialized result.
        /// </summary>
        /// <param name="toolName">The name of the tool to invoke.</param>
        /// <param name="parameter">The parameters to pass to the tool as a Hashtable.</param>
        /// <returns>A JSON string containing the serialized result of the tool invocation.</returns>
        /// <exception cref="Exception">Thrown when the specified tool is not found in the registry.</exception>
        public static string InvokeTool(string toolName, Hashtable parameter)
        {
            if (tools.Contains(toolName))
            {
                ToolMetadata toolMetadata = (ToolMetadata)tools[toolName];
                MethodInfo method = toolMetadata.Method;
                Debug.WriteLine($"Tool name: {toolName}, method: {method.Name}");

                object[] methodParams = null;
                if (toolMetadata.MethodType != null)
                {
                    methodParams = new object[1];
                    Type paramType = toolMetadata.MethodType;
                    if (McpToolJsonHelper.IsPrimitiveType(paramType) || paramType == typeof(string))
                    {
                        // For primitive types, extract the "value" key and convert to target type
                        object value = parameter["value"];
                        if (value != null)
                        {
                            methodParams[0] = ConvertToPrimitiveType(value, paramType);
                        }
                    }
                    else
                    {
                        // For complex types, use our recursive deserialization
                        methodParams[0] = DeserializeFromHashtable(parameter, paramType);
                    }
                }

                object result = method.Invoke(toolMetadata.Target, methodParams);

                // Handle serialization based on return type
                if (result == null)
                {
                    return "null";
                }

                Type resultType = result.GetType();

                // For strings, return as-is with quotes
                // For primitive types, convert to string and add quotes
                if (McpToolJsonHelper.IsPrimitiveType(resultType) || resultType == typeof(string))
                {
                    var stringResult = result.GetType() == typeof(bool) ? result.ToString().ToLower() : result.ToString();
                    return "\"" + stringResult + "\"";
                }
                // For complex objects, serialize to JSON and add quotes around the entire JSON
                else
                {
                    string jsonResult = JsonConvert.SerializeObject(result);
                    return JsonConvert.SerializeObject(jsonResult);
                }
            }

            throw new Exception("Tool not found");
        }
    }
}
