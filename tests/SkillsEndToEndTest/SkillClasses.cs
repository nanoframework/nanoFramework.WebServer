// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.WebServer.Skills;

namespace SkillsEndToEndTest
{
    // --- Climate control skill ---

    public class TargetTempInput
    {
        [Description("Target temperature in Celsius")]
        public double Temperature { get; set; }
    }

    public class HvacStatus
    {
        [Description("Current temperature in Celsius")]
        public double CurrentTemp { get; set; }

        [Description("Target temperature in Celsius")]
        public double TargetTemp { get; set; }

        [Description("Whether the HVAC unit is currently running")]
        public bool IsRunning { get; set; }

        [Description("Current operating mode (cooling, heating, idle)")]
        public string Mode { get; set; }
    }

    [Skill("climate-control", "Climate Control", "HVAC management for building zones including temperature reading, target setting and status reporting", "1.0")]
    [SkillTag("temperature")]
    [SkillTag("hvac")]
    [SkillTag("sensor")]
    [SkillExample("What is the current temperature?")]
    [SkillExample("Set the target temperature to 22 degrees")]
    [SkillExample("Show me the HVAC system status")]
    public class ClimateSkill
    {
        private static double _targetTemp = 22.0;
        private static double _currentTemp = 21.5;

        [SkillAction("GetTemperature", "Reads the current room temperature in Celsius")]
        public static double GetTemperature()
        {
            return _currentTemp;
        }

        [SkillAction("SetTargetTemp", "Sets the target temperature for the HVAC system")]
        public static bool SetTargetTemp(TargetTempInput input)
        {
            if (input.Temperature < 10 || input.Temperature > 35)
            {
                return false;
            }

            _targetTemp = input.Temperature;
            return true;
        }

        [SkillAction("GetStatus", "Returns full HVAC system status including temperatures and mode",
            outputDescription: "Complete HVAC status object")]
        public static HvacStatus GetStatus()
        {
            return new HvacStatus
            {
                CurrentTemp = _currentTemp,
                TargetTemp = _targetTemp,
                IsRunning = true,
                Mode = _currentTemp > _targetTemp ? "cooling" : _currentTemp < _targetTemp ? "heating" : "idle"
            };
        }

        [SkillAction("GetDocumentation", "Returns setup and calibration guide as markdown",
            contentType: "text/markdown")]
        public static string GetDocumentation()
        {
            return "# Climate Control Setup Guide\n\n" +
                   "## Sensor Calibration\n" +
                   "1. Place the sensor in a controlled environment\n" +
                   "2. Wait 5 minutes for stabilization\n" +
                   "3. Compare readings with a reference thermometer\n\n" +
                   "## Configuration\n" +
                   "- **Target Range**: 10C - 35C\n" +
                   "- **Polling Interval**: 30 seconds\n" +
                   "- **Default Target**: 22C\n\n" +
                   "## Troubleshooting\n" +
                   "| Symptom | Possible Cause | Fix |\n" +
                   "|---------|---------------|-----|\n" +
                   "| No readings | Sensor disconnected | Check wiring |\n" +
                   "| Erratic values | Electrical noise | Add shielding |\n";
        }
    }

    // --- Lighting control skill ---

    public class BrightnessInput
    {
        [Description("Brightness level from 0 to 100")]
        public int Level { get; set; }

        [Description("Optional zone name (e.g. 'living-room', 'kitchen')")]
        public string Zone { get; set; }
    }

    [Skill("lighting", "Lighting Control", "Smart lighting management for building zones including brightness control and toggling", "1.0")]
    [SkillTag("light")]
    [SkillTag("automation")]
    [SkillExample("Turn on the lights")]
    [SkillExample("Set brightness to 75%")]
    [SkillExample("Toggle the lights off")]
    public class LightingSkill
    {
        private static int _brightness = 50;
        private static bool _isOn = true;

        [SkillAction("GetBrightness", "Reads the current brightness level as a value from 0 to 100")]
        public static int GetBrightness()
        {
            return _isOn ? _brightness : 0;
        }

        [SkillAction("SetBrightness", "Sets the brightness level and optional zone")]
        public static string SetBrightness(BrightnessInput input)
        {
            if (input.Level < 0 || input.Level > 100)
            {
                return "Error: Level must be between 0 and 100";
            }

            _brightness = input.Level;
            _isOn = input.Level > 0;
            string zone = string.IsNullOrEmpty(input.Zone) ? "all zones" : input.Zone;
            return $"Brightness set to {input.Level}% in {zone}";
        }

        [SkillAction("Toggle", "Toggles the lights on or off and returns the new state (true = on, false = off)")]
        public static bool Toggle()
        {
            _isOn = !_isOn;
            return _isOn;
        }
    }
}
