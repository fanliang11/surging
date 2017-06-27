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
namespace Magnum.Pipeline.Segments
{
    using System;
    using System.Collections.Generic;

	public abstract class InterceptorSegment :
        PipeSegment
    {
        protected InterceptorSegment(Pipe pipe)
            : base(PipeSegmentType.Interceptor, pipe.MessageType)
        {
            Output = pipe;
        }

        public Pipe Output { get; private set; }

        public abstract InterceptorSegment Clone(Pipe output);
    }

	public class InterceptorSegment<TMessage> :
        InterceptorSegment
        where TMessage : class
    {
        public MessageConsumer<TMessage> BeforeEachMessage { get; private set; }
        public MessageConsumer<TMessage> AfterEachMessage { get; private set; }

        public InterceptorSegment(Pipe pipe, MessageConsumer<TMessage> beforeEachMessage, MessageConsumer<TMessage> afterEachMessage)
            : base(pipe)
        {
            BeforeEachMessage = beforeEachMessage;
            AfterEachMessage = afterEachMessage;
        }

        public override IEnumerable<MessageConsumer<T>> Accept<T>(T message)
        {
            InvokeBeforeEachMessageAction(message);

            foreach (var consumer in Output.Accept(message))
            {
                yield return consumer;
            }

            InvokeAfterEachMessageAction(message);
        }

        private void InvokeBeforeEachMessageAction<T>(T message)
        {
            InvokeAction(BeforeEachMessage, message);
        }

        private void InvokeAfterEachMessageAction<T>(T message)
        {
            InvokeAction(AfterEachMessage, message);
        }

        private static void InvokeAction<T>(MessageConsumer<TMessage> action, T message)
        {
            var msg = message as TMessage;
            if (msg == null)
                return;

            if (action != null)
                action(message as TMessage);
        }

        public override InterceptorSegment Clone(Pipe output)
        {
            return new InterceptorSegment<TMessage>(output, BeforeEachMessage, AfterEachMessage);
        }
    }
}