//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;

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
    }
}
