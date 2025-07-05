# Event-Driven Programming

The nanoFramework WebServer provides a powerful event-driven architecture that allows developers to handle HTTP requests dynamically and monitor server status changes. This approach offers flexibility for scenarios where attribute-based routing isn't sufficient or when you need fine-grained control over request processing.

## Table of Contents

1. [Overview](#overview)
2. [CommandReceived Event](#commandreceived-event)
3. [WebServerStatusChanged Event](#webserverstatuschanged-event)
4. [WebServerEventArgs](#webservereventargs)
5. [Basic Event Handling Examples](#basic-event-handling-examples)
6. [Advanced Scenarios](#advanced-scenarios)
7. [Event Handling Best Practices](#event-handling-best-practices)
8. [Error Handling](#error-handling)
9. [Performance Considerations](#performance-considerations)

## Overview

The nanoFramework WebServer supports two primary events:

- **CommandReceived**: Triggered when an HTTP request is received that doesn't match any registered controller routes
- **WebServerStatusChanged**: Triggered when the server status changes (starting, running, stopped)

Event-driven programming is particularly useful for:

- Dynamic request handling without predefined routes
- Custom authentication and authorization logic
- Request logging and monitoring
- Server lifecycle management
- Fallback handling for unmatched routes

## CommandReceived Event

The `CommandReceived` event is the primary mechanism for handling HTTP requests in an event-driven manner.

### Event Signature

```csharp
public delegate void GetRequestHandler(object obj, WebServerEventArgs e);
public event GetRequestHandler CommandReceived;
```

### Basic Usage

```csharp
using System;
using System.Diagnostics;
using System.Net;
using nanoFramework.WebServer;

// Create WebServer instance
var server = new WebServer(80, HttpProtocol.Http);

// Subscribe to the event
server.CommandReceived += ServerCommandReceived;

// Start the server
server.Start();

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    var method = e.Context.Request.HttpMethod;
    
    Debug.WriteLine($"Command received: {url}, Method: {method}");

    if (url.ToLower() == "/hello")
    {
        WebServer.OutPutStream(e.Context.Response, "Hello from nanoFramework!");
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

### Parameter Handling

Extract and process URL parameters in event handlers:

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    if (url.ToLower().IndexOf("/param.htm") == 0)
    {
        // Extract parameters from URL
        var parameters = WebServer.DecodeParam(url);
        
        string response = "<html><head><title>Parameters</title></head><body>";
        response += "URL Parameters:<br/>";
        
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                response += $"Parameter: {param.Name} = {param.Value}<br/>";
            }
        }
        
        response += "</body></html>";
        WebServer.OutPutStream(e.Context.Response, response);
    }
}
```

### File Serving Example

> [!IMPORTANT] You need support for File System to use the file `nanoFramework.WebSer.FileSystem` nuget.

The following example shows the basic to download and create a file. In this example, they'll both be writen and read from then internal storage.

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    if (url.IndexOf("/download/") == 0)
    {
        string fileName = url.Substring(10); // Remove "/download/"
        string filePath = $"I:\\{fileName}";
        
        if (File.Exists(filePath))
        {
            WebServer.SendFileOverHTTP(e.Context.Response, filePath);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }
    else if (url.ToLower() == "/createfile")
    {
        // Create a test file
        File.WriteAllText("I:\\test.txt", "This is a dynamically created file");
        WebServer.OutPutStream(e.Context.Response, "File created successfully");
    }
}
```

## WebServerStatusChanged Event

Monitor server status changes to implement robust server management.

### Event Signature

```csharp
public delegate void WebServerStatusHandler(object obj, WebServerStatusEventArgs e);
public event WebServerStatusHandler WebServerStatusChanged;
```

### Status Values

The server can be in one of the following states:

```csharp
public enum WebServerStatus
{
    Stopped,
    Running
}
```

### Basic Status Monitoring

```csharp
var server = new WebServer(80, HttpProtocol.Http);

// Subscribe to status changes
server.WebServerStatusChanged += OnWebServerStatusChanged;

server.Start();

private static void OnWebServerStatusChanged(object obj, WebServerStatusEventArgs e)
{
    Debug.WriteLine($"Server status changed to: {e.Status}");
    
    if (e.Status == WebServerStatus.Running)
    {
        Debug.WriteLine("Server is now accepting requests");
        // Initialize additional services
    }
    else if (e.Status == WebServerStatus.Stopped)
    {
        Debug.WriteLine("Server has stopped");
        // Cleanup or restart logic
    }
}
```

### Server Recovery Pattern

```csharp
private static void OnWebServerStatusChanged(object obj, WebServerStatusEventArgs e)
{
    if (e.Status == WebServerStatus.Stopped)
    {
        Debug.WriteLine("Server stopped unexpectedly. Attempting restart...");
        
        // Wait a moment before restart
        Thread.Sleep(5000);
        
        try
        {
            var server = (WebServer)obj;
            if (server.Start())
            {
                Debug.WriteLine("Server successfully restarted");
            }
            else
            {
                Debug.WriteLine("Failed to restart server");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error restarting server: {ex.Message}");
            // You may want to reboot your board for example here.
        }
    }
}
```

## WebServerEventArgs

The `WebServerEventArgs` class provides access to the HTTP context and all request/response information.

### Properties

```csharp
public class WebServerEventArgs
{
    public HttpListenerContext Context { get; protected set; }
}
```

### Accessing Request Information

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var request = e.Context.Request;
    var response = e.Context.Response;
    
    // Request properties
    string url = request.RawUrl;
    string method = request.HttpMethod;
    string contentType = request.ContentType;
    var headers = request.Headers;
    var inputStream = request.InputStream;
    
    // Process request data
    if (method == "POST" && request.InputStream.Length > 0)
    {
        byte[] buffer = new byte[request.InputStream.Length];
        request.InputStream.Read(buffer, 0, buffer.Length);
        string postData = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        
        Debug.WriteLine($"POST data: {postData}");
    }
    
    // Send response
    WebServer.OutPutStream(response, "Request processed successfully");
}
```

## Basic Event Handling Examples

This section will present various event handeling patterns.

### Simple Text Response

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    switch (url.ToLower())
    {
        case "/":
            WebServer.OutPutStream(e.Context.Response, "Welcome to nanoFramework WebServer!");
            break;
            
        case "/time":
            WebServer.OutPutStream(e.Context.Response, $"Current time: {DateTime.UtcNow}");
            break;
            
        case "/info":
            var info = $"Server running on nanoFramework\nUptime: {Environment.TickCount}ms";
            WebServer.OutPutStream(e.Context.Response, info);
            break;
            
        default:
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
            break;
    }
}
```

### HTML Response with Dynamic Content

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    if (url.ToLower() == "/dashboard")
    {
        // Note: in the real life, you'll remove all the carrier returns and spaces to save spae and make it as efficient
        // This is nicely stringify for easy reading.
        string html = $@"
        <html>
        <head>
            <title>nanoFramework Dashboard</title>
            <style>body {{ font-family: Arial, sans-serif; }}</style>
        </head>
        <body>
            <h1>System Dashboard</h1>
            <p>Current Time: {DateTime.UtcNow}</p>
            <p>Uptime: {Environment.TickCount} ms</p>
            <p>Free Memory: {System.GC.GetTotalMemory(false)} bytes</p>
            <a href='/'>Home</a> | <a href='/info'>Info</a>
        </body>
        </html>";
        
        WebServer.OutPutStream(e.Context.Response, html);
    }
}
```

### JSON API Response

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    if (url.ToLower().IndexOf("/api/") == 0)
    {
        e.Context.Response.ContentType = "application/json";
        
        if (url.ToLower() == "/api/status")
        {
            string json = $@"{{
                ""status"": ""running"",
                ""timestamp"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}"",
                ""uptime"": {Environment.TickCount},
                ""memory"": {System.GC.GetTotalMemory(false)}
            }}";
            
            WebServer.OutPutStream(e.Context.Response, json);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }
}
```

## Advanced Scenarios

This section presents advance scenarios.

### Request Logging and Analytics

This is a simple logging scenario. Note that [nanoFramework also provides a logging library](https://github.com/nanoframework/nanoFramework.Logging).

```csharp
private static readonly ArrayList RequestLog = new ArrayList();

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var request = e.Context.Request;
    
    // Log request details
    var logEntry = new
    {
        Timestamp = DateTime.UtcNow,
        Method = request.HttpMethod,
        Url = request.RawUrl,
        UserAgent = request.Headers["User-Agent"],
        RemoteEndPoint = request.RemoteEndPoint?.ToString()
    };
    
    RequestLog.Add(logEntry);
    Debug.WriteLine($"Request logged: {request.HttpMethod} {request.RawUrl}");
    
    // Limit log size
    if (RequestLog.Count > 100)
    {
        RequestLog.RemoveAt(0);
    }
    
    // Handle request normally
    HandleRequest(e);
}

private static void HandleRequest(WebServerEventArgs e)
{
    // Your normal request handling logic
    var url = e.Context.Request.RawUrl;
    
    if (url == "/logs")
    {
        // Return request logs
        string response = "Recent Requests:\n";
        foreach (var entry in RequestLog)
        {
            response += $"{entry}\n";
        }
        WebServer.OutPutStream(e.Context.Response, response);
    }
    else
    {
        // Handle other requests
        WebServer.OutPutStream(e.Context.Response, "Request processed");
    }
}
```

### Custom Authentication

This example shows you how you can manage your own authentication mechanism.

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var request = e.Context.Request;
    var url = request.RawUrl;
    
    // Check if route requires authentication
    if (RequiresAuth(url))
    {
        if (!IsAuthenticated(request))
        {
            e.Context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Secure Area\"");
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.Unauthorized);
            return;
        }
    }
    
    // Process authenticated request
    HandleAuthenticatedRequest(e);
}

private static bool RequiresAuth(string url)
{
    return url.StartsWith("/admin/") || url.StartsWith("/secure/");
}

private static bool IsAuthenticated(HttpListenerRequest request)
{
    var credentials = request.Credentials;
    if (credentials == null) return false;
    
    // Check credentials against your authentication system
    return credentials.UserName == "admin" && credentials.Password == "password";
}
```

### Content Type Handling

Here is a simple pattern that will allow you to handle different types of content published.

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var request = e.Context.Request;
    var response = e.Context.Response;
    
    // Handle different content types
    switch (request.ContentType?.ToLower())
    {
        case "application/json":
            HandleJsonRequest(e);
            break;
            
        case "application/x-www-form-urlencoded":
            HandleFormRequest(e);
            break;
            
        case "multipart/form-data":
            HandleMultipartRequest(e);
            break;
            
        default:
            HandleDefaultRequest(e);
            break;
    }
}

private static void HandleJsonRequest(WebServerEventArgs e)
{
    // Read JSON payload
    var buffer = new byte[e.Context.Request.InputStream.Length];
    e.Context.Request.InputStream.Read(buffer, 0, buffer.Length);
    string jsonData = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
    
    Debug.WriteLine($"Received JSON: {jsonData}");
    
    // Process JSON and respond
    e.Context.Response.ContentType = "application/json";
    WebServer.OutPutStream(e.Context.Response, "{\"status\":\"success\"}");
}
```

## Event Handling Best Practices

This section provides good practices and patterns.

### 1. Always Handle Exceptions

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    try
    {
        HandleRequest(e);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error handling request: {ex.Message}");
        
        try
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.InternalServerError);
        }
        catch
        {
            // Context might be disposed, ignore
        }
    }
}
```

### 2. Use Asynchronous Processing for Long Operations

While nanoFramework does not have a await/async yet, you can use threads. And for long running tasks, use `Threads` to run your long process, and provide then another endpoint to check if your process is finished or not.

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    if (url == "/longprocess")
    {
        // Start long operation in background thread
        new Thread(() =>
        {
            try
            {
                ProcessLongRunningTask(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Background task error: {ex.Message}");
            }
        }).Start();
        
        // Return immediate response
        WebServer.OutPutStream(e.Context.Response, "Processing started");
    }
    // Here, you would implement another routing for returning the status of your long process.
}
```

### 3. Validate Input Data

One of the key elements is to never trust what's send to you and always validate everything. If you are using controllers, this is done for you. If you can't use controlles, then, always check the inputs.

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var request = e.Context.Request;
    
    // Validate HTTP method
    if (request.HttpMethod != "GET" && request.HttpMethod != "POST")
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.MethodNotAllowed);
        return;
    }
    
    // Validate URL format
    if (string.IsNullOrEmpty(request.RawUrl))
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
        return;
    }
    
    // Continue with processing
    HandleValidatedRequest(e);
}
```

### 4. Implement Proper Resource Cleanup

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    FileStream fileStream = null;
    
    try
    {
        var url = e.Context.Request.RawUrl;
        
        if (url.StartsWith("/file/"))
        {
            string fileName = url.Substring(6);
            fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            
            // Process file
            WebServer.SendFileOverHTTP(e.Context.Response, fileName);
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error: {ex.Message}");
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.InternalServerError);
    }
    finally
    {
        fileStream?.Dispose();
    }
}
```

## Error Handling

This section provides details on error handeling for specific cases typically out of memory and IO exceptions. While this appear in this overall event driven section, this can also apply to controllers.

### Graceful Error Recovery

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var request = e.Context.Request;
    var response = e.Context.Response;
    
    try
    {
        ProcessRequest(e);
    }
    catch (OutOfMemoryException)
    {
        // Force garbage collection
        System.GC.Collect();
        
        response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
        WebServer.OutPutStream(response, "Service temporarily unavailable - low memory");
    }
    catch (System.IO.IOException ioEx)
    {
        Debug.WriteLine($"IO Error: {ioEx.Message}");
        WebServer.OutputHttpCode(response, HttpStatusCode.InternalServerError);
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Unexpected error: {ex.Message}");
        WebServer.OutputHttpCode(response, HttpStatusCode.InternalServerError);
    }
}
```

### Request Timeout Handling

Here is an handy pattern with a simple timer to manage timeout. You can acheive something similar with cancellation token as well.

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var timeoutTimer = new Timer(HandleTimeout, e.Context, 30000, Timeout.Infinite);
    
    try
    {
        ProcessRequest(e);
        timeoutTimer.Dispose();
    }
    catch
    {
        timeoutTimer.Dispose();
        throw;
    }
}

private static void HandleTimeout(object state)
{
    var context = (HttpListenerContext)state;
    
    try
    {
        WebServer.OutputHttpCode(context.Response, HttpStatusCode.RequestTimeout);
    }
    catch
    {
        // Context might be disposed
    }
}
```

## Performance Considerations

While those advices are in this event driven section, they will also apply in controllers.

### 1. Minimize Allocations in Event Handlers

```csharp
// Reuse string builders and buffers
private static readonly StringBuilder ResponseBuilder = new StringBuilder();
private static readonly byte[] Buffer = new byte[1024];

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    ResponseBuilder.Clear();
    ResponseBuilder.Append("Response data: ");
    ResponseBuilder.Append(DateTime.UtcNow);
    
    WebServer.OutPutStream(e.Context.Response, ResponseBuilder.ToString());
}
```

### 2. Cache Frequently Used Data

```csharp
private static readonly Hashtable ResponseCache = new Hashtable();

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    // Check cache first
    if (ResponseCache.Contains(url))
    {
        string cachedResponse = (string)ResponseCache[url];
        WebServer.OutPutStream(e.Context.Response, cachedResponse);
        return;
    }
    
    // Generate response
    string response = GenerateResponse(url);
    
    // Cache response (with size limit)
    if (ResponseCache.Count < 50)
    {
        ResponseCache[url] = response;
    }
    
    WebServer.OutPutStream(e.Context.Response, response);
}
```

### 3. Use Efficient String Operations

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    // Use IndexOf instead of StartsWith for better performance on nanoFramework
    if (url.IndexOf("/api/") == 0)
    {
        HandleApiRequest(e);
    }
    else if (url.IndexOf("/static/") == 0)
    {
        HandleStaticContent(e);
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

The event-driven approach provides maximum flexibility for handling HTTP requests in nanoFramework applications. By combining the `CommandReceived` and `WebServerStatusChanged` events with proper error handling and performance considerations, you can build robust and responsive web applications that handle a wide variety of scenarios.
