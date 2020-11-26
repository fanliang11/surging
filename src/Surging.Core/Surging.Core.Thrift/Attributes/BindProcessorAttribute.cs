using System;
using System.Collections.Generic;
using System.Text; 

namespace Surging.Core.Thrift.Attributes
{
    public class BindProcessorAttribute: Attribute
    {
        public BindProcessorAttribute(Type processorType) 
        {
            ProcessorType = processorType;
        }

        public Type ProcessorType { get; set; }
    }
}
