using nanoFramework.TestFramework;
using System;

namespace nanoFramework.WebServer.Tests
{
    [TestClass]
    public class WebServerTests
    {
        // IsRouteMatch GET, POST, PUT, DELETE, PATCH
        // IsRouteMatch CaseSensitive true/false

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
        public void IsRouteMatch_Should_ReturnTrueForMatchingMethodAndRoute(string method, string url, string invokedUrl)
        {
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
    }
}
