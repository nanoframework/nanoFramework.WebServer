// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace nanoFramework.WebServer.HttpMultipartParser
{
    internal static class HashtableUtility
    {
        public static bool TryGetValue(this Hashtable hashtable, string key, out string value) 
        {
            if (hashtable != null && hashtable.Contains(key))
            {
                var obj = hashtable[key];
                value = obj == null ? string.Empty : obj.ToString();
                return true;
            }

            value = null;
            return false;
        }
    }
}
