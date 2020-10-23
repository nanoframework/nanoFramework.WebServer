# .NET nanoFramework WebServer

This is a simple nanoFrmaework WebServer. Features:

- Handle multithread requests
- Serve static files on any storage
- Handle parameter in URL
- Possible to have multiple WebServer running at the same time
- supports GET/PUT and any other word
- Supports any type of header
- Supports content in POST
- Reflection for easy usage of controllers and notion of routes
- Helpers to return error code directly facilitating REST API
- HTTPS support

Limitations:
- Does not support any zip way
- No URL decode/encode implemented yet

## Usage

You just need to specify a port and a timeout for the querries and add an event handler when a request is incoming. With this first way, you will have an event raised every time you'll receive a request.

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

In this case, you're passing 2 classes where you have public methods decorated which will be called everytime the route is found.

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

In this example, the `RoutePostTest` will be called everytime the called url will be `test` or `Test2` or `tEst42` or `TEST`, the url can be with parameters and the method GET. Be aware that `Test` won't call the function, neither `test/`.

The `RouteAnyTest`is called whenever the url is `test/any` whatever the method is.

There is a more advance example with simple REST API to get a list of Person and add a Person. Check it in the [sample](./WebServer.Sample/ControllerPerson.cs).

**Important**
* By default the routes are not case sensitive and the attribute **must** be lowercase
* If you want to use case sensite routes like in the previous example, use the attribute `CaseSensitive`. As in the previous example, you **must** write the route as you want it to be restonded too

## A simple GPIO controller REST API

You will find in simple [GPIO controller sample](./WebServer.GpioRest) REST API. The controller not case sensitive and is working like this:

- To open the pin 2 as output: http://yoururl/open/2/output
- To open pin 4 as input: http://yoururl/open/4/input
- To write the value high to pin 2: http://yoururl/write/2/high
  - You can use high or 1, it has the same effect and will place the pin in high value
  - You can use low of 0, it has the same effect and will place the pin in low value
- To read the pin 4: http://yoururl/read/4, you will get as a raw text `high`or `low`depending on the state

## Managing incoming querries thru events

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
var files = storage.GetFiles();
foreach (var file in files)
{
    if (file.Name == url)
    {
        WebServer.SendFileOverHTTP(e.Context.Response, file);
        return;
    }
}

WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
```

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
        byte[] buff = new byte[e.Context.Request.ContentLength64];
        e.Context.Request.InputStream.Read(buff, 0, buff.Length);
        ret += $"Hex string representation:\r\n";
        for (int i = 0; i < buff.Length; i++)
        {
            ret += buff[i].ToString("X") + " ";
        }

    }

    WebServer.OutPutStream(e.Context.Response, ret);
}
```

This API example is basic but as you get the method, you can choose what to do.

As you get the url, you can check for a specific controller called. And you have the parameters and the content payload!

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

You can of course use the routes as defined earlier. Both will work, event or route with the notion of controller.
