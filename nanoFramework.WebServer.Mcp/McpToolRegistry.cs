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

                                tools.Add(attribute.Name, new ToolMetadata
                                {
                                    Name = attribute.Name,
                                    Description = attribute.Description,
                                    InputType = inputType,
                                    OutputType = McpToolJsonHelper.GenerateOutputJson(method.ReturnType, attribute.OutputDescription),
                                    Method = method,
                                    MethodType = parameters.Length > 0 ? parameters[0].ParameterType : null,
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

                sb.Remove(sb.Length - 1, 1);
                sb.Append("],\"nextCursor\":null");
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

                object result = method.Invoke(null, methodParams);

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
