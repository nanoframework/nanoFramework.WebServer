// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace McpServerTests
{
    [TestClass]
    public class TestMcpToolsAttributeTests
    {
        [TestMethod]
        public void TestSimpleTypeString()
        {
            // Arrange
            Type inputType = typeof(string);
            string expectedJson = "[{\"name\":\"value\",\"type\":\"string\",\"description\":\"value\"}]";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple string type does not match the expected output.");
        }
    }
}
