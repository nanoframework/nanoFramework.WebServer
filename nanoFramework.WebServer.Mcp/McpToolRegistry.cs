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
    public static class McpToolRegistry
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
                return; // Tools already discovered
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
                                if (parameters.Length > 0)
                                {
                                    inputType = McpToolJsonHelper.GenerateInputJson(parameters[0].ParameterType);
                                }

                                tools.Add(attribute.Name, new ToolMetadata
                                {
                                    Name = attribute.Name,
                                    Description = attribute.Description,
                                    InputType = inputType,
                                    OutputType = McpToolJsonHelper.GenerateOutputJson(method.ReturnType, attribute.OutputDescription),
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

        private static bool IsPrimitiveType(Type type)
        {
            return type == typeof(bool) ||
                   type == typeof(byte) ||
                   type == typeof(sbyte) ||
                   type == typeof(char) ||
                   type == typeof(double) ||
                   type == typeof(float) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(short) ||
                   type == typeof(ushort);
        }

        private static object CreateInstance(Type type)
        {
            // Get the default constructor
            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
            {
                throw new Exception($"Type {type.Name} does not have a parameterless constructor");
            }

            return constructor.Invoke(new object[0]);
        }

        /// <summary>
        /// Recursively deserializes a Hashtable into a strongly-typed object by mapping properties and handling nested objects.
        /// </summary>
        /// <param name="hashtable">The Hashtable containing the data to deserialize.</param>
        /// <param name="targetType">The target type to deserialize the data into.</param>
        /// <returns>A new instance of the target type with properties populated from the Hashtable, or null if hashtable or targetType is null.</returns>
        private static object DeserializeFromHashtable(Hashtable hashtable, Type targetType)
        {
            if (hashtable == null || targetType == null)
            {
                return null;
            }

            // For primitive types and strings, try direct conversion
            if (IsPrimitiveType(targetType) || targetType == typeof(string))
            {
                // This shouldn't happen in our context, but handle it gracefully
                return hashtable;
            }

            // Create instance of the target type
            object instance = CreateInstance(targetType);

            // Get all methods of the target type
            MethodInfo[] methods = targetType.GetMethods();

            // Find setter methods (set_PropertyName)
            foreach (MethodInfo method in methods)
            {
                if (!method.Name.StartsWith("set_") || method.GetParameters().Length != 1)
                {
                    continue;
                }

                // Extract property name from setter method name
                string propertyName = method.Name.Substring(4); // Remove "set_" prefix

                // Check if the hashtable contains this property
                if (!hashtable.Contains(propertyName))
                {
                    continue;
                }

                object value = hashtable[propertyName];
                if (value == null)
                {
                    continue;
                }

                try
                {
                    // Get the parameter type of the setter method (which is the property type)
                    Type propertyType = method.GetParameters()[0].ParameterType;

                    // Handle primitive types and strings
                    if (IsPrimitiveType(propertyType) || propertyType == typeof(string))
                    {
                        // Direct assignment for primitive types and strings
                        if (propertyType == typeof(string))
                        {
                            method.Invoke(instance, new object[] { value.ToString() });
                        }
                        else if (propertyType == typeof(int))
                        {
                            method.Invoke(instance, new object[] { Convert.ToInt32(value.ToString()) });
                        }
                        else if (propertyType == typeof(double))
                        {
                            method.Invoke(instance, new object[] { Convert.ToDouble(value.ToString()) });
                        }
                        else if (propertyType == typeof(bool))
                        {
                            try
                            {
                                method.Invoke(instance, new object[] { Convert.ToBoolean(Convert.ToByte(value.ToString())) });
                            }
                            catch (Exception)
                            {
                                method.Invoke(instance, new object[] { value.ToString().ToLower() == "true" ? true : false });
                            }
                        }
                        else if (propertyType == typeof(long))
                        {
                            method.Invoke(instance, new object[] { Convert.ToInt64(value.ToString()) });
                        }
                        else if (propertyType == typeof(float))
                        {
                            method.Invoke(instance, new object[] { Convert.ToSingle(value.ToString()) });
                        }
                        else if (propertyType == typeof(byte))
                        {
                            method.Invoke(instance, new object[] { Convert.ToByte(value.ToString()) });
                        }
                        else if (propertyType == typeof(short))
                        {
                            method.Invoke(instance, new object[] { Convert.ToInt16(value.ToString()) });
                        }
                        else if (propertyType == typeof(char))
                        {
                            try
                            {
                                method.Invoke(instance, new object[] { Convert.ToChar(Convert.ToUInt16(value.ToString())) });
                            }
                            catch (Exception)
                            {
                                method.Invoke(instance, new object[] { string.IsNullOrEmpty(value.ToString()) ? '\0' : value.ToString()[0] });
                            }
                        }
                    }
                    else
                    {
                        // Handle complex types (nested objects)
                        if (value is string stringValue)
                        {
                            // The nested object is serialized as a JSON string
                            var nestedHashtable = (Hashtable)JsonConvert.DeserializeObject(stringValue, typeof(Hashtable));
                            object nestedObject = DeserializeFromHashtable(nestedHashtable, propertyType);
                            method.Invoke(instance, new object[] { nestedObject });
                        }
                        else if (value is Hashtable nestedHashtable)
                        {
                            // The nested object is already a Hashtable
                            object nestedObject = DeserializeFromHashtable(nestedHashtable, propertyType);
                            method.Invoke(instance, new object[] { nestedObject });
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip properties that can't be set
                    continue;
                }
            }

            return instance;
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
                ParameterInfo[] parametersInfo = method.GetParameters();
#if DEBUG
                foreach(ParameterInfo parameterInfo in parametersInfo)
                {
                    Debug.WriteLine($"method type: {parameterInfo.ParameterType.FullName}");
                }
#endif

                object[] methodParams = null;
                if (parametersInfo.Length > 0)
                {
                    methodParams = new object[parametersInfo.Length];
                    Type paramType = parametersInfo[0].ParameterType;

                    if (IsPrimitiveType(paramType) || paramType == typeof(string))
                    {
                        // For primitive types, use direct assignment
                        methodParams[0] = parameter;
                    }
                    else
                    {
                        // For complex types, use our recursive deserialization
                        methodParams[0] = DeserializeFromHashtable(parameter, paramType);
                    }
                }

                object result = method.Invoke(null, methodParams);
                return JsonConvert.SerializeObject(result);
            }

            throw new Exception("Tool not found");
        }
    }
}
