// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;
using nanoFramework.WebServer;
using nanoFramework.WebServer.Skills;

namespace SkillsEndToEndTest
{
    public partial class Program
    {
        private static WebServer _server;

        public static void Main()
        {
            Debug.WriteLine("Hello from Skills Discovery Server!");

            var res = WifiNetworkHelper.ConnectDhcp(Ssid, Password, requiresDateTime: true, token: new CancellationTokenSource(60_000).Token);
            if (!res)
            {
                Debug.WriteLine("Impossible to connect to wifi, most likely invalid credentials");
                return;
            }

            Debug.WriteLine($"Connected with wifi credentials. IP Address: {GetCurrentIPAddress()}");

            // Discover and register all skills
            SkillRegistry.DiscoverSkills(new Type[] { typeof(ClimateSkill), typeof(LightingSkill) });
            Debug.WriteLine("Skills discovered and registered.");

            // Configure Agent Card metadata
            SkillDiscoveryController.AgentName = "nanoFramework IoT Device";
            SkillDiscoveryController.AgentDescription = "Embedded HVAC and lighting controller with sensor capabilities";
            SkillDiscoveryController.AgentVersion = "1.0.0";

            // Start the web server with the Skills Discovery controller
            _server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(SkillDiscoveryController) });
            _server.CommandReceived += ServerCommandReceived;
            _server.Start();

            Debug.WriteLine("Skills Discovery Server started. Endpoints:");
            Debug.WriteLine("  GET  /.well-known/agent-card.json  — A2A Agent Card");
            Debug.WriteLine("  GET  /skills                       — Skills list");
            Debug.WriteLine("  POST /skills/invoke                — Invoke skill action");

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
            return ni.IPv4Address.ToString();
        }
    }
}
