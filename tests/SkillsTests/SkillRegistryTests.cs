// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using nanoFramework.TestFramework;
using nanoFramework.WebServer.Skills;

namespace SkillsTests
{
    // Test parameter classes
    public class TargetTempInput
    {
        public double Temperature { get; set; }
    }

    public class NestedSkillParam
    {
        public string Label { get; set; }
        public TargetTempInput Config { get; set; }
    }

    // Test skill class - climate control
    [Skill("climate-control", "Climate Control", "HVAC management for building zones", "1.0")]
    [SkillTag("temperature")]
    [SkillTag("hvac")]
    [SkillTag("sensor")]
    [SkillExample("What is the current temperature?")]
    [SkillExample("Set the target temperature to 22 degrees")]
    public static class TestClimateSkill
    {
        [SkillAction("GetTemperature", "Reads current room temperature")]
        public static double GetTemperature()
        {
            return 22.5;
        }

        [SkillAction("SetTargetTemp", "Sets the target temperature")]
        public static bool SetTargetTemp(TargetTempInput input)
        {
            return input.Temperature > 0 && input.Temperature < 40;
        }

        [SkillAction("GetDocumentation", "Returns setup and calibration guide",
            contentType: "text/markdown")]
        public static string GetDocumentation()
        {
            return "# Climate Control Setup Guide\n\n## Configuration\n- **Target Range**: 18-28C\n";
        }

        // Non-action method - should be ignored
        public static string HelperMethod()
        {
            return "Not an action";
        }
    }

    // Test skill class - lighting
    [Skill("lighting", "Lighting", "Smart lighting control", "2.0")]
    [SkillTag("light")]
    [SkillTag("sensor")]
    [SkillExample("Turn on the lights")]
    public static class TestLightingSkill
    {
        [SkillAction("GetBrightness", "Reads current brightness level")]
        public static int GetBrightness()
        {
            return 75;
        }

        [SkillAction("SetBrightness", "Sets brightness level")]
        public static string SetBrightness(int level)
        {
            return "Brightness set to " + level;
        }
    }

    // Empty class with no skill attribute
    public static class NotASkillClass
    {
        public static string DoSomething()
        {
            return "Not a skill";
        }
    }

    // Skill class with nested parameters
    [Skill("nested-skill", "Nested Skill", "Skill with nested parameters")]
    [SkillTag("test")]
    public static class TestNestedSkill
    {
        [SkillAction("ProcessNested", "Processes nested input")]
        public static string ProcessNested(NestedSkillParam param)
        {
            return param.Label + ": " + param.Config.Temperature;
        }
    }

    [TestClass]
    public class SkillRegistryTests
    {
        [Setup]
        public void Setup()
        {
            SkillRegistry.Reset();
            SkillRegistry.DiscoverSkills(new Type[]
            {
                typeof(TestClimateSkill),
                typeof(TestLightingSkill),
                typeof(TestNestedSkill),
                typeof(NotASkillClass)
            });
        }

        [TestMethod]
        public void DiscoverSkills_FindsDecoratedClasses()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsNotNull(json, "Skills JSON should not be null");
            Assert.IsTrue(json.Contains("\"climate-control\""), "Should find climate-control skill");
            Assert.IsTrue(json.Contains("\"lighting\""), "Should find lighting skill");
            Assert.IsFalse(json.Contains("NotASkillClass"), "Should not include non-skill class");
            Assert.IsFalse(json.Contains("DoSomething"), "Should not include non-skill methods");
        }

