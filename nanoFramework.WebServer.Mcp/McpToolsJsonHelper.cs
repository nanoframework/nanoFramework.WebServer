// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Mcp
{
    public static class McpToolJsonHelper
    {
        public static string GenerateInputJson(Type inputType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            AppendPropertiesJson(sb, inputType, true);
            sb.Append("]");
            return sb.ToString();
        }

        private static void AppendPropertiesJson(StringBuilder sb, Type type, bool isFirst)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            // Find all property names by looking for get_ methods
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name.StartsWith("get_") && method.GetParameters().Length == 0)
                {
                    string propName = method.Name.Substring(4);
                    Type propType = method.ReturnType;

                    if (!isFirst) sb.Append(",");
                    isFirst = false;

                    sb.Append("{");
                    sb.Append("\"name\":\"").Append(propName).Append("\",");
                    string mappedType = MapType(propType);
                    sb.Append("\"type\":\"").Append(mappedType).Append("\",");
                    sb.Append("\"description\":\"").Append(propName).Append("\"");
                    if (mappedType == "object")
                    {
                        sb.Append(",\"properties\":[");
                        AppendPropertiesJson(sb, propType, true);
                        sb.Append("]");
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
                type == typeof(long) || type == typeof(short) || type == typeof(byte))
            {
                return "number";
            }
            else if (type == typeof(bool))
            {
                return "boolean";
            }
            else if (type.IsArray)
            {
                return "array";
            }
            else
            if (type.IsClass && type != typeof(string))
            {
                return "object";
            }

            return "string";
        }
    }
}
