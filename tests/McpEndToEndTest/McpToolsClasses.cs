// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    public class Person
    {
        [Description("A person object with basic details.")]
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Age { get; set; } = "30";  // Default age
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
        [McpServerTool("echo","The echoed string")]
        public static string Echo(string echo) => echo;

        [McpServerTool("process_person", "Processes a person object.", "the output is person processed.")]
        public static string ProcessPerson(Person person)
        {
            //return $"Processed: {person.Name} {person.Surname}, Age: {person.Age}, Location: {person.Address.City}, {person.Address.Country}";
            return $"Processed: {person.Name} {person.Surname}, Age: {person.Age}"; //, Location: {person.Address.City}, {person.Address.Country}";
        }

        [McpServerTool("get_default_person", "Returns a default person object.", "the output is a default person object.")]
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

        [McpServerTool("get_default_address", "Returns a default address object.")]
        public Address GetDefaultAddress()
        {
            return new Address
            {
                Street = "456 Elm St",
                City = "Sample City",
                PostalCode = "67890",
                Country = "Sample Country"
            };
        }
    }
}