        [TestMethod]
        public void DiscoverSkills_CollectsTags()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"tags\":["), "Should contain tags array");
            Assert.IsTrue(json.Contains("\"temperature\""), "Should contain temperature tag");
            Assert.IsTrue(json.Contains("\"hvac\""), "Should contain hvac tag");
            Assert.IsTrue(json.Contains("\"sensor\""), "Should contain sensor tag");
        }

        [TestMethod]
        public void DiscoverSkills_CollectsExamples()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"examples\":["), "Should contain examples array");
            Assert.IsTrue(json.Contains("What is the current temperature?"), "Should contain first example");
            Assert.IsTrue(json.Contains("Set the target temperature to 22 degrees"), "Should contain second example");
        }

        [TestMethod]
        public void DiscoverSkills_FindsActions()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"actions\":["), "Should contain actions array");
            Assert.IsTrue(json.Contains("\"GetTemperature\""), "Should find GetTemperature action");
            Assert.IsTrue(json.Contains("\"SetTargetTemp\""), "Should find SetTargetTemp action");
            Assert.IsTrue(json.Contains("\"GetDocumentation\""), "Should find GetDocumentation action");
            Assert.IsFalse(json.Contains("\"HelperMethod\""), "Should not include non-action methods");
        }

        [TestMethod]
        public void DiscoverSkills_SetsInputOutputModes()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"inputModes\":["), "Should contain inputModes array");
            Assert.IsTrue(json.Contains("\"outputModes\":["), "Should contain outputModes array");
            Assert.IsTrue(json.Contains("\"text/plain\""), "Should contain text/plain input mode (parameterless actions)");
            Assert.IsTrue(json.Contains("\"application/json\""), "Should contain application/json mode");
            Assert.IsTrue(json.Contains("\"text/markdown\""), "Should contain text/markdown output mode");
        }

        [TestMethod]
        public void DiscoverSkills_MarkdownActionHasContentType()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"contentType\":\"text/markdown\""), "Markdown action should have contentType in JSON");
        }

        [TestMethod]
        public void DiscoverSkills_VersionIncluded()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"version\":\"1.0\""), "Climate skill should have version 1.0");
            Assert.IsTrue(json.Contains("\"version\":\"2.0\""), "Lighting skill should have version 2.0");
        }

        [TestMethod]
        public void GetSkillsArrayJson_ValidJsonStructure()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.StartsWith("["), "Skills JSON should start with [");
            Assert.IsTrue(json.EndsWith("]"), "Skills JSON should end with ]");
            Assert.IsTrue(json.Contains("\"id\":\"climate-control\""), "Should contain skill id");
            Assert.IsTrue(json.Contains("\"name\":\"Climate Control\""), "Should contain skill name");
            Assert.IsTrue(json.Contains("\"description\":\"HVAC management for building zones\""), "Should contain skill description");
        }

        [TestMethod]
        public void GetSkillJson_BySkillId_ReturnsCorrectSkill()
        {
            // Act
            string json = SkillRegistry.GetSkillJson("climate-control");

            // Assert
            Assert.IsNotNull(json, "Should find climate-control skill");
            Assert.IsTrue(json.Contains("\"Climate Control\""), "Should contain correct name");
            Assert.IsFalse(json.Contains("\"lighting\""), "Should not contain other skill ids");
        }

        [TestMethod]
        public void GetSkillJson_NonExistentId_ReturnsNull()
        {
            // Act
            string json = SkillRegistry.GetSkillJson("nonexistent");

            // Assert
            Assert.IsNull(json, "Should return null for non-existent skill");
        }

        [TestMethod]
        public void GetSkillsByTagJson_MatchingTag_ReturnsFilteredSkills()
        {
            // Act
            string json = SkillRegistry.GetSkillsByTagJson("sensor");

            // Assert
            Assert.IsNotNull(json, "Should return non-null JSON");
            Assert.IsTrue(json.Contains("\"climate-control\""), "Should contain climate-control (has sensor tag)");
            Assert.IsTrue(json.Contains("\"lighting\""), "Should contain lighting (has sensor tag)");
        }

        [TestMethod]
        public void GetSkillsByTagJson_UniqueTag_ReturnsOneSkill()
        {
            // Act
            string json = SkillRegistry.GetSkillsByTagJson("hvac");

            // Assert
            Assert.IsTrue(json.Contains("\"climate-control\""), "Should contain climate-control (has hvac tag)");
            Assert.IsFalse(json.Contains("\"lighting\""), "Should not contain lighting (no hvac tag)");
        }

        [TestMethod]
        public void GetSkillsByTagJson_NonExistentTag_ReturnsEmptyArray()
        {
            // Act
            string json = SkillRegistry.GetSkillsByTagJson("nonexistent");

            // Assert
            Assert.AreEqual("[]", json, "Should return empty array for non-matching tag");
        }

        [TestMethod]
        public void GetActionContentType_JsonAction_ReturnsApplicationJson()
        {
            // Act
            string contentType = SkillRegistry.GetActionContentType("climate-control", "GetTemperature");

            // Assert
            Assert.AreEqual("application/json", contentType, "Default content type should be application/json");
        }

        [TestMethod]
        public void GetActionContentType_MarkdownAction_ReturnsTextMarkdown()
        {
            // Act
            string contentType = SkillRegistry.GetActionContentType("climate-control", "GetDocumentation");

            // Assert
            Assert.AreEqual("text/markdown", contentType, "Markdown action should return text/markdown");
        }

        [TestMethod]
        public void GetActionContentType_NonExistentSkill_ReturnsNull()
        {
            // Act
            string contentType = SkillRegistry.GetActionContentType("nonexistent", "GetTemperature");

            // Assert
            Assert.IsNull(contentType, "Should return null for non-existent skill");
        }

        [TestMethod]
        public void GetActionContentType_NonExistentAction_ReturnsNull()
        {
            // Act
            string contentType = SkillRegistry.GetActionContentType("climate-control", "NonExistent");

            // Assert
            Assert.IsNull(contentType, "Should return null for non-existent action");
        }

        [TestMethod]
        public void InvokeAction_NoParameters_ReturnsResult()
        {
            // Act
            string result = SkillRegistry.InvokeAction("climate-control", "GetTemperature", null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("22.5"), "Should return temperature value");
        }

        [TestMethod]
        public void InvokeAction_ComplexParameter_ReturnsResult()
        {
            // Arrange
            Hashtable arguments = new Hashtable();
            arguments.Add("Temperature", "22.0");

            // Act
            string result = SkillRegistry.InvokeAction("climate-control", "SetTargetTemp", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("true"), "Should return true for valid temperature");
        }

        [TestMethod]
        public void InvokeAction_PrimitiveParameter_ReturnsResult()
        {
            // Arrange
            Hashtable arguments = new Hashtable();
            arguments.Add("value", "80");

            // Act
            string result = SkillRegistry.InvokeAction("lighting", "SetBrightness", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("80"), "Should contain brightness level");
        }

        [TestMethod]
        public void InvokeAction_MarkdownAction_ReturnsRawMarkdown()
        {
            // Act
            string result = SkillRegistry.InvokeAction("climate-control", "GetDocumentation", null);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("# Climate Control Setup Guide"), "Should contain markdown heading");
            Assert.IsTrue(result.Contains("## Configuration"), "Should contain markdown sub-heading");
            Assert.IsFalse(result.StartsWith("\""), "Markdown result should not be JSON-quoted");
        }

        [TestMethod]
        public void InvokeAction_NestedParameter_ReturnsResult()
        {
            // Arrange
            Hashtable config = new Hashtable();
            config.Add("Temperature", "42");
            Hashtable arguments = new Hashtable();
            arguments.Add("Label", "Test");
            arguments.Add("Config", config);

            // Act
            string result = SkillRegistry.InvokeAction("nested-skill", "ProcessNested", arguments);

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("Test"), "Should contain label");
            Assert.IsTrue(result.Contains("42"), "Should contain nested temperature");
        }

        [TestMethod]
        public void InvokeAction_NonExistentSkill_ThrowsException()
        {
            // Act & Assert
            bool exceptionThrown = false;
            try
            {
                SkillRegistry.InvokeAction("nonexistent", "GetTemperature", null);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Exception should have been thrown");
        }

        [TestMethod]
        public void InvokeAction_NonExistentAction_ThrowsException()
        {
            // Act & Assert
            bool exceptionThrown = false;
            try
            {
                SkillRegistry.InvokeAction("climate-control", "NonExistent", null);
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Exception should have been thrown");
        }

        [TestMethod]
        public void DiscoverSkills_Idempotent_SecondCallIgnored()
        {
            // Arrange — reset and register only climate skill
            SkillRegistry.Reset();
            SkillRegistry.DiscoverSkills(new Type[] { typeof(TestClimateSkill) });
            string firstCall = SkillRegistry.GetSkillsArrayJson();

            // Act — second call with different types should be ignored
            SkillRegistry.DiscoverSkills(new Type[] { typeof(TestLightingSkill) });
            string secondCall = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.AreEqual(firstCall, secondCall, "Second call should not change results");
            Assert.IsTrue(firstCall.Contains("\"climate-control\""), "Should contain first-registered skill");
            Assert.IsFalse(secondCall.Contains("\"lighting\""), "Should not contain second-call skill");

            // Restore state for subsequent tests since [Setup] only runs once
            SkillRegistry.Reset();
            SkillRegistry.DiscoverSkills(new Type[]
            {
                typeof(TestClimateSkill),
                typeof(TestLightingSkill),
                typeof(TestNestedSkill),
                typeof(NotASkillClass)
            });
        }

        [TestMethod]
        public void DiscoverSkills_ComplexActionInputSchema_Generated()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"inputSchema\":{"), "SetTargetTemp should have inputSchema");
            Assert.IsTrue(json.Contains("\"Temperature\""), "Schema should contain Temperature property");
        }

        [TestMethod]
        public void DiscoverSkills_MultipleSkills_AllRegistered()
        {
            // Act
            string json = SkillRegistry.GetSkillsArrayJson();

            // Assert
            Assert.IsTrue(json.Contains("\"climate-control\""), "Should contain climate-control");
            Assert.IsTrue(json.Contains("\"lighting\""), "Should contain lighting");
            Assert.IsTrue(json.Contains("\"nested-skill\""), "Should contain nested-skill");
        }
    }
}
