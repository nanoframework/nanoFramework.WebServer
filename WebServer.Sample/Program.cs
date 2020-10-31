//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Threading;
using System.Diagnostics;
using nanoFramework.Networking;
using System.Device.Gpio;
using System.Text;
using System.Net;

#if HAS_WIFI
using Windows.Devices.WiFi;
#endif

#if HAS_STORAGE
using Windows.Storage;
#endif

namespace nanoFramework.WebServer.Sample
{
    public class Program
    {

#if HAS_WIFI
        private static string MySsid = "ssid";
        private static string MyPassword = "password";
        private static bool _isConnected = false;
#endif

#if HAS_STORAGE
        private static StorageFolder _storage;
#endif

        private static GpioController _controller;
        private static string _securityKey = "MySecurityKey42";

        public static void Main()
        {
            Debug.WriteLine("Hello from a webserver!");

            try
            {
                int connectRetry = 0;

#if HAS_WIFI
                // Get the first WiFI Adapter
                WiFiAdapter wifi = WiFiAdapter.FindAllAdapters()[0];
                Debug.WriteLine("Getting wifi adaptor");

                wifi.AvailableNetworksChanged += WifiAvailableNetworksChanged;

            rescan:
                wifi.ScanAsync();
                Debug.WriteLine("Scanning...");

                var timeout = DateTime.UtcNow.AddSeconds(10);
                while (!_isConnected)
                {
                    Thread.Sleep(100);
                    if (DateTime.UtcNow > timeout)
                    {
                        goto rescan;
                    }
                }
#endif

                NetworkHelpers.SetupAndConnectNetwork(true);

                Debug.WriteLine("Waiting for network up and IP address...");

                NetworkHelpers.IpAddressAvailable.WaitOne();
                NetworkHelpers.DateTimeAvailable.WaitOne();

#if HAS_WIFI
                while (!NetworkHelpers.CheckIP())
                {
                    Thread.Sleep(500);
                    connectRetry++;
                    if (connectRetry == 5)
                    {
                        connectRetry = 0;
                        goto rescan;
                    }
                }
#endif

#if HAS_STORAGE
                _storage = KnownFolders.RemovableDevices.GetFolders()[0];
#endif

                _controller = new GpioController();

                // Instantiate a new web server on port 80.
                using (WebServer server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ControllerPerson), typeof(ControllerTest), typeof(ControllerAuth) }))
                {
                    // To test authentication with various scenarios
                    server.ApiKey = _securityKey;
                    server.Credential = new NetworkCredential("user", "password");
                    // Add a handler for commands that are received by the server.
                    server.CommandReceived += ServerCommandReceived;

                    // Start the server.
                    server.Start();

                    Thread.Sleep(Timeout.Infinite);
                }

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"{ex}");
            }
        }

#if HAS_WIFI
        private static void WifiAvailableNetworksChanged(WiFiAdapter sender, object e)
        {
            var wifiNetworks = sender.NetworkReport.AvailableNetworks;
            foreach (var net in wifiNetworks)
            {
                Debug.WriteLine($"SSID: {net.Ssid}, strength: {net.SignalBars}");
                if (net.Ssid == MySsid)
                {
                    if (_isConnected)
                    {
                        sender.Disconnect();
                        _isConnected = false;
                        Thread.Sleep(3000);
                    }
                    // Connect to network
                    WiFiConnectionResult result = sender.Connect(net, WiFiReconnectionKind.Automatic, MyPassword);

                    // Display status
                    if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                    {
                        Debug.WriteLine($"Connected to Wifi network {net.Ssid}");
                        _isConnected = true;
                        break;
                    }
                    else
                    {
                        Debug.WriteLine($"Error {result.ConnectionStatus} connecting to Wifi network");
                    }
                }

            }
        }
