//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Htp Error Code
    /// </summary>
    public enum HttpCode
    {
        /// <summary>Continue</summary>
        Continue = 100,

        /// <summary>OK</summary>
        OK = 200,

        /// <summary>Created</summary>
        Created = 201,

        /// <summary>Accepted</summary>
        Accepted = 202,

        /// <summary>Bad Request</summary>
        BadRequest = 400,

        /// <summary>Unauthorized</summary>
        Unauthorized = 401,

        /// <summary>Forbidden</summary>
        Forbidden = 403,

        /// <summary>Not Found</summary>
        NotFound = 404,

        /// <summary>Method Not Allowed</summary>
        MethodNotAllowed = 405,

        /// <summary>Not Accepted</summary>
        NotAccepted = 406,

        /// <summary>Request Timeout</summary>
        RequestTimeout = 408,

        /// <summary>Conflict</summary>
        Conflict = 409,

        /// <summary>Internal Server Error</summary>
        InternalServerError = 500,

        /// <summary>Not Implemented</summary>
        NotImplemented = 501,

        /// <summary>Service Unavailable</summary>
        ServiceUnavailable = 503,
    }
}
