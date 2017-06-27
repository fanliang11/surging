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
    using System.Threading;

	public class InputSegment :
        PipeSegment
    {
        public InputSegment(Pipe pipe)
            : base(PipeSegmentType.Input, pipe.MessageType)
        {
            _output = pipe;
        }

        private Pipe _output;

        public Pipe Output
        {
            get { return _output; }
        }

        public void ReplaceOutput(Pipe original, Pipe replacement)
        {
            if (original.MessageType != replacement.MessageType)
                throw new ArgumentException("The replacement pipe is not of the same message type");

            Interlocked.CompareExchange(ref _output, replacement, original);
            if (_output != replacement)
            {
                throw new InvalidOperationException("The pipeline has been modified since it was last requested");
            }
        }

        public override IEnumerable<MessageConsumer<T>> Accept<T>(T message)
        {
            return _output.Accept(message);
        }
    }
}