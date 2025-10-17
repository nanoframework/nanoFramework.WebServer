//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Net;
using nanoFramework.TestFramework;

namespace nanoFramework.WebServer.Tests
{
    [TestClass]
    public class WebServerTests
    {
        [TestMethod]
        public void IsRouteMatch_Should_ReturnFalseForNotMatchingMethod()
        {
            // Arrange
            var route = new CallbackRoutes()
            {
                Method = "GET",
                Route = "/api/test"
            };

            // Act
            var result = WebServer.IsRouteMatch(route, "POST", "/api/test");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DataRow("GET", "/api/test", "GET", "/api/test")]
        [DataRow("", "/api/test", "GET", "/api/test")]
        [DataRow("POST", "/api/test", "POST", "/api/test")]
        [DataRow("PUT", "/api/test", "PUT", "/api/test")]
        [DataRow("PATCH", "/api/test", "PATCH", "/api/test")]
        [DataRow("DELETE", "/api/test", "DELETE", "/api/test")]
        [DataRow("GET", "/API/TEST", "GET", "/api/test")]
        [DataRow("POST", "/API/TEST", "POST", "/api/test")]
        [DataRow("PUT", "/API/TEST", "PUT", "/api/test")]
        [DataRow("PATCH", "/API/TEST", "PATCH", "/api/test")]
        [DataRow("DELETE", "/API/TEST", "DELETE", "/api/test")]
        [DataRow("GET", "/api/test", "GET", "/API/TEST")]
        [DataRow("POST", "/api/test", "POST", "/api/test")]
        [DataRow("PUT", "/api/test", "PUT", "/API/TEST")]
        [DataRow("PATCH", "/api/test", "PATCH", "/API/TEST")]
        [DataRow("DELETE", "/api/test", "DELETE", "/API/TEST")]
        [DataRow("GET", "/api/test", "GET", "/api/test?id=1234")]
        [DataRow("GET", "/api/test", "GET", "/api/test?id=")]
        [DataRow("GET", "/api/test/resource/name", "GET", "/api/test/resource/name")]
        [DataRow("GET", "/api/test/resource/name", "GET", "/api/test/resource/name?id=1234")]
        [DataRow("GET", "/api/test/resource/name", "GET", "/api/test/resource/name?test=")]
        [DataRow("GET", "/api/test/resource/name", "GET", "/api/test/resource/name?")]
        [DataRow("GET", "/api/test/resource/name", "GET", "/api/test/resource/name?test=&id=123&app=something")]
        [DataRow("", "", "GET", "/")]
        public void IsRouteMatch_Should_ReturnTrueForMatchingMethodAndRoute(string routeMethod, string routeUrl, string invokedMethod, string invokedUrl)
        {
            Console.WriteLine($"Params: routeMethod: {routeMethod} routeUrl: {routeUrl} invokedMethod: {invokedMethod} invokedUrl: {invokedUrl}");
            // Arrange
            var route = new CallbackRoutes()
            {
                Method = routeMethod,
                Route = routeUrl,
                CaseSensitive = false
            };

            // Act
            var result = WebServer.IsRouteMatch(route, invokedMethod, invokedUrl);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsRouteMatch_Should_ReturnTrueForMatchingMethodAndRouteCaseSensitive()
        {
            // Arrange
            var routeMethod = "POST";
            var routeUrl = "/api/test";
            var invokedMethod = "POST";
            var invokedUrlMatch = "/api/test";
            var invokedUrlNotMatch = "/API/TEST";
            var route = new CallbackRoutes()
            {
                Method = routeMethod,
                Route = routeUrl,
                CaseSensitive = true
            };

            // Act
            var resultMatch = WebServer.IsRouteMatch(route, invokedMethod, invokedUrlMatch);
            var resultNotMatch = WebServer.IsRouteMatch(route, invokedMethod, invokedUrlNotMatch);

            // Assert
            Assert.IsTrue(resultMatch);
            Assert.IsFalse(resultNotMatch);
        }

        [TestMethod]
        [DataRow("/api/devices/{id}", "/api/devices/123", true)]
        [DataRow("/api/devices/{id}", "/api/devices/device123", true)]
        [DataRow("/api/devices/{id}", "/api/devices/123abc", true)]
        [DataRow("/api/devices/{id}/actions", "/api/devices/123/actions", true)]
        [DataRow("/api/devices/{deviceId}/sensors/{sensorId}", "/api/devices/123/sensors/456", true)]
        [DataRow("/api/devices/{id}", "/api/devices/", false)]
        [DataRow("/api/devices/{id}", "/api/devices/123/456", false)]
        [DataRow("/api/devices/{id}", "/api/devices", false)]
        [DataRow("/api/devices/{id}", "/api/different/123", false)]
        [DataRow("/api/devices/{id}/actions", "/api/devices/123", false)]
        [DataRow("/api/devices/{deviceId}/sensors/{sensorId}", "/api/devices/123/sensors", false)]
        public void IsRouteMatch_Should_HandleParameterizedRoutes(string routeTemplate, string requestUrl, bool shouldMatch)
        {
            // Arrange
            var route = new CallbackRoutes()
            {
                Method = "GET",
                Route = routeTemplate,
                CaseSensitive = false
            };

            // Act
            var result = WebServer.IsRouteMatch(route, "GET", requestUrl);

            // Assert
            if (shouldMatch)
            {
                Assert.IsTrue(result, $"Route '{routeTemplate}' should match URL '{requestUrl}'");
            }
            else
            {
                Assert.IsFalse(result, $"Route '{routeTemplate}' should not match URL '{requestUrl}'");
            }
        }

        [TestMethod]
        [DataRow("/api/devices/{id}", "/api/devices/123", "id", "123")]
        [DataRow("/api/devices/{deviceId}/sensors/{sensorId}", "/api/devices/mydevice/sensors/mysensor", "deviceId", "mydevice")]
        [DataRow("/api/devices/{deviceId}/sensors/{sensorId}", "/api/devices/mydevice/sensors/mysensor", "sensorId", "mysensor")]
        [DataRow("/users/{userId}/posts/{postId}/comments", "/users/john/posts/100/comments", "userId", "john")]
        [DataRow("/users/{userId}/posts/{postId}/comments", "/users/john/posts/100/comments", "postId", "100")]
        public void ExtractRouteParameters_Should_ExtractParameterValues(string routeTemplate, string requestUrl, string paramName, string expectedValue)
        {
            // Act
            var parameters = WebServer.ExtractRouteParameters(routeTemplate, requestUrl, false);

            // Assert
            Assert.IsNotNull(parameters, "Route parameters should not be null");
            
            string actualValue = null;
            foreach (UrlParameter param in parameters)
            {
                if (param.Name.ToLower() == paramName.ToLower())
                {
                    actualValue = param.Value;
                    break;
                }
            }
            
            Assert.AreEqual(expectedValue, actualValue, $"Parameter '{paramName}' should have value '{expectedValue}'");
        }

        [TestMethod]
        public void ExtractRouteParameters_Should_ReturnNullForNonMatchingRoute()
        {
            // Act
            var parameters = WebServer.ExtractRouteParameters("/api/devices/{id}", "/api/users/123", false);

            // Assert
            Assert.IsNull(parameters, "Should return null for non-matching routes");
        }

        [TestMethod]
        public void ExtractRouteParameters_Should_ReturnNullForEmptyInputs()
        {
            // Act
            var parameters1 = WebServer.ExtractRouteParameters("", "/api/test", false);
            var parameters2 = WebServer.ExtractRouteParameters("/api/test", "", false);
            var parameters3 = WebServer.ExtractRouteParameters(null, "/api/test", false);
            var parameters4 = WebServer.ExtractRouteParameters("/api/test", null, false);

            // Assert
            Assert.IsNull(parameters1, "Should return null for empty route template");
            Assert.IsNull(parameters2, "Should return null for empty URL");
            Assert.IsNull(parameters3, "Should return null for null route template");
            Assert.IsNull(parameters4, "Should return null for null URL");
        }

        [TestMethod]
        public void ExtractRouteParameters_Should_HandleQueryParameters()
        {
            // Arrange
            var routeTemplate = "/api/devices/{id}";
            var requestUrl = "/api/devices/123?filter=active&sort=name";

            // Act
            var parameters = WebServer.ExtractRouteParameters(routeTemplate, requestUrl, false);

            // Assert
            Assert.IsNotNull(parameters, "Route parameters should not be null");
            Assert.AreEqual(1, parameters.Length, "Should have exactly one route parameter");
            Assert.AreEqual("id", parameters[0].Name, "Parameter name should be 'id'");
            Assert.AreEqual("123", parameters[0].Value, "Parameter value should be '123'");
        }

        [TestMethod]
        public void WebServerConstructors()
        {
            // Test basic constructor with port and protocol only (HTTP)
            var httpServer = new WebServer(8080, HttpProtocol.Http);
            Assert.AreEqual(8080, httpServer.Port);
            Assert.AreEqual(HttpProtocol.Http, httpServer.Protocol);
            httpServer.Dispose();

            // Test basic constructor with port and protocol only (HTTPS)  
            var httpsServer = new WebServer(8443, HttpProtocol.Https);
            Assert.AreEqual(8443, httpsServer.Port);
            Assert.AreEqual(HttpProtocol.Https, httpsServer.Protocol);
            httpsServer.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_WithIPAddress_HTTP()
        {
            // Test constructor with IP address for HTTP
            var address = IPAddress.Parse("127.0.0.1");
            var server = new WebServer(8080, HttpProtocol.Http, address);

            Assert.AreEqual(8080, server.Port);
            Assert.AreEqual(HttpProtocol.Http, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_WithIPAddress_HTTPS()
        {
            // Test constructor with IP address for HTTPS
            var address = IPAddress.Parse("192.168.1.100");
            var server = new WebServer(8443, HttpProtocol.Https, address);

            Assert.AreEqual(8443, server.Port);
            Assert.AreEqual(HttpProtocol.Https, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_WithoutIPAddress()
        {
            // Test constructor without (should bind to default interface)
            var server = new WebServer(9000, HttpProtocol.Http);

            Assert.AreEqual(9000, server.Port);
            Assert.AreEqual(HttpProtocol.Http, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_WithControllers_HTTP()
        {
            // Test constructor with controllers for HTTP
            var controllers = new Type[] { typeof(TestController) };
            var server = new WebServer(8080, HttpProtocol.Http, controllers);

            Assert.AreEqual(8080, server.Port);
            Assert.AreEqual(HttpProtocol.Http, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_WithControllers_HTTPS()
        {
            // Test constructor with controllers for HTTPS
            var controllers = new Type[] { typeof(TestController), typeof(AnotherTestController) };
            var server = new WebServer(8443, HttpProtocol.Https, controllers);

            Assert.AreEqual(8443, server.Port);
            Assert.AreEqual(HttpProtocol.Https, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_WithEmptyControllers()
        {
            // Test constructor with empty controllers array
            var controllers = new Type[0];
            var server = new WebServer(9002, HttpProtocol.Http, controllers);

            Assert.AreEqual(9002, server.Port);
            Assert.AreEqual(HttpProtocol.Http, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_FullConstructor_HTTP()
        {
            // Test full constructor with IP address and controllers for HTTP
            var address = IPAddress.Parse("10.0.0.1");
            var controllers = new Type[] { typeof(TestController) };
            var server = new WebServer(8080, HttpProtocol.Http, address, controllers);

            Assert.AreEqual(8080, server.Port);
            Assert.AreEqual(HttpProtocol.Http, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_FullConstructor_HTTPS()
        {
            // Test full constructor with IP address and controllers for HTTPS
            var address = IPAddress.Parse("172.16.0.1");
            var controllers = new Type[] { typeof(TestController), typeof(AnotherTestController) };
            var server = new WebServer(8443, HttpProtocol.Https, address, controllers);

            Assert.AreEqual(8443, server.Port);
            Assert.AreEqual(HttpProtocol.Https, server.Protocol);
            server.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_IsRunningProperty()
        {
            // Test IsRunning property is false initially for all constructor variations
            var server1 = new WebServer(8080, HttpProtocol.Http);
            var server2 = new WebServer(8081, HttpProtocol.Http, IPAddress.Parse("127.0.0.1"));
            var server3 = new WebServer(8082, HttpProtocol.Http, new Type[] { typeof(TestController) });
            var server4 = new WebServer(8083, HttpProtocol.Http, IPAddress.Parse("127.0.0.1"), new Type[] { typeof(TestController) });

            Assert.IsFalse(server1.IsRunning);
            Assert.IsFalse(server2.IsRunning);
            Assert.IsFalse(server3.IsRunning);
            Assert.IsFalse(server4.IsRunning);

            server1.Dispose();
            server2.Dispose();
            server3.Dispose();
            server4.Dispose();
        }

        [TestMethod]
        public void WebServerConstructor_DifferentPortNumbers()
        {
            // Test various port numbers across different constructor overloads
            var server1 = new WebServer(80, HttpProtocol.Http);
            var server2 = new WebServer(443, HttpProtocol.Https, IPAddress.Parse("127.0.0.1"));
            var server3 = new WebServer(3000, HttpProtocol.Http, new Type[] { typeof(TestController) });
            var server4 = new WebServer(5000, HttpProtocol.Https, IPAddress.Parse("127.0.0.1"), new Type[] { typeof(TestController) });

            Assert.AreEqual(80, server1.Port);
            Assert.AreEqual(443, server2.Port);
            Assert.AreEqual(3000, server3.Port);
            Assert.AreEqual(5000, server4.Port);

            server1.Dispose();
            server2.Dispose();
            server3.Dispose();
            server4.Dispose();
        }
    }

    // Test controller classes for constructor tests
    internal class TestController
    {
        [Route("test")]
        public void TestMethod(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Test response");
        }
    }

    internal class AnotherTestController
    {
        [Route("another")]
        public void AnotherMethod(WebServerEventArgs e)
        {
            WebServer.OutputAsStream(e.Context.Response, "Another response");
        }
    }
}
