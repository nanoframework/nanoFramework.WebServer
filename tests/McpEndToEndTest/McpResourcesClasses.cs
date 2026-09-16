// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    public class McpResources
    {
        [McpServerResource("device://info", "device_info", "Basic information about the device running the MCP server.")]
        public static string GetDeviceInfo() => "McpEndToEndTest device running nanoFramework.WebServer.Mcp";

        [McpServerResource("device://default-person", "default_person", "Returns the default person object as a resource.", "application/json")]
        public Person GetDefaultPerson()
        {
            return new Person
            {
                Name = "John",
                Surname = "Doe",
                Age = "30",
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "Anytown",
                    PostalCode = "12345",
                    Country = "USA"
                }
            };
        }
    }
}
