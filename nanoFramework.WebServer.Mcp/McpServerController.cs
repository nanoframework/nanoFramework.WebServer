// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using nanoFramework.Json;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// McpServerController class provides endpoints for handling requests related to MCP (Model Context Protocol) tools.
    /// </summary>
    public class McpServerController
    {
        /// <summary>
        /// Handles POST requests to the "mcp" route.
        /// Processes the incoming request, invokes the specified tool with provided parameters, and writes the result to the response stream in JSON format.
        /// </summary>
        /// <param name="e">The web server event arguments containing the HTTP context and request/response information.</param>
        [Route("mcp"), Method("POST")]
        public void HandleMcpRequest(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "application/json";
            int id = 0;
            StringBuilder sb = new StringBuilder();

            try
            {
                // Read the POST body from the request stream
                var requestStream = e.Context.Request.InputStream;
                byte[] buffer = new byte[requestStream.Length];
                requestStream.Read(buffer, 0, buffer.Length);
                string requestBody = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                Debug.WriteLine($"Request Body: {requestBody}");

                Hashtable request = (Hashtable)JsonConvert.DeserializeObject(requestBody, typeof(Hashtable));

//#if DEBUG
//                foreach (string key in request.Keys)
//                {
//                    Debug.WriteLine($"Key: {key}, Value: {request[key]}");
//                }
//#endif                

                // Sets jsonrpc version
                sb.Append("{\"jsonrpc\": \"2.0\"");
                // Check if we have an id if yes, add it to the answer
                if (request.ContainsKey("id"))
                {
                    id = Convert.ToInt32(request["id"].ToString());
                    sb.Append($",\"id\":{id}");
                }

                if (request.ContainsKey("method"))
                {
                    // Case the server us initilaized
                    if (request["method"].ToString() == "notifications/initialized")
                    {
                        WebServer.OutputHttpCode(e.Context.Response, System.Net.HttpStatusCode.OK);
                        return;
                    }

                    if (request["method"].ToString() == "initialize")
                    {
                        // TODO: check the received version and adjust with error message or set this version
                        sb.Append(",\"result\":{\"protocolVersion\":\"2025-03-26\"");

                        // Add capabilities
                        sb.Append($",\"capabilities\":{{\"logging\":{{}},\"prompts\":{{\"listChanged\":false}},\"resources\":{{\"subscribe\":false,\"listChanged\":false}},\"tools\":{{\"listChanged\":false}}}}");

                        // Add serverInfo
                        sb.Append($",\"serverInfo\":{{\"name\":\"nanoFramework\",\"version\":\"1.0.0\"}}");

                        // Add instructions
                        sb.Append($",\"instructions\":\"This is an embedded device and only 1 request at a time should be sent. Make sure you pass numbers as numbers and not as string.\"}}}}");
                        //sb.Append("\r\n\r\n{\"jsonrpc\":\"2.0\",\"method\":\"notifications/initialized\"}");
                        WebServer.OutPutStream(e.Context.Response, sb.ToString());
                        return;
                    }

                    if (request["method"].ToString() == "tools/list")
                    {
                        // This is a request for the list of tools
                        string toolListJson = McpToolRegistry.GetToolMetadataJson();
                        sb.Append($",\"result\":{{{toolListJson}}}}}");
                        WebServer.OutPutStream(e.Context.Response, sb.ToString());
                        return;
                    }

                    // Check if the method starts with "tools/call" and extract the method name to call the tool
                    if (request["method"].ToString() == "tools/call")
                    {
                        //string param = JsonConvert.SerializeObject(((Hashtable)request["params"])["arguments"]);
                        string toolName = ((Hashtable)request["params"])["name"].ToString();
                        Hashtable param = ((Hashtable)request["params"])["arguments"] == null ? null : (Hashtable)((Hashtable)request["params"])["arguments"];

                        string result = McpToolRegistry.InvokeTool(toolName, param);

                        //Debug.WriteLine($"Tool: {toolName}, Param: {param}");

                        sb.Append($",\"result\":{{\"content\":[{{\"type\":\"text\",\"text\":{JsonConvert.SerializeObject(result)}}}]}}}}");
                        WebServer.OutPutStream(e.Context.Response, sb.ToString());
                        return;
                    }
                    else
                    {
                        sb.Append($",\"error\":{{\"code\":-32601,\"message\":\"Method not found\"}}}}");
                        WebServer.OutPutStream(e.Context.Response, sb.ToString());
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                WebServer.OutPutStream(e.Context.Response, $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"error\":{{\"code\":-32602,\"message\":\"{ex.Message}\"}}}}");
            }
        }

        /// <summary>
        /// Handles GET requests to the "tools" route.
        /// Returns a JSON list of all available tools and their metadata.
        /// </summary>
        /// <param name="e">The web server event arguments containing the HTTP context and request/response information.</param>
        [Route("tools"), Method("GET")]
        public void GetToolList(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "application/json";
            WebServer.OutputHttpCode(e.Context.Response, System.Net.HttpStatusCode.NotFound);
            return;

            try
            {
                Debug.WriteLine($"GET request for tool list received: {e.Context.Request.RawUrl}");
                string toolListJson = McpToolRegistry.GetToolMetadataJson();
                WebServer.OutPutStream(e.Context.Response, $"{{\"jsonrpc\":\"2.0\",\"result\":{{{toolListJson}}}}}");
            }
            catch (Exception ex)
            {
                WebServer.OutPutStream(e.Context.Response, $"{{\"jsonrpc\":\"2.0\",\"error\":{{\"code\":-32602,\"message\":\"{ex.Message}\"}}}}");
            }

            // Free up the memory used
            Runtime.Native.GC.Run(true);
        }

        /// <summary>
        /// Handles GET requests to the "mcp" route.
        /// Returns the same tool list as <see cref="GetToolList"/>.
        /// </summary>
        /// <param name="e">The web server event arguments containing the HTTP context and request/response information.</param>
        [Route("mcp"), Method("GET")]
        public void GetToolMcpList(WebServerEventArgs e) => GetToolList(e);
    }
}
