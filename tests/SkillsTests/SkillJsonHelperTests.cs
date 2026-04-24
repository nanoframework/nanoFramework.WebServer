// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.Skills;

namespace SkillsTests
{
    // Test classes for complex type testing
    public class SimpleInputClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ComplexInputClass
    {
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public double Score { get; set; }
        public SimpleInputClass Nested { get; set; }
    }

    public class OutputWithDescription
    {
        public string Status
        {
            [Description("The current status")]
            get;
            set;
        }

        public int Count
        {
            [Description("Number of items")]
            get;
            set;
        }
    }

    [TestClass]
    public class SkillJsonHelperTests
    {
        [TestMethod]
        public void GenerateInputJson_StringType_ReturnsValueSchema()
        {
            // Arrange
            Type inputType = typeof(string);
            string expected = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\",\"description\":\"Input parameter of type String\"}},\"required\":[]}";

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.AreEqual(expected, result, "String type schema does not match expected output.");
        }

        [TestMethod]
        public void GenerateInputJson_IntType_ReturnsNumberSchema()
        {
            // Arrange
            Type inputType = typeof(int);
            string expected = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Int32\"}},\"required\":[]}";

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.AreEqual(expected, result, "Int type schema does not match expected output.");
        }

        [TestMethod]
        public void GenerateInputJson_BoolType_ReturnsBooleanSchema()
        {
            // Arrange
            Type inputType = typeof(bool);
            string expected = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"boolean\",\"description\":\"Input parameter of type Boolean\"}},\"required\":[]}";

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.AreEqual(expected, result, "Bool type schema does not match expected output.");
        }

        [TestMethod]
        public void GenerateInputJson_DoubleType_ReturnsNumberSchema()
        {
            // Arrange
            Type inputType = typeof(double);
            string expected = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Double\"}},\"required\":[]}";

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.AreEqual(expected, result, "Double type schema does not match expected output.");
        }

        [TestMethod]
        public void GenerateInputJson_FloatType_ReturnsNumberSchema()
        {
            // Arrange
            Type inputType = typeof(float);
            string expected = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Single\"}},\"required\":[]}";

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.AreEqual(expected, result, "Float type schema does not match expected output.");
        }

        [TestMethod]
        public void GenerateInputJson_LongType_ReturnsNumberSchema()
        {
            // Arrange
            Type inputType = typeof(long);
            string expected = "{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"number\",\"description\":\"Input parameter of type Int64\"}},\"required\":[]}";

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.AreEqual(expected, result, "Long type schema does not match expected output.");
        }

        [TestMethod]
        public void GenerateInputJson_SimpleClass_ReturnsObjectSchema()
        {
            // Arrange
            Type inputType = typeof(SimpleInputClass);

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("\"type\":\"object\""), "Should be object type");
            Assert.IsTrue(result.Contains("\"Name\""), "Should contain Name property");
            Assert.IsTrue(result.Contains("\"Age\""), "Should contain Age property");
            Assert.IsTrue(result.Contains("\"type\":\"string\""), "Name should be string type");
            Assert.IsTrue(result.Contains("\"type\":\"number\""), "Age should be number type");
        }

        [TestMethod]
        public void GenerateInputJson_ComplexClass_ReturnsNestedSchema()
        {
            // Arrange
            Type inputType = typeof(ComplexInputClass);

            // Act
            string result = SkillJsonHelper.GenerateInputJson(inputType);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("\"Title\""), "Should contain Title property");
            Assert.IsTrue(result.Contains("\"IsActive\""), "Should contain IsActive property");
            Assert.IsTrue(result.Contains("\"Score\""), "Should contain Score property");
            Assert.IsTrue(result.Contains("\"Nested\""), "Should contain Nested property");
            Assert.IsTrue(result.Contains("\"type\":\"boolean\""), "IsActive should be boolean type");
        }

        [TestMethod]
        public void GenerateOutputJson_SimpleClass_ContainsDescriptions()
        {
            // Arrange
            Type outputType = typeof(OutputWithDescription);

            // Act
            string result = SkillJsonHelper.GenerateOutputJson(outputType, "Test output");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("\"type\":\"object\""), "Should be object type");
            Assert.IsTrue(result.Contains("\"description\":\"Test output\""), "Should contain top-level description");
            Assert.IsTrue(result.Contains("\"The current status\""), "Should contain Status description");
            Assert.IsTrue(result.Contains("\"Number of items\""), "Should contain Count description");
        }

        [TestMethod]
        public void IsPrimitiveType_PrimitiveTypes_ReturnsTrue()
        {
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(int)), "int should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(bool)), "bool should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(double)), "double should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(float)), "float should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(long)), "long should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(short)), "short should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(byte)), "byte should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(char)), "char should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(uint)), "uint should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(ulong)), "ulong should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(ushort)), "ushort should be primitive");
            Assert.IsTrue(SkillJsonHelper.IsPrimitiveType(typeof(sbyte)), "sbyte should be primitive");
        }

        [TestMethod]
        public void IsPrimitiveType_NonPrimitiveTypes_ReturnsFalse()
        {
            Assert.IsFalse(SkillJsonHelper.IsPrimitiveType(typeof(string)), "string should not be primitive");
            Assert.IsFalse(SkillJsonHelper.IsPrimitiveType(typeof(SimpleInputClass)), "class should not be primitive");
        }
    }
}
