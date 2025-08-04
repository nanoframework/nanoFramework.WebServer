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
        /// The supported version of the MCP protocol.
        /// </summary>
        public const string SupportedVersion = "2025-03-26";

        /// <summary>
        /// Gets or sets the server name.
        /// </summary>
        public static string ServerName { get; set; } = "nanoFramework";

        /// <summary>
        /// Gets or sets the server version.
        /// </summary>
        public static string ServerVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the instructions for using the MCP server.
        /// </summary>
        public static string Instructions { get; set; } = "This is an embedded device and only 1 request at a time should be sent.";

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
                        // Check if client sent params with protocolVersion
                        if (request.ContainsKey("params") && request["params"] is Hashtable initParams)
                        {
                            if (initParams.ContainsKey("protocolVersion"))
                            {
                                string clientVersion = initParams["protocolVersion"].ToString();
                                if (clientVersion != SupportedVersion)
                                {
                                    sb.Append($",\"error\":{{\"code\":-32602,\"message\":\"Unsupported protocol version\",\"data\":{{\"supported\":[\"{SupportedVersion}\"],\"requested\":\"{clientVersion}\"}}}}}}");
                                    WebServer.OutPutStream(e.Context.Response, sb.ToString());
                                    return;
                                }
                            }
                        }

                        sb.Append($",\"result\":{{\"protocolVersion\":\"{SupportedVersion}\"");

                        // Add capabilities
                        sb.Append($",\"capabilities\":{{\"logging\":{{}},\"prompts\":{{\"listChanged\":false}},\"resources\":{{\"subscribe\":false,\"listChanged\":false}},\"tools\":{{\"listChanged\":false}}}}");

                        // Add serverInfo
                        sb.Append($",\"serverInfo\":{{\"name\":\"{ServerName}\",\"version\":\"{ServerVersion}\"}}");

                        // Add instructions
                        sb.Append($",\"instructions\":\"{Instructions}\"}}}}");
                    }
                    else if (request["method"].ToString() == "tools/list")
                    {
                        // This is a request for the list of tools
                        string toolListJson = McpToolRegistry.GetToolMetadataJson();
                        sb.Append($",\"result\":{{{toolListJson}}}}}");
                    }
                    else if (request["method"].ToString() == "tools/call")
                    {
                        string toolName = ((Hashtable)request["params"])["name"].ToString();
                        Hashtable param = ((Hashtable)request["params"])["arguments"] == null ? null : (Hashtable)((Hashtable)request["params"])["arguments"];

                        string result = McpToolRegistry.InvokeTool(toolName, param);

                        sb.Append($",\"result\":{{\"content\":[{{\"type\":\"text\",\"text\":{result}}}]}}}}");
                    }
                    else if (request["method"].ToString() == "prompts/list")
                    {
                        string promptListJson = McpPromptRegistry.GetPromptMetadataJson();
                        sb.Append($",\"result\":{{{promptListJson}}}}}");
                    }
                    else if (request["method"].ToString() == "prompts/get")
                    {
                        string promptName = ((Hashtable)request["params"])["name"].ToString();
                        Hashtable arguments = ((Hashtable)request["params"])["arguments"] == null ? null : (Hashtable)((Hashtable)request["params"])["arguments"];

                        string result = McpPromptRegistry.InvokePrompt(promptName, arguments);
                        sb.Append($",\"result\":{result}}}");
                    }
                    else
                    {
                        sb.Append($",\"error\":{{\"code\":-32601,\"message\":\"Method not found\"}}}}");
                    }

                    Debug.WriteLine();
                    Debug.WriteLine($"Response: {sb.ToString()}");
                    Debug.WriteLine();

                    WebServer.OutPutStream(e.Context.Response, sb.ToString());
                    return;
                }
            }
            catch (Exception ex)
            {
                WebServer.OutPutStream(e.Context.Response, $"{{\"jsonrpc\":\"2.0\",\"id\":{id},\"error\":{{\"code\":-32602,\"message\":\"{ex.Message}\"}}}}");
            }
        }
    }
}
