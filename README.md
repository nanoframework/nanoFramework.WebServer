[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoframework.WebServer&metric=alert_status)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoframework.WebServer) [![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=nanoframework_lib-nanoframework.WebServer&metric=reliability_rating)](https://sonarcloud.io/dashboard?id=nanoframework_lib-nanoframework.WebServer) [![NuGet](https://img.shields.io/nuget/dt/nanoFramework.WebServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer/) [![#yourfirstpr](https://img.shields.io/badge/first--timers--only-friendly-blue.svg)](https://github.com/nanoframework/Home/blob/main/CONTRIBUTING.md) [![Discord](https://img.shields.io/discord/478725473862549535.svg?logo=discord&logoColor=white&label=Discord&color=7289DA)](https://discord.gg/gCyBu8T)

![nanoFramework logo](https://raw.githubusercontent.com/nanoframework/Home/main/resources/logo/nanoFramework-repo-logo.png)

-----

### Welcome to the .NET **nanoFramework** WebServer repository including Model Context Protocol (MCP)

## Build status

| Component | Build Status | NuGet Package |
|:-|---|---|
| nanoFramework.WebServer | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_apis/build/status/nanoFramework.WebServer?repoName=nanoframework%2FnanoFramework.WebServer&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_build/latest?definitionId=65&repoName=nanoframework%2FnanoFramework.WebServer&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.WebServer.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer/) |
| nanoFramework.WebServer.FileSystem | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_apis/build/status/nanoFramework.WebServer?repoName=nanoframework%2FnanoFramework.WebServer&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_build/latest?definitionId=65&repoName=nanoframework%2FnanoFramework.WebServer&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.WebServer.FileSystem.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer.FileSystem/) |
| nanoFramework.WebServer.Mcp | [![Build Status](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_apis/build/status/nanoFramework.WebServer?repoName=nanoframework%2FnanoFramework.WebServer&branchName=main)](https://dev.azure.com/nanoframework/nanoFramework.WebServer/_build/latest?definitionId=65&repoName=nanoframework%2FnanoFramework.WebServer&branchName=main) | [![NuGet](https://img.shields.io/nuget/v/nanoFramework.WebServer.Mcp.svg?label=NuGet&style=flat&logo=nuget)](https://www.nuget.org/packages/nanoFramework.WebServer.Mcp/) |

## .NET nanoFramework WebServer and Model Context Protocol (MCP) extension

This library was coded by [Laurent Ellerbach](https://github.com/Ellerbach) who generously offered it to the .NET **nanoFramework** project.

This is a simple nanoFramework WebServer. Features:

- Handle multi-thread requests
- Serve static files from any storage using [`nanoFramework.WebServer.FileSystem` NuGet](https://www.nuget.org/packages/nanoFramework.WebServer.FileSystem). Requires a target device with support for storage (having `System.IO.FileSystem` capability).
- Handle parameter in URL
- Possible to have multiple WebServer running at the same time
- supports GET/PUT and any other word
- Supports any type of header
- Supports content in POST
- Reflection for easy usage of controllers and notion of routes
- Helpers to return error code directly facilitating REST API
- HTTPS support
- [URL decode/encode](https://github.com/nanoframework/lib-nanoFramework.System.Net.Http/blob/develop/nanoFramework.System.Net.Http/Http/System.Net.HttpUtility.cs)
- **Model Context Protocol (MCP) support** for AI agent integration with automatic tool discovery and invocation. [MCP](https://github.com/modelcontextprotocol/) is a protocol specifically designed for a smooth integration with generative AI agents. The protocol is based over HTTP and this implementation is a minimal one allowing you an easy and working implementation with any kind of agent supporting MCP! More details in [this implementation here](#model-context-protocol-mcp-support).

Limitations:

- Does not support any zip in the request or response stream

## Usage

You just need to specify a port and a timeout for the queries and add an event handler when a request is incoming. With this first way, you will have an event raised every time you'll receive a request.

```csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http)
{
    // Add a handler for commands that are received by the server.
    server.CommandReceived += ServerCommandReceived;

    // Start the server.
    server.Start();

    Thread.Sleep(Timeout.Infinite);
}
```

You can as well pass a controller where you can use decoration for the routes and method supported.

```csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ControllerPerson), typeof(ControllerTest) }))
{
    // Start the server.
    server.Start();

    Thread.Sleep(Timeout.Infinite);
}
```

In this case, you're passing 2 classes where you have public methods decorated which will be called every time the route is found.

With the previous example, a very simple and straight forward Test controller will look like that:

```csharp
public class ControllerTest
{
    [Route("test"), Route("Test2"), Route("tEst42"), Route("TEST")]
    [CaseSensitive]
    [Method("GET")]
    public void RoutePostTest(WebServerEventArgs e)
    {
        string route = $"The route asked is {e.Context.Request.RawUrl.TrimStart('/').Split('/')[0]}";
        e.Context.Response.ContentType = "text/plain";
        WebServer.OutPutStream(e.Context.Response, route);
    }

    [Route("test/any")]
    public void RouteAnyTest(WebServerEventArgs e)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
    }
}
```

In this example, the `RoutePostTest` will be called every time the called url will be `test` or `Test2` or `tEst42` or `TEST`, the url can be with parameters and the method GET. Be aware that `Test` won't call the function, neither `test/`.

The `RouteAnyTest`is called whenever the url is `test/any` whatever the method is.

There is a more advance example with simple REST API to get a list of Person and add a Person. Check it in the [sample](./WebServer.Sample/ControllerPerson.cs).

> [!Important]
>
> By default the routes are not case sensitive and the attribute **must** be lowercase.
> If you want to use case sensitive routes like in the previous example, use the attribute `CaseSensitive`. As in the previous example, you **must** write the route as you want it to be responded to.

## A simple GPIO controller REST API

You will find in simple [GPIO controller sample](https://github.com/nanoframework/Samples/tree/main/samples/Webserver/WebServer.GpioRest) REST API. The controller not case sensitive and is working like this:

- To open the pin 2 as output: http://yoururl/open/2/output
- To open pin 4 as input: http://yoururl/open/4/input
- To write the value high to pin 2: http://yoururl/write/2/high
  - You can use high or 1, it has the same effect and will place the pin in high value
  - You can use low of 0, it has the same effect and will place the pin in low value
- To read the pin 4: http://yoururl/read/4, you will get as a raw text `high`or `low`depending on the state

## Authentication on controllers

Controllers support authentication. 3 types of authentications are currently implemented on controllers only:

- Basic: the classic user and password following the HTTP standard. Usage:
  - `[Authentication("Basic")]` will use the default credential of the webserver
  - `[Authentication("Basic:myuser mypassword")]` will use myuser as a user and my password as a password. Note: the user cannot contains spaces.
- APiKey in header: add ApiKey in headers with the API key. Usage:
  - `[Authentication("ApiKey")]` will use the default credential of the webserver
  - `[Authentication("ApiKeyc:akey")]` will use akey as ApiKey.
- None: no authentication required. Usage:
  - `[Authentication("None")]` will use the default credential of the webserver

The Authentication attribute applies to both public Classes an public Methods.

As for the rest of the controller, you can add attributes to define them, override them. The following example gives an idea of what can be done:

```csharp
[Authentication("Basic")]
class ControllerAuth
{
    [Route("authbasic")]
    public void Basic(WebServerEventArgs e)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
    }

    [Route("authbasicspecial")]
    [Authentication("Basic:user2 password")]
    public void Special(WebServerEventArgs e)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
    }

    [Authentication("ApiKey:superKey1234")]
    [Route("authapi")]
    public void Key(WebServerEventArgs e)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
    }

    [Route("authnone")]
    [Authentication("None")]
    public void None(WebServerEventArgs e)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
    }

    [Authentication("ApiKey")]
    [Route("authdefaultapi")]
    public void DefaultApi(WebServerEventArgs e)
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
    }
}
```

And you can pass default credentials to the server:

```csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ControllerPerson), typeof(ControllerTest), typeof(ControllerAuth) }))
{
    // To test authentication with various scenarios
    server.ApiKey = "ATopSecretAPIKey1234";
    server.Credential = new NetworkCredential("topuser", "topPassword");

    // Start the server.
    server.Start();

    Thread.Sleep(Timeout.Infinite);
}
```

With the previous example the following happens:

- All the controller by default, even when nothing is specified will use the controller credentials. In our case, the Basic authentication with the default user (topuser) and password (topPassword) will be used.
  - When calling http://yoururl/authbasic from a browser, you will be prompted for the user and password, use the default one topuser and topPassword to get access
  - When calling http://yoururl/authnone, you won't be prompted because the authentication has been overridden for no authentication
  - When calling http://yoururl/authbasicspecial, the user and password are different from the defautl ones, user2 and password is the right couple here
- If you would have define in the controller a specific user and password like `[Authentication("Basic:myuser mypassword")]`, then the default one for all the controller would have been myuser and mypassword
- When calling http://yoururl/authapi, you must pass the header `ApiKey` (case sensitive) with the value `superKey1234` to get authorized, this is overridden the default Basic authentication
- When calling http://yoururl/authdefaultapi, the default key `ATopSecretAPIKey1234` will be used so you have to pass it in the headers of the request

All up, this is an example to show how to use authentication, it's been defined to allow flexibility.

The webserver supports having multiple authentication methods or credentials for the same route. Each pair of authentication method plus credentials should have its own method in the controller:

```csharp
class MixedController
{

    [Route("sameroute")]
    [Authentication("Basic")]
    public void Basic(WebServerEventArgs e)
    {
        WebServer.OutPutStream(e.Context.Response, "sameroute: Basic");
    }

    [Authentication("ApiKey:superKey1234")]
    [Route("sameroute")]
    public void Key(WebServerEventArgs e)
    {
        WebServer.OutPutStream(e.Context.Response, "sameroute: API key #1");
    }

    [Authentication("ApiKey:superKey5678")]
    [Route("sameroute")]
    public void Key2(WebServerEventArgs e)
    {
        WebServer.OutPutStream(e.Context.Response, "sameroute: API key #2");
    }

    [Route("sameroute")]
    public void None(WebServerEventArgs e)
    {
        WebServer.OutPutStream(e.Context.Response, "sameroute: Public");
    }
}
```

The webserver selects the route for a request:

- If there are no matching methods, a not-found response (404) is returned.
- If authentication information is passed in the header of the request, then only methods that require authentication are considered. If one of the method's credentials matches the credentials passed in the request, that method is called. Otherwise a non-authorized response (401) will be returned.
- If no authentication information is passed in the header of the request:
	- If one of the methods does not require authentication, that method is called.
	- Otherwise a non-authorized response (401) will be returned. If one of the methods requires basic authentication, the `WWW-Authenticate` header is included to request credentials.

The webserver does not support more than one matching method. Calling multiple methods most likely results in an exception as a subsequent method tries to modify a response that is already processed by the first method. The webserver does not know what to do and returns an internal server error (500). The body of the response lists the matching methods.

Having multiple matching methods is considered a programming error. One way this occurs is if two methods in a controller accidentally have the same route. Returning an internal server error with the names of the methods makes it easy to discover the error. It is expected that the error is discovered and fixed in testing. Then the internal error will not occur in the application that is deployed to a device. 

## Managing incoming queries thru events

Very basic usage is the following:

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

    if (url.ToLower() == "/sayhello")
    {
        // This is simple raw text returned
        WebServer.OutPutStream(e.Context.Response, "It's working, url is empty, this is just raw text, /sayhello is just returning a raw text");
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

You can do more advance scenario like returning a full HTML page:

```csharp
WebServer.OutPutStream(e.Context.Response, "<html><head>" +
    "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
    "<a href='/Text.txt'>Download the Text.txt file</a><br>" +
    "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
```

And can get parameters from a URL a an example from the previous link on the param.html page:

```csharp
if (url.ToLower().IndexOf("/param.htm") == 0)
{
    // Test with parameters
    var parameters = WebServer.decryptParam(url);
    string toOutput = "<html><head>" +
        "<title>Hi from nanoFramework Server</title></head><body>Here are the parameters of this URL: <br />";
    foreach (var par in parameters)
    {
        toOutput += $"Parameter name: {par.Name}, Value: {par.Value}<br />";
    }
    toOutput += "</body></html>";
    WebServer.OutPutStream(e.Context.Response, toOutput);
}
```

And server static files:

```csharp
// E = USB storage
// D = SD Card
// I = Internal storage
// Adjust this based on your configuration
const string DirectoryPath = "I:\\";
string[] _listFiles;

// Gets the list of all files in a specific directory
// See the MountExample for more details if you need to mount an SD card and adjust here
// https://github.com/nanoframework/Samples/blob/main/samples/System.IO.FileSystem/MountExample/Program.cs
_listFiles = Directory.GetFiles(DirectoryPath);
// Remove the root directory
for (int i = 0; i < _listFiles.Length; i++)
{
    _listFiles[i] = _listFiles[i].Substring(DirectoryPath.Length);
}

var fileName = url.Substring(1);
// Note that the file name is case sensitive
// Very simple example serving a static file on an SD card                   
foreach (var file in _listFiles)
{
    if (file == fileName)
    {
        WebServer.SendFileOverHTTP(e.Context.Response, DirectoryPath + file);
        return;
    }
}

WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
```

> [!Important]
>
> Serving files requires the `nanoFramework.WebServer.FileSystem` nuget **AND** that the device supports storage so `System.IO.FileSystem`.

And also **REST API** is supported, here is a comprehensive example:

```csharp
if (url.ToLower().IndexOf("/api/") == 0)
{
    string ret = $"Your request type is: {e.Context.Request.HttpMethod}\r\n";
    ret += $"The request URL is: {e.Context.Request.RawUrl}\r\n";
    var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);
    if (parameters != null)
    {
        ret += "List of url parameters:\r\n";
        foreach (var param in parameters)
        {
            ret += $"  Parameter name: {param.Name}, value: {param.Value}\r\n";
        }
    }

    if (e.Context.Request.Headers != null)
    {
        ret += $"Number of headers: {e.Context.Request.Headers.Count}\r\n";
    }
    else
    {
        ret += "There is no header in this request\r\n";
    }

    foreach (var head in e.Context.Request.Headers?.AllKeys)
    {
        ret += $"  Header name: {head}, Values:";
        var vals = e.Context.Request.Headers.GetValues(head);
        foreach (var val in vals)
        {
            ret += $"{val} ";
        }

        ret += "\r\n";
    }

    if (e.Context.Request.ContentLength64 > 0)
    {

        ret += $"Size of content: {e.Context.Request.ContentLength64}\r\n";

        var contentTypes = e.Context.Request.Headers?.GetValues("Content-Type");
        var isMultipartForm = contentTypes != null && contentTypes.Length > 0 && contentTypes[0].StartsWith("multipart/form-data;");

        if(isMultipartForm)
        {
            var form = e.Context.Request.ReadForm();
            ret += $"Received a form with {form.Parameters.Length} parameters and {form.Files.Length} files.";
        }
        else 
        {
            var body = e.Context.Request.ReadBody();

            ret += $"Request body hex string representation:\r\n";
            for (int i = 0; i < body.Length; i++)
            {
                ret += body[i].ToString("X") + " ";
            }
        }

    }

    WebServer.OutPutStream(e.Context.Response, ret);
}
```

This API example is basic but as you get the method, you can choose what to do.

As you get the url, you can check for a specific controller called. And you have the parameters and the content payload!

Notice the extension methods to read the body of the request:

- ReadBody will read the data from the InputStream while the data is flowing in which might be in multiple passes depending on the size of the body
- ReadForm allows to read a multipart/form-data form and returns the text key/value pairs as well as any files in the request

Example of a result with call:

![result](./doc/POSTcapture.jpg)

And more! Check the complete example for more about this WebServer!

## Using HTTPS

You will need to generate a certificate and keys:

```csharp
X509Certificate _myWebServerCertificate509 = new X509Certificate2(_myWebServerCrt, _myWebServerPrivateKey, "1234");

// X509 RSA key PEM format 2048 bytes
        // generate with openssl:
        // > openssl req -newkey rsa:2048 -nodes -keyout selfcert.key -x509 -days 365 -out selfcert.crt
        // and paste selfcert.crt content below:
        private const string _myWebServerCrt =
@"-----BEGIN CERTIFICATE-----
MORETEXT
-----END CERTIFICATE-----";

        // this one is generated with the command below. We need a password.
        // > openssl rsa -des3 -in selfcert.key -out selfcertenc.key
        // the one below was encoded with '1234' as the password.
        private const string _myWebServerPrivateKey =
@"-----BEGIN RSA PRIVATE KEY-----
MORETEXTANDENCRYPTED
-----END RSA PRIVATE KEY-----";

using (WebServer server = new WebServer(443, HttpProtocol.Https)
{
    // Add a handler for commands that are received by the server.
    server.CommandReceived += ServerCommandReceived;
    server.HttpsCert = _myWebServerCertificate509;

    server.SslProtocols = System.Net.Security.SslProtocols.Tls | System.Net.Security.SslProtocols.Tls11 | System.Net.Security.SslProtocols.Tls12;
    // Start the server.
    server.Start();

    Thread.Sleep(Timeout.Infinite);
}
```

> [!IMPORTANT]
> Because the certificate above is not issued from a Certificate Authority it won't be recognized as a valid certificate. If you want to access the nanoFramework device with your browser, for example, you'll have to add the [CRT file](WebServer.Sample\webserver-cert.crt) as a trusted one. On Windows, you just have to double click on the CRT file and then click "Install Certificate...".

You can of course use the routes as defined earlier. Both will work, event or route with the notion of controller.

## WebServer status

It is possible to subscribe to an event to get the WebServer status. That can be useful to restart the server, put in place a retry mechanism or equivalent.

```csharp
server.WebServerStatusChanged += WebServerStatusChanged;

private static void WebServerStatusChanged(object obj, WebServerStatusEventArgs e)
{
    // Do whatever you need like restarting the server
    Debug.WriteLine($"The web server is now {(e.Status == WebServerStatus.Running ? "running" : "stopped" )}");
}
```

## E2E tests

There is a collection of postman tests `nanoFramework WebServer E2E Tests.postman_collection.json` in WebServerE2ETests which should be used for testing WebServer in real world scenario. Usage is simple:
- Import json file into Postman
- Deploy WebServerE2ETests to your device - copy IP
- Set the `base_url` variable to match your device IP address 
- Choose request you want to test or run whole collection and check tests results.

The WebServerE2ETests project requires the name and credentials for the WiFi access point. That is stored in the WiFi.cs file that is not part of the git repository. Build the WebServerE2ETests to create a template for that file, then change the SSID and credentials. Your credentials will not be part of a commit.


## Model Context Protocol (MCP) Support

The nanoFramework WebServer provides comprehensive support for the Model Context Protocol (MCP), enabling AI agents and language models to directly interact with your embedded devices. MCP allows AI systems to discover, invoke, and receive responses from tools running on your nanoFramework device.

### Overview

The MCP implementation in nanoFramework WebServer includes:

- **Automatic tool discovery** through reflection and attributes
- **JSON-RPC 2.0 compliant** request/response handling
- **Type-safe parameter handling** with automatic deserialization from JSON to .NET objects
- **Flexible authentication** options (none, basic auth, API key)
- **Complex object support** for both input parameters and return values
- **Robust error handling** and validation

The supported version is [2025-03-26](https://github.com/modelcontextprotocol/modelcontextprotocol/blob/main/schema/2025-03-26/schema.json). Only Server features are implemented. And there is no notification neither Server Sent Events (SSE) support. The returned type is only string.

### Defining MCP Tools

MCP tools are defined using the `[McpServerTool]` attribute on static or instance methods. The attribute accepts a tool name, description, and optional output description:

```csharp
public class McpTools
{
    // Simple tool with primitive parameter and return type
    [McpServerTool("echo", "Echoes the input string back to the caller")]
    public static string Echo(string input) => input;

    // Tool with numeric parameters
    [McpServerTool("calculate_square", "Calculates the square of a number minus 1")]
    public static float CalculateSquare(float number) => number * number - 1;

    // Tool with complex object parameter
    [McpServerTool("process_person", "Processes a person object and returns a summary", "A formatted string with person details")]
    public static string ProcessPerson(Person person)
    {
        return $"Processed: {person.Name} {person.Surname}, Age: {person.Age}, Location: {person.Address.City}, {person.Address.Country}";
    }

    // Tool returning complex objects
    [McpServerTool("get_default_person", "Returns a default person object", "A person object with default values")]
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
```

> [!Important]
> Only none or 1 parameter is supported for the tools. .NET nanoFramework in the relection does not support names in functions, only types are available. And the AI Agent won't necessarily send in order the paramters. It means, it's not technically possible to know which parameter is which. If you need more than one parameter, create a class. Complex types as shown in the examples are supported.

### Complex Object Definitions

You can use complex objects as parameters and return types. Use the `[Description]` attribute to provide schema documentation:

```csharp
public class Person
{
    [Description("The person's first name")]
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    [Description("The person's age in years")]
    public int Age { get; set; } = 30;
    
    public Address Address { get; set; } = new Address();
}

public class Address
{
    public string Street { get; set; } = "Unknown";
    public string City { get; set; } = "Unknown";
    public string PostalCode { get; set; } = "00000";
    public string Country { get; set; } = "Unknown";
}
```

### Setting Up MCP Server

To enable MCP support in your WebServer, follow these steps:

```csharp
public static void Main()
{
    // Connect to WiFi (device-specific code)
    var connected = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: new CancellationTokenSource(60_000).Token);
    if (!connected)
    {
        Debug.WriteLine("Failed to connect to WiFi");
        return;
    }

    // Step 1: Discover and register MCP tools
    McpToolRegistry.DiscoverTools(new Type[] { typeof(McpTools) });
    Debug.WriteLine("MCP Tools discovered and registered.");

    // Step 2: Start WebServer with MCP controller
    // You can add more types if you also want to use it as a Web Server
    // Note: HTTPS and certs are also supported, see the pervious sections
    using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
    {        // Optional: Customize MCP server information and instructions
        // This will override the default server name "nanoFramework" and version "1.0.0"
        McpServerController.ServerName = "MyIoTDevice";
        McpServerController.ServerVersion = "2.1.0";
        
        // Optional: Customize instructions sent to AI agents
        // This will override the default instruction about single request limitation
        McpServerController.Instructions = "This is my custom IoT device. Please send requests one at a time and wait for responses. Supports GPIO control and sensor readings.";
        
        server.Start();
        Thread.Sleep(Timeout.Infinite);
    }
}
```

### MCP Authentication Options

The MCP implementation supports three authentication modes:

#### 1. No Authentication (Default)
```csharp
// Use McpServerController for no authentication
var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) });
```

#### 2. Basic Authentication
```csharp
// Use McpServerBasicAuthenticationController for basic auth
var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerBasicAuthenticationController) });
server.Credential = new NetworkCredential("username", "password");
```

#### 3. API Key Authentication
```csharp
// Use McpServerKeyAuthenticationController for API key auth
var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerKeyAuthenticationController) });
server.ApiKey = "your-secret-api-key";
```

### MCP Request/Response Examples

You have a collection of [tests queries](./tests/McpEndToEndTest/requests.http) available. To run them, install the [VS Code REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client). And I encourage you to get familiar with the way of working using the [McpEndToEndTest](./tests/McpEndToEndTest/) project.

#### Tool Discovery Request

```json
POST /mcp
{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/list"
}
```

#### Tool Discovery Response

```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "result": {
        "tools": [
            {
                "name": "echo",
                "description": "Echoes the input string back to the caller",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "value": {"type": "string"}
                    },
                }
            },
            {
                "name": "process_person",
                "description": "Processes a person object and returns a summary",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "person": {
                            "type": "object",
                            "properties": {
                                "Name": {"type": "string", "description": "The person's first name"},
                                "Surname": {"type": "string"},
                                "Age": {"type": "number", "description": "The person's age in years"},
                                "Address": {
                                    "type": "object",
                                    "properties": {
                                        "Street": {"type": "string"},
                                        "City": {"type": "string"},
                                        "PostalCode": {"type": "string"},
                                        "Country": {"type": "string"}
                                    }
                                }
                            }
                        }
                    },
                }
            }
        ]
    }
}
```

> [!Note]
> the `required` field is not supported. You'll have to manage in the code the fact that you may not receive all the elements.
> In case, you require more elements, just send back to the agent that you need the missing fields, it will ask the user and send you back a proper query. With history, it will learn and call you properly the next time in most of the cases.

#### Tool Invocation Request

```json
POST /mcp
{
    "jsonrpc": "2.0",
    "id": 2,
    "method": "tools/call",
    "params": {
        "name": "process_person",
        "arguments": {
            "person": {
                "Name": "Alice",
                "Surname": "Smith",
                "Age": "28",
                "Address": {
                    "Street": "789 Oak Ave",
                    "City": "Springfield",
                    "PostalCode": "54321",
                    "Country": "USA"
                }
            }
        }
    }
}
```

> [!Note]
> Most agents will not send you numbers as number in JSON serializaton, like in the example with the age. The library will always try to convert the serialized element as the target type. It can be sent as a string, if a number is inside, it will be deserialized properly.

#### Tool Invocation Response

```json
{
    "jsonrpc": "2.0",
    "id": 2,
    "result": {
        "content": [
            {
                "type": "text",
                "text": "Processed: Alice Smith, Age: 28, Location: Springfield, USA"
            }
        ]
    }
}
```

### MCP Protocol Flow

1. **Initialization**: AI agent sends `initialize` request to establish connection
2. **Tool Discovery**: Agent requests available tools via `tools/list`
3. **Tool Invocation**: Agent calls specific tools via `tools/call` with parameters
4. **Response Handling**: Server returns results in MCP-compliant format

### Error Handling

The MCP implementation provides robust error handling:

```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "error": {
        "code": -32601,
        "message": "Method not found"
    }
}
```

Common error codes:
- `-32601`: Method not found
- `-32602`: Invalid parameters or internal error

### Best Practices

1. **Tool Design**: Keep tools focused on single responsibilities
2. **Type Safety**: Use strongly-typed parameters and return values
3. **Documentation**: Provide clear descriptions for tools and complex parameters
4. **Error Handling**: Implement proper validation in your tool methods
5. **Memory Management**: Be mindful of memory usage on embedded devices
6. **Authentication**: Use appropriate authentication for your security requirements
7. **SSL**: Use SSL encryption with certificate to protect the data transfer especially if you expose your service over Internet

### Complete Example

For a complete working example, see the [McpEndToEndTest](tests/McpEndToEndTest/) project which demonstrates:

- Tool discovery and registration
- Various parameter types (primitive, complex objects)
- WiFi connectivity setup
- Server configuration with MCP support

### .NET 10 MCP Client with Azure OpenAI

The repository also includes a [.NET 10 MCP client example](tests/McpClientTest/) that demonstrates how to connect to your nanoFramework MCP server from a full .NET application using Azure OpenAI and Semantic Kernel. This client example shows:

- **Azure OpenAI integration** using Semantic Kernel
- **MCP client connectivity** to nanoFramework devices
- **Automatic tool discovery** and registration as AI functions
- **Interactive chat interface** that can invoke tools on your embedded device
- **Real-time communication** between AI agents and nanoFramework hardware

The client uses the official ModelContextProtocol NuGet package and can automatically discover and invoke any tools exposed by your nanoFramework MCP server, enabling seamless AI-to-hardware interactions.

```csharp
// Example: Connect .NET client to nanoFramework MCP server
var mcpToolboxClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions()
    {
        Endpoint = new Uri("http://192.168.1.139/mcp"), // Your nanoFramework device IP
        TransportMode = HttpTransportMode.StreamableHttp,
    }, new HttpClient()));

// Register discovered tools with Semantic Kernel
var tools = await mcpToolboxClient.ListToolsAsync();
kernel.Plugins.AddFromFunctions("MyDeviceTools", tools.Select(t => t.AsKernelFunction()));
```

This comprehensive MCP support enables your nanoFramework devices to seamlessly integrate with AI systems and language models, opening up new possibilities for intelligent embedded applications.

## Feedback and documentation

For documentation, providing feedback, issues and finding out how to contribute please refer to the [Home repo](https://github.com/nanoframework/Home).

Join our Discord community [here](https://discord.gg/gCyBu8T).

## Credits

The list of contributors to this project can be found at [CONTRIBUTORS](https://github.com/nanoframework/Home/blob/main/CONTRIBUTORS.md).

## License

The **nanoFramework** WebServer library is licensed under the [MIT license](LICENSE.md).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behaviour in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct).

## .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).
