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
    using Extensions;

    public abstract class PipeSegment :
        Pipe
    {
        protected PipeSegment(PipeSegmentType segmentType, Type messageType)
        {
            SegmentType = segmentType;
            MessageType = messageType;
        }

        public virtual void Send<T>(T message)
            where T : class
        {
            Accept(message).Each(consumer => consumer(message));
        }

        public virtual IEnumerable<MessageConsumer<T>> Accept<T>(T message)
            where T : class
        {
            yield break;
        }

        public PipeSegmentType SegmentType { get; private set; }

        public Type MessageType { get; private set; }

        public static InputSegment Input(Pipe pipe)
        {
            return new InputSegment(pipe);
        }

		public static InputSegment New()
		{
			return Input(End());	
		}

        public static EndSegment End()
        {
            return End<object>();
        }

        public static EndSegment End<T>()
            where T : class
        {
            return End(typeof (T));
        }

        public static EndSegment End(Type type)
        {
            return new EndSegment(type);
        }

        public static FilterSegment Filter<T>(Pipe pipe)
            where T : class
        {
            return Filter(pipe, typeof (T));
        }

        public static FilterSegment Filter(Pipe pipe, Type inputType)
        {
            return new FilterSegment(pipe, inputType);
        }

        public static FilterSegment Filter<T>(Pipe pipe, Predicate<T> accept)
            where T : class
        {
            return new FilterSegment<T>(pipe, accept);
        }

        public static RecipientListSegment RecipientList<T>(IEnumerable<Pipe> recipients)
            where T : class
        {
            return RecipientList(typeof (T), recipients);
        }

        public static RecipientListSegment RecipientList(Type messageType, IEnumerable<Pipe> recipients)
        {
            return new RecipientListSegment(messageType, recipients);
        }

        public static MessageConsumerSegment Consumer<T>(MessageConsumer<T> consumer)
            where T : class
        {
            return new MessageConsumerSegment<T>(consumer);
        }

        public static MessageConsumerSegment Consumer<TConsumer, TMessage>(TConsumer consumer)
			where TConsumer : IConsumer<TMessage>
            where TMessage : class
        {
            return new MessageConsumerSegment<TMessage>(typeof(TConsumer), consumer);
        }

        public static MessageConsumerSegment Consumer<TConsumer, TMessage>(Func<TConsumer> getConsumer)
			where TConsumer : IConsumer<TMessage>
            where TMessage : class
        {
        	return new MessageConsumerSegment<TMessage>(typeof(TConsumer), () => getConsumer());
        }

    	public static InterceptorSegment Interceptor<T>(Pipe pipe, Action<IInterceptorConfigurator<T>> configureAction)
            where T : class
        {
            var configurator = new InterceptorConfigurator<T>();

            configureAction(configurator);

            return new InterceptorSegment<T>(pipe, configurator.BeforeEachMessageDelegate, configurator.AfterEachMessageDelegate);
        }
    }
}