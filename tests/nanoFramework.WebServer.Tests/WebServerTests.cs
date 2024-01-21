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
        public void IsRouteMatch_Should_ReturnFalseForEmptyMethod()
        {
            // Arrange
            var route = new CallbackRoutes()
            {
                Method = "GET",
                Route = "/api/test"
            };

            // Act
            var result = WebServer.IsRouteMatch(route, "", "/api/test");

            // Assert
            Assert.IsFalse(result);
        }
        
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
        [DataRow("GET", "/api/test", "/api/test")]
        [DataRow("POST", "/api/test", "/api/test")]
        [DataRow("PUT", "/api/test", "/api/test")]
        [DataRow("PATCH", "/api/test", "/api/test")]
        [DataRow("DELETE", "/api/test", "/api/test")]
        [DataRow("GET", "/API/TEST", "/api/test")]
        [DataRow("POST", "/API/TEST", "/api/test")]
        [DataRow("PUT", "/API/TEST", "/api/test")]
        [DataRow("PATCH", "/API/TEST", "/api/test")]
        [DataRow("DELETE", "/API/TEST", "/api/test")]
        [DataRow("GET", "/api/test", "/API/TEST")]
        [DataRow("POST", "/api/test", "/api/test")]
        [DataRow("PUT", "/api/test", "/API/TEST")]
        [DataRow("PATCH", "/api/test", "/API/TEST")]
        [DataRow("DELETE", "/api/test", "/API/TEST")]
        [DataRow("GET", "/api/test", "/api/test?id=1234")]
        [DataRow("GET", "/api/test", "/api/test?id=")]
        [DataRow("GET", "/api/test/resource/name", "/api/test/resource/name")]
        [DataRow("GET", "/api/test/resource/name", "/api/test/resource/name?id=1234")]
        [DataRow("GET", "/api/test/resource/name", "/api/test/resource/name?test=")]
        [DataRow("GET", "/api/test/resource/name", "/api/test/resource/name?")]
        [DataRow("GET", "/api/test/resource/name", "/api/test/resource/name?test=&id=123&app=something")]
        public void IsRouteMatch_Should_ReturnTrueForMatchingMethodAndRoute(string method, string url, string invokedUrl)
        {
            Console.WriteLine(invokedUrl);
            // Arrange
            var route = new CallbackRoutes()
            {
                Method = method,
                Route = url,
                CaseSensitive = false
            };

            // Act
            var result = WebServer.IsRouteMatch(route, method, invokedUrl);

            // Assert
            Assert.IsTrue(result);
        }

        // TODO: Case sensitive tests
    }
}
