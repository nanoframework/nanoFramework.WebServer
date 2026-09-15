// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using nanoFramework.Json;

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Registry for Model Context Protocol (MCP) resources, allowing discovery and reading of resources defined with the <see cref="McpServerResourceAttribute"/>.
    /// </summary>
    public class McpResourceRegistry : RegistryBase
    {
        private static readonly Hashtable resources = new Hashtable();
        private static bool isInitialized = false;

        /// <summary>
        /// Discovers MCP resources by scanning the provided types for methods decorated with the <see cref="McpServerResourceAttribute"/>.
        /// This method should be called once to populate the resource registry.
        /// </summary>
        /// <param name="mcpResources">An array of types to scan for MCP resources.</param>
        public static void DiscoverResources(Type[] mcpResources)
        {
            if (isInitialized)
            {
                // Resources already discovered
                return;
            }

            foreach (Type mcpResource in mcpResources)
            {
                RegisterResources(mcpResource, null, null);
            }

            isInitialized = true;
        }

        /// <summary>
        /// Discovers MCP resources by scanning the provided object instances for methods decorated with the <see cref="McpServerResourceAttribute"/>.
        /// Instance methods are invoked against the object they were discovered on, allowing live device objects to expose resources.
        /// This overload is additive and can be called in addition to the static <see cref="DiscoverResources(Type[])"/> discovery.
        /// </summary>
        /// <param name="resourceInstances">An array of object instances to scan for MCP resources.</param>
        public static void DiscoverResources(object[] resourceInstances)
        {
            DiscoverResources(resourceInstances, null);
        }

        /// <summary>
        /// Discovers MCP resources on the provided object instances, prepending an optional per-instance URI prefix to every
        /// discovered resource URI so that several instances of the same type can be registered without URI collisions.
        /// This overload is additive and can be called in addition to the static <see cref="DiscoverResources(Type[])"/> discovery.
        /// </summary>
        /// <param name="resourceInstances">An array of object instances to scan for MCP resources.</param>
        /// <param name="uriPrefixes">An optional parallel array of URI prefixes; a null or empty entry means no prefix. Each prefix must produce an absolute RFC 3986 URI when prepended to the resource URI (for example, <c>instance-</c> with <c>sensor://reading</c>). May be null to apply no prefixes.</param>
        public static void DiscoverResources(object[] resourceInstances, string[] uriPrefixes)
        {
            if (resourceInstances == null)
            {
                return;
            }

            for (int i = 0; i < resourceInstances.Length; i++)
            {
                object instance = resourceInstances[i];
                if (instance == null)
                {
                    continue;
                }

                string prefix = (uriPrefixes != null && i < uriPrefixes.Length) ? uriPrefixes[i] : null;
                RegisterResources(instance.GetType(), instance, prefix);
            }
        }

        /// <summary>
        /// Scans a type for methods decorated with the McpServerResourceAttribute and registers each as a resource.
        /// </summary>
        /// <param name="mcpResource">The type to scan.</param>
        /// <param name="target">The instance to invoke discovered instance methods against, or null for static-only discovery.</param>
        /// <param name="uriPrefix">An optional prefix prepended to every discovered resource URI; null or empty means no prefix. The resulting resource URI must be an absolute RFC 3986 URI.</param>
        private static void RegisterResources(Type mcpResource, object target, string uriPrefix)
        {
            MethodInfo[] methods = mcpResource.GetMethods();
            foreach (MethodInfo method in methods)
            {
                try
                {
                    var allAttribute = method.GetCustomAttributes(true);
                    foreach (var attrib in allAttribute)
                    {
                        if (attrib.GetType() != typeof(McpServerResourceAttribute))
                        {
                            continue;
                        }

                        McpServerResourceAttribute attribute = (McpServerResourceAttribute)attrib;
                        if (attribute != null)
                        {
                            // Skip instance methods when no target instance is available; they cannot be invoked.
                            if (target == null && !method.IsStatic)
                            {
                                continue;
                            }

                            // Resources are no-argument getters. Their returned value is represented as text.
                            if (method.GetParameters().Length > 0)
                            {
                                continue;
                            }

                            Uri resourceUri = CombineResourceUri(uriPrefix, attribute.Uri);

                            resources.Add(resourceUri.AbsoluteUri, new ResourceMetadata
                            {
                                Uri = resourceUri,
                                Name = attribute.Name,
                                Description = attribute.Description,
                                MimeType = IsSimpleResourceType(method.ReturnType) ?
                                    (string.IsNullOrEmpty(attribute.MimeType) ? "text/plain" : attribute.MimeType) :
                                    "application/json",
                                Method = method,
                                Target = (target != null && !method.IsStatic) ? target : null,
                            });
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Gets the metadata of all registered MCP resources in JSON format.
        /// This method should be called after DiscoverResources to retrieve the resource metadata.
        /// </summary>
        /// <returns>A JSON string containing the metadata of all registered resources.</returns>
        /// <exception cref="Exception">Thrown if there is an error building the resources list.</exception>
        public static string GetResourceMetadataJson()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"resources\":[");

                foreach (ResourceMetadata resource in resources.Values)
                {
                    sb.Append(resource.ToString());
                    sb.Append(",");
                }

                if (resources.Count > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                sb.Append("]");
                return sb.ToString();
            }
            catch (Exception)
            {
                throw new Exception("Impossible to build resources list.");
            }
        }

        /// <summary>
        /// Reads a registered MCP resource by URI and returns the serialized result.
        /// </summary>
        /// <param name="uri">The URI of the resource to read.</param>
        /// <returns>A JSON string containing the serialized contents of the resource.</returns>
        /// <exception cref="Exception">Thrown when the specified resource is not found in the registry.</exception>
        public static string ReadResource(string uri)
        {
            Uri resourceUri;

            try
            {
                resourceUri = new Uri(uri, UriKind.Absolute);
            }
            catch (Exception)
            {
                throw new ResourceNotFoundException(uri);
            }

            if (resources.Contains(resourceUri.AbsoluteUri))
            {
                ResourceMetadata resourceMetadata = (ResourceMetadata)resources[resourceUri.AbsoluteUri];
                MethodInfo method = resourceMetadata.Method;
                Debug.WriteLine($"Resource uri: {resourceUri.AbsoluteUri}, method: {method.Name}");

                object result = method.Invoke(resourceMetadata.Target, null);

                bool isSimpleResult = result == null || IsSimpleResourceType(result.GetType());
                string text = isSimpleResult ?
                    JsonConvert.SerializeObject(result == null ? string.Empty : result.ToString()) :
                    JsonConvert.SerializeObject(JsonConvert.SerializeObject(result));

                StringBuilder sb = new StringBuilder();
                sb.Append($"{{\"contents\":[{{\"uri\":{JsonConvert.SerializeObject(resourceMetadata.Uri.AbsoluteUri)},\"mimeType\":{JsonConvert.SerializeObject(resourceMetadata.MimeType)},\"text\":{text}}}]}}");
                return sb.ToString();
            }

            throw new ResourceNotFoundException(uri);
        }

        /// <summary>
        /// Represents a request for a resource that is not registered.
        /// </summary>
        public class ResourceNotFoundException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
            /// </summary>
            /// <param name="uri">The URI that was not registered.</param>
            public ResourceNotFoundException(string uri)
                : base("Resource not found")
            {
                Uri = uri;
            }

            /// <summary>
            /// Gets the URI that was not registered.
            /// </summary>
            public string Uri { get; }
        }

        private static Uri CombineResourceUri(string uriPrefix, Uri uri)
        {
            if (string.IsNullOrEmpty(uriPrefix))
            {
                return uri;
            }

            StringBuilder builder = new StringBuilder(uriPrefix.Length + uri.OriginalString.Length);
            builder.Append(uriPrefix);
            builder.Append(uri.OriginalString);
            return new Uri(builder.ToString(), UriKind.Absolute);
        }

        private static bool IsSimpleResourceType(Type type)
        {
            return McpToolJsonHelper.IsPrimitiveType(type) || type == typeof(string);
        }
    }
}
