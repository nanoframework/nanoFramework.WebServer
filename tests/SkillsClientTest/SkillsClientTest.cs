#!/usr/bin/dotnet run

#:package DotNetEnv@3.1.1
#:package Microsoft.SemanticKernel@1.74.0
#:package Microsoft.SemanticKernel.Agents.Core@1.74.0

// Skills Discovery E2E Test — Agent Consumer
// Connects to a nanoFramework device running the Skills Discovery Service,
// discovers available skills via the A2A Agent Card, registers them as
// Semantic Kernel functions, and runs an interactive AI agent that can
// invoke device skills through natural language.
//
// Prerequisites:
//   1. A nanoFramework device running the SkillsEndToEndTest firmware
//   2. .env file with Azure OpenAI credentials and device host
//
// Usage:
//   dotnet run SkillsClientTest.cs
//
// Try asking:
//   "What is the current temperature?"
//   "Set the target temperature to 25 degrees"
//   "Show me the HVAC status"
//   "Get the climate control documentation"
//   "What's the brightness level?"
//   "Set brightness to 80% in the kitchen"

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute'
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute'

// Load environment variables from .env file
DotNetEnv.Env.Load();

// -----------------------------------------------------------------
// 1. Configuration
// -----------------------------------------------------------------
var deviceHost = DotNetEnv.Env.GetString("DEVICE_HOST", "192.168.1.139:80");
var deploymentName = DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_NAME");
var endpoint = DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_ENDPOINT");
var apiKey = DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_API_KEY");

var httpClient = new HttpClient { BaseAddress = new Uri($"http://{deviceHost}") };

Console.WriteLine($"Skills Discovery Agent — connecting to {deviceHost}");
Console.WriteLine("---");

// -----------------------------------------------------------------
// 2. Discover skills from the nanoFramework device (A2A Agent Card)
// -----------------------------------------------------------------
Console.WriteLine("Fetching Agent Card from /.well-known/agent-card.json ...");
string agentCardJson;
try
{
    agentCardJson = await httpClient.GetStringAsync("/.well-known/agent-card.json");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: Could not connect to device at {deviceHost}: {ex.Message}");
    Console.WriteLine("Make sure the nanoFramework device is running the SkillsEndToEndTest firmware.");
    return;
}

Console.WriteLine("Agent Card received:");
Console.WriteLine(agentCardJson);
Console.WriteLine("---");

var agentCard = JsonDocument.Parse(agentCardJson);
var agentName = agentCard.RootElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "Unknown";
var skills = agentCard.RootElement.GetProperty("skills");

Console.WriteLine($"Device: {agentName}");
Console.WriteLine($"Skills discovered: {skills.GetArrayLength()}");
Console.WriteLine();

// -----------------------------------------------------------------
// 3. Build Semantic Kernel with Azure OpenAI
// -----------------------------------------------------------------
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey)
    .Build();

// -----------------------------------------------------------------
// 4. Create KernelFunctions from discovered skill actions
// -----------------------------------------------------------------
var functions = new List<KernelFunction>();

