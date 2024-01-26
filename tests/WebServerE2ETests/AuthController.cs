// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.

using nanoFramework.WebServer;
using System.Net;

namespace WebServerE2ETests
{
    [Authentication("Basic")]
    class AuthController
    {
        [Route("authbasic")]
        public void Basic(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Route("authbasicspecial")]
        [Authentication("Basic:user2 password")]
        public void Special(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Authentication("ApiKey:superKey1234")]
        [Route("authapi")]
        public void Key(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Route("authnone")]
        [Authentication("None")]
        public void None(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Authentication("ApiKey")]
        [Route("authdefaultapi")]
        public void DefaultApi(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }
    }
}
