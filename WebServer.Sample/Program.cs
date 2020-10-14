//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Threading;
using System.Diagnostics;
using Windows.Devices.WiFi;
using nanoFramework.Networking;
using Windows.Storage;

namespace nanoFramework.WebServer.Sample
{
    public class Program
    {
        private static string MySsid = "YOURSSID";
        private static string MyPassword = "YourPASSWORD";
        private static bool isConnected = false;
        private static StorageFolder storage;

        public static void Main()
        {
            Debug.WriteLine("Hello from a webserver!");

            try
            {
                // Get the first WiFI Adapter
                WiFiAdapter wifi = WiFiAdapter.FindAllAdapters()[0];
                Debug.WriteLine("Getting wifi adaptor");

                wifi.AvailableNetworksChanged += WifiAvailableNetworksChanged;
            rescan:
                wifi.ScanAsync();
                Debug.WriteLine("Scanning...");

                var timeout = DateTime.UtcNow.AddSeconds(10);
                while (!isConnected)
                {
                    Thread.Sleep(100);
                    if (DateTime.UtcNow > timeout)
                    {
                        goto rescan;
                    }
                }

                NetworkHelpers.SetupAndConnectNetwork(false);

                Debug.WriteLine("Waiting for network up and IP address...");

                NetworkHelpers.IpAddressAvailable.WaitOne();

                storage = KnownFolders.RemovableDevices.GetFolders()[0];

                // Instantiate a new web server on port 80.
                using (WebServer server = new WebServer(80, TimeSpan.FromSeconds(2)))
                {
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

        private static void WifiAvailableNetworksChanged(WiFiAdapter sender, object e)
        {
            var wifiNetworks = sender.NetworkReport.AvailableNetworks;
            foreach (var net in wifiNetworks)
            {
                Debug.WriteLine($"SSID: {net.Ssid}, strength: {net.SignalBars}");
                if (net.Ssid == MySsid)
                {
                    // Connect to network
                    WiFiConnectionResult result = sender.Connect(net, WiFiReconnectionKind.Automatic, MyPassword);

                    // Display status
                    if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                    {
                        Debug.WriteLine($"Connected to Wifi network {net.Ssid}");
                        isConnected = true;
                        break;
                    }
                    else
                    {
                        Debug.WriteLine($"Error {result.ConnectionStatus} connecting to Wifi network");
                    }
                }

            }
        }

        private static void ServerCommandReceived(object source, WebServer.WebServerEventArgs e)
        {
            try
            {                
                var url = e.RawURL;
                Debug.WriteLine("Command received:" + e.RawURL);

                if (url.ToLower() == "sayhello")
                {
                    // This is simple raw text returned
                    WebServer.OutPutStream(e.Response, "It's working, url is empty, this is just raw text, /sayhello is just returning a raw text");
                }
                else if (url.Length == 0)
                {
                    // Here you can return a real html page for example

                    WebServer.OutPutStream(e.Response, "HTTP/1.1 200 OK\r\nContent-Type: text/html; charset=utf-8\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n<html><head>" +
                        "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
                        "<a href='/Text.txt'>Download the Text.txt file</a><br>" +
                        "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
                }
                else if(url.ToLower() == "useinternal")
                {
                    // This tells the web server to use the internal storage and create a simple text file
                    storage = KnownFolders.InternalDevices.GetFolders()[0];
                    var testFile = storage.CreateFile("text.txt", CreationCollisionOption.ReplaceExisting);
                    FileIO.WriteText(testFile, "This is an example of file\r\nAnd this is the second line");
                    WebServer.OutPutStream(e.Response, "Created a test file text.txt on internal storage");
                }
                else if(url.ToLower().IndexOf("param.htm") == 0)
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
                else
                {
                    // Very simple example serving a static file on an SD card
                    var files = storage.GetFiles();                    
                    foreach(var file in files)
                    {
                        if( file.Name == url)
                        {
                            WebServer.SendFileOverHTTP(e.Response, file);
                            return;
                        }
                    }

                    WebServer.OutPutStream(e.Response, $"HTTP/1.1 404");
                }
            }
            catch (Exception)
            {
                WebServer.OutPutStream(e.Response, "HTTP/1.1 500");
            }
        }
    }
}
