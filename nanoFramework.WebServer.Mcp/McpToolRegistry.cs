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
        /// Converts a value to the specified primitive type with appropriate type conversion and error handling.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="targetType">The target primitive type to convert to.</param>
        /// <returns>The converted value as the target type.</returns>
        private static object ConvertToPrimitiveType(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            else if (targetType == typeof(int))
            {
                return Convert.ToInt32(value.ToString());
            }
            else if (targetType == typeof(double))
            {
                return Convert.ToDouble(value.ToString());
            }
            else if (targetType == typeof(bool))
            {
                // If it's a 0 or a 1
                if (value.ToString().Length == 1)
                {
                    try
                    {
                        return Convert.ToBoolean(Convert.ToByte(value.ToString()));
                    }
                    catch (Exception)
                    {
                        // Nothing on purpose, we will handle it below
                    }
                }

                // Then it's a tex
                return value.ToString().ToLower() == "true";
            }
            else if (targetType == typeof(long))
            {
                return Convert.ToInt64(value.ToString());
            }
            else if (targetType == typeof(float))
            {
                return Convert.ToSingle(value.ToString());
            }
            else if (targetType == typeof(byte))
            {
                return Convert.ToByte(value.ToString());
            }
            else if (targetType == typeof(short))
            {
                return Convert.ToInt16(value.ToString());
            }
            else if (targetType == typeof(char))
            {
                try
                {
                    return Convert.ToChar(Convert.ToUInt16(value.ToString()));
                }
                catch (Exception)
                {
                    return string.IsNullOrEmpty(value.ToString()) ? '\0' : value.ToString()[0];
                }
            }
            else if (targetType == typeof(uint))
            {
                return Convert.ToUInt32(value.ToString());
            }
            else if (targetType == typeof(ulong))
            {
                return Convert.ToUInt64(value.ToString());
            }
            else if (targetType == typeof(ushort))
            {
                return Convert.ToUInt16(value.ToString());
            }
            else if (targetType == typeof(sbyte))
            {
                return Convert.ToSByte(value.ToString());
            }

            // Fallback - return the original value
            return value;
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
            if (McpToolJsonHelper.IsPrimitiveType(targetType) || targetType == typeof(string))
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
                    Type propertyType = method.GetParameters()[0].ParameterType;                    // Handle primitive types and strings
                    if (McpToolJsonHelper.IsPrimitiveType(propertyType) || propertyType == typeof(string))
                    {
                        // Use the centralized conversion function
                        object convertedValue = ConvertToPrimitiveType(value, propertyType);
                        method.Invoke(instance, new object[] { convertedValue });
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