foreach (var skill in skills.EnumerateArray())
{
    var skillId = skill.GetProperty("id").GetString()!;
    var skillName = skill.GetProperty("name").GetString()!;

    if (!skill.TryGetProperty("actions", out var actions))
    {
        continue;
    }

    foreach (var action in actions.EnumerateArray())
    {
        var actionName = action.GetProperty("name").GetString()!;
        var actionDesc = action.TryGetProperty("description", out var descProp) ? descProp.GetString() : "";
        var hasContentType = action.TryGetProperty("contentType", out var ctProp);
        var contentType = hasContentType ? ctProp.GetString() : "application/json";

        // Build a rich description including the input schema so the AI knows what arguments to provide
        bool hasInput = action.TryGetProperty("inputSchema", out var inputSchema);
        string? schemaStr = hasInput ? inputSchema.GetRawText() : null;

        string fullDescription = $"[{skillName}] {actionDesc}";
        if (hasInput)
        {
            fullDescription += $"\nInput (JSON): {schemaStr}";
        }
        if (contentType == "text/markdown")
        {
            fullDescription += "\nReturns: Markdown document";
        }

        // Capture for closure
        var capturedSkillId = skillId;
        var capturedActionName = actionName;

        // Clean function name: replace hyphens with underscores for SK compatibility
        var funcName = $"{skillId.Replace("-", "_")}_{actionName}";

        KernelFunction func;
        if (hasInput)
        {
            // Action with input parameters — single JSON string argument
            func = KernelFunctionFactory.CreateFromMethod(
                async ([Description("JSON object with the input parameters matching the schema")] string arguments) =>
                {
                    Console.WriteLine($"  -> Invoking {capturedSkillId}/{capturedActionName} with: {arguments}");
                    var payload = $"{{\"skill\":\"{capturedSkillId}\",\"action\":\"{capturedActionName}\",\"arguments\":{arguments}}}";
                    var response = await httpClient.PostAsync("/skills/invoke",
                        new StringContent(payload, Encoding.UTF8, "application/json"));
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"  <- Response [{response.StatusCode}]: {result}");
                    return result;
                },
                functionName: funcName,
                description: fullDescription);
        }
        else
        {
            // Parameterless action
            func = KernelFunctionFactory.CreateFromMethod(
                async () =>
                {
                    Console.WriteLine($"  -> Invoking {capturedSkillId}/{capturedActionName} (no args)");
                    var payload = $"{{\"skill\":\"{capturedSkillId}\",\"action\":\"{capturedActionName}\",\"arguments\":{{}}}}";
                    var response = await httpClient.PostAsync("/skills/invoke",
                        new StringContent(payload, Encoding.UTF8, "application/json"));
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"  <- Response [{response.StatusCode}]: {result}");
                    return result;
                },
                functionName: funcName,
                description: fullDescription);
        }

        functions.Add(func);
    }
}

// Register all skill functions as a Semantic Kernel plugin
kernel.Plugins.AddFromFunctions("nanoFrameworkSkills", functions);

Console.WriteLine($"Registered {functions.Count} skill actions as Kernel functions:");
foreach (var f in functions)
{
    Console.WriteLine($"  {f.Name}: {f.Description?.Split('\n')[0]}");
}
Console.WriteLine("---\n");

// -----------------------------------------------------------------
// 5. Create a ChatCompletionAgent with auto function calling
// -----------------------------------------------------------------
ChatCompletionAgent agent = new()
{
    Name = "IoTSkillsAgent",
    Instructions =
        "You are an AI assistant connected to a nanoFramework IoT device via the Skills Discovery API. " +
        "The device exposes capabilities (skills) that you can invoke using the registered functions. " +
        "When a user asks about the device or its capabilities, use the appropriate function to get real data. " +
        "Always explain what you're doing before invoking a function, and present results clearly. " +
        "For markdown responses, display them formatted. " +
        "If an action fails, explain the error to the user.",
    Kernel = kernel,
    Arguments = new KernelArguments(
        new PromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        })
};

// -----------------------------------------------------------------
// 6. Interactive chat loop
// -----------------------------------------------------------------
Console.WriteLine("Skills Discovery Agent ready. Type your questions (Ctrl+C to exit).");
Console.WriteLine("Examples:");
Console.WriteLine("  - What is the current temperature?");
Console.WriteLine("  - Set the target temperature to 25 degrees");
Console.WriteLine("  - Show me the HVAC system documentation");
Console.WriteLine("  - What's the brightness? Set it to 80% in the kitchen");
Console.WriteLine();

AgentThread? thread = null;
Console.Write("User > ");
string? userInput;

while ((userInput = Console.ReadLine()) is not null)
{
    if (string.IsNullOrWhiteSpace(userInput))
    {
        Console.Write("User > ");
        continue;
    }

    ChatMessageContent message = new(AuthorRole.User, userInput);

    try
    {
        await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message, thread))
        {
            if (!string.IsNullOrEmpty(response.Message.Content))
            {
                Console.WriteLine("Assistant > " + response.Message.Content);
            }

            thread = response.Thread;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

    Console.Write("\nUser > ");
}
