// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System.Collections;

namespace nanoFramework.WebServer.Mcp
{
    internal static class HashtableExtension
    {
        public static bool ContainsKey(this Hashtable hashtable, string key)
        {
            foreach (object k in hashtable.Keys)
            {
                if (k is string strKey && strKey.Equals(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
