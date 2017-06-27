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

	public abstract class MessageConsumerSegment :
		PipeSegment
	{
		protected MessageConsumerSegment(Type messageType, Type consumerType)
			: base(PipeSegmentType.MessageConsumer, messageType)
		{
			ConsumerType = consumerType;
		}

		public Type ConsumerType { get; private set; }
	}

	public class MessageConsumerSegment<TMessage> :
		MessageConsumerSegment
		where TMessage : class
	{
		private readonly Func<MessageConsumer<TMessage>> _getConsumer;

		public MessageConsumerSegment(MessageConsumer<TMessage> consumer)
			: base(typeof (TMessage), typeof (MessageConsumer<TMessage>))
		{
			_getConsumer = () => consumer;
		}

		public MessageConsumerSegment(Type consumerType, IConsumer<TMessage> consumer)
			: base(typeof (TMessage), consumerType)
		{
			_getConsumer = () => consumer.Consume;
		}

		public MessageConsumerSegment(Type consumerType, Func<IConsumer<TMessage>> getConsumer)
			: base(typeof (TMessage), consumerType)
		{
			_getConsumer = () => getConsumer().Consume;
		}

		public override IEnumerable<MessageConsumer<T>> Accept<T>(T message)
		{
			TMessage msg = message as TMessage;
			if (msg != null)
			{
				var consumer = _getConsumer();

				yield return x => consumer(x as TMessage);
			}
		}
	}
}