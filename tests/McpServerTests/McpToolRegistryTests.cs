// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    // Test parameter classes
    public class TestParameter
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class NestedTestParameter
    {
        public string Title { get; set; }
        public TestParameter Details { get; set; }
        public bool IsEnabled { get; set; }
    }

    // Test tool classes with MCP tools
    public static class TestToolsClass
    {
        [McpServerTool("simple_tool", "A simple test tool")]
        public static string SimpleTool(string input)
        {
            return $"Processed: {input}";
        }

        [McpServerTool("complex_tool", "A complex test tool", "Complex result")]
        public static TestParameter ComplexTool(TestParameter param)
        {
            return new TestParameter { Name = param.Name + "_processed", Value = param.Value * 2 };
        }

        [McpServerTool("nested_tool", "A nested parameter tool", "Nested result")]
        public static string NestedTool(NestedTestParameter param)
        {
            return $"{param.Title}: {param.Details.Name} = {param.Details.Value}, Enabled: {param.IsEnabled}";
        }

        [McpServerTool("primitive_int_tool", "A primitive int tool")]
        public static int PrimitiveIntTool(int number)
        {
            return number * 2;
        }

        [McpServerTool("primitive_bool_tool", "A primitive bool tool")]
        public static bool PrimitiveBoolTool(bool flag)
        {
            return !flag;
        }

        [McpServerTool("no_param_tool", "A tool with no parameters")]
        public static string NoParamTool()
        {
            return "No parameters required";
        }

        // Method without attribute - should be ignored
        public static string NonToolMethod(string input)
        {
            return input;
        }
    }

    public static class EmptyToolsClass
    {
        // No MCP tools
        public static string RegularMethod()
        {
            return "Not a tool";
        }
    }
    [TestClass]
    public class McpToolRegistryTests
    {
        [TestMethod]
        public void TestDiscoverToolsAndGetMetadataSimple()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("\"tools\":["), "Metadata should contain tools array");
            Assert.IsTrue(metadataJson.Contains("\"nextCursor\":null"), "Metadata should contain nextCursor");
            Assert.IsTrue(metadataJson.Contains("simple_tool"), "Metadata should contain simple_tool");
            Assert.IsTrue(metadataJson.Contains("complex_tool"), "Metadata should contain complex_tool");
            Assert.IsTrue(metadataJson.Contains("nested_tool"), "Metadata should contain nested_tool");
            Assert.IsTrue(metadataJson.Contains("primitive_int_tool"), "Metadata should contain primitive_int_tool");
            Assert.IsTrue(metadataJson.Contains("primitive_bool_tool"), "Metadata should contain primitive_bool_tool");
            Assert.IsTrue(metadataJson.Contains("no_param_tool"), "Metadata should contain no_param_tool");
            Assert.IsFalse(metadataJson.Contains("NonToolMethod"), "Metadata should not contain non-tool methods");
        }

        [TestMethod]
        public void TestDiscoverToolsEmptyClass()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(EmptyToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("\"tools\":["), "Metadata should contain tools array");
            Assert.IsTrue(metadataJson.Contains("\"nextCursor\":null"), "Metadata should contain nextCursor");
            // Should have empty tools array or just the tools from previous test
        }

        [TestMethod]
        public void TestDiscoverToolsMultipleClasses()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass), typeof(EmptyToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("simple_tool"), "Metadata should contain tools from TestToolsClass");
            Assert.IsFalse(metadataJson.Contains("RegularMethod"), "Metadata should not contain regular methods");
        }

        [TestMethod]
        public void TestGetMetadataJsonStructure()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");

            // Check overall structure
            Assert.IsTrue(metadataJson.StartsWith("\"tools\":["), "Metadata should start with tools array");
            Assert.IsTrue(metadataJson.EndsWith("],\"nextCursor\":null"), "Metadata should end with nextCursor");

            // Check for tool properties
            Assert.IsTrue(metadataJson.Contains("\"name\":"), "Metadata should contain tool names");
            Assert.IsTrue(metadataJson.Contains("\"description\":"), "Metadata should contain tool descriptions");
            Assert.IsTrue(metadataJson.Contains("\"inputSchema\":"), "Metadata should contain input schemas");

            // Check specific tool content
            Assert.IsTrue(metadataJson.Contains("A simple test tool"), "Metadata should contain tool descriptions");
            Assert.IsTrue(metadataJson.Contains("A complex test tool"), "Metadata should contain complex tool description");
        }

        [TestMethod]
        public void TestGetMetadataJsonWithPrimitiveTypes()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");

            // Check for primitive type handling
            Assert.IsTrue(metadataJson.Contains("primitive_int_tool"), "Metadata should contain primitive int tool");
            Assert.IsTrue(metadataJson.Contains("primitive_bool_tool"), "Metadata should contain primitive bool tool");

            // Verify primitive types are handled correctly in input schema
            Assert.IsTrue(metadataJson.Contains("\"type\":\"object\""), "Primitive inputs should be wrapped in object schema");
            Assert.IsTrue(metadataJson.Contains("\"value\""), "Primitive inputs should have value property");
        }

        [TestMethod]
        public void TestGetMetadataJsonWithComplexTypes()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");

            // Check for complex type handling
            Assert.IsTrue(metadataJson.Contains("complex_tool"), "Metadata should contain complex tool");
            Assert.IsTrue(metadataJson.Contains("nested_tool"), "Metadata should contain nested tool");

            // Verify complex types have proper object schemas
            Assert.IsTrue(metadataJson.Contains("\"properties\""), "Complex types should have properties");
            Assert.IsTrue(metadataJson.Contains("Name"), "Complex types should include property names");
            Assert.IsTrue(metadataJson.Contains("Value"), "Complex types should include property names");
        }

        [TestMethod]
        public void TestGetMetadataJsonWithNoParameterTool()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };

            // Act
            McpToolRegistry.DiscoverTools(toolTypes);
            string metadataJson = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("no_param_tool"), "Metadata should contain no parameter tool");

            // Tools with no parameters should still have valid structure
            Assert.IsTrue(metadataJson.Contains("A tool with no parameters"), "Should contain no-param tool description");
        }

        [TestMethod]
        public void TestDiscoverToolsCalledMultipleTimes()
        {
            // Arrange
            Type[] toolTypes1 = new Type[] { typeof(TestToolsClass) };
            Type[] toolTypes2 = new Type[] { typeof(EmptyToolsClass) };

            // Act - Call DiscoverTools multiple times
            McpToolRegistry.DiscoverTools(toolTypes1);
            string firstCall = McpToolRegistry.GetToolMetadataJson();

            McpToolRegistry.DiscoverTools(toolTypes2); // Should be ignored as already initialized
            string secondCall = McpToolRegistry.GetToolMetadataJson();

            // Assert
            Assert.AreEqual(firstCall, secondCall, "Multiple calls to DiscoverTools should not change the result");
            Assert.IsTrue(firstCall.Contains("simple_tool"), "Should still contain tools from first discovery");
        }

        [TestMethod]
        public void TestGetMetadataJsonEmptyRegistry()
        {
            // Note: This test might not work in isolation due to static nature
            // But it tests the exception handling
            try
            {
                // This should work even with empty tools if the registry has been initialized
                string metadataJson = McpToolRegistry.GetToolMetadataJson();
                Assert.IsNotNull(metadataJson, "Metadata JSON should not be null even for empty registry");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Impossible to build tools list.", ex.Message, "Should throw expected exception for empty tools");
            }
        }

        // Tests for InvokeTool function
        [TestMethod]
        public void TestInvokeToolSimpleStringType()
        {
            // Arrange - Simulate how HandleMcpRequest creates the Hashtable for simple types
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create Hashtable as it would be created from JSON: {"value":"Laurent is the best"}
            Hashtable arguments = new Hashtable();
            arguments.Add("value", "Laurent is the best");

            // Act
            string result = McpToolRegistry.InvokeTool("simple_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("Processed: Laurent is the best"), "Result should contain processed string");
        }

        [TestMethod]
        public void TestInvokeToolPrimitiveIntType()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create Hashtable for primitive int: {"value":"42"}
            Hashtable arguments = new Hashtable();
            arguments.Add("value", "42"); // JSON numbers come as strings

            // Act
            string result = McpToolRegistry.InvokeTool("primitive_int_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("84"), "Result should contain doubled value (42 * 2 = 84)");
        }

        [TestMethod]
        public void TestInvokeToolPrimitiveBoolType()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create Hashtable for primitive bool: {"value":"true"}
            Hashtable arguments = new Hashtable();
            arguments.Add("value", "true");

            // Act
            string result = McpToolRegistry.InvokeTool("primitive_bool_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("false"), "Result should contain inverted boolean (true -> false)");
        }

        [TestMethod]
        public void TestInvokeToolComplexType()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create Hashtable for complex type: {"Name":"John","Value":"100"}
            Hashtable arguments = new Hashtable();
            arguments.Add("Name", "John");
            arguments.Add("Value", "100");

            // Act
            string result = McpToolRegistry.InvokeTool("complex_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("John_processed"), "Result should contain processed name");
            Assert.IsTrue(result.Contains("200"), "Result should contain doubled value (100 * 2 = 200)");
        }

        [TestMethod]
        public void TestInvokeToolNestedComplexType()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create Hashtable for nested type - simulating the JSON structure:
            // {"Title":"Test","IsEnabled":"true","Details":"{\"Name\":\"John\",\"Value\":\"50\"}"}
            Hashtable arguments = new Hashtable();
            arguments.Add("Title", "Test Title");
            arguments.Add("IsEnabled", "true");
            arguments.Add("Details", "{\"Name\":\"John\",\"Value\":\"50\"}"); // Nested object as JSON string

            // Act
            string result = McpToolRegistry.InvokeTool("nested_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("Test Title"), "Result should contain the title");
            Assert.IsTrue(result.Contains("John"), "Result should contain nested name");
            Assert.IsTrue(result.Contains("50"), "Result should contain nested value");
            Assert.IsTrue(result.Contains("Enabled: True"), "Result should contain enabled status");
        }

        [TestMethod]
        public void TestInvokeToolNoParameters()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create empty Hashtable for no parameters
            Hashtable arguments = new Hashtable();

            // Act
            string result = McpToolRegistry.InvokeTool("no_param_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("No parameters required"), "Result should contain expected message");
        }

        [TestMethod]
        public void TestInvokeToolNonExistentTool()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            Hashtable arguments = new Hashtable();
            arguments.Add("value", "test");

            // Act & Assert
            Assert.ThrowsException(typeof(Exception), () =>
            {
                McpToolRegistry.InvokeTool("non_existent_tool", arguments);
            }, "Should throw exception for non-existent tool");
        }

        [TestMethod]
        public void TestInvokeToolWithNullArguments()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Act
            string result = McpToolRegistry.InvokeTool("no_param_tool", null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null even with null arguments");
        }

        [TestMethod]
        public void TestInvokeToolPrimitiveTypeConversions()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Test different string representations of numbers
            Hashtable arguments1 = new Hashtable();
            arguments1.Add("value", "0");
            string result1 = McpToolRegistry.InvokeTool("primitive_int_tool", arguments1);
            Assert.IsTrue(result1.Contains("0"), "Should handle zero correctly");

            Hashtable arguments2 = new Hashtable();
            arguments2.Add("value", "-10");
            string result2 = McpToolRegistry.InvokeTool("primitive_int_tool", arguments2);
            Assert.IsTrue(result2.Contains("-20"), "Should handle negative numbers correctly");

            // Test boolean variations
            Hashtable arguments3 = new Hashtable();
            arguments3.Add("value", "false");
            string result3 = McpToolRegistry.InvokeTool("primitive_bool_tool", arguments3);
            Assert.IsTrue(result3.Contains("true"), "Should handle false -> true conversion");
        }

        [TestMethod]
        public void TestInvokeToolComplexTypeWithMissingProperties()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            // Create Hashtable with only partial properties
            Hashtable arguments = new Hashtable();
            arguments.Add("Name", "PartialJohn");
            // Missing Value property

            // Act
            string result = McpToolRegistry.InvokeTool("complex_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("PartialJohn_processed"), "Should process available properties");
            // Should handle missing Value property gracefully (default to 0)
        }

        [TestMethod]
        public void TestInvokeToolReturnJsonSerialization()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestToolsClass) };
            McpToolRegistry.DiscoverTools(toolTypes);

            Hashtable arguments = new Hashtable();
            arguments.Add("Name", "TestUser");
            arguments.Add("Value", "25");

            // Act
            string result = McpToolRegistry.InvokeTool("complex_tool", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            // Result should be valid JSON
            Assert.IsTrue(result.StartsWith("{") || result.StartsWith("\""), "Result should be valid JSON");
            Assert.IsTrue(result.Contains("TestUser_processed"), "Should contain processed data");
            Assert.IsTrue(result.Contains("50"), "Should contain doubled value");
        }
    }
}
