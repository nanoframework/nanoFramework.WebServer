// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.

using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace WebServerE2ETests
{
    public partial class Program
    {
        private static WebServer _server;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework WebServer end to end tests!");

            var res = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: new CancellationTokenSource(60_000).Token);
            if (!res)
            {
                Debug.WriteLine("Impossible to connect to wifi, most likely invalid credentials");
                return;
            }

            Debug.WriteLine($"Connected with wifi credentials. IP Address: {GetCurrentIPAddress()}");
            _server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(SimpleRouteController), typeof(AuthController) });
            // To test authentication with various scenarios
            _server.ApiKey = "ATopSecretAPIKey1234";
            _server.Credential = new NetworkCredential("topuser", "topPassword");
            // Add a handler for commands that are received by the server.
            _server.CommandReceived += ServerCommandReceived;
            _server.WebServerStatusChanged += WebServerStatusChanged;

            // Start the server.
            _server.Start();

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        private static void WebServerStatusChanged(object obj, WebServerStatusEventArgs e)
        {
            Debug.WriteLine($"The web server is now {(e.Status == WebServerStatus.Running ? "running" : "stopped" )}");
        }

        private static void ServerCommandReceived(object obj, WebServerEventArgs e)
        {
            const string FileName = "I:\\Text.txt";
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"{nameof(ServerCommandReceived)} {e.Context.Request.HttpMethod} {url}");

            if (url.ToLower().IndexOf("/param.htm") == 0)
            {
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
                return;
            }
            else if (url.IndexOf("/Text.txt") == 0)
            {
                if (File.Exists(FileName))
                {
                    WebServer.SendFileOverHTTP(e.Context.Response, FileName);
                    return;
                }
                else
                {
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                    return;
                }
            }
            else if (url.IndexOf("/Text2.txt") == 0)
            {
                WebServer.SendFileOverHTTP(e.Context.Response, "Text2.txt", Encoding.UTF8.GetBytes("This is a test file for WebServer"));
                return;
            }
            else if (url.ToLower().IndexOf("/useinternal") == 0)
            {
                File.WriteAllText(FileName, "This is a test file for WebServer");
                return;
            }
            else
            {
                WebServer.OutPutStream(e.Context.Response, "<html><head>" +
                    "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
                    "<a href='/Text.txt'>Download the Text.txt file</a><br>" +
                    "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
                return;
            }
        }

        public static string GetCurrentIPAddress()
        {
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            // get first NI ( Wifi on ESP32 )
            return ni.IPv4Address.ToString();
        }
    }
}
