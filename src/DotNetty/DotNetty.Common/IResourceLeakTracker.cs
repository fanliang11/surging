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

namespace DotNetty.Common
{
    public interface IResourceLeakTracker
    {
        /// <summary>
        /// Records the caller's current stack trace so that the <see cref="ResourceLeakDetector"/> can tell where the leaked
        /// resource was accessed lastly. This method is a shortcut to <see cref="Record(object)"/> with <c>null</c> as an argument..
        /// </summary>
        void Record();

        /// <summary>
        /// Records the caller's current stack trace and the specified additional arbitrary information
        /// so that the <see cref="ResourceLeakDetector"/> can tell where the leaked resource was accessed lastly.
        /// </summary>
        /// <param name="hint"></param>
        void Record(object hint);

        /// <summary>
        /// Close the leak so that <see cref="IResourceLeakTracker"/> does not warn about leaked resources.
        /// After this method is called a leak associated with this ResourceLeakTracker should not be reported.
        /// </summary>
        /// <returns><c>true</c> if called first time, <c>false</c> if called already</returns>
        bool Close(object trackedObject);
    }
}