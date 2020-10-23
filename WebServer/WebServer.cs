//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Windows.Storage;
using Windows.Storage.Streams;


namespace nanoFramework.WebServer
{
    public class WebServer : IDisposable
    {
        /// <summary>
        /// URL parameter separation character
        /// </summary>
        public const char ParamSeparator = '&';

        /// <summary>
        /// URL parameter start character
        /// </summary>
        public const char ParamStart = '?';

        /// <summary>
        /// URL parameter equal character
        /// </summary>
        public const char ParamEqual = '=';

        private const int MaxSizeBuffer = 1024;

        #region internal objects

        private bool _cancel = false;
        private Thread _serverThread = null;
        private ArrayList _callbackRoutes;
        private HttpListener _listener;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the port the server listens on.
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// The type of Http protocol used, http or https
        /// </summary>
        public HttpProtocol Protocol { get; protected set; }

        /// <summary>
        /// The Https certificate to use
        /// </summary>
        public X509Certificate HttpsCert
        {
            get => _listener.HttpsCert;

            set => _listener.HttpsCert = value;
        }

        /// <summary>
        /// SSL protocols
        /// </summary>
        public SslProtocols SslProtocols
        {
            get => _listener.SslProtocols;

            set => _listener.SslProtocols = value;
        }


        #endregion

        #region Param

