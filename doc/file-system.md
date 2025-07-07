# File System Support

The nanoFramework WebServer provides comprehensive support for serving static files from various storage devices including SD cards, USB storage, and internal flash storage.

## Overview

File system support is provided through the [`nanoFramework.WebServer.FileSystem`](https://www.nuget.org/packages/nanoFramework.WebServer.FileSystem/) NuGet package, which enables your WebServer to:

- **Serve static files** from any mounted storage device
- **Automatic MIME type detection** based on file extensions
- **Efficient file streaming** with chunked transfer for large files
- **Support multiple storage types** (SD Card, USB, Internal Storage)
- **Memory-efficient serving** with configurable buffer sizes

## Requirements

- **NuGet Package**: `nanoFramework.WebServer.FileSystem`
- **Device Capability**: Target device must support `System.IO.FileSystem`
- **Storage Device**: SD Card, USB storage, or internal flash storage

## Storage Types and Paths

Different storage devices are mounted with specific drive letters:

| Storage Type | Drive Letter | Example Path | Notes |
|--------------|--------------|--------------|-------|
| Internal Storage | `I:\` | `I:\webpage.html` | Built-in flash storage |
| SD Card | `D:\` | `D:\images\logo.png` | Requires SD card mounting |
| USB Storage | `E:\` | `E:\documents\file.pdf` | USB mass storage devices |

## Basic File Serving

### Event-Driven Approach

```csharp
using (WebServer server = new WebServer(80, HttpProtocol.Http))
{
    server.CommandReceived += ServerCommandReceived;
    server.Start();
    Thread.Sleep(Timeout.Infinite);
}

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    const string DirectoryPath = "I:\\"; // Internal storage
    var url = e.Context.Request.RawUrl;
    var fileName = url.Substring(1); // Remove leading '/'

    // Check if file exists and serve it
    string filePath = DirectoryPath + fileName;
    if (File.Exists(filePath))
    {
        WebServer.SendFileOverHTTP(e.Context.Response, filePath);
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

### Controller-Based Approach

```csharp
public class FileController
{
    private const string StoragePath = "D:\\"; // SD Card storage

    [Route("files")]
    [Method("GET")]
    public void ServeFiles(WebServerEventArgs e)
    {
        var url = e.Context.Request.RawUrl;
        var fileName = url.Substring("/files/".Length);
        
        string filePath = StoragePath + fileName;
        if (File.Exists(filePath))
        {
            WebServer.SendFileOverHTTP(e.Context.Response, filePath);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }
}
```

## Advanced File Serving

### Directory Listing and File Discovery

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    const string DirectoryPath = "I:\\";
    var url = e.Context.Request.RawUrl;
    
    // Get list of all files in directory
    string[] fileList = Directory.GetFiles(DirectoryPath);
    
    // Remove directory path from file names for comparison
    for (int i = 0; i < fileList.Length; i++)
    {
        fileList[i] = fileList[i].Substring(DirectoryPath.Length);
    }
    
    var requestedFile = url.Substring(1); // Remove leading '/'
    
    // Search for the requested file (case-sensitive)
    foreach (var file in fileList)
    {
        if (file == requestedFile)
        {
            WebServer.SendFileOverHTTP(e.Context.Response, DirectoryPath + file);
            return;
        }
    }
    
    // File not found
    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
}
```

### Serving Files from Memory

You can also serve file content directly from memory without physical files:

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var url = e.Context.Request.RawUrl;
    
    if (url == "/dynamic.txt")
    {
        string content = $"Generated at: {DateTime.UtcNow}";
        byte[] contentBytes = Encoding.UTF8.GetBytes(content);
        
        WebServer.SendFileOverHTTP(e.Context.Response, "dynamic.txt", contentBytes, "text/plain");
    }
    else if (url == "/config.json")
    {
        string jsonContent = "{\"server\":\"nanoFramework\",\"version\":\"1.0\"}";
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonContent);
        
        WebServer.SendFileOverHTTP(e.Context.Response, "config.json", jsonBytes);
    }
}
```

## MIME Type Detection

The WebServer automatically detects MIME types based on file extensions:

| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.html`, `.htm` | `text/html` | HTML pages |
| `.txt`, `.cs`, `.csproj` | `text/plain` | Plain text files |
| `.css` | `text/css` | Stylesheets |
| `.js` | `application/javascript` | JavaScript files |
| `.json` | `application/json` | JSON data |
| `.pdf` | `application/pdf` | PDF documents |
| `.zip` | `application/zip` | ZIP archives |
| `.jpg`, `.jpeg` | `image/jpeg` | JPEG images |
| `.png` | `image/png` | PNG images |
| `.gif` | `image/gif` | GIF images |
| `.bmp` | `image/bmp` | Bitmap images |
| `.ico` | `image/x-icon` | Icon files |
| `.mp3` | `audio/mpeg` | MP3 audio |

### Custom MIME Types

You can specify custom MIME types when serving files:

```csharp
// Serve with custom MIME type
WebServer.SendFileOverHTTP(e.Context.Response, filePath, "application/custom");

// Serve from memory with custom MIME type
WebServer.SendFileOverHTTP(e.Context.Response, "data.bin", binaryData, "application/octet-stream");
```

## SD Card Setup

To serve files from an SD Card, you need to mount it first:

```csharp
// Mount SD card (device-specific implementation)
// See: https://github.com/nanoframework/Samples/blob/main/samples/System.IO.FileSystem/MountExample/Program.cs

public static void Main()
{
    // Mount SD card to D: drive
    // This is device-specific - check your device documentation
    
    // Start WebServer after SD card is mounted
    using (WebServer server = new WebServer(80, HttpProtocol.Http))
    {
        server.CommandReceived += ServerCommandReceived;
        server.Start();
        Thread.Sleep(Timeout.Infinite);
    }
}

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    const string SdCardPath = "D:\\"; // SD Card mount point
    var fileName = e.Context.Request.RawUrl.Substring(1);
    
    string filePath = SdCardPath + fileName;
    if (File.Exists(filePath))
    {
        WebServer.SendFileOverHTTP(e.Context.Response, filePath);
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

## Performance Considerations

### Buffer Size

The WebServer uses an internal buffer for file streaming. For large files, the content is sent in chunks to manage memory efficiently.

### File Caching

For frequently accessed small files, consider loading them into memory at startup (note that is is consume memory, you should only do this on boards where you have enough memory):

```csharp
public class CachedFileServer
{
    private static readonly Hashtable _fileCache = new Hashtable();
    
    static CachedFileServer()
    {
        // Cache frequently accessed files
        CacheFile("index.html", "I:\\index.html");
        CacheFile("style.css", "I:\\style.css");
        CacheFile("script.js", "I:\\script.js");
    }
    
    private static void CacheFile(string name, string path)
    {
        if (File.Exists(path))
        {
            byte[] content = File.ReadAllBytes(path);
            _fileCache[name] = content;
        }
    }
    
    public static void ServeFile(WebServerEventArgs e, string fileName)
    {
        if (_fileCache.Contains(fileName))
        {
            byte[] content = (byte[])_fileCache[fileName];
            WebServer.SendFileOverHTTP(e.Context.Response, fileName, content);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }
}
```

## Security Considerations

Here are some important security considerations.

### Path Traversal Protection

If you are sticked to a specific directory, you may want to validate file paths to prevent directory traversal attacks:

```csharp
private static bool IsValidPath(string fileName)
{
    // Reject paths with directory traversal attempts
    if (fileName.Contains("..") || fileName.Contains("\\") || fileName.Contains("/"))
    {
        return false;
    }
    
    // Only allow alphanumeric characters, dots, and hyphens
    foreach (char c in fileName)
    {
        if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_')
        {
            return false;
        }
    }
    
    return true;
}

private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var fileName = e.Context.Request.RawUrl.Substring(1);
    
    if (!IsValidPath(fileName))
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
        return;
    }
    
    // Proceed with file serving
    string filePath = "I:\\yourdirectory\\" + fileName;
    if (File.Exists(filePath))
    {
        WebServer.SendFileOverHTTP(e.Context.Response, filePath);
    }
    else
    {
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

### File Access Control

Implement authentication (here implemented with basic, [more information on authentication](./authentication.md)) for sensitive files:

```csharp
[Route("secure")]
[Authentication("Basic")]
public class SecureFileController
{
    [Route("documents")]
    [Method("GET")]
    public void ServeSecureFiles(WebServerEventArgs e)
    {
        // Only authenticated users can access these files
        var fileName = e.Context.Request.QueryString["file"];
        string filePath = "I:\\secure\\" + fileName;
        
        if (File.Exists(filePath))
        {
            WebServer.SendFileOverHTTP(e.Context.Response, filePath);
        }
        else
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }
}
```

## Complete Example

Here's a complete file server implementation (you'll have to add the nanoFramework `System.Device.Wifi` nuget):

```csharp
using System;
using System.IO;
using System.Net;
using System.Threading;
using nanoFramework.Networking;
using nanoFramework.WebServer;

public class Program
{
    private const string StoragePath = "I:\\";
    
    public static void Main()
    {
        // This connects to the wifi
        var res = WifiNetworkHelper.ConnectDhcp("YourSsid", "YourPassword", requiresDateTime: true, token: new CancellationTokenSource(60_000).Token);
        if (!res)
        {
            Debug.WriteLine("Impossible to connect to wifi, most likely invalid credentials");
            return;
        }

        Debug.WriteLine($"Connected with wifi credentials. IP Address: {GetCurrentIPAddress()}");

        // Initialize storage and create sample files
        InitializeStorage();
        
        using (WebServer server = new WebServer(80, HttpProtocol.Http))
        {
            server.CommandReceived += ServerCommandReceived;
            server.Start();
                        
            Console.WriteLine($"Serving files from: {StoragePath}");
            
            Thread.Sleep(Timeout.Infinite);
        }
    }
    
    private static void InitializeStorage()
    {
        // Create sample files
        if (!File.Exists(StoragePath + "index.html"))
        {
            string html = @"<!DOCTYPE html>
<html>
<head><title>nanoFramework File Server</title></head>
<body>
    <h1>Welcome to nanoFramework File Server</h1>
    <ul>
        <li><a href='/sample.txt'>Download sample.txt</a></li>
        <li><a href='/data.json'>Download data.json</a></li>
    </ul>
</body>
</html>";
            File.WriteAllText(StoragePath + "index.html", html);
        }
        
        if (!File.Exists(StoragePath + "sample.txt"))
        {
            File.WriteAllText(StoragePath + "sample.txt", "Hello from nanoFramework!");
        }
        
        if (!File.Exists(StoragePath + "data.json"))
        {
            string json = "{\"message\":\"Hello\",\"timestamp\":\"" + DateTime.UtcNow.ToString() + "\"}";
            File.WriteAllText(StoragePath + "data.json", json);
        }
    }
    
    private static void ServerCommandReceived(object source, WebServerEventArgs e)
    {
        var url = e.Context.Request.RawUrl;
        var fileName = url == "/" ? "index.html" : url.Substring(1);
        
        // Validate file name for security
        if (!IsValidFileName(fileName))
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
            return;
        }
        
        string filePath = StoragePath + fileName;
        
        if (File.Exists(filePath))
        {
            Console.WriteLine($"Serving file: {fileName}");
            WebServer.SendFileOverHTTP(e.Context.Response, filePath);
        }
        else
        {
            Console.WriteLine($"File not found: {fileName}");
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }
    }
    
    private static bool IsValidFileName(string fileName)
    {
        return !string.IsNullOrEmpty(fileName) && 
               !fileName.Contains("..") && 
               !fileName.Contains("\\") && 
               !fileName.StartsWith("/");
    }

    private static string GetCurrentIPAddress()
    {
        NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

        // get first NI ( Wifi on ESP32 )
        return ni.IPv4Address.ToString();
    }
}
```

## Troubleshooting

This section will give you couple of tips and tricks to help you find potential issues. Few things to keep in mind:

- `Console.WriteLine` will always display the message in the output
- `Debug.WriteLine` will only display it when debug is enabled

If you are trying to understand what's happening in release mode, use `Console.WriteLine` and connect to the com port at the 921600 speed.

### Common Issues

1. **File Not Found**: Ensure the file path is correct and the file exists
2. **Permission Denied**: Check file system permissions and device capabilities
3. **Memory Issues**: Use file streaming for large files instead of loading into memory
4. **SD Card Not Mounted**: Verify SD card mounting before serving files

### Debug Tips

```csharp
private static void ServerCommandReceived(object source, WebServerEventArgs e)
{
    var fileName = e.Context.Request.RawUrl.Substring(1);
    string filePath = StoragePath + fileName;
    
    Console.WriteLine($"Requested file: {fileName}");
    Console.WriteLine($"Full path: {filePath}");
    Console.WriteLine($"File exists: {File.Exists(filePath)}");
    
    if (File.Exists(filePath))
    {
        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"File size: {fileInfo.Length} bytes");
        WebServer.SendFileOverHTTP(e.Context.Response, filePath);
    }
    else
    {
        // List available files for debugging
        string[] files = Directory.GetFiles(StoragePath);
        Console.WriteLine("Available files:");
        foreach (var file in files)
        {
            Console.WriteLine($"  {file.Substring(StoragePath.Length)}");
        }
        
        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
    }
}
```

## Related Resources

- [SD Card Mounting Example](https://github.com/nanoframework/Samples/blob/main/samples/System.IO.FileSystem/MountExample/Program.cs)
- [WebServer E2E Tests](../tests/WebServerE2ETests/) - Contains file serving examples
- [Controllers and Routing](./controllers-routing.md) - For controller-based file serving
- [Authentication](./authentication.md) - For securing file access

The file system support in nanoFramework WebServer provides a robust foundation for serving static content from embedded devices, making it easy to create web interfaces, serve configuration files, or provide downloadable content directly from your IoT devices.
