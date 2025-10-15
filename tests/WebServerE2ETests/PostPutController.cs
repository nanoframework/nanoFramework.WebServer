// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net;
using nanoFramework.WebServer;

namespace WebServerE2ETests
{
    class PostPutController
    {
        [Method("POST")]
        [Route("post")]
        public void Post(WebServerEventArgs e)
        {
            byte[] buff = new byte[e.Context.Request.InputStream.Length];
            e.Context.Request.InputStream.Read(buff, 0, buff.Length);
            var txt = e.Context.Request.ContentType.Contains("text") ? System.Text.Encoding.UTF8.GetString(buff, 0, buff.Length) : BitConverter.ToString(buff);
            WebServer.OutputAsStream(e.Context.Response, $"POST: {txt}");
        }

        [Method("PUT")]
        [Route("put")]
        public void Put(WebServerEventArgs e)
        {
            byte[] buff = new byte[e.Context.Request.InputStream.Length];
            e.Context.Request.InputStream.Read(buff, 0, buff.Length);
            var txt = e.Context.Request.ContentType.Contains("text") ? System.Text.Encoding.UTF8.GetString(buff, 0, buff.Length) : BitConverter.ToString(buff);
            WebServer.OutputAsStream(e.Context.Response, $"PUT: {txt}");
        }

    }
}
