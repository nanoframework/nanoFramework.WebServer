# Model Context Protocol (MCP) Support

The nanoFramework WebServer provides comprehensive support for the Model Context Protocol (MCP), enabling AI agents and language models to directly interact with your embedded devices. MCP allows AI systems to discover, invoke, and receive responses from tools running on your nanoFramework device.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Defining MCP Tools](#defining-mcp-tools)
- [Complex Object Support](#complex-object-support)
- [Server Setup](#server-setup)
- [Authentication Options](#authentication-options)
- [Protocol Flow](#protocol-flow)
- [Request/Response Examples](#requestresponse-examples)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)
- [Complete Examples](#complete-examples)
- [Client Integration](#client-integration)
- [Troubleshooting](#troubleshooting)

## Overview

The Model Context Protocol (MCP) is an open standard that enables seamless integration between AI applications and external data sources or tools. The nanoFramework implementation provides a lightweight, efficient MCP server that runs directly on embedded devices.

### Key Features

- **Automatic tool discovery** through reflection and attributes
- **JSON-RPC 2.0 compliant** request/response handling
- **Type-safe parameter handling** with automatic deserialization from JSON to .NET objects
- **Flexible authentication** options (none, basic auth, API key)
- **Complex object support** for both input parameters and return values
- **Robust error handling** and validation
- **Memory efficient** implementation optimized for embedded devices
- **HTTPS support** with SSL/TLS encryption

### Supported Version

This implementation supports MCP protocol version **2025-03-26** as defined in the [official schema](https://github.com/modelcontextprotocol/modelcontextprotocol/blob/main/schema/2025-03-26/schema.json).

### Limitations

- **Server features only**: No client-side features implemented
- **No notifications**: Server-sent events and notifications are not supported
- **Single parameter limitation**: Tools can have zero or one parameter (use classes for multiple values)
- **Text responses only**: All tool responses are returned as text content, classes will be serialized and send as text.

## Requirements

- **NuGet Package**: `nanoFramework.WebServer.Mcp`
- **Network Connectivity**: WiFi, Ethernet, or other network connection
- **Memory**: Sufficient RAM for JSON parsing and object serialization, also reflection used is quite memory intensive

## Quick Start

Here's a minimal MCP server setup:

```csharp
using System;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Mcp;

public class SimpleMcpTools
{
    [McpServerTool("hello", "Returns a greeting message")]
    public static string SayHello(string name)
    {
        return $"Hello, {name}! Greetings from nanoFramework.";
    }
}

public class Program
{
    public static void Main()
    {
        // Connect to WiFi (device-specific implementation)
        // ConnectToWiFi();

        // Discover and register tools
        McpToolRegistry.DiscoverTools(new Type[] { typeof(SimpleMcpTools) });

        // Start MCP server
        using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
        {
            server.Start();
            Console.WriteLine("MCP server running on port 80");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
```

## Defining MCP Tools

### Basic Tool Definition

Use the `[McpServerTool]` attribute to mark methods as MCP tools:

```csharp
public class IoTTools
{
    [McpServerTool("read_temperature", "Reads the current temperature from the sensor")]
    public static string ReadTemperature()
    {
        // Your sensor reading implementation
        float temperature = ReadTemperatureSensor();
        return $"{temperature:F1}°C";
    }

    [McpServerTool("toggle_led", "Toggles the device LED on or off", "Returns the status of the led")]
    public static string ToggleLed()
    {
        // Your LED control implementation
        bool isOn = ToggleDeviceLed();
        return $"LED is now {(isOn ? "ON" : "OFF")}";
    }
}
```

### Tools with Parameters

Tools can accept a single parameter of any type:

```csharp
public class AdvancedTools
{
    [McpServerTool("set_brightness", "Sets LED brightness level")]
    public static string SetBrightness(int level)
    {
        if (level < 0 || level > 100)
        {
            return "Error: Brightness must be between 0 and 100";
        }
        
        SetLedBrightness(level);
        return $"Brightness set to {level}%";
    }

    [McpServerTool("calculate_power", "Calculates power consumption")]
    public static string CalculatePower(float voltage)
    {
        float current = GetCurrentReading();
        float power = voltage * current;
        return $"Power: {power:F2}W (V: {voltage:F1}V, I: {current:F3}A)";
    }
}
```

### Tools with Output Descriptions

Provide output descriptions for better AI understanding:

```csharp
public class DocumentedTools
{
    [McpServerTool("get_system_info", "Retrieves system information", "JSON object containing device status")]
    public static string GetSystemInfo()
    {
        return "{\"device\":\"ESP32\",\"memory\":\"75%\",\"uptime\":\"2d 5h 30m\"}";
    }

    [McpServerTool("read_sensors", "Reads all available sensors", "Comma-separated sensor readings")]
    public static string ReadAllSensors()
    {
        return "Temperature: 23.5°C, Humidity: 65%, Pressure: 1013 hPa";
    }
}
```

## Complex Object Support

This MCP implementation supports complex types which can be implemented with classes and nested classes.

### Defining Complex Types

Use classes to handle multiple parameters or complex data structures:

```csharp
public class DeviceConfig
{
    [Description("Device name identifier")]
    public string DeviceName { get; set; }
    
    [Description("Operating mode: auto, manual, or sleep")]
    public string Mode { get; set; }
    
    [Description("Update interval in seconds")]
    public int UpdateInterval { get; set; } = 60;
    
    public WifiSettings Wifi { get; set; } = new WifiSettings();
}

public class WifiSettings
{
    [Description("WiFi network SSID")]
    public string SSID { get; set; }
    
    [Description("WiFi signal strength in dBm")]
    public int SignalStrength { get; set; }
    
    [Description("Connection status")]
    public bool IsConnected { get; set; }
}
```

### Tools with Complex Parameters

This example shows how complex objects with nested classes can be handled transparently and smoothly:

```csharp
public class ConfigurationTools
{
    [McpServerTool("configure_device", "Updates device configuration", "Configuration update status")]
    public static string ConfigureDevice(DeviceConfig config)
    {
        try
        {
            // Validate configuration
            if (string.IsNullOrEmpty(config.DeviceName))
            {
                return "Error: Device name is required";
            }
            
            if (config.UpdateInterval < 10 || config.UpdateInterval > 3600)
            {
                return "Error: Update interval must be between 10 and 3600 seconds";
            }
            
            // Apply configuration
            ApplyDeviceConfig(config);
            
            return $"Device '{config.DeviceName}' configured successfully. Mode: {config.Mode}, Interval: {config.UpdateInterval}s";
        }
        catch (Exception ex)
        {
            return $"Configuration error: {ex.Message}";
        }
    }

    [McpServerTool("get_wifi_status", "Retrieves WiFi connection status", "WiFi status information")]
    public static WifiSettings GetWifiStatus()
    {
        return new WifiSettings
        {
            SSID = GetCurrentSSID(),
            SignalStrength = GetSignalStrength(),
            IsConnected = IsWifiConnected()
        };
    }
}
```

### Nested Objects

Support for deeply nested object structures and types, for example:

```csharp
public class SensorReading
{
    public string SensorId { get; set; }
    public SensorData Data { get; set; }
    public Metadata Info { get; set; }
}

public class SensorData
{
    public float Value { get; set; }
    public string Unit { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Metadata
{
    public string Location { get; set; }
    public CalibrationInfo Calibration { get; set; }
}

public class CalibrationInfo
{
    public DateTime LastCalibrated { get; set; }
    public float Offset { get; set; }
}
```

## Server Setup

This section will go through the setup and configuration of the MCP Server.

### Basic Server Configuration

```csharp
public static void Main()
{
    // Step 1: Connect to network
    var connected = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true);
    if (!connected)
    {
        Console.WriteLine("Failed to connect to WiFi");
        return;
    }

    // Step 2: Discover and register MCP tools
    McpToolRegistry.DiscoverTools(new Type[] { 
        typeof(IoTTools), 
        typeof(ConfigurationTools),
        typeof(SensorTools)
    });

    // Step 3: Start WebServer with MCP support
    using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
    {
        server.Start();
        Console.WriteLine($"MCP server running on http://{NetworkHelper.GetLocalIpAddress()}");
        Thread.Sleep(Timeout.Infinite);
    }
}
```

### Custom Server Information

Customize the server identity and instructions:

```csharp
using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
{
    // Customize server information
    McpServerController.ServerName = "SmartThermostat";
    McpServerController.ServerVersion = "2.1.0";
    
    // Provide custom instructions for AI agents
    McpServerController.Instructions = @"
        This is a smart thermostat device with the following capabilities:
        - Temperature and humidity monitoring
        - HVAC system control (heating/cooling)
        - Schedule management
        - Energy usage tracking
        
        Please send requests one at a time and wait for responses.
        All temperature values are in Celsius unless specified otherwise.
    ";
    
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

### HTTPS Configuration

For secure communication, configure HTTPS. See the [HTTPS documentation](./https-certificates.md).

```csharp
// Generate or load your certificate
X509Certificate2 certificate = LoadOrGenerateCertificate();

using (var server = new WebServer(443, HttpProtocol.Https, new Type[] { typeof(McpServerController) }))
{
    server.HttpsCert = certificate;
    server.SslProtocols = SslProtocols.Tls12;
    
    McpServerController.ServerName = "SecureIoTDevice";
    
    server.Start();
    Console.WriteLine("Secure MCP server running on HTTPS port 443");
    Thread.Sleep(Timeout.Infinite);
}
```

## Authentication Options

### 1. No Authentication (Default)

Suitable for development and trusted networks:

```csharp
using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
{
    // No authentication configuration needed
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

### 2. Basic Authentication

Username and password authentication:

```csharp
using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerBasicAuthenticationController) }))
{
    // Set default credentials
    server.Credential = new NetworkCredential("admin", "securepassword123");
    
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

### 3. API Key Authentication

Token-based authentication:

```csharp
using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerKeyAuthenticationController) }))
{
    // Set API key
    server.ApiKey = "mcp-key-abc123def456ghi789";
    
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

Note that any authentication can be combined with HTTPS.

### Authentication in Client Requests

When authentication is enabled, clients must include credentials:

**Basic Authentication:**

```http
POST /mcp HTTP/1.1
Authorization: Basic YWRtaW46c2VjdXJlcGFzc3dvcmQxMjM=
Content-Type: application/json

{"jsonrpc":"2.0","method":"tools/list","id":1}
```

**API Key Authentication:**

```http
POST /mcp HTTP/1.1
ApiKey: mcp-key-abc123def456ghi789
Content-Type: application/json

{"jsonrpc":"2.0","method":"tools/list","id":1}
```

## Protocol Flow

### 1. Initialization

AI agent establishes connection with the MCP server:

```json
POST /mcp
{
    "jsonrpc": "2.0",
    "method": "initialize",
    "params": {
        "protocolVersion": "2025-03-26",
        "capabilities": {},
        "clientInfo": {
            "name": "AI Assistant",
            "version": "1.0.0"
        }
    },
    "id": 1
}
```

**Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "result": {
        "protocolVersion": "2025-03-26",
        "capabilities": {
            "tools": {}
        },
        "serverInfo": {
            "name": "SmartThermostat",
            "version": "2.1.0"
        },
        "instructions": "This is a smart thermostat device..."
    }
}
```

### 2. Tool Discovery

Agent discovers available tools:

```json
POST /mcp
{
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 2
}
```

The response will be the list of the tools. See next section for detailed examples.

### 3. Tool Invocation

Agent calls specific tools with parameters:

```json
POST /mcp
{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "set_temperature",
        "arguments": {
            "target": 22.5,
            "mode": "heat"
        }
    },
    "id": 3
}
```

## Request/Response Examples

This section shows real exampled of requests and responses.

### Tool Discovery

**Request:**

```json
POST /mcp

{
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 1
}
```

**Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "result": {
        "tools": [
            {
                "name": "read_temperature",
                "description": "Reads the current temperature from the sensor",
                "inputSchema": {
                    "type": "object",
                    "properties": {},
                    "required": []
                }
            },
            {
                "name": "set_brightness",
                "description": "Sets LED brightness level",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "value": {
                            "type": "number",
                            "description": "Input parameter of type Int32"
                        }
                    },
                    "required": []
                }
            },
            {
                "name": "configure_device",
                "description": "Updates device configuration",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "DeviceName": {
                            "type": "string",
                            "description": "Device name identifier"
                        },
                        "Mode": {
                            "type": "string",
                            "description": "Operating mode: auto, manual, or sleep"
                        },
                        "UpdateInterval": {
                            "type": "number",
                            "description": "Update interval in seconds"
                        },
                        "Wifi": {
                            "type": "object",
                            "properties": {
                                "SSID": {
                                    "type": "string",
                                    "description": "WiFi network SSID"
                                },
                                "SignalStrength": {
                                    "type": "number",
                                    "description": "WiFi signal strength in dBm"
                                },
                                "IsConnected": {
                                    "type": "boolean",
                                    "description": "Connection status"
                                }
                            }
                        }
                    },
                    "required": []
                }
            }
        ],
        "nextCursor": null
    }
}
```

### Simple Tool Invocation

**Request:**

```json
POST /mcp
{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "read_temperature",
        "arguments": {}
    },
    "id": 2
}
```

**Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 2,
    "result": {
        "content": [
            {
                "type": "text",
                "text": "23.5°C"
            }
        ]
    }
}
```

### Complex Tool Invocation

**Request:**

```json
POST /mcp

{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "configure_device",
        "arguments": {
            "DeviceName": "Thermostat-01",
            "Mode": "auto",
            "UpdateInterval": 120,
            "Wifi": {
                "SSID": "HomeNetwork",
                "SignalStrength": -45,
                "IsConnected": true
            }
        }
    },
    "id": 3
}
```

Note that in most cases LLM will send the payload where numbers are integrated into strings. The nanoFramework MCP server knows how to deal with this and will always cast to the proper type:

```json
POST /mcp

{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "configure_device",
        "arguments": {
            "DeviceName": "Thermostat-01",
            "Mode": "auto",
            "UpdateInterval": "120",
            "Wifi": {
                "SSID": "HomeNetwork",
                "SignalStrength": "-45",
                "IsConnected": "true"
            }
        }
    },
    "id": 3
}
```

**Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 3,
    "result": {
        "content": [
            {
                "type": "text",
                "text": "Device 'Thermostat-01' configured successfully. Mode: auto, Interval: 120s"
            }
        ]
    }
}
```

## Error Handling

The .NET nanoFramework MCP Server knows how to handle properly errors. The following will show examples of request and error responses.

### Protocol Version Mismatch

**Request with unsupported version:**

```json
{
    "jsonrpc": "2.0",
    "method": "initialize",
    "params": {
        "protocolVersion": "1.0.0"
    },
    "id": 1
}
```

**Error Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 1,
    "error": {
        "code": -32602,
        "message": "Unsupported protocol version",
        "data": {
            "supported": ["2025-03-26"],
            "requested": "1.0.0"
        }
    }
}
```

### Tool Not Found

**Request:**

```json
{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "nonexistent_tool",
        "arguments": {}
    },
    "id": 4
}
```

**Error Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 4,
    "error": {
        "code": -32601,
        "message": "Tool 'nonexistent_tool' not found"
    }
}
```

### Invalid Method

**Request:**

```json
{
    "jsonrpc": "2.0",
    "method": "invalid/method",
    "id": 5
}
```

**Error Response:**

```json
{
    "jsonrpc": "2.0",
    "id": 5,
    "error": {
        "code": -32601,
        "message": "Method not found"
    }
}
```

### Common Error Codes

| Code | Description | Common Causes |
|------|-------------|---------------|
| -32600 | Invalid Request | Malformed JSON-RPC |
| -32601 | Method Not Found | Unknown method or tool |
| -32602 | Invalid Params | Wrong parameters or unsupported protocol version |
| -32603 | Internal Error | Server-side errors |

## Best Practices

### Tool Design

1. **Single Responsibility**: Each tool should have one clear purpose
2. **Descriptive Names**: Use clear, descriptive tool names
3. **Comprehensive Descriptions**: Provide detailed descriptions for tools and parameters
4. **Error Handling**: Implement proper validation and error reporting
5. **Consistent Return Types**: Use consistent response formats

```csharp
public class WellDesignedTools
{
    [McpServerTool("measure_distance", "Measures distance using ultrasonic sensor", "Distance in centimeters")]
    public static string MeasureDistance()
    {
        try
        {
            float distance = UltrasonicSensor.GetDistance();
            
            if (distance < 0)
            {
                return "Error: Sensor reading failed";
            }
            
            if (distance > 400)
            {
                return "Out of range (max 400cm)";
            }
            
            return $"{distance:F1} cm";
        }
        catch (Exception ex)
        {
            return $"Sensor error: {ex.Message}";
        }
    }
}
```

### Performance Optimization

1. **Memory Management**: Be mindful of memory usage on embedded devices
2. **Efficient Serialization**: Keep JSON payloads small
3. **Caching**: Cache frequently accessed data
4. **Async Operations**: Use appropriate patterns for long-running operations

```csharp
public class OptimizedTools
{
    private static string _cachedSystemInfo;
    private static DateTime _lastInfoUpdate;
    
    [McpServerTool("get_cached_info", "Gets cached system information")]
    public static string GetCachedSystemInfo()
    {
        // Cache system info for 30 seconds
        if (_cachedSystemInfo == null || DateTime.UtcNow - _lastInfoUpdate > TimeSpan.FromSeconds(30))
        {
            _cachedSystemInfo = GenerateSystemInfo();
            _lastInfoUpdate = DateTime.UtcNow;
        }
        
        return _cachedSystemInfo;
    }
}
```

### Security Considerations

1. **Authentication**: Use appropriate authentication for your security requirements
2. **Input Validation**: Always validate tool parameters
3. **Rate Limiting**: Consider implementing rate limiting for sensitive operations
4. **HTTPS**: Use HTTPS for production deployments

```csharp
public class SecureTools
{
    private static DateTime _lastResetTime = DateTime.MinValue;
    
    [McpServerTool("factory_reset", "Performs factory reset (requires confirmation)")]
    public static string FactoryReset(ResetRequest request)
    {
        // Prevent frequent resets
        if (DateTime.UtcNow - _lastResetTime < TimeSpan.FromMinutes(10))
        {
            return "Error: Factory reset was performed recently. Please wait 10 minutes.";
        }
        
        // Validate confirmation
        if (request.ConfirmationCode != "FACTORY_RESET_CONFIRMED")
        {
            return "Error: Invalid confirmation code";
        }
        
        // Additional validation
        if (string.IsNullOrEmpty(request.Reason))
        {
            return "Error: Reset reason is required";
        }
        
        _lastResetTime = DateTime.UtcNow;
        PerformFactoryReset();
        
        return "Factory reset completed successfully";
    }
}

public class ResetRequest
{
    [Description("Confirmation code (must be 'FACTORY_RESET_CONFIRMED')")]
    public string ConfirmationCode { get; set; }
    
    [Description("Reason for factory reset")]
    public string Reason { get; set; }
}
```

## Complete Examples

### Smart Thermostat

A complete thermostat implementation with multiple tools:

```csharp
using System;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Mcp;

public class ThermostatConfig
{
    [Description("Target temperature in Celsius")]
    public float TargetTemperature { get; set; } = 22.0f;
    
    [Description("Operating mode: heat, cool, auto, or off")]
    public string Mode { get; set; } = "auto";
    
    [Description("Enable schedule-based operation")]
    public bool ScheduleEnabled { get; set; } = true;
}

public class ThermostatStatus
{
    public float CurrentTemperature { get; set; }
    public float TargetTemperature { get; set; }
    public string Mode { get; set; }
    public bool IsHeating { get; set; }
    public bool IsCooling { get; set; }
    public float Humidity { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class ThermostatTools
{
    private static ThermostatConfig _config = new ThermostatConfig();
    private static bool _isHeating = false;
    private static bool _isCooling = false;
    
    [McpServerTool("get_temperature", "Reads current temperature and humidity")]
    public static string GetTemperature()
    {
        float temp = ReadTemperatureSensor();
        float humidity = ReadHumiditySensor();
        
        return $"Temperature: {temp:F1}°C, Humidity: {humidity:F0}%";
    }
    
    [McpServerTool("set_target_temperature", "Sets the target temperature")]
    public static string SetTargetTemperature(float temperature)
    {
        if (temperature < 5 || temperature > 35)
        {
            return "Error: Temperature must be between 5°C and 35°C";
        }
        
        _config.TargetTemperature = temperature;
        UpdateThermostatControl();
        
        return $"Target temperature set to {temperature:F1}°C";
    }
    
    [McpServerTool("configure_thermostat", "Updates thermostat configuration")]
    public static string ConfigureThermostat(ThermostatConfig config)
    {
        if (config.TargetTemperature < 5 || config.TargetTemperature > 35)
        {
            return "Error: Target temperature must be between 5°C and 35°C";
        }
        
        if (config.Mode != "heat" && config.Mode != "cool" && config.Mode != "auto" && config.Mode != "off")
        {
            return "Error: Mode must be 'heat', 'cool', 'auto', or 'off'";
        }
        
        _config = config;
        UpdateThermostatControl();
        
        return $"Thermostat configured: {config.TargetTemperature:F1}°C, Mode: {config.Mode}";
    }
    
    [McpServerTool("get_status", "Gets complete thermostat status", "JSON object with thermostat status")]
    public static ThermostatStatus GetStatus()
    {
        return new ThermostatStatus
        {
            CurrentTemperature = ReadTemperatureSensor(),
            TargetTemperature = _config.TargetTemperature,
            Mode = _config.Mode,
            IsHeating = _isHeating,
            IsCooling = _isCooling,
            Humidity = ReadHumiditySensor(),
            LastUpdate = DateTime.UtcNow
        };
    }
    
    private static float ReadTemperatureSensor()
    {
        // Simulate sensor reading
        return 23.5f + (float)(new Random().NextDouble() - 0.5) * 2;
    }
    
    private static float ReadHumiditySensor()
    {
        // Simulate sensor reading
        return 65f + (float)(new Random().NextDouble() - 0.5) * 10;
    }
    
    private static void UpdateThermostatControl()
    {
        float currentTemp = ReadTemperatureSensor();
        
        switch (_config.Mode.ToLower())
        {
            case "heat":
                _isHeating = currentTemp < _config.TargetTemperature - 0.5f;
                _isCooling = false;
                break;
            case "cool":
                _isHeating = false;
                _isCooling = currentTemp > _config.TargetTemperature + 0.5f;
                break;
            case "auto":
                _isHeating = currentTemp < _config.TargetTemperature - 1.0f;
                _isCooling = currentTemp > _config.TargetTemperature + 1.0f;
                break;
            case "off":
                _isHeating = false;
                _isCooling = false;
                break;
        }
    }
}

public class Program
{
    private const string Ssid = "YourWiFiSSID";
    private const string Password = "YourWiFiPassword";
    
    public static void Main()
    {
        Console.WriteLine("Starting Smart Thermostat MCP Server...");
        
        // Connect to WiFi
        var connected = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true);
        if (!connected)
        {
            Console.WriteLine("Failed to connect to WiFi");
            return;
        }
        
        Console.WriteLine($"Connected to WiFi. IP: {GetCurrentIPAddress()}");
        
        // Register MCP tools
        McpToolRegistry.DiscoverTools(new Type[] { typeof(ThermostatTools) });
        Console.WriteLine("Thermostat tools registered");
        
        // Start MCP server
        using (var server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) }))
        {
            McpServerController.ServerName = "SmartThermostat";
            McpServerController.ServerVersion = "1.0.0";
            McpServerController.Instructions = @"
                Smart Thermostat with the following capabilities:
                - Temperature and humidity monitoring
                - Target temperature control (5°C to 35°C)
                - Operating modes: heat, cool, auto, off
                - Real-time status reporting
                
                All temperatures are in Celsius.
            ";
            
            server.Start();
            Console.WriteLine("Smart Thermostat MCP server is running!");
            Console.WriteLine($"Access via: http://{NetworkHelper.GetLocalIpAddress()}/mcp");
            
            Thread.Sleep(Timeout.Infinite);
        }
    }

    private static string GetCurrentIPAddress()
    {
        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

        // get first NI ( Wifi on ESP32 )
        return ni.IPv4Address.ToString();
    }
}
```

## Client Integration

### .NET MCP Client

The repository includes a [.NET 10 MCP client example](../tests/McpClientTest/) that demonstrates integration with Azure OpenAI:

```csharp
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;

// Connect to nanoFramework MCP server
var mcpClient = await McpClientFactory.CreateAsync(
    new SseClientTransport(new SseClientTransportOptions()
    {
        Endpoint = new Uri("http://192.168.1.100/mcp"), // Your device IP
        TransportMode = HttpTransportMode.StreamableHttp,
    }, new HttpClient()));

// Initialize the connection
await mcpClient.InitializeAsync();

// Discover available tools
var tools = await mcpClient.ListToolsAsync();
Console.WriteLine($"Discovered {tools.Length} tools");

// Create Semantic Kernel and register tools
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4", endpoint, apiKey)
    .Build();

// Register MCP tools as kernel functions
kernel.Plugins.AddFromFunctions("ThermostatTools", 
    tools.Select(tool => tool.AsKernelFunction()));

// Use AI with device tools
var response = await kernel.InvokePromptAsync(
    "What's the current temperature and set it to 24 degrees?");

Console.WriteLine(response);
```

### Python MCP Client

Example Python client using the official MCP SDK:

```python
import asyncio
from mcp import Client
from mcp.client.transport.http import HttpTransport

async def main():
    # Connect to nanoFramework device
    transport = HttpTransport("http://192.168.1.100/mcp")
    client = Client(transport)
    
    # Initialize connection
    await client.connect()
    
    # List available tools
    tools = await client.list_tools()
    print(f"Available tools: {[tool.name for tool in tools]}")
    
    # Call a tool
    result = await client.call_tool("get_temperature", {})
    print(f"Temperature: {result.content[0].text}")
    
    # Configure thermostat
    config_result = await client.call_tool("configure_thermostat", {
        "TargetTemperature": 24.0,
        "Mode": "auto",
        "ScheduleEnabled": True
    })
    print(f"Configuration: {config_result.content[0].text}")

if __name__ == "__main__":
    asyncio.run(main())
```

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Verify WiFi connection
   - Check IP address and port
   - Ensure you are using proper IP and port

2. **Tool Not Found**
   - Verify tool is properly decorated with `[McpServerTool]`
   - Check that the class is included in `DiscoverTools()`
   - Ensure method is public and static (or instance if using instance methods)
   - Ensure you have only 0 or 1 parameter to the function

3. **Authentication Errors**
   - Verify credentials are correctly configured
   - Check authentication headers in requests
   - Ensure correct authentication controller is used

4. **Memory Issues**
   - Monitor device memory usage
   - Consider reducing JSON payload sizes
   - Implement caching for frequently accessed data (this consue memory as well)

### Debug Tips

Enable detailed logging:

```csharp
public class DebuggingTools
{
    [McpServerTool("debug_info", "Gets debugging information")]
    public static string GetDebugInfo()
    {
        var info = new
        {
            FreeMemory = GC.GetTotalMemory(false),
            UpTime = DateTime.UtcNow - _startTime,
            RequestCount = _requestCount,
            LastError = _lastError ?? "None"
        };
        
        return JsonConvert.SerializeObject(info);
    }
    
    private static DateTime _startTime = DateTime.UtcNow;
    private static int _requestCount = 0;
    private static string _lastError = null;
}
```

### Testing with HTTP Tools

Use tools like curl or VS Code REST Client (adjust your local IP address):

```http
### Test tool discovery
POST http://192.168.1.100/mcp
Content-Type: application/json

{
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 1
}

### Test tool invocation
POST http://192.168.1.100/mcp
Content-Type: application/json

{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "get_temperature",
        "arguments": {}
    },
    "id": 2
}
```

### Performance Monitoring

Monitor key metrics:

```csharp
public class PerformanceTools
{
    private static int _totalRequests = 0;
    private static TimeSpan _totalProcessingTime = TimeSpan.Zero;
    
    [McpServerTool("get_performance", "Gets performance metrics")]
    public static string GetPerformanceMetrics()
    {
        var avgProcessingTime = _totalRequests > 0 
            ? _totalProcessingTime.TotalMilliseconds / _totalRequests 
            : 0;
            
        return $"Requests: {_totalRequests}, Avg Time: {avgProcessingTime:F2}ms, Free Memory: {GC.GetTotalMemory(false)} bytes";
    }
}
```

## Related Resources

- [MCP Official Specification](https://github.com/modelcontextprotocol/modelcontextprotocol)
- [WebServer Authentication Guide](./authentication.md)
- [HTTPS Configuration](./https-certificates.md)
- [E2E Test Examples](../tests/McpEndToEndTest/)
- [.NET Client Example](../tests/McpClientTest/)

The Model Context Protocol support in nanoFramework WebServer enables powerful AI-device interactions, making embedded systems accessible to modern AI applications and opening new possibilities for intelligent IoT solutions.
