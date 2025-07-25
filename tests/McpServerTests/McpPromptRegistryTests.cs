// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    public class PromptParameter
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    // Test prompt classes with MCP prompts
    public static class TestPromptsClass
    {
        [McpServerPrompt("simple_prompt", "A simple prompt description")]
        public static PromptMessage[] SimplePrompt()
        {
            return new[] {
                new PromptMessage("This is a prompt example")
            };
        }

        [McpServerPrompt("anohter_simple_prompt", "Another prompt description")]
        public static PromptMessage[] AnotherSimplePrompt()
        {
            return new[] {
                new PromptMessage("This is another prompt example")
            };
        }

        [McpServerPrompt("prompt_with_string_parameter", "A prompt with string parameter")]
        [McpPromptParameter("input", "Input string parameter")]
        public static PromptMessage[] PromptWithStringParameter(string input)
        {
            return new[] {
                new PromptMessage($"Received input: {input}")
            };
        }

        [McpServerPrompt("prompt_with_complex_parameter", "A prompt with complex parameter")]
        [McpPromptParameter("Name", "Name of the person")]
        [McpPromptParameter("Value", "Value associated with the name")]
        public static PromptMessage[] PromptWithComplexParameter(string name, string value)
        {
            return new[] {
                new PromptMessage($"Received complex parameter: {name} with value {value}")
            };
        }

        [McpServerPrompt("dynamic_prompt_without_param", "A dynamic prompt without parameters")]
        public static PromptMessage[] DynamicPromptWithoutParam()
        {
            return new[] {
                new PromptMessage("This is a dynamic prompt without parameters"),
                new PromptMessage("It can return multiple messages"),
                new PromptMessage("This is useful for complex prompts")
            };
        }

        [McpServerPrompt("dynamic_prompt_with_param", "A dynamic prompt with parameters")]
        [McpPromptParameter("value", "Input value for dynamic prompt")]
        public static PromptMessage[] DynamicPromptWithParam(string value)
        {
            return new[] {
                new PromptMessage("Analyze these system logs and the code file for any issues"),
                new PromptMessage($"Consider the input value: {value}"){ Role = Role.Assistant }
            };
        }
    }

    [TestClass]
    public class McpPromptRegistryTests
    {
        [TestMethod]
        public void TestDiscoverPromtpsAndGetMetadataSimple()
        {
            // Arrange
            Type[] toolTypes = new Type[] { typeof(TestPromptsClass) };

            // Act
            McpPromptRegistry.DiscoverPrompts(toolTypes);
            string metadataJson = McpPromptRegistry.GetPromptMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("simple_prompt"), "Metadata should contain simple_prompt");
            Assert.IsTrue(metadataJson.Contains("anohter_simple_prompt"), "Metadata should contain anohter_simple_prompt");
        }

        [TestMethod]
        public void TestGetMetadataJsonStructure()
        {
            // Arrange
            Type[] promptTypes = new Type[] { typeof(TestPromptsClass) };

            // Act
            McpPromptRegistry.DiscoverPrompts(promptTypes);
            string metadataJson = McpPromptRegistry.GetPromptMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");

            // Check overall structure
            Assert.IsTrue(metadataJson.StartsWith("\"prompts\":["), "Metadata should start with prompts array");
            Assert.IsTrue(metadataJson.EndsWith("],\"nextCursor\":null"), "Metadata should end with nextCursor");

            // Check for prompts properties
            Assert.IsTrue(metadataJson.Contains("\"name\":"), "Metadata should contain prompts names");
            Assert.IsTrue(metadataJson.Contains("\"description\":"), "Metadata should contain prompts descriptions");

            // Check specific tool content
            Assert.IsTrue(metadataJson.Contains("A simple prompt description"), $"Metadata should contain simple prompt description. Got: '{metadataJson}'");
            Assert.IsTrue(metadataJson.Contains("Another prompt description"), $"Metadata should contain another prompt description. Got: '{metadataJson}'");
        }

        [TestMethod]
        public void TestDiscoverToolsCalledMultipleTimes()
        {
            // Arrange
            Type[] promptTypes1 = new Type[] { typeof(TestPromptsClass) };
            Type[] promptTypes2 = new Type[] { typeof(TestPromptsClass) };

            // Act - Call DiscoverTools multiple times
            McpPromptRegistry.DiscoverPrompts(promptTypes1);
            string firstCall = McpPromptRegistry.GetPromptMetadataJson();

            // Should be ignored as already initialized
            McpPromptRegistry.DiscoverPrompts(promptTypes2);
            string secondCall = McpPromptRegistry.GetPromptMetadataJson();

            // Assert
            Assert.AreEqual(firstCall, secondCall, "Multiple calls to DiscoverPrompts should not change the result");
            Assert.IsTrue(firstCall.Contains("simple_prompt"), "Should still contain prompt from first discovery");
        }

        [TestMethod]
        public void TestGetMetadataJsonEmptyRegistry()
        {
            // Note: This test might not work in isolation due to static nature
            // But it tests the exception handling
            try
            {
                // This should work even with empty tools if the registry has been initialized
                string metadataJson = McpPromptRegistry.GetPromptMetadataJson();
                Assert.IsNotNull(metadataJson, "Metadata JSON should not be null even for empty registry");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Impossible to build tools list.", ex.Message, "Should throw expected exception for empty tools");
            }
        }

        // Tests for InvokePrompt function
        [TestMethod]
        public void TestInvokePromptSimple()
        {
            // Arrange - Simulate how HandleMcpRequest creates the Hashtable for simple types
            Type[] promptTypes = new Type[] { typeof(TestPromptsClass) };
            McpPromptRegistry.DiscoverPrompts(promptTypes);

            // Act
            string result = McpPromptRegistry.InvokePrompt("simple_prompt", null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("This is a prompt example"), "Result should contain 'This is a prompt example'");
        }

        [TestMethod]
        public void TestInvokePromptWithStringParameter()
        {
            // Arrange
            Type[] promptTypes = new Type[] { typeof(TestPromptsClass) };
            McpPromptRegistry.DiscoverPrompts(promptTypes);

            // Create arguments Hashtable
            var arguments = new Hashtable();
            arguments["value"] = "Test input";

            // Act
            string result = McpPromptRegistry.InvokePrompt("prompt_with_string_parameter", arguments);

            // debug purposes only
            OutputHelper.WriteLine($">>>{result}<<<");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("Received input: Test input"), "Result should contain 'Received input: Test input'");
        }

        [TestMethod]
        public void TestInvokePromptWithComplexParameter()
        {
            // Arrange
            Type[] promptTypes = new Type[] { typeof(TestPromptsClass) };
            McpPromptRegistry.DiscoverPrompts(promptTypes);

            // Create arguments Hashtable with complex parameter
            var arguments = new Hashtable();
            arguments.Add("Name", "John");
            arguments.Add("Value", "100");

            // Act
            string result = McpPromptRegistry.InvokePrompt("prompt_with_complex_parameter", arguments);

            // debug purposes only
            OutputHelper.WriteLine($">>>{result}<<<");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("Received complex parameter: John with value 100"), "Result should contain 'Received complex parameter: Test with value 42'");
        }

        [TestMethod]
        public void TestInvokeDynamicPromptWithoutParam()
        {
            // Arrange
            Type[] promptTypes = new Type[] { typeof(TestPromptsClass) };
            McpPromptRegistry.DiscoverPrompts(promptTypes);

            // Act
            string result = McpPromptRegistry.InvokePrompt("dynamic_prompt_without_param", null);

            // debug purposes only
            OutputHelper.WriteLine($">>>{result}<<<");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("This is a dynamic prompt without parameters"), "Result should contain 'This is a dynamic prompt without parameters'");
            Assert.IsTrue(result.Contains("It can return multiple messages"), "Result should contain 'It can return multiple messages'");
            Assert.IsTrue(result.Contains("\"role\":\"user\""), "Result should contain role 'user' in the message");
            Assert.IsFalse(result.Contains("\"role\":\"assistant\""), "Result should not contain role 'assistant' in the message");
        }

        [TestMethod]
        public void TestInvokeDynamicPromptWithParam()
        {
            // Arrange
            Type[] promptTypes = new Type[] { typeof(TestPromptsClass) };
            McpPromptRegistry.DiscoverPrompts(promptTypes);

            // Create arguments Hashtable with input parameter
            var arguments = new Hashtable();
            arguments["value"] = "Test input for dynamic prompt";

            // Act
            string result = McpPromptRegistry.InvokePrompt("dynamic_prompt_with_param", arguments);

            // debug purposes only
            OutputHelper.WriteLine($">>>{result}<<<");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("Analyze these system logs and the code file for any issues"), "Result should contain analysis message");
            Assert.IsTrue(result.Contains("Consider the input value: Test input for dynamic prompt"), "Result should contain input value");
            Assert.IsTrue(result.Contains("\"role\":\"assistant\""), "Result should contain role 'assistant' in the message");
        }
    }
}
