#!/usr/bin/dotnet run

#:package DotNetEnv@3.1.1
#:package ModelContextProtocol@0.2.0-preview.3
#:package Microsoft.SemanticKernel@1.49.0

// Note: this is .NET single file. Run with: dotnet run McpClientTest.cs

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

// Load environment variables from .env file
DotNetEnv.Env.Load();

//
// 1. Create MCP Toolbox client (SSE/HTTP)
//
var mcpToolboxClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions()
    {
        Endpoint = new Uri("http://192.168.1.139/mcp"),
        TransportMode = HttpTransportMode.StreamableHttp,
    }, new HttpClient()));
// --

var kernel = Kernel.CreateBuilder()
                    .AddAzureOpenAIChatCompletion(
                        DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_NAME"),
                        DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_ENDPOINT"),
                        DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_API_KEY")
                    )
                    .Build();

//
// 2. Register MCP Toolbox client as a tool
//
var tools = await mcpToolboxClient.ListToolsAsync().ConfigureAwait(false);

// Print those tools
Console.WriteLine("// Available tools:");
foreach (var t in tools) Console.WriteLine($"{t.Name}: {t.Description}");
Console.WriteLine("// --");

// Load them as AI functions in the kernel
#pragma warning disable SKEXP0001
kernel.Plugins.AddFromFunctions("MyComputerToolbox", tools.Select(aiFunction => aiFunction.AsKernelFunction()));
// --

var history = new ChatHistory();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

Console.Write("User > ");
string? userInput;

while ((userInput = Console.ReadLine()) is not null)
{
    // Add user input
    history.AddUserMessage(userInput);

    OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
    {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,

    };

    // Get the response from the AI
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);

    // Get user input again
    Console.Write("User > ");
}
