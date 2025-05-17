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
 * Copyright (c) The DotNetty Project (Microsoft). All rights reserved.
 *
 *   https://github.com/azure/dotnetty
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */


namespace DotNetty.Transport.Channels
{
    using System.IO;
    using DotNetty.Common;

    public interface IFileRegion : IReferenceCounted
    {
        /// <summary>Returns the offset in the file where the transfer began.</summary>
        long Position { get; }

        /// <summary>Returns the bytes which was transferred already.</summary>
        long Transferred { get; }

        /// <summary>Returns the number of bytes to transfer.</summary>
        long Count { get; }

        /// <summary>Transfers the content of this file region to the specified channel.</summary>
        /// <param name="target">the destination of the transfer</param>
        /// <param name="position">the relative offset of the file where the transfer
        /// begins from.  For example, <tt>0</tt> will make the
        /// transfer start from <see cref="Position"/>th byte and
        /// <tt><see cref="Count"/> - 1</tt> will make the last
        /// byte of the region transferred.</param>
        /// <returns></returns>
        long TransferTo(Stream target, long position);
    }
}
