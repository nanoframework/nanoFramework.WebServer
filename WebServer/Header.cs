//
// Copyright (c) 2020 Laurent Ellerbach and the project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Text;

namespace nanoFramework.WebServer
{
    /// <summary>
    /// Header class
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Name of the header
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of the header
        /// </summary>
        public string Value { get; set; }
    }
}
