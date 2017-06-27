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
	using System.Collections.Generic;
	using Extensions;
	using Graphing;
	using Segments;


	public class GraphPipelineVisitor :
		AbstractPipeVisitor
	{
		readonly List<Edge> _edges = new List<Edge>();
		readonly Stack<Vertex> _stack = new Stack<Vertex>();
		readonly Dictionary<int, Vertex> _vertices = new Dictionary<int, Vertex>();
		Vertex _lastNodeVertex;

		public PipelineGraphData GetGraphData()
		{
			return new PipelineGraphData(_vertices.Values, _edges);
		}

		public new void Visit(Pipe pipe)
		{
			base.Visit(pipe);
		}

		protected override Pipe VisitInput(InputSegment input)
		{
			_lastNodeVertex = GetSink(input.GetHashCode(), () => "Input", typeof(InputSegment), input.MessageType);

			if (_stack.Count > 0)
				_edges.Add(new Edge(_stack.Peek(), _lastNodeVertex, _lastNodeVertex.TargetType.Name));

			return Recurse(() => base.VisitInput(input));
		}

		protected override Pipe VisitEnd(EndSegment end)
		{
			_lastNodeVertex = GetSink(end.GetHashCode(), () => "End", typeof(EndSegment), end.MessageType);

			if (_stack.Count > 0)
				_edges.Add(new Edge(_stack.Peek(), _lastNodeVertex, _lastNodeVertex.TargetType.Name));

			return base.VisitEnd(end);
		}

		protected override Pipe VisitFilter(FilterSegment filter)
		{
			_lastNodeVertex = GetSink(filter.GetHashCode(), () => "Filter", typeof(FilterSegment), filter.MessageType);

			if (_stack.Count > 0)
				_edges.Add(new Edge(_stack.Peek(), _lastNodeVertex, _lastNodeVertex.TargetType.Name));

			return Recurse(() => base.VisitFilter(filter));
		}

		protected override Pipe VisitInterceptor(InterceptorSegment interceptor)
		{
			_lastNodeVertex = GetSink(interceptor.GetHashCode(), () => "Interceptor", typeof(InterceptorSegment),
			                          interceptor.MessageType);

			if (_stack.Count > 0)
				_edges.Add(new Edge(_stack.Peek(), _lastNodeVertex, _lastNodeVertex.TargetType.Name));

			return Recurse(() => base.VisitInterceptor(interceptor));
		}

		protected override Pipe VisitMessageConsumer(MessageConsumerSegment messageConsumer)
		{
			_lastNodeVertex = GetSink(messageConsumer.GetHashCode(), () => "Consumer", typeof(MessageConsumerSegment),
			                          messageConsumer.MessageType);

			if (_stack.Count > 0)
				_edges.Add(new Edge(_stack.Peek(), _lastNodeVertex, _lastNodeVertex.TargetType.Name));

			return Recurse(() => base.VisitMessageConsumer(messageConsumer));
		}

		protected override Pipe VisitRecipientList(RecipientListSegment recipientList)
		{
			_lastNodeVertex = GetSink(recipientList.GetHashCode(), () => "List", typeof(RecipientListSegment),
			                          recipientList.MessageType);

			if (_stack.Count > 0)
				_edges.Add(new Edge(_stack.Peek(), _lastNodeVertex, _lastNodeVertex.TargetType.Name));

			return Recurse(() => base.VisitRecipientList(recipientList));
		}

		Pipe Recurse(Func<Pipe> action)
		{
			_stack.Push(_lastNodeVertex);

			Pipe result = action();

			_stack.Pop();

			return result;
		}

		Vertex GetSink(int key, Func<string> getTitle, Type nodeType, Type objectType)
		{
			return _vertices.Retrieve(key, () =>
				{
					var newSink = new Vertex(nodeType, objectType, getTitle());

					return newSink;
				});
		}
	}
}