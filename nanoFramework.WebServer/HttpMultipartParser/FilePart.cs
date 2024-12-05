// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.IO;

namespace nanoFramework.WebServer.HttpMultipartParser
{
    /// <summary>Represents a single file extracted from a multipart/form-data stream.</summary>
    public class FilePart
    {
        /// <summary>Initializes a new instance of the <see cref="FilePart" /> class.</summary>
        /// <param name="name">The name of the input field used for the upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="data">The file data.</param>
        /// <param name="additionalProperties">Additional properties associated with this file.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="contentDisposition">The content disposition.</param>
        public FilePart(string name, string fileName, Stream data, Hashtable additionalProperties, string contentType, string contentDisposition)
        {
            string[] parts = fileName?.Split(GetInvalidFileNameChars());

            Name = name;
            FileName = parts != null && parts.Length > 0 ? parts[parts.Length - 1] : string.Empty;
            Data = data;
            ContentType = contentType;
            ContentDisposition = contentDisposition;
            AdditionalProperties = additionalProperties;
        }

        /// <summary>Gets the data.</summary>
        public Stream Data
        {
            get;
        }

        /// <summary>Gets the file name.</summary>
        public string FileName
        {
            get;
        }

        /// <summary>Gets the name.</summary>
        public string Name
        {
            get;
        }

        /// <summary>Gets the content-type. Defaults to text/plain if unspecified.</summary>
        public string ContentType
        {
            get;
        }

        /// <summary>Gets the content-disposition. Defaults to form-data if unspecified.</summary>
        public string ContentDisposition
        {
            get;
        }

        /// <summary>
        /// Gets the additional properties associated with this file.
        /// An additional property is any property other than the "well known" ones such as name, filename, content-type, etc.
        /// </summary>
        public Hashtable AdditionalProperties
        {
            get;
            private set;
        }

        private static char[] GetInvalidFileNameChars() => new char[]
        {
            '\"', '<', '>', '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '\\', '/'
        };
    }
}
