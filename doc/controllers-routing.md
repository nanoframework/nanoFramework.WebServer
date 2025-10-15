# Controllers and Routing

This guide covers how to use controller-based routing in nanoFramework WebServer.

## Overview

Controllers provide a clean way to organize your web endpoints using attributes and method decorations. Instead of handling all requests in a single event handler, you can create multiple controller classes with decorated methods that handle specific routes.

## Basic Controller Setup

This is a basic example with a controller names `MyController`. You can have as many controllers as you want.

```csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(MyController) }))
{
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

## Route Attributes

Single and multi route is supported.

### Single Route

```csharp
public class TestController
{
    [Route("test")]
    [Method("GET")]
    public void GetTest(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Test endpoint");
    }
}
```

### Multiple Routes

```csharp
public class TestController
{
    [Route("test"), Route("Test2"), Route("tEst42"), Route("TEST")]
    [CaseSensitive]
    [Method("GET")]
    public void MultipleRoutes(WebServerEventArgs e)
    {
        string route = e.Context.Request.RawUrl.TrimStart('/').Split('/')[0];
        WebServer.OutputAsStream(e.Context.Response, $"Route: {route}");
    }
}
```

### Case Sensitivity

By default, routes are **not case sensitive** and the attribute **must** be lowercase. Use `[CaseSensitive]` to have them case sensitive.

```csharp
public class TestController
{
    [Route("test")]  // Will match: test, TEST, Test, TeSt, etc.
    public void CaseInsensitive(WebServerEventArgs e)
    {
        // Implementation
    }

    [Route("Test")]  // Case sensitive - matches only "Test"
    [CaseSensitive]
    public void CaseSensitive(WebServerEventArgs e)
    {
        // Implementation
    }
}
```

## HTTP Methods

This section describes the different methods and how to use them.

### Specific Methods

```csharp
public class ApiController
{
    [Route("api/data")]
    [Method("GET")]
    public void GetData(WebServerEventArgs e)
    {
        // Handle GET requests
    }

    [Route("api/data")]
    [Method("POST")]
    public void PostData(WebServerEventArgs e)
    {
        // Handle POST requests
    }

    [Route("api/data")]
    [Method("PUT")]
    public void PutData(WebServerEventArgs e)
    {
        // Handle PUT requests
    }

    [Route("api/data")]
    [Method("DELETE")]
    public void DeleteData(WebServerEventArgs e)
    {
        // Handle DELETE requests
    }
}
```

### Any Method

```csharp
public class TestController
{
    [Route("api/any")]
    public void HandleAnyMethod(WebServerEventArgs e)
    {
        // Will handle GET, POST, PUT, DELETE, etc.
        string method = e.Context.Request.HttpMethod;
        WebServer.OutputAsStream(e.Context.Response, $"Method: {method}");
    }
}
```

## URL Parameters

URL can contains parameters. A specific function `DecodeParam` will allow you to decode them. For path parameters, the pattern is provided below.

### Query Parameters

```csharp
[Route("api/search")]
public void Search(WebServerEventArgs e)
{
    var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);
    foreach (var param in parameters)
    {
        Debug.WriteLine($"{param.Name}: {param.Value}");
    }
}
```

Example URL: `/api/search?q=nanoframework&category=iot&limit=10`

### Path Parameters

#### Traditional Path Parsing

```csharp
[Route("api/users")]
public void GetUser(WebServerEventArgs e)
{
    string url = e.Context.Request.RawUrl;
    string[] segments = url.TrimStart('/').Split('/');
    
    if (segments.Length > 2)
    {
        string userId = segments[2]; // /api/users/123
        WebServer.OutputAsStream(e.Context.Response, $"User ID: {userId}");
    }
}
```

#### Parameterized Routes

nanoFramework WebServer supports parameterized routes with named placeholders using curly braces `{}`. The `GetRouteParameter` method allows you to extract parameter values by name:

```csharp
[Route("api/users/{id}")]
public void GetUserById(WebServerEventArgs e)
{
    string userId = e.GetRouteParameter("id");
    WebServer.OutputAsStream(e.Context.Response, $"User ID: {userId}");
}

[Route("api/users/{userId}/sensors/{sensorId}")]
public void GetUserSensor(WebServerEventArgs e)
{
    string userId = e.GetRouteParameter("userId");
    string sensorId = e.GetRouteParameter("sensorId");
    
    WebServer.OutputAsStream(e.Context.Response, 
        $"User: {userId}, Sensor: {sensorId}");
}

[Route("api/devices/{deviceId}/measurements/{type}")]
public void GetDeviceMeasurement(WebServerEventArgs e)
{
    string deviceId = e.GetRouteParameter("deviceId");
    string measurementType = e.GetRouteParameter("type");
    
    // Example: /api/devices/esp32-001/measurements/temperature
    WebServer.OutputAsStream(e.Context.Response, 
        $"Device {deviceId} - {measurementType} data");
}
```

Example URLs that would match these routes:
- `/api/users/123` → `id = "123"`
- `/api/users/abc123/sensors/temp01` → `userId = "abc123"`, `sensorId = "temp01"`
- `/api/devices/esp32-001/measurements/temperature` → `deviceId = "esp32-001"`, `type = "temperature"`

## REST API Example

A very [detailed REST API sample](./rest-api.md) walk through is available as well.

The following example shows the key principles for a REST API using GET, POST and DELETE methods, including both traditional and parameterized routes.

```csharp
public class PersonController
{
    private static ArrayList persons = new ArrayList();

