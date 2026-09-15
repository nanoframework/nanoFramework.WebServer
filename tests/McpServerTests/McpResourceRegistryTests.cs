// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    // Static resource class with an MCP resource
    public static class TestResourcesClass
    {
        [McpServerResource("device://info", "Device info", "Static device info", "text/plain")]
        public static string GetInfo() => "nanoFramework device";
    }

    // Instance resource class - resources are instance methods reading per-instance state
    public class InstanceResourceProvider
    {
        private readonly string _state;

        public InstanceResourceProvider(string s)
        {
            _state = s;
        }

        [McpServerResource("sensor://reading", "Reading", "Current reading")]
        public string Read() => _state;
    }

    // Resource class with invalid resource methods - none should be registered
    public class InvalidResourceProvider
    {
        [McpServerResource("bad://withparam", "With param", "Takes a parameter")]
        public string WithParam(string input) => input;

        [McpServerResource("bad://nonstring", "Non string", "Returns non-string")]
        public int NonString() => 42;
    }

    [TestClass]
    public class McpResourceRegistryTests
    {
        [Setup]
        public void Setup()
        {
            // Register the static test resources first so device://info exists regardless of test execution order.
            // The isInitialized gate makes this the winning Type[] discovery.
            McpResourceRegistry.DiscoverResources(new Type[] { typeof(TestResourcesClass) });
        }

        [TestMethod]
        public void TestDiscoverResourcesRegistersStaticMetadata()
        {
            // Arrange
            Type[] resourceTypes = new Type[] { typeof(TestResourcesClass) };

            // Act
            McpResourceRegistry.DiscoverResources(resourceTypes);
            string metadataJson = McpResourceRegistry.GetResourceMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("\"resources\":["), "Metadata should contain resources array");
            Assert.IsTrue(metadataJson.Contains("device://info"), "Metadata should contain the resource uri");
            Assert.IsTrue(metadataJson.Contains("Device info"), "Metadata should contain the resource name");
            Assert.IsTrue(metadataJson.Contains("text/plain"), "Metadata should contain the resource mimeType");
        }

        [TestMethod]
        public void TestReadStaticResource()
        {
            // Arrange
            McpResourceRegistry.DiscoverResources(new Type[] { typeof(TestResourcesClass) });

            // Act
            string result = McpResourceRegistry.ReadResource("device://info");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("\"contents\""), "Result should contain contents array");
            Assert.IsTrue(result.Contains("device://info"), "Result should contain the resource uri");
            Assert.IsTrue(result.Contains("nanoFramework device"), "Result should contain the resource text");
        }

        [TestMethod]
        public void TestReadUnknownResourceThrows()
        {
            // Act & Assert
            Assert.ThrowsException(typeof(Exception), () =>
            {
                McpResourceRegistry.ReadResource("unknown://missing");
            }, "Should throw exception for unknown resource uri");
        }

        [TestMethod]
        public void TestDiscoverInstanceResourcesRegistersMetadata()
        {
            // Act - instance discovery is additive and ungated
            McpResourceRegistry.DiscoverResources(
                new object[] { new InstanceResourceProvider("42") },
                new string[] { "a_" });
            string metadataJson = McpResourceRegistry.GetResourceMetadataJson();

            // Assert
            Assert.IsNotNull(metadataJson, "Metadata JSON should not be null");
            Assert.IsTrue(metadataJson.Contains("a_sensor://reading"), "Metadata should contain the prefixed instance resource uri");
            Assert.IsTrue(metadataJson.Contains("device://info"), "Static resources should still be present after instance discovery");
        }

        [TestMethod]
        public void TestReadInstanceResourceUsesTarget()
        {
            // Arrange - a distinct prefix keeps this test's resource isolated from the shared static registry
            McpResourceRegistry.DiscoverResources(
                new object[] { new InstanceResourceProvider("42") },
                new string[] { "read_" });

            // Act
            string result = McpResourceRegistry.ReadResource("read_sensor://reading");

            // Assert - proves the stored instance (state = 42), not a static, was used as the invocation target
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("42"), "Result should reflect the instance's own state (42)");
        }

        [TestMethod]
        public void TestTwoInstancesIndependent()
        {
            // Arrange - two instances of the same class, distinct prefixes and distinct state
            McpResourceRegistry.DiscoverResources(
                new object[] { new InstanceResourceProvider("10"), new InstanceResourceProvider("20") },
                new string[] { "ta_", "tb_" });

            // Act
            string a = McpResourceRegistry.ReadResource("ta_sensor://reading");
            string b = McpResourceRegistry.ReadResource("tb_sensor://reading");

            // Assert - each prefixed resource reads against its own independent target
            Assert.IsTrue(a.Contains("10"), "Instance a should report its own state (10)");
            Assert.IsTrue(b.Contains("20"), "Instance b should report its own state (20)");
        }

        [TestMethod]
        public void TestInvalidResourceMethodsNotRegistered()
        {
            // Act - a method with a parameter and a method returning non-string must be skipped at discovery
            McpResourceRegistry.DiscoverResources(
                new object[] { new InvalidResourceProvider() },
                new string[] { "inv_" });
            string metadataJson = McpResourceRegistry.GetResourceMetadataJson();

            // Assert
            Assert.IsFalse(metadataJson.Contains("bad://withparam"), "Resource method taking a parameter should not be registered");
            Assert.IsFalse(metadataJson.Contains("bad://nonstring"), "Resource method returning non-string should not be registered");
        }
    }
}