        /// <summary>
        /// Get an array of parameters from a URL
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static UrlParameter[] DecodeParam(string parameter)
        {
            UrlParameter[] retParams = null;
            int i = parameter.IndexOf(ParamStart);
            int j = i;
            int k;

            if (i >= 0)
            {
                //look at the number of = and ;

                while ((i < parameter.Length) || (i == -1))
                {
                    j = parameter.IndexOf(ParamEqual, i);
                    if (j > i)
                    {
                        //first param!
                        if (retParams == null)
                        {
                            retParams = new UrlParameter[1];
                            retParams[0] = new UrlParameter();
                        }
                        else
                        {
                            UrlParameter[] rettempParams = new UrlParameter[retParams.Length + 1];
                            retParams.CopyTo(rettempParams, 0);
                            rettempParams[rettempParams.Length - 1] = new UrlParameter();
                            retParams = new UrlParameter[rettempParams.Length];
                            rettempParams.CopyTo(retParams, 0);
                        }
                        k = parameter.IndexOf(ParamSeparator, j);
                        retParams[retParams.Length - 1].Name = parameter.Substring(i + 1, j - i - 1);
                        // Nothing at the end
                        if (k == j)
                        {
                            retParams[retParams.Length - 1].Value = "";
                        }
                        // Normal case
                        else if (k > j)
                        {
                            retParams[retParams.Length - 1].Value = parameter.Substring(j + 1, k - j - 1);
                        }
                        // We're at the end
                        else
                        {
                            retParams[retParams.Length - 1].Value = parameter.Substring(j + 1, parameter.Length - j - 1);
                        }
                        if (k > 0)
                            i = parameter.IndexOf(ParamSeparator, k);
                        else
                            i = parameter.Length;
                    }
                    else
                        i = -1;
                }
            }
            return retParams;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new webserver.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="timeout">Timeout to listen and respond to a request in millisecond.</param>
        public WebServer(int port, HttpProtocol protocol) : this(port, protocol, null)
        { }

        public WebServer(int port, HttpProtocol protocol, Type[] controllers)
        {
            _callbackRoutes = new ArrayList();

            if (controllers != null)
            {
                foreach (var controller in controllers)
                {
                    var functions = controller.GetMethods();
                    foreach (var func in functions)
                    {
                        var attributes = func.GetCustomAttributes(true);
                        CallbackRoutes callbackRoutes = null;
                        foreach (var attrib in attributes)
                        {
                            if (typeof(RouteAttribute) == attrib.GetType())
                            {
                                callbackRoutes = new CallbackRoutes();
                                callbackRoutes.Route = ((RouteAttribute)attrib).Route;
                                break;
                            }
                        }
                        if (callbackRoutes != null)
                        {
                            callbackRoutes.Callback = func;
                            foreach (var attrib in attributes)
                            {
                                if (typeof(MethodAttribute) == attrib.GetType())
                                {
                                    callbackRoutes.Method = ((MethodAttribute)attrib).Method;
                                    break;
                                }
                            }
                            _callbackRoutes.Add(callbackRoutes);
                        }
                    }

                }
            }

            foreach (var callback in _callbackRoutes)
            {
                var cb = (CallbackRoutes)callback;
                Debug.WriteLine($"{cb.Callback.Name}, {cb.Route}, {cb.Method}");
            }

            Protocol = protocol;
            Port = port;
            string prefix = Protocol == HttpProtocol.Http ? "http" : "https";
            _listener = new HttpListener(prefix, port);
            _serverThread = new Thread(StartListener);
            Debug.WriteLine("Web server started on port " + port.ToString());
        }

        #endregion

        #region Events

        /// <summary>
        /// Delegate for the CommandReceived event.
        /// </summary>
        public delegate void GetRequestHandler(object obj, WebServerEventArgs e);

        /// <summary>
        /// CommandReceived event is triggered when a valid command (plus parameters) is received.
        /// Valid commands are defined in the AllowedCommands property.
        /// </summary>
        public event GetRequestHandler CommandReceived;

        #endregion

        #region Public and private methods

        /// <summary>
        /// Start the multi threaded server.
        /// </summary>
        public bool Start()
        {
            bool bStarted = true;
            // List Ethernet interfaces, so we can determine the server's address
            ListInterfaces();
            // start server           
            try
            {
                _cancel = false;
                _serverThread.Start();
                Debug.WriteLine("Started server in thread " + _serverThread.GetHashCode().ToString());
            }
            catch
            {   //if there is a problem, maybe due to the fact we did not wait enough
                _cancel = true;
                bStarted = false;
            }
            return bStarted;
        }

        /// <summary>
        /// Restart the server.
        /// </summary>
        private bool Restart()
        {
            Stop();
            return Start();
        }

        /// <summary>
        /// Stop the multi threaded server.
        /// </summary>
        public void Stop()
        {
            _cancel = true;
            Thread.Sleep(100);
            _serverThread.Abort();
            Debug.WriteLine("Stoped server in thread ");
        }

        /// <summary>
        /// Output a stream
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="strResponse">the stream to output</param>
        public static void OutPutStream(HttpListenerResponse response, string strResponse)
        {
            if (response == null)
            {
                return;
            }

            byte[] messageBody = Encoding.UTF8.GetBytes(strResponse);
            response.ContentLength64 = messageBody.Length;
            response.OutputStream.Write(messageBody, 0, messageBody.Length);
        }

        /// <summary>
        /// Output an HTTP Code and close the connection
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="code">the http code</param>
        public static void OutputHttpCode(HttpListenerResponse response, HttpCode code)
        {
            if (response == null)
            {
                return;
            }

            // This is needed to force the 200 OK without body to go thru
            response.ContentLength64 = 0;
            response.KeepAlive = false;
            response.StatusCode = (int)code;
        }

        /// <summary>
        /// Read the timeout for a request to be send.
        /// </summary>
        public static void SendFileOverHTTP(HttpListenerResponse response, StorageFile strFilePath)
        {
            string ContentType = "text/html";
            //determine the type of file for the http header
            if (strFilePath.FileType.ToLower() == ".cs" ||
                strFilePath.FileType.ToLower() == ".txt" ||
                strFilePath.FileType.ToLower() == ".csproj"
            )
            {
                ContentType = "text/plain";
            }
            else if (strFilePath.FileType.ToLower() == ".jpg" ||
                strFilePath.FileType.ToLower() == ".bmp" ||
                strFilePath.FileType.ToLower() == ".jpeg" ||
                strFilePath.FileType.ToLower() == ".png"
              )
            {
                ContentType = "image";
            }
            else if (strFilePath.FileType.ToLower() == ".htm" ||
                strFilePath.FileType.ToLower() == ".html"
              )
            {
                ContentType = "text/html";
            }
            else if (strFilePath.FileType.ToLower() == ".mp3")
            {
                ContentType = "audio/mpeg";
            }
            else if (strFilePath.FileType.ToLower() == ".css")
            {
                ContentType = "text/css";
            }

            try
            {
                IBuffer readBuffer = FileIO.ReadBuffer(strFilePath);
                long fileLength = readBuffer.Length;

                response.ContentType = ContentType;
                response.ContentLength64 = fileLength;
                // Now loops sending all the data.

                byte[] buf = new byte[MaxSizeBuffer];
                using (DataReader dataReader = DataReader.FromBuffer(readBuffer))
                {
                    for (long bytesSent = 0; bytesSent < fileLength;)
                    {
                        // Determines amount of data left.
                        long bytesToRead = fileLength - bytesSent;
                        bytesToRead = bytesToRead < MaxSizeBuffer ? bytesToRead : MaxSizeBuffer;
                        // Reads the data.
                        dataReader.ReadBytes(buf);
                        // Writes data to browser
                        response.OutputStream.Write(buf, 0, (int)bytesToRead);
                        // allow some time to physically send the bits. Can be reduce to 10 or even less if not too much other code running in parallel
                        //Thread.Sleep((int)TimeSleepSocketWork.TotalMilliseconds);
                        // Updates bytes read.
                        bytesSent += bytesToRead;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private void StartListener()
        {
            _listener.Start();
            while (!_cancel)
            {
                HttpListenerContext context = _listener.GetContext();

                bool isRoute = false;

                foreach (var rt in _callbackRoutes)
                {
                    var route = (CallbackRoutes)rt;
                    var urlParam = context.Request.RawUrl.IndexOf(ParamStart);
                    bool isFound = false;
                    int incForSlash = route.Route.IndexOf('/') == 0 ? 0 : 1;
                    if (context.Request.RawUrl.IndexOf(route.Route) == incForSlash)
                    {
                        if (urlParam > 0)
                        {
                            if (urlParam == route.Route.Length + incForSlash)
                            {
                                isFound = true;
                            }
                        }
                        else
                        {
                            isFound = true;
                        }

                        if (isFound && ((route.Method == string.Empty || (context.Request.HttpMethod == route.Method))))
                        {
                            isRoute = true;
                            new Thread(() =>
                            {
                                route.Callback.Invoke(null, new object[] { new WebServerEventArgs(context) });
                                context.Response.Close();
                                context.Close();
                            }).Start();
                        }
                    }
                }

                if (!isRoute)
                {
                    new Thread(() =>
                    {
                        CommandReceived?.Invoke(this, new WebServerEventArgs(context));
                        context.Response.Close();
                        context.Close();
                    }).Start();
                }

            }
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }

        /// <summary>
        /// List all IP address, useful for debug only
        /// </summary>
        private void ListInterfaces()
        {
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
            Debug.WriteLine("Number of Interfaces: " + ifaces.Length.ToString());
            foreach (NetworkInterface iface in ifaces)
            {
                Debug.WriteLine("IP:  " + iface.IPv4Address + "/" + iface.IPv4SubnetMask);
            }
        }

        /// <summary>
        /// Dispose of any resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing">Dispose of resources?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serverThread = null;
            }
        }

        #endregion
    }
}