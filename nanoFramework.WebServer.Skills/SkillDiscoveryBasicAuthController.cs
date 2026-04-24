// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.WebServer;

namespace nanoFramework.WebServer.Skills
{
    /// <summary>
    /// Skill discovery controller with basic (user, password) authentication.
    /// </summary>
    [Authentication("Basic")]
    public class SkillDiscoveryBasicAuthController : SkillDiscoveryController
    {
    }
}
