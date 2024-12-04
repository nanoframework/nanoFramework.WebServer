﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;

namespace nanoFramework.WebServer.HttpMultipartParser
{
    internal class LineBuffer : IDisposable
    {
        private readonly Hashtable _data = new();

        public void Dispose() => _data.Clear();

        public void Write(byte[] bytes, int offset, int count)
        {
            byte[] chunk = new byte[count];

            Array.Copy(bytes, offset, chunk, 0, count);
            
            _data.Add(_data.Count, chunk);
            Length += count;
        }

        public int Length { get; private set; } = 0;

        public byte[] ToArray(bool clear = false)
        {
            byte[] result = new byte[Length];
            int pos = 0;

            for (int i = 0; i < _data.Count; i++)
            {
                object data = _data[i];

                if (data is byte b)
                {
                    result[pos++] = b;
                }
                else if (data is byte[] array)
                {
                    Array.Copy(array, 0, result, pos, array.Length);
                    pos += array.Length;
                }
                _data[i] = null;
            }

            if (clear)
            {
                Clear();
            }

            return result;
        }

        public void Clear()
        {
            _data.Clear();
            Length = 0;
        }
    }
}
