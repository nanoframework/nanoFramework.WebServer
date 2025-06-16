// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace nanoFramework.WebServer.Mcp
{
    [AttributeUsage(AttributeTargets.Method)]
    public class McpToolAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }
        public string InputType { get; internal set; }
        public string OutputType { get; internal set; }
        public Type DefaultValue { get; }

        public McpToolAttribute(string name, string description, string inputType = null, string outputType = null, Type defaultValue = null)
        {
            Name = name;
            Description = description;
            InputType = inputType;
            OutputType = outputType;
            DefaultValue = defaultValue;
        }

        public void SetInputType(object inputType)
        {
            InputType = McpToolJsonHelper.GenerateInputJson(inputType.GetType());
        }

        public void SetOutputType(string outputType)
        {
            OutputType = McpToolJsonHelper.GenerateInputJson(outputType.GetType());
        }
    }
}
