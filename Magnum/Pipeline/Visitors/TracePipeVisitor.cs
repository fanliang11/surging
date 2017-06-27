// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Magnum.Pipeline.Visitors
{
	using System;
	using Segments;


	public class TracePipeVisitor :
		AbstractPipeVisitor
	{
		int _depth;

		public void Trace(Pipe pipe)
		{
			base.Visit(pipe);
		}

		protected override Pipe VisitInput(InputSegment input)
		{
			WriteLine(input);

			return base.VisitInput(input);
		}

		protected override Pipe VisitEnd(EndSegment end)
		{
			WriteLine(end);

			return base.VisitEnd(end);
		}

		protected override Pipe VisitFilter(FilterSegment filter)
		{
			WriteLine(filter, "Allow " + filter.Output.MessageType);

			return base.VisitFilter(filter);
		}

		protected override Pipe VisitInterceptor(InterceptorSegment interceptor)
		{
			WriteLine(interceptor);

			return base.VisitInterceptor(interceptor);
		}

		protected override Pipe VisitMessageConsumer(MessageConsumerSegment messageConsumer)
		{
			WriteLine(messageConsumer, "Consumer Type = " + messageConsumer.ConsumerType);

			return base.VisitMessageConsumer(messageConsumer);
		}

		protected override Pipe VisitRecipientList(RecipientListSegment recipientList)
		{
			WriteLine(recipientList);
			return Indent(() => { return base.VisitRecipientList(recipientList); });
		}

		void WriteLine(Pipe node)
		{
			WriteLine(node, "");
		}

		void WriteLine(Pipe node, string message)
		{
			string formatted = string.Format(Pad() + "{0}<{1}>: {2}", node.SegmentType, node.MessageType.Name, message);

			System.Diagnostics.Trace.WriteLine(formatted);
		}

		string Pad()
		{
			return new string('\t', _depth);
		}

		void Indent(Action action)
		{
			_depth++;
			try
			{
				action();
			}
			finally
			{
				_depth--;
			}
		}

		T Indent<T>(Func<T> visitor)
		{
			_depth++;
			try
			{
				return visitor();
			}
			finally
			{
				_depth--;
			}
		}
	}
}