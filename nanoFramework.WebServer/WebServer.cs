// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
#if FILESYSTEM
using System.IO;
#endif
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
namespace nanoFramework.WebServer
{
    /// <summary>
    /// This class instantiates a web server.
    /// </summary>
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
        private const string DefaultRouteMethod = "GET";

        #region internal objects

        private bool _cancel = true;
        private Thread _serverThread = null;
        private readonly ArrayList _callbackRoutes;
        private readonly HttpListener _listener;

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

        /// <summary>
        /// Network credential used for default user:password couple during basic authentication
        /// </summary>
        public NetworkCredential Credential { get; set; }

        /// <summary>
        /// Default APiKey to be used for authentication when no key is specified in the attribute
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets a value indicating whether the web server is running.
        /// </summary>
        public bool IsRunning => !_cancel;

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
            int k;

            if (i >= 0)
            {
                while ((i < parameter.Length) || (i == -1))
                {
                    int j = parameter.IndexOf(ParamEqual, i);
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

        /// <summary>
        /// Extracts route parameters from a URL that matches a parameterized route.
        /// </summary>
        /// <param name="route">The route template with parameters (e.g., "/api/devices/{id}").</param>
        /// <param name="rawUrl">The actual URL being requested.</param>
        /// <param name="caseSensitive">Whether the comparison should be case sensitive.</param>
        /// <returns>An array of UrlParameter objects containing the parameter names and values, or null if the route doesn't match.</returns>
        public static UrlParameter[] ExtractRouteParameters(string route, string rawUrl, bool caseSensitive = false)
        {
            if (string.IsNullOrEmpty(route) || string.IsNullOrEmpty(rawUrl))
            {
                return null;
            }

            // Remove query parameters from the URL for matching
            var urlParam = rawUrl.IndexOf(ParamStart);
            var urlPath = urlParam > 0 ? rawUrl.Substring(0, urlParam) : rawUrl;

            // Normalize the URL path and route for comparison
            var urlToCompare = caseSensitive ? urlPath : urlPath.ToLower();
            var routeToCompare = caseSensitive ? route : route.ToLower();

            // Ensure both paths start with '/' for consistent segment splitting
            if (!urlToCompare.StartsWith("/"))
            {
                urlToCompare = "/" + urlToCompare;
            }
            if (!routeToCompare.StartsWith("/"))
            {
                routeToCompare = "/" + routeToCompare;
            }

            // Split into segments
            var urlSegments = urlToCompare.Split('/');
            var routeSegments = routeToCompare.Split('/');

            // Number of segments must match
            if (urlSegments.Length != routeSegments.Length)
            {
                return null;
            }

            ArrayList parameters = new ArrayList();

            // Compare each segment and extract parameters
            for (int i = 0; i < routeSegments.Length; i++)
            {
                var routeSegment = routeSegments[i];
                var urlSegment = urlSegments[i];

                // Skip empty segments (from leading slash)
                if (string.IsNullOrEmpty(routeSegment) && string.IsNullOrEmpty(urlSegment))
                {
                    continue;
                }

                // Check if this is a parameter segment (starts and ends with curly braces)
                if (routeSegment.Length > 2 &&
                    routeSegment.StartsWith("{") &&
                    routeSegment.EndsWith("}"))
                {
                    // Parameter segment matches any non-empty segment that doesn't contain '/'
                    if (string.IsNullOrEmpty(urlSegment) || urlSegment.IndexOf('/') >= 0)
                    {
                        return null;
                    }

                    // Extract parameter name (remove curly braces)
                    var paramName = routeSegment.Substring(1, routeSegment.Length - 2);
                    parameters.Add(new UrlParameter { Name = paramName, Value = urlSegments[i] }); // Use original case for value
                    continue;
                }

                // Exact match required for non-parameter segments
                if (routeSegment != urlSegment)
                {
                    return null;
                }
            }

            // Convert ArrayList to array
            if (parameters.Count == 0)
            {
                return null;
            }

            var result = new UrlParameter[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                result[i] = (UrlParameter)parameters[i];
            }

            return result;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new web server.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="protocol"><see cref="HttpProtocol"/> version to use with web server.</param>
        /// <param name="address">IP address to bind to. If <c>null</c>, the server will bind to the default network interface.</param>
        public WebServer(
            int port,
            HttpProtocol protocol,
            IPAddress address) : this(port, protocol, address, null)
        { }

        /// <summary>
        /// Instantiates a new web server.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="protocol"><see cref="HttpProtocol"/> version to use with web server.</param>
        /// <param name="controllers">Controllers to use with this web server.</param>
        public WebServer(
            int port,
            HttpProtocol protocol,
            Type[] controllers) : this(port, protocol, null, controllers)
        { }

        /// <summary>
        /// Instantiates a new web server.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="protocol"><see cref="HttpProtocol"/> version to use with web server.</param>
        /// <param name="address">IP address to bind to. If <see langword="null"/>, will bind to default address.</param>
        /// <param name="controllers">Controllers to use with this web server.</param>
        public WebServer(
            int port,
            HttpProtocol protocol,
            IPAddress address,
            Type[] controllers)
        {
            _callbackRoutes = new ArrayList();

            RegisterControllers(controllers);

            Protocol = protocol;
            Port = port;

            string prefix = Protocol == HttpProtocol.Http ? "http" : "https";

            _listener = new HttpListener(
                prefix,
                port,
                address);
        }

        private void RegisterControllers(Type[] controllers)
        {
            if (controllers == null)
            {
                return;
            }

            foreach (var controller in controllers)
            {
                var controlAttribs = controller.GetCustomAttributes(true);
                Authentication authentication = null;
                foreach (var ctrlAttrib in controlAttribs)
                {
                    if (typeof(AuthenticationAttribute) == ctrlAttrib.GetType())
                    {
                        var strAuth = ((AuthenticationAttribute)ctrlAttrib).AuthenticationMethod;
                        // We do support only None, Basic and ApiKey, raising an exception if this doesn't start by any
                        authentication = ExtractAuthentication(strAuth);
                    }
                }

                var functions = controller.GetMethods();
                foreach (var func in functions)
                {
                    var attributes = func.GetCustomAttributes(true);
                    foreach (var attrib in attributes)
                    {
                        if (typeof(RouteAttribute) != attrib.GetType())
                        {
                            continue;
                        }

                        var callbackRoutes = new CallbackRoutes
                        {
                            Route = ((RouteAttribute)attrib).Route,
                            CaseSensitive = false,
                            Method = string.Empty,
                            Authentication = authentication,
                            Callback = func
                        };

                        foreach (var attribute in attributes)
                        {
                            if (typeof(MethodAttribute) == attribute.GetType())
                            {
                                callbackRoutes.Method = ((MethodAttribute)attribute).Method;
                            }
                            else if (typeof(CaseSensitiveAttribute) == attribute.GetType())
                            {
                                callbackRoutes.CaseSensitive = true;
                            }
                            else if (typeof(AuthenticationAttribute) == attribute.GetType())
                            {
                                var strAuth = ((AuthenticationAttribute)attribute).AuthenticationMethod;
                                // A method can have a different authentication than the main class, so we override if any
                                callbackRoutes.Authentication = ExtractAuthentication(strAuth);
                            }
                        }

                        _callbackRoutes.Add(callbackRoutes);
                        Debug.WriteLine($"{callbackRoutes.Callback.Name}, {callbackRoutes.Route}, {callbackRoutes.Method}, {callbackRoutes.CaseSensitive}");
                    }
                }
            }
        }

        private Authentication ExtractAuthentication(string strAuth)
        {
            const string _none = "None";
            const string _basic = "Basic";
            const string _apiKey = "ApiKey";

            Authentication authentication;
            if (strAuth.IndexOf(_none) == 0)
            {
                if (strAuth.Length == _none.Length)
                {
                    authentication = new Authentication();
                }
                else
                {
                    throw new ArgumentException($"Authentication attribute None can only be used alone");
                }
            }
            else if (strAuth.IndexOf(_basic) == 0)
            {
                if (strAuth.Length == _basic.Length)
                {
                    authentication = new Authentication((NetworkCredential)null);
                }
                else
                {
                    var sep = strAuth.IndexOf(':');
                    if (sep == _basic.Length)
                    {
                        var space = strAuth.IndexOf(' ');
                        if (space < 0)
                        {
                            throw new ArgumentException($"Authentication attribute Basic should be 'Basic:user password'");
                        }

                        var user = strAuth.Substring(sep + 1, space - sep - 1);
                        var password = strAuth.Substring(space + 1);
                        authentication = new Authentication(new NetworkCredential(user, password, (global::System.Net.AuthenticationType)AuthenticationType.Basic));
                    }
                    else
                    {
                        throw new ArgumentException($"Authentication attribute Basic should be 'Basic:user password'");
                    }
                }
            }
            else if (strAuth.IndexOf(_apiKey) == 0)
            {
                if (strAuth.Length == _apiKey.Length)
                {
                    authentication = new Authentication(string.Empty);
                }
                else
                {
                    var sep = strAuth.IndexOf(':');
                    if (sep == _apiKey.Length)
                    {
                        var key = strAuth.Substring(sep + 1);
                        authentication = new Authentication(key);
                    }
                    else
                    {
                        throw new ArgumentException($"Authentication attribute ApiKey should be 'ApiKey:thekey'");
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Authentication attribute can only start with Basic, None or ApiKey and case sensitive");
            }
            return authentication;
        }

        #endregion

        #region Events

        /// <summary>
        /// Delegate for the CommandReceived event.
        /// </summary>
        /// <param name="obj">The source of the event.</param>
        /// <param name="e">A WebServerEventArgs that contains the event data.</param>
        public delegate void GetRequestHandler(object obj, WebServerEventArgs e);

        /// <summary>
        /// CommandReceived event is triggered when a valid command (plus parameters) is received.
        /// Valid commands are defined in the AllowedCommands property.
        /// </summary>
        public event GetRequestHandler CommandReceived;

        /// <summary>
        /// Represents the method that will handle the WebServerStatusChanged event of a WebServer.
        /// </summary>
        /// <param name="obj">The source of the event.</param>
        /// <param name="e">A WebServerStatusEventArgs that contains the event data.</param>
        public delegate void WebServerStatusHandler(object obj, WebServerStatusEventArgs e);

        /// <summary>
        /// Occurs when the status of the WebServer changes.
        /// </summary>
        public event WebServerStatusHandler WebServerStatusChanged;

        #endregion

        #region Public and private methods

        /// <summary>
        /// Start the multi threaded server.
        /// </summary>
        public bool Start()
        {
            if (_serverThread == null)
            {
                _serverThread = new Thread(StartListener);
            }

            bool bStarted = true;
#if DEBUG
            // List Ethernet interfaces, so we can determine the server's address
            ListInterfaces();
#endif
            // start server           
            try
            {
                _cancel = false;
                _serverThread.Start();
                Debug.WriteLine("Web server started on port " + Port);
                Debug.WriteLine("Started server in thread " + _serverThread.GetHashCode());
            }
            catch
            {   //if there is a problem, maybe due to the fact we did not wait enough
                _cancel = true;
                bStarted = false;
            }

            if (bStarted)
            {
                WebServerStatusChanged?.Invoke(this, new WebServerStatusEventArgs(WebServerStatus.Running));
            }

            return bStarted;
        }

        /// <summary>
        /// Stop the multi threaded server.
        /// </summary>
        public void Stop()
        {
            _cancel = true;
            Thread.Sleep(100);
            _serverThread.Abort();
            _serverThread = null;
            // Event is generate in the running thread
            Debug.WriteLine("Stopped server in thread ");
        }

        /// <summary>
        /// Output a stream
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="strResponse">the stream to output</param>
        [Obsolete("Use OutputAsStream instead. This method will be removed in a future version.")]
        public static void OutPutStream(HttpListenerResponse response, string strResponse) => OutputAsStream(response, strResponse);

        /// <summary>
        /// Output a string content as a stream.
        /// </summary>
        /// <param name="response">The response to write to.</param>
        /// <param name="content">the stream to output</param>
        public static void OutputAsStream(
            HttpListenerResponse response,
            string content)
        {
            if (response == null)
            {
                return;
            }

            byte[] messageBody = Encoding.UTF8.GetBytes(content);

            response.ContentLength64 = messageBody.Length;
            response.OutputStream.Write(
                messageBody,
                0,
                messageBody.Length);
        }

        /// <summary>
        /// Output an HTTP Code and close the connection
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="code">the http code</param>
        public static void OutputHttpCode(HttpListenerResponse response, HttpStatusCode code)
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

#if FILESYSTEM
        /// <summary>
        /// Return a file from Storage over HTTP response.
        /// </summary>
        /// <param name="response"><see cref="HttpListenerResponse"/> to send the content over.</param>
        /// <param name="strFilePath">The file to send</param>
        /// <param name="contentType">The type of file, if empty string, then will use auto detection</param>
        public static void SendFileOverHTTP(HttpListenerResponse response, string strFilePath, string contentType = "")
        {
            contentType = contentType == string.Empty ? GetContentTypeFromFileName(strFilePath.Substring(strFilePath.LastIndexOf(".") + 1)) : contentType;

            byte[] buf = new byte[MaxSizeBuffer];
            using FileStream dataReader = new FileStream(strFilePath, FileMode.Open, FileAccess.Read);

            long fileLength = dataReader.Length;
            response.ContentType = contentType;
            response.ContentLength64 = fileLength;
            response.SendChunked = false;
            // Now loops sending all the data.
            for (long bytesSent = 0; bytesSent < fileLength;)
            {
                // Determines amount of data left.
                long bytesToRead = fileLength - bytesSent;
                bytesToRead = bytesToRead < MaxSizeBuffer ? bytesToRead : MaxSizeBuffer;

                // Reads the data.
                dataReader.Read(buf, 0, (int)bytesToRead);

                // Writes data to browser
                response.OutputStream.Write(buf, 0, (int)bytesToRead);

                // Updates bytes read.
                bytesSent += bytesToRead;
            }
        }
#endif

        /// <summary>
        /// Send file content over HTTP response.
        /// </summary>
        /// <param name="response"><see cref="HttpListenerResponse"/> to send the content over.</param>
        /// <param name="fileName">Name of the file to send over <see cref="HttpListenerResponse"/>.</param>
        /// <param name="content">Content of the file to send.</param>
        /// <param name="contentType">The type of file, if empty string, then will use auto detection.</param>
        public static void SendFileOverHTTP(HttpListenerResponse response, string fileName, byte[] content, string contentType = "")
        {
            // If no extension, we will get the full file name
            contentType = contentType == "" ? GetContentTypeFromFileName(fileName.Substring(fileName.LastIndexOf('.') + 1)) : contentType;
            response.ContentType = contentType;
            response.ContentLength64 = content.Length;
            response.SendChunked = false;
            // Now loop to send all the data.

            for (long bytesSent = 0; bytesSent < content.Length;)
            {
                // Determines amount of data left
                long bytesToSend = content.Length - bytesSent;
                bytesToSend = bytesToSend < MaxSizeBuffer ? bytesToSend : MaxSizeBuffer;

                // Writes data to output stream
                response.OutputStream.Write(content, (int)bytesSent, (int)bytesToSend);

                // update bytes sent
                bytesSent += bytesToSend;
            }
        }

        private void StartListener()
        {
            try
            {
                _listener.Start();
                while (!_cancel)
                {
                    HttpListenerContext context = _listener.GetContext();
                    if (context == null)
                    {
                        return;
                    }

                    new Thread(() =>
                    {
                        //This is for handling with transitory or bad requests
                        if (context.Request.RawUrl == null)
                        {
                            return;
                        }

                        CallbackRoutes selectedRoute = null;
                        bool selectedRouteHasAuth = false;
                        string multipleCallback = null;
                        bool hasAuthRoutes = false;
                        string basicAuthNoCred = null;
                        bool authFailed = false;

                        // Variables used only within the "for". They are here for performance reasons
                        bool mustAuthenticate;
                        bool isAuthOk;

                        foreach (CallbackRoutes route in _callbackRoutes)
                        {
                            if (!IsRouteMatch(route, context.Request.HttpMethod, context.Request.RawUrl))
                            {
                                continue;
                            }

                            // Check auth first
                            mustAuthenticate = route.Authentication != null && route.Authentication.AuthenticationType != AuthenticationType.None;
                            if (mustAuthenticate)
                            {
                                hasAuthRoutes = true;
                                if (route.Authentication.AuthenticationType == AuthenticationType.Basic)
                                {
                                    var credReq = context.Request.Credentials;
                                    if (credReq is null)
                                    {
                                        if (basicAuthNoCred is null)
                                        {
                                            basicAuthNoCred = route.Route;
                                        }

                                        continue;
                                    }

                                    var credSite = route.Authentication.Credentials ?? Credential;

                                    isAuthOk = credSite != null
                                        && (credSite.UserName == credReq.UserName)
                                        && (credSite.Password == credReq.Password);
                                }
                                else if (route.Authentication.AuthenticationType == AuthenticationType.ApiKey)
                                {
                                    var apikeyReq = GetApiKeyFromHeaders(context.Request.Headers);
                                    if (apikeyReq is null)
                                    {
                                        continue;
                                    }

                                    var apikeySite = route.Authentication.ApiKey ?? ApiKey;

                                    isAuthOk = apikeyReq == apikeySite;
                                }
                                else
                                {
                                    isAuthOk = false;
                                }

                                if (isAuthOk)
                                {
                                    // This route can be used and has precedence over non-authenticated routes
                                    if (!selectedRouteHasAuth)
                                    {
                                        selectedRoute = null;
                                        multipleCallback = null;
                                    }

                                    selectedRouteHasAuth = true;
                                }
                                else
                                {
                                    authFailed = true;
                                    continue;
                                }
                            }
                            else if (selectedRouteHasAuth || authFailed)
                            {
                                // The selected route has authentication and/or a route exists with failed authentication.
                                // Those have precedence over non-authenticated routes
                                continue;
                            }

                            if (selectedRoute is null)
                            {
                                selectedRoute = route;
                            }
                            else
                            {
                                multipleCallback ??= $"Multiple matching callbacks: {selectedRoute.Callback.DeclaringType.FullName}.{selectedRoute.Callback.Name}";
                                multipleCallback += $", {route.Callback.DeclaringType.FullName}.{route.Callback.Name}";
                            }
                        }


                        if (selectedRoute is null)
                        {
                            if (hasAuthRoutes)
                            {
                                if (!authFailed && basicAuthNoCred is not null)
                                {
                                    context.Response.Headers.Add("WWW-Authenticate",
                                        $"Basic realm=\"Access to {basicAuthNoCred}\"");
                                }

                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentLength64 = 0;
                            }
                            else if (CommandReceived != null)
                            {
                                // Starting a new thread to be able to handle a new request in parallel
                                CommandReceived.Invoke(this, new WebServerEventArgs(context));
                            }
                            else
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                context.Response.ContentLength64 = 0;
                            }
                        }
                        else if (multipleCallback is not null)
                        {
                            multipleCallback += ".";
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            OutputAsStream(context.Response, multipleCallback);
                        }
                        else
                        {
                            InvokeRoute(selectedRoute, context);
                        }

                        HandleContextResponse(context);
                    }).Start();

                }

                if (_listener.IsListening)
                {
                    _listener.Stop();
                }
            }
            catch
            {
                // If we are here then set the server state to not running
                _cancel = true;
            }

            WebServerStatusChanged?.Invoke(this, new WebServerStatusEventArgs(WebServerStatus.Stopped));
        }

        /// <summary>
        /// Checks if route matches called resource.
        /// For internal use only.
        /// </summary>
        /// <param name="route">Route to check.</param>
        /// <param name="method">Invoked resource method.</param>
        /// <param name="rawUrl">Invoked resource URL.</param>
        /// <returns></returns>
        public static bool IsRouteMatch(CallbackRoutes route, string method, string rawUrl)
        {
            if (method != (string.IsNullOrEmpty(route.Method) ? DefaultRouteMethod : route.Method))
            {
                return false;
            }

            // Remove query parameters from the URL for matching
            var urlParam = rawUrl.IndexOf(ParamStart);
            var urlPath = urlParam > 0 ? rawUrl.Substring(0, urlParam) : rawUrl;

            // Normalize the URL path and route for comparison
            var urlToCompare = route.CaseSensitive ? urlPath : urlPath.ToLower();
            var routeToCompare = route.CaseSensitive ? route.Route : route.Route.ToLower();

            // Ensure both paths start with '/' for consistent segment splitting
            if (!urlToCompare.StartsWith("/"))
            {
                urlToCompare = "/" + urlToCompare;
            }

            if (!routeToCompare.StartsWith("/"))
            {
                routeToCompare = "/" + routeToCompare;
            }

            // Split into segments
            var urlSegments = urlToCompare.Split('/');
            var routeSegments = routeToCompare.Split('/');

            // Number of segments must match
            if (urlSegments.Length != routeSegments.Length)
            {
                return false;
            }

            // Compare each segment
            for (int i = 0; i < routeSegments.Length; i++)
            {
                var routeSegment = routeSegments[i];
                var urlSegment = urlSegments[i];

                // Skip empty segments (from leading slash)
                if (string.IsNullOrEmpty(routeSegment) && string.IsNullOrEmpty(urlSegment))
                {
                    continue;
                }

                // Check if this is a parameter segment (starts and ends with curly braces)
                if (routeSegment.Length > 2 &&
                    routeSegment.StartsWith("{") &&
                    routeSegment.EndsWith("}"))
                {
                    // Parameter segment matches any non-empty segment that doesn't contain '/'
                    if (string.IsNullOrEmpty(urlSegment) || urlSegment.IndexOf('/') >= 0)
                    {
                        return false;
                    }
                    // Parameter matches, continue to next segment
                    continue;
                }

                // Exact match required for non-parameter segments
                if (routeSegment != urlSegment)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method which invokes route. Can be overriden to inject custom logic.
        /// </summary>
        /// <param name="route">Current rounte to invoke. Resolved based on parameters.</param>
        /// <param name="context">Context of current request.</param>
        protected virtual void InvokeRoute(CallbackRoutes route, HttpListenerContext context)
        {
            // Extract route parameters if the route contains parameter placeholders
            var routeParameters = ExtractRouteParameters(route.Route, context.Request.RawUrl, route.CaseSensitive);

            // Create WebServerEventArgs with or without route parameters
            var eventArgs = routeParameters != null
                ? new WebServerEventArgs(context, routeParameters)
                : new WebServerEventArgs(context);

            route.Callback.Invoke(null, new object[] { eventArgs });
        }

        private static void HandleContextResponse(HttpListenerContext context)
        {
            // When context has been handed over to WebsocketServer, it will be null at this point
            // Do nothing this is a websocket that is managed by a websocketserver that is responsible for the context now. 
            if (context.Response == null)
            {
                return;
            }

            try
            {
                context.Response.Close();
            }
            catch
            {
                // Nothing on purpose
            }

            try
            {
                context.Close();
            }
            catch
            {
                // Nothing on purpose
            }
        }

        private string GetApiKeyFromHeaders(WebHeaderCollection headers)
        {
            var sec = headers.GetValues("ApiKey");
            if (sec != null
                && sec.Length > 0)
            {
                return sec[0];
            }

            return null;
        }

        /// <summary>
        /// List all IP address, useful for debug only
        /// </summary>
        private void ListInterfaces()
        {
            NetworkInterface[] ifaces = NetworkInterface.GetAllNetworkInterfaces();
            Debug.WriteLine("Number of Interfaces: " + ifaces.Length);
            foreach (NetworkInterface iface in ifaces)
            {
                Debug.WriteLine("IP:  " + iface.IPv4Address + "/" + iface.IPv4SubnetMask);
            }
        }

        /// <summary>
        /// Get the MIME-type for a file name.
        /// </summary>
        /// <param name="fileExt">File extension to get content type for.</param>
        /// <returns>The MIME-type for the file name.</returns>
        private static string GetContentTypeFromFileName(string fileExt)
        {
            // normalize to lower case to speed comparison
            fileExt = fileExt.ToLower();

            string contentType = "text/html";

            //determine the type of file for the http header
            if (fileExt == "cs" ||
                fileExt == "txt" ||
                fileExt == "csproj")
            {
                contentType = "text/plain";
            }
            else if (fileExt == "jpg" ||
                fileExt == "jpeg" ||
                fileExt == "jpe")
            {
                contentType = "image/jpeg";
            }
            else if (fileExt == "bmp" ||
                fileExt == "png" ||
                fileExt == "gif" ||
                fileExt == "ief")
            {
                contentType = $"image/{fileExt}";
            }
            else if (fileExt == "htm" ||
                fileExt == "html")
            {
                contentType = "text/html";
            }
            else if (fileExt == "mp3")
            {
                contentType = "audio/mpeg";
            }
            else if (fileExt == "css")
            {
                contentType = "text/css";
            }
            else if (fileExt == "ico")
            {
                contentType = "image/x-icon";
            }
            else if (fileExt == "zip" ||
                fileExt == "json" ||
                fileExt == "pdf")
            {
                contentType = $"application/{fileExt}";
            }

            return contentType;
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
