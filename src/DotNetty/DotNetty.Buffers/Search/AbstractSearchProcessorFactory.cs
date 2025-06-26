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
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

//namespace DotNetty.Buffers
//{
//    using DotNetty.Common.Utilities;

//    /// <summary>
//    /// Base class for precomputed factories that create {@link SearchProcessor}s.
//    /// 
//    /// <para>Different factories implement different search algorithms with performance characteristics that
//    /// depend on a use case, so it is advisable to benchmark a concrete use case with different algorithms
//    /// before choosing one of them.</para>
//    /// 
//    /// <para>A concrete instance of {@link AbstractSearchProcessorFactory} is built for searching for a concrete sequence of bytes
//    /// (the {@code needle}), it contains precomputed data needed to perform the search, and is meant to be reused
//    /// whenever searching for the same {@code needle}.</para>
//    /// 
//    /// <para><b>Note:</b> implementations of {@link SearchProcessor} scan the {@link io.netty.buffer.ByteBuf} sequentially,
//    /// one byte after another, without doing any random access. As a result, when using {@link SearchProcessor}
//    /// with such methods as {@link io.netty.buffer.ByteBuf#forEachByte}, these methods return the index of the last byte
//    /// of the found byte sequence within the {@link io.netty.buffer.ByteBuf} (which might feel counterintuitive,
//    /// and different from {@link io.netty.buffer.ByteBufUtil#indexOf} which returns the index of the first byte
//    /// of found sequence).</para>
//    /// 
//    /// <para>A {@link SearchProcessor} is implemented as a
//    /// <a href="https://en.wikipedia.org/wiki/Finite-state_machine">Finite State Automaton</a> that contains a
//    /// small internal state which is updated with every byte processed. As a result, an instance of {@link SearchProcessor}
//    /// should not be reused across independent search sessions (eg. for searching in different
//    /// {@link io.netty.buffer.ByteBuf}s). A new instance should be created with {@link AbstractSearchProcessorFactory} for
//    /// every search session. However, a {@link SearchProcessor} can (and should) be reused within the search session,
//    /// eg. when searching for all occurrences of the {@code needle} within the same {@code haystack}. That way, it can
//    /// also detect overlapping occurrences of the {@code needle} (eg. a string "ABABAB" contains two occurences of "BAB"
//    /// that overlap by one character "B"). For this to work correctly, after an occurrence of the {@code needle} is
//    /// found ending at index {@code idx}, the search should continue starting from the index {@code idx + 1}.
//    /// </para>
//    /// Example (given that the {@code haystack} is a {@link io.netty.buffer.ByteBuf} containing "ABABAB" and
//    /// the {@code needle} is "BAB"):
//    /// <code>
//    ///     SearchProcessorFactory factory =
//    ///         SearchProcessorFactory.newKmpSearchProcessorFactory(needle.getBytes(CharsetUtil.UTF_8));
//    ///     SearchProcessor processor = factory.newSearchProcessor();
//    ///
//    ///     int idx1 = haystack.forEachByte(processor);
//    ///     // idx1 is 3 (index of the last character of the first occurrence of the needle in the haystack)
//    ///
//    ///     int continueFrom1 = idx1 + 1;
//    ///     // continue the search starting from the next character
//    ///
//    ///     int idx2 = haystack.forEachByte(continueFrom1, haystack.readableBytes() - continueFrom1, processor);
//    ///     // idx2 is 5 (index of the last character of the second occurrence of the needle in the haystack)
//    ///
//    ///     int continueFrom2 = idx2 + 1;
//    ///     // continue the search starting from the next character
//    ///
//    ///     int idx3 = haystack.forEachByte(continueFrom2, haystack.readableBytes() - continueFrom2, processor);
//    ///     // idx3 is -1 (no more occurrences of the needle)
//    ///
//    ///     // After this search session is complete, processor should be discarded.
//    ///     // To search for the same needle again, reuse the same factory to get a new SearchProcessor.
//    /// </code>
//    /// </summary>
//    public abstract class AbstractSearchProcessorFactory : ISearchProcessorFactory
//    {
//        public abstract ISearchProcessor NewSearchProcessor();
//    }
//}