# AI Agent Skills Discovery

The nanoFramework WebServer provides a lightweight skills discovery service that enables AI agents to discover and invoke capabilities exposed by embedded devices. Skills follow the [A2A (Agent2Agent) protocol](https://a2a-protocol.org) conventions for agent-to-agent discovery, while using an attribute-driven developer experience inspired by Semantic Kernel plugins.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Defining Skills](#defining-skills)
- [Skill Actions](#skill-actions)
- [Tags and Examples](#tags-and-examples)
- [Markdown Document Support](#markdown-document-support)
- [Complex Object Support](#complex-object-support)
- [Server Setup](#server-setup)
- [Authentication Options](#authentication-options)
- [HTTP API Reference](#http-api-reference)
- [Using Alongside MCP](#using-alongside-mcp)
- [Best Practices](#best-practices)
- [Complete Example](#complete-example)

## Overview

A **skill** is a named, described capability that an embedded device advertises to any AI agent or orchestrator. Skills group related actions (invokable functions) and expose metadata — tags, version, input/output contract — so an agent can discover **what the device can do** before deciding **how** to invoke it.

The implementation serves an **A2A-compatible Agent Card** at `/.well-known/agent-card.json`, enabling standard agent discovery.

### Key Features

- **Automatic skill discovery** through reflection and attributes
- **A2A-compatible Agent Card** served at `/.well-known/agent-card.json`
- **Markdown document support** — skills can return `text/markdown` content
- **Tag-based filtering** for semantic matching by orchestrators
- **Example prompts** to help LLMs understand when to invoke a skill
- **Type-safe parameter handling** with automatic deserialization
- **Flexible authentication** options (none, basic auth, API key)
- **Complex object support** for input parameters and return values
- **Memory efficient** implementation optimized for embedded devices
- **Protocol-agnostic** — works standalone or alongside MCP

### Limitations

- **Single parameter limitation**: Actions can have zero or one parameter (use classes for multiple values)
- **Static methods only**: Skill actions must be static methods
- **No streaming**: Responses are returned as complete payloads
- **Discovery only**: The full A2A task lifecycle is not implemented — only discovery and invocation

## Requirements

- **NuGet Package**: `nanoFramework.WebServer.Skills`
- **Network Connectivity**: WiFi, Ethernet, or other network connection
- **Memory**: Sufficient RAM for JSON parsing and reflection

## Installation

Install the `nanoFramework.WebServer.Skills` NuGet package. This includes the core `nanoFramework.WebServer` as a dependency.

## Quick Start

```csharp
using System;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Skills;

[Skill("hello", "Hello Skill", "A simple greeting skill")]
[SkillTag("greeting")]
[SkillExample("Say hello to someone")]
public class HelloSkill
{
    [SkillAction("SayHello", "Returns a greeting message")]
    public static string SayHello(string name)
    {
        return $"Hello, {name}! Greetings from nanoFramework.";
    }
}

public class Program
{
    public static void Main()
    {
        // Discover and register skills
        SkillRegistry.DiscoverSkills(new Type[] { typeof(HelloSkill) });

        // Start server
        using (var server = new WebServer(80, HttpProtocol.Http,
            new Type[] { typeof(SkillDiscoveryController) }))
        {
            server.Start();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
```

Agents can now:
- `GET /.well-known/agent-card.json` — discover available skills
- `GET /skills` — lightweight skills list
- `POST /skills/invoke` — invoke skill actions

## Defining Skills

Use the `[Skill]` attribute on a class to mark it as a discoverable skill. The attribute takes four parameters aligned with the A2A `AgentSkill` schema:

```csharp
[Skill("climate-control", "Climate Control", "HVAC management for building zones", "1.0")]
public class ClimateSkill
{
    // ... actions
}
```

| Parameter | A2A Field | Description |
| --- | --- | --- |
| `id` | `AgentSkill.id` | Unique identifier for the skill (required) |
| `name` | `AgentSkill.name` | Human-readable display name (required) |
| `description` | `AgentSkill.description` | Detailed description of what the skill does (required) |
| `version` | — | Skill version string, defaults to "1.0" |

## Skill Actions

Actions are the invokable functions within a skill. Use the `[SkillAction]` attribute on **static methods**:

```csharp
[Skill("sensors", "Sensors", "Device sensor readings")]
public class SensorSkill
{
    [SkillAction("ReadTemperature", "Reads the current temperature from the sensor")]
    public static double ReadTemperature()
    {
        return TemperatureSensor.Read();
    }

    [SkillAction("SetThreshold", "Sets the alert temperature threshold")]
    public static bool SetThreshold(ThresholdInput input)
    {
        return AlertSystem.SetThreshold(input.Value);
    }
}
```

### Actions with Output Description

Provide output descriptions for better AI understanding:

```csharp
[SkillAction("GetStatus", "Retrieves system status", outputDescription: "JSON object with device metrics")]
public static DeviceStatus GetStatus()
{
    return new DeviceStatus { Uptime = "2d 5h", Memory = "75%" };
}
```

## Tags and Examples

### Tags

Tags enable semantic matching by orchestrators. Use the `[SkillTag]` attribute (one tag per attribute, apply multiple times):

```csharp
[Skill("climate-control", "Climate Control", "HVAC management")]
[SkillTag("temperature")]
[SkillTag("hvac")]
[SkillTag("sensor")]
[SkillTag("indoor")]
public class ClimateSkill { }
```

Tags appear in the A2A `AgentSkill.tags` field and can be used to filter skills via `GET .well-known/agent-card.json?tag=sensor`.

### Examples

Examples help LLMs understand when to invoke a skill. Use the `[SkillExample]` attribute:

```csharp
[Skill("climate-control", "Climate Control", "HVAC management")]
[SkillExample("What is the current temperature?")]
[SkillExample("Set the target temperature to 22 degrees")]
[SkillExample("Show the HVAC system status")]
public class ClimateSkill { }
```

## Markdown Document Support

Skills can return markdown documents via the `contentType` parameter on `[SkillAction]`. This is critical for AI agents that need structured documentation from devices.

```csharp
[SkillAction("GetDocumentation", "Returns setup and calibration guide",
    contentType: "text/markdown")]
public static string GetDocumentation()
{
    return "# Climate Control Setup Guide\n\n" +
           "## Sensor Calibration\n" +
           "1. Place the sensor in a controlled environment...\n" +
           "2. Wait 5 minutes for stabilization...\n\n" +
           "## Configuration\n" +
           "- **Target Range**: 18°C — 28°C\n" +
           "- **Polling Interval**: 30 seconds\n";
}
```

When invoked, the response is returned with `Content-Type: text/markdown` directly (not JSON-wrapped):

```http
HTTP/1.1 200 OK
Content-Type: text/markdown

# Climate Control Setup Guide

## Sensor Calibration
1. Place the sensor in a controlled environment...
2. Wait 5 minutes for stabilization...
```

The `outputModes` field in the A2A Agent Card automatically includes `text/markdown` for skills that have markdown actions.

## Complex Object Support

Use classes for actions that require multiple input parameters:

```csharp
public class DeviceConfig
{
    public string DeviceName
    {
        [Description("Device name identifier")]
        get;
        set;
    }

    public int UpdateInterval
    {
        [Description("Update interval in seconds")]
        get;
        set;
    }
}

[Skill("config", "Configuration", "Device configuration management")]
[SkillTag("configuration")]
public class ConfigSkill
{
    [SkillAction("Configure", "Updates device configuration")]
    public static string Configure(DeviceConfig config)
    {
        ApplyConfig(config);
        return "Configuration applied: " + config.DeviceName;
    }
}
```

Nested objects are also supported:

```csharp
public class SensorConfig
{
    public string SensorId { get; set; }
    public CalibrationSettings Calibration { get; set; }
}

public class CalibrationSettings
{
    public float Offset { get; set; }
    public float Scale { get; set; }
}
```

When invoking, nested objects are passed as JSON strings within the arguments Hashtable.

## Server Setup

### Basic Configuration

```csharp
// Discover skills
SkillRegistry.DiscoverSkills(new Type[]
{
    typeof(ClimateSkill),
    typeof(SensorSkill),
    typeof(ConfigSkill)
});

// Start server
using (var server = new WebServer(80, HttpProtocol.Http,
    new Type[] { typeof(SkillDiscoveryController) }))
{
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

### Custom Agent Card Information

Customize the Agent Card identity:

```csharp
SkillDiscoveryController.AgentName = "SmartThermostat";
SkillDiscoveryController.AgentDescription = "Embedded HVAC controller with sensor capabilities";
SkillDiscoveryController.AgentVersion = "2.1.0";
SkillDiscoveryController.AgentUrl = "http://192.168.1.100";
```

### HTTPS Configuration

For secure communication, configure HTTPS. See the [HTTPS documentation](./https-certificates.md).

```csharp
using (var server = new WebServer(443, HttpProtocol.Https,
    new Type[] { typeof(SkillDiscoveryController) }))
{
    server.HttpsCert = certificate;
    server.SslProtocols = SslProtocols.Tls12;
    server.Start();
}
```

## Authentication Options

### No Authentication (Default)

```csharp
new Type[] { typeof(SkillDiscoveryController) }
```

### Basic Authentication

```csharp
var server = new WebServer(80, HttpProtocol.Http,
    new Type[] { typeof(SkillDiscoveryBasicAuthController) });
server.Credential = new NetworkCredential("admin", "password");
```

### API Key Authentication

```csharp
var server = new WebServer(80, HttpProtocol.Http,
    new Type[] { typeof(SkillDiscoveryApiKeyAuthController) });
server.ApiKey = "your-secret-key";
```

## HTTP API Reference

### Agent Card Discovery

```
GET /.well-known/agent-card.json
```

Returns an A2A-compatible Agent Card:

```json
{
  "name": "SmartThermostat",
  "description": "Embedded HVAC controller",
  "version": "1.0.0",
  "skills": [
    {
      "id": "climate-control",
      "name": "Climate Control",
      "description": "HVAC management for building zones",
      "version": "1.0",
      "tags": ["temperature", "hvac", "sensor"],
      "examples": ["What is the current temperature?"],
      "inputModes": ["application/json", "text/plain"],
      "outputModes": ["application/json", "text/markdown"],
      "actions": [
        {
          "name": "GetTemperature",
          "description": "Reads current temperature"
        },
        {
          "name": "GetDocumentation",
          "description": "Returns setup guide",
          "contentType": "text/markdown"
        }
      ]
    }
  ]
}
```

### Filtering

Filter by skill ID:
```
GET /.well-known/agent-card.json?skill=climate-control
```

Filter by tag:
```
GET /.well-known/agent-card.json?tag=sensor
```

### Skills List (Lightweight)

```
GET /skills
```

Returns just the skills array:
```json
{
  "skills": [ ... ]
}
```

### Invoke an Action

```
POST /skills/invoke
Content-Type: application/json

{
  "skill": "climate-control",
  "action": "GetTemperature",
  "arguments": {}
}
```

JSON response:
```json
{
  "result": "22.5"
}
```

Markdown response (when action has `contentType: "text/markdown"`):
```http
HTTP/1.1 200 OK
Content-Type: text/markdown

# Climate Control Setup Guide
...
```

### Error Responses

```json
{ "error": { "code": -1, "message": "Error description" } }
{ "error": { "code": -2, "message": "Skill or action not found" } }
{ "error": { "code": -3, "message": "Missing 'skill' or 'action' field" } }
```

## Using Alongside MCP

Skills and MCP are independent, peer packages. Both can be active simultaneously:

```csharp
// Discover both
McpToolRegistry.DiscoverTools(new Type[] { typeof(MyMcpTools) });
SkillRegistry.DiscoverSkills(new Type[] { typeof(ClimateSkill) });

// Register both controllers
using (var server = new WebServer(80, HttpProtocol.Http,
    new Type[] { typeof(McpServerController), typeof(SkillDiscoveryController) }))
{
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
// MCP clients use POST /mcp
// A2A agents use GET /.well-known/agent-card.json
```

## Best Practices

1. **Keep markdown documents concise** — embedded devices have limited RAM. Aim for under 4KB per markdown response.
2. **Use meaningful tags** — tags are the primary mechanism for agents to match skills. Use domain-specific keywords.
3. **Provide examples** — LLMs use examples to understand when to invoke a skill. Include 2-3 representative prompts.
4. **Use descriptive action names** — action names should clearly indicate what they do (e.g., `GetTemperature`, not `Read`).
5. **Group related actions** — a skill should represent a coherent capability, not a single function.
6. **Use `[Description]` on properties** — property descriptions appear in the JSON schema and help AI agents understand parameter structure.
7. **One parameter per action** — use a class to wrap multiple values into a single parameter.
8. **Static methods only** — skill actions must be static methods to avoid instantiation overhead.

## Complete Example

```csharp
using System;
using System.Threading;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Skills;

// Define a skill with full A2A-compatible metadata
[Skill("climate-control", "Climate Control",
    "HVAC management for building zones", "1.0")]
[SkillTag("temperature")]
[SkillTag("hvac")]
[SkillTag("sensor")]
[SkillExample("What is the current temperature?")]
[SkillExample("Set the target temperature to 22 degrees")]
public class ClimateSkill
{
    [SkillAction("GetTemperature", "Reads current room temperature")]
    public static double GetTemperature()
    {
        return TemperatureSensor.Read();
    }

    [SkillAction("SetTargetTemp", "Sets the target temperature")]
    public static bool SetTargetTemp(TargetTempInput input)
    {
        return HvacController.SetTarget(input.Temperature);
    }

    [SkillAction("GetStatus", "Returns HVAC system status",
        outputDescription: "HVAC status with temperature and mode")]
    public static HvacStatus GetStatus()
    {
        return new HvacStatus
        {
            CurrentTemp = TemperatureSensor.Read(),
            TargetTemp = HvacController.GetTarget(),
            Mode = HvacController.GetMode()
        };
    }

    [SkillAction("GetDocumentation",
        "Returns setup and calibration guide",
        contentType: "text/markdown")]
    public static string GetDocumentation()
    {
        return "# Climate Control Setup Guide\n\n" +
               "## Sensor Calibration\n" +
               "1. Place the sensor in a controlled environment...\n" +
               "2. Wait 5 minutes for stabilization...\n\n" +
               "## Configuration\n" +
               "- **Target Range**: 18°C — 28°C\n" +
               "- **Polling Interval**: 30 seconds\n";
    }
}

public class TargetTempInput
{
    public double Temperature
    {
        [Description("Target temperature in Celsius")]
        get;
        set;
    }
}

public class HvacStatus
{
    public double CurrentTemp { get; set; }
    public double TargetTemp { get; set; }
    public string Mode { get; set; }
}

public class Program
{
    public static void Main()
    {
        // Connect to WiFi
        // WifiNetworkHelper.ConnectDhcp(ssid, password);

        // Discover skills
        SkillRegistry.DiscoverSkills(new Type[] { typeof(ClimateSkill) });

        // Configure Agent Card
        SkillDiscoveryController.AgentName = "SmartThermostat";
        SkillDiscoveryController.AgentDescription =
            "Embedded HVAC controller with sensor capabilities";
        SkillDiscoveryController.AgentVersion = "1.0.0";

        // Start server
        using (var server = new WebServer(80, HttpProtocol.Http,
            new Type[] { typeof(SkillDiscoveryController) }))
        {
            server.Start();
            Console.WriteLine("Skills discovery server running on port 80");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
```
