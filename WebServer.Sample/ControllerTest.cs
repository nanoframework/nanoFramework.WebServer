using nanoFramework.WebServer;
using System;
using System.Text;

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
