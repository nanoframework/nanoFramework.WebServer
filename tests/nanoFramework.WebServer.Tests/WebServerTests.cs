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
    }
}
