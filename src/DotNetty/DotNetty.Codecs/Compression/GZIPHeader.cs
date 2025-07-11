/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) Microsoft. All rights reserved.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

//JZlib 0.0.* were released under the GNU LGPL license.Later, we have switched
//over to a BSD-style license. 

//------------------------------------------------------------------------------
//Copyright (c) 2000-2011 ymnk, JCraft, Inc.All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:

//  1. Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.

//  2. Redistributions in binary form must reproduce the above copyright
//     notice, this list of conditions and the following disclaimer in 
//     the documentation and/or other materials provided with the distribution.

//  3. The names of the authors may not be used to endorse or promote products
//     derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
//INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL JCRAFT,
//INC.OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
//INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
//OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING
//NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
//EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// ReSharper disable ArrangeThisQualifier
// ReSharper disable InconsistentNaming
namespace DotNetty.Codecs.Compression
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// https://github.com/ymnk/jzlib/blob/master/src/main/java/com/jcraft/jzlib/GZIPHeader.java
    /// 
    /// http://www.ietf.org/rfc/rfc1952.txt
    /// </summary>
    sealed class GZIPHeader
    {
        static readonly Encoding ISOEncoding = Encoding.GetEncoding("ISO-8859-1");
        static readonly byte Platform;

        public const byte OS_MSDOS = (byte)0x00;
        public const byte OS_AMIGA = (byte)0x01;
        public const byte OS_VMS = (byte)0x02;
        public const byte OS_UNIX = (byte)0x03;
        public const byte OS_ATARI = (byte)0x05;
        public const byte OS_OS2 = (byte)0x06;
        public const byte OS_MACOS = (byte)0x07;
        public const byte OS_TOPS20 = (byte)0x0a;
        public const byte OS_WIN32 = (byte)0x0b;
        public const byte OS_VMCMS = (byte)0x04;
        public const byte OS_ZSYSTEM = (byte)0x08;
        public const byte OS_CPM = (byte)0x09;
        public const byte OS_QDOS = (byte)0x0c;
        public const byte OS_RISCOS = (byte)0x0d;
        public const byte OS_UNKNOWN = (byte)0xff;

        bool text = false;
        bool fhcrc = false;
        internal long time;
        internal int xflags;
        internal int os;
        internal byte[] extra;
        internal byte[] name;
        internal byte[] comment;
        internal int hcrc;
        internal long crc;
        //bool done = false;
        long mtime = 0;

        static GZIPHeader()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Platform = OS_WIN32;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Platform = OS_UNIX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Platform = OS_MACOS;
            }
            else
            {
                Platform = OS_UNKNOWN;
            }
        }

        internal GZIPHeader()
        {
            this.os = Platform;
        }

        public void SetModifiedTime(long value) => this.mtime = value;

        public long GetModifiedTime() => this.mtime;

        public void SetOS(int value)
        {
            if ((0 <= value && value <= 13) || value == 255)
                this.os = value;
            else
                throw new ArgumentException(nameof(value));
        }

        public int GetOS() => this.os;

        public void SetName(string value) => this.name = ISOEncoding.GetBytes(value);

        public string GetName() => this.name is null ? string.Empty : ISOEncoding.GetString(this.name);

        public void SetComment(string value) => this.comment = ISOEncoding.GetBytes(value);

        public string GetComment() => this.comment is null ? string.Empty : ISOEncoding.GetString(this.comment);

        public void SetCRC(long value) => this.crc = value;

        public long GetCRC() => this.crc;

        internal void Put(Deflate d)
        {
            int flag = 0;
            if (text)
            {
                flag |= 1;     // FTEXT
            }
            if (fhcrc)
            {
                flag |= 2;     // FHCRC
            }
            if (extra is object)
            {
                flag |= 4;     // FEXTRA
            }
            if (name is object)
            {
                flag |= 8;    // FNAME
            }
            if (comment is object)
            {
                flag |= 16;   // FCOMMENT
            }
            int xfl = 0;
            if (d.level == JZlib.Z_BEST_SPEED)
            {
                xfl |= 4;
            }
            else if (d.level == JZlib.Z_BEST_COMPRESSION)
            {
                xfl |= 2;
            }

            d.Put_short(unchecked((short)0x8b1f));  // ID1 ID2
            d.Put_byte((byte)8);         // CM(Compression Method)
            d.Put_byte((byte)flag);
            d.Put_byte((byte)mtime);
            d.Put_byte((byte)(mtime >> 8));
            d.Put_byte((byte)(mtime >> 16));
            d.Put_byte((byte)(mtime >> 24));
            d.Put_byte((byte)xfl);
            d.Put_byte((byte)os);

            if (extra is object)
            {
                d.Put_byte((byte)extra.Length);
                d.Put_byte((byte)(extra.Length >> 8));
                d.Put_byte(extra, 0, extra.Length);
            }

            if (name is object)
            {
                d.Put_byte(name, 0, name.Length);
                d.Put_byte((byte)0);
            }

            if (comment is object)
            {
                d.Put_byte(comment, 0, comment.Length);
                d.Put_byte((byte)0);
            }
        }

        public GZIPHeader Clone()
        {
            var gheader = new GZIPHeader();
            byte[] tmp;
            if (gheader.extra is object)
            {
                tmp = new byte[gheader.extra.Length];
                Array.Copy(gheader.extra, 0, tmp, 0, tmp.Length);
                gheader.extra = tmp;
            }

            if (gheader.name is object)
            {
                tmp = new byte[gheader.name.Length];
                Array.Copy(gheader.name, 0, tmp, 0, tmp.Length);
                gheader.name = tmp;
            }

            if (gheader.comment is object)
            {
                tmp = new byte[gheader.comment.Length];
                Array.Copy(gheader.comment, 0, tmp, 0, tmp.Length);
                gheader.comment = tmp;
            }

            return gheader;
        }
    }
}
