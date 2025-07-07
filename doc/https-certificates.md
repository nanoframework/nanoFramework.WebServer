# HTTPS and Certificates

This guide covers how to configure HTTPS/SSL encryption with certificates in nanoFramework WebServer.

## Overview

The WebServer supports HTTPS connections using X.509 certificates for encrypted communication. This is essential for production deployments, especially when using authentication or transmitting sensitive data.

## Certificate Generation

This section will give you an overview of basic certificate generation and how to use those with nanoFramework.

### Using OpenSSL

You can use `openssl` to generate a self-signed certificate for testing. Download on different platforms is [available here](https://github.com/openssl/openssl?tab=readme-ov-file#download).

```bash
# Generate private key and certificate
openssl req -newkey rsa:2048 -nodes -keyout server.key -x509 -days 365 -out server.crt

# Create password-protected private key
openssl rsa -des3 -in server.key -out server-encrypted.key
# Enter password when prompted (e.g., "1234")
```

### Certificate Files

You'll get two files:
- `server.crt` - The certificate (public key)
- `server-encrypted.key` - The encrypted private key

## Basic HTTPS Setup

With the generated certificates in the previous step:

```csharp
public static void Main()
{
    // Certificate as string constants
    const string serverCrt = @"-----BEGIN CERTIFICATE-----
MIIDXTCCAkWgAwIBAgIJAKL0UG+mRnNjMA0GCSqGSIb3DQEBCwUAMEUxCzAJBgNV
BAYTAkFVMRMwEQYDVQQIDApTb21lLVN0YXRlMSEwHwYDVQQKDBhJbnRlcm5ldCBX
... (rest of certificate)
-----END CERTIFICATE-----";

    const string serverKey = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
DEK-Info: DES-EDE3-CBC,2B9FDBE5B2B6AD34

WgJz8pSS8RQUFNjPsrG/s2pGYCJ5FghVnS5s6H+mDfY0+XqJdBcm0I2WZLy/
... (rest of encrypted private key)
-----END RSA PRIVATE KEY-----";

    // Create certificate object
    X509Certificate2 certificate = new X509Certificate2(
        Encoding.UTF8.GetBytes(serverCrt), 
        Encoding.UTF8.GetBytes(serverKey), 
        "1234"  // Password used to encrypt the private key
    );

    // Create HTTPS server
    using (WebServer server = new WebServer(443, HttpProtocol.Https, new Type[] { typeof(MyController) }))
    {
        server.HttpsCert = certificate;
        server.SslProtocols = SslProtocols.Tls12;
        
        server.Start();
        Thread.Sleep(Timeout.Infinite);
    }
}
```

## Complete HTTPS Example

```csharp
public class SecureController
{
    [Route("secure/data")]
    [Authentication("Basic")]
    public void GetSecureData(WebServerEventArgs e)
    {
        var data = new 
        { 
            message = "Secure data over HTTPS", 
            timestamp = DateTime.UtcNow,
            encrypted = true
        };
        
        string json = JsonConvert.SerializeObject(data);
        e.Context.Response.ContentType = "application/json";
        WebServer.OutPutStream(e.Context.Response, json);
    }
}

public static void Main()
{
    // Connect to WiFi
    var connected = WifiNetworkHelper.ConnectDhcp(ssid, password, requiresDateTime: true);
    if (!connected) return;

    // Load certificate
    X509Certificate2 cert = LoadCertificate();

    using (WebServer server = new WebServer(443, HttpProtocol.Https, new Type[] { typeof(SecureController) }))
    {
        // Configure HTTPS
        server.HttpsCert = cert;
        server.SslProtocols = SslProtocols.Tls12;
        
        // Configure authentication
        server.Credential = new NetworkCredential("admin", "securepass");
        
        server.Start();
        Debug.WriteLine("HTTPS server started on port 443");
        Thread.Sleep(Timeout.Infinite);
    }
}

private static X509Certificate2 LoadCertificate()
{
    // Certificate content (replace with your actual certificate)
    const string certContent = @"-----BEGIN CERTIFICATE-----
... your certificate content here ...
-----END CERTIFICATE-----";

    const string keyContent = @"-----BEGIN RSA PRIVATE KEY-----
... your encrypted private key here ...
-----END RSA PRIVATE KEY-----";

    return new X509Certificate2(
        Encoding.UTF8.GetBytes(certContent),
        Encoding.UTF8.GetBytes(keyContent),
        "your-private-key-password"
    );
}
```

## SSL/TLS Protocol Configuration

This section goes through different options for the SSL configuration.

### TLS 1.2 Only (Minimum Recommended)

```csharp
server.SslProtocols = SslProtocols.Tls12;
```

### Multiple TLS Versions

```csharp
server.SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
```

### Available Options

- `SslProtocols.Tls` - TLS 1.0
- `SslProtocols.Tls11` - TLS 1.1  
- `SslProtocols.Tls12` - TLS 1.2 (minimum recommended)
- `SslProtocols.Tls13` - TLS 1.3

## Certificate Storage Options

You have 4 diufferent ways to store certificates and use them:

- **Embed in code**: this is not really recommended for production as you cannot easilly replace the certificate except by flashing the device with a new code.
- **From resources**: similar as for the in code, this solution won't allow you to easilly replace the certificate.
- **From Visual Studio**: in the .NET nanoFramework extension, select your device, then `Edit Network Configuration`, then `General`, you can upload root CA and device CA.
- **From storage**: typically added in the (typically internal) storage at setup time or flash time, this is suitable for production as can also be replaced more easilly.
- **At flash time**: during the initial flash process, this can be deployed as well as the wifi credential. Also suitable for production and also easier to flash later on.

### Embedded in Code

```csharp
public static class Certificates
{
    public const string ServerCertificate = @"-----BEGIN CERTIFICATE-----
MIIDXTCCAkWgAwIBAgIJAKL0UG+mRnNjMA0GCSqGSIb3DQEBCwUAMEUxCzAJBgNV
... certificate content ...
-----END CERTIFICATE-----";

    public const string ServerPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
Proc-Type: 4,ENCRYPTED
... private key content ...
-----END RSA PRIVATE KEY-----";
}
```

> [!Note] You can upload mutliple certificates at once, add them one after the other one, in the same file/string each properly separated by the `BEGIN CERTIFICATE` and `END CERTIFICATE` markers.

### From Resources

```csharp
// Add certificate files as embedded resources in your project
public static X509Certificate2 LoadFromResources()
{
    var certBytes = Resources.GetBytes(Resources.BinaryResources.server_crt);
    var keyBytes = Resources.GetBytes(Resources.BinaryResources.server_key);
    
    return new X509Certificate2(certBytes, keyBytes, "password");
}
```

### From File System

```csharp
// If your device supports file system
public static X509Certificate2 LoadFromFiles()
{
    var certBytes = File.ReadAllBytes("I:\\certificates\\server.crt");
    var keyBytes = File.ReadAllBytes("I:\\certificates\\server.key");
    
    return new X509Certificate2(certBytes, keyBytes, "password");
}
```

### Uploading them in the device at flash time

You can upload the certificates directly into the device by following the steps described in the [naoff documentation](https://github.com/nanoframework/nanoFirmwareFlasher?tab=readme-ov-file#deploy-wireless-wireless-access-point-ethernet-configuration-and-certificates).

Note that the same method can be used as well to deploy the certificated into the internal storage as well.

## Production Considerations

There are couple of elements to consider when creating a certificate and embedding it into your nanoFramework device.

### Valid Certificate Authority

For production, use certificates from a trusted Certificate Authority (CA):

1. **Let's Encrypt** - Free certificates, typically used for hobbyist usage which still want some security
2. **Commercial CAs** - Paid certificates, typically used for a device you'll sell
3. **Internal CA** - For enterprise environments (commercial CA can also be used in enterprise environments)

### Certificate Installation

Because self-signed certificates aren't trusted by browsers, you may need to:

1. **Install certificate** in browser/OS certificate store
2. **Add security exception** in browser
3. **Use proper CA-signed certificate** for production

### Windows Certificate Installation

1. Save the certificate as `server.crt`
2. Double-click the file
3. Click "Install Certificate..."
4. Choose "Local Machine" or "Current User"
5. Select "Trusted Root Certification Authorities"
6. Complete the installation

## MCP with HTTPS

The Model Context Procol server also works with HTTPS.

```csharp
public static void Main()
{
    X509Certificate2 cert = LoadCertificate();
    
    // Discover MCP tools
    McpToolRegistry.DiscoverTools(new Type[] { typeof(IoTTools) });

    using (var server = new WebServer(443, HttpProtocol.Https, new Type[] { typeof(McpServerController) }))
    {
        // Configure HTTPS
        server.HttpsCert = cert;
        server.SslProtocols = SslProtocols.Tls12;
        
        // Configure MCP
        McpServerController.ServerName = "SecureIoTDevice";
        McpServerController.Instructions = "Secure IoT device accessible via HTTPS only";
        
        server.Start();
        Debug.WriteLine("Secure MCP server started");
        Thread.Sleep(Timeout.Infinite);
    }
}
```

## Testing HTTPS

Here are several ways to test your HTTPS endpoints securely and effectively:

### Using REST Client VS Code Extension

The [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension for VS Code provides an excellent way to test your HTTPS endpoints directly from your development environment.

Create a `.http` file in your project:

```http
### Test basic HTTPS endpoint
GET https://192.168.1.100/api/status
Accept: application/json

### Test with authentication
GET https://192.168.1.100/secure/data
Authorization: Basic YWRtaW46cGFzc3dvcmQ=
Accept: application/json

### Test MCP over HTTPS
POST https://192.168.1.100/mcp
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "method": "tools/list",
  "id": 1
}

### Test file upload
POST https://192.168.1.100/api/upload
Content-Type: multipart/form-data; boundary=boundary

--boundary
Content-Disposition: form-data; name="file"; filename="test.txt"
Content-Type: text/plain

Hello World!
--boundary--
```

**For Self-Signed Certificates**: Add these settings to your VS Code `settings.json`:

```json
{
  "rest-client.enableTelemetry": false,
  "rest-client.environmentVariables": {
    "$shared": {
      "host": "192.168.1.100"
    }
  },
  "http.proxyStrictSSL": false
}
```

### Using curl

#### Secure Testing (Recommended)

For production environments, always verify certificates properly:

```bash
# Test with proper certificate verification
curl --cacert ca-certificate.pem https://192.168.1.100/api/status

# If using a certificate authority bundle
curl --capath /etc/ssl/certs https://192.168.1.100/api/status

# Test with client certificate authentication
curl --cert client.pem --key client-key.pem https://192.168.1.100/secure/data

# Test with basic authentication and proper certificate verification
curl --cacert ca-certificate.pem -u admin:password https://192.168.1.100/secure/data

# MCP over HTTPS with certificate verification
curl --cacert ca-certificate.pem -X POST https://192.168.1.100/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"tools/list","id":1}'
```

#### Development/Testing Only

**Warning**: Only use these commands during development with self-signed certificates. Never use `-k` in production:

```bash
# DEVELOPMENT ONLY: Ignore certificate verification (NOT for production)
curl -k https://192.168.1.100/api/status

# DEVELOPMENT ONLY: With authentication, ignoring certificate errors
curl -k -u admin:password https://192.168.1.100/secure/data

# DEVELOPMENT ONLY: MCP testing with certificate verification disabled
curl -k -X POST https://192.168.1.100/mcp \
  -H "Content-Type: application/json" \
  -d '{"jsonrpc":"2.0","method":"tools/list","id":1}'
```

#### Adding CA Certificate for Verification

To properly verify your self-signed or custom certificates:

**On Windows (PowerShell)**:

```powershell
# Add to Windows certificate store
Import-Certificate -FilePath "ca-certificate.crt" -CertStoreLocation "Cert:\LocalMachine\Root"

# Test after adding to store
curl https://192.168.1.100/api/status
```

**On Linux**:

```bash
# Copy CA certificate to system store
sudo cp ca-certificate.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates

# Test with system certificates
curl https://192.168.1.100/api/status
```

**On macOS**:

```bash
# Add to system keychain
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain ca-certificate.crt

# Test with system certificates
curl https://192.168.1.100/api/status
```

### Using Browser Developer Tools

Modern browsers provide excellent tools for testing HTTPS endpoints:

1. **Network Tab**: Monitor request/response details, timing, and certificate information
2. **Security Tab**: View certificate details and validation status
3. **Console**: Use `fetch()` API for programmatic testing

```javascript
// Test API endpoint from browser console
fetch('https://192.168.1.100/api/status')
  .then(response => response.json())
  .then(data => console.log(data))
  .catch(error => console.error('Error:', error));

// Test with authentication
fetch('https://192.168.1.100/secure/data', {
  headers: {
    'Authorization': 'Basic ' + btoa('admin:password')
  }
})
  .then(response => response.json())
  .then(data => console.log(data));
```

## Troubleshooting

### Common Issues

1. **Certificate not trusted**: Install certificate in browser/OS
2. **Wrong private key password**: Verify the password used during key encryption
3. **Certificate format**: Ensure PEM format with proper headers
4. **Port conflicts**: Ensure port 443 is available
5. **Memory issues**: HTTPS uses more memory than HTTP
