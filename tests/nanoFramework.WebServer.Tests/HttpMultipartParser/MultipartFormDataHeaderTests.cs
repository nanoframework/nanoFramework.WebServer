// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;
using nanoFramework.WebServer.HttpMultipartParser;
using System.Collections;

namespace nanoFramework.WebServer.Tests
{
    [TestClass]
    public class MultipartFormDataHeaderTests
    {
        [TestMethod]
        public void FormPartHeaderTest()
        {
            Hashtable headers = new();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"paramname\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "paramname");

            headers.Clear();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"param;name\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "param;name");

            headers.Clear();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"param=name\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "param=name");

            headers.Clear();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"param:name\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "param:name");

            headers.Clear();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"param name\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "param name");
        }

        [TestMethod]
        public void FilePartHeaderTest()
        {
            Hashtable headers = new();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"file\"; filename=\"somefile.ext\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "file");
            ValidateHeaders(headers, "filename", "somefile.ext");

            headers.Clear();
            HeaderUtility.ParseHeaders("Content-Disposition: form-data; name=\"f i;l=e\"; filename=\";some=fi-le.ext :\"", headers);
            ValidateHeaders(headers, "Content-Disposition", "form-data");
            ValidateHeaders(headers, "name", "f i;l=e");
            ValidateHeaders(headers, "filename", ";some=fi-le.ext :");
        }

        private void ValidateHeaders(Hashtable headers, string key, string value)
        {
            Assert.IsNotNull(headers);
            Assert.IsTrue(headers.Contains(key.ToLower()));
            Assert.AreSame(headers[key.ToLower()], value);
        }
    }
}
