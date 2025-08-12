// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Mcp;

namespace McpEndToEndTest
{
    public partial class Program
    {
        private static WebServer _server;

        public static void Main()
        {
            Debug.WriteLine("Hello from MCP Server!");

            var res = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: new CancellationTokenSource(60_000).Token);
            if (!res)
            {
                Debug.WriteLine("Impossible to connect to wifi, most likely invalid credentials");
                return;
            }

            Debug.WriteLine($"Connected with wifi credentials. IP Address: {GetCurrentIPAddress()}");

            McpToolRegistry.DiscoverTools(new Type[] { typeof(McpServerTests.McpTools) });
            Debug.WriteLine("MCP Tools discovered and registered.");

            McpPromptRegistry.DiscoverPrompts(new Type[] { typeof(McpServerTests.McpPrompts) });
            Debug.WriteLine("MCP Prompts discovered and registered.");

            _server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(McpServerController) });
            _server.CommandReceived += ServerCommandReceived;
            // Start the server.
            _server.Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void ServerCommandReceived(object obj, WebServerEventArgs e)
        {
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"{nameof(ServerCommandReceived)} {e.Context.Request.HttpMethod} {url}");
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }

        public static string GetCurrentIPAddress()
        {
            NetworkInterface ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            // get first NI ( Wifi on ESP32 )
            return ni.IPv4Address.ToString();
        }
    }
}
