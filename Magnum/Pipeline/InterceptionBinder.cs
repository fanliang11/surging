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
    using Segments;
    using Visitors;

    /// <summary>
    /// Inserts a segment into the pipeline as an inline filter
    /// </summary>
    public class InterceptionBinder :
        AbstractPipeVisitor
    {
        private readonly Type _messageType;
        private readonly Func<Pipe, Pipe> _buildSegment;
        private bool _found;

        public InterceptionBinder(Type messageType, Func<Pipe,Pipe> buildSegment)
        {
            _messageType = messageType;
            _buildSegment = buildSegment;
        }

        public void Bind(Pipe pipe)
        {
            base.Visit(pipe);
        }

        protected override Pipe VisitInput(InputSegment input)
        {
            if (input == null)
                return null;

            var output = input.Output;
            if (output.MessageType == _messageType && !_found)
            {
                _found = true;

                Pipe newOutput = _buildSegment(Visit(output));

                input.ReplaceOutput(output, newOutput);

                return input;
            }

            Pipe pipe = Visit(output);
            if (pipe != output)
            {
                input.ReplaceOutput(output, pipe);
            }

            return input;
        }
    }
}