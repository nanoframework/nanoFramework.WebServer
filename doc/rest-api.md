# REST API Development

The nanoFramework WebServer provides comprehensive support for building RESTful APIs on embedded devices. This guide covers everything from basic API endpoints to advanced features like parameter handling, content negotiation, and error responses.

## Table of Contents

- [Overview](#overview)
- [Quick Start](#quick-start)
- [HTTP Methods](#http-methods)
- [URL Parameters](#url-parameters)
- [Request Body Handling](#request-body-handling)
- [Response Formats](#response-formats)
- [Error Handling](#error-handling)
- [Content Types](#content-types)
- [Authentication](#authentication)
- [Advanced Examples](#advanced-examples)
- [Best Practices](#best-practices)
- [Testing](#testing)

## Overview

REST (Representational State Transfer) APIs enable communication between clients and your nanoFramework device using standard HTTP methods and JSON data exchange. The WebServer provides built-in support for:

- **HTTP Methods**: GET, POST, PUT, DELETE, PATCH, etc.
- **Parameter Extraction**: URL parameters, query strings, and request bodies
- **Content Negotiation**: JSON, XML, plain text, and custom formats
- **Status Codes**: Standard HTTP response codes
- **Authentication**: Basic Auth, API Keys, and custom schemes
- **Error Handling**: Structured error responses

## Quick Start

### Basic API Controller

To have more information about controllers, routes, method and authentication, check out the [specific Controller documentation](./controllers-routing.md).

```csharp
using System;
using System.Net;
using nanoFramework.WebServer;

public class ApiController
{
    [Route("api/status")]
    [Method("GET")]
    public void GetStatus(WebServerEventArgs e)
    {
        var response = new GetResponse()
        {
            Status = "running",
            Timestamp = DateTime.UtcNow.ToString(),
            Uptime = "2d 5h 30m"
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
    }
    
    [Route("api/hello")]
    [Method("POST")]
    public void PostHello(WebServerEventArgs e)
    {
        if (e.Context.Request.ContentLength64 > 0)
        {
            var body = e.Context.Request.ReadBody();
            var content = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            
            var response = $"{{\"message\":\"Hello, {content}!\"}}";
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, response);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
        }
    }
}
```

### Starting the API Server

This is the minimal code requiring a valid network connectin through ethernet or wifi.

```csharp
public static void Main()
{
    // Connect to a network. This is device specific.

    using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ApiController) }))
    {
        server.Start();
        Console.WriteLine("REST API server running on port 80");
        Thread.Sleep(Timeout.Infinite);
    }
}
```

## HTTP Methods

This section will provide various example of methods and routes.

### Data Models

First, let's define the data classes used in our examples:

```csharp
public class DeviceInfo
{
    public string DeviceId { get; set; }
    public string Model { get; set; }
    public string Firmware { get; set; }
    public long FreeMemory { get; set; }
    public TimeSpan Uptime { get; set; }
    public string Status { get; set; }
    public DateTime LastSeen { get; set; }
    public string Location { get; set; }
    
    public DeviceInfo()
    {
        Status = "unknown";
        LastSeen = DateTime.UtcNow;
        Location = "unknown";
    }
    
    public DeviceInfo(string deviceId, string model, string firmware)
    {
        DeviceId = deviceId;
        Model = model;
        Firmware = firmware;
        Status = "online";
        LastSeen = DateTime.UtcNow;
        Location = "default";
        FreeMemory = GC.GetTotalMemory(false);
    }
}

public class Sensor
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsActive { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public string SensorType { get; set; }
    
    public Sensor()
    {
        Timestamp = DateTime.UtcNow;
        IsActive = true;
        MinValue = double.MinValue;
        MaxValue = double.MaxValue;
        SensorType = "generic";
    }
    
    public Sensor(int id, string name, double value, string unit)
    {
        Id = id;
        Name = name;
        Value = value;
        Unit = unit;
        Timestamp = DateTime.UtcNow;
        IsActive = true;
        MinValue = double.MinValue;
        MaxValue = double.MaxValue;
        SensorType = "generic";
    }
    
    public Sensor(int id, string name, double value, string unit, string sensorType, double minValue, double maxValue)
    {
        Id = id;
        Name = name;
        Value = value;
        Unit = unit;
        SensorType = sensorType;
        MinValue = minValue;
        MaxValue = maxValue;
        Timestamp = DateTime.UtcNow;
        IsActive = true;
    }
    
    public bool IsValueInRange()
    {
        return Value >= MinValue && Value <= MaxValue;
    }
    
    public void UpdateValue(double newValue)
    {
        Value = newValue;
        Timestamp = DateTime.UtcNow;
    }
}
```

This is just an example. For best unit management, it is recommended to use [UnitsNet](https://github.com/angularsen/UnitsNet). UnitsNet is supported by .NET nanoFramework. Each unit is packaged as a separated nuget.

### GET - Retrieve Data

```csharp
public class DeviceController
{
    [Route("api/device/info")]
    [Method("GET")]
    public void GetDeviceInfo(WebServerEventArgs e)
    {
        var deviceInfo = new DeviceInfo()
        {
            DeviceId = "ESP32-001",
            Model = "ESP32-WROOM-32",
            Firmware = "1.2.3",
            FreeMemory = GC.GetTotalMemory(false),
            Uptime = DateTime.UtcNow - _startTime
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(deviceInfo));
    }
    
    [Route("api/sensors")]
    [Method("GET")]
    public void GetSensors(WebServerEventArgs e)
    {
        var sensors = new Sensor[]
        {
            new Sensor() { Id = 1, Name = "Temperature", Value = 23.5, Unit = "°C" },
            new Sensor() { Id = 2, Name = "Humidity", Value = 65.2, Unit = "%" },
            new Sensor() { Id = 3, Name = "Pressure", Value = 1013.25, Unit = "hPa" }
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(sensors));
    }
    
    private static DateTime _startTime = DateTime.UtcNow;
}
```

### POST - Create Data

```csharp
public class ConfigController
{
    private static DateTime _startTime = DateTime.UtcNow;
    private static DeviceInfo _deviceInfo;
    private static Sensor[] _sensors;
    
    static DeviceController()
    {
        // Initialize device info
        _deviceInfo = new DeviceInfo("ESP32-001", "ESP32-WROOM-32", "1.2.3")
        {
            Location = "IoT Lab",
            Status = "online"
        };
        
        // Initialize sensors
        _sensors = new Sensor[]
        {
            new Sensor(1, "Temperature", 23.5, "°C", "environmental", -40, 85),
            new Sensor(2, "Humidity", 65.2, "%", "environmental", 0, 100),
            new Sensor(3, "Pressure", 1013.25, "hPa", "environmental", 300, 1100),
            new Sensor(4, "Light", 450, "lux", "optical", 0, 100000),
            new Sensor(5, "Motion", 0, "bool", "digital", 0, 1)
        };
    }

    [Route("api/config")]
    [Method("POST")]
    public void CreateConfig(WebServerEventArgs e)
    {
        try
        {
            if (e.Context.Request.ContentLength64 == 0)
            {
                var error = $"{{\"error\":\"Request body is required\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WebServer.OutputAsStream(e.Context.Response, error);
                return;
            }
            
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var config = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            // Validate required fields
            if (!config.Contains("name") || !config.Contains("value"))
            {
                var error = $"{{\"error\":\"Missing required fields: name, value\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WebServer.OutputAsStream(e.Context.Response, error);
                return;
            }
            
            // Store configuration (implement your storage logic)
            StoreConfiguration(config["name"].ToString(), config["value"].ToString());
            
            var response = new CreateResponse()
            {
                Message = "Configuration created successfully",
                Id = GenerateConfigId(),
                Timestamp = DateTime.UtcNow.ToString()
            };
            
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.Created;
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
        }
        catch (Exception ex)
        {
            var error = $"{{\"error\":\"Internal server error: {ex.Message}\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }
    
    private void StoreConfiguration(string name, string value)
    {
        // Implement your configuration storage logic
        // Typically in a Hashtable with nanoFramework
    }
    
    private string GenerateConfigId()
    {
        return Guid.NewGuid().ToString();
    }
}
```

### PUT - Update Data

```csharp
public class LedController
{
    [Route("api/led")]
    [Method("PUT")]
    public void UpdateLed(WebServerEventArgs e)
    {
        try
        {
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var request = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            if (!request.Contains("state"))
            {
                var error = $"{{\"error\":\"Missing 'state' field\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WebServer.OutputAsStream(e.Context.Response, error);
                return;
            }
            
            var state = request["state"].ToString().ToLower();
            var brightness = request.Contains("brightness") ? 
                Convert.ToInt32(request["brightness"]) : 100;
            
            if (state != "on" && state != "off")
            {
                var error = $"{{\"error\":\"State must be 'on' or 'off'\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WebServer.OutputAsStream(e.Context.Response, error);
                return;
            }
            
            // Update LED (implement your GPIO logic)
            UpdateLedState(state == "on", brightness);
            
            var response = new UpdateResponse()
            {
                Message = "LED updated successfully",
                State = state,
                Brightness = brightness,
                Timestamp = DateTime.UtcNow.ToString()
            };
            
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
        }
        catch (Exception ex)
        {
            var error = $"{{\"error\":\"Failed to update LED: {ex.Message}\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }
    
    private void UpdateLedState(bool isOn, int brightness)
    {
        // Implement your LED control logic
    }
}
```

### DELETE - Remove Data

```csharp
public class DataController
{
    private static Hashtable _dataStore = new Hashtable();

    public class DeleteResponse
    {
        public string Message { get; set; }
        public int Count { get; set; }
        public string Timestamp { get; set; }
    }

    // Delete all data
    [Route("api/data")]
    [Method("DELETE")]
    public void DeleteAllData(WebServerEventArgs e)
    {
        var count = _dataStore.Count;
        _dataStore.Clear();
        
        var response = new DeleteResponse()
        {
            Message = "All data deleted successfully",
            Count = count,
            Timestamp = DateTime.UtcNow.ToString()
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
    }

    // Delete specific item by ID using parameterized route
    [Route("api/data/{id}")]
    [Method("DELETE")]
    public void DeleteDataById(WebServerEventArgs e)
    {
        string id = e.GetRouteParameter("id");
        
        if (_dataStore.Contains(id))
        {
            _dataStore.Remove(id);
            
            var response = new DeleteResponse()
            {
                Message = $"Data with id '{id}' deleted successfully",
                Count = 1,
                Timestamp = DateTime.UtcNow.ToString()
            };
            
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, response);
        }
        else
        {
            var error = $"{{\"error\":\"Data with id '{id}' not found\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }
}
```

## URL Parameters

.NET nanoFramework WebServer allows you to get access to all the parameters passed in the URL or the URL itself with parameters in the path.

### Path Parameters

#### Parameterized Routes

Use parameterized routes with named placeholders for cleaner, more maintainable code:

```csharp
public class UserController
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class UserSettingResponse
    {
        public string UserId { get; set; }
        public string SettingName { get; set; }
        public string Value { get; set; }
    }

    [Route("api/users/{id}")]
    [Method("GET")]
    public void GetUser(WebServerEventArgs e)
    {
        string userId = e.GetRouteParameter("id");
        
        // Simulate user lookup
        var user = GetUserById(userId);
        
        if (user != null)
        {
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(user));
        }
        else
        {
            var error = $"{{\"error\":\"User {userId} not found\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }

    [Route("api/users/{userId}/settings/{settingName}")]
    [Method("GET")]
    public void GetUserSetting(WebServerEventArgs e)
    {
        string userId = e.GetRouteParameter("userId");
        string settingName = e.GetRouteParameter("settingName");
        
        var setting = GetUserSetting(userId, settingName);
        if (setting != null)
        {
            var response = new UserSettingResponse()
            {
                UserId = userId,
                SettingName = settingName,
                Value = setting
            };
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
        }
        else
        {
            var error = $"{{\"error\":\"Setting {settingName} not found for user {userId}\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }
    
    private User GetUserById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        // Implement user lookup logic
        return new User() { Id = id, Name = "John Doe", Email = "john@example.com" };
    }
    
    private string GetUserSetting(string userId, string setting)
    {
        // Implement setting lookup logic
        return "default_value";
    }
}
```

### Query Parameters

Handle URL query parameters:

```csharp
public class SearchController
{
    public class SearchResponse
    {
        public string Query { get; set; }
        public int Total { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string SortBy { get; set; }
        public object[] Results { get; set; }
    }

    [Route("api/search")]
    [Method("GET")]
    public void Search(WebServerEventArgs e)
    {
        var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);
        
        string query = "";
        int limit = 10;
        int offset = 0;
        string sortBy = "relevance";
        
        // Extract query parameters
        foreach (var param in parameters)
        {
            switch (param.Name.ToLower())
            {
                case "q":
                case "query":
                    query = param.Value;
                    break;
                case "limit":
                    if (int.TryParse(param.Value, out int parsedLimit))
                        limit = Math.Min(parsedLimit, 100); // Max 100 results
                    break;
                case "offset":
                    if (int.TryParse(param.Value, out int parsedOffset))
                        offset = Math.Max(parsedOffset, 0);
                    break;
                case "sort":
                    sortBy = param.Value;
                    break;
            }
        }
        
        if (string.IsNullOrEmpty(query))
        {
            var error = $"{{\"error\":\"Query parameter 'q' is required\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            WebServer.OutputAsStream(e.Context.Response, error);
            return;
        }
        
        // Perform search
        var results = PerformSearch(query, limit, offset, sortBy);
        
        var response = new SearchResponse()
        {
            Query = query,
            Total = results.Length,
            Limit = limit,
            Offset = offset,
            SortBy = sortBy,
            Results = results
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
    }
    
    private object[] PerformSearch(string query, int limit, int offset, string sortBy)
    {
        // Implement search logic
        return new object[]
        {
            { "\"id\": 1, \"title\":\"Result 1\", \"score\": 0.95" },
            { "\"id\": 2, \"title\":\"Result 2\", \"score\": 0.8" },
        };
    }
}
```

## Request Body Handling

This section will explain how to handle forms submissions.

### JSON Request Bodies

```csharp
public class ProductController
{
    public class ProductCreationResponse
    {
        public string Message { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Timestamp { get; set;}
    }

    [Route("api/products")]
    [Method("POST")]
    public void CreateProduct(WebServerEventArgs e)
    {
        try
        {
            // Check content type
            var contentType = e.Context.Request.Headers?.GetValues("Content-Type")?[0];
            if (contentType != "application/json")
            {
                var error = $"{{\"error\":\"Content-Type must be application/json\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                WebServer.OutputAsStream(e.Context.Response, error);
                return;
            }
            
            // Read and parse JSON body
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var product = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            // Validate required fields
            var requiredFields = new string[] { "name", "price", "category" };
            foreach (var field in requiredFields)
            {
                if (!product.Contains(field))
                {
                    var error = $"{{\"error\":\"Missing required field: {field}\"}}";
                    e.Context.Response.ContentType = "application/json";
                    e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(error));
                    return;
                }
            }
            
            // Validate data types and ranges
            if (!decimal.TryParse(product["price"].ToString(), out decimal price) || price <= 0)
            {
                var error = $"{{\"error\":\"Price must be a positive number\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                WebServer.OutputAsStream(e.Context.Response, error);
                return;
            }
            
            // Create product
            var productId = CreateProduct(product);
            
            var response = new ProductCreationResponse()
            {
                Message = "Product created successfully",
                Id = productId,
                Name = product["name"],
                Price = price,
                Category = product["category"],
                Timestamp = DateTime.UtcNow.ToString()
            };
            
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.Created;
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
        }
        catch (Exception ex)
        {
            var error = $"{{\"error\":\"Failed to create product: {ex.Message}\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }
    
    private string CreateProduct(Hashtable product)
    {
        // Implement product creation logic
        return Guid.NewGuid().ToString();
    }
}
```

### Form Data Handling

```csharp
public class UploadController
{
    [Route("api/upload")]
    [Method("POST")]
    public void UploadFile(WebServerEventArgs e)
    {
        try
        {
            var contentType = e.Context.Request.Headers?.GetValues("Content-Type")?[0];
            
            if (contentType != null && contentType.StartsWith("multipart/form-data"))
            {
                // Handle multipart form data
                var form = e.Context.Request.ReadForm();
                
                var response = new UploadResponse()
                {
                    Message = "Upload processed successfully",
                    Parameters = form.Parameters.Length,
                    Files = form.Files.Length,
                    Timestamp = DateTime.UtcNow.ToString()
                };
                
                e.Context.Response.ContentType = "application/json";
                WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
            }
            else if (contentType == "application/x-www-form-urlencoded")
            {
                // Handle URL-encoded form data
                var body = e.Context.Request.ReadBody();
                var formData = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
                
                // Parse form data (implement parsing logic)
                var fields = ParseFormData(formData);
                
                var response = new UploadResponse()
                {
                    Message = "Form data processed successfully",
                    Fields = fields,
                    Timestamp = DateTime.UtcNow.ToString()
                };
                
                e.Context.Response.ContentType = "application/json";
                WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
            }
            else
            {
                var error = $"{{\"error\":\"Unsupported content type\"}}";
                e.Context.Response.ContentType = "application/json";
                e.Context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                WebServer.OutputAsStream(e.Context.Response, error);
            }
        }
        catch (Exception ex)
        {
            var error = $"{{\"error\":\"Upload failed: {ex.Message}\"}}";
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            WebServer.OutputAsStream(e.Context.Response, error);
        }
    }
    
    private Hashtable ParseFormData(string formData)
    {
        var result = new Hashtable();
        var pairs = formData.Split('&');
        
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                result[keyValue[0]] = keyValue[1];
            }
        }
        
        return result;
    }
}
```

## Response Formats

This section shows you the patterns for JSON, XML and CSV as returned types, the nanoFramework way!

### JSON Responses

```csharp
public class ResponseController
{
    public class SensorDataResponse
    {
        public bool Success { get; set; }
        public SensorData Data { get; set; }
        public ResponseMeta Meta { get; set; }
    }

    public class SensorData
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public string Timestamp { get; set; }
    }

    public class ResponseMeta
    {
        public string Version { get; set; }
        public string Source { get; set;
    }

    [Route("api/data/json")]
    [Method("GET")]
    public void GetJsonResponse(WebServerEventArgs e)
    {
        var data = new SensorDataResponse()
        {
            Success = true,
            Data = new SensorData()
            {
                Temperature = 23.5,
                Humidity = 65.2,
                Pressure = 1013.25,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            },
            Meta = new ResponseMeta()
            {
                Version = "1.0",
                Source = "sensor_array_1"
            }
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(data));
    }
}
```

### XML Responses

```csharp
public class XmlController
{
    [Route("api/data/xml")]
    [Method("GET")]
    public void GetXmlResponse(WebServerEventArgs e)
    {
        // As there is no official XML serializer/deserializer in nanoFramework, you'll have to create the XML manually.
        // Note that in the real life, you will also remove all the career return and spaces to gain space.
        // The date time shows how to best add it into an XML. You should do the same for the sensor data.
        // Here, they are static just for the sample.
        var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<response>
    <success>true</success>
    <data>
        <temperature>23.5</temperature>
        <humidity>65.2</humidity>
        <pressure>1013.25</pressure>
        <timestamp>" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + @"</timestamp>
    </data>
</response>";
        
        e.Context.Response.ContentType = "application/xml";
        WebServer.OutputAsStream(e.Context.Response, xml);
    }
}
```

### CSV Responses

```csharp
public class CsvController
{
    [Route("api/data/csv")]
    [Method("GET")]
    public void GetCsvResponse(WebServerEventArgs e)
    {
        var csv = new StringBuilder();
        csv.AppendLine("timestamp,temperature,humidity,pressure");
        // This is a static example but you'll get those data from a sensor history for example.
        csv.AppendLine($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},23.5,65.2,1013.25");
        csv.AppendLine($"{DateTime.UtcNow.AddMinutes(-1):yyyy-MM-dd HH:mm:ss},23.3,65.5,1013.20");
        csv.AppendLine($"{DateTime.UtcNow.AddMinutes(-2):yyyy-MM-dd HH:mm:ss},23.7,64.8,1013.30");
        
        e.Context.Response.ContentType = "text/csv";
        e.Context.Response.Headers.Add("Content-Disposition", "attachment; filename=sensor_data.csv");
        WebServer.OutputAsStream(e.Context.Response, csv.ToString());
    }
}
```

## Error Handling

Here is an handy sample on how you can manage errors with a standard returned class.

### Standardized Error Responses

```csharp
public class ErrorHandlingController
{
    public class ErrorDetail
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Timestamp { get; set; }
        public string Path { get; set;}
    }

    [Route("api/test/error")]
    [Method("GET")]
    public void TestError(WebServerEventArgs e)
    {
        var parameters = WebServer.DecodeParam(e.Context.Request.RawUrl);
        var errorType = "500"; // Default
        
        foreach (UrlParameter param in parameters)
        {
            if (param.Name == "type")
            {
                errorType = param.Value;
                break;
            }
        }
        
        switch (errorType)
        {
            case "400":
                SendError(e.Context.Response, HttpStatusCode.BadRequest, 
                    "BAD_REQUEST", "The request was malformed or invalid");
                break;
            case "401":
                SendError(e.Context.Response, HttpStatusCode.Unauthorized, 
                    "UNAUTHORIZED", "Authentication is required");
                break;
            case "403":
                SendError(e.Context.Response, HttpStatusCode.Forbidden, 
                    "FORBIDDEN", "Access to this resource is forbidden");
                break;
            case "404":
                SendError(e.Context.Response, HttpStatusCode.NotFound, 
                    "NOT_FOUND", "The requested resource was not found");
                break;
            case "429":
                SendError(e.Context.Response, (HttpStatusCode)429, 
                    "RATE_LIMITED", "Too many requests, please try again later");
                break;
            default:
                SendError(e.Context.Response, HttpStatusCode.InternalServerError, 
                    "INTERNAL_ERROR", "An unexpected error occurred");
                break;
        }
    }
    
    private void SendError(HttpListenerResponse response, HttpStatusCode statusCode, 
        string errorCode, string message)
    {
        var error = new ErrorDetail()
        {
            Code = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            Path = response.StatusDescription
        };
        
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;
        WebServer.OutputAsStream(response, JsonConvert.SerializeObject(error));
    }
}
```

### Global Error Handler

```csharp
public class GlobalErrorHandler
{
    public static void HandleError(WebServerEventArgs e, Exception ex)
    {
        var error = new ErrorDetail()
        {
            Code = "INTERNAL_ERROR",
            Message = "An unexpected error occurred",
            Details = ex.Message,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        e.Context.Response.ContentType = "application/json";
        e.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(error));
    }
}
```

## Content Types

Some application may need to support different content types. Here is a pattern to return what's needed and serialize it.

### Content Negotiation

```csharp
public class ContentNegotiationController
{
    public class SensorReading
    {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [Route("api/data")]
    [Method("GET")]
    public void GetData(WebServerEventArgs e)
    {
        var acceptHeader = e.Context.Request.Headers?.GetValues("Accept")?[0] ?? "application/json";
        
        var data = new SensorReading()
        {
            Temperature = 23.5,
            Humidity = 65.2,
            Timestamp = DateTime.UtcNow
        };
        
        if (acceptHeader.Contains("application/xml"))
        {
            // In real life, you'll remove the spaces and career return to gain space
            // XML serialization needs to be done manually as there is no official nanoFramework nuget
            var xml = $@"<?xml version=""1.0""?>
<data>
    <temperature>{data.temperature}</temperature>
    <humidity>{data.humidity}</humidity>
    <timestamp>{data.timestamp:yyyy-MM-ddTHH:mm:ssZ}</timestamp>
</data>";
            
            e.Context.Response.ContentType = "application/xml";
            WebServer.OutputAsStream(e.Context.Response, xml);
        }
        else if (acceptHeader.Contains("text/plain"))
        {
            var text = $"Temperature: {data.temperature}°C\nHumidity: {data.humidity}%\nTimestamp: {data.timestamp}";
            
            e.Context.Response.ContentType = "text/plain";
            WebServer.OutputAsStream(e.Context.Response, text);
        }
        else
        {
            // Default to JSON
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(data));
        }
    }
}
```

## Authentication

For detailed authentication, see the [specific documentation](./authentication.md). This section represents a summary.

### API Key Authentication

This samples show ho

```csharp
[Authentication("ApiKey")]
public class SecureApiController
{
    [Route("api/secure/data")]
    [Method("GET")]
    public void GetSecureData(WebServerEventArgs e)
    {
        var secureData = new SecureData()
        {
            SensitiveInfo = "This is protected data",
            AccessLevel = "admin",
            Timestamp = DateTime.UtcNow.ToString()
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(secureData));
    }
    
    [Route("api/secure/config")]
    [Method("POST")]
    [Authentication("ApiKey:special-admin-key")]
    public void UpdateConfig(WebServerEventArgs e)
    {
        var body = e.Context.Request.ReadBody();
        var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
        
        // Process configuration update
        var response = new ConfigResponse()
        {
            Message = "Configuration updated successfully",
            Timestamp = DateTime.UtcNow.ToString()
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
    }
}
```

## Advanced Examples

### Resource CRUD API

Complete CRUD (Create, Read, Update, Delete) API for managing IoT devices:

```csharp
public class IoTDeviceController
{
    private static Hashtable _devices = new Hashtable();

    public class DeviceResponse
    {
        public int Total { get; set; }
        public object[] Devices { get; set; }
    }

    public class Device
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public string LastSeen { get; set; }
    }

    public class SensorDataResponse
    {
        public string DeviceId { get; set; }
        public string SensorId { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SensorConfigResponse
    {
        public string DeviceId { get; set; }
        public string SensorId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class DeleteResponse
    {
        public string Message { get; set; }
        public string Timestamp { get; set; }
    }
    
    // GET /api/devices - List all devices
    [Route("api/devices")]
    [Method("GET")]
    public void GetDevices(WebServerEventArgs e)
    {
        var devices = new object[_devices.Count];
        int index = 0;
        
        foreach (DictionaryEntry entry in _devices)
        {
            devices[index++] = entry.Value;
        }
        
        var response = new DeviceResponse()
        {
            Total = _devices.Count,
            Devices = devices
        };
        
        e.Context.Response.ContentType = "application/json";
        WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
    }
    
    // GET /api/devices/{id} - Get specific device
    [Route("api/devices/{id}")]
    [Method("GET")]
    public void GetDevice(WebServerEventArgs e)
    {
        string deviceId = e.GetRouteParameter("id");
        
        if (_devices.Contains(deviceId))
        {
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(_devices[deviceId]));
        }
        else
        {
            SendNotFound(e.Context.Response, $"Device {deviceId} not found");
        }
    }
    
    // GET /api/devices/{deviceId}/sensors/{sensorId} - Get specific sensor data
    [Route("api/devices/{deviceId}/sensors/{sensorId}")]
    [Method("GET")]
    public void GetDeviceSensor(WebServerEventArgs e)
    {
        string deviceId = e.GetRouteParameter("deviceId");
        string sensorId = e.GetRouteParameter("sensorId");
        
        if (_devices.Contains(deviceId))
        {
            var sensorData = GetSensorData(deviceId, sensorId);
            if (sensorData != null)
            {
                e.Context.Response.ContentType = "application/json";
                WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(sensorData));
            }
            else
            {
                SendNotFound(e.Context.Response, $"Sensor {sensorId} not found on device {deviceId}");
            }
        }
        else
        {
            SendNotFound(e.Context.Response, $"Device {deviceId} not found");
        }
    }
    
    // POST /api/devices - Create new device
    [Route("api/devices")]
    [Method("POST")]
    public void CreateDevice(WebServerEventArgs e)
    {
        try
        {
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var deviceData = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            // Validate required fields
            if (!deviceData.Contains("name") || !deviceData.Contains("type"))
            {
                SendBadRequest(e.Context.Response, "Missing required fields: name, type");
                return;
            }
            
            var deviceId = Guid.NewGuid().ToString();
            var device = new Device()
            {
                Id = deviceId,
                Name = deviceData["name"].ToString(),
                Type = deviceData["type"].ToString(),
                Status = "offline",
                CreatedAt = DateTime.UtcNow.ToString(),
                LastSeen = DateTime.UtcNow.ToString()
            };
            
            _devices[deviceId] = device;
            
            e.Context.Response.ContentType = "application/json";
            e.Context.Response.StatusCode = (int)HttpStatusCode.Created;
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(device));
        }
        catch (Exception ex)
        {
            SendInternalError(e.Context.Response, ex.Message);
        }
    }
    
    // PUT /api/devices/{id} - Update device
    [Route("api/devices/{id}")]
    [Method("PUT")]
    public void UpdateDevice(WebServerEventArgs e)
    {
        string deviceId = e.GetRouteParameter("id");
        
        if (!_devices.Contains(deviceId))
        {
            SendNotFound(e.Context.Response, $"Device {deviceId} not found");
            return;
        }
        
        try
        {
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var updateData = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            var currentDevice = _devices[deviceId] as Hashtable;
            
            // Update allowed fields
            if (updateData.Contains("name"))
            {    
                currentDevice["name"] = updateData["name"];
            }
            if (updateData.Contains("status"))
            {    
                currentDevice["status"] = updateData["status"];
            }
            if (updateData.Contains("type"))
            {
                currentDevice["type"] = updateData["type"];
            }
            
            currentDevice["lastSeen"] = DateTime.UtcNow.ToString();
            
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(currentDevice));
        }
        catch (Exception ex)
        {
            SendInternalError(e.Context.Response, ex.Message);
        }
    }
    
    // PUT /api/devices/{deviceId}/sensors/{sensorId} - Update sensor configuration
    [Route("api/devices/{deviceId}/sensors/{sensorId}")]
    [Method("PUT")]
    public void UpdateDeviceSensor(WebServerEventArgs e)
    {
        string deviceId = e.GetRouteParameter("deviceId");
        string sensorId = e.GetRouteParameter("sensorId");
        
        if (!_devices.Contains(deviceId))
        {
            SendNotFound(e.Context.Response, $"Device {deviceId} not found");
            return;
        }
        
        try
        {
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var sensorUpdate = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            var result = UpdateSensorConfiguration(deviceId, sensorId, sensorUpdate);
            
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(result));
        }
        catch (Exception ex)
        {
            SendInternalError(e.Context.Response, ex.Message);
        }
    }
    
    // DELETE /api/devices/{id} - Delete device
    [Route("api/devices/{id}")]
    [Method("DELETE")]
    public void DeleteDevice(WebServerEventArgs e)
    {
        string deviceId = e.GetRouteParameter("id");
        
        if (_devices.Contains(deviceId))
        {
            _devices.Remove(deviceId);
            
            var response = new DeleteResponse()
            {
                Message = $"Device {deviceId} deleted successfully",
                Timestamp = DateTime.UtcNow.ToString()
            };
            
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
        }
        else
        {
            SendNotFound(e.Context.Response, $"Device {deviceId} not found");
        }
    }
    
    // Helper methods
    private SensorDataResponse GetSensorData(string deviceId, string sensorId)
    {
        // Implement sensor data retrieval logic
        return new SensorDataResponse()
        {
            DeviceId = deviceId,
            SensorId = sensorId,
            Value = 23.5,
            Unit = "°C",
            Timestamp = DateTime.UtcNow
        };
    }
    
    private SensorConfigResponse UpdateSensorConfiguration(string deviceId, string sensorId, Hashtable config)
    {
        // Implement sensor configuration update logic
        return new SensorConfigResponse()
        {
            DeviceId = deviceId,
            SensorId = sensorId,
            Message = "Sensor configuration updated",
            Timestamp = DateTime.UtcNow
        };
    }

    private void SendBadRequest(HttpListenerResponse response, string message)
    {
        var error = $"{{\"error\":\"{message}\"}}";
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        WebServer.OutputAsStream(response, error);
    }

    private void SendNotFound(HttpListenerResponse response, string message)
    {
        var error = $"{{\"error\":\"{message}\"}}";
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.NotFound;
        WebServer.OutputAsStream(response, error);
    }

    private void SendInternalError(HttpListenerResponse response, string message)
    {
        var error = $"{{\"error\":\"Internal server error: {message}\"}}";
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        WebServer.OutputAsStream(response, error);
    }
}
```

### Batch Operations API

```csharp
public class BatchController
{
    [Route("api/batch/sensors")]
    [Method("POST")]
    public void BatchUpdateSensors(WebServerEventArgs e)
    {
        try
        {
            var body = e.Context.Request.ReadBody();
            var json = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);
            var batchRequest = JsonConvert.DeserializeObject(json, typeof(Hashtable)) as Hashtable;
            
            if (!batchRequest.Contains("operations"))
            {
                SendBadRequest(e.Context.Response, "Missing 'operations' array");
                return;
            }
            
            var operations = batchRequest["operations"] as ArrayList;
            var results = new ArrayList();
            
            foreach (Hashtable operation in operations)
            {
                var result = ProcessOperation(operation);
                results.Add(result);
            }
            
            var response = new BatchResponse()
            {
                Message = "Batch operation completed",
                TotalOperations = operations.Count,
                Results = results.ToArray(),
                Timestamp = DateTime.UtcNow.ToString()
            };
            
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputAsStream(e.Context.Response, JsonConvert.SerializeObject(response));
        }
        catch (Exception ex)
        {
            SendInternalError(e.Context.Response, ex.Message);
        }
    }
    
    private object ProcessOperation(Hashtable operation)
    {
        try
        {
            var sensorId = operation["sensorId"].ToString();
            var action = operation["action"].ToString();
            var value = operation.Contains("value") ? operation["value"] : null;
            
            // Process the operation based on action type
            switch (action.ToLower())
            {
                case "read":
                    return new Sensor() { SensorId = sensorId, Action = action, Value = ReadSensor(sensorId), Success = true };
                case "write":
                    WriteSensor(sensorId, value);
                    return new Sensor() { SensorId = sensorId, Action = action, Value = value, Success = true };
                case "reset":
                    ResetSensor(sensorId);
                    return new Sensor() { SensorId = sensorId, Action = action, Success = true };
                default:
                    return new Sensor() { SensorId = sensorId, Action = action, Success = false, Error = "Unknown action" };
            }
        }
        catch (Exception ex)
        {
            return new Sensor() { 
                SensorId = operation.Contains("sensorId") ? operation["sensorId"] : "unknown",
                Action = operation.Contains("action") ? operation["action"] : "unknown",
                Success = false, 
                Error = ex.Message 
            };
        }
    }
    
    private object ReadSensor(string sensorId)
    {
        // Implement sensor reading logic
        return new Random().NextDouble() * 100;
    }
    
    private void WriteSensor(string sensorId, object value)
    {
        // Implement sensor writing logic
    }
    
    private void ResetSensor(string sensorId)
    {
        // Implement sensor reset logic
    }
    
    private void SendBadRequest(HttpListenerResponse response, string message)
    {
        var error = $"{{\"error\":\"{message}\"}}";
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.BadRequest;
        WebServer.OutputAsStream(response, error);
    }
    
    private void SendInternalError(HttpListenerResponse response, string message)
    {
        var error = $"{{\"error\":\"Internal server error: {message}\"}}";
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        WebServer.OutputAsStream(response, error);
    }
}
```

## Best Practices

### 1. Consistent Response Format

Use a standardized response format across all endpoints:

```csharp
public class StandardResponse
{
    public bool Success { get; set; }
    public object Data { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
    public object Meta { get; set; }
}

public static class ResponseHelper
{
    public static void SendSuccessResponse(HttpListenerResponse response, object data, string message = null)
    {
        var standardResponse = new StandardResponse
        {
            Success = true,
            Data = data,
            Message = message,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        response.ContentType = "application/json";
        WebServer.OutputAsStream(response, JsonConvert.SerializeObject(standardResponse));
    }
    
    public static void SendErrorResponse(HttpListenerResponse response, HttpStatusCode statusCode, string message)
    {
        var standardResponse = new StandardResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
        
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode;
        WebServer.OutputAsStream(response, JsonConvert.SerializeObject(standardResponse));
    }
}
```

### 2. Input Validation

```csharp
public static class ValidationHelper
{
    public static bool ValidateRequired(Hashtable data, string[] requiredFields, out string missingField)
    {
        foreach (string field in requiredFields)
        {
            if (!data.Contains(field) || data[field] == null || string.IsNullOrEmpty(data[field].ToString()))
            {
                missingField = field;
                return false;
            }
        }
        missingField = null;
        return true;
    }
    
    public static bool ValidateRange(object value, double min, double max)
    {
        if (double.TryParse(value.ToString(), out double numValue))
        {
            return numValue >= min && numValue <= max;
        }
        return false;
    }
    
    public static bool ValidateEmail(string email)
    {
        return !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
    }
}
```

### 3. Rate Limiting

```csharp
public class RateLimiter
{
    private static Hashtable _requestCounts = new Hashtable();
    private static readonly int MaxRequests = 100;
    private static readonly TimeSpan WindowSize = TimeSpan.FromMinutes(1);
    
    public static bool IsRateLimited(string clientId)
    {
        var now = DateTime.UtcNow;
        var key = $"{clientId}_{now:yyyyMMddHHmm}";
        
        if (!_requestCounts.Contains(key))
        {
            _requestCounts[key] = 1;
            CleanupOldEntries(now);
            return false;
        }
        
        var count = (int)_requestCounts[key];
        if (count >= MaxRequests)
        {
            return true;
        }
        
        _requestCounts[key] = count + 1;
        return false;
    }
    
    private static void CleanupOldEntries(DateTime now)
    {
        var keysToRemove = new ArrayList();
        
        foreach (DictionaryEntry entry in _requestCounts)
        {
            var key = entry.Key.ToString();
            var timestamp = key.Substring(key.LastIndexOf('_') + 1);
            
            if (DateTime.TryParseExact(timestamp, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime entryTime))
            {
                if (now - entryTime > WindowSize)
                {
                    keysToRemove.Add(key);
                }
            }
        }
        
        foreach (string key in keysToRemove)
        {
            _requestCounts.Remove(key);
        }
    }
}
```

## Testing

### Using HTTP Tools

Test your REST API with curl, Postman, or VS Code REST Client:

```http
### Get device status
GET http://192.168.1.100/api/status
Accept: application/json

### Get all devices
GET http://192.168.1.100/api/devices
Accept: application/json

### Get specific device by ID
GET http://192.168.1.100/api/devices/123e4567-e89b-12d3-a456-426614174000
Accept: application/json

### Get specific sensor data
GET http://192.168.1.100/api/devices/esp32-001/sensors/temperature
Accept: application/json

### Create a new device
POST http://192.168.1.100/api/devices
Content-Type: application/json

{
    "name": "Temperature Sensor 01",
    "type": "temperature",
    "location": "Living Room"
}

### Update device by ID
PUT http://192.168.1.100/api/devices/123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json

{
    "name": "Updated Temperature Sensor",
    "status": "online"
}

### Update sensor configuration
PUT http://192.168.1.100/api/devices/esp32-001/sensors/temperature
Content-Type: application/json

{
    "sampleRate": 5000,
    "threshold": 25.0
}

### Delete specific device
DELETE http://192.168.1.100/api/devices/123e4567-e89b-12d3-a456-426614174000

### Search with query parameters
GET http://192.168.1.100/api/search?q=temperature&limit=10&sort=date
Accept: application/json

### Test error handling
GET http://192.168.1.100/api/test/error?type=404
```

## Related Resources

- [Controllers and Routing](./controllers-routing.md) - Route configuration and controller setup
- [Authentication](./authentication.md) - Securing your APIs
- [File System Support](./file-system.md) - Serving static files
- [Event-Driven Programming](./event-driven.md) - Alternative to controller-based APIs
- [Examples and Samples](./examples.md) - More complete examples

The REST API support in nanoFramework WebServer provides a solid foundation for building modern, scalable APIs on embedded devices, enabling seamless integration with web applications, mobile apps, and other IoT systems.
