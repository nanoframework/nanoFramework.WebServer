//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

namespace nanoFramework.WebServer.Sample
{
    public class ControllerTest
    {
        [Route("test")]
        [Method("GET")]
        public void RoutePostTest(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpCode.OK);
        }

        [Route("test/any")]
        public void RouteAnyTest(WebServerEventArgs e)
        {
            WebServer.OutputHttpCode(e.Context.Response, HttpCode.OK);
        }
    }
}
