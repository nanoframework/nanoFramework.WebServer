// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;

namespace McpServerTests
{
    // Test classes for complex type testing
    public class SimpleTestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ComplexTestClass
    {
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public double Score { get; set; }
        public SimpleTestClass NestedObject { get; set; }
    }

    public class EmptyTestClass
    {
        // No properties
    }

    [TestClass]
    public class TestMcpToolsAttributeTests
    {
        [TestMethod]
        public void TestSimpleTypeString()
        {
            // Arrange
            Type inputType = typeof(string);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\",\"description\":\"Input parameter of type String\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple string type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeInt()
        {
            // Arrange
            Type inputType = typeof(int);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Int32\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple int type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeBool()
        {
            // Arrange
            Type inputType = typeof(bool);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"boolean\",\"description\":\"Input parameter of type Boolean\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple bool type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeDouble()
        {
            // Arrange
            Type inputType = typeof(double);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Double\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple double type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeFloat()
        {
            // Arrange
            Type inputType = typeof(float);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Single\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple float type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeLong()
        {
            // Arrange
            Type inputType = typeof(long);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Int64\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple long type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeByte()
        {
            // Arrange
            Type inputType = typeof(byte);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Byte\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple byte type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeShort()
        {
            // Arrange
            Type inputType = typeof(short);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Int16\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple short type does not match the expected output.");
        }

        [TestMethod]
        public void TestSimpleTypeChar()
        {
            // Arrange
            Type inputType = typeof(char);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\",\"description\":\"Input parameter of type Char\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple char type does not match the expected output.");
        }

        [TestMethod]
        public void TestComplexTypeSimple()
        {
            // Arrange
            Type inputType = typeof(SimpleTestClass);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"Name\":{\"type\":\"string\",\"description\":\"Name\"},\"Age\":{\"type\":\"number\",\"description\":\"Age\"}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a simple complex type does not match the expected output.");
        }

        [TestMethod]
        public void TestComplexTypeNested()
        {
            // Arrange
            Type inputType = typeof(ComplexTestClass);
            string expectedJson = "{\"type\":\"object\",\"properties\":{\"Title\":{\"type\":\"string\",\"description\":\"Title\"},\"IsActive\":{\"type\":\"boolean\",\"description\":\"IsActive\"},\"Score\":{\"type\":\"number\",\"description\":\"Score\"},\"NestedObject\":{\"type\":\"object\",\"description\":\"NestedObject\",\"properties\":{\"Name\":{\"type\":\"string\",\"description\":\"Name\"},\"Age\":{\"type\":\"number\",\"description\":\"Age\"}}}},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for a nested complex type does not match the expected output.");
        }

        [TestMethod]
        public void TestComplexTypeEmpty()
        {
            // Arrange
            Type inputType = typeof(EmptyTestClass);
            string expectedJson = "{\"type\":\"object\",\"properties\":{},\"required\":[]}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated JSON for an empty complex type does not match the expected output.");
        }

        [TestMethod]
        public void TestNullInputType()
        {
            // Arrange
            Type inputType = null;
            // Act & Assert
            Assert.ThrowsException(typeof(NullReferenceException), () =>
            {
                nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateInputJson(inputType);            }, "GenerateInputJson should throw an exception for null input type.");
        }

        // Tests for GenerateOutputJson function
        [TestMethod]
        public void TestOutputJsonSimpleTypeString()
        {
            // Arrange
            Type outputType = typeof(string);
            string description = "Test string output";
            string expectedJson = "{\"type\":\"string\",\"description\":\"Test string output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a simple string type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonSimpleTypeInt()
        {
            // Arrange
            Type outputType = typeof(int);
            string description = "Test integer output";
            string expectedJson = "{\"type\":\"number\",\"description\":\"Test integer output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a simple int type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonSimpleTypeBool()
        {
            // Arrange
            Type outputType = typeof(bool);
            string description = "Test boolean output";
            string expectedJson = "{\"type\":\"boolean\",\"description\":\"Test boolean output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a simple bool type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonSimpleTypeDouble()
        {
            // Arrange
            Type outputType = typeof(double);
            string description = "Test double output";
            string expectedJson = "{\"type\":\"number\",\"description\":\"Test double output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a simple double type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonComplexTypeSimple()
        {
            // Arrange
            Type outputType = typeof(SimpleTestClass);
            string description = "Test simple complex output";
            string expectedJson = "{\"type\":\"object\",\"description\":\"Test simple complex output\",\"properties\":{\"Name\":{\"type\":\"string\",\"description\":\"Name\"},\"Age\":{\"type\":\"number\",\"description\":\"Age\"}}}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a simple complex type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonComplexTypeNested()
        {
            // Arrange
            Type outputType = typeof(ComplexTestClass);
            string description = "Test nested complex output";
            string expectedJson = "{\"type\":\"object\",\"description\":\"Test nested complex output\",\"properties\":{\"Title\":{\"type\":\"string\",\"description\":\"Title\"},\"IsActive\":{\"type\":\"boolean\",\"description\":\"IsActive\"},\"Score\":{\"type\":\"number\",\"description\":\"Score\"},\"NestedObject\":{\"type\":\"object\",\"description\":\"NestedObject\",\"properties\":{\"Name\":{\"type\":\"string\",\"description\":\"Name\"},\"Age\":{\"type\":\"number\",\"description\":\"Age\"}}}}}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a nested complex type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonComplexTypeEmpty()
        {
            // Arrange
            Type outputType = typeof(EmptyTestClass);
            string description = "Test empty complex output";
            string expectedJson = "{\"type\":\"object\",\"description\":\"Test empty complex output\",\"properties\":{}}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for an empty complex type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonWithEmptyDescription()
        {
            // Arrange
            Type outputType = typeof(string);
            string description = "";
            string expectedJson = "{\"type\":\"string\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON with empty description does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonWithNullDescription()
        {
            // Arrange
            Type outputType = typeof(int);
            string description = null;
            string expectedJson = "{\"type\":\"number\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON with null description does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonNullOutputType()
        {
            // Arrange
            Type outputType = null;
            string description = "Test description";
            // Act & Assert
            Assert.ThrowsException(typeof(NullReferenceException), () =>
            {
                nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);            }, "GenerateOutputJson should throw an exception for null output type.");
        }

        [TestMethod]
        public void TestOutputJsonArrayType()
        {
            // Arrange
            Type outputType = typeof(string[]);
            string description = "Test array output";
            string expectedJson = "{\"type\":\"array\",\"description\":\"Test array output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for an array type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonIntArrayType()
        {
            // Arrange
            Type outputType = typeof(int[]);
            string description = "Test integer array output";
            string expectedJson = "{\"type\":\"array\",\"description\":\"Test integer array output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for an integer array type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonLongType()
        {
            // Arrange
            Type outputType = typeof(long);
            string description = "Test long output";
            string expectedJson = "{\"type\":\"number\",\"description\":\"Test long output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a long type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonFloatType()
        {
            // Arrange
            Type outputType = typeof(float);
            string description = "Test float output";
            string expectedJson = "{\"type\":\"number\",\"description\":\"Test float output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a float type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonByteType()
        {
            // Arrange
            Type outputType = typeof(byte);
            string description = "Test byte output";
            string expectedJson = "{\"type\":\"number\",\"description\":\"Test byte output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a byte type does not match the expected output.");
        }

        [TestMethod]
        public void TestOutputJsonShortType()
        {
            // Arrange
            Type outputType = typeof(short);
            string description = "Test short output";
            string expectedJson = "{\"type\":\"number\",\"description\":\"Test short output\"}";
            // Act
            string resultJson = nanoFramework.WebServer.Mcp.McpToolJsonHelper.GenerateOutputJson(outputType, description);
            // Assert
            Assert.AreEqual(expectedJson, resultJson, "The generated output JSON for a short type does not match the expected output.");
        }
    }
}
