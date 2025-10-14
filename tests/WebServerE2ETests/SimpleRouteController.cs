// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Net;
using nanoFramework.WebServer;

namespace WebServerE2ETests
{
    internal class SimpleRouteController
    {
        [Route("okcode")]
        public void OutputWithOKCode(WebServerEventArgs e)
        {
            Debug.WriteLine($"{nameof(OutputWithOKCode)} {e.Context.Request.HttpMethod} {e.Context.Request.RawUrl}");
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Route("notfoundcode")]
        public void OutputWithNotFoundCode(WebServerEventArgs e)
        {
            Debug.WriteLine($"{nameof(OutputWithNotFoundCode)} {e.Context.Request.HttpMethod} {e.Context.Request.RawUrl}");
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
        }

        [Route("oktext")]
        public void OutputWithOKText(WebServerEventArgs e)
        {
            Debug.WriteLine($"{nameof(OutputWithOKText)} {e.Context.Request.HttpMethod} {e.Context.Request.RawUrl}");
            WebServer.OutputAsStream(e.Context.Response, "OK");
        }

        [Route("test"), Route("Test2"), Route("tEst42"), Route("TEST")]
        [CaseSensitive]
        [Method("GET")]
        public void RouteGetTest(WebServerEventArgs e)
        {
            string route = $"The route asked is {e.Context.Request.RawUrl.TrimStart('/').Split('/')[0]}";
            e.Context.Response.ContentType = "text/plain";
            WebServer.OutputAsStream(e.Context.Response, route);
        }

        [Route("test/any")]
        public void RouteAnyTest(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Route("multiplecallback")]
        public void FirstOfMultipleCallback(WebServerEventArgs e)
        {
            Debug.WriteLine($"{nameof(FirstOfMultipleCallback)} {e.Context.Request.HttpMethod} {e.Context.Request.RawUrl}");
            WebServer.OutputAsStream(e.Context.Response, nameof(FirstOfMultipleCallback));
        }

        [Route("multiplecallback")]
        public void SecondOfMultipleCallback(WebServerEventArgs e)
        {
            Debug.WriteLine($"{nameof(SecondOfMultipleCallback)} {e.Context.Request.HttpMethod} {e.Context.Request.RawUrl}");
            WebServer.OutputAsStream(e.Context.Response, nameof(SecondOfMultipleCallback));
        }
    }
}
