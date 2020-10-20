//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the port the server listens on.
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// Read the timeout for a request to be send.
        /// </summary>
        public TimeSpan Timeout { get; protected set; } = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// Time to wait while using the socket and have the socket outputting the data
        /// </summary>
        public static TimeSpan TimeSleepSocketWork { get; set; } = TimeSpan.FromMilliseconds(10);

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
        public WebServer(int port, TimeSpan timeout) : this(port, timeout, null)
        { }

        public WebServer(int port, TimeSpan timeout, Type[] controllers)
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

            Timeout = timeout;
            Port = port;
            _serverThread = new Thread(StartServer);
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
        public static void OutPutStream(Socket response, string strResponse)
        {
            if (response == null)
            {
                return;
            }

            byte[] messageBody = Encoding.UTF8.GetBytes(strResponse);
            response.Send(messageBody, 0, messageBody.Length, SocketFlags.None);
            //allow time to physically send the bits
            Thread.Sleep((int)TimeSleepSocketWork.TotalMilliseconds);
        }

        /// <summary>
        /// Output an HTTP Code and close the connection
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="code">the http code</param>
        public static void OutputHttpCode(Socket response, HttpCode code)
        {
            if (response == null)
            {
                return;
            }


            string strResponse = string.Empty;
            switch (code)
            {
                case HttpCode.Continue:
                    strResponse = $"HTTP/1.1 100 Continue\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.OK:
                    strResponse = $"HTTP/1.1 200 OK\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.Created:
                    strResponse = $"HTTP/1.1 201 Created\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.Accepted:
                    strResponse = $"HTTP/1.1 202 Accepted\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.BadRequest:
                    strResponse = $"HTTP/1.1 400 Bad Request\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.Unauthorized:
                    strResponse = $"HTTP/1.1 401 Unauthorized\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.Forbidden:
                    strResponse = $"HTTP/1.1 403 Forbidden\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.NotFound:
                    strResponse = $"HTTP/1.1 404 Not Found\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.MethodNotAllowed:
                    strResponse = $"HTTP/1.1 405 Method Not Allowed\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.NotAccepted:
                    strResponse = $"HTTP/1.1 406 Not Accepted\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.RequestTimeout:
                    strResponse = $"HTTP/1.1 408 Request Timeout\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.Conflict:
                    strResponse = $"HTTP/1.1 409 Conflict\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.InternalServerError:
                    strResponse = $"HTTP/1.1 500 Internal Server Error\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.NotImplemented:
                    strResponse = $"HTTP/1.1 501 Not Implemented\r\nConnection: Close\r\n\r\n";
                    break;
                case HttpCode.ServiceUnavailable:
                    strResponse = $"HTTP/1.1 503 Service Unavailable\r\nConnection: Close\r\n\r\n";
                    break;
                default:
                    strResponse = $"HTTP/1.1 400 Bad Request\r\nConnection: Close\r\n\r\n";
                    break;
            }

            byte[] messageBody = Encoding.UTF8.GetBytes(strResponse);
            response.Send(messageBody, 0, messageBody.Length, SocketFlags.None);
            //allow time to physically send the bits
            Thread.Sleep((int)TimeSleepSocketWork.TotalMilliseconds);
        }

        /// <summary>
        /// Read the timeout for a request to be send.
        /// </summary>
        public static void SendFileOverHTTP(Socket response, StorageFile strFilePath)
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

                string strResp = $"HTTP/1.1 200 OK\r\nContent-Type: {ContentType}; charset=UTF-8\r\nContent-Length: {fileLength}\r\nCache-Control: no-cache\r\nConnection: close\r\n\r\n";
                OutPutStream(response, strResp);
                // can be improved by building here the HTTP HEader string and return the length of the file
                // Debug.WriteLine("File length " + fileLength);
                // Now loops sending all the data.

                byte[] buf = new byte[MaxSizeBuffer];
                using (DataReader dataReader = DataReader.FromBuffer(readBuffer))
                {
                    for (long bytesSent = 0; bytesSent < fileLength;)
                    {
                        // Determines amount of data left.
                        long bytesToRead = fileLength - bytesSent;
                        bytesToRead = bytesToRead < MaxSizeBuffer ? bytesToRead : MaxSizeBuffer;
                        // This is not very elegant but there is now way to say we want to read part of the destination buffer
                        if (bytesToRead < MaxSizeBuffer)
                        {
                            buf = new byte[bytesToRead];
                        }
                        // Reads the data.
                        dataReader.ReadBytes(buf);
                        // Writes data to browser
                        response.Send(buf, 0, (int)bytesToRead, SocketFlags.None);
                        // allow some time to physically send the bits. Can be reduce to 10 or even less if not too much other code running in parallel
                        Thread.Sleep((int)TimeSleepSocketWork.TotalMilliseconds);
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

        /// <summary>
        /// Starts the server.
        /// </summary>
        private void StartServer()
        {
            using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                //set a receive Timeout to avoid too long connection 
                server.ReceiveTimeout = (int)Timeout.TotalMilliseconds * 10;
                server.Bind(new IPEndPoint(IPAddress.Any, this.Port));
                server.Listen(int.MaxValue);
                while (!_cancel)
                {
                    try
                    {
                        using (Socket connection = server.Accept())
                        {
                            if (connection.Poll(-1, SelectMode.SelectRead))
                            {
                                // Create buffer and receive raw bytes.
                                byte[] bytes = new byte[connection.Available];
                                int count = connection.Receive(bytes);
                                // Debug.WriteLine("Request received from " + connection.RemoteEndPoint.ToString());
                                //setup some time for send timeout as 10s.
                                //necessary to avoid any problem when multiple requests are done the same time.
                                connection.SendTimeout = (int)Timeout.TotalMilliseconds;
                                // Convert to string, will include HTTP headers.
                                string rawData = new string(Encoding.UTF8.GetChars(bytes));
                                // Debug.WriteLine(rawData);
                                string mURI = string.Empty;
                                string mMethod = string.Empty;
                                Header[] headers = null;
                                byte[] content = null;

                                // Remove GET + Space
                                // pull out uri and remove the first /
                                if (rawData.Length > 5)
                                {
                                    int contentLength = 0;
                                    int uriStart = rawData.IndexOf(' ') + 2;
                                    mMethod = rawData.Substring(0, uriStart - 2);
                                    int httpInfo = rawData.IndexOf(' ', uriStart);
                                    mURI = rawData.Substring(uriStart, httpInfo - uriStart);
                                    int endHttpInfo = rawData.IndexOf('\n', httpInfo);
                                    // TODO: capture HTTP/1.1, etc
                                    int doubleCrNl = rawData.IndexOf("\r\n\r\n");
                                    doubleCrNl = doubleCrNl > 0 ? doubleCrNl : rawData.Length - 1;
                                    // Now find the headers
                                    var split = rawData.Substring(endHttpInfo, doubleCrNl - endHttpInfo).Split('\r');
                                    headers = new Header[split.Length];
                                    int inc = 0;
                                    foreach (var sp in split)
                                    {
                                        var spClean = sp.Trim('\n').Split(':');
                                        var header = new Header() { Name = spClean[0], Value = spClean.Length > 1 ? spClean[1].TrimStart(' ') : string.Empty };
                                        headers[inc++] = header;
                                        if (header.Name == "Content-Length")
                                        {
                                            contentLength = Convert.ToInt32(header.Value);
                                        }
                                    }

                                    if (rawData.Length - doubleCrNl + 4 < contentLength)
                                    {
                                        contentLength = rawData.Length - doubleCrNl + 4;
                                    }

                                    if (contentLength > 0)
                                    {
                                        content = new byte[contentLength];
                                        for (int i = 0; i < contentLength; i++)
                                        {
                                            content[i] = bytes[doubleCrNl + 4 + i];
                                        }
                                    }
                                }

                                bool isRoute = false;

                                foreach (var rt in _callbackRoutes)
                                {
                                    var route = (CallbackRoutes)rt;
                                    var urlParam = mURI.IndexOf(ParamStart);
                                    bool isFound = false;
                                    if ((mURI.IndexOf(route.Route) == 0))
                                    {
                                        if (urlParam > 0)
                                        {
                                            if (urlParam == route.Route.Length)
                                            {
                                                isFound = true;
                                            }
                                        }
                                        else
                                        {
                                            isFound = true;
                                        }

                                        if (isFound && ((route.Method == string.Empty || (mMethod == route.Method))))
                                        {
                                            route.Callback.Invoke(null, new object[] { new WebServerEventArgs(connection, mURI, mMethod, headers, content) });
                                            isRoute = true;
                                        }
                                    }
                                }

                                if (!isRoute)
                                {
                                    CommandReceived?.Invoke(this, new WebServerEventArgs(connection, mURI, mMethod, headers, content));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //this may be due to a bad IP address
                        Debug.WriteLine(e.Message);
                    }
                }
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