// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace nanoFramework.WebServer.HttpMultipartParser
{
    /// <summary>Provides methods to read a stream line by line while still returning the bytes.</summary>
    internal class LineReader
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer;
        private readonly LineBuffer _lineBuffer = new();
        int _availableBytes = -1;
        int _position = 0;

        /// <summary>Initializes a new instance of the <see cref="LineReader"/> class.</summary>
        /// <param name="stream">The input stream to read from.</param>
        /// <param name="bufferSize">The buffer size to use for new buffers.</param>
        public LineReader(Stream stream, int bufferSize)
        {
            _stream = stream;
            _buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Reads a line from the stack delimited by the newline for this platform.
        /// The newline characters will not be included in the stream.
        /// </summary>
        /// <param name="excludeNewLine">
        /// If true the newline characters will be stripped when the line returns.
        /// When reading binary data, newline characters are meaningfull and should be returned
        /// </param>
        /// <returns>The byte[] containing the line or null if end of stream.</returns>
        public byte[] ReadByteLine(bool excludeNewLine = true)
        {
            while (ReadFromStream() > 0)
            {
                for (int i = _position; i < _availableBytes; i++)
                {
                    if (_buffer[i] == '\n')
                    {
                        // newline found, time to return the line
                        int length = GetLength(excludeNewLine, i);

                        byte[] line = GetLine(length);

                        _position = i + 1;

                        return line;
                    }
                }

                // if we get here, no newline found in current buffer
                // store what we have left in the buffer into the lineBuffer
                _lineBuffer.Write(_buffer, _position, _availableBytes - _position);
                _position = _availableBytes;
            }

#pragma warning disable S1168 // null and empty do have meaning
            // no more bytes available, return what's in the lineBuffer
            // if lineBuffer is empty, we're truly done, return null!
            return _lineBuffer.Length == 0 ? null : _lineBuffer.ToArray(true);
#pragma warning restore S1168
        }

        private byte[] GetLine(int length)
        {
            byte[] line;
            if (_lineBuffer.Length > 0)
            {
                _lineBuffer.Write(_buffer, _position, length);
                line = _lineBuffer.ToArray(true);
            }
            else
            {
                line = new byte[length];
                Array.Copy(_buffer, _position, line, 0, length);
            }

            return line;
        }

        private int GetLength(bool excludeNewLine, int currentPosition)
        {
            int length = currentPosition - _position + 1;

            if (excludeNewLine)
            {
                length -= currentPosition > 0 && _buffer[currentPosition - 1] == '\r' ? 2 : 1;
            }

            return length;
        }

        private int ReadFromStream()
        {
            if (_position >= _availableBytes)
            {
                // The stream is (should be) a NetworkStream which might still be receiving data while
                // we're already processing. Give the stream a chance to receive more data or we might
                // end up with "zero bytes read" too soon...
                Thread.Sleep(1);

                long streamLength = _stream.Length;

                if (streamLength > _buffer.Length)
                {
                    streamLength = _buffer.Length;
                }

                _availableBytes = _stream.Read(_buffer, 0, (int)streamLength);
                _position = 0;
            }

            return _availableBytes;
        }

        /// <summary>
        /// Reads a line from the stack delimited by the newline for this platform.
        /// The newline characters will not be included in the stream.
        /// </summary>
        /// <returns>The <see cref="string" /> containing the line or null if end of stream.</returns>
        public string ReadLine()
        {
            byte[] data = ReadByteLine();
            return data == null ? null : Encoding.UTF8.GetString(data, 0, data.Length);
        }
    }
}
