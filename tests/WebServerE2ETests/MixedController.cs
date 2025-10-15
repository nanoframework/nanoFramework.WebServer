// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.WebServer;

namespace WebServerE2ETests
{
    class MixedController
    {
        #region ApiKey + public
        [Route("authapikeyandpublic")]
        [Authentication("ApiKey:superKey1234")]
        public void ApiKeyAndPublicApiKey(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Public: ApiKey");
        }

        [Route("authapikeyandpublic")]
        public void ApiKeyAndPublicPublic(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Public: Public");
        }
        #endregion

        #region Basic + public
        [Route("authbasicandpublic")]
        [Authentication("Basic:user2 password")]
        public void BasicAndPublicBasic(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Basic+Public: Basic");
        }

        [Route("authbasicandpublic")]
        public void BasicAndPublicPublic(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Basic+Public: Public");
        }
        #endregion

        #region Basic + ApiKey + Public
        [Route("authapikeybasicandpublic")]
        public void ApiKeyBasicAndPublicPublic(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Basic+Public: Public");
        }

        [Route("authapikeybasicandpublic")]
        [Authentication("Basic:user3 password")]
        public void ApiKeyBasicAndPublicBasic3(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Basic+Public: Basic user3");
        }

        [Route("authapikeybasicandpublic")]
        [Authentication("Basic:user2 password")]
        public void ApiKeyBasicAndPublicBasic2(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Basic+Public: Basic user2");
        }

        [Authentication("ApiKey:superKey1234")]
        [Route("authapikeybasicandpublic")]
        public void ApiKeyBasicAndPublicApiKey(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Basic+Public: ApiKey");
        }

        [Authentication("ApiKey:superKey42")]
        [Route("authapikeybasicandpublic")]
        public void ApiKeyBasicAndPublicApiKey2(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "ApiKey+Basic+Public: ApiKey 2");
        }
        #endregion

        #region Multiple callbacks
        [Route("authmultiple")]
        public void MultiplePublic1(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Multiple: Public1");
        }

        [Route("authmultiple")]
        [Authentication("Basic:user2 password")]
        public void MultipleBasic1(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Multiple: Basic1");
        }

        [Route("authmultiple")]
        public void MultiplePublic2(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Multiple: Public2");
        }

        [Authentication("ApiKey:superKey1234")]
        [Route("authmultiple")]
        public void MultipleApiKey1(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Multiple: ApiKey1");
        }

        [Route("authmultiple")]
        [Authentication("Basic:user2 password")]
        public void MultipleBasic2(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Multiple: Basic2");
        }

        [Authentication("ApiKey:superKey1234")]
        [Route("authmultiple")]
        public void MultipleApiKey2(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Multiple: ApiKey2");
        }
        #endregion
    }
}
