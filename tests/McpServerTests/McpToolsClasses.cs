// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    public class Person
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; } = 30;  // Default age
        public Address Address { get; set; } = new Address();  // Default address
    }

    public class Address
    {
        public string Street { get; set; } = "Unknown";
        public string City { get; set; } = "Unknown";
        public string PostalCode { get; set; } = "00000";
        public string Country { get; set; } = "Unknown";
    }

    public class McpTools
    {
        [McpServerTool("process_person", "Processes a person object.")]
        public static string ProcessPerson(Person person)
        {
            return $"Processed: {person.Name} {person.Surname}, Age: {person.Age}, Location: {person.Address.City}, {person.Address.Country}";
        }
    }
}
