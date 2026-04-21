// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using nanoFramework.Json;
using nanoFramework.WebServer;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Controller providing A2A-compatible skill discovery and invocation endpoints.
    /// Serves the Agent Card at .well-known/agent-card.json and skill invocation at skills/invoke.
    /// </summary>
    public class SkillDiscoveryController
    {
        /// <summary>
        /// Gets or sets the agent name displayed in the Agent Card.
        /// </summary>
        public static string AgentName { get; set; } = "nanoFramework";

        /// <summary>
        /// Gets or sets the agent description displayed in the Agent Card.
        /// </summary>
        public static string AgentDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the agent version displayed in the Agent Card.
        /// </summary>
        public static string AgentVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets the agent URL for the Agent Card.
        /// </summary>
        public static string AgentUrl { get; set; } = string.Empty;

        /// <summary>
        /// Handles GET requests to .well-known/agent-card.json.
        /// Returns an A2A-compatible Agent Card with the registered skills.
        /// Supports optional query parameters: ?skill=id to filter by skill, ?tag=value to filter by tag.
        /// </summary>
        /// <param name="e">The web server event arguments.</param>
        [Route(".well-known/agent-card.json"), Method("GET")]
        public void GetAgentCard(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "application/json";

            try
            {
                // Check for query parameters
                string skillFilter = null;
                string tagFilter = null;
                string rawUrl = e.Context.Request.RawUrl;
                int paramIndex = rawUrl.IndexOf('?');
                if (paramIndex > 0)
                {
                    string queryString = rawUrl.Substring(paramIndex + 1);
                    string[] pairs = queryString.Split('&');
                    foreach (string pair in pairs)
                    {
                        int eqIndex = pair.IndexOf('=');
                        if (eqIndex > 0)
                        {
                            string key = pair.Substring(0, eqIndex);
                            string value = pair.Substring(eqIndex + 1);
                            if (key == "skill")
                            {
                                skillFilter = value;
                            }
                            else if (key == "tag")
                            {
                                tagFilter = value;
                            }
                        }
                    }
                }

                StringBuilder sb = new StringBuilder();
                sb.Append("{\"name\":\"");
                sb.Append(SkillJsonHelper.EscapeJsonString(AgentName));
                sb.Append("\"");

                if (!string.IsNullOrEmpty(AgentDescription))
                {
                    sb.Append(",\"description\":\"");
                    sb.Append(SkillJsonHelper.EscapeJsonString(AgentDescription));
                    sb.Append("\"");
                }

                sb.Append(",\"version\":\"");
                sb.Append(SkillJsonHelper.EscapeJsonString(AgentVersion));
                sb.Append("\"");

                if (!string.IsNullOrEmpty(AgentUrl))
                {
                    sb.Append(",\"url\":\"");
                    sb.Append(SkillJsonHelper.EscapeJsonString(AgentUrl));
                    sb.Append("\"");
                }

                // Add skills
                sb.Append(",\"skills\":");
                if (skillFilter != null)
                {
                    string skillJson = SkillRegistry.GetSkillJson(skillFilter);
                    if (skillJson != null)
                    {
                        sb.Append("[");
                        sb.Append(skillJson);
                        sb.Append("]");
                    }
                    else
                    {
                        sb.Append("[]");
                    }
                }
                else if (tagFilter != null)
                {
                    sb.Append(SkillRegistry.GetSkillsByTagJson(tagFilter));
                }
                else
                {
                    sb.Append(SkillRegistry.GetSkillsArrayJson());
                }

                sb.Append("}");

                Debug.WriteLine($"Agent Card response: {sb}");
                WebServer.OutputAsStream(e.Context.Response, sb.ToString());
            }
            catch (Exception ex)
            {
                WebServer.OutputAsStream(e.Context.Response,
                    "{\"error\":{\"code\":-1,\"message\":\"" + SkillJsonHelper.EscapeJsonString(ex.Message) + "\"}}");
            }
        }

        /// <summary>
        /// Handles GET requests to the skills endpoint.
        /// Returns a lightweight JSON response with just the skills array.
        /// </summary>
        /// <param name="e">The web server event arguments.</param>
        [Route("skills"), Method("GET")]
        public void ListSkills(WebServerEventArgs e)
        {
            e.Context.Response.ContentType = "application/json";

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"skills\":");
                sb.Append(SkillRegistry.GetSkillsArrayJson());
                sb.Append("}");

                WebServer.OutputAsStream(e.Context.Response, sb.ToString());
            }
            catch (Exception ex)
            {
                WebServer.OutputAsStream(e.Context.Response,
                    "{\"error\":{\"code\":-1,\"message\":\"" + SkillJsonHelper.EscapeJsonString(ex.Message) + "\"}}");
            }
        }

        /// <summary>
        /// Handles POST requests to skills/invoke.
        /// Invokes a skill action with the provided arguments.
        /// Request body: { "skill": "skill-id", "action": "ActionName", "arguments": { ... } }
        /// Response content type matches the action's declared ContentType.
        /// </summary>
        /// <param name="e">The web server event arguments.</param>
        [Route("skills/invoke"), Method("POST")]
        public void InvokeSkillAction(WebServerEventArgs e)
        {
            try
            {
                // Read the POST body
                var requestStream = e.Context.Request.InputStream;
                byte[] buffer = new byte[requestStream.Length];
                requestStream.Read(buffer, 0, buffer.Length);
                string requestBody = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                Debug.WriteLine($"Skill invoke request: {requestBody}");

                Hashtable request = (Hashtable)JsonConvert.DeserializeObject(requestBody, typeof(Hashtable));

                if (!request.Contains("skill") || !request.Contains("action"))
                {
                    e.Context.Response.ContentType = "application/json";
                    WebServer.OutputAsStream(e.Context.Response,
                        "{\"error\":{\"code\":-3,\"message\":\"Missing 'skill' or 'action' field\"}}");
                    return;
                }

                string skillId = request["skill"].ToString();
                string actionName = request["action"].ToString();
                Hashtable arguments = request.Contains("arguments") && request["arguments"] != null
                    ? (Hashtable)request["arguments"]
                    : null;

                // Get the content type for the action before invoking
                string contentType = SkillRegistry.GetActionContentType(skillId, actionName);
                if (contentType == null)
                {
                    e.Context.Response.ContentType = "application/json";
                    WebServer.OutputAsStream(e.Context.Response,
                        "{\"error\":{\"code\":-2,\"message\":\"Skill or action not found\"}}");
                    return;
                }

                string result = SkillRegistry.InvokeAction(skillId, actionName, arguments);

                Debug.WriteLine($"Skill invoke result (contentType: {contentType}): {result}");

                // For text-based content types, return raw content
                if (contentType == "text/markdown" || contentType == "text/plain")
                {
                    e.Context.Response.ContentType = contentType;
                    WebServer.OutputAsStream(e.Context.Response, result);
                }
                else
                {
                    // For JSON, wrap in result envelope
                    e.Context.Response.ContentType = "application/json";
                    WebServer.OutputAsStream(e.Context.Response, "{\"result\":" + result + "}");
                }
            }
            catch (Exception ex)
            {
                e.Context.Response.ContentType = "application/json";
                WebServer.OutputAsStream(e.Context.Response,
                    "{\"error\":{\"code\":-1,\"message\":\"" + SkillJsonHelper.EscapeJsonString(ex.Message) + "\"}}");
            }
        }
    }
}
