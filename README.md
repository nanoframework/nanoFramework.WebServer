# nanoFrmaework WebServer

This is a simple nanoFrmaework WebServer. Features:

- Handle multithread requests
- Serve static files on any storage
- Handle parameter in URL
- Possible to have multiple WebServer running at the same time

## Usage

You just need to specify a port and a timeout for the querries and add an event handler when a request is incoming.

```csharp
using (WebServer server = new WebServer(80, TimeSpan.FromSeconds(2)))
{
    // Add a handler for commands that are received by the server.
    server.CommandReceived += ServerCommandReceived;

    // Start the server.
    server.Start();

    Thread.Sleep(Timeout.Infinite);
}
```

## Managing incoming querries

Very basic usage is the following:

```csharp
private static void ServerCommandReceived(object source, WebServer.WebServerEventArgs e)
{
    var url = e.RawURL;
    Debug.WriteLine("Command received:" + e.RawURL);

    if (url.ToLower() == "sayhello")
    {
        // This is simple raw text returned
        WebServer.OutPutStream(e.Response, "It's working, url is empty, this is just raw text, /sayhello is just returning a raw text");
    }
    else
    {
        WebServer.OutPutStream(e.Response, $"HTTP/1.1 404");
    }
}
```

You can do more advance scenario like returning a full HTML page:

```csharp
WebServer.OutPutStream(e.Response, "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n<html><head>" +
    "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
    "<a href='/Text.txt'>Download the Text.txt file</a><br>" +
    "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
```

And can get parameters from a URL a an example from the previous link on the param.html page:

```csharp
if (url.ToLower().IndexOf("param.htm") == 0)
{
    // Test with parameters
    var parameters = WebServer.decryptParam(url);
    string toOutput = "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n<html><head>" +
        "<title>Hi from nanoFramework Server</title></head><body>Here are the parameters of this URL: <br />";
    foreach (var par in parameters)
    {
        toOutput += $"Parameter name: {par.Name}, Value: {par.Value}<br />";
    }
    toOutput += "</body></html>";
    WebServer.OutPutStream(e.Response, toOutput);
}
```

And server static files:

```csharp
var files = storage.GetFiles();
foreach (var file in files)
{
    if (file.Name == url)
    {
        WebServer.SendFileOverHTTP(e.Response, file);
        return;
    }
}

WebServer.OutPutStream(e.Response, $"HTTP/1.1 404");
```

And more! Check the complete example for more about this WebServer!