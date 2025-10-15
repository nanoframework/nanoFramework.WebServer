# Authentication

This guide covers the authentication options available in nanoFramework WebServer.

## Overview

The WebServer supports three types of authentication that can be applied to controllers and individual methods:

1. **Basic Authentication** - HTTP Basic Auth with username/password
2. **API Key Authentication** - Custom header-based authentication
3. **None** - No authentication required

Authentication can be configured at both the class level (applies to all methods) and method level (overrides class-level settings).

## Basic Authentication

### Default Credentials

```csharp
[Authentication("Basic")]
public class SecureController
{
    [Route("secure/data")]
    public void GetSecureData(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Secure data");
    }
}

// Server setup with default credentials
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(SecureController) }))
{
    server.Credential = new NetworkCredential("admin", "password");
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

### Custom Credentials

```csharp
public class UserController
{
    [Route("admin")]
    [Authentication("Basic:admin secretpassword")]
    public void AdminPanel(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Admin panel");
    }

    [Route("user")]
    [Authentication("Basic:user userpass")]
    public void UserPanel(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "User panel");
    }
}
```

**Note**: The username cannot contain spaces. Use the format: `"Basic:username password"`

## API Key Authentication

### Default API Key

```csharp
[Authentication("ApiKey")]
public class ApiController
{
    [Route("api/data")]
    public void GetData(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "API data");
    }
}

// Server setup with default API key
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ApiController) }))
{
    server.ApiKey = "MySecretApiKey123";
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

### Custom API Key

```csharp
public class ServiceController
{
    [Route("service/premium")]
    [Authentication("ApiKey:premium-key-789")]
    public void PremiumService(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Premium service");
    }

    [Route("service/basic")]
    [Authentication("ApiKey:basic-key-456")]
    public void BasicService(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Basic service");
    }
}
```

### API Key Usage

Clients must include the API key in the request headers:

```http
GET /api/data HTTP/1.1
Host: 192.168.1.100
ApiKey: MySecretApiKey123
```

**Note**: The header name `ApiKey` is case-sensitive.

## No Authentication

```csharp
[Authentication("None")]
public class PublicController
{
    [Route("public/info")]
    public void GetPublicInfo(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Public information");
    }
}
```

## Mixed Authentication Example

```csharp
[Authentication("Basic")]  // Default for all methods in this class
public class MixedController
{
    [Route("secure/basic")]
    public void BasicAuth(WebServerEventArgs e)
    {
        // Uses class-level Basic authentication
        WebServer.OutputAsStream(e.Context.Response, "Basic auth data");
    }

    [Route("secure/api")]
    [Authentication("ApiKey:special-key-123")]  // Override with API key
    public void ApiKeyAuth(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "API key data");
    }

    [Route("secure/custom")]
    [Authentication("Basic:customuser custompass")]  // Override with custom basic auth
    public void CustomAuth(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Custom auth data");
    }

    [Route("public")]
    [Authentication("None")]  // Override to allow public access
    public void PublicAccess(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Public data");
    }
}
```

## Multiple Authentication for Same Route

The WebServer supports multiple authentication methods for the same route by creating separate methods:

```csharp
public class MultiAuthController
{
    [Route("data")]
    [Authentication("Basic")]
    public void DataBasicAuth(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Data via Basic auth");
    }

    [Route("data")]
    [Authentication("ApiKey:key1")]
    public void DataApiKey1(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Data via API key 1");
    }

    [Route("data")]
    [Authentication("ApiKey:key2")]
    public void DataApiKey2(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Data via API key 2");
    }

    [Route("data")]
    public void DataPublic(WebServerEventArgs e)
    {
        WebServer.OutputAsStream(e.Context.Response, "Public data");
    }
}
```

## Authentication Flow

The server selects the route for a request using this logic:

1. **No matching methods**: Returns 404 (Not Found)
2. **Authentication provided in request headers**:
   - Finds methods requiring authentication
   - If credentials match: Calls the matching method
   - If credentials don't match: Returns 401 (Unauthorized)
3. **No authentication provided**:
   - If a public method exists (no auth required): Calls that method
   - If only auth-required methods exist: Returns 401 (Unauthorized)
   - For Basic auth methods: Includes `WWW-Authenticate` header

## Server Configuration

```csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(MyController) }))
{
    // Set default credentials for Basic authentication
    server.Credential = new NetworkCredential("defaultuser", "defaultpass");
    
    // Set default API key
    server.ApiKey = "DefaultApiKey123";
    
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

## Security Best Practices

1. **Use HTTPS**: Always use HTTPS in production to protect credentials
2. **Strong Credentials**: Use strong passwords and API keys
3. **Rotate Keys**: Regularly rotate API keys and passwords
4. **Principle of Least Privilege**: Grant minimal necessary access
5. **Secure Storage**: Avoid hardcoding credentials in source code

## HTTPS with Authentication

```csharp
// Load certificate
X509Certificate2 cert = new X509Certificate2(certBytes, privateKeyBytes, "password");

using (WebServer server = new WebServer(443, HttpProtocol.Https, new Type[] { typeof(SecureController) }))
{
    server.HttpsCert = cert;
    server.SslProtocols = SslProtocols.Tls12;
    server.Credential = new NetworkCredential("admin", "securepassword");
    server.ApiKey = "SecureApiKey456";
    
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}
```

## Testing Authentication

### Basic Auth with curl

```bash
# With default credentials
curl -u admin:password http://192.168.1.100/secure/data

# With custom credentials
curl -u customuser:custompass http://192.168.1.100/secure/custom
```

### API Key with curl

```bash
# With API key
curl -H "ApiKey: MySecretApiKey123" http://192.168.1.100/api/data

# With custom API key
curl -H "ApiKey: special-key-123" http://192.168.1.100/secure/api
```

## Error Responses

### 401 Unauthorized (Basic Auth)

```http
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Basic realm="nanoFramework"
Content-Length: 0
```

### 401 Unauthorized (API Key)

```http
HTTP/1.1 401 Unauthorized
Content-Length: 0
```

### 500 Internal Server Error (Multiple Methods)

When multiple methods match the same route with conflicting authentication, the server returns 500 with details about the conflicting methods.

## Troubleshooting

1. **Always 401**: Check if method requires authentication and credentials are provided
2. **Wrong credentials**: Verify username/password or API key matches configuration
3. **Case sensitivity**: API key header name is case-sensitive (`ApiKey`)
4. **Multiple methods**: Ensure no conflicting methods for the same route/auth combination
5. **Default vs custom**: Remember that method-level attributes override class-level ones
