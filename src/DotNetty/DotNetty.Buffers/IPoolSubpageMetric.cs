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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com)
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Buffers
{
    public interface IPoolSubpageMetric
    {
        /// Return the number of maximal elements that can be allocated out of the sub-page.
        int MaxNumElements { get; }

        /// Return the number of available elements to be allocated.
        int NumAvailable { get; }

        /// Return the size (in bytes) of the elements that will be allocated.
        int ElementSize { get; }

        /// Return the size (in bytes) of this page.
        int PageSize { get; }
    }
}