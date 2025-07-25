// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.WebServer.Mcp;

namespace McpServerTests
{
    public class McpPrompts
    {
        [McpServerPrompt("tools_discovery", "Discover all available tools")]
        public static PromptMessage[] ToolsDiscovery()
        {
            return new PromptMessage[]
            {
                new PromptMessage("List all available tools and their method signatures: Echo, SuperMath, ProcessPerson, GetDefaultAddress, GetDefaultPerson. Show parameter names/types and return types.")
            };
        }

        [McpServerPrompt("echo_sanity_check", "Another test prompt")]
        public static PromptMessage[] AnotherSimplePrompt()
        {
            return new PromptMessage[]
            {
                new PromptMessage("Call Echo with the string 'Hello MCP world!' and return the response.")
            };
        }

        [McpServerPrompt("supermaty_basic_usage", "Demonstrate basic usage of SuperMath tool")]
        public static PromptMessage[] SuperMathBasicUsage()
        {
            return new PromptMessage[]
            {
                new PromptMessage("Run SuperMath to multiply 56 x 78. If the tool fails or returns nothing, indicate the failure; otherwise, report the result.")
            };
        }

        [McpServerPrompt("process_person_usage", "Demonstrate usage of ProcessPerson tool")]
        public static PromptMessage[] ProcessPersonUsage()
        {
            return new PromptMessage[]
            {
                new PromptMessage("Create a Person object with Name: 'Alice', Surname: 'Smith', Age: '25', Address: { Street: '456 Elm St', City: 'Springfield', PostalCode: '67890', Country: 'USA' }. Then call ProcessPerson with this object and return the response.")
            };
        }

        [McpServerPrompt("processperson_workflow", "Demonstrate a workflow using ProcessPerson tool")]
        public static PromptMessage[] ProcessPersonWorkflow()
        {
            return new PromptMessage[]
            {
                new PromptMessage("Call GetDefaultPerson, then pass its output into ProcessPerson. Return both the initial and processed person object.")
            };
        }

        [McpServerPrompt("get_default_address_integration", "Demonstrate integration with GetDefaultAddress tool")]
        public static PromptMessage[] GetDefaultAddressIntegration()
        {
            return new PromptMessage[]
            {
                new PromptMessage("First call GetDefaultPerson, then call GetDefaultAddress. Combine both into a summary like: 'Person X lives at Y'.")
            };
        }

        [McpServerPrompt("high_level_agent_prompt", "Adds logic and semantic intelligence on top of tools")]
        public static PromptMessage[] HighLevelAgentPrompt()
        {
            return new PromptMessage[]
            {
                new PromptMessage("You're a data-summary agent. Fetch the default person and address, then produce a human -readable summary. If age > 30, add '(senior)', otherwise '(junior)' at the end.")
            };
        }

        [McpServerPrompt("confirmation_flow", "Tests conversational context, user confirmation, and conditional logic")]
        public static PromptMessage[] ConfirmationFlow()
        {
            return new PromptMessage[]
            {
                new PromptMessage("Before calling ProcessPerson, ask: 'Do you want to process the default person? [yes/no]'. If user says yes, proceed; else return 'Operation canceled.'")
            };
        }

        [McpServerPrompt("summarize_person", "Fetches person and address, processes the person, uses ageThreshold to label as junior or senior.")]
        [McpPromptParameter("ageThreshold", "The age threshold to determine if the person is a senior or junior.")]
        public static PromptMessage[] SummarizePerson(string ageThreshold)
        {
            return new PromptMessage[]
            {
                new PromptMessage($"Please perform the following steps:\r\n1. Call GetDefaultPerson() -> person.\r\n2. Call GetDefaultAddress() -> address.\r\n3. If person.Age > {ageThreshold} then set label = \"senior\"; otherwise set label = \"junior\".\r\n4. Call ProcessPerson(person) -> processed.\r\n5. Return a JSON object:\r\n{{ \"name\": person.Name, \"age\": person.Age, \"label\": label, \"address\": address, \"processed\": processed }}")
            };
        }
    }
}