#endif
        private static void ServerCommandReceived(object source, WebServerEventArgs e)
        {
            try
            {
                var url = e.Context.Request.RawUrl;
                Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

                if (url.ToLower() == "/sayhello")
                {
                    // This is simple raw text returned
                    WebServer.OutPutStream(e.Context.Response, "It's working, url is empty, this is just raw text, /sayhello is just returning a raw text");
                }
                else if (url.Length <= 1)
                {
                    // Here you can return a real html page for example

                    WebServer.OutPutStream(e.Context.Response, "<html><head>" +
                        "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
                        "<a href='/Text.txt'>Download the Text.txt file</a><br>" +
                        "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
                }
#if HAS_STORAGE
                else if (url.ToLower() == "/useinternal")
                {
                    // This tells the web server to use the internal storage and create a simple text file
                    _storage = KnownFolders.InternalDevices.GetFolders()[0];
                    var testFile = _storage.CreateFile("text.txt", CreationCollisionOption.ReplaceExisting);
                    FileIO.WriteText(testFile, "This is an example of file\r\nAnd this is the second line");
                    WebServer.OutPutStream(e.Context.Response, "Created a test file text.txt on internal storage");
                }
#endif
                else if (url.ToLower().IndexOf("/param.htm") == 0)
                {
                    ParamHtml(e);
                }
                else if (url.ToLower().IndexOf("/api/") == 0)
                {
                    // Check the routes and dispatch
                    var routes = url.TrimStart('/').Split('/');
                    if (routes.Length > 3)
                    {
                        // Check the security key
                        if (!CheckAPiKey(e.Context.Request.Headers))
                        {
                            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.Forbidden);
                            return;
                        }

                        var pinNumber = Convert.ToInt16(routes[2]);

                        // Do we have gpio ?
                        if (routes[1].ToLower() == "gpio")
                        {
                            if ((routes[3].ToLower() == "high") || (routes[3].ToLower() == "1"))
                            {
                                _controller.Write(pinNumber, PinValue.High);
                            }
                            else if ((routes[3].ToLower() == "low") || (routes[3].ToLower() == "0"))
                            {
                                _controller.Write(pinNumber, PinValue.Low);
                            }
                            else
                            {
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                                return;
                            }

                            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
                            return;
                        }
                        else if (routes[1].ToLower() == "open")
                        {
                            if (routes[3].ToLower() == "input")
                            {
                                if (!_controller.IsPinOpen(pinNumber))
                                {
                                    _controller.OpenPin(pinNumber);
                                }

                                _controller.SetPinMode(pinNumber, PinMode.Input);
                            }
                            else if (routes[3].ToLower() == "output")
                            {
                                if (!_controller.IsPinOpen(pinNumber))
                                {
                                    _controller.OpenPin(pinNumber);
                                }

                                _controller.SetPinMode(pinNumber, PinMode.Output);
                            }
                            else
                            {
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                                return;
                            }
                        }
                        else if (routes[1].ToLower() == "close")
                        {
                            if (_controller.IsPinOpen(pinNumber))
                            {
                                _controller.ClosePin(pinNumber);
                            }
                        }
                        else
                        {
                            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                            return;
                        }

                        WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
                        return;
                    }
                    else if (routes.Length == 2)
                    {
                        if (routes[1].ToLower() == "apikey")
                        {
                            // Check the security key
                            if (!CheckAPiKey(e.Context.Request.Headers))
                            {
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.Forbidden);
                                return;
                            }

                            if (e.Context.Request.HttpMethod != "POST")
                            {
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                                return;
                            }

                            // Get the param from the body
                            if (e.Context.Request.ContentLength64 == 0)
                            {
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                                return;
                            }

                            byte[] buff = new byte[e.Context.Request.ContentLength64];
                            e.Context.Request.InputStream.Read(buff, 0, buff.Length);
                            string rawData = new string(Encoding.UTF8.GetChars(buff));
                            var parameters = rawData.Split('=');
                            if (parameters.Length < 2)
                            {
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                                return;
                            }

                            if (parameters[0].ToLower() == "newkey")
                            {
                                _securityKey = parameters[1];
                                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
                                return;
                            }

                            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.BadRequest);
                            return;
                        }
                    }
                    else
                    {
                        ApiDefault(e);
                    }
                }

#if HAS_STORAGE
                else
                {
                    // Very simple example serving a static file on an SD card
                    var files = _storage.GetFiles();
                    foreach (var file in files)
                    {
                        if (file.Name == url)
                        {
                            WebServer.SendFileOverHTTP(e.Context.Response, file);
                            return;
                        }
                    }

                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                }
#endif

            }
            catch (Exception)
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.InternalServerError);
            }
        }

        private static bool CheckAPiKey(WebHeaderCollection headers)
        {
            var sec = headers.GetValues("ApiKey");
            if (sec != null)
            {
                if (sec.Length > 0)
                {
                    if (sec[0] == _securityKey)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void ParamHtml(WebServerEventArgs e)
        {
            var url = e.Context.Request.RawUrl;
            // Test with parameters
            var parameters = WebServer.DecodeParam(url);
            string toOutput = "<html><head>" +
                "<title>Hi from nanoFramework Server</title></head><body>Here are the parameters of this URL: <br />";
            foreach (var par in parameters)
            {
                toOutput += $"Parameter name: {par.Name}, Value: {par.Value}<br />";
            }
            toOutput += "</body></html>";
            WebServer.OutPutStream(e.Context.Response, toOutput);
        }

        private static void ApiDefault(WebServerEventArgs e)
        {
            string ret = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain; charset=UTF-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n";
            ret += $"Your request type is: {e.Context.Request.HttpMethod}\r\n";
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
    }
}
