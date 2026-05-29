// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Provides utility methods for generating JSON schemas that describe the input and output parameters of skill actions.
    /// </summary>
    public static class SkillJsonHelper
    {
        /// <summary>
        /// Generates a JSON object schema describing the input parameters for a skill action.
        /// </summary>
        /// <param name="inputType">The <see cref="Type"/> representing the input parameter type.</param>
        /// <returns>A JSON string representing the input parameters schema.</returns>
        public static string GenerateInputJson(Type inputType)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{\"type\":\"object\",\"properties\":{");
            AppendInputPropertiesJson(sb, inputType, true);
            sb.Append("},\"required\":[]}");

            return sb.ToString();
        }

        /// <summary>
        /// Checks if the specified <see cref="Type"/> is a primitive type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to check.</param>
        /// <returns><c>true</c> if the type is a primitive type; otherwise, <c>false</c>.</returns>
        public static bool IsPrimitiveType(Type type)
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

        /// <summary>
        /// Generates a JSON object describing the output schema for a skill action.
        /// </summary>
        /// <param name="outputType">The <see cref="Type"/> of the output object.</param>
        /// <param name="description">A description of the output.</param>
        /// <returns>A JSON string representing the output schema.</returns>
        public static string GenerateOutputJson(Type outputType, string description)
        {
            StringBuilder sb = new StringBuilder();
            AppendOutputJson(sb, outputType, description);
            return sb.ToString();
        }

        private static void AppendOutputJson(StringBuilder sb, Type type, string description)
        {
            string mappedType = MapType(type);

            sb.Append("{");
            sb.Append("\"type\":\"").Append(mappedType).Append("\"");

            bool hasDescription = !string.IsNullOrEmpty(description);
            if (hasDescription)
            {
                sb.Append(",\"description\":\"").Append(EscapeJsonString(description)).Append("\"");
            }

            if (mappedType == "object")
            {
                sb.Append(",\"properties\":{");
                AppendOutputPropertiesJson(sb, type, true);
                sb.Append("}");
            }

            sb.Append("}");
        }

        private static void AppendOutputPropertiesJson(StringBuilder sb, Type type, bool isFirst)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name.StartsWith("get_") && method.GetParameters().Length == 0)
                {
                    string propName = method.Name.Substring(4);

                    Type propType = method.ReturnType;
                    string mappedType = MapType(propType);

                    if (!isFirst)
                    {
                        sb.Append(",");
                    }

                    isFirst = false;

                    sb.Append("\"").Append(propName).Append("\":");
                    if (mappedType == "object")
                    {
                        AppendOutputJson(sb, propType, GetTypeDescription(method, propName));
                    }
                    else
                    {
                        sb.Append("{");
                        sb.Append("\"type\":\"").Append(mappedType).Append("\",");
                        sb.Append("\"description\":\"").Append(EscapeJsonString(GetTypeDescription(method, propName))).Append("\"");
                        sb.Append("}");
                    }
                }
            }
        }

        private static string GetTypeDescription(MethodInfo method, string propName)
        {
            var atibs = method.GetCustomAttributes(false);
            string desc = propName;
            for (int j = 0; j < atibs.Length; j++)
            {
                if (atibs[j] is DescriptionAttribute descAttrib)
                {
                    desc = descAttrib.Description;
                    break;
                }
            }

            return desc;
        }

        private static void AppendInputPropertiesJson(StringBuilder sb, Type type, bool isFirst)
        {
            // If it's a primitive type or string, create a single property entry
            if (IsPrimitiveType(type) || type == typeof(string))
            {
                string mappedType = MapType(type);
                if (!isFirst)
                {
                    sb.Append(",");
                }

                sb.Append("\"value\":{");
                sb.Append("\"type\":\"").Append(mappedType).Append("\",");
                sb.Append("\"description\":\"Input parameter of type ").Append(type.Name).Append("\"");
                sb.Append("}");
                return;
            }

            // For complex types, analyze methods to find properties
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name.StartsWith("get_") && method.GetParameters().Length == 0)
                {
                    string propName = method.Name.Substring(4);
                    Type propType = method.ReturnType;

                    if (!isFirst)
                    {
                        sb.Append(",");
                    }

                    isFirst = false;
                    sb.Append($"\"{propName}\":{{");
                    string mappedType = MapType(propType);
                    sb.Append("\"type\":\"").Append(mappedType).Append("\",");
                    sb.Append("\"description\":\"").Append(EscapeJsonString(GetTypeDescription(method, propName))).Append("\"");
                    if (mappedType == "object")
                    {
                        sb.Append(",\"properties\":{");
                        AppendInputPropertiesJson(sb, propType, true);
                        sb.Append("}");
                    }

                    sb.Append("}");
                }
            }
        }

        private static string MapType(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(int) || type == typeof(double) || type == typeof(float) ||
                type == typeof(long) || type == typeof(short) || type == typeof(byte) ||
                type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
                type == typeof(sbyte))
            {
                return "number";
            }
            else if (type == typeof(bool))
            {
                return "boolean";
            }
            else if (type == typeof(char))
            {
                return "string";
            }
            else if (type.IsArray)
            {
                return "array";
            }
            else if (type.IsClass && type != typeof(string))
            {
                return "object";
            }

            return "string";
        }

        /// <summary>
        /// Escapes special characters in a string for use in JSON values.
        /// Handles backslash, double-quote, newline, carriage return, and tab.
        /// </summary>
        /// <param name="value">The string to escape.</param>
        /// <returns>The escaped string safe for JSON embedding.</returns>
        internal static string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            StringBuilder sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
