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
    using System.Linq;

	public class RecipientListSegment :
        PipeSegment
    {
        public Pipe[] Recipients { get; private set; }

        public RecipientListSegment(Type messageType, IEnumerable<Pipe> recipients)
            : base(PipeSegmentType.RecipientList, messageType)
        {
            Recipients = recipients.ToArray();
        }

        public override IEnumerable<MessageConsumer<T>> Accept<T>(T message)
        {
            for (int i = 0; i < Recipients.Length; i++)
            {
                foreach (var consumer in Recipients[i].Accept(message))
                {
                    yield return consumer;
                }
            }
        }
    }
}