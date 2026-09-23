#!/usr/bin/dotnet run

#:package DotNetEnv@3.1.1
#:package ModelContextProtocol@0.2.0-preview.3
#:package Microsoft.SemanticKernel@1.74.0
#:property JsonSerializerIsReflectionEnabledByDefault=true

// Note: this is .NET single file. Run with: dotnet run McpClientTest.cs

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// Load environment variables from .env file
var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".env");
if (!File.Exists(envPath))
{
    envPath = Path.Combine("tests", "McpClientTest", ".env");
}

DotNetEnv.Env.Load(envPath);

var deploymentName = DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_NAME");
var deploymentEndpoint = DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_ENDPOINT");
var apiKey = DotNetEnv.Env.GetString("AZUREAI_DEPLOYMENT_API_KEY");

if (string.IsNullOrWhiteSpace(deploymentName)
    || string.IsNullOrWhiteSpace(deploymentEndpoint)
    || string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException(
        $"Missing Azure OpenAI configuration in '{Path.GetFullPath(envPath)}'. " +
        "Set AZUREAI_DEPLOYMENT_NAME, AZUREAI_DEPLOYMENT_ENDPOINT, and AZUREAI_DEPLOYMENT_API_KEY.");
}

//
// 1. Create MCP Toolbox client (SSE/HTTP)
//
var mcpToolboxClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions()
    {
        Endpoint = new Uri("http://172.20.10.2/mcp"),
        TransportMode = HttpTransportMode.StreamableHttp,
    }, new HttpClient(new ContentLengthHandler(new HttpClientHandler()))));
// --

var kernel = Kernel.CreateBuilder()
                    .AddAzureOpenAIChatCompletion(
                        deploymentName,
                        deploymentEndpoint,
                        apiKey
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
kernel.Plugins.AddFromFunctions("nanoFramework", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

// Resources are not loaded into the model context automatically. Expose a function that issues a
// new resources/read request on every invocation, so a model can explicitly ask for fresh state.
var resources = await mcpToolboxClient.ListResourcesAsync().ConfigureAwait(false);

Console.WriteLine("// Available resources:");
foreach (var resource in resources)
{
    Console.WriteLine($"{resource.Uri}: {resource.Description}");
}
Console.WriteLine("// --");

var resourceReadCount = 0;

async Task<string> ReadResourceFreshAsync(string uri)
{
    var readNumber = ++resourceReadCount;
    Console.WriteLine($"// resources/read #{readNumber}: {uri}");

    var result = await mcpToolboxClient.ReadResourceAsync(uri).ConfigureAwait(false);
    var contents = FormatResourceContents(result);

    Console.WriteLine($"// resources/read #{readNumber} completed");
    return contents;
}

Func<string, Task<string>> readResource = ReadResourceFreshAsync;
var readResourceFunction = KernelFunctionFactory.CreateFromMethod(
    method: readResource,
    functionName: "read_resource",
    description: "Read the current contents of an MCP resource URI. Every call sends a new resources/read request. Call this again when current data is needed; do not assume a previous result is still current.");
kernel.Plugins.AddFromFunctions("mcp_resources", new[] { readResourceFunction });

// Check available prompts
Console.WriteLine("// Available prompts:");

try
{
    var prompts = await mcpToolboxClient.ListPromptsAsync().ConfigureAwait(false);

    List<KernelFunction> functionPrompts = new List<KernelFunction>();

    foreach (var p in prompts)
    {
        Console.WriteLine($"{p.Name}: {p.Description}");

        // compose parameters list, if any
        Dictionary<string, object?> promptArguments = new Dictionary<string, object?>();

        if (p.ProtocolPrompt.Arguments is not null)
        {
            foreach (ModelContextProtocol.Protocol.PromptArgument argument in p.ProtocolPrompt.Arguments)
            {
                if (argument.Required.HasValue && argument.Required.Value)
                {
                    // simplification here
                    // we assume that the only prompt argument from the list is the ageThreshold which we are hard coding to "65"
                    promptArguments.Add(argument.Name, "65");
                }
                else
                {
                    promptArguments.Add(argument.Name, string.Empty);
                }
            }
        }

        var promptResult = await mcpToolboxClient.GetPromptAsync(p.Name, promptArguments);

        var promptTemplate = string.Join("\n", promptResult.Messages.Select(m => m.Content));

        var semanticFunction = KernelFunctionFactory.CreateFromPrompt(
            promptTemplate: promptTemplate,
            executionSettings: (PromptExecutionSettings?)null, // Explicit cast to resolve ambiguity
            functionName: p.Name,
            description: promptResult.Description,
            templateFormat: "semantic-kernel"
        );

        functionPrompts.Add(semanticFunction);
    }

    if (functionPrompts.Any())
    {
        kernel.Plugins.AddFromFunctions("from_prompts", functionPrompts);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading prompts: {ex.Message}");
}
finally
{
    Console.WriteLine("// --");
}

var history = new ChatHistory();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

Console.Write("User > ");
string? userInput;

while ((userInput = Console.ReadLine()) is not null)
{
    if (userInput.StartsWith(":read-twice ", StringComparison.Ordinal))
    {
        var uri = userInput.Substring(":read-twice ".Length).Trim();

        if (string.IsNullOrEmpty(uri))
        {
            Console.WriteLine("Usage: :read-twice <resource-uri>");
        }
        else
        {
            var firstRead = await ReadResourceFreshAsync(uri);
            var secondRead = await ReadResourceFreshAsync(uri);

            Console.WriteLine($"// Two independent resources/read requests completed. Contents were {(string.Equals(firstRead, secondRead, StringComparison.Ordinal) ? "identical" : "different")}.");
        }

        Console.Write("User > ");
        continue;
    }

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

static string FormatResourceContents(ReadResourceResult result)
{
    return string.Join(
        "\n\n",
        result.Contents.Select(content => content switch
        {
            TextResourceContents text => text.Text,
            BlobResourceContents blob => $"[Binary resource content (base64): {blob.Blob}]",
            _ => content.ToString() ?? string.Empty,
        }));
}

sealed class ContentLengthHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Content is { Headers.ContentLength: null } content)
        {
            var bufferedContent = new ByteArrayContent(
                await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));

            foreach (var header in content.Headers)
            {
                bufferedContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            request.Content = bufferedContent;
            content.Dispose();
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
