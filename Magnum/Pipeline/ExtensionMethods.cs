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
	using Segments;
	using Visitors;


	public static class ExtensionMethods
    {
        public static ISubscriptionScope NewSubscriptionScope(this Pipe pipe)
        {
            return new SubscriptionScope(pipe);
        }

		public static Pipe New(this Pipe ignored)
		{
			return PipeSegment.Input(PipeSegment.End());
		}

		public static PipelineGraphData GetGraphData(this Pipe pipe)
		{
			var visitor = new GraphPipelineVisitor();
			visitor.Visit(pipe);

			return visitor.GetGraphData();
		}
    }
}