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

namespace DotNetty.Codecs.Http.Multipart
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DefaultHttpDataFactory : IHttpDataFactory
    {
        // Proposed default MINSIZE as 16 KB.
        public static readonly long MinSize = 0x4000;

        // Proposed default MAXSIZE = -1 as UNLIMITED
        public static readonly long MaxSize = -1;

        private readonly bool _useDisk;
        private readonly bool _checkSize;
        private readonly long _minSize;
        private long _maxSize = MaxSize;
        private readonly Encoding _charset = HttpConstants.DefaultEncoding;

        private string _baseDir;
        private bool _deleteOnExit; // false is a good default cause true leaks

        // Keep all HttpDatas until cleanAllHttpData() is called.
        private readonly ConcurrentDictionary<IHttpRequest, List<IHttpData>> _requestFileDeleteMap =
            new ConcurrentDictionary<IHttpRequest, List<IHttpData>>(IdentityComparer.Default);

        // HttpData will be in memory if less than default size (16KB).
        // The type will be Mixed.
        public DefaultHttpDataFactory()
        {
            _useDisk = false;
            _checkSize = true;
            _minSize = MinSize;
        }

        internal DefaultHttpDataFactory(Encoding charset) : this()
        {
            _charset = charset;
        }

        // HttpData will be always on Disk if useDisk is True, else always in Memory if False
        public DefaultHttpDataFactory(bool useDisk)
        {
            _useDisk = useDisk;
            _checkSize = false;
        }

        internal DefaultHttpDataFactory(bool useDisk, Encoding charset) : this(useDisk)
        {
            _charset = charset;
        }

        public DefaultHttpDataFactory(long minSize)
        {
            _useDisk = false;
            _checkSize = true;
            _minSize = minSize;
        }

        internal DefaultHttpDataFactory(long minSize, Encoding charset) : this(minSize)
        {
            _charset = charset;
        }

        /// <summary>
        /// Override global <see cref="DiskAttribute.BaseDirectory"/> and <see cref="DiskFileUpload.BaseDirectory"/> values.
        /// </summary>
        /// <param name="baseDir">directory path where to store disk attributes and file uploads.</param>
        public void SetBaseDir(string baseDir)
        {
            _baseDir = baseDir;
        }

        /// <summary>
        /// Override global <see cref="DiskAttribute.DeleteOnExitTemporaryFile"/> and
        /// <see cref="DiskFileUpload.DeleteOnExitTemporaryFile"/> values.
        /// </summary>
        /// <param name="deleteOnExit"><c>true</c> if temporary files should be deleted with the JVM, false otherwise.</param>
        public void SetDeleteOnExit(bool deleteOnExit)
        {
            _deleteOnExit = deleteOnExit;
        }

        public void SetMaxLimit(long max) => _maxSize = max;

        List<IHttpData> GetList(IHttpRequest request)
        {
            List<IHttpData> list = _requestFileDeleteMap.GetOrAdd(request, _ => new List<IHttpData>());
            return list;
        }

        public IAttribute CreateAttribute(IHttpRequest request, string name)
        {
            if (_useDisk)
            {
                var diskAttribute = new DiskAttribute(name, _charset, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                List<IHttpData> list = GetList(request);
                list.Add(diskAttribute);
                return diskAttribute;
            }
            if (_checkSize)
            {
                var mixedAttribute = new MixedAttribute(name, _minSize, _charset, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                List<IHttpData> list = GetList(request);
                list.Add(mixedAttribute);
                return mixedAttribute;
            }
            var attribute = new MemoryAttribute(name)
            {
                MaxSize = _maxSize
            };
            return attribute;
        }

        public IAttribute CreateAttribute(IHttpRequest request, string name, long definedSize)
        {
            if (_useDisk)
            {
                var diskAttribute = new DiskAttribute(name, definedSize, _charset, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                List<IHttpData> list = GetList(request);
                list.Add(diskAttribute);
                return diskAttribute;
            }
            if (_checkSize)
            {
                var mixedAttribute = new MixedAttribute(name, definedSize, _minSize, _charset, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                List<IHttpData> list = GetList(request);
                list.Add(mixedAttribute);
                return mixedAttribute;
            }
            var attribute = new MemoryAttribute(name, definedSize)
            {
                MaxSize = _maxSize
            };
            return attribute;
        }

        static void CheckHttpDataSize(IHttpData data)
        {
            try
            {
                data.CheckSize(data.Length);
            }
            catch (IOException)
            {
                ThrowHelper.ThrowArgumentException_AttrBigger();
            }
        }

        public IAttribute CreateAttribute(IHttpRequest request, string name, string value)
        {
            if (_useDisk)
            {
                IAttribute attribute;
                try
                {
                    attribute = new DiskAttribute(name, value, _charset, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };
                }
                catch (IOException)
                {
                    // revert to Mixed mode
                    attribute = new MixedAttribute(name, value, _minSize, _charset, _baseDir, _deleteOnExit)
                    {
                        MaxSize = _maxSize
                    };
                }
                CheckHttpDataSize(attribute);
                List<IHttpData> list = GetList(request);
                list.Add(attribute);
                return attribute;
            }
            if (_checkSize)
            {
                var mixedAttribute = new MixedAttribute(name, value, _minSize, _charset, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                CheckHttpDataSize(mixedAttribute);
                List<IHttpData> list = GetList(request);
                list.Add(mixedAttribute);
                return mixedAttribute;
            }
            try
            {
                var attribute = new MemoryAttribute(name, value, _charset)
                {
                    MaxSize = _maxSize
                };
                CheckHttpDataSize(attribute);
                return attribute;
            }
            catch (IOException e)
            {
                throw new ArgumentException($"({request}, {name}, {value})", e);
            }
        }

        public IFileUpload CreateFileUpload(IHttpRequest request, string name, string fileName,
            string contentType, string contentTransferEncoding, Encoding encoding,
            long size)
        {
            if (_useDisk)
            {
                var fileUpload = new DiskFileUpload(name, fileName, contentType,
                    contentTransferEncoding, encoding, size, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                CheckHttpDataSize(fileUpload);
                List<IHttpData> list = GetList(request);
                list.Add(fileUpload);
                return fileUpload;
            }
            if (_checkSize)
            {
                var fileUpload = new MixedFileUpload(name, fileName, contentType,
                    contentTransferEncoding, encoding, size, _minSize, _baseDir, _deleteOnExit)
                {
                    MaxSize = _maxSize
                };
                CheckHttpDataSize(fileUpload);
                List<IHttpData> list = GetList(request);
                list.Add(fileUpload);
                return fileUpload;
            }
            var memoryFileUpload = new MemoryFileUpload(name, fileName, contentType,
                contentTransferEncoding, encoding, size)
            {
                MaxSize = _maxSize
            };
            CheckHttpDataSize(memoryFileUpload);
            return memoryFileUpload;
        }

        public void RemoveHttpDataFromClean(IHttpRequest request, IInterfaceHttpData data)
        {
            if (!(data is IHttpData httpData))
            {
                return;
            }

            // Do not use getList because it adds empty list to requestFileDeleteMap
            // if request is not found
            if (!_requestFileDeleteMap.TryGetValue(request, out List<IHttpData> list))
            {
                return;
            }

            // Can't simply call list.remove(data), because different data items may be equal.
            // Need to check identity.
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], httpData))
                {
                    index = i;
                    break;
                }
            }
            if ((uint)index < (uint)list.Count) // index != -1
            {
                list.RemoveAt(index);
            }
            if (0u >= (uint)list.Count)
            {
                _ = _requestFileDeleteMap.TryRemove(request, out _);
            }
        }

        public void CleanRequestHttpData(IHttpRequest request)
        {
            if (_requestFileDeleteMap.TryRemove(request, out List<IHttpData> list))
            {
                foreach (IHttpData data in list)
                {
                    _ = data.Release();
                }
            }
        }

        public void CleanAllHttpData()
        {
            while (!_requestFileDeleteMap.IsEmpty)
            {
                IHttpRequest[] keys = _requestFileDeleteMap.Keys.ToArray();
                foreach (IHttpRequest key in keys)
                {
                    if (_requestFileDeleteMap.TryRemove(key, out List<IHttpData> list))
                    {
                        foreach (IHttpData data in list)
                        {
                            _ = data.Release();
                        }
                    }
                }
            }
        }

        // Similar to IdentityHashMap in Java
        sealed class IdentityComparer : IEqualityComparer<IHttpRequest>
        {
            internal static readonly IdentityComparer Default = new IdentityComparer();

            public bool Equals(IHttpRequest x, IHttpRequest y) => ReferenceEquals(x, y);

            public int GetHashCode(IHttpRequest obj) => obj.GetHashCode();
        }
    }
}
