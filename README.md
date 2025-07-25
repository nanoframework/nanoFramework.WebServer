[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoframework.WebServer&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoframework.WebServer) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoframework.WebServer&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoframework.WebServer) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.WebServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

# .NET nanoFramework WebServer with Model Context Protocol (MCP)

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.WebServer | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_apis/build/status/nanoFramework.WebServer?repoName=nanoframework%2FnanoFramework.WebServer&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_build/latest?definitionId=65&repoName=nanoframework%2FnanoFramework.WebServer&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.WebServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer/) |
| nanoFramework.WebServer.FileSystem | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_apis/build/status/nanoFramework.WebServer?repoName=nanoframework%2FnanoFramework.WebServer&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_build/latest?definitionId=65&repoName=nanoframework%2FnanoFramework.WebServer&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.WebServer.FileSystem.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer.FileSystem/) |
| nanoFramework.WebServer.Mcp | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_apis/build/status/nanoFramework.WebServer?repoName=nanoframework%2FnanoFramework.WebServer&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_build/latest?definitionId=65&repoName=nanoframework%2FnanoFramework.WebServer&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.WebServer.Mcp.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer.Mcp/) |

## Overview

This library provides a lightweight, multi-threaded HTTP/HTTPS WebServer for .NET nanoFramework with comprehensive **Model Context Protocol (MCP)** support for AI agent integration.

### Key Features

- **Multi-threaded request handling**
- **Static file serving** with FileSystem support
- **RESTful API support** with parameter handling
- **Route-based controllers** with attribute decoration
- **Authentication support** (Basic, API Key)
- **HTTPS/SSL support** with certificates
- **Model Context Protocol (MCP)** for AI agent integration
- **Automatic tool discovery** and JSON-RPC 2.0 compliance

## Quick Start

### Basic Event Based WebServer

Using the Web Server is very straight forward and supports event based calls.

'''csharp
// You need to be connected to a wifi or ethernet connection with a proper IP Address

using (WebServer server = new WebServer(80, HttpProtocol.Http))
{
    server.CommandReceived += ServerCommandReceived;
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    if (e.Context.Request.RawUrl.ToLower() == "/hello")
    {
        WebServer.OutPutStream(e.Context.Response, "Hello from nanoFramework!");
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
'''

### Controller-Based WebServer

Controllers are supported including with parametarized routes like 'api/led/{id}/dosomething/{order}'.

'''csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(MyController) }))
{
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}

public class MyController
{
    [Route("api/hello")]
    [Method("GET")]
    public void Hello(WebServerEventArgs e)
    {
        WebServer.OutPutStream(e.Context.Response, "Hello from Controller!");
    }

    [Route("api/led/{id}")]
    [Method("GET")]
    public void LedState(WebServerEventArgs e)
    {
        string ledId = e.GetRouteParameter("id");
        WebServer.OutPutStream(e.Context.Response, $"You selected Led {ledId}!");
    }
}
'''

## Model Context Protocol (MCP) Support

Enable AI agents to interact with your embedded devices through standardized tools and JSON-RPC 2.0 protocol.

### Defining MCP Tools

'''csharp
public class IoTTools
{
    [McpServerTool("read_sensor", "Reads temperature from sensor")]
    public static string ReadTemperature()
    {
        // Your sensor reading code
        return "23.5°C";
    }

    [McpServerTool("control_led", "Controls device LED", "Uutput the statusof the LED")]
    public static string ControlLed(LedCommand command)
    {
        // Your LED control code
        return $"LED set to {command.State}";
    }
}

public class LedCommand
{
    [Description("LED state: on, off, or blink")]
    public string State { get; set; }
}
'''

### Defining MCP Prompts

You can define reusable, high-level prompts for AI agents using the `McpServerPrompt` attribute. Prompts encapsulate multi-step instructions or workflows that can be invoked by agents.

Here's a simple example:

```csharp
using nanoFramework.WebServer.Mcp;

public class McpPrompts
{
    [McpServerPrompt("echo_sanity_check", "Echo test prompt")]
    public static PromptMessage[] EchoSanityCheck()
    {
        return new PromptMessage[]
        {
            new PromptMessage("Call Echo with the string 'Hello MCP world!' and return the response.")
        };
    }
}
```

Prompts can be discovered and invoked by AI agents in the same way as tools. You can also define prompts with parameters using the `McpPromptParameter` attribute.

### Setting Up MCP Server

'''csharp
public static void Main()
{
    // Connect to WiFi first
    var connected = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true);
    
    // Discover and register MCP tools
    McpToolRegistry.DiscoverTools(new Type[] { typeof(IoTTools) });

    // Discover and register MCP prompts
    McpPromptRegistry.DiscoverPrompts(new Type[] { typeof(McpPrompts) });
    
    // Start WebServer with MCP support
    using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
    {
        // Optional customization
        McpServerController.ServerName = "MyIoTDevice";
        McpServerController.Instructions = "IoT device with sensor and LED control capabilities.";
        
        server.Start();
        Thread.Sleep(Timeout.Infinite);
    }
}
'''

### AI Agent Integration

Once running, AI agents can discover and invoke your tools:

'''json
// Tool discovery
POST /mcp
{
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 1
}

// Tool invocation
POST /mcp
{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "control_led",
        "arguments": {"State": "on"}
    },
    "id": 2
}
'''

## Documentation

| Topic | Description |
|-------|-------------|
| [Controllers and Routing](./doc/controllers-routing.md) | Learn about route attributes, method decorations, and URL parameters |
| [Authentication](./doc/authentication.md) | Configure Basic Auth, API Key, and custom authentication |
| [HTTPS and Certificates](./doc/https-certificates.md) | Set up SSL/TLS encryption with certificates |
| [File System Support](./doc/file-system.md) | Serve static files from storage devices |
| [Model Context Protocol (MCP)](./doc/model-context-protocol.md) | Complete MCP guide for AI agent integration |
| [REST API Development](./doc/rest-api.md) | Build RESTful APIs with request/response handling |
| [Event-Driven Programming](./doc/event-driven.md) | Handle requests through events and status monitoring |
| [Examples and Samples](./doc/examples.md) | Working examples and code samples |

## Limitations

- No compression support in request/response streams
- MCP implementation supports server features only (no notifications or SSE)
- No or single parameter limitation for MCP tools (use complex objects for multiple parameters)
- Prompt parameters, when declared, are always mandatory.

## Installation

Install 'nanoFramework.WebServer' for the Web Server without File System support. Install 'nanoFramework.WebServer.FileSystem' for file serving, so with devices supporting File System.
Install 'nanoFramework.WebServer.Mcp' for MCP support. It does contains the full 'nanoFramework.WebServer' but does not include native file serving. You can add this feature fairly easilly by reusing the code function serving it.

## Contributing

For documentation, feedback, issues and contributions, please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

Licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
