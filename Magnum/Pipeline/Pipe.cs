// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Pipeline
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A pipeline is used for the delivery of messages (which are really just objects of any type)
    /// </summary>
    public interface Pipe
    {
        /// <summary>
        /// Sends a message through the pipe
        /// </summary>
        /// <typeparam name="T">Captures the generic type of the message</typeparam>
        /// <param name="message">The message to send</param>
        void Send<T>(T message)
            where T : class;

        /// <summary>
        /// Returns an enumeration of consumers that are interested in the message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        IEnumerable<MessageConsumer<T>> Accept<T>(T message)
            where T : class;

        /// <summary>
        /// The type of this pipeline node
        /// </summary>
        PipeSegmentType SegmentType { get; }

        /// <summary>
        /// The type accepted by this segment
        /// </summary>
        Type MessageType { get; }
    }
}