    [Route("api/persons")]
    [Method("GET")]
    public void GetPersons(WebServerEventArgs e)
    {
        string json = JsonConvert.SerializeObject(persons);
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, json);
    }

    [Route("api/persons/{id}")]
    [Method("GET")]
    public void GetPersonById(WebServerEventArgs e)
    {
        string personId = e.GetRouteParameter("id");
        
        // Find person by ID logic here
        var person = FindPersonById(personId);
        if (person != null)
        {
            string json = JsonConvert.SerializeObject(person);
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, json);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }

    [Route("api/persons")]
    [Method("POST")]
    public void CreatePerson(WebServerEventArgs e)
    {
        if (e.Context.Request.ContentLength64 > 0)
        {
            var body = e.Context.Request.ReadBody();
            var json = Encoding.UTF8.GetString(body, 0, body.Length);
            var person = JsonConvert.DeserializeObject(json, typeof(Person));
            
            persons.Add(person);
            
            e.Context.Response.StatusCode = 201;
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.Created);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
        }
    }

    [Route("api/persons/{id}")]
    [Method("DELETE")]
    public void DeletePersonById(WebServerEventArgs e)
    {
        string personId = e.GetRouteParameter("id");
        
        if (personId != null)
        {
            // Remove person by ID logic here
            bool removed = RemovePersonById(personId);
            if (removed)
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
            }
            else
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
            }
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
        }
    }

    // Traditional query parameter approach for compatibility
    [Route("api/persons")]
    [Method("DELETE")]
    public void DeletePerson(WebServerEventArgs e)
    {
        var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);
        string id = null;
        
        foreach (var param in parameters)
        {
            if (param.Name.ToLower() == "id")
            {
                id = param.Value;
                break;
            }
        }

        if (id != null)
        {
            // Remove person logic here
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
        }
    }
}
```

## Best Practices

1. **Organization**: Group related endpoints in the same controller
2. **Naming**: Use descriptive method names that indicate their purpose
3. **HTTP Status Codes**: Return appropriate status codes for different scenarios
4. **Content Types**: Set correct content types for responses
5. **Error Handling**: Implement proper error handling and validation
6. **URL Structure**: Use consistent URL patterns (e.g., `/api/resource` for collections)
7. **Parameterized Routes**: Prefer parameterized routes (`/api/users/{id}`) over query parameters for resource identification as they provide cleaner URLs and better RESTful design
8. **Parameter Validation**: Always validate route parameters before using them in your logic

##  Route Conflicts

nanoFramework WebServer will always try to best handle the route. That said, route conflicts can exist.

If multiple methods match the same route, the server will return an Internal Server Error (500) with details about the conflicting methods. This is considered a programming error that should be fixed during development.

```csharp
// This will cause a conflict - avoid this!
public class ConflictController
{
    [Route("test")]
    public void Method1(WebServerEventArgs e) { }

    [Route("test")]
    public void Method2(WebServerEventArgs e) { }  // Conflict!
}
```

## Request Data Access

When handling HTTP requests in your controllers, you often need to access data sent by the client. The nanoFramework WebServer provides several ways to extract and process incoming request data through the `WebServerEventArgs` parameter.

The request data can come in various forms:

- **HTTP Headers**: Metadata about the request (content type, authorization, user agent, etc.)
- **Request Body**: The main payload data sent with POST, PUT, and PATCH requests
- **URL Parameters**: Query string parameters and path segments
- **Form Data**: HTML form submissions including file uploads

This section covers how to access headers and request body data in your controller methods.

### Headers

HTTP headers contain important metadata about the request. Common headers include `Content-Type`, `Authorization`, `User-Agent`, and custom headers. You can access all headers through the `Headers` collection:

```csharp
[Route("api/info")]
public void GetRequestInfo(WebServerEventArgs e)
{
    foreach (string headerName in e.Context.Request.Headers.AllKeys)
    {
        string[] values = e.Context.Request.Headers.GetValues(headerName);
        Debug.WriteLine($"Header: {headerName} = {string.Join(", ", values)}");
    }
}

### Request Body

```csharp
[Route("api/upload")]
[Method("POST")]
public void HandleUpload(WebServerEventArgs e)
{
    if (e.Context.Request.ContentLength64 > 0)
    {
        var contentTypes = e.Context.Request.Headers?.GetValues("Content-Type");
        var isMultipartForm = contentTypes != null && contentTypes.Length > 0 && 
                             contentTypes[0].StartsWith("multipart/form-data;");

        if (isMultipartForm)
        {
            var form = e.Context.Request.ReadForm();
            Debug.WriteLine($"Form has {form.Parameters.Length} parameters and {form.Files.Length} files");
        }
        else
        {
            var body = e.Context.Request.ReadBody();
            string content = Encoding.UTF8.GetString(body, 0, body.Length);
            Debug.WriteLine($"Body content: {content}");
        }
    }
}
```

## Response Helpers

This section contains how to handle JSON and HTML responses.

### JSON Response

```csharp
[Route("api/data")]
public void GetData(WebServerEventArgs e)
{
    var data = new { message = "Hello", timestamp = DateTime.UtcNow };
    string json = JsonConvert.SerializeObject(data);
    
    e.Context.Response.ContentType = "application/json";
    WebServer.OutputAsStream(e.Context.Response, json);
}
```

### HTML Response

```csharp
[Route("page")]
public void GetPage(WebServerEventArgs e)
{
    string html = "<html><body><h1>Hello nanoFramework!</h1></body></html>";
    e.Context.Response.ContentType = "text/html";
    WebServer.OutputAsStream(e.Context.Response, html);
}
```

### Custom Status Codes

Bonus point if you read up to here, you can also create your custom status code!

```csharp
[Route("api/status")]
public void CustomStatus(WebServerEventArgs e)
{
    e.Context.Response.StatusCode = 418; // I'm a teapot
    WebServer.OutputAsStream(e.Context.Response, "I'm a teapot!");
}
```
