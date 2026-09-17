// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    public static class McpResources
    {
        [McpServerResource("device/info", "Device info", "Static nanoFramework device information", "text/plain")]
        public static string GetDeviceInfo() => "nanoFramework device";
    }

    public class SensorResources
    {
        private readonly string _reading;

        public SensorResources(string reading)
        {
            _reading = reading;
        }

        [McpServerResource("sensor/reading", "Sensor reading", "Current sensor reading", "text/plain")]
        public string GetReading() => _reading;
    }
}
