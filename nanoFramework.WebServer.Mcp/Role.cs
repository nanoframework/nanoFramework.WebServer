// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.WebServer.Mcp
{
    /// <summary>
    /// Represents the type of role in the Model Context Protocol conversation.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Corresponds to a human user in the conversation.
        /// </summary>
        User,

        /// <summary>
        /// Corresponds to the AI assistant in the conversation.
        /// </summary>
        Assistant
    }
}